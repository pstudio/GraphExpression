using System;

namespace GraphExpressionDrawer.ViewModels
{
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
            set { if (value < XEnd) _xStart = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The highest value on the x axis.
        /// Must be greater than XStart.
        /// </summary>
        public float XEnd
        {
            get { return _xEnd; }
            set { if (value > XStart) _xEnd = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The lowest value on the y axis.
        /// Must be less than YEnd.
        /// </summary>
        public float YStart
        {
            get { return _yStart; }
            set { if (value < YEnd) _yStart = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The highest value on the y axis.
        /// Must be greater than YStart.
        /// </summary>
        public float YEnd
        {
            get { return _yEnd; }
            set { if (value > YStart) _yEnd = value; OnPropertyChanged(); }
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
    }

}
