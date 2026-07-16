const recorders = new WeakMap();
const databaseName = "paceletics-running-analysis";
const databaseVersion = 3;
const recordingStoreName = "recordings";
const settingsStoreName = "settings";
const recordingDirectoryKey = "recordingDirectory";
const metadataFileSuffix = ".paceletics.json";
const recordingPackageSuffix = ".paceletics.zip";
const recordingArtifactType = "paceletics-running-analysis-recording";
const zipLocalFileHeaderSignature = 0x04034b50;
const zipCentralDirectorySignature = 0x02014b50;
const zipEndOfCentralDirectorySignature = 0x06054b50;
const defaultPerspective = "side";
const missingAthleteUserIdMessage = "The local recording metadata does not contain an athlete user id.";
const fileSystemStorageMode = "file-system";
const browserStorageMode = "browser";
const browserRecordingDirectoryName = "recordings";
const uploadStatusQueued = "queued";
const uploadStatusNeedsExport = "needs-export";
const uploadStatusUploading = "uploading";
const uploadStatusUploaded = "uploaded";
const uploadStatusFailed = "failed";
let cachedRecordingDirectoryHandle = null;

export async function startRecording(videoElement, metadata = {}) {
  if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    throw new Error("Camera access is not supported by this browser.");
  }

  if (!window.MediaRecorder) {
    throw new Error("Video recording is not supported by this browser.");
  }

  await disposeRecorder(videoElement);

  const contentType = getSupportedContentType();
  let stream = null;
  let writable = null;

  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: "environment" },
      audio: false
    });

    const directoryHandle = await tryGetWritableRecordingDirectory();
    const storageMode = directoryHandle ? fileSystemStorageMode : browserStorageMode;
    const localId = createLocalId();
    const recording = createRecordingMetadata(
      metadata,
      contentType || "video/webm",
      localId,
      directoryHandle,
      storageMode);
    const chunks = [];

    if (directoryHandle) {
      const fileHandle = await directoryHandle.getFileHandle(recording.fileName, { create: true });
      writable = await fileHandle.createWritable();
      recording.fileHandle = fileHandle;
    } else {
      const fileHandle = await createBrowserRecordingFile(recording.fileName);
      if (fileHandle) {
        writable = await fileHandle.createWritable();
        recording.browserFileName = recording.fileName;
      }
    }

    const recorder = new MediaRecorder(
      stream,
      contentType ? { mimeType: contentType } : undefined
    );
    recording.contentType = recording.contentType || recorder.mimeType || "video/webm";

    const state = {
      recorder,
      stream,
      directoryHandle,
      writable,
      chunks,
      recording,
      writeChain: Promise.resolve(),
      writeError: null
    };

    recorder.addEventListener("dataavailable", event => {
      if (!event.data || event.data.size === 0) {
        return;
      }

      const chunk = event.data;
      state.recording.size += chunk.size;

      if (!state.writable) {
        state.chunks.push(chunk);
        return;
      }

      state.writeChain = state.writeChain
        .then(async () => {
          await state.writable.write(chunk);
        })
        .catch(error => {
          state.writeError = error;
        });
    });

    videoElement.srcObject = stream;
    await videoElement.play();
    recorder.start(1000);

    recorders.set(videoElement, state);
  } catch (error) {
    if (writable?.abort) {
      try {
        await writable.abort();
      } catch {
        // Best-effort cleanup after a failed start.
      }
    }

    stopStream(videoElement, stream);
    throw error;
  }
}

export async function stopRecording(videoElement) {
  const state = recorders.get(videoElement);
  if (!state) {
    throw new Error("No active recording was found.");
  }

  if (state.recorder.state !== "inactive") {
    await new Promise((resolve, reject) => {
      state.recorder.addEventListener("stop", () => {
        resolve();
      }, { once: true });

      state.recorder.addEventListener("error", event => {
        reject(event.error || new Error("Recording failed."));
      }, { once: true });

      state.recorder.stop();
    });
  }

  stopStream(videoElement, state.stream);
  recorders.delete(videoElement);

  try {
    await state.writeChain;
    if (state.writeError) {
      throw state.writeError;
    }

    if (state.writable) {
      await state.writable.close();
      if (state.directoryHandle) {
        await writeRecordingPackageFile(state.directoryHandle, state.recording);
        await removeDirectoryFile(state.directoryHandle, state.recording.fileName);
        delete state.recording.fileHandle;
      }
    } else {
      state.recording.blob = new Blob(state.chunks, { type: state.recording.contentType });
      state.recording.size = state.recording.blob.size;
    }

    await storeRecording(state.recording);
  } catch (error) {
    if (state.writable?.abort) {
      try {
        await state.writable.abort();
      } catch {
        // Best-effort cleanup after a failed stop.
      }
    }

    throw error;
  }

  return toRecordingPayload(state.recording);
}

export async function chooseRecordingDirectory() {
  if (!window.showDirectoryPicker) {
    return await getBrowserRecordingStorageStatus();
  }

  const directoryHandle = await window.showDirectoryPicker({
    id: "paceletics-running-analysis",
    mode: "readwrite"
  });
  await ensureHandlePermission(directoryHandle, "readwrite", true);
  cachedRecordingDirectoryHandle = directoryHandle;
  await saveSetting(recordingDirectoryKey, directoryHandle);
  await importRecordingsFromDirectory(directoryHandle);

  return await getRecordingStorageStatus();
}

export async function chooseRecordingFiles() {
  if (!window.File || !window.FileReader) {
    return {
      importedCount: 0,
      skippedCount: 0,
      errorMessage: "This browser cannot read selected recording files."
    };
  }

  const files = await selectRecordingFiles();
  if (!files || files.length === 0) {
    return {
      importedCount: 0,
      skippedCount: 0,
      errorMessage: ""
    };
  }

  return await importRecordingsFromFiles(files);
}

export async function getRecordingStorageStatus() {
  if (!window.showDirectoryPicker) {
    return await getBrowserRecordingStorageStatus();
  }

  const directoryHandle = await loadRecordingDirectoryHandle();
  if (!directoryHandle) {
    return {
      isSupported: true,
      supportsDeviceFolder: true,
      isConfigured: false,
      isReady: false,
      folderName: "",
      permissionState: "",
      storageLocation: "",
      errorMessage: ""
    };
  }

  try {
    const permissionState = await ensureHandlePermission(directoryHandle, "readwrite", false);
    return {
      isSupported: true,
      supportsDeviceFolder: true,
      isConfigured: true,
      isReady: permissionState === "granted",
      folderName: directoryHandle.name || "",
      permissionState,
      storageLocation: `Device folder selected in browser: ${directoryHandle.name || "(unnamed folder)"}`,
      errorMessage: permissionState === "granted"
        ? ""
        : "The browser needs permission to read and write the selected folder."
    };
  } catch (error) {
    return {
      isSupported: true,
      supportsDeviceFolder: true,
      isConfigured: true,
      isReady: false,
      folderName: directoryHandle.name || "",
      permissionState: "denied",
      storageLocation: `Device folder selected in browser: ${directoryHandle.name || "(unnamed folder)"}`,
      errorMessage: error?.message || "The selected recording folder is not available."
    };
  }
}

