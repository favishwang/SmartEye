# YOLOv5 ONNX 모델

## yolov5n.onnx 다운로드

CarDetectionTest는 **YOLOv5** ONNX 모델을 사용합니다.

### 방법 1: SourceForge에서 다운로드

- https://sourceforge.net/projects/yolov5.mirror/files/v7.0/yolov5n.onnx/
- `yolov5n.onnx` 파일을 이 `Data` 폴더에 저장

### 방법 2: PyTorch에서 ONNX로 변환

```bash
git clone https://github.com/ultralytics/yolov5
cd yolov5
pip install -r requirements.txt
python export.py --weights yolov5n.pt --include onnx
```

생성된 `yolov5n.onnx`를 `CarDetectionTest/Data/` 폴더로 복사하세요.

### 참고

- **OnnxRuntime**: Microsoft.ML.OnnxRuntime NuGet 패키지 사용
- **입력**: 640×640, NCHW, RGB, 0~1 정규화
- **출력**: [1, 25200, 85] (x,y,w,h, objectness, 80 class scores)
- **차량 클래스(COCO)**: car=2, motorcycle=3, bus=5, truck=7
