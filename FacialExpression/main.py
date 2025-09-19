from fastapi import FastAPI, File, UploadFile
from ultralytics import YOLO
import cv2
import numpy as np

app = FastAPI()
model = YOLO("best.pt")  # the trained model

@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    contents = await file.read()
    np_img = np.frombuffer(contents, np.uint8)
    frame = cv2.imdecode(np_img, cv2.IMREAD_COLOR)

    results = model(frame)

    expression = None
    for r in results:
        for box in r.boxes:
            cls_id = int(box.cls[0])
            expression = model.names[cls_id]  # take the first detected expression
            break  # stop after first detection
        if expression:
            break

    if not expression:
        return {"expression": "neutral"}  # fallback if no face detected

    return {"expression": expression}
@app.get("/health")
def health():
    return {"status": "ok"}



@app.post("/predict-detailed")
async def predict_detailed(file: UploadFile = File(...)):
    contents = await file.read()
    np_img = np.frombuffer(contents, np.uint8)
    frame = cv2.imdecode(np_img, cv2.IMREAD_COLOR)

    results = model(frame)

    detections = []
    for r in results:
        for box in r.boxes:
            cls_id = int(box.cls[0])
            x1, y1, x2, y2 = box.xyxy[0].tolist()  # bounding box coords
            detections.append({
                "expression": model.names[cls_id],
                "bbox": [x1, y1, x2, y2]
            })

    return {"detections": detections or [{"expression": "neutral"}]}
if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="127.0.0.1", port=8000, reload=True)