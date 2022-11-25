using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Graphics.Imaging;
using System.Threading;
using Windows.Perception.Spatial;
using System.Runtime.InteropServices.WindowsRuntime;
using CameraCaptureWinRT.Helpers;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Provides simplified control over video capturing and recieving frames.
    /// </summary>
    public class VideoCaptureModule
    {
        private const MediaStreamType STREAM_TYPE = MediaStreamType.VideoRecord;

        /// <summary>
        /// Event called, when frame is recieved.
        /// <br/> Because the frame reader raises the event on its own thread, you may need to implement some synchronization logic to make sure that you aren't attempting to access the same data from multiple threads.
        /// </summary>
        public event Action<FrameData> OnFrameArrived;

        /// <summary>
        /// Returns used source group (null if settings are uninitialized).
        /// </summary>
        public MediaFrameSourceGroup SourceGroup { get; private set; }
        /// <summary>
        /// Returns used source info (null if settings are uninitialized).
        /// </summary>
        public MediaFrameSourceInfo SourceInfo { get; private set; }

        /// <summary>
        /// Stream type used for capturing.
        /// <br/> Always returns <see cref="MediaStreamType.VideoRecord"/> in current implementation.
        /// </summary>
        public MediaStreamType StreamType => STREAM_TYPE;

        /// <summary>
        /// The kind of data captured.
        /// <br/> Throws <see cref="InvalidOperationException"/> if called while settings are uninitialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public MediaFrameSourceKind SourceKind
        {
            get
            {
                if (_mediaCapture == null)
                    throw new InvalidOperationException("Trying to access property of uninitialized object. Call InitializeAsync first.");
                return _moduleSettings.SourceKind;
            }
        }

        /// <summary>
        /// Specifies the output <see cref="FrameData"/>'s byte array format 
        /// <br/> Throws <see cref="InvalidOperationException"/> if called while settings are uninitialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public BitmapPixelFormat TargetFormat
        {
            get
            {
                if (_mediaCapture == null)
                    throw new InvalidOperationException("Trying to access property of uninitialized object. Call InitializeAsync first.");
                return _moduleSettings.TargetFormat;
            }
        }

        /// <summary>
        /// Specifies the way that the system should manage frames acquired from the when a new frame arrives before the app has finished processing the previous frame
        /// <br/> Throws <see cref="InvalidOperationException"/> if called while settings are uninitialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public MediaFrameReaderAcquisitionMode AcquisitionMode
        {
            get
            {
                if (_mediaCapture == null)
                    throw new InvalidOperationException("Trying to access property of uninitialized object. Call InitializeAsync first.");
                return _moduleSettings.AcquisitionMode;
            }
        }


        private MediaCapture _mediaCapture;
        private MediaFrameReader _frameReader;

        private VideoCaptureModuleSettings _moduleSettings;


        /// <summary>
        /// Gets all sources available for the specified source kind.
        /// </summary>
        /// <param name="sourceKind"></param>
        /// <returns></returns>
        public static async Task<IList<SourceDescription>> GetAvailableSources(MediaFrameSourceKind sourceKind)
        {
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            List<SourceDescription> result = new List<SourceDescription>();
            for (int i = 0; i < allGroups.Count; i++)
            {
                var currentGroup = allGroups[i];
                var sourcesForGroup = currentGroup.SourceInfos;

                for (int j = 0; j < sourcesForGroup.Count; j++)
                {
                    var currentSource = sourcesForGroup[j];
                    if (currentSource.MediaStreamType == STREAM_TYPE && currentSource.SourceKind == sourceKind)
                        result.Add(new SourceDescription(currentGroup, currentSource));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the default source that is suitable for capturing with the specified source kind
        /// </summary>
        /// <param name="sourceKind"></param>
        /// <returns>The source, or null if none found</returns>
        public static async Task<SourceDescription> GetDefaultSource(MediaFrameSourceKind sourceKind)
        {
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            var selectedGroup = allGroups.Select(x =>
            new
            {
                group = x,
                sources = x.SourceInfos.Where(y => y.MediaStreamType == STREAM_TYPE && y.SourceKind == sourceKind)
            }).FirstOrDefault(x => x.sources.Count() > 0);

            if (selectedGroup == null)
                return null;

            return new SourceDescription(selectedGroup.group, selectedGroup.sources.First());
        }

        /// <summary>
        /// Creates empty VideoCaptureModule.
        /// <br/> Call InitializeSettingsAsync to initialize for capturing.
        /// </summary>
        public VideoCaptureModule() { }

        /// <summary>
        /// Initializes capture with specified settings.
        /// <br/> If already initialized, stops any ongoing capturing and reinitializes (doesn't restart capturing).
        /// </summary>
        /// <param name="settings"></param>
        public async Task InitializeSettingsAsync(VideoCaptureModuleSettings settings)
        {
            await UninitializeSettingsAsync();

            var source = settings.TargetSource ?? await GetDefaultSource(settings.SourceKind);
            if (source == null)
                throw new NullReferenceException("Source given for VideoCaptureModule settings is null and no available source found to fall back to default");

            SourceGroup = source.SourceGroup;
            SourceInfo = source.SourceInfo;

            this._moduleSettings = settings;

            MediaCaptureInitializationSettings mediaCaptureInitSettings = new MediaCaptureInitializationSettings()
            {
                VideoDeviceId = SourceGroup.Id,
                SourceGroup = SourceGroup,
                SharingMode = settings.SharingMode,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
            };

            // Find and set profile with resolution (if found)
            var resolution = settings.TargetResolution;
            if (_moduleSettings.SharingMode != MediaCaptureSharingMode.SharedReadOnly && resolution != null && MediaCapture.IsVideoProfileSupported(SourceGroup.Id))
            {
                var profiles = MediaCapture.FindAllVideoProfiles(SourceGroup.Id);
                var matches = (from prof in profiles
                               from desc in prof.SupportedRecordMediaDescription
                               where desc.Width == resolution.Width
                                   && desc.Height == resolution.Height
                                   && Math.Abs(desc.FrameRate - resolution.FrameRate) < 0.001d
                                   && desc.Subtype == resolution.Subtype
                               select new { prof, desc });
                var match = matches.FirstOrDefault();
                if (match?.prof != null)
                    mediaCaptureInitSettings.VideoProfile = match.prof;
                if (match?.desc != null)
                    mediaCaptureInitSettings.RecordMediaDescription = match.desc;
            }


            _mediaCapture = new MediaCapture();
            try
            {
                // Initialize MediaCapture with selected group.
                // This can raise an exception if the source no longer exists,
                // or if the source could not be initialized.
                await _mediaCapture.InitializeAsync(mediaCaptureInitSettings);
            }
            catch (Exception e)
            {
                await UninitializeSettingsAsync();
                throw e;
            }

            if (_moduleSettings.SharingMode != MediaCaptureSharingMode.SharedReadOnly && resolution != null)
            {
                // Initializing succeded
                // Try to set properties this way as well
                // Profiles don't work on some devices
                IEnumerable<VideoEncodingProperties> allVideoProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(SourceInfo.MediaStreamType).Select(x => x as VideoEncodingProperties);
                VideoEncodingProperties selectedStreamProperties = allVideoProperties.FirstOrDefault(
                    x => x.Width == resolution.Width
                            && x.Height == resolution.Height
                            && Math.Abs(
                                (x.FrameRate.Denominator != 0 ? ((double)x.FrameRate.Numerator / x.FrameRate.Denominator) : 0)
                                    - resolution.FrameRate) < 0.001d
                            && x.Subtype == resolution.Subtype);

                if (selectedStreamProperties != null)
                {
                    try
                    {
                        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(SourceInfo.MediaStreamType, selectedStreamProperties);
                    }
                    catch (Exception e)
                    {
                        // Exclusive control is not available
                        // Continue with shared read only ?
                        await UninitializeSettingsAsync();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// Allocates resources neccessary for capturing and starts it.
        /// <br/> If capturing already, it restarts with a new buffer (unprocessed elements are dropped).
        /// </summary>
        /// <returns>
        /// Success if starting capturing was successful.
        /// </returns>
        public async Task<MediaFrameReaderStartStatus> StartCapturingAsync()
        {
            await StopCapturingAsync();

            var frameSource = _mediaCapture.FrameSources[SourceInfo.Id];
            _frameReader = await _mediaCapture.CreateFrameReaderAsync(frameSource);

            _frameReader.AcquisitionMode = AcquisitionMode;
            _frameReader.FrameArrived += FrameReader_FrameArrived;

            var status = await _frameReader.StartAsync();

            if (status != MediaFrameReaderStartStatus.Success)
            {
                await StopCapturingAsync();
                return status;
            }
            return status;
        }

        /// <summary>
        /// Stops (if ongoing) capturing and cleans up resources associated with active capturing.
        /// </summary>
        /// <returns></returns>
        public async Task StopCapturingAsync()
        {
            if (_frameReader != null)
            {
                _frameReader.FrameArrived -= FrameReader_FrameArrived;
                await _frameReader.StopAsync();
                _frameReader.Dispose();
                _frameReader = null;
            }
        }

        /// <summary>
        /// Stops (if ongoing) capturing, and clears settings.
        /// <br/> (Events are preserved).
        /// </summary>
        public async Task UninitializeSettingsAsync()
        {
            await StopCapturingAsync();

            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }

            SourceGroup = null;
            SourceInfo = null;
            _moduleSettings = null;
        }

        /// <summary>
        /// Method added to <see cref="MediaFrameReader.FrameArrived"/> event.
        /// </summary>
        private void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            FrameData frameData = null;
            using (MediaFrameReference mediaReference = sender.TryAcquireLatestFrame())
            {
                // Did not fail
                if (mediaReference != null)
                {
                    frameData = MakeFrameData(mediaReference);
                    OnFrameArrived?.Invoke(frameData);
                }
            }
        }


        /// <summary>
        /// Converts <see cref="VideoMediaFrame"/> to <see cref="FrameData"/>
        /// </summary>
        private unsafe FrameData MakeFrameData(MediaFrameReference frameReference)
        {
            byte[] result;
            VideoMediaFrame videoFrameReference = frameReference.VideoMediaFrame;
            SoftwareBitmap bitmap = videoFrameReference.SoftwareBitmap;

            SoftwareBitmap converted = null;
            if (bitmap.BitmapPixelFormat != TargetFormat)
            {
                // Convert
                converted = SoftwareBitmap.Convert(bitmap, TargetFormat);
                // Dispose original
                bitmap.Dispose();
                // Set bitmap to converted
                bitmap = converted;
            }

            FrameData frameData = null;
            using (BitmapBuffer buffer = bitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* dataInBytes, out uint capacity);

                result = new byte[capacity];
                BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                for (int i = 0; i < bufferLayout.Height; i++)
                {
                    for (int j = 0; j < bufferLayout.Width; j++)
                    {
                        result[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0];

                        result[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1];

                        result[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2];

                        result[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3] = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3];
                    }
                }
                
                var properties = frameReference.Properties;
                frameData = new FrameData()
                {
                    cameraSpatialCoordSystemObj = properties[MFGuidHelper.MFSampleExtension_Spatial_CameraCoordinateSystem],
                    projectionTransformBytes = properties[MFGuidHelper.MFSampleExtension_Spatial_CameraProjectionTransform],
                    viewTransformBytes = properties[MFGuidHelper.MFSampleExtension_Spatial_CameraViewTransform],

                    bytes = result,
                    width = bufferLayout.Width,
                    height = bufferLayout.Height,
                    pixelFormat = bitmap.BitmapPixelFormat
                };
            }

            bitmap.Dispose();
            return frameData;
        }
    }
}
