using MvCameraControl;
using System;

namespace IndustrialCameraManager.Abstractions
{
    /// <summary>
    /// Camera接口
    /// </summary>
    public interface ICamera : IDisposable
    {
        /// <summary>
        /// 是否已打开
        /// </summary>
        /// <value>
        /// <c>true</c> if已打开;否则，<c>false</c>。
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// 是否正在取流
        /// </summary>
        /// <value>
        /// <c>true</c> if正在取流;否则，<c>false</c>。
        /// </value>
        bool IsGrabbing { get; }

        /// <summary>
        /// 打开Camera
        /// </summary>
        /// <returns>
        /// <see cref="CameraResult" />
        /// </returns>
        CameraResult Open();

        /// <summary>
        /// 关闭Camera
        /// </summary>
        /// <returns>
        /// <see cref="CameraResult" />
        /// </returns>
        CameraResult Close();

        /// <summary>
        /// 开始取流
        /// </summary>
        /// <returns>
        /// <see cref="CameraResult" />
        /// </returns>
        CameraResult Grab();

        /// <summary>
        /// 停止取流
        /// </summary>
        void StopGrab();

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>
        /// <see cref="CameraResult" />
        /// </returns>
        CameraResult SetParam<T>(string paramName, T value);

        public CameraResult SetParam(string paramName, int value);

        public CameraResult SetParam(string paramName, float value);

        public CameraResult SetParam(string paramName, bool value);

        public CameraResult SetParam(string paramName, string value);

        public CameraResult SetEnumParam(string paramName, string value);


        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <returns>
        /// <see cref="T" />
        /// </returns>
        T GetParam<T>(string paramName);

        string GetEnumValue(string paramName);
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <returns>
        /// <see cref="CameraResult" />
        /// </returns>
        CameraResult ExecuteCommand(string command);


    }
}
