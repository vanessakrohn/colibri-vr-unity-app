﻿/// Copyright 2019-2020 MINES ParisTech (PSL University)
/// This work is licensed under the terms of the MIT license, see the LICENSE file.
/// 
/// Author: Grégoire Dupont de Dinechin, gregoire@dinechin.org

using System.IO;
using System.Collections;
using UnityEngine;

namespace COLIBRIVR.ExternalConnectors
{

    /// <summary>
    /// Class handling the connection between COLIBRI VR and Blender.
    /// For more information on Blender, see: https://www.blender.org/
    /// </summary>
    public static class BlenderConnector
    {

#region CONST_FIELDS

        public const string convertPLYtoOBJOutputFileName = "meshed-delaunay.obj";

#endregion //CONST_FIELDS

#region STATIC_PROPERTIES
        
        private static string _externalPythonScriptsDir
        {
            get
            {
                return Path.Combine(PackageReference.GetPackagePath(false), Path.Combine("Runtime", "ExternalConnectors"));
            }
        }
        private static string _convertPLYtoOBJFileName { get { return "Blender_ConvertPLYtoOBJ.py"; } }
        private static string _checkOBJMeshInfoFileName { get { return "Blender_CheckOBJMeshInfo.py"; } }
        private static string _simplifyOBJFileName { get { return "Blender_SimplifyOBJ.py"; } }
        private static string _smartUVProjectOBJFileName { get { return "Blender_SmartUVProjectOBJ.py"; } }
        private static string[] _harmlessWarnings { get { return new string[] {"Failed to set 44100hz, got 48000hz instead", "Failed to set 48000hz, got 44100hz instead"}; } }

#endregion //STATIC_PROPERTIES

#if UNITY_EDITOR

#region STATIC_METHODS

        /// <summary>
        /// Initializes a coroutine that launches a Blender python command.
        /// </summary>
        /// <param name="clearConsole"></param> True if the console should be cleared, false otherwise.
        /// <param name="displayProgressBar"></param> Outputs whether to display a progress bar.
        /// <param name="stopOnError"></param> Outputs whether to stop on error.
        /// <param name="progressBarParams"></param> Outputs paramters for the progress bar.
        private static void InitBlenderCoroutineParams(bool clearConsole, out bool displayProgressBar, out bool stopOnError, out string[] progressBarParams)
        {
            // Initialize the command parameters.
            displayProgressBar = true;
            stopOnError = true;
            progressBarParams = new string[3];
            progressBarParams[0] = "2";
            progressBarParams[2] = "Processing canceled by user.";
        }

        /// <summary>
        /// Formats a command for Blender python from the given arguments.
        /// </summary>
        /// <param name="fileName"></param> The name of the python file to call.
        /// <param name="args"></param> The arguments to provide for the python file.
        /// <returns></returns> The formatted command.
        private static string FormatBlenderCommand(string fileName, params string[] args)
        {
            string formattedExePath = BlenderSettings.formattedBlenderExePath;
            string formattedFileName = GeneralToolkit.FormatPathForCommand(fileName);
            string formattedScriptsDir = GeneralToolkit.FormatPathForCommand(_externalPythonScriptsDir);
            string commandArgs = string.Empty;
            foreach(string arg in args)
                commandArgs += GeneralToolkit.FormatPathForCommand(arg) + " ";
            string command = "CALL " + formattedExePath + " --factory-startup --background --python " + formattedFileName + " -- " + formattedScriptsDir + " " + commandArgs;
            return command;
        }

        /// <summary>
        /// Coroutine that converts a .PLY mesh to a .OBJ file.
        /// </summary>
        /// <param name="caller"></param> The Blender helper object calling this method.
        /// <param name="inputFilePath"></param> The full path to the input .PLY file.
        /// <param name="outputFilePath"></param> The full path to the output .OBJ file.
        /// <param name="storeFaceCount"></param> Action that stores the mesh's face count.
        /// <returns></returns>
        public static IEnumerator RunConvertPLYtoOBJCoroutine(BlenderHelper caller, string inputFilePath, string outputFilePath, System.Diagnostics.DataReceivedEventHandler storeFaceCount)
        {
            // Indicate to the user that the process has started.
            GeneralToolkit.ResetCancelableProgressBar(true, true);
            // Initialize the coroutine parameters.
            bool displayProgressBar; bool stopOnError; string[] progressBarParams;
            InitBlenderCoroutineParams(true, out displayProgressBar, out stopOnError, out progressBarParams);
            progressBarParams[1] = "Convert .PLY to .OBJ";
            // Launch the command.
            string command = FormatBlenderCommand(_convertPLYtoOBJFileName, inputFilePath, outputFilePath);
            yield return caller.StartCoroutine(GeneralToolkit.RunCommandCoroutine(typeof(BlenderConnector), command, _externalPythonScriptsDir, displayProgressBar, storeFaceCount, _harmlessWarnings, stopOnError, progressBarParams));
            // Update the data handler to see the converted mesh object.
            caller.dataHandler.CheckStatusOfSourceData();
            // Display the converted mesh in the Scene view.
            caller.DisplayMeshInSceneView(outputFilePath);
            // Indicate to the user that the process has ended.
            GeneralToolkit.ResetCancelableProgressBar(false, false);
        }