export async function listSavedRecordings(captureSessionId, requestDirectoryPermission = false, folderOnly = false) {
  const scannedLocalIds = await importRecordingsFromConfiguredDirectory(requestDirectoryPermission);

  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readonly");
  const store = transaction.objectStore(recordingStoreName);
  const recordings = await requestToPromise(store.getAll());
  await transactionToPromise(transaction);

  return recordings
    .filter(recording => !folderOnly || shouldIncludeFolderOnlyRecording(recording, scannedLocalIds))
    .filter(recording => !captureSessionId || getRecordingCaptureSessionId(recording) === captureSessionId)
    .map(toRecordingPayload)
    .sort((left, right) => left.recordedAt.localeCompare(right.recordedAt));
}

export async function openSavedRecording(localId) {
  const recording = await getSavedRecording(localId);
  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  if (recording.storageMode === "file-system") {
    if (recording.packageFileHandle || recording.packageFileName) {
      const packageFileHandle = recording.packageFileHandle
        || await getFileHandleFromDirectory(recording.packageFileName);
      await ensureHandlePermission(packageFileHandle, "read", true);
      return await openRecordingFromPackageFile(await packageFileHandle.getFile(), recording);
    }

    const fileHandle = recording.fileHandle
      || await getFileHandleFromDirectory(recording.fileName);
    await ensureHandlePermission(fileHandle, "read", true);
    return await fileHandle.getFile();
  }

  if (recording.storageMode === browserStorageMode && recording.browserFileName) {
    const file = await openBrowserRecordingFile(recording.browserFileName);
    if (file) {
      return file;
    }
  }

  if (recording.blob) {
    return recording.blob;
  }

  throw new Error("The saved recording file was not found on this device.");
}

export async function exportSavedRecording(localId) {
  const recording = await getSavedRecording(localId);
  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  if (recording.storageMode === fileSystemStorageMode) {
    return {
      exported: true,
      uploadStatus: recording.uploadStatus || uploadStatusQueued,
      message: "The recording is already saved in the selected device folder."
    };
  }

  const source = await openSavedRecording(localId);
  const file = source instanceof File && source.name === recording.fileName
    ? source
    : new File([source], recording.fileName, {
      type: recording.contentType || source.type || "application/octet-stream",
      lastModified: Date.parse(recording.recordedAt) || Date.now()
    });
  const metadata = toRecordingMetadata({
    ...recording,
    uploadStatus: uploadStatusQueued,
    storageMode: fileSystemStorageMode,
    exportedAt: new Date().toISOString()
  });
  const metadataFileName = metadata.metadataFileName || `${recording.fileName}${metadataFileSuffix}`;
  const metadataFile = new File(
    [JSON.stringify(metadata, null, 2)],
    metadataFileName,
    {
      type: "application/json",
      lastModified: Date.now()
    });
  const packageFile = await createRecordingPackageFile(recording, [file, metadataFile]);

  await shareRecordingFiles([file, metadataFile], packageFile);

  return await updateSavedRecording(localId, current => {
    current.exportedAt = new Date().toISOString();
    current.uploadStatus = uploadStatusQueued;
    current.lastError = "";
  });
}

export async function openSavedRecordingMetadata(localId) {
  const recording = await getSavedRecording(localId);
  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  const metadata = toRecordingMetadata(recording);
  const metadataFileName = metadata.metadataFileName || `${recording.fileName}${metadataFileSuffix}`;
  return new File(
    [JSON.stringify(metadata, null, 2)],
    metadataFileName,
    {
      type: "application/json",
      lastModified: Date.now()
    });
}
export async function markSavedRecordingUploadStarted(localId) {
  return await updateSavedRecording(localId, recording => {
    if (recording.uploadStatus === uploadStatusNeedsExport) {
      throw new Error("Export the recording to device files before uploading. The browser copy is only temporary.");
    }

    recording.uploadStatus = uploadStatusUploading;
    recording.uploadAttempts = (recording.uploadAttempts || 0) + 1;
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = "";
  });
}

export async function markSavedRecordingUploadSucceeded(localId, driveFileUrl) {
  return await updateSavedRecording(localId, recording => {
    recording.uploadStatus = uploadStatusUploaded;
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = "";
    recording.driveFileUrl = driveFileUrl || "";
  });
}

export async function markSavedRecordingUploadFailed(localId, errorMessage) {
  return await updateSavedRecording(localId, recording => {
    recording.uploadStatus = uploadStatusFailed;
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = errorMessage || "Upload failed.";
  });
}

export async function deleteSavedRecording(localId) {
  const recording = await getSavedRecording(localId);
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  store.delete(localId);
  await transactionToPromise(transaction);

  if (recording?.storageMode === browserStorageMode && recording.browserFileName) {
    await deleteBrowserRecordingFile(recording.browserFileName);
  }
}

export async function disposeRecorder(videoElement) {
  const state = recorders.get(videoElement);
  if (!state) {
    return;
  }

  if (state.recorder && state.recorder.state !== "inactive") {
    try {
      await stopRecording(videoElement);
      return;
    } catch {
      // Fall through to best-effort cleanup below.
    }
  }

  if (state.writable?.abort) {
    try {
      await state.writable.abort();
    } catch {
      // Best-effort cleanup during component disposal.
    }
  }

  stopStream(videoElement, state.stream);
  recorders.delete(videoElement);
}

export function isOnline() {
  return navigator.onLine;
}

function stopStream(videoElement, stream) {
  if (stream) {
    for (const track of stream.getTracks()) {
      track.stop();
    }
  }

  if (videoElement) {
    videoElement.pause();
    videoElement.srcObject = null;
  }
}

function getSupportedContentType() {
  const candidates = [
    "video/webm;codecs=vp9",
    "video/webm;codecs=vp8",
    "video/webm",
    "video/mp4"
  ];

  return candidates.find(candidate => MediaRecorder.isTypeSupported(candidate)) || "";
}

