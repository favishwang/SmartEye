using System;
using System.IO.MemoryMappedFiles;

namespace SmartEye.Feature
{
    /// <summary>
    /// UnityCamera 공유 메모리에서 이미지 프레임을 읽습니다.
    /// </summary>
    public class UnityCameraReader : IDisposable
    {
        private MemoryMappedFile? _mmf;
        private MemoryMappedViewAccessor? _accessor;
        private bool _disposed;

        public const int Width = 1920;
        public const int Height = 1080;
        public const int Channels = 3;
        public const string SharedMemoryName = "UnityCamera";

        public static int FrameSize => Width * Height * Channels;

        /// <summary>로그 메시지 전달용 콜백</summary>
        public Action<string>? OnLog { get; set; }

        public bool IsConnected => _mmf != null && _accessor != null;

        /// <summary>
        /// UnityCamera MMF에 연결합니다. Unity가 MMF를 생성한 후 호출해야 합니다.
        /// </summary>
        public bool TryConnect()
        {
            try
            {
                _mmf?.Dispose();
                _accessor?.Dispose();

                _mmf = MemoryMappedFile.OpenExisting(SharedMemoryName);
                _accessor = _mmf.CreateViewAccessor();
                Log($"[UnityCamera] 연결 성공 ({Width}x{Height})");
                return true;
            }
            catch (Exception ex)
            {
                Log($"[UnityCamera] 연결 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 프레임을 읽어 buffer에 복사합니다.
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool ReadFrame(byte[] buffer)
        {
            if (_accessor == null || buffer.Length < FrameSize)
            {
                Log($"[UnityCamera] 읽기 실패: 연결되지 않음 또는 버퍼 크기 부족");
                return false;
            }

            try
            {
                _accessor.ReadArray(0, buffer, 0, FrameSize);
                return true;
            }
            catch (Exception ex)
            {
                Log($"[UnityCamera] 읽기 오류: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _accessor?.Dispose();
            _mmf?.Dispose();
            _accessor = null;
            _mmf = null;
            _disposed = true;
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
