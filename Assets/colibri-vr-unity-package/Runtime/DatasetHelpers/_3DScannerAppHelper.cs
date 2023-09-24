/// Copyright 2019-2020 MINES ParisTech (PSL University)
/// This work is licensed under the terms of the MIT license, see the LICENSE file.
/// 
/// Author: Grégoire Dupont de Dinechin, gregoire@dinechin.org

using UnityEngine;
using System.IO;
using System.Linq;

namespace COLIBRIVR.DatasetHelpers
{
    /// <summary>
    /// Class that can be used to parse datasets exported from the 3D Scanner App
    /// </summary>
    [ExecuteInEditMode]
    public class _3DScannerAppHelper : MonoBehaviour
    {
        #region CONST_FIELDS

        public const string propertyNameScaleFactor = "scaleFactor";
        public const string propertyNameRepositionAroundCenter = "repositionAroundCenter";

        #endregion //CONST_FIELDS

        #region PROPERTIES

        public DataHandler dataHandler
        {
            get { return _dataHandler; }
        }

        public CameraSetup cameraSetup
        {
            get { return _cameraSetup; }
        }

        #endregion //PROPERTIES

        #region FIELDS

        public float scaleFactor;
        public bool repositionAroundCenter;

        [SerializeField] private DataHandler _dataHandler;
        [SerializeField] private CameraSetup _cameraSetup;

        #endregion //FIELDS

        #region INHERITANCE_METHODS

        /// <summary>
        /// Resets the object's properties.
        /// </summary>
        void Reset()
        {
            // Reset the key child components.
            _cameraSetup = CameraSetup.CreateOrResetCameraSetup(transform);
            _dataHandler = DataHandler.CreateOrResetDataHandler(transform);
            // Reset other properties.
            scaleFactor = 0.01f;
            repositionAroundCenter = true;
        }

        /// <summary>
        /// On destroy, destroys the created objects.
        /// </summary>
        void OnDestroy()
        {
            if (!GeneralToolkit.IsStartingNewScene())
                GeneralToolkit.RemoveChildComponents(transform, typeof(CameraSetup), typeof(DataHandler));
        }

        #endregion //INHERITANCE_METHODS

        #region METHODS

#if UNITY_EDITOR

