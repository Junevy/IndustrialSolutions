using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using IndustrialCameraManager.HikVision;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VMControls.WPF.Release;

namespace IndustialSolution.Test.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly CameraManager cameraManager;
        private readonly ICameraProvider provider;
        private ICamera camera;

        //private string path = @"D:\\Desktop";

        [ObservableProperty]
        private ICameraInfo? selectedCameraInfo;

        [ObservableProperty]
        private BitmapSource currentFrame;

        public ObservableCollection<ICameraInfo> CamInfoList { get; private set; } = [];
        public VmMainViewConfigControl VmMain { get; private set; } = new();

        public VmRenderControl VmRender { get; private set; } = new();



        public MainWindowViewModel(CameraManager cameraManager, ICameraProvider provider)
        {
            this.cameraManager = cameraManager;
            this.provider = provider;

            //VmRender.SetBackground("#3b3e41");
            VmRender.ChangeMenuVisibility(true);
            //VmRender.ShrinkView();
            VmRender.ChangeTopBarVisibility(false);
            //this.VmMain.RenderSize = new Size(1,1);
            //this.VmMain.IsOpenParams = false;
        }

        [RelayCommand]
        public void EnumerateCameras()
        {
            var infoList = provider.Enumerate();

            foreach (var info in infoList)
            {
                CamInfoList.Add(info);
            }
        }

        [RelayCommand]
        public void OpenCamera()
        {
            try
            {
                this.camera = cameraManager.GetOrCreate(selectedCameraInfo);
                if (!camera.IsOpen)
                {
                    var result = camera.Open();

                    if (!result.IsSuccess)
                        MessageBox.Show("Open failed.");
                }
            }
            catch (Exception)
            {
                // 处理异常，例如显示错误消息
            }
        }

        [RelayCommand]
        public void StartGrab()
        {
            try
            {
                if (camera == null || !camera.IsOpen)
                    throw new InvalidOperationException("Open camera first!");

                var result = camera.Grab();
                if (!result.IsSuccess)
                {
                    camera.StopGrab();
                    MessageBox.Show("Start grab failed");
                    return;
                }

                if (camera is HikCamera hikCamera)
                {
                    var subs = hikCamera.Stream.Subscribe(ProcessFrame);
                }
            }
            catch (Exception)
            {

            }
        }

        [RelayCommand]
        public void SaveBmp()
        {

        }

        [RelayCommand]
        public void Close()
        {
            camera.Close();
        }

        private async Task ProcessFrame(IFrame frame)
        {
            try
            {
                //frame.GetInner<IFrameOut>();
                var bmp = frame.GetBitmap();
                //VmRender.ImageSource = bmp;

                //var stride = frame.ImageSize / frame.Height;
                //var bmpSource = IntPtrToBitmapSource(frame.PixelDataPtr, (int)frame.Width, (int)frame.Height, (int)stride, PixelFormats.Gray8);

                //App.Current.Dispatcher.Invoke(() =>
                //{
                //this.CurrentFrame = bmpSource;

                //});
            }
            catch (Exception)
            {

            }
        }

        [RelayCommand]
        public void Test()
        {
            //var result = camera.GetParam<float>("ExposureTime");

            VM.Core.VmSolution.Load(@"D:\Desktop\WorkSpace\VMDevelop\OCR_F166.sol");

        }

        //public static BitmapSource IntPtrToBitmapSource(IntPtr dataPtr, int width, int height, int stride, PixelFormat format)
        //{
        //    try
        //    {
        //        // 创建 InteropBitmap（需配合内存映射，这里是简化版）
        //        // 或使用 WriteableBitmap 并调用 WritePixels
        //        //var bitmap = new WriteableBitmap(width, height, 2448, 2448, format, null);
        //        //bitmap.WritePixels(new Int32Rect(0, 0, width, height), dataPtr, (int)(height * stride), stride);

        //        App.Current.Dispatcher.Invoke(() =>
        //        {
        //            var bitmap = new WriteableBitmap(width, height, 96, 96, format, null);
        //            bitmap.WritePixels(new Int32Rect(0, 0, width, height), dataPtr, stride * height, stride);
        //            return bitmap;
        //        });

        //        return null;

        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
    }
}
