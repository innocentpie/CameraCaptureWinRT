using Windows.Graphics.Imaging;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Stores the data of the captured frame.
    /// </summary>
    public class FrameData
    {
        /// <summary>
        /// The captures frame's raw bytes.
        /// </summary>
        public byte[] bytes;

        /// <summary>
        /// The format of the bytes.
        /// </summary>
        public BitmapPixelFormat pixelFormat;

        /// <summary>
        /// Frame pixel width.
        /// </summary>
        public int width;
        /// <summary>
        /// Frame pixel height.
        /// </summary>
        public int height;
    }
}
