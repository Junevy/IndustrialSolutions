using VMControls.WPF.Release;

namespace VisionServices.Controls
{
    public class VmControls
    {
        public VmRenderControl ImageRender { get; set; } = new();

        public VmMainViewConfigControl VmMainView { get; set; } = new();

    }
}
