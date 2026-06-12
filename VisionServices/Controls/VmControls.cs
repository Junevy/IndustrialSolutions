using System.Windows;
using System.Windows.Controls;
using VisionServices.Core;
using VMControls.Interface;
using VMControls.WPF.Release;

namespace VisionServices.Controls
{
    internal class VmControls : IVisionControls
    {
        private VmRenderControl imageRender = new();
        private VmMainViewConfigControl mainRender = new();

        public FrameworkElement ImageRender { get => imageRender; }
        public FrameworkElement MainRender { get => mainRender; }
        //public bool ChangeBackground { get; }

        public VmControls()
        {
            if (this.ImageRender is VmRenderControl render)
            {
                render.ChangeTopBarVisibility(false);
                render.Loaded += ChangeBackground;

                // 强制让它完成初始化（调用内部的OnApplyTemplate等）
                render.ApplyTemplate();
                render.Arrange(new Rect(0, 0, 100, 100));
                render.Measure(new Size(100, 100));

                // 关键：强制渲染一次，触发所有原生资源加载
                var presenter = new ContentPresenter { Content = render };
                presenter.Measure(new Size(100, 100));
                presenter.Arrange(new Rect(0, 0, 100, 100));
                presenter.UpdateLayout();
            }

            if (this.MainRender is VmMainViewConfigControl mainConfig)
            {
                // 强制让它完成初始化（调用内部的OnApplyTemplate等）
                mainConfig.ApplyTemplate();
                mainConfig.Arrange(new Rect(0, 0, 100, 100));
                mainConfig.Measure(new Size(100, 100));

                // 关键：强制渲染一次，触发所有原生资源加载
                var presenter = new ContentPresenter { Content = mainConfig };
                presenter.Measure(new Size(100, 100));
                presenter.Arrange(new Rect(0, 0, 100, 100));
                presenter.UpdateLayout();
            }
        }

        private void ChangeBackground(object sender, RoutedEventArgs e)
        {
            if (this.ImageRender is VmRenderControl render)
            {
                render.SetBackground(@"D:\iCloud\iCloudDrive\WorkSpace\Resource\Images\Resources\whiteCheckBoard7.png");
            }
        }

        public bool SetModuSource(FrameworkElement control, object moduSource)
        {
            if (control == null || moduSource == null) throw new ArgumentNullException(nameof(control));

            if (control is VmRenderControl renderControl && moduSource is IVmModule module)
            {
                renderControl.ModuleSource = module;
                return true;
            }

            return false;
        }
    }
}
