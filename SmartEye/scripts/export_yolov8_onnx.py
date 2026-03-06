"""YOLOv8n ONNX 모델 내보내기. 실행: pip install ultralytics && python export_yolov8_onnx.py"""
from pathlib import Path

try:
    from ultralytics import YOLO
except ImportError:
    print("ultralytics 설치 필요: pip install ultralytics")
    exit(1)

# yolov8n.pt 자동 다운로드 후 ONNX로 내보내기
model = YOLO("yolov8n.pt")
path = model.export(format="onnx", imgsz=640)

# Data 폴더로 복사
dest = Path(__file__).resolve().parent.parent / "Data" / "yolov8n.onnx"
dest.parent.mkdir(parents=True, exist_ok=True)
Path(path).rename(dest)
print(f"저장됨: {dest}")
