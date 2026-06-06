using System;
using System.Threading.Tasks;

namespace IndustrialCameraManager.Abstractions
{
    /// <summary>
    /// Camera流接口，在Camera取流时，如何处理流数据
    /// </summary>
    public interface ICameraStream : IDisposable
    {
        /// <summary>
        /// 订阅者数量
        /// </summary>
        /// <value>
        /// 订阅者数量
        /// </value>
        int SubscriberCount { get; }

        /// <summary>
        /// 发布一帧图像
        /// </summary>
        /// <param name="frame">一帧图像</param>
        void Publish(IFrame frame);

        /// <summary>
        /// 订阅指定相机的帧图像数据
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="handler">处理函数</param>
        /// <param name="capacity">图像缓存容量</param>
        void Subscribe(string serialNumber, Func<IFrame, Task> handler, int capacity);

        /// <summary>
        /// 取消订阅指定相机的帧的处理数据
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <returns>
        /// 是否成功取消订阅
        /// </returns>
        bool Unsubscribe(string serialNumber);
    }
}
