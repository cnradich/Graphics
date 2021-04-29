using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Legacy;
using UnityEditor.Rendering.HighDefinition.ShaderGraph.Legacy;
using static UnityEngine.Rendering.HighDefinition.HDMaterialProperties;
using static UnityEditor.Rendering.HighDefinition.HDShaderUtils;

namespace UnityEditor.Rendering.HighDefinition.ShaderGraph
{
    sealed partial class HDToonSubTarget : ILegacyTarget
    {
        public bool TryUpgradeFromMasterNode(IMasterNode1 masterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        {
            blockMap = null;
            switch (masterNode)
            {
                // case ToonMasterNode1 toonMasterNode:
                //     UpgradeToonMasterNode(toonMasterNode, out blockMap);
                //     return true;
                // case HDToonMasterNode1 hdToonMasterNode:
                //     UpgradeHDToonMasterNode(hdToonMasterNode, out blockMap);
                //     return true;
                default:
                    return false;
            }
        }

        // void UpgradeToonMasterNode(ToonMasterNode1 toonMasterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        // {
        //     m_MigrateFromOldCrossPipelineSG = true;
        //     m_MigrateFromOldSG = true;
        //
        //     // Set data
        //     systemData.surfaceType = (SurfaceType)toonMasterNode.m_SurfaceType;
        //     systemData.blendMode = HDSubShaderUtilities.UpgradeLegacyAlphaModeToBlendMode((int)toonMasterNode.m_AlphaMode);
        //     // Previous master node wasn't having any renderingPass. Assign it correctly now.
        //     systemData.renderQueueType = systemData.surfaceType == SurfaceType.Opaque ? HDRenderQueue.RenderQueueType.Opaque : HDRenderQueue.RenderQueueType.Transparent;
        //     systemData.doubleSidedMode = toonMasterNode.m_TwoSided ? DoubleSidedMode.Enabled : DoubleSidedMode.Disabled;
        //     systemData.alphaTest = HDSubShaderUtilities.UpgradeLegacyAlphaClip(toonMasterNode);
        //     systemData.dotsInstancing = false;
        //     systemData.transparentZWrite = false;
        //     builtinData.addPrecomputedVelocity = false;
        //     target.customEditorGUI = toonMasterNode.m_OverrideEnabled ? toonMasterNode.m_ShaderGUIOverride : "";
        //
        //     // Set blockmap
        //     blockMap = new Dictionary<BlockFieldDescriptor, int>()
        //     {
        //         { BlockFields.VertexDescription.Position, 9 },
        //         { BlockFields.VertexDescription.Normal, 10 },
        //         { BlockFields.VertexDescription.Tangent, 11 },
        //         { BlockFields.SurfaceDescription.BaseColor, 0 },
        //         { BlockFields.SurfaceDescription.Alpha, 7 },
        //         { BlockFields.SurfaceDescription.AlphaClipThreshold, 8 },
        //     };
        // }
        //
        // void UpgradeHDToonMasterNode(HDToonMasterNode1 hdToonMasterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        // {
        //     m_MigrateFromOldSG = true;
        //
        //     // Set data
        //     systemData.surfaceType = (SurfaceType)hdToonMasterNode.m_SurfaceType;
        //     systemData.blendMode = HDSubShaderUtilities.UpgradeLegacyAlphaModeToBlendMode((int)hdToonMasterNode.m_AlphaMode);
        //     systemData.renderQueueType = HDRenderQueue.MigrateRenderQueueToHDRP10(hdToonMasterNode.m_RenderingPass);
        //     // Patch rendering pass in case the master node had an old configuration
        //     if (systemData.renderQueueType == HDRenderQueue.RenderQueueType.Background)
        //         systemData.renderQueueType = HDRenderQueue.RenderQueueType.Opaque;
        //     systemData.alphaTest = hdToonMasterNode.m_AlphaTest;
        //     systemData.sortPriority = hdToonMasterNode.m_SortPriority;
        //     systemData.doubleSidedMode = hdToonMasterNode.m_DoubleSided ? DoubleSidedMode.Enabled : DoubleSidedMode.Disabled;
        //     systemData.transparentZWrite = hdToonMasterNode.m_ZWrite;
        //     systemData.transparentCullMode = hdToonMasterNode.m_transparentCullMode;
        //     systemData.zTest = hdToonMasterNode.m_ZTest;
        //     systemData.dotsInstancing = hdToonMasterNode.m_DOTSInstancing;
        //
        //     builtinData.transparencyFog = hdToonMasterNode.m_TransparencyFog;
        //     builtinData.distortion = hdToonMasterNode.m_Distortion;
        //     builtinData.distortionMode = hdToonMasterNode.m_DistortionMode;
        //     builtinData.distortionDepthTest = hdToonMasterNode.m_DistortionDepthTest;
        //     builtinData.alphaToMask = hdToonMasterNode.m_AlphaToMask;
        //     builtinData.addPrecomputedVelocity = hdToonMasterNode.m_AddPrecomputedVelocity;
        //
        //     toonData.distortionOnly = hdToonMasterNode.m_DistortionOnly;
        //     toonData.enableShadowMatte = hdToonMasterNode.m_EnableShadowMatte;
        //     target.customEditorGUI = hdToonMasterNode.m_OverrideEnabled ? hdToonMasterNode.m_ShaderGUIOverride : "";
        //
        //     // Set blockmap
        //     blockMap = new Dictionary<BlockFieldDescriptor, int>()
        //     {
        //         { BlockFields.VertexDescription.Position, 9 },
        //         { BlockFields.VertexDescription.Normal, 13 },
        //         { BlockFields.VertexDescription.Tangent, 14 },
        //         { BlockFields.SurfaceDescription.BaseColor, 0 },
        //         { BlockFields.SurfaceDescription.Alpha, 7 },
        //         { BlockFields.SurfaceDescription.AlphaClipThreshold, 8 },
        //         { BlockFields.SurfaceDescription.Emission, 12 },
        //     };
        //
        //     // Distortion
        //     if (systemData.surfaceType == SurfaceType.Transparent && builtinData.distortion)
        //     {
        //         blockMap.Add(HDBlockFields.SurfaceDescription.Distortion, 10);
        //         blockMap.Add(HDBlockFields.SurfaceDescription.DistortionBlur, 11);
        //     }
        //
        //     // Shadow Matte
        //     if (toonData.enableShadowMatte)
        //     {
        //         blockMap.Add(HDBlockFields.SurfaceDescription.ShadowTint, 15);
        //     }
        // }
    }
}