function createRecordingMetadata(metadata, contentType, localId, directoryHandle, storageMode) {
  const captureSessionId = getMetadataValue(metadata, "captureSessionId") || getMetadataValue(metadata, "analysisEventId");
  const captureExternalEventId = getMetadataValue(metadata, "captureExternalEventId") || getMetadataValue(metadata, "analysisExternalEventId");
  const courseId = getMetadataValue(metadata, "courseId");
  const captureTitle = getMetadataValue(metadata, "captureTitle") || getMetadataValue(metadata, "analysisTitle");
  const captureStartsAt = getMetadataValue(metadata, "captureStartsAt") || getMetadataValue(metadata, "analysisStartsAt");
  const participantId = getMetadataValue(metadata, "participantId");
  const athleteUserId = getMetadataValue(metadata, "athleteUserId");
  const athleteEmail = getMetadataValue(metadata, "athleteEmail");
  const participantName = getMetadataValue(metadata, "participantName");
  const fileNamePrefix = getMetadataValue(metadata, "fileNamePrefix");
  const perspective = normalizePerspective(getMetadataValue(metadata, "perspective"));

  if (!athleteUserId || !participantId || !fileNamePrefix) {
    throw new Error("Recording metadata is incomplete.");
  }

  const fileExtension = contentType.includes("mp4") ? "mp4" : "webm";
  const recordedAt = new Date().toISOString();
  const fileName = buildFileName(fileNamePrefix, fileExtension, localId, perspective);
  const metadataFileName = `${fileName}${metadataFileSuffix}`;

  return {
    artifactType: recordingArtifactType,
    localId,
    captureSessionId,
    captureExternalEventId,
    analysisEventId: captureSessionId,
    analysisExternalEventId: captureExternalEventId,
    courseId,
    captureTitle,
    captureStartsAt,
    analysisTitle: captureTitle,
    analysisStartsAt: captureStartsAt,
    participantId,
    athleteUserId,
    athleteEmail,
    participantName,
    perspective,
    fileName,
    contentType,
    fileExtension,
    size: 0,
    recordedAt,
    storageMode,
    folderName: directoryHandle?.name || "",
    metadataFileName,
    uploadStatus: storageMode === browserStorageMode ? uploadStatusNeedsExport : uploadStatusQueued,
    uploadAttempts: 0,
    lastUploadAt: null,
    lastError: "",
    driveFileUrl: ""
  };
}

async function createBrowserRecordingFile(fileName) {
  const directoryHandle = await getBrowserRecordingDirectory(true);
  return directoryHandle
    ? await directoryHandle.getFileHandle(fileName, { create: true })
    : null;
}

async function openBrowserRecordingFile(fileName) {
  try {
    const directoryHandle = await getBrowserRecordingDirectory(false);
    if (!directoryHandle) {
      return null;
    }

    const fileHandle = await directoryHandle.getFileHandle(fileName, { create: false });
    return await fileHandle.getFile();
  } catch {
    return null;
  }
}

async function deleteBrowserRecordingFile(fileName) {
  try {
    const directoryHandle = await getBrowserRecordingDirectory(false);
    if (directoryHandle?.removeEntry) {
      await directoryHandle.removeEntry(fileName);
    }
  } catch {
    // Best-effort cleanup; the queue record is already gone.
  }
}

async function getBrowserRecordingDirectory(create) {
  if (!navigator.storage?.getDirectory) {
    return null;
  }

  const root = await navigator.storage.getDirectory();
  if (!root?.getDirectoryHandle) {
    return null;
  }

  return await root.getDirectoryHandle(browserRecordingDirectoryName, { create });
}

async function storeRecording(recording) {
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  store.put(recording);
  await transactionToPromise(transaction);
}

async function getSavedRecording(localId) {
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readonly");
  const store = transaction.objectStore(recordingStoreName);
  const recording = await requestToPromise(store.get(localId));
  await transactionToPromise(transaction);
  return recording;
}

async function requireRecordingDirectory() {
  if (!window.showDirectoryPicker) {
    throw new Error("This browser cannot save recordings directly to a device folder.");
  }

  const directoryHandle = await loadRecordingDirectoryHandle();
  if (!directoryHandle) {
    throw new Error("Select a local recording folder before recording.");
  }

  const permissionState = await ensureHandlePermission(directoryHandle, "readwrite", true);
  if (permissionState !== "granted") {
    throw new Error("The browser does not have permission to write to the selected recording folder.");
  }

  return directoryHandle;
}

async function tryGetWritableRecordingDirectory() {
  if (!window.showDirectoryPicker) {
    await ensureBrowserRecordingStorage();
    return null;
  }

  const directoryHandle = await loadRecordingDirectoryHandle();
  if (!directoryHandle) {
    await ensureBrowserRecordingStorage();
    return null;
  }

  const permissionState = await ensureHandlePermission(directoryHandle, "readwrite", true);
  if (permissionState !== "granted") {
    await ensureBrowserRecordingStorage();
    return null;
  }

  return directoryHandle;
}

async function getFileHandleFromDirectory(fileName) {
  const directoryHandle = await requireRecordingDirectory();
  return await directoryHandle.getFileHandle(fileName, { create: false });
}

async function importRecordingsFromConfiguredDirectory(requestPermission = false) {
  if (!window.showDirectoryPicker) {
    return null;
  }

  const directoryHandle = await loadRecordingDirectoryHandle();
  if (!directoryHandle) {
    return null;
  }

  return await importRecordingsFromDirectory(directoryHandle, requestPermission);
}

async function importRecordingsFromDirectory(directoryHandle, requestPermission = false) {
  if (!directoryHandle?.entries) {
    return null;
  }

  const permissionState = await ensureHandlePermission(directoryHandle, "readwrite", requestPermission);
  if (permissionState !== "granted") {
    return null;
  }

  const recordings = [];
  for await (const [metadataFileName, handle] of directoryHandle.entries()) {
    if (handle.kind !== "file") {
      continue;
    }

    const recording = isZipFileName(metadataFileName)
      ? await readRecordingPackageFile(directoryHandle, metadataFileName, handle)
      : metadataFileName.endsWith(metadataFileSuffix)
        ? await readRecordingMetadataFile(directoryHandle, metadataFileName, handle)
        : null;

    if (recording) {
      recordings.push(recording);
    }
  }

  if (recordings.length === 0) {
    return [];
  }

  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  for (const recording of recordings) {
    store.put(recording);
  }

  await transactionToPromise(transaction);
  return recordings.map(recording => recording.localId);
}

