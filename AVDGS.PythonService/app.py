import cv2
import time
from flask import Flask, Response, jsonify
from ultralytics import YOLO

app = Flask(__name__)

# Load YOLO once
model = YOLO("yolov8n.pt")

# Open webcam
cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 960)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 540)

latest_status = {
    "aircraftDetected": False,
    "count": 0,
    "lastSeen": None
}

def gen_frames():
    global latest_status

    while True:
        success, frame = cap.read()
        if not success:
            time.sleep(0.2)
            continue

        # Detect airplane only (COCO class 4)
        results = model.predict(source=frame, classes=[4], conf=0.5, verbose=False)
        r = results[0]

        annotated = r.plot()

        detected = len(r.boxes) > 0
        latest_status["aircraftDetected"] = detected
        latest_status["count"] = len(r.boxes)
        if detected:
            latest_status["lastSeen"] = time.strftime("%Y-%m-%d %H:%M:%S")

        # Encode JPEG
        ok, buffer = cv2.imencode(".jpg", annotated)
        if not ok:
            continue

        frame_bytes = buffer.tobytes()

        yield (b"--frame\r\n"
               b"Content-Type: image/jpeg\r\n\r\n" + frame_bytes + b"\r\n")

@app.route("/video")
def video():
    return Response(gen_frames(), mimetype="multipart/x-mixed-replace; boundary=frame")

@app.route("/status")
def status():
    return jsonify(latest_status)

@app.route("/health")
def health():
    return "OK", 200

if __name__ == "__main__":
    # 0.0.0.0 lets other devices in LAN access it (optional)
    app.run(host="0.0.0.0", port=5001, debug=False, threaded=True)
