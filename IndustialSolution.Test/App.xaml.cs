using IndustialSolution.Test.ViewModels;
using IndustialSolution.Test.Views;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using IndustrialCameraManager.HikVision;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace IndustialSolution.Test
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        public IServiceProvider Provider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitialContainer();

            Window mw = this.Provider.GetRequiredService<MainWindow>();
            mw.Show();
        }

        private void InitialContainer()
        {
            var container = new ServiceCollection();

            container.AddSingleton<MainWindow>();
            container.AddSingleton<MainWindowViewModel>();

            container.AddSingleton<ICameraProvider, HikCameraProvider>();
            container.AddSingleton<CameraManager>();

            this.Provider = container.BuildServiceProvider();
        }
    }

}