async function readRecordingMetadataFile(directoryHandle, metadataFileName, metadataHandle) {
  try {
    const metadataFile = await metadataHandle.getFile();
    const metadata = JSON.parse(await metadataFile.text());

    if (!metadata.localId
      || !metadata.participantId
      || !metadata.fileName) {
      return null;
    }

    const fileHandle = await directoryHandle.getFileHandle(metadata.fileName, { create: false });
    const videoFile = await fileHandle.getFile();
    const detectedContentType = metadata.contentType || videoFile.type || "";
    const fileExtension = metadata.fileExtension || getFileExtension(metadata.fileName);

    if (!isSupportedVideoRecording(metadata, videoFile, detectedContentType, fileExtension)) {
      return null;
    }

    const contentType = detectedContentType || (fileExtension === "mp4" ? "video/mp4" : "video/webm");
    const isMissingAthleteUserId = !metadata.athleteUserId;

    return {
      localId: metadata.localId,
      captureSessionId: metadata.captureSessionId || metadata.analysisEventId || "",
      captureExternalEventId: metadata.captureExternalEventId || metadata.analysisExternalEventId || "",
      analysisEventId: metadata.analysisEventId || metadata.captureSessionId || "",
      analysisExternalEventId: metadata.analysisExternalEventId || metadata.captureExternalEventId || "",
      courseId: metadata.courseId || "",
      captureTitle: metadata.captureTitle || metadata.analysisTitle || "",
      captureStartsAt: metadata.captureStartsAt || metadata.analysisStartsAt || null,
      analysisTitle: metadata.analysisTitle || metadata.captureTitle || "",
      analysisStartsAt: metadata.analysisStartsAt || metadata.captureStartsAt || null,
      participantId: metadata.participantId,
      athleteUserId: metadata.athleteUserId,
      athleteEmail: metadata.athleteEmail || "",
      participantName: metadata.participantName || "",
      perspective: normalizePerspective(metadata.perspective),
      fileName: metadata.fileName,
      contentType,
      fileExtension,
      size: metadata.size || videoFile.size,
      recordedAt: metadata.recordedAt || new Date(videoFile.lastModified || Date.now()).toISOString(),
      storageMode: fileSystemStorageMode,
      folderName: directoryHandle.name || metadata.folderName || "",
      metadataFileName,
      fileHandle,
      uploadStatus: isMissingAthleteUserId ? uploadStatusFailed : metadata.uploadStatus || uploadStatusQueued,
      uploadAttempts: metadata.uploadAttempts || 0,
      lastUploadAt: metadata.lastUploadAt || null,
      lastError: isMissingAthleteUserId ? missingAthleteUserIdMessage : metadata.lastError || "",
      driveFileUrl: metadata.driveFileUrl || ""
    };
  } catch {
    return null;
  }
}

async function readRecordingPackageFile(directoryHandle, packageFileName, packageHandle) {
  try {
    const packageFile = await packageHandle.getFile();
    const files = extractStoredZipFiles(packageFile, new Uint8Array(await packageFile.arrayBuffer()));
    const metadataFile = files.find(file => file.name.endsWith(metadataFileSuffix));
    if (!metadataFile) {
      return null;
    }

    const metadata = JSON.parse(await metadataFile.text());
    const videoFile = files.find(file => file.name === metadata.fileName)
      || files.find(file => file.type.startsWith("video/") || isVideoFileName(file.name));
    const recording = createRecordingFromImportedFiles(metadata, metadataFile.name, videoFile);
    if (!recording) {
      return null;
    }

    return {
      ...recording,
      storageMode: fileSystemStorageMode,
      folderName: directoryHandle.name || metadata.folderName || "",
      packageFileName,
      packageFileHandle: packageHandle,
      blob: null
    };
  } catch {
    return null;
  }
}

async function importRecordingsFromFiles(files) {
  const selectedFiles = Array.from(files || []);
  const expandedFiles = [
    ...selectedFiles.filter(file => !isZipFileName(file.name)),
    ...await extractRecordingFilesFromZipPackages(selectedFiles.filter(file => isZipFileName(file.name)))
  ];
  const metadataFiles = expandedFiles.filter(file => file.name.endsWith(metadataFileSuffix));
  const videoFilesByName = new Map(expandedFiles
    .filter(file => file.type.startsWith("video/") || isVideoFileName(file.name))
    .map(file => [file.name, file]));
  const recordings = [];
  let skippedCount = 0;

  for (const metadataFile of metadataFiles) {
    try {
      const metadata = JSON.parse(await metadataFile.text());
      const videoFileName = metadata.fileName || "";
      const videoFile = videoFilesByName.get(videoFileName);

      if (!videoFile) {
        skippedCount++;
        continue;
      }

      const recording = createRecordingFromImportedFiles(metadata, metadataFile.name, videoFile);
      if (recording) {
        recordings.push(recording);
      } else {
        skippedCount++;
      }
    } catch {
      skippedCount++;
    }
  }

  if (recordings.length > 0) {
    const database = await openDatabase();
    const transaction = database.transaction(recordingStoreName, "readwrite");
    const store = transaction.objectStore(recordingStoreName);
    for (const recording of recordings) {
      store.put(recording);
    }

    await transactionToPromise(transaction);
  }

  return {
    importedCount: recordings.length,
    skippedCount: skippedCount + Math.max(0, metadataFiles.length === 0 ? selectedFiles.length : 0),
    errorMessage: metadataFiles.length === 0
      ? "Select a PaceLetics ZIP package, or select the PaceLetics JSON metadata file together with the recording video file."
      : ""
  };
}

