using CameraCaptureWinRT.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices.Core;
using Windows.Perception.Spatial;
using Windows.UI.Xaml.Media.Animation;

namespace CameraCaptureWinRT
{
    /// <summary>
    /// Stores the data of the captured frame.
    /// </summary>
    public class FrameData
    {
        /// <summary>
        /// Represents the intrinsics that describe the camera distortion model.
        /// </summary>
        public CameraIntrinsics cameraIntrinsics;

        /// <summary>
        /// The camera's spatial coordinate system.
        /// </summary>
        public SpatialCoordinateSystem cameraSpatialCoordinateSystem;

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


        /// <summary>
        /// Gets the ray for the given pixel coord in the camera's coordinate system
        /// <br/> Use <see cref="cameraSpatialCoordinateSystem"/> to transform between coordinate systems
        /// </summary>
        /// <param name="pixelCoord">
        /// The coordinate for the pixel
        /// <br/> The image coordinate must be expressed in pixels, with the origin in the top-left corner of the image; that is, +X pointing right, and +Y pointing down.
        /// </param>
        /// <returns>The returned ray in the frame sample's own coordinate system</returns>
        public SpatialRay GetRayAtPixel(Point pixelCoord)
        {
            // Pixel coordinate:
            // (0,0) ------> (x)
            //      |
            //      |
            //      V
            //     (y)

            // Unprojected coordinate:
            //     (y)
            //      ^
            //      |
            //      |   x (PrincipalPoint)
            //      |
            // (0,0) ------> (x)


            // Undistort pixel
            Point undistortedPixelCoord = cameraIntrinsics.UndistortPoint(pixelCoord);

            // Unproject to the plane unit distance away from the camera
            // Plane center is at PrincipalPoint
            // This position is in camera space
            // See: https://learn.microsoft.com/en-us/uwp/api/windows.media.devices.core.cameraintrinsics.unprojectatunitdepth?view=winrt-22621
            Vector2 unprojectedPixelCoord = cameraIntrinsics.UnprojectAtUnitDepth(undistortedPixelCoord);

            // The ray's direction is the firection of the unprojected pixel coord
            // the ray's start is the camera position (0, 0, 0) in the camera space
            Vector3 rayOrigin = Vector3.Zero;
            Vector3 rayDirection = new Vector3(unprojectedPixelCoord.X, unprojectedPixelCoord.Y, 1f);
            rayDirection /= rayDirection.Length();

            return new SpatialRay()
            {
                Origin = rayOrigin,
                Direction = rayDirection
            };
        }

    }
}
