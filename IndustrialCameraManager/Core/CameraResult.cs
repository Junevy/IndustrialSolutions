using MvCameraControl;

namespace IndustrialCameraManager.Core
{
    public class CameraResult
    {
        public bool IsSuccess { get; }

        public int Code { get; }

        public string Message { get; }

        public CameraResult(bool isSuccess, int code)
        {
            this.IsSuccess = isSuccess;
            this.Code = code;
        }

        public CameraResult(bool isSuccess, int code, string msg)
        {
            this.IsSuccess = isSuccess;
            this.Code = code;
            this.Message = msg;
        }

        public static CameraResult Result(bool result, int errorCode)
        {
            return new CameraResult(result, errorCode);
        }

        public static CameraResult Fail(int errorCode, string errMsg = "")
        {
            return new CameraResult(false, errorCode, errMsg);
        }

        public static CameraResult Success(int errorCode, string errMsg = "")
        {
            return new CameraResult(true, errorCode, errMsg);
        }
    }
}