        // /// <summary>
        // /// Coroutine that checks an .OBJ's mesh information.
        // /// </summary>
        // /// <param name="caller"></param> The object calling this method.
        // /// <param name="inputFilePath"></param> The full path to the input .OBJ file.
        // /// <param name="storeFaceCount"></param> Action that stores the mesh's face count.
        // /// <returns></returns>
        // public static IEnumerator RunCheckOBJMeshInfoCoroutine(MonoBehaviour caller, string inputFilePath, System.Diagnostics.DataReceivedEventHandler storeFaceCount)
        // {
        //     // Indicate to the user that the process has started.
        //     GeneralToolkit.ResetCancelableProgressBar(true, false);
        //     // Initialize the coroutine parameters.
        //     bool displayProgressBar; bool stopOnError; string[] progressBarParams;
        //     InitBlenderCoroutineParams(false, out displayProgressBar, out stopOnError, out progressBarParams);
        //     progressBarParams[1] = "Check .OBJ mesh information";
        //     // Launch the command.
        //     string command = FormatBlenderCommand(_checkOBJMeshInfoFileName, inputFilePath);
        //     yield return caller.StartCoroutine(GeneralToolkit.RunCommandCoroutine(typeof(BlenderConnector), command, _externalPythonScriptsDir, displayProgressBar, storeFaceCount, _harmlessWarnings, stopOnError, progressBarParams));
        //     // Indicate to the user that the process has ended.
        //     GeneralToolkit.ResetCancelableProgressBar(false, false);
        // }

        /// <summary>
        /// Coroutine that simplifies the mesh in a .OBJ file using the decimation modifier.
        /// </summary>
        /// <param name="caller"></param> The Blender helper calling this method.
        /// <param name="inputFilePath"></param> The full path to the input .OBJ file.
        /// <param name="outputFilePath"></param> The full path to the output .OBJ file.
        /// <param name="storeFaceCount"></param> Action that stores the mesh's face count.
        /// <returns></returns>
        public static IEnumerator RunSimplifyOBJCoroutine(BlenderHelper caller, string inputFilePath, string outputFilePath, System.Diagnostics.DataReceivedEventHandler storeFaceCount)
        {
            // Indicate to the user that the process has started.
            GeneralToolkit.ResetCancelableProgressBar(true, true);
            // Initialize the coroutine parameters.
            bool displayProgressBar; bool stopOnError; string[] progressBarParams;
            InitBlenderCoroutineParams(true, out displayProgressBar, out stopOnError, out progressBarParams);
            progressBarParams[1] = "Convert .PLY to .OBJ";
            // Launch the command.
            string command = FormatBlenderCommand(_simplifyOBJFileName, inputFilePath, outputFilePath);
            yield return caller.StartCoroutine(GeneralToolkit.RunCommandCoroutine(typeof(BlenderConnector), command, _externalPythonScriptsDir, displayProgressBar, storeFaceCount, _harmlessWarnings, stopOnError, progressBarParams));
            // Display the simplified mesh in the Scene view.
            caller.DisplayMeshInSceneView(outputFilePath);
            // Indicate to the user that the process has ended.
            GeneralToolkit.ResetCancelableProgressBar(false, false);
        }

        /// <summary>
        /// Coroutine that applies the Smart UV Project algorithm to the given .OBJ file.
        /// </summary>
        /// <param name="caller"></param> The Blender helper calling this method.
        /// <param name="inputFilePath"></param> The full path to the input .OBJ file.
        /// <param name="outputFilePath"></param> The full path to the output .OBJ file.
        /// <returns></returns>
        public static IEnumerator RunSmartUVProjectOBJCoroutine(BlenderHelper caller, string inputFilePath, string outputFilePath)
        {
            // Indicate to the user that the process has started.
            GeneralToolkit.ResetCancelableProgressBar(true, true);
            // Initialize the coroutine parameters.
            bool displayProgressBar; bool stopOnError; string[] progressBarParams;
            InitBlenderCoroutineParams(true, out displayProgressBar, out stopOnError, out progressBarParams);
            progressBarParams[1] = "Smart UV Project on .OBJ";
            // Launch the command.
            string command = FormatBlenderCommand(_smartUVProjectOBJFileName, inputFilePath, outputFilePath);
            yield return caller.StartCoroutine(GeneralToolkit.RunCommandCoroutine(typeof(BlenderConnector), command, _externalPythonScriptsDir, displayProgressBar, null, _harmlessWarnings, stopOnError, progressBarParams));
            // Display the uv-mapped mesh in the Scene view.
            caller.DisplayMeshInSceneView(outputFilePath);
            // Indicate to the user that the process has ended.
            GeneralToolkit.ResetCancelableProgressBar(false, false);
        }

#endregion //STATIC_METHODS

#endif //UNITY_EDITOR

    }

}