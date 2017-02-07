using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphExpressionDrawer.ViewModels;

namespace GraphExpressionDrawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GraphSystemViewModel _graphSystem;

        public MainWindow()
        {
            InitializeComponent();

            _graphSystem = new GraphSystemViewModel(GraphCanvas);
            DataContext = _graphSystem;
        }

        private void AddGraphButton_OnClick(object sender, RoutedEventArgs e)
        {
            //_graphSystem.AddGraph(ExpressionTextBox.Text);
            _graphSystem.AddGraph();
        }
    }
}
