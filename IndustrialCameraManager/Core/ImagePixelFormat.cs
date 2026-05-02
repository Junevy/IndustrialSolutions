namespace IndustrialCameraManager.Core
{
    public enum ImagePixelFormat
    {
        Unknown = 0,

        // 灰度
        Mono8,
        Mono10,
        Mono12,
        Mono14,
        Mono16,

        // Bayer
        BayerRG8,
        BayerRG10,
        BayerRG12,
        BayerGB8,
        BayerBG8,

        // RGB
        RGB8,
        BGR8,

        // 高级
        YUV422,
        YUV444,

        // Packed（可选）
        Mono10Packed,
        BayerRG12Packed
    }
}
