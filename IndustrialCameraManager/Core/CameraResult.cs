using MvCameraControl;

namespace IndustrialCameraManager.Core
{
    public class CameraResult
    {
        public bool IsSuccess { get; }

        public int Code { get; }

        public CameraResult(bool isSuccess, int code)
        {
            this.IsSuccess = isSuccess;
            this.Code = code;
        }

        public static CameraResult Result(bool result, int errorCode)
        {
            return new CameraResult(result, errorCode);
        }

        public static CameraResult Fail(int errorCode)
        {
            return new CameraResult(false, errorCode);
        }

        public static CameraResult Success(int errorCode)
        {
            return new CameraResult(true, errorCode);
        }
    }
}
