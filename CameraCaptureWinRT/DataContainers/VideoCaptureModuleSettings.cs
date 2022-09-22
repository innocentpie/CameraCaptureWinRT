using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Provides settings for the capture modules.
    /// </summary>
    public class VideoCaptureModuleSettings
    {
        /// <summary>
        /// Specifying target source is required.
        /// </summary>
        public VideoCaptureModuleSettings(MediaFrameSourceInfo targetSource)
        {
            this.targetSource = targetSource;
        }

        /// <summary>
        /// The source to capture on.
        /// <br/> If source is not found, initialization fails.
        /// </summary>
        public MediaFrameSourceInfo targetSource;

        /// <summary>
        /// The kind of data captured.
        /// <br/> Note that only appropriate source kind should be selected for the use.
        /// <br/> (eg. for the VideoCaptureModule selecting audio will fail).
        /// </summary>
        public MediaFrameSourceKind sourceKind = MediaFrameSourceKind.Color;

        /// <summary>
        /// Target resolution of the capture.
        /// <br/> If resolution is not available, capture falls back to the source's default configuration.
        /// </summary>
        public ResolutionDescription targetResolution = null;

        /// <summary>
        /// Specifies the output FrameData's byte array format.
        /// <br/> (If the natively produced frames are not in the target format, the output will be converted to the target).
        /// </summary>
        public BitmapPixelFormat targetFormat = BitmapPixelFormat.Bgra8;

        /// <summary>
        /// Specifies the way that the system should manage frames acquired from the when a new frame arrives before the app has finished processing the previous frame.
        /// </summary>
        public MediaFrameReaderAcquisitionMode acquisitionMode = MediaFrameReaderAcquisitionMode.Buffered;

        /// <summary>
        /// Specifies the sharing mod in which the media capture will be initialized.
        /// <br/> Note that using MediaCaptureSharingMode.SharedReadOnly will not allow setting the target resolution, and will use the currently used or default resolution.
        /// </summary>
        public MediaCaptureSharingMode sharingMode = MediaCaptureSharingMode.ExclusiveControl;
    }
}