        /// <summary>
        /// Parses the camera setup from a directory containing an "images" folder with a dataset exported from the 3D Scanner App, and saves the parsed setup in this directory.
        /// </summary>
        public void ParseCameraSetup()
        {
            // Inform of process start.
            Debug.Log(GeneralToolkit.FormatScriptMessage(this.GetType(),
                "Started parsing camera setup for a dataset exported from the 3D Scanner App located at: " +
                dataHandler.colorDirectory + "."));
            // Get the files in the "images" folder.
            FileInfo[] fileInfos = GeneralToolkit.GetFilesByExtension(dataHandler.colorDirectory, ".jpg", ".png");
            // Get the files in the "optimized_poses" folder.
            FileInfo[] optimizedPosesFileInfos = GeneralToolkit.GetFilesByExtension(Path.Combine(dataHandler.dataDirectory, "optimized_poses"), ".json");
            var usablePoses = optimizedPosesFileInfos.Where(x =>
            {
                var name = x.Name.Split(".json")[0];
                return fileInfos.Any(y => y.Name.Contains(name));
            }).ToArray();
            
            // Determine the pixel resolution of the images.
            Texture2D tempTex = new Texture2D(1, 1);
            GeneralToolkit.LoadTexture(fileInfos[0].FullName, ref tempTex);
            Vector2Int pixelResolution = new Vector2Int(tempTex.width, tempTex.height);
            DestroyImmediate(tempTex);
            // Prepare repositioning around center if it is selected.
            Vector3 meanPos = Vector3.zero;
            // Reset the camera models to fit the color count.
            _cameraSetup.ResetCameraModels();
            _cameraSetup.cameraModels = new CameraModel[usablePoses.Length];
            // Iteratively add each camera model to the setup.
            for (int iter = 0; iter < usablePoses.Length; iter++)
            {
                var json = File.ReadAllText(usablePoses[iter].FullName);
                var data = JsonUtility.FromJson<Data>(json);

                var matrix = new Matrix4x4();
                matrix.SetRow(0, new Vector4(data.cameraPoseARFrame[0], data.cameraPoseARFrame[1], data.cameraPoseARFrame[2], data.cameraPoseARFrame[3]));
                matrix.SetRow(1, new Vector4(data.cameraPoseARFrame[4], data.cameraPoseARFrame[5], data.cameraPoseARFrame[6], data.cameraPoseARFrame[7]));
                matrix.SetRow(2, new Vector4(data.cameraPoseARFrame[8], data.cameraPoseARFrame[9], data.cameraPoseARFrame[10], data.cameraPoseARFrame[11]));
                matrix.SetRow(3, new Vector4(data.cameraPoseARFrame[12], data.cameraPoseARFrame[13], data.cameraPoseARFrame[14], data.cameraPoseARFrame[15]));

                var scaledFocalX = data.intrinsics[0];
                var scaledOffsetX = data.intrinsics[2];
                var scaledFocalY = data.intrinsics[4];
                var scaledOffsetY = data.intrinsics[5];
                var scale = data.intrinsics[8];
                
                var fovX = Camera.FocalLengthToFieldOfView(scaledFocalX / scale, pixelResolution.x);
                var fovY = Camera.FocalLengthToFieldOfView(scaledFocalY / scale, pixelResolution.y);
                
                //var rot = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                var pos = (Vector3)matrix.GetColumn(3);
                
                // see https://github.com/StephenGuanqi/Unity-ARKit-Plugin/blob/7abc42d5553de5d3149f8770494d8275c50cdc16/Assets/Plugins/iOS/UnityARKit/Utility/UnityARMatrixOps.cs
                pos.z = -pos.z;
                var rot = QuaternionFromMatrix(matrix);
                rot.x = -rot.z;
                rot.w = -rot.w;

                CameraModel cameraModel = _cameraSetup.AddCameraModel(iter);
                cameraModel.SetCameraReferenceIndexAndImageName(cameraModel.cameraReferenceIndex, fileInfos[iter].Name);
                // string CAMERA_ID = GeneralToolkit.ToString(cameraModel.cameraReferenceIndex);
                // string MODEL = cameraModel.modelName;
                // string WIDTH = GeneralToolkit.ToString(cameraModel.pixelResolution.x);
                // string HEIGHT = GeneralToolkit.ToString(cameraModel.pixelResolution.y);
                // string PARAMS_FOCALLENGTH = GeneralToolkit.ToString(Camera.FieldOfViewToFocalLength(cameraModel.fieldOfView.x, cameraModel.pixelResolution.x));
                // string PARAMS_CENTERWIDTH = GeneralToolkit.ToString(cameraModel.pixelResolution.x / 2);
                // string PARAMS_CENTERHEIGHT = GeneralToolkit.ToString(cameraModel.pixelResolution.y / 2);
                // string line = CAMERA_ID + " " + MODEL + " " + WIDTH + " " + HEIGHT + " " + PARAMS_FOCALLENGTH + " " + PARAMS_CENTERWIDTH + " " + PARAMS_CENTERHEIGHT;
                
                
                // Store the image's pixel resolution in the camera model.
                cameraModel.pixelResolution = pixelResolution;
                cameraModel.fieldOfView = new Vector2(fovX, fovY);
                
                // Store the image's name in the camera model.
                // FileInfo fileInfo = fileInfos[iter];
                // cameraModel.SetCameraReferenceIndexAndImageName(cameraModel.cameraReferenceIndex, fileInfo.Name);
                // // Store the image's position in the model.
                // // string[] split = fileInfo.Name.Split('_');
                // // float positionY = -GeneralToolkit.ParseFloat(split[split.Length - 3]);
                // // float positionX = GeneralToolkit.ParseFloat(split[split.Length - 2]);
                // // Vector3 pos = scaleFactor * new Vector3(positionX, positionY, 0);
                cameraModel.transform.position = pos;
                cameraModel.transform.rotation = rot;
                //
                //
                //
                meanPos += pos;
            }
            
            // If it is selected, reposition the camera setup around its center position.
            if(repositionAroundCenter)
            {
                meanPos /= dataHandler.sourceColorCount;
                for(int iter = 0; iter < usablePoses.Length; iter++)
                {
                    CameraModel cameraModel = _cameraSetup.cameraModels[iter];
                    cameraModel.transform.position = cameraModel.transform.position - meanPos;
                }
            }
            // Temporarily move the color images to a safe location.
            Debug.Log(dataHandler.dataDirectory);
            string tempDirectoryPath = Path.Combine(GeneralToolkit.GetDirectoryBefore(dataHandler.dataDirectory), "temp");
            GeneralToolkit.Move(PathType.Directory, dataHandler.colorDirectory, tempDirectoryPath);
            // Save the camera setup information (this would also have cleared the "images" folder if it was still there).
            Acquisition.Acquisition.SaveAcquisitionInformation(dataHandler, cameraSetup);
            // Move the color images back into their original location.
            GeneralToolkit.Move(PathType.Directory, tempDirectoryPath, dataHandler.colorDirectory);
            // Update the camera models of the setup object.
            _cameraSetup.FindCameraModels();
            // Inform of end of process.
            Debug.Log(GeneralToolkit.FormatScriptMessage(this.GetType(), "Finished parsing camera setup. Result can be previewed in the Scene view."));
        }

#endif //UNITY_EDITOR

        #endregion //METHODS

        class Data
        {
            public float[] cameraPoseARFrame;
            public float[] intrinsics;
        }
        
        static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
            q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
            q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
            q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
            q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
            q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
            q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
            return q;
        }
    }

}