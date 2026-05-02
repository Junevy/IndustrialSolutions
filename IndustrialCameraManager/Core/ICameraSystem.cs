namespace IndustrialCameraManager.Core
{
    /// <summary>
    /// SDK 的初始化与生命周期控制接口
    /// </summary>
    public interface ICameraSystem
    {
        void Initialize();

        void Release();
    }
}
