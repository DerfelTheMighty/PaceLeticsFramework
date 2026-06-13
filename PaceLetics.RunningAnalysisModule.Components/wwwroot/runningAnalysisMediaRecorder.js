const recorders = new WeakMap();
const databaseName = "paceletics-running-analysis";
const databaseVersion = 2;
const recordingStoreName = "recordings";
const settingsStoreName = "settings";
const recordingDirectoryKey = "recordingDirectory";
const metadataFileSuffix = ".paceletics.json";
const recordingArtifactType = "paceletics-running-analysis-recording";
const missingAthleteUserIdMessage = "The local recording metadata does not contain an athlete user id.";
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

    const directoryHandle = await requireRecordingDirectory();
    const localId = createLocalId();
    const recording = createRecordingMetadata(
      metadata,
      contentType || "video/webm",
      localId,
      directoryHandle);
    const fileHandle = await directoryHandle.getFileHandle(recording.fileName, { create: true });
    writable = await fileHandle.createWritable();
    recording.fileHandle = fileHandle;

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
      recording,
      writeChain: Promise.resolve(),
      writeError: null
    };

    recorder.addEventListener("dataavailable", event => {
      if (!event.data || event.data.size === 0) {
        return;
      }

      const chunk = event.data;
      state.writeChain = state.writeChain
        .then(async () => {
          await state.writable.write(chunk);
          state.recording.size += chunk.size;
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

    await state.writable.close();
    await writeRecordingMetadataFile(state.directoryHandle, state.recording);
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
    return getUnsupportedStorageStatus();
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

export async function getRecordingStorageStatus() {
  if (!window.showDirectoryPicker) {
    return getUnsupportedStorageStatus();
  }

  const directoryHandle = await loadRecordingDirectoryHandle();
  if (!directoryHandle) {
    return {
      isSupported: true,
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
      isConfigured: true,
      isReady: false,
      folderName: directoryHandle.name || "",
      permissionState: "denied",
      storageLocation: `Device folder selected in browser: ${directoryHandle.name || "(unnamed folder)"}`,
      errorMessage: error?.message || "The selected recording folder is not available."
    };
  }
}

export async function listSavedRecordings(analysisEventId, requestDirectoryPermission = false, folderOnly = false) {
  const scannedLocalIds = await importRecordingsFromConfiguredDirectory(requestDirectoryPermission);

  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readonly");
  const store = transaction.objectStore(recordingStoreName);
  const recordings = await requestToPromise(store.getAll());
  await transactionToPromise(transaction);

  return recordings
    .filter(recording => !folderOnly || (Array.isArray(scannedLocalIds) && scannedLocalIds.includes(recording.localId)))
    .filter(recording => !analysisEventId || recording.analysisEventId === analysisEventId)
    .map(toRecordingPayload)
    .sort((left, right) => left.recordedAt.localeCompare(right.recordedAt));
}

export async function openSavedRecording(localId) {
  const recording = await getSavedRecording(localId);
  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  if (recording.storageMode === "file-system") {
    const fileHandle = recording.fileHandle
      || await getFileHandleFromDirectory(recording.fileName);
    await ensureHandlePermission(fileHandle, "read", true);
    return await fileHandle.getFile();
  }

  if (recording.blob) {
    return recording.blob;
  }

  throw new Error("The saved recording file was not found on this device.");
}

export async function markSavedRecordingUploadStarted(localId) {
  return await updateSavedRecording(localId, recording => {
    recording.uploadStatus = "uploading";
    recording.uploadAttempts = (recording.uploadAttempts || 0) + 1;
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = "";
  });
}

export async function markSavedRecordingUploadSucceeded(localId, driveFileUrl) {
  return await updateSavedRecording(localId, recording => {
    recording.uploadStatus = "uploaded";
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = "";
    recording.driveFileUrl = driveFileUrl || "";
  });
}

export async function markSavedRecordingUploadFailed(localId, errorMessage) {
  return await updateSavedRecording(localId, recording => {
    recording.uploadStatus = "failed";
    recording.lastUploadAt = new Date().toISOString();
    recording.lastError = errorMessage || "Upload failed.";
  });
}

export async function deleteSavedRecording(localId) {
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  store.delete(localId);
  await transactionToPromise(transaction);
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

function createRecordingMetadata(metadata, contentType, localId, directoryHandle) {
  const analysisEventId = getMetadataValue(metadata, "analysisEventId");
  const analysisExternalEventId = getMetadataValue(metadata, "analysisExternalEventId");
  const courseId = getMetadataValue(metadata, "courseId");
  const analysisTitle = getMetadataValue(metadata, "analysisTitle");
  const analysisStartsAt = getMetadataValue(metadata, "analysisStartsAt");
  const participantId = getMetadataValue(metadata, "participantId");
  const athleteUserId = getMetadataValue(metadata, "athleteUserId");
  const athleteEmail = getMetadataValue(metadata, "athleteEmail");
  const participantName = getMetadataValue(metadata, "participantName");
  const fileNamePrefix = getMetadataValue(metadata, "fileNamePrefix");

  if (!athleteUserId || !participantId || !fileNamePrefix) {
    throw new Error("Recording metadata is incomplete.");
  }

  const fileExtension = contentType.includes("mp4") ? "mp4" : "webm";
  const recordedAt = new Date().toISOString();
  const fileName = buildFileName(fileNamePrefix, fileExtension, localId);
  const metadataFileName = `${fileName}${metadataFileSuffix}`;

  return {
    artifactType: recordingArtifactType,
    localId,
    analysisEventId,
    analysisExternalEventId,
    courseId,
    analysisTitle,
    analysisStartsAt,
    participantId,
    athleteUserId,
    athleteEmail,
    participantName,
    fileName,
    contentType,
    fileExtension,
    size: 0,
    recordedAt,
    storageMode: "file-system",
    folderName: directoryHandle.name || "",
    metadataFileName,
    uploadStatus: "queued",
    uploadAttempts: 0,
    lastUploadAt: null,
    lastError: "",
    driveFileUrl: ""
  };
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
    if (handle.kind !== "file" || !metadataFileName.endsWith(metadataFileSuffix)) {
      continue;
    }

    const recording = await readRecordingMetadataFile(directoryHandle, metadataFileName, handle);
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
      analysisEventId: metadata.analysisEventId || "",
      analysisExternalEventId: metadata.analysisExternalEventId || "",
      courseId: metadata.courseId || "",
      analysisTitle: metadata.analysisTitle || "",
      analysisStartsAt: metadata.analysisStartsAt || null,
      participantId: metadata.participantId,
      athleteUserId: metadata.athleteUserId,
      athleteEmail: metadata.athleteEmail || "",
      participantName: metadata.participantName || "",
      fileName: metadata.fileName,
      contentType,
      fileExtension,
      size: metadata.size || videoFile.size,
      recordedAt: metadata.recordedAt || new Date(videoFile.lastModified || Date.now()).toISOString(),
      storageMode: "file-system",
      folderName: directoryHandle.name || metadata.folderName || "",
      metadataFileName,
      fileHandle,
      uploadStatus: isMissingAthleteUserId ? "failed" : metadata.uploadStatus || "queued",
      uploadAttempts: metadata.uploadAttempts || 0,
      lastUploadAt: metadata.lastUploadAt || null,
      lastError: isMissingAthleteUserId ? missingAthleteUserIdMessage : metadata.lastError || "",
      driveFileUrl: metadata.driveFileUrl || ""
    };
  } catch {
    return null;
  }
}

async function writeRecordingMetadataFile(directoryHandle, recording) {
  const metadataFileName = recording.metadataFileName || `${recording.fileName}${metadataFileSuffix}`;
  recording.metadataFileName = metadataFileName;

  const metadataHandle = await directoryHandle.getFileHandle(metadataFileName, { create: true });
  const writable = await metadataHandle.createWritable();
  await writable.write(JSON.stringify(toRecordingMetadata(recording), null, 2));
  await writable.close();
}

async function tryWriteRecordingMetadataFile(recording) {
  if (recording.storageMode !== "file-system") {
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
      if (!database.objectStoreNames.contains(recordingStoreName)) {
        const store = database.createObjectStore(recordingStoreName, { keyPath: "localId" });
        store.createIndex("analysisEventId", "analysisEventId", { unique: false });
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
  return {
    localId: recording.localId,
    analysisEventId: recording.analysisEventId || "",
    analysisExternalEventId: recording.analysisExternalEventId || "",
    courseId: recording.courseId || "",
    analysisTitle: recording.analysisTitle || "",
    analysisStartsAt: recording.analysisStartsAt || null,
    participantId: recording.participantId,
    athleteUserId: recording.athleteUserId || "",
    athleteEmail: recording.athleteEmail || "",
    participantName: recording.participantName || "",
    fileName: recording.fileName,
    contentType: recording.contentType,
    fileExtension: recording.fileExtension,
    size: recording.size,
    recordedAt: recording.recordedAt,
    storageLocation: getRecordingStorageLocation(recording),
    uploadStatus: recording.uploadStatus || "queued",
    uploadAttempts: recording.uploadAttempts || 0,
    lastUploadAt: recording.lastUploadAt || null,
    lastError: recording.lastError || "",
    driveFileUrl: recording.driveFileUrl || ""
  };
}

function toRecordingMetadata(recording) {
  return {
    artifactType: recordingArtifactType,
    localId: recording.localId,
    analysisEventId: recording.analysisEventId || "",
    analysisExternalEventId: recording.analysisExternalEventId || "",
    courseId: recording.courseId || "",
    analysisTitle: recording.analysisTitle || "",
    analysisStartsAt: recording.analysisStartsAt || null,
    participantId: recording.participantId,
    athleteUserId: recording.athleteUserId || "",
    athleteEmail: recording.athleteEmail || "",
    participantName: recording.participantName || "",
    fileName: recording.fileName,
    contentType: recording.contentType,
    fileExtension: recording.fileExtension,
    size: recording.size,
    recordedAt: recording.recordedAt,
    storageMode: "file-system",
    folderName: recording.folderName || "",
    metadataFileName: recording.metadataFileName || `${recording.fileName}${metadataFileSuffix}`,
    uploadStatus: recording.uploadStatus || "queued",
    uploadAttempts: recording.uploadAttempts || 0,
    lastUploadAt: recording.lastUploadAt || null,
    lastError: recording.lastError || "",
    driveFileUrl: recording.driveFileUrl || ""
  };
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

function getUnsupportedStorageStatus() {
  return {
    isSupported: false,
    isConfigured: false,
    isReady: false,
    folderName: "",
    permissionState: "",
    storageLocation: "",
    errorMessage: "This browser does not support direct recording to a device folder."
  };
}

function getRecordingStorageLocation(recording) {
  if (recording.storageMode === "file-system") {
    return `Device folder: ${recording.folderName || "(selected folder)"}/${recording.fileName}`;
  }

  return `Browser IndexedDB: ${databaseName}/${recordingStoreName}/${recording.localId}`;
}

function buildFileName(fileNamePrefix, fileExtension, localId) {
  const safePrefix = sanitizeFileName(fileNamePrefix || "recording");
  const safeExtension = sanitizeFileName(fileExtension || "webm").replaceAll(".", "");
  return `${safePrefix}-${localId.slice(0, 8)}.${safeExtension || "webm"}`;
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

function sanitizeFileName(value) {
  return value
    .trim()
    .replace(/[<>:"/\\|?*\u0000-\u001F]/g, "-")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
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
