using System.Windows;

namespace VisionServices.Core
{
    public interface IControl
    {
        public FrameworkElement ImageRender { get; }

        public FrameworkElement MainRender { get; }

        public bool SetModuSource(FrameworkElement control, object moduSource);
    }
}
