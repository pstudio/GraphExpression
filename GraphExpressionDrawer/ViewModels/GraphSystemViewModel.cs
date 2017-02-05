using System;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphExpressionDrawer.Models;
using GraphExpressionEvaluator.Interpreter;

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
        private static readonly Interpreter Interpreter = new Interpreter();

        private readonly Canvas _canvas;
        private Matrix _worldToScreenMatrix;

        public ObservableCollection<GraphViewModel> Graphs { get; }

        public CoordSettings CoordSettings { get; }

        // Axis normalization
        public AxisNormalization AxisNormalization { get; set; }

        public GraphSystemViewModel(Canvas canvas)
        {
            _canvas = canvas;

            Graphs = new ObservableCollection<GraphViewModel>();

            CoordSettings = new CoordSettings();
            AxisNormalization = AxisNormalization.None;

            _canvas.SizeChanged += (sender, e) => DrawGraphSystem(); //Canvas_SizeChanged;
            CoordSettings.PropertyChanged += (sender, e) => DrawGraphSystem(); //CoordSettings_PropertyChanged;
            Graphs.CollectionChanged += (sender, e) => DrawGraphSystem();
        }

        public void AddGraph(string expression)
        {
            var graph = new GraphViewModel(expression);
            graph.PropertyChanged += (sender, e) => DrawGraphSystem();
            Graphs.Add(graph);
        }

        private void DrawGraphSystem()
        {
            UpdateTransformationMatrix();
            _canvas.Children.Clear();
            DrawAxis();
            DrawGraphs();

            //var ellipse = new Ellipse()
            //{
            //    Stroke = Brushes.Blue,
            //    Fill = Brushes.BlueViolet,
            //    StrokeThickness = 5,
            //    Width = 50,
            //    Height = 50
            //};

            //var origin = WorldToScreen(new Point(0, 1));
            //Canvas.SetLeft(ellipse, origin.X);
            //Canvas.SetTop(ellipse, origin.Y);
            //_canvas.Children.Add(ellipse);
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

        private void DrawGraphs()
        {
            foreach (var graph in Graphs)
            {
                if (graph.DrawGraph)
                    DrawGraph(graph);
            }
        }

        private void DrawGraph(GraphViewModel graph)
        {
            var pointCollection = new PointCollection();

            for (float x = CoordSettings.XStart; x <= CoordSettings.XEnd; x++)
            {
                var y = Interpreter.Evaluate(graph.Graph.ByteCode, x);
                var point = new Point(x, y);
                pointCollection.Add(WorldToScreen(point));
            }

            var polyline = new Polyline()
            {
                Stroke = new SolidColorBrush(graph.GraphColor),
                StrokeThickness = 1,
                Points = pointCollection
            };

            _canvas.Children.Add(polyline);
        }

        private Point WorldToScreen(Point point) => _worldToScreenMatrix.Transform(point);
    }

}
