const recorders = new WeakMap();
const databaseName = "paceletics-running-analysis";
const databaseVersion = 2;
const recordingStoreName = "recordings";
const settingsStoreName = "settings";
const recordingDirectoryKey = "recordingDirectory";

export async function startRecording(videoElement) {
  if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    throw new Error("Camera access is not supported by this browser.");
  }

  if (!window.MediaRecorder) {
    throw new Error("Video recording is not supported by this browser.");
  }

  disposeRecorder(videoElement);

  const stream = await navigator.mediaDevices.getUserMedia({
    video: { facingMode: "environment" },
    audio: false
  });

  const contentType = getSupportedContentType();
  const chunks = [];
  const recorder = new MediaRecorder(
    stream,
    contentType ? { mimeType: contentType } : undefined
  );

  recorder.addEventListener("dataavailable", event => {
    if (event.data && event.data.size > 0) {
      chunks.push(event.data);
    }
  });

  videoElement.srcObject = stream;
  await videoElement.play();
  recorder.start();

  recorders.set(videoElement, {
    recorder,
    stream,
    chunks,
    contentType: contentType || recorder.mimeType || "video/webm"
  });
}

export async function stopRecording(videoElement, metadata = {}) {
  const state = recorders.get(videoElement);
  if (!state) {
    throw new Error("No active recording was found.");
  }

  const blob = await new Promise((resolve, reject) => {
    state.recorder.addEventListener("stop", () => {
      resolve(new Blob(state.chunks, { type: state.contentType }));
    }, { once: true });

    state.recorder.addEventListener("error", event => {
      reject(event.error || new Error("Recording failed."));
    }, { once: true });

    state.recorder.stop();
  });

  stopStream(videoElement, state.stream);
  recorders.delete(videoElement);

  const contentType = blob.type || state.contentType || "video/webm";
  return await saveRecording(blob, contentType, metadata);
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
  await saveSetting(recordingDirectoryKey, directoryHandle);

  return await getRecordingStorageStatus();
}

export async function getRecordingStorageStatus() {
  if (!window.showDirectoryPicker) {
    return getUnsupportedStorageStatus();
  }

  const directoryHandle = await loadSetting(recordingDirectoryKey);
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

export async function listSavedRecordings(analysisEventId) {
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readonly");
  const store = transaction.objectStore(recordingStoreName);
  const recordings = await requestToPromise(store.getAll());
  await transactionToPromise(transaction);

  return recordings
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

export function disposeRecorder(videoElement) {
  const state = recorders.get(videoElement);
  if (!state) {
    return;
  }

  if (state.recorder && state.recorder.state !== "inactive") {
    state.recorder.stop();
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

async function saveRecording(blob, contentType, metadata) {
  const analysisEventId = getMetadataValue(metadata, "analysisEventId");
  const participantId = getMetadataValue(metadata, "participantId");
  const participantName = getMetadataValue(metadata, "participantName");
  const fileNamePrefix = getMetadataValue(metadata, "fileNamePrefix");

  if (!analysisEventId || !participantId || !fileNamePrefix) {
    throw new Error("Recording metadata is incomplete.");
  }

  const fileExtension = contentType.includes("mp4") ? "mp4" : "webm";
  const localId = createLocalId();
  const recordedAt = new Date().toISOString();
  const fileName = buildFileName(fileNamePrefix, fileExtension, localId);
  const directoryHandle = await requireRecordingDirectory();
  const fileHandle = await directoryHandle.getFileHandle(fileName, { create: true });
  const writable = await fileHandle.createWritable();
  await writable.write(blob);
  await writable.close();

  const recording = {
    localId,
    analysisEventId,
    participantId,
    participantName,
    fileName,
    contentType,
    fileExtension,
    size: blob.size,
    recordedAt,
    storageMode: "file-system",
    folderName: directoryHandle.name || "",
    fileHandle,
    uploadStatus: "queued",
    uploadAttempts: 0,
    lastUploadAt: null,
    lastError: "",
    driveFileUrl: ""
  };

  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  store.put(recording);
  await transactionToPromise(transaction);

  return toRecordingPayload(recording);
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

  const directoryHandle = await loadSetting(recordingDirectoryKey);
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

async function updateSavedRecording(localId, updater) {
  const database = await openDatabase();
  const transaction = database.transaction(recordingStoreName, "readwrite");
  const store = transaction.objectStore(recordingStoreName);
  const recording = await requestToPromise(store.get(localId));

  if (!recording) {
    throw new Error("The saved recording was not found on this device.");
  }

  updater(recording);
  store.put(recording);
  await transactionToPromise(transaction);

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
    analysisEventId: recording.analysisEventId,
    participantId: recording.participantId,
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
  return (metadata?.[camelCaseName] || metadata?.[pascalCaseName] || "").trim();
}

function createLocalId() {
  if (crypto.randomUUID) {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