function createRecordingFromImportedFiles(metadata, metadataFileName, videoFile) {
  if (!metadata.localId || !metadata.participantId || !videoFile) {
    return null;
  }

  const detectedContentType = metadata.contentType || videoFile.type || "";
  const fileExtension = metadata.fileExtension || getFileExtension(videoFile.name);

  if (!isSupportedVideoRecording(metadata, videoFile, detectedContentType, fileExtension)) {
    return null;
  }

  const contentType = detectedContentType || (fileExtension === "mp4" ? "video/mp4" : "video/webm");
  const isMissingAthleteUserId = !metadata.athleteUserId;

  return {
    localId: metadata.localId,
    captureSessionId: metadata.captureSessionId || metadata.analysisEventId || "",
    captureExternalEventId: metadata.captureExternalEventId || metadata.analysisExternalEventId || "",
    analysisEventId: metadata.analysisEventId || metadata.captureSessionId || "",
    analysisExternalEventId: metadata.analysisExternalEventId || metadata.captureExternalEventId || "",
    courseId: metadata.courseId || "",
    captureTitle: metadata.captureTitle || metadata.analysisTitle || "",
    captureStartsAt: metadata.captureStartsAt || metadata.analysisStartsAt || null,
    analysisTitle: metadata.analysisTitle || metadata.captureTitle || "",
    analysisStartsAt: metadata.analysisStartsAt || metadata.captureStartsAt || null,
    participantId: metadata.participantId,
    athleteUserId: metadata.athleteUserId,
    athleteEmail: metadata.athleteEmail || "",
    participantName: metadata.participantName || "",
    perspective: normalizePerspective(metadata.perspective),
    fileName: metadata.fileName || videoFile.name,
    contentType,
    fileExtension,
    size: metadata.size || videoFile.size,
    recordedAt: metadata.recordedAt || new Date(videoFile.lastModified || Date.now()).toISOString(),
    storageMode: browserStorageMode,
    folderName: "",
    metadataFileName,
    blob: videoFile,
    uploadStatus: isMissingAthleteUserId ? uploadStatusFailed : metadata.uploadStatus || uploadStatusQueued,
    uploadAttempts: metadata.uploadAttempts || 0,
    lastUploadAt: metadata.lastUploadAt || null,
    lastError: isMissingAthleteUserId ? missingAthleteUserIdMessage : metadata.lastError || "",
    driveFileUrl: metadata.driveFileUrl || ""
  };
}

async function writeRecordingMetadataFile(directoryHandle, recording) {
  const metadataFileName = recording.metadataFileName || `${recording.fileName}${metadataFileSuffix}`;
  recording.metadataFileName = metadataFileName;

  const metadataHandle = await directoryHandle.getFileHandle(metadataFileName, { create: true });
  const writable = await metadataHandle.createWritable();
  await writable.write(JSON.stringify(toRecordingMetadata(recording), null, 2));
  await writable.close();
}

async function writeRecordingPackageFile(directoryHandle, recording) {
  const fileHandle = recording.fileHandle
    || await directoryHandle.getFileHandle(recording.fileName, { create: false });
  const videoFile = await fileHandle.getFile();
  recording.packageFileName = recording.packageFileName
    || `${getFileNameWithoutExtension(recording.fileName)}${recordingPackageSuffix}`;
  const metadata = toRecordingMetadata(recording);
  const metadataFileName = metadata.metadataFileName || `${recording.fileName}${metadataFileSuffix}`;
  const metadataFile = new File(
    [JSON.stringify(metadata, null, 2)],
    metadataFileName,
    {
      type: "application/json",
      lastModified: Date.now()
    });
  const packageFile = await createRecordingPackageFile(recording, [videoFile, metadataFile]);
  const packageHandle = await directoryHandle.getFileHandle(packageFile.name, { create: true });
  const writable = await packageHandle.createWritable();
  await writable.write(packageFile);
  await writable.close();

  recording.packageFileName = packageFile.name;
  recording.packageFileHandle = packageHandle;
}

async function openRecordingFromPackageFile(packageFile, recording) {
  const files = extractStoredZipFiles(packageFile, new Uint8Array(await packageFile.arrayBuffer()));
  const videoFile = files.find(file => file.name === recording.fileName)
    || files.find(file => file.type.startsWith("video/") || isVideoFileName(file.name));

  if (!videoFile) {
    throw new Error("The recording package does not contain the video file.");
  }

  return videoFile;
}

async function removeDirectoryFile(directoryHandle, fileName) {
  try {
    if (directoryHandle?.removeEntry && fileName) {
      await directoryHandle.removeEntry(fileName);
    }
  } catch {
    // Best-effort cleanup; the packaged recording is already written.
  }
}

async function tryWriteRecordingMetadataFile(recording) {
  if (recording.storageMode !== "file-system" || recording.packageFileName) {
    return;
  }

  try {
    const directoryHandle = await loadRecordingDirectoryHandle();
    if (!directoryHandle) {
      return;
    }

    const permissionState = await ensureHandlePermission(directoryHandle, "readwrite", false);
    if (permissionState !== "granted") {
      return;
    }

    await writeRecordingMetadataFile(directoryHandle, recording);
  } catch {
    // IndexedDB remains the live status store when the browser cannot update the sidecar file.
  }
}

async function updateSavedRecording(localId, updater) {
  const recording = await getSavedRecording(localId);
  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  updater(recording);

  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  store.put(recording);
  await transactionToPromise(transaction);
  await tryWriteRecordingMetadataFile(recording);

  return toRecordingPayload(recording);
}

function openDatabase() {
  if (!window.indexedDB) {
    throw new Error("Local recording storage is not supported by this browser.");
  }

  return new Promise((resolve, reject) => {
    const request = indexedDB.open(databaseName, databaseVersion);

    request.addEventListener("upgradeneeded", () => {
      const database = request.result;
      let store = null;
      if (!database.objectStoreNames.contains(recordingStoreName)) {
        store = database.createObjectStore(recordingStoreName, { keyPath: "localId" });
        store.createIndex("analysisEventId", "analysisEventId", { unique: false });
      } else {
        store = request.transaction.objectStore(recordingStoreName);
      }

      if (store && !store.indexNames.contains("captureSessionId")) {
        store.createIndex("captureSessionId", "captureSessionId", { unique: false });
      }

      if (!database.objectStoreNames.contains(settingsStoreName)) {
        database.createObjectStore(settingsStoreName);
      }
    });

    request.addEventListener("success", () => resolve(request.result));
    request.addEventListener("error", () => reject(request.error || new Error("Could not open local recording storage.")));
  });
}

function requestToPromise(request) {
  return new Promise((resolve, reject) => {
    request.addEventListener("success", () => resolve(request.result));
    request.addEventListener("error", () => reject(request.error || new Error("Local recording storage request failed.")));
  });
}

function transactionToPromise(transaction) {
  return new Promise((resolve, reject) => {
    transaction.addEventListener("complete", resolve);
    transaction.addEventListener("abort", () => reject(transaction.error || new Error("Local recording storage transaction was aborted.")));
    transaction.addEventListener("error", () => reject(transaction.error || new Error("Local recording storage transaction failed.")));
  });
}

