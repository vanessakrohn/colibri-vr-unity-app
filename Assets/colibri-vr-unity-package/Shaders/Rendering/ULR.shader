/// Copyright 2019-2020 MINES ParisTech (PSL University)
/// This work is licensed under the terms of the MIT license, see the LICENSE file.
/// 
/// Author: Grégoire Dupont de Dinechin, gregoire@dinechin.org



/// <summary> 
/// Blends the source data based on the per-vertex Unstructured Lumigraph Rendering (ULR) algorithm.
/// This is the FPS-friendly but lower-quality option: camera blending weights are computed in the vertex shader instead of the fragment shader.
///
/// This shader is inspired by an algorithm presented in:
/// Buehler et al., Unstructured Lumigraph Rendering, 2001, https://doi.org/10.1145/383259.383309.
/// </summary>
Shader "COLIBRIVR/Rendering/ULR"
{
    Properties
    {
        _ColorData ("Color data", 2DArray) = "white" {}
        _DepthData ("Depth data", 2DArray) = "white" {}
        _GlobalTextureMap ("Global texture map", 2D) = "black" {}
        _LossyScale ("Lossy scale", Vector) = (0, 0, 0)
        _SourceCamCount ("Number of source cameras", int) = 1
        _BlendCamCount ("Number of source cameras that will have a non-zero blending weight for a given fragment", int) = 1
        _MaxBlendAngle ("Maximum angle difference (degrees) between source ray and view ray for the color value to be blended", float) = 180.0
        _ResolutionWeight ("Relative impact of the {resolution} factor compared to the {angle difference} factor in the ULR algorithm", float) = 0.2
        _DepthCorrectionFactor ("Factor for depth correction", float) = 0.1
        _GlobalTextureMapWeight ("Relative weight of the global texture map, compared to the estimated color.", float) = 0.1
        _IsColorSourceCamIndices ("Whether the displayed colors help visualize the source camera indices instead of the actual texture colors.", int) = 0
        _BlendFieldComputationParams ("Parameters defining which vertices will compute their part of the blending field", Vector) = (0, 0, 0)
        _ExcludedSourceView ("Excluded source camera index", int) = -1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #pragma require geometry
            #pragma require 2darray
            #pragma target 4.5
            //
            #include "./../CGIncludes/ULRCG.cginc"
            
            #include "UnityCG.cginc"

            struct appdata
            {
                uint vertexID : SV_VertexID;
                float4 objectXYZW : POSITION;
                float3 objectNormalXYZ : NORMAL;
                float2 texUV : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 clipXYZW : SV_POSITION;
                float2 texUV : TEXCOORD0;
                uint vertexID : TEXCOORD1;
                float3 worldXYZ : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 clipXYZW : SV_POSITION;
                float2 texUV : TEXCOORD0;
                float3 worldXYZ : TEXCOORD2;
                SourceCamContribution sourceCamContributions[_MaxBlendCamCount] : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata v)
            {
                v2g o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2g, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.clipXYZW = UnityObjectToClipPos(v.objectXYZW);
                o.texUV = v.texUV;
                o.vertexID = v.vertexID;
                o.worldXYZ = mul(unity_ObjectToWorld, v.objectXYZW).xyz;
                if (ShouldBlendFieldBeComputedInVertex(o.vertexID))
                {
                    float4 worldNormalXYZ = mul(unity_ObjectToWorld, v.objectNormalXYZ);
                    SourceCamContribution sourceCamContributions[_MaxBlendCamCount];
                    ComputeCamWeightsForVertex(o.worldXYZ, worldNormalXYZ, true, sourceCamContributions);
                    TransferArraysToBuffers(o.vertexID, sourceCamContributions);
                }
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                DEFAULT_UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i[0])

                float3 vertexIDs = uint3(i[0].vertexID, i[1].vertexID, i[2].vertexID);
                SourceCamContributionsForTriangle sourceCamContributionsForTriangle;
                SortIndexWeightsListForTriangle(vertexIDs, sourceCamContributionsForTriangle);
                uint blendCamCount = GetBlendCamCount();
                [unroll]
                for (uint iter = 0; iter < 3; iter++)
                {
                    g2f o;
                    UNITY_INITIALIZE_OUTPUT(g2f, o);

                    v2g iterVert = i[iter];

                    UNITY_SETUP_INSTANCE_ID(iterVert);

                    o.clipXYZW = iterVert.clipXYZW;
                    o.texUV = iterVert.texUV;
                    o.worldXYZ = iterVert.worldXYZ;
                    [unroll]
                    for (uint blendCamIndex = 0; blendCamIndex < blendCamCount; blendCamIndex++)
                    {
                        uint sourceCamIndex = sourceCamContributionsForTriangle.indexList[blendCamIndex];
                        o.sourceCamContributions[blendCamIndex].index = sourceCamIndex;
                        o.sourceCamContributions[blendCamIndex].texUV = ComputeSourceTexUV(
                            iterVert.worldXYZ, sourceCamIndex);
                        o.sourceCamContributions[blendCamIndex].weight = sourceCamContributionsForTriangle.weightList[
                            blendCamIndex][iter];
                    }

                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(iterVert, o);
                    triangleStream.Append(o);
                }
            }

            fixed4 frag(g2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 color = ComputeColorFromVertexCamWeights(i.sourceCamContributions, true, i.worldXYZ);
                NormalizeByAlpha(color);
                if (_IsColorSourceCamIndices == 0)
                    MergeColorWithGlobalTextureMap(color, i.texUV);
                if (color.a == 0)
                    clip(-1);
                return color;
            }
            ENDCG
        }
    }
}