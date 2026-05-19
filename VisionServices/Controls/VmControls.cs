using VMControls.WPF.Release;

namespace VisionServices.Controls
{
    public class VmControls
    {
        public VmRenderControl VmRender { get; set; } = new();

        public VmMainViewConfigControl VmMainView { get; set; } = new();

    }
}
