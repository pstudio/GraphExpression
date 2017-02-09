using System;

namespace GraphExpressionDrawer.ViewModels
{
    public enum AxisNormalization
    {
        None,
        X,
        Y
    }

    // Settings for a Cartesian coordinate system
    public class CoordSettings : ObservableObject
    {
        private float _xStart;
        private float _xEnd;
        private float _yStart;
        private float _yEnd;

        /// <summary>
        /// The lowest value on the x axis.
        /// Must be less than XEnd.
        /// </summary>
        public float XStart
        {
            get { return _xStart; }
            set
            {
                if (value >= XEnd) return;
                if (value == XStart) return;
                
                _xStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The highest value on the x axis.
        /// Must be greater than XStart.
        /// </summary>
        public float XEnd
        {
            get { return _xEnd; }
            set {
                if (value <= XStart) return;
                if (value == XEnd) return;

                _xEnd = value;
                OnPropertyChanged(); }
        }

        /// <summary>
        /// The lowest value on the y axis.
        /// Must be less than YEnd.
        /// </summary>
        public float YStart
        {
            get { return _yStart; }
            set
            {
                if (value >= YEnd) return;
                if (value == YStart) return;

                _yStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The highest value on the y axis.
        /// Must be greater than YStart.
        /// </summary>
        public float YEnd
        {
            get { return _yEnd; }
            set
            {
                if (value <= YStart) return;
                if (value == YEnd) return;

                _yEnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates a new settings object for a Cartesian coordinate system.
        /// </summary>
        /// <param name="xStart">Lowest value of x axis. Must be less than xEnd</param>
        /// <param name="xEnd">Highest value of x axis. Must be greater than xStart</param>
        /// <param name="yStart">Lowest value of y axis. Must be less than yEnd</param>
        /// <param name="yEnd">Highest value of y axis. Must be greater than yStart</param>
        /// <exception cref="ArgumentException">Throws an ArgumentException if either start values are greater or equal to the corresponding end value</exception>
        public CoordSettings(float xStart, float xEnd, float yStart, float yEnd)
        {
            // Validate input
            if (xStart >= xEnd)
            {
                throw new ArgumentException("xStart must be less than xEnd");
            }

            if (yStart >= yEnd)
            {
                throw new ArgumentException("yStart must be less than yEnd");
            }

            XStart = xStart;
            XEnd = xEnd;
            YStart = yStart;
            YEnd = yEnd;

        }

        /// <summary>
        /// Creates a default settings object for a Cartesian coordinate system with the following values:
        /// XStart  = -10
        /// XEnd    = +10
        /// YStart  = -10
        /// YEnd    = +10
        /// </summary>
        public CoordSettings() : this(-10.0f, +10.0f, -10.0f, +10.0f) { }

        public void NormalizeAxis(AxisNormalization axis, float width, float height)
        {
            switch (axis)
            {
                case AxisNormalization.None:
                    break;
                case AxisNormalization.X:
                    YEnd = YStart + CalcNewAxisRange(XStart, XEnd, width, height);
                    break;
                case AxisNormalization.Y:
                    XEnd = XStart + CalcNewAxisRange(YStart, YEnd, height, width);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float CalcNewAxisRange(float start, float end, float srcLength, float dstLength)
        {
            var steps = Math.Abs(end - start);
            var stepLength = srcLength / steps;
            return dstLength / stepLength;
        }
    }

}
