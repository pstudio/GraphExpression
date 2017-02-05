using System.Windows;
using System.Windows.Media;
using GraphExpressionDrawer.Models;

namespace GraphExpressionDrawer.ViewModels
{
    public class GraphViewModel : ObservableObject
    {
        private static readonly IExpressionCompiler GraphCompiler = new ExpressionCompiler();
        private bool _drawGraph;
        private Color _graphColor;

        /// <summary>
        /// The model this is a view for
        /// </summary>
        public Graph Graph { get; }

        /// <summary>
        /// The expression for the graph
        /// </summary>
        public string Expression
        {
            get { return Graph.Expression; }
            set
            {
                Graph.Expression = value;
                OnPropertyChanged(nameof(Expression));
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(ErrorMessageVisibility));
            }
        }

        /// <summary>
        /// If there's a problem with parsing the expression this tells what the problem is
        /// </summary>
        public string ErrorMessage => Graph.Error;
        /// <summary>
        /// Should the error message be shown?
        /// </summary>
        public Visibility ErrorMessageVisibility => Graph.Valid ? Visibility.Hidden : Visibility.Visible;

        /// <summary>
        /// Should the graph be rendered
        /// </summary>
        public bool DrawGraph
        {
            get { return _drawGraph; }
            set
            {
                _drawGraph = value;
                OnPropertyChanged(nameof(DrawGraph));
            }
        }

        /// <summary>
        /// The color of the rendered graph
        /// </summary>
        public Color GraphColor
        {
            get { return _graphColor; }
            set
            {
                _graphColor = value;
                OnPropertyChanged(nameof(GraphColor));
            }
        }


        public GraphViewModel(string expression)
        {
            Graph = new Graph(GraphCompiler) {Expression = expression};
            DrawGraph = true;
            GraphColor = Color.FromRgb(0, 0, 255);
        }
    }

}
