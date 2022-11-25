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
        public object  projectionTransformBytes;
        public object viewTransformBytes;
        public object cameraSpatialCoordSystemObj;

        /// <summary>
        /// Since the image sometimes comes as already undistored, this can be set to specify if the image should be distorted before calculating the ray for a pixel
        /// </summary>
        internal bool _shouldUndistort;

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


        // Implementation taken from:
        // https://github.com/VulcanTechnologies/HoloLensCameraStream/blob/master/HoloLensCameraStream/Plugin%20Project/VideoCaptureSample.cs

        /// <summary>
        /// This returns the transform matrix at the time the photo was captured, if location data if available.
        /// If it's not, that is probably an indication that the HoloLens is not tracking and its location is not known.
        /// It could also mean the VideoCapture stream is not running.
        /// If location data is unavailable then the camera to world matrix will be set to the identity matrix.
        /// </summary>
        /// <param name="outMatrix">The transform matrix used to convert between coordinate spaces.
        /// The matrix will have to be converted to a Unity matrix before it can be used by methods in the UnityEngine namespace.
        /// See https://forum.unity3d.com/threads/locatable-camera-in-unity.398803/ for details.</param>
        public bool TryGetCameraToWorldMatrix(SpatialCoordinateSystem worldOrigin, out Matrix4x4 outMatrix)
        {
            if (viewTransformBytes == null || cameraSpatialCoordSystemObj == null)
            {
                outMatrix = Matrix4x4.Identity;
                return false;
            }

            SpatialCoordinateSystem cameraCoordSystem = cameraSpatialCoordSystemObj as SpatialCoordinateSystem;
            if(cameraCoordSystem == null)
            {
                outMatrix = Matrix4x4.Identity;
                return false;
            }

            Matrix4x4? tryCoordSystemTransf = cameraCoordSystem.TryGetTransformTo(worldOrigin);
            if (tryCoordSystemTransf == null)
            {
                outMatrix = Matrix4x4.Identity;
                return false;
            }

            Matrix4x4 toViewTransf = ConvertByteArrayToMatrix4x4(viewTransformBytes as byte[]);

            // Transpose the matrices to obtain a proper transform matrix
            toViewTransf = Matrix4x4.Transpose(toViewTransf);
            Matrix4x4 coordSystemTransf = Matrix4x4.Transpose(tryCoordSystemTransf.Value);

            Matrix4x4.Invert(toViewTransf, out Matrix4x4 fromViewTransf);
            Matrix4x4 viewToWorldInUnityCoordsMatrix = Matrix4x4.Multiply(coordSystemTransf, fromViewTransf);
;

            // Change from right handed coordinate system to left handed UnityEngine
            viewToWorldInUnityCoordsMatrix.M31 *= -1f;
            viewToWorldInUnityCoordsMatrix.M32 *= -1f;
            viewToWorldInUnityCoordsMatrix.M33 *= -1f;
            viewToWorldInUnityCoordsMatrix.M34 *= -1f;

            outMatrix = viewToWorldInUnityCoordsMatrix;

            return true;
        }

        /// <summary>
        /// This returns the projection matrix at the time the photo was captured, if location data if available.
        /// If it's not, that is probably an indication that the HoloLens is not tracking and its location is not known.
        /// It could also mean the VideoCapture stream is not running.
        /// If location data is unavailable then the projecgtion matrix will be set to the identity matrix.
        /// </summary>
        /// <param name="outMatrix">The projection matrix used to match the true camera projection.
        /// The matrix will have to be converted to a Unity matrix before it can be used by methods in the UnityEngine namespace.
        /// See https://forum.unity3d.com/threads/locatable-camera-in-unity.398803/ for details.</param>
        public bool TryGetProjectionMatrix(out Matrix4x4 outMatrix)
        {
            if (projectionTransformBytes == null)
            {
                outMatrix = Matrix4x4.Identity;
                return false;
            }

            Matrix4x4 projectionMatrix = ConvertByteArrayToMatrix4x4(projectionTransformBytes as byte[]);

            // Transpose matrix to match expected Unity format
            projectionMatrix = Matrix4x4.Transpose(projectionMatrix);
            outMatrix = projectionMatrix;
            return true;
        }

        private Matrix4x4 ConvertByteArrayToMatrix4x4(byte[] matrixAsBytes)
        {
            if (matrixAsBytes == null)
            {
                throw new ArgumentNullException("matrixAsBytes");
            }

            if (matrixAsBytes.Length != 64)
            {
                throw new Exception("Cannot convert byte[] to Matrix4x4. Size of array should be 64, but it is " + matrixAsBytes.Length);
            }

            var m = matrixAsBytes;
            return new Matrix4x4(
                BitConverter.ToSingle(m, 0),
                BitConverter.ToSingle(m, 4),
                BitConverter.ToSingle(m, 8),
                BitConverter.ToSingle(m, 12),
                BitConverter.ToSingle(m, 16),
                BitConverter.ToSingle(m, 20),
                BitConverter.ToSingle(m, 24),
                BitConverter.ToSingle(m, 28),
                BitConverter.ToSingle(m, 32),
                BitConverter.ToSingle(m, 36),
                BitConverter.ToSingle(m, 40),
                BitConverter.ToSingle(m, 44),
                BitConverter.ToSingle(m, 48),
                BitConverter.ToSingle(m, 52),
                BitConverter.ToSingle(m, 56),
                BitConverter.ToSingle(m, 60));
        }
    }
}