function toRecordingPayload(recording) {
  const captureSessionId = getRecordingCaptureSessionId(recording);
  const captureExternalEventId = recording.captureExternalEventId || recording.analysisExternalEventId || "";
  const captureTitle = recording.captureTitle || recording.analysisTitle || "";
  const captureStartsAt = recording.captureStartsAt || recording.analysisStartsAt || null;

  return {
    localId: recording.localId,
    captureSessionId,
    captureExternalEventId,
    analysisEventId: recording.analysisEventId || captureSessionId,
    analysisExternalEventId: recording.analysisExternalEventId || captureExternalEventId,
    courseId: recording.courseId || "",
    captureTitle,
    captureStartsAt,
    analysisTitle: recording.analysisTitle || captureTitle,
    analysisStartsAt: recording.analysisStartsAt || captureStartsAt,
    participantId: recording.participantId,
    athleteUserId: recording.athleteUserId || "",
    athleteEmail: recording.athleteEmail || "",
    participantName: recording.participantName || "",
    perspective: normalizePerspective(recording.perspective),
    fileName: recording.fileName,
    contentType: recording.contentType,
    fileExtension: recording.fileExtension,
    size: recording.size,
    recordedAt: recording.recordedAt,
    storageLocation: getRecordingStorageLocation(recording),
    uploadStatus: recording.uploadStatus || uploadStatusQueued,
    storageMode: recording.storageMode || "",
    uploadAttempts: recording.uploadAttempts || 0,
    lastUploadAt: recording.lastUploadAt || null,
    lastError: recording.lastError || "",
    driveFileUrl: recording.driveFileUrl || ""
  };
}

function shouldIncludeFolderOnlyRecording(recording, scannedLocalIds) {
  if (recording.storageMode !== fileSystemStorageMode) {
    return true;
  }

  return Array.isArray(scannedLocalIds) && scannedLocalIds.includes(recording.localId);
}

function toRecordingMetadata(recording) {
  const captureSessionId = getRecordingCaptureSessionId(recording);
  const captureExternalEventId = recording.captureExternalEventId || recording.analysisExternalEventId || "";
  const captureTitle = recording.captureTitle || recording.analysisTitle || "";
  const captureStartsAt = recording.captureStartsAt || recording.analysisStartsAt || null;

  return {
    artifactType: recordingArtifactType,
    localId: recording.localId,
    captureSessionId,
    captureExternalEventId,
    analysisEventId: recording.analysisEventId || captureSessionId,
    analysisExternalEventId: recording.analysisExternalEventId || captureExternalEventId,
    courseId: recording.courseId || "",
    captureTitle,
    captureStartsAt,
    analysisTitle: recording.analysisTitle || captureTitle,
    analysisStartsAt: recording.analysisStartsAt || captureStartsAt,
    participantId: recording.participantId,
    athleteUserId: recording.athleteUserId || "",
    athleteEmail: recording.athleteEmail || "",
    participantName: recording.participantName || "",
    perspective: normalizePerspective(recording.perspective),
    fileName: recording.fileName,
    contentType: recording.contentType,
    fileExtension: recording.fileExtension,
    size: recording.size,
    recordedAt: recording.recordedAt,
    storageMode: recording.storageMode || fileSystemStorageMode,
    folderName: recording.folderName || "",
    metadataFileName: recording.metadataFileName || `${recording.fileName}${metadataFileSuffix}`,
    packageFileName: recording.packageFileName || "",
    browserFileName: recording.browserFileName || "",
    uploadStatus: recording.uploadStatus || uploadStatusQueued,
    uploadAttempts: recording.uploadAttempts || 0,
    lastUploadAt: recording.lastUploadAt || null,
    lastError: recording.lastError || "",
    driveFileUrl: recording.driveFileUrl || ""
  };
}

function getRecordingCaptureSessionId(recording) {
  return recording.captureSessionId || recording.analysisEventId || "";
}

async function loadRecordingDirectoryHandle() {
  if (cachedRecordingDirectoryHandle) {
    return cachedRecordingDirectoryHandle;
  }

  const directoryHandle = await loadSetting(recordingDirectoryKey);
  cachedRecordingDirectoryHandle = directoryHandle || null;
  return cachedRecordingDirectoryHandle;
}

async function saveSetting(key, value) {
  const database = await openDatabase();
  const transaction = database.transaction(settingsStoreName, "readwrite");
  const store = transaction.objectStore(settingsStoreName);
  store.put(value, key);
  await transactionToPromise(transaction);
}

async function loadSetting(key) {
  const database = await openDatabase();
  const transaction = database.transaction(settingsStoreName, "readonly");
  const store = transaction.objectStore(settingsStoreName);
  const value = await requestToPromise(store.get(key));
  await transactionToPromise(transaction);
  return value;
}

async function ensureHandlePermission(handle, mode, requestIfNeeded) {
  const options = { mode };
  let permissionState = await handle.queryPermission(options);
  if (permissionState !== "granted" && requestIfNeeded) {
    permissionState = await handle.requestPermission(options);
  }

  return permissionState;
}

async function getBrowserRecordingStorageStatus() {
  try {
    await ensureBrowserRecordingStorage();
    return {
      isSupported: true,
      supportsDeviceFolder: false,
      isConfigured: true,
      isReady: true,
      folderName: await getBrowserStorageLabel(),
      permissionState: "granted",
      storageLocation: await getBrowserStorageLocation(),
      errorMessage: ""
    };
  } catch (error) {
    return getUnsupportedStorageStatus(error?.message || "This browser cannot store recordings locally.");
  }
}

async function ensureBrowserRecordingStorage() {
  if (!window.indexedDB) {
    throw new Error("Local recording storage is not supported by this browser.");
  }

  const database = await openDatabase();
  database.close();
}

async function getBrowserStorageLabel() {
  return await hasBrowserFileStorage()
    ? "Browser file storage"
    : "Browser storage";
}

async function getBrowserStorageLocation() {
  return await hasBrowserFileStorage()
    ? `Browser OPFS: ${browserRecordingDirectoryName}`
    : `Browser IndexedDB: ${databaseName}/${recordingStoreName}`;
}

async function hasBrowserFileStorage() {
  try {
    const directoryHandle = await getBrowserRecordingDirectory(true);
    return !!directoryHandle;
  } catch {
    return false;
  }
}

function getUnsupportedStorageStatus(errorMessage) {
  return {
    isSupported: false,
    supportsDeviceFolder: false,
    isConfigured: false,
    isReady: false,
    folderName: "",
    permissionState: "",
    storageLocation: "",
    errorMessage: errorMessage || "This browser cannot store recordings locally."
  };
}

function getRecordingStorageLocation(recording) {
  if (recording.storageMode === fileSystemStorageMode) {
    return `Device folder: ${recording.folderName || "(selected folder)"}/${recording.packageFileName || recording.fileName}`;
  }

  if (recording.browserFileName) {
    return `Browser OPFS: ${browserRecordingDirectoryName}/${recording.browserFileName}`;
  }

  return `Browser IndexedDB: ${databaseName}/${recordingStoreName}/${recording.localId}`;
}

