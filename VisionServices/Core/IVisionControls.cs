using System.Windows;

namespace VisionServices.Core
{
    public interface IVisionControls
    {
        public FrameworkElement ImageRender { get; }

        public FrameworkElement MainRender { get; }

        public bool SetModuSource(FrameworkElement control, object moduSource);
    }
}
