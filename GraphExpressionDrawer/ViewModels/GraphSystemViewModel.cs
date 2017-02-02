using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphExpressionDrawer.Models;

namespace GraphExpressionDrawer.ViewModels
{
    public enum AxisNormalization
    {
        None,
        X,
        Y
    }

    /// <summary>
    /// View-model for graphs in a Cartesian coordinate system
    /// </summary>
    public class GraphSystemViewModel : ObservableObject
    {
        private readonly Canvas _canvas;
        private Matrix _worldToScreenMatrix;

        public CoordSettings CoordSettings { get; }

        // Axis normalization
        public AxisNormalization AxisNormalization { get; set; }

        public GraphSystemViewModel(Canvas canvas)
        {
            _canvas = canvas;

            CoordSettings = new CoordSettings();
            AxisNormalization = AxisNormalization.None;

            _canvas.SizeChanged += (sender, e) => DrawGraphs(); //Canvas_SizeChanged;
            CoordSettings.PropertyChanged += (sender, e) => DrawGraphs(); //CoordSettings_PropertyChanged;
        }

        private void CoordSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Canvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            
        }

        private void DrawGraphs()
        {
            UpdateTransformationMatrix();
            _canvas.Children.Clear();
            DrawAxis();

            var ellipse = new Ellipse()
            {
                Stroke = Brushes.Blue,
                Fill = Brushes.BlueViolet,
                StrokeThickness = 5,
                Width = 50,
                Height = 50
            };
            var origin = WorldToScreen(new Point(0, 1));
            Canvas.SetLeft(ellipse, origin.X);
            Canvas.SetTop(ellipse, origin.Y);
            _canvas.Children.Add(ellipse);
        }

        private void UpdateTransformationMatrix()
        {
            _worldToScreenMatrix = Matrix.Identity;
            _worldToScreenMatrix.Translate(-CoordSettings.XStart, -CoordSettings.YEnd);

            // Scaling
            var scaleX = _canvas.ActualWidth/(CoordSettings.XEnd - CoordSettings.XStart);
            var scaleY = _canvas.ActualHeight/(CoordSettings.YStart - CoordSettings.YEnd);
            _worldToScreenMatrix.Scale(scaleX, scaleY);

            //_worldToScreenMatrix.Translate();
        }

        private void DrawAxis()
        {
            // X-axis
            var xAxisGroup = new GeometryGroup();
            xAxisGroup.Children.Add(new LineGeometry(WorldToScreen(new Point(CoordSettings.XStart, 0)), WorldToScreen(new Point(CoordSettings.XEnd, 0))));
            var xAxisPath = new Path() {StrokeThickness = 1, Stroke = Brushes.Black, Data = xAxisGroup};

            // Y-axis
            var yAxisGroup = new GeometryGroup();
            yAxisGroup.Children.Add(new LineGeometry(WorldToScreen(new Point(0, CoordSettings.YStart)), WorldToScreen(new Point(0, CoordSettings.YEnd))));
            var yAxisPath = new Path() { StrokeThickness = 1, Stroke = Brushes.Black, Data = yAxisGroup };

            _canvas.Children.Add(xAxisPath);
            _canvas.Children.Add(yAxisPath);
        }

        private Point WorldToScreen(Point point) => _worldToScreenMatrix.Transform(point);
    }

}
