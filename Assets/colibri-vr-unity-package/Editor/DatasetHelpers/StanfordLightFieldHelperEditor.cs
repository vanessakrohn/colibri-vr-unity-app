﻿/// Copyright 2019-2020 MINES ParisTech (PSL University)
/// This work is licensed under the terms of the MIT license, see the LICENSE file.
/// 
/// Author: Grégoire Dupont de Dinechin, gregoire@dinechin.org

using UnityEngine;
using UnityEditor;

namespace COLIBRIVR.DatasetHelpers
{

#if UNITY_EDITOR

    /// <summary>
    /// Editor class for StanfordLightFieldHelper.
    /// </summary>
    [CustomEditor(typeof(StanfordLightFieldHelper))]
    public class StanfordLightFieldHelperEditor : Editor
    {

#region FIELDS

        private StanfordLightFieldHelper _targetObject;
        private SerializedProperty _propertyScaleFactor;
        private SerializedProperty _propertyRepositionAroundCenter;

#endregion //FIELDS

#region INHERITANCE_METHODS

        /// <summary>
        /// On selection, sets up the target properties.
        /// </summary>
        void OnEnable()
        {
            _targetObject = (StanfordLightFieldHelper)serializedObject.targetObject;
            // Get the target properties.
            _propertyScaleFactor = serializedObject.FindProperty(StanfordLightFieldHelper.propertyNameScaleFactor);
            _propertyRepositionAroundCenter = serializedObject.FindProperty(StanfordLightFieldHelper.propertyNameRepositionAroundCenter);
        }

        /// <summary>
        /// Indicates that the object has frame bounds.
        /// </summary>
        /// <returns></returns> True.
        private bool HasFrameBounds()
        {
            return true;
        }

        /// <summary>
        /// On being asked for them, returns the object's frame bounds.
        /// </summary>
        /// <returns></returns>
        private Bounds OnGetFrameBounds()
        {
            return _targetObject.cameraSetup.GetFrameBounds();
        }

        /// <summary>
        /// Displays a GUI enabling the user to set up a COLIBRI VR directory structure from a set of images from the Stanford Light Field Archive.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Start the GUI.
            GeneralToolkit.EditorStart(serializedObject, _targetObject);

            // Enable the user to select the input directory.
            GeneralToolkit.EditorNewSection("Data directory");
            SectionDataDirectory();

            // Enable the user to parse the camera setup from the set of images.
            GeneralToolkit.EditorNewSection("Parse camera setup");
            SectionParseCameraSetup();

            // End the GUI.
            GeneralToolkit.EditorEnd(serializedObject);
        }

#endregion //INHERITANCE_METHODS

#region METHODS

        /// <summary>
        /// Enables the user to choose the directory containing the rectified images from the Stanford Light Field Archive.
        /// </summary>
        public void SectionDataDirectory()
        {
            EditorGUILayout.Space();
            string label = "The rectified images downloaded from the Stanford Light Field Archive should be placed in a folder named \"images\".";
            EditorGUILayout.LabelField(label, GeneralToolkit.wordWrapStyle);
            label = "Please specify the parent directory containing this \"images\" folder:";
            EditorGUILayout.LabelField(label, GeneralToolkit.wordWrapStyle);
            EditorGUILayout.Space();
            string searchTitle = "Select the parent directory of the directory containing the rectified images";
            string tooltip = "Path must be within the Unity project folder (but can be outside of Assets).";
            bool clicked;
            string outPath;
            GeneralToolkit.EditorPathSearch(out clicked, out outPath, PathType.Directory, _targetObject.dataHandler.dataDirectory, searchTitle, tooltip, Color.grey);
            _targetObject.dataHandler.ChangeDataDirectory(outPath, clicked);
            _targetObject.dataHandler.CheckStatusOfSourceData();
            EditorGUILayout.LabelField("Detected color images: " + _targetObject.dataHandler.sourceColorCount +  ".");
        }

        /// <summary>
        /// Enables the user to parse the camera setup from the set of images.
        /// </summary>
        public void SectionParseCameraSetup()
        {
            string label = "Scaling factor: ";
            string tooltip = "Original position values from the Stanford Light Field Archive will be scaled by this factor.";
            _propertyScaleFactor.floatValue = EditorGUILayout.FloatField(new GUIContent(label, tooltip), _propertyScaleFactor.floatValue);
            label = "Reposition: ";
            tooltip = "Original position values from the Stanford Light Field Archive will be repositioned around their mean value, to result in a centered object.";
            _propertyRepositionAroundCenter.boolValue = EditorGUILayout.Toggle(new GUIContent(label, tooltip), _propertyRepositionAroundCenter.boolValue);
            bool isGUIEnabled = GUI.enabled;
            GUI.enabled = isGUIEnabled && _targetObject.dataHandler.sourceColorCount > 0;
            EditorGUILayout.Space();
            label = "Parse and save";
            tooltip = "Camera setup will be parsed and saved here: \"" + _targetObject.dataHandler.dataDirectory + "\".";
            if(_targetObject.dataHandler.sourceColorCount < 1)
                tooltip = "No color images were found in the \"images\" folder of the directory: \"" + _targetObject.dataHandler.dataDirectory + "\".";
            if(GUILayout.Button(new GUIContent(label, tooltip)))
            {
                label = "Setup will be parsed, and directory structure will be modified. Are you ready to proceed?";
                tooltip = "Launching this process will modify the folder: \"" + _targetObject.dataHandler.dataDirectory + "\". Are you ready to proceed?";
                if(EditorUtility.DisplayDialog(label, tooltip, "Yes", "No"))
                {
                    _targetObject.ParseCameraSetup();
                }
            }
        }

#endregion //METHODS

    }

#endif //UNITY_EDITOR

}
