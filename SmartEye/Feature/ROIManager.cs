using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartEye.Feature
{
    /// <summary>
    /// ROI(관심 영역) 목록을 관리하고 텍스트 표현을 제공합니다.
    /// </summary>
    public class ROIManager
    {
        private readonly List<ROIRegion> _regions = new();
        private readonly object _lock = new();

        /// <summary>ROI 영역 목록 (인덱스 순서)</summary>
        public IReadOnlyList<ROIRegion> Regions
        {
            get { lock (_lock) { return _regions.ToList(); } }
        }

        /// <summary>
        /// ROI 영역 목록을 갱신합니다.
        /// </summary>
        /// <param name="rects">검출된 사각형 목록 (면적 내림차순 정렬됨)</param>
        public void Update(IEnumerable<Rect> rects)
        {
            lock (_lock)
            {
                _regions.Clear();
                int index = 0;
                foreach (var rect in rects)
                {
                    _regions.Add(new ROIRegion(index, rect));
                    index++;
                }
            }
        }

        /// <summary>
        /// ROI 목록을 텍스트로 반환합니다.
        /// </summary>
        public string ToDisplayText()
        {
            var regions = Regions;
            if (regions.Count == 0)
                return "ROI: 없음";

            var sb = new StringBuilder();
            sb.AppendLine($"ROI: {regions.Count}개");
            foreach (var r in regions)
            {
                sb.AppendLine($"  [{r.Index}] X:{r.Rect.X} Y:{r.Rect.Y} W:{r.Rect.Width} H:{r.Rect.Height}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 단일 ROI 영역 정보
        /// </summary>
        public record ROIRegion(int Index, Rect Rect);
    }
}
