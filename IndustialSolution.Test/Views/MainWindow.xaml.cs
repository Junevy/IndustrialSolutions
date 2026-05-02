using IndustialSolution.Test.ViewModels;
using System.Windows;

namespace IndustialSolution.Test.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}