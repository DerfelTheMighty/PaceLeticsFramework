const recorders = new WeakMap();

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

export async function stopRecording(videoElement) {
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
  const buffer = await blob.arrayBuffer();

  return {
    data: new Uint8Array(buffer),
    contentType,
    fileExtension: contentType.includes("mp4") ? "mp4" : "webm"
  };
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
