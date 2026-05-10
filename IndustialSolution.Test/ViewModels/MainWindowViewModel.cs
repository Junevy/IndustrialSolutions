using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IndustrialCameraManager.Common;
using IndustrialCameraManager.Core;
using System.Collections.ObjectModel;
using System.Windows;
using VMControls.WPF.Release;

namespace IndustialSolution.Test.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly CameraService cameraService;

        [ObservableProperty]
        private ICameraInfo? selectedCameraInfo;

        public ObservableCollection<ICameraInfo> CamInfoList => cameraService.CamInfoList;
        public VmMainViewConfigControl VmMain { get; private set; } = new();
        public VmRenderControl VmRender { get; private set; } = new();


        public MainWindowViewModel(CameraService cameraService)
        {
            this.cameraService = cameraService;
            VmRender.ChangeMenuVisibility(true);
            VmRender.ChangeTopBarVisibility(false);
        }

        [RelayCommand]
        public void EnumerateCameras()
        {
            cameraService.EnumerateCameras();
        }

        [RelayCommand]
        public void OpenCamera()
        {
            var result = cameraService.OpenCamera(SelectedCameraInfo);
            if (!result.IsSuccess)
                MessageBox.Show(result.Message);
        }

        [RelayCommand]
        public void StartGrab()
        {
            var result = cameraService.StartGrab(SelectedCameraInfo?.SerialNumber, ProcessFrame);
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
        public void SoftTrigger()
        {
            var result = cameraService.SoftTrigger(SelectedCameraInfo?.SerialNumber);
            if (!result.IsSuccess)
                MessageBox.Show(result.Message);
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

                frame.GetBitmap().Save(@"D:\Desktop\WorkSpace\VMDevelop\test.bmp");

            }
            catch (Exception)
            {

            }
        }

        [RelayCommand]
        public void LoadSolution()
        {
            VM.Core.VmSolution.Load(@"D:\Desktop\WorkSpace\VMDevelop\OCR_F166.sol");
        }
    }
}
