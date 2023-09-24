/// Copyright 2019-2020 MINES ParisTech (PSL University)
/// This work is licensed under the terms of the MIT license, see the LICENSE file.
/// 
/// Author: Grégoire Dupont de Dinechin, gregoire@dinechin.org
 
using UnityEngine;
using COLIBRIVR.Processing;

namespace COLIBRIVR
{

    public class AsyncMethod : MonoBehaviour
    {

#region CONST_FIELDS
        
        protected const string _propertyNameMaxBlendAngle = "_maxBlendAngle";
        protected const string _shaderNameSourceCamIndex = "_SourceCamIndex";
        protected const string _shaderNameSourceCamPosXYZ = "_SourceCamPosXYZ";
        protected const string _shaderNameSourceCamCount = "_SourceCamCount";
        protected const string _shaderNameSourceCamIsOmnidirectional = "_SourceCamIsOmnidirectional";
        protected const string _shaderNameDistanceToClosest = "_SourceCamDistanceToClosest";
        protected const string _shaderNameMaxBlendAngle = "_MaxBlendAngle";
        protected const string _shaderNameFocalLength = "_FocalLength";

#endregion //CONST_FIELDS
        
#region FIELDS

        public Rendering.AsyncRendering renderingCaller;
        public Processing.AsyncProcessing processingCaller;
        public DataHandler dataHandler;
        public CameraSetup cameraSetup;

        public AsyncColorTextureArray PMColorTextureArray;
        // public AsyncPerViewMeshesFS PMPerViewMeshesFS;
        public AsyncPerViewMeshesQSTR PMPerViewMeshesQSTR;
        public AsyncGlobalMeshEF PMGlobalMeshEF;
        public AsyncDepthTextureArray PMDepthTextureArray;
        public AsyncGlobalTextureMap PMGlobalTextureMap;
        public AsyncPerViewMeshesQSTRDTA PMPerViewMeshesQSTRDTA;

#endregion //FIELDS

#region INHERITANCE_METHODS

        /// <summary>
        /// On reset, reset the object's properties.
        /// </summary>
        public virtual void Reset()
        {
            renderingCaller = GeneralToolkit.GetParentOfType<Rendering.AsyncRendering>(transform);
            if(renderingCaller != null)
                processingCaller = renderingCaller.processing;
            else
                processingCaller = GeneralToolkit.GetParentOfType<Processing.AsyncProcessing>(transform);
            if(processingCaller != null)
            {
                dataHandler = processingCaller.dataHandler;
                cameraSetup = processingCaller.cameraSetup;
            }
        }

        /// <summary>
        /// Initializes the links to other methods.
        /// </summary>
        public virtual void InitializeLinks()
        {
            if(processingCaller != null)
            {
                AsyncProcessingMethod[] processingMethods = processingCaller.processingMethods;
                if(processingMethods != null)
                {
                    PMColorTextureArray = (AsyncColorTextureArray) processingMethods[AsyncProcessingMethod.indexColorTextureArray];
                    // PMPerViewMeshesFS = (PerViewMeshesFS) processingMethods[ProcessingMethod.indexPerViewMeshesFS];
                    PMPerViewMeshesQSTR = (AsyncPerViewMeshesQSTR) processingMethods[AsyncProcessingMethod.indexPerViewMeshesQSTR];
                    PMGlobalMeshEF = (AsyncGlobalMeshEF) processingMethods[AsyncProcessingMethod.indexGlobalMeshEF];
                    PMDepthTextureArray = (AsyncDepthTextureArray) processingMethods[AsyncProcessingMethod.indexDepthTextureArray];
                    PMGlobalTextureMap = (AsyncGlobalTextureMap) processingMethods[AsyncProcessingMethod.indexGlobalTextureMap];
                    PMPerViewMeshesQSTRDTA = (AsyncPerViewMeshesQSTRDTA) processingMethods[AsyncProcessingMethod.indexPerViewMeshesQSTRDTA];
                }
            }
        }

#endregion //INHERITANCE_METHODS

    }

}
