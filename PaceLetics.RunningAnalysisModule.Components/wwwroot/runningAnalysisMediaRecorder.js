const recorders = new WeakMap();
const databaseName = "paceletics-running-analysis";
const databaseVersion = 1;
const recordingStoreName = "recordings";

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
  if (!recording || !recording.blob) {
    throw new Error("The saved recording was not found on this device.");
  }

  return recording.blob;
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
  const recording = {
    localId,
    analysisEventId,
    participantId,
    participantName,
    fileName: `${fileNamePrefix}.${fileExtension}`,
    contentType,
    fileExtension,
    size: blob.size,
    recordedAt,
    blob
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
    fileName: recording.fileName,
    contentType: recording.contentType,
    fileExtension: recording.fileExtension,
    size: recording.size,
    recordedAt: recording.recordedAt
  };
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
