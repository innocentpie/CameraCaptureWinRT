using System;
using Windows.Media.Capture;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Describes a resolution with framerate.
    /// </summary>
    public class ResolutionDescription
    {
        public ResolutionDescription(MediaCaptureVideoProfileMediaDescription description)
        {
            width = description.Width;
            height = description.Height;
            frameRate = description.FrameRate;
            format = description.Subtype;
        }

        public ResolutionDescription(uint width, uint height, double frameRate, string format)
        {
            this.width = width;
            this.height = height;
            this.frameRate = frameRate;
            this.format = format;
        }

        /// <summary>
        /// Return aspect ration (or double.NaN if height is 0).
        /// </summary>
        public double AspectRatio
        {
            get { return Math.Round((height != 0) ? (width / (double)height) : double.NaN, 2); }
        }

        /// <summary>
        /// Resolution width in pixels.
        /// </summary>
        public uint width;
        /// <summary>
        /// Resolution height in pixels.
        /// </summary>
        public uint height;
        /// <summary>
        /// Resolution's frame rate (FPS).
        /// </summary>
        public double frameRate;
        /// <summary>
        /// The native recieved format (such as NV12 or BGRA8).
        /// <br/>(Referred to as Subtype in the media capture library).
        /// </summary>
        public string format;

        /// <summary>
        /// Returns resolution as a human readable string.
        /// </summary>
        /// <returns>Example: "1280x720@29.97FPS [Nv12]"</returns>
        public override string ToString()
        {
            return $"{width}x{height}@{frameRate.ToString("0.##")}FPS [{format}]";
        }

        /// <summary>
        /// Compares to other ResolutionDescription by value.
        /// </summary>
        /// <returns>True, if all their fields are equal.</returns>
        public bool ValueEquals(ResolutionDescription toCompareWith)
        {
            if (toCompareWith == null)
                return false;

            return this.format == toCompareWith.format
                && this.frameRate == toCompareWith.frameRate
                && this.width == toCompareWith.width
                && this.height == toCompareWith.height;
        }
    }
}
