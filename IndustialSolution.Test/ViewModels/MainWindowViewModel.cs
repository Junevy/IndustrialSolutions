using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageSourceModuleCs;
using IndustrialCameraManager.Abstractions;
using IndustrialCameraManager.Common;
using System.Collections.ObjectModel;
using System.Windows;
using VM.Core;
using VM.PlatformSDKCS;
using VMControls.WPF.Release;

namespace IndustialSolution.Test.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly CameraService cameraService;

        [ObservableProperty]
        private ICameraInfo? selectedCameraInfo;

        public ObservableCollection<ICameraInfo> CamInfoList { get; set; } = new();
        public VmMainViewConfigControl VmMain { get; private set; } = new();
        public VmRenderControl VmRender { get; private set; } = new();


        public MainWindowViewModel(CameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        [RelayCommand]
        public void EnumerateCameras()
        {
            foreach (var cameraInfo in cameraService.EnumerateCameras())
                CamInfoList.Add(cameraInfo);
        }

        [RelayCommand]
        public void OpenCamera()
        {
            if (SelectedCameraInfo is null)
            {
                MessageBox.Show("Select a camera first");
                return;
            }

            var result = cameraService.OpenCamera(SelectedCameraInfo);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message);
                return;
            }

            var subResult = cameraService.SubscribeFrameStream(SelectedCameraInfo.SerialNumber, "saveImg", ProcessFrame);
            if (!subResult)
                MessageBox.Show("Subscribe the camera frame stream error");
        }

        [RelayCommand]
        public void StartGrab()
        {
            var result = cameraService.StartGrab(SelectedCameraInfo?.SerialNumber);
            if (!result.IsSuccess)
                MessageBox.Show(result.Message);
        }

        [RelayCommand]
        public void StopGrab()
        {
            var result = cameraService.StopGrab(SelectedCameraInfo?.SerialNumber);
            if (!result.IsSuccess)
                MessageBox.Show(result.Message);
        }

        [RelayCommand]
        public void SaveBmp()
        {

        }

        [RelayCommand]
        public void SoftTriggerOnece()
        {
            VmSolution.Load(@"D:\Desktop\WorkSpace\VMDevelop\OCR_F166.sol");

            var result = cameraService.SetTrigger(SelectedCameraInfo?.SerialNumber, "Software", false);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.Message);
                return;
            }

            StartGrab();

            var exeResult = cameraService.ExecuteCommand(selectedCameraInfo?.SerialNumber, "TriggerSoftware");
            if (!exeResult.IsSuccess)
                MessageBox.Show(exeResult.Message);
        }

        [RelayCommand]
        public void Close()
        {
            var result = cameraService.Close(SelectedCameraInfo?.SerialNumber);
            if (!result.IsSuccess)
                MessageBox.Show(result.Message);
        }



        private async Task ProcessFrame(IFrame frame)
        {
            try
            {



                //frame.GetBitmap().Save(@"D:\Desktop\WorkSpace\VMDevelop\test.bmp");

                ImageBaseData image =
                    new ImageBaseData(frame.Data, (uint)frame.Data.Length, (int)frame.Width, (int)frame.Height, 17825795);

                var imageSource = VmSolution.Instance["流程1.图像源1"] as ImageSourceModuleTool;

                imageSource.ModuParams.ImageSourceType = ImageSourceParam.ImageSourceTypeEnum.SDK;


                imageSource.SetImageData(image);

                //imageSource.SetImagePath(@"D:\Desktop\WorkSpace\VMDevelop\test.bmp");


                //imageSource?.Run();



                //imageSource?.ModuParams.SetInputImage(inputImageData);


                //imageSource.SetImageData(data);

                VmSolution.Instance.Run();

                //VmSolution.Instance.SyncRun();

                App.Current.Dispatcher.Invoke(() =>
                {
                    VmRender.ModuleSource = imageSource;

                });




            }
            catch (Exception)
            {

            }
        }

        [RelayCommand]
        public void LoadSolution()
        {
            //VM.Core.VmSolution.Load(@"D:\Desktop\WorkSpace\VMDevelop\OCR_F166.sol");

            //var imageSource = VmSolution.Instance["流程1.图像源1"] as ImageSourceModuleTool;
            //imageSource.SetImageData
            ;
        }
    }
}