async function shareRecordingFiles(files, packageFile) {
  if (packageFile && navigator.canShare?.({ files: [packageFile] }) && navigator.share) {
    await navigator.share({ files: [packageFile] });
    return;
  }

  if (navigator.canShare?.({ files }) && navigator.share) {
    await navigator.share({ files });
    return;
  }

  if (packageFile && downloadRecordingPackage(packageFile)) {
    return;
  }

  throw new Error("This browser cannot share or download the recording package.");
}

function downloadRecordingPackage(packageFile) {
  if (typeof document === "undefined"
    || typeof URL === "undefined"
    || typeof URL.createObjectURL !== "function") {
    return false;
  }

  const container = document.body || document.documentElement;
  if (!container) {
    return false;
  }

  const downloadBlob = new Blob([packageFile], { type: "application/octet-stream" });
  const objectUrl = URL.createObjectURL(downloadBlob);
  const link = document.createElement("a");
  link.href = objectUrl;
  link.download = packageFile.name;
  link.rel = "noopener";
  link.style.display = "none";
  container.appendChild(link);
  link.click();
  link.remove();
  window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
  return true;
}

async function createRecordingPackageFile(recording, files) {
  const packageName = recording.packageFileName
    || `${getFileNameWithoutExtension(recording.fileName)}${recordingPackageSuffix}`;
  const zipBlob = await createStoredZipBlob(files);
  return new File([zipBlob], packageName, {
    type: "application/zip",
    lastModified: Date.now()
  });
}

async function createStoredZipBlob(files) {
  const encoder = new TextEncoder();
  const localParts = [];
  const centralParts = [];
  let offset = 0;

  for (const file of files) {
    const fileNameBytes = encoder.encode(file.name);
    const data = new Uint8Array(await file.arrayBuffer());
    const crc = crc32(data);
    const dosDateTime = getDosDateTime(file.lastModified || Date.now());
    const localHeader = createZipLocalFileHeader(fileNameBytes, data.length, crc, dosDateTime);
    const centralHeader = createZipCentralDirectoryHeader(fileNameBytes, data.length, crc, dosDateTime, offset);

    localParts.push(localHeader, data);
    centralParts.push(centralHeader);
    offset += localHeader.length + data.length;
  }

  const centralDirectoryOffset = offset;
  const centralDirectorySize = centralParts.reduce((sum, part) => sum + part.length, 0);
  const endRecord = createZipEndOfCentralDirectory(files.length, centralDirectorySize, centralDirectoryOffset);

  return new Blob([...localParts, ...centralParts, endRecord], { type: "application/zip" });
}

function createZipLocalFileHeader(fileNameBytes, size, crc, dosDateTime) {
  const header = new Uint8Array(30 + fileNameBytes.length);
  const view = new DataView(header.buffer);
  view.setUint32(0, zipLocalFileHeaderSignature, true);
  view.setUint16(4, 20, true);
  view.setUint16(6, 0, true);
  view.setUint16(8, 0, true);
  view.setUint16(10, dosDateTime.time, true);
  view.setUint16(12, dosDateTime.date, true);
  view.setUint32(14, crc, true);
  view.setUint32(18, size, true);
  view.setUint32(22, size, true);
  view.setUint16(26, fileNameBytes.length, true);
  view.setUint16(28, 0, true);
  header.set(fileNameBytes, 30);
  return header;
}

function createZipCentralDirectoryHeader(fileNameBytes, size, crc, dosDateTime, localHeaderOffset) {
  const header = new Uint8Array(46 + fileNameBytes.length);
  const view = new DataView(header.buffer);
  view.setUint32(0, zipCentralDirectorySignature, true);
  view.setUint16(4, 20, true);
  view.setUint16(6, 20, true);
  view.setUint16(8, 0, true);
  view.setUint16(10, 0, true);
  view.setUint16(12, dosDateTime.time, true);
  view.setUint16(14, dosDateTime.date, true);
  view.setUint32(16, crc, true);
  view.setUint32(20, size, true);
  view.setUint32(24, size, true);
  view.setUint16(28, fileNameBytes.length, true);
  view.setUint16(30, 0, true);
  view.setUint16(32, 0, true);
  view.setUint16(34, 0, true);
  view.setUint16(36, 0, true);
  view.setUint32(38, 0, true);
  view.setUint32(42, localHeaderOffset, true);
  header.set(fileNameBytes, 46);
  return header;
}

function createZipEndOfCentralDirectory(fileCount, centralDirectorySize, centralDirectoryOffset) {
  const record = new Uint8Array(22);
  const view = new DataView(record.buffer);
  view.setUint32(0, zipEndOfCentralDirectorySignature, true);
  view.setUint16(4, 0, true);
  view.setUint16(6, 0, true);
  view.setUint16(8, fileCount, true);
  view.setUint16(10, fileCount, true);
  view.setUint32(12, centralDirectorySize, true);
  view.setUint32(16, centralDirectoryOffset, true);
  view.setUint16(20, 0, true);
  return record;
}

function getDosDateTime(value) {
  const date = new Date(value);
  const year = Math.max(1980, date.getFullYear());
  return {
    time: (date.getHours() << 11) | (date.getMinutes() << 5) | Math.floor(date.getSeconds() / 2),
    date: ((year - 1980) << 9) | ((date.getMonth() + 1) << 5) | date.getDate()
  };
}

let crc32Table = null;

