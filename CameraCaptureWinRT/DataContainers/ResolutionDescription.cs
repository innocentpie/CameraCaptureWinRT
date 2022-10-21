using System;
using System.Runtime.Serialization;
using Windows.Media.Capture;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Describes a resolution with framerate and format.
    /// </summary>
    [DataContract]
    public class ResolutionDescription
    {
        public ResolutionDescription(MediaCaptureVideoProfileMediaDescription description)
        {
            Width = description.Width;
            Height = description.Height;
            FrameRate = description.FrameRate;
            Subtype = description.Subtype;
        }

        public ResolutionDescription(uint width, uint height, double frameRate, string format = null)
        {
            this.Width = width;
            this.Height = height;
            this.FrameRate = frameRate;
            this.Subtype = format;
        }

        /// <summary>
        /// Return aspect ration (or double.NaN if height is 0).
        /// </summary>
        public double AspectRatio
        {
            get { return Math.Round((Height != 0) ? (Width / (double)Height) : double.NaN, 2); }
        }

        /// <summary>
        /// Resolution width in pixels.
        /// </summary>
        [DataMember]
        public uint Width { get; set; }
        /// <summary>
        /// Resolution height in pixels.
        /// </summary>
        [DataMember]
        public uint Height { get; set; }
        /// <summary>
        /// Resolution's frame rate (FPS).
        /// </summary>
        [DataMember]
        public double FrameRate { get; set; }
        /// <summary>
        /// The native recieved format (such as NV12 or BGRA8).
        /// <br/>(Referred to as Subtype in the media capture library).
        /// </summary>
        [DataMember]
        public string Subtype { get; set; }


        /// <summary>
        /// Returns resolution as a human readable string.
        /// </summary>
        /// <returns>Example: "1280x720@29.97FPS [Nv12]"</returns>
        public override string ToString()
        {
            return $"{Width}x{Height}@{FrameRate.ToString("0.##")}FPS [{Subtype}]";
        }

        /// <summary>
        /// Compares to other <see cref="ResolutionDescription"/> by value.
        /// </summary>
        /// <returns>True, if all their fields are equal.</returns>
        public bool ValueEquals(ResolutionDescription toCompareWith)
        {
            if (toCompareWith == null)
                return false;

            return this.Subtype == toCompareWith.Subtype
                && this.FrameRate == toCompareWith.FrameRate
                && this.Width == toCompareWith.Width
                && this.Height == toCompareWith.Height;
        }
    }
}
