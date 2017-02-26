using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphExpressionDrawer.Models;
using GraphExpressionDrawer.ViewModels.Commands;
using GraphExpressionEvaluator.Interpreter;
using ovp;

namespace GraphExpressionDrawer.ViewModels
{

    #region Enums

    public enum GraphRenderMethod
    {
        Linear,
        Bezier
    }

    #endregion


    /// <summary>
    /// View-model for graphs in a Cartesian coordinate system
    /// </summary>
    public class GraphSystemViewModel : ObservableObject
    {
        private static readonly Interpreter Interpreter = new Interpreter();

        private readonly Canvas _canvas;
        private RectangleGeometry _graphClippingBounds;

        private Matrix _worldToScreenMatrix;
        private GraphRenderMethod _graphRenderMethod;
        private int _graphResolution;
        private AxisNormalization _axisNormalization;

        public ObservableCollection<GraphViewModel> Graphs { get; }
        public GraphViewModel CurrentGraph { get; set; }

        public RelayCommand AddGraphCommand { get; private set; }

        public CoordSettings CoordSettings { get; }

        /// <summary>
        ///  Axis normalization
        /// </summary>
        public AxisNormalization AxisNormalization
        {
            get { return _axisNormalization; }
            set
            {
                _axisNormalization = value;
                CoordSettings.NormalizeAxis(_axisNormalization, (float) _canvas.ActualWidth, (float) _canvas.ActualHeight);
            }
        }

        /// <summary>
        /// Indicates how the graphs should be drawn.
        /// Linear - The graph consists of straight lines
        /// Bezier - The graph is built from bezier curves that approximate the graphs curvature
        /// </summary>
        public GraphRenderMethod GraphRenderMethod
        {
            get { return _graphRenderMethod; }
            set
            {
                _graphRenderMethod = value; 
                DrawGraphSystem();
            }
        }

        /// <summary>
        /// Indicates how high a resolution should be used to draw the graphs.
        /// Higher number indicates better quality.
        /// </summary>
        public int GraphResolution
        {
            get { return _graphResolution; }
            set
            {
                _graphResolution = value; 
                DrawGraphSystem();
            }
        }

        public GraphSystemViewModel(Canvas canvas)
        {
            _canvas = canvas;

            Graphs = new ObservableCollection<GraphViewModel>();
            NewGraph();

            CoordSettings = new CoordSettings();
            AxisNormalization = AxisNormalization.None;
            GraphRenderMethod = GraphRenderMethod.Linear;
            GraphResolution = 1;

            AddGraphCommand = new RelayCommand((parameter) => AddGraph(), (parameter) => CurrentGraphValid());

            _canvas.SizeChanged += OnChanged; //DrawGraphSystem(); //Canvas_SizeChanged;
            CoordSettings.PropertyChanged += OnChanged;
            Graphs.CollectionChanged += (sender, e) => DrawGraphSystem();
        }

        private void OnChanged(object sender, EventArgs e)
        {
            CoordSettings.NormalizeAxis(AxisNormalization, (float) _canvas.ActualWidth, (float) _canvas.ActualHeight);
            DrawGraphSystem();
        }

        private void NewGraph()
        {
            CurrentGraph = new GraphViewModel("");
            CurrentGraph.GraphColor = Color.FromRgb(0, 0, 0);
            CurrentGraph.PropertyChanged += (sender, e) => DrawGraphSystem();
            OnPropertyChanged(nameof(CurrentGraph));

        }

        public void AddGraph()
        {
            //var graph = new GraphViewModel(CurrentGraph.Expression);
            //graph.PropertyChanged += (sender, e) => DrawGraphSystem();
            //Graphs.Add(graph);

            //CurrentGraph.Expression = "";

            Graphs.Add(CurrentGraph);

            NewGraph();
        }

        #region Graph Drawing

        // Graph drawing is based on:
        // http://csharphelper.com/blog/2014/09/draw-graph-wpf-c/

        private void DrawGraphSystem()
        {
            _graphClippingBounds = new RectangleGeometry(new Rect(new Size(_canvas.ActualWidth, _canvas.ActualHeight)));

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
            var xAxisPath = new Path
            {
                StrokeThickness = 1, Stroke = Brushes.Black, Data = xAxisGroup, Clip = _graphClippingBounds, ClipToBounds = true
            };

            // Y-axis
            var yAxisGroup = new GeometryGroup();
            yAxisGroup.Children.Add(new LineGeometry(WorldToScreen(new Point(0, CoordSettings.YStart)), WorldToScreen(new Point(0, CoordSettings.YEnd))));
            var yAxisPath = new Path
            {
                StrokeThickness = 1, Stroke = Brushes.Black, Data = yAxisGroup, Clip = _graphClippingBounds, ClipToBounds = true
            };

            _canvas.Children.Add(xAxisPath);
            _canvas.Children.Add(yAxisPath);
        }

        private void DrawGraphs()
        {
            foreach (var graph in Graphs.Where(graph => graph.DrawGraph))
            {
                DrawGraph(graph);
            }

            if (CurrentGraphValid())
            {
                DrawGraph(CurrentGraph);
            }
        }

        private bool CurrentGraphValid()
        {
            return CurrentGraph.Graph.Valid && !CurrentGraph.Expression.Equals(string.Empty);
        }

        private void DrawGraph(GraphViewModel graph)
        {
            var points = new PointCollection();


            for (float x = CoordSettings.XStart - 1; x <= CoordSettings.XEnd + 1; x += 1f/GraphResolution)
                // we expand the range to iterate over to prevent any weird tangent errors at the ends of the graph
            {
                try
                {
                    var y = Interpreter.Evaluate(graph.Graph.ByteCode, x);
                    var point = new Point(x, y);
                    points.Add(WorldToScreen(point));
                }
                catch (Interpreter.InterpreterException e)
                {
                    MessageBox.Show($"Graph: '{graph.Expression}' could not be drawn.{Environment.NewLine}{e}", "Draw Graph Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                    //Console.WriteLine(e);
                }
            }

            UIElement graphElement;

            switch (GraphRenderMethod)
            {
                case GraphRenderMethod.Linear:
                    graphElement = DrawLineGraph(graph, points);
                    break;
                case GraphRenderMethod.Bezier:
                    graphElement = DrawBezierGraph(graph, points);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            graphElement.Clip = _graphClippingBounds;
            graphElement.ClipToBounds = true;

            _canvas.Children.Add(graphElement);
        }

        private static Path DrawBezierGraph(GraphViewModel graph, PointCollection points)
        {
            Point[] controlPoints1, controlPoints2;
            BezierSpline.GetCurveControlPoints(points.ToArray(), out controlPoints1, out controlPoints2);

            var segments = new PathSegmentCollection();
            for (int i = 0; i < controlPoints1.Length; i++)
            {
                segments.Add(new BezierSegment(controlPoints1[i], controlPoints2[i], points[i + 1], true));
            }

            var figure = new PathFigure(points[0], segments, false);
            var geometry = new PathGeometry(new[] {figure});
            var path = new Path()
            {
                Stroke = new SolidColorBrush(graph.GraphColor), StrokeThickness = 1, Data = geometry
            };

            return path;
        }

        private static Polyline DrawLineGraph(GraphViewModel graph, PointCollection points) => new Polyline()
        {
            Stroke = new SolidColorBrush(graph.GraphColor), StrokeThickness = 1, Points = points
        };

        private Point WorldToScreen(Point point) => _worldToScreenMatrix.Transform(point);

        #endregion
    }
}