function crc32(data) {
  if (!crc32Table) {
    crc32Table = new Uint32Array(256);
    for (let index = 0; index < 256; index++) {
      let value = index;
      for (let bit = 0; bit < 8; bit++) {
        value = value & 1 ? 0xedb88320 ^ (value >>> 1) : value >>> 1;
      }

      crc32Table[index] = value >>> 0;
    }
  }

  let crc = 0xffffffff;
  for (const byte of data) {
    crc = crc32Table[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  }

  return (crc ^ 0xffffffff) >>> 0;
}

async function extractRecordingFilesFromZipPackages(zipFiles) {
  const extractedFiles = [];
  for (const zipFile of zipFiles) {
    try {
      extractedFiles.push(...extractStoredZipFiles(zipFile, new Uint8Array(await zipFile.arrayBuffer())));
    } catch {
      // Ignore invalid ZIP packages; the import summary reports unmatched selections.
    }
  }

  return extractedFiles;
}

function extractStoredZipFiles(zipFile, bytes) {
  const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
  const endOffset = findZipEndOfCentralDirectory(view);
  if (endOffset < 0) {
    return [];
  }

  const decoder = new TextDecoder();
  const entryCount = view.getUint16(endOffset + 10, true);
  let centralOffset = view.getUint32(endOffset + 16, true);
  const files = [];

  for (let index = 0; index < entryCount; index++) {
    if (view.getUint32(centralOffset, true) !== zipCentralDirectorySignature) {
      break;
    }

    const compressionMethod = view.getUint16(centralOffset + 10, true);
    const compressedSize = view.getUint32(centralOffset + 20, true);
    const uncompressedSize = view.getUint32(centralOffset + 24, true);
    const fileNameLength = view.getUint16(centralOffset + 28, true);
    const extraLength = view.getUint16(centralOffset + 30, true);
    const commentLength = view.getUint16(centralOffset + 32, true);
    const localHeaderOffset = view.getUint32(centralOffset + 42, true);
    const rawName = decoder.decode(bytes.slice(centralOffset + 46, centralOffset + 46 + fileNameLength));
    const fileName = getZipEntryFileName(rawName);

    if (compressionMethod === 0
      && compressedSize === uncompressedSize
      && fileName
      && (fileName.endsWith(metadataFileSuffix) || isVideoFileName(fileName))) {
      const localFile = extractStoredZipFile(bytes, view, localHeaderOffset, compressedSize, fileName, zipFile.lastModified);
      if (localFile) {
        files.push(localFile);
      }
    }

    centralOffset += 46 + fileNameLength + extraLength + commentLength;
  }

  return files;
}

function extractStoredZipFile(bytes, view, localHeaderOffset, size, fileName, lastModified) {
  if (view.getUint32(localHeaderOffset, true) !== zipLocalFileHeaderSignature) {
    return null;
  }

  const fileNameLength = view.getUint16(localHeaderOffset + 26, true);
  const extraLength = view.getUint16(localHeaderOffset + 28, true);
  const dataStart = localHeaderOffset + 30 + fileNameLength + extraLength;
  const dataEnd = dataStart + size;
  if (dataEnd > bytes.length) {
    return null;
  }

  return new File([bytes.slice(dataStart, dataEnd)], fileName, {
    type: getRecordingFileContentType(fileName),
    lastModified: lastModified || Date.now()
  });
}

function findZipEndOfCentralDirectory(view) {
  const minOffset = Math.max(0, view.byteLength - 65557);
  for (let offset = view.byteLength - 22; offset >= minOffset; offset--) {
    if (view.getUint32(offset, true) === zipEndOfCentralDirectorySignature) {
      return offset;
    }
  }

  return -1;
}

function getZipEntryFileName(value) {
  const normalized = `${value || ""}`.replaceAll("\\", "/");
  const fileName = normalized.split("/").filter(Boolean).pop() || "";
  return sanitizeFileName(fileName);
}

function getRecordingFileContentType(fileName) {
  const extension = getFileExtension(fileName).toLowerCase();
  if (extension === "json") {
    return "application/json";
  }

  if (extension === "mp4") {
    return "video/mp4";
  }

  if (extension === "mov") {
    return "video/quicktime";
  }

  if (extension === "webm") {
    return "video/webm";
  }

  if (extension === "zip") {
    return "application/zip";
  }

  return "application/octet-stream";
}

function getFileNameWithoutExtension(fileName) {
  const extensionStart = fileName.lastIndexOf(".");
  return extensionStart > 0 ? fileName.slice(0, extensionStart) : fileName;
}

function isZipFileName(fileName) {
  return getFileExtension(fileName).toLowerCase() === "zip";
}
function buildFileName(fileNamePrefix, fileExtension, localId, perspective = defaultPerspective) {
  const safePrefix = sanitizeFileName(fileNamePrefix || "recording");
  const safePerspective = sanitizeFileName(normalizePerspective(perspective));
  const safeExtension = sanitizeFileName(fileExtension || "webm").replaceAll(".", "");
  return `${safePrefix}-${safePerspective}-${localId.slice(0, 8)}.${safeExtension || "webm"}`;
}

function getFileExtension(fileName) {
  const extensionStart = fileName.lastIndexOf(".");
  if (extensionStart < 0 || extensionStart === fileName.length - 1) {
    return "";
  }

  return sanitizeFileName(fileName.slice(extensionStart + 1));
}

function isSupportedVideoRecording(metadata, videoFile, contentType, fileExtension) {
  if (metadata.artifactType && metadata.artifactType !== recordingArtifactType) {
    return false;
  }

  if (!videoFile || videoFile.size <= 0) {
    return false;
  }

  const normalizedContentType = (contentType || videoFile.type || "").toLowerCase();
  const normalizedExtension = (fileExtension || getFileExtension(videoFile.name || "")).toLowerCase();

  return normalizedContentType.startsWith("video/")
    || normalizedExtension === "webm"
    || normalizedExtension === "mp4";
}

function isVideoFileName(fileName) {
  const extension = getFileExtension(fileName).toLowerCase();
  return extension === "webm" || extension === "mp4" || extension === "mov";
}

function selectRecordingFiles() {
  return new Promise(resolve => {
    const input = document.createElement("input");
    input.type = "file";
    input.multiple = true;
    input.accept = [
      "video/*",
      ".webm",
      ".mp4",
      ".mov",
      ".json",
      metadataFileSuffix,
      ".zip"
    ].join(",");
    input.style.position = "fixed";
    input.style.left = "-10000px";
    input.style.top = "0";

    input.addEventListener("change", () => {
      const files = input.files;
      input.remove();
      resolve(files);
    }, { once: true });

    input.addEventListener("cancel", () => {
      input.remove();
      resolve(null);
    }, { once: true });

    document.body.appendChild(input);
    input.click();
  });
}

function sanitizeFileName(value) {
  return value
    .trim()
    .replace(/[<>:"/\\|?*\u0000-\u001F]/g, "-")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
}

function normalizePerspective(value) {
  const normalized = `${value || ""}`.trim().toLowerCase();
  return normalized === "rear" || normalized === "back" || normalized === "hinten"
    ? "rear"
    : defaultPerspective;
}

function getMetadataValue(metadata, camelCaseName) {
  const pascalCaseName = `${camelCaseName[0].toUpperCase()}${camelCaseName.slice(1)}`;
  const value = metadata?.[camelCaseName] ?? metadata?.[pascalCaseName] ?? "";
  return value === null || value === undefined ? "" : `${value}`.trim();
}

function createLocalId() {
  if (crypto.randomUUID) {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
