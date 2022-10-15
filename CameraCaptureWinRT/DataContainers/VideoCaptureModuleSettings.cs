using CameraCaptureWinRT.Modules;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        /// Create settings
        /// <br/> You must specify a valid source. If the source is null, or not available, initializing the video capture module fails
        /// </summary>
        /// <param name="targetSource"></param>
        public VideoCaptureModuleSettings(SourceDescription targetSource)
        {
            TargetSource = targetSource;
        }

        /// <summary>
        /// The source to capture on.
        /// <br/> If source is not found, initialization fails.
        /// </summary>
        public SourceDescription TargetSource { get; set; }

        /// <summary>
        /// The kind of data captured.
        /// <br/> Note that only appropriate source kind should be selected for the use.
        /// <br/> (eg. for the <see cref="VideoCaptureModule"/> selecting audio will fail).
        /// </summary>
        public MediaFrameSourceKind SourceKind { get; set; } = MediaFrameSourceKind.Color;

        /// <summary>
        /// Target resolution of the capture.
        /// <br/> If the resolution is not available, capture falls back to the source's default configuration.
        /// </summary>
        public ResolutionDescription TargetResolution { get; set; } = null;

        /// <summary>
        /// Specifies the output <see cref="FrameData"/>'s byte array format.
        /// <br/> (If the natively produced frames are not in the target format, the output will be converted to the target).
        /// </summary>
        public BitmapPixelFormat TargetFormat { get; set; } = BitmapPixelFormat.Bgra8;

        /// <summary>
        /// Specifies the way that the system should manage frames acquired from the when a new frame arrives before the app has finished processing the previous frame.
        /// </summary>
        public MediaFrameReaderAcquisitionMode AcquisitionMode { get; set; }  = MediaFrameReaderAcquisitionMode.Buffered;

        /// <summary>
        /// Specifies the sharing mode the media capture will be initialized with.
        /// <br/> Note that using <see cref="MediaCaptureSharingMode.SharedReadOnly"/> will not allow setting the target resolution, and will use the currently used or default resolution.
        /// </summary>
        public MediaCaptureSharingMode SharingMode { get; set; } = MediaCaptureSharingMode.ExclusiveControl;
    }
}
