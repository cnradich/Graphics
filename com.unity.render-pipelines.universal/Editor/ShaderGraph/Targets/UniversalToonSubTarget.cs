using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


// ReSharper disable once CheckNamespace
namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    /// <summary>
    ///     This class is copied from <see cref="UniversalUnlitSubTarget" />, so if something doesn't make sense, refer
    ///     to the associated statement in that implementation for further context.
    /// </summary>
    internal sealed class UniversalToonSubTarget : SubTarget<UniversalTarget>, ILegacyTarget
    {
        // UniversalToonSubTarget.cs GUID
        private static readonly GUID metaGuid = new GUID("ad2b4a78a0becdc4f9817116d60f92b5");

        [SerializeField]
        private NormalDropOffSpace normalDropOffSpace = NormalDropOffSpace.Tangent;

        public UniversalToonSubTarget() => displayName = "Toon";

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(metaGuid, AssetCollection.Flags.SourceDependency);

            SubShaderDescriptor[] subShaders = {SubShaders.Toon, SubShaders.ToonDots};

            for (int i = 0; i < subShaders.Length; i++)
            {
                subShaders[i].renderType = target.renderType;
                subShaders[i].renderQueue = target.renderQueue;
                context.AddSubShader(subShaders[i]);
            }
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            bool isOpaque = target.surfaceType == SurfaceType.Opaque;
            context.AddField(UniversalFields.SurfaceOpaque, isOpaque);
            context.AddField(UniversalFields.SurfaceTransparent, !isOpaque);
            context.AddField(UniversalFields.BlendAdd, !isOpaque && target.alphaMode == AlphaMode.Additive);
            context.AddField(Fields.BlendAlpha, !isOpaque && target.alphaMode == AlphaMode.Alpha);
            context.AddField(UniversalFields.BlendMultiply, !isOpaque && target.alphaMode == AlphaMode.Multiply);
            context.AddField(UniversalFields.BlendPremultiply, !isOpaque && target.alphaMode == AlphaMode.Premultiply);

            context.AddField(UniversalFields.NormalDropOffOS, normalDropOffSpace == NormalDropOffSpace.Object);
            context.AddField(UniversalFields.NormalDropOffTS, normalDropOffSpace == NormalDropOffSpace.Tangent);
            context.AddField(UniversalFields.NormalDropOffWS, normalDropOffSpace == NormalDropOffSpace.World);
            context.AddField(UniversalFields.SpecularSetup);

            BlockFieldDescriptor[] descriptors = context.blocks.Select(x => x.descriptor).ToArray();
            context.AddField
            (
                UniversalFields.Normal,
                descriptors.Contains(BlockFields.SurfaceDescription.NormalOS) ||
                descriptors.Contains(BlockFields.SurfaceDescription.NormalTS) ||
                descriptors.Contains(BlockFields.SurfaceDescription.NormalWS)
            );
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(ToonBlockFields.SurfaceDescription.ShadowColor);
            context.AddBlock(BlockFields.SurfaceDescription.NormalOS, normalDropOffSpace == NormalDropOffSpace.Object);
            context.AddBlock(BlockFields.SurfaceDescription.NormalTS, normalDropOffSpace == NormalDropOffSpace.Tangent);
            context.AddBlock(BlockFields.SurfaceDescription.NormalWS, normalDropOffSpace == NormalDropOffSpace.World);
            context.AddBlock(BlockFields.SurfaceDescription.Emission);
            context.AddBlock(BlockFields.SurfaceDescription.Occlusion);
            context.AddBlock(BlockFields.SurfaceDescription.Specular);
            context.AddBlock(ToonBlockFields.SurfaceDescription.NumLightLevels);
            context.AddBlock(ToonBlockFields.SurfaceDescription.TerminatorBias);
            context.AddBlock
            (
                BlockFields.SurfaceDescription.Alpha,
                target.surfaceType == SurfaceType.Transparent || target.alphaClip
            );
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, target.alphaClip);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="onChange"></param>
        /// <param name="registerUndo"></param>
        public override void GetPropertiesGUI
        (
            ref TargetPropertyGUIContext context,
            Action onChange,
            Action<string> registerUndo
        )
        {
            context.AddProperty
            (
                "Surface",
                new EnumField(SurfaceType.Opaque) {value = target.surfaceType},
                evt =>
                {
                    if (Equals(target.surfaceType, evt.newValue)) return;

                    registerUndo("Change Surface");
                    target.surfaceType = (SurfaceType) evt.newValue;
                    onChange();
                }
            );

            context.AddProperty
            (
                "Blend",
                new EnumField(AlphaMode.Alpha) {value = target.alphaMode},
                target.surfaceType == SurfaceType.Transparent,
                evt =>
                {
                    if (Equals(target.alphaMode, evt.newValue)) return;

                    registerUndo("Change Blend");
                    target.alphaMode = (AlphaMode) evt.newValue;
                    onChange();
                }
            );

            context.AddProperty
            (
                "Alpha Clip",
                new Toggle {value = target.alphaClip},
                evt =>
                {
                    if (Equals(target.alphaClip, evt.newValue)) return;

                    registerUndo("Change Alpha Clip");
                    target.alphaClip = evt.newValue;
                    onChange();
                }
            );

            context.AddProperty
            (
                "Two Sided",
                new Toggle {value = target.twoSided},
                evt =>
                {
                    if (Equals(target.twoSided, evt.newValue)) return;

                    registerUndo("Change Two Sided");
                    target.twoSided = evt.newValue;
                    onChange();
                }
            );

            context.AddProperty
            (
                "Fragment Normal Space",
                new EnumField(NormalDropOffSpace.Tangent) {value = normalDropOffSpace},
                evt =>
                {
                    if (Equals(normalDropOffSpace, evt.newValue)) return;

                    registerUndo("Change Fragment Normal Space");
                    normalDropOffSpace = (NormalDropOffSpace) evt.newValue;
                    onChange();
                }
            );
        }

        public bool TryUpgradeFromMasterNode
        (
            IMasterNode1 masterNode,
            out Dictionary<BlockFieldDescriptor, int> blockMap
        )
        {
            Debug.LogWarning("Couldn't upgrade from master node. (This probably shouldn't happen for toon shader?)");
            blockMap = null;
            return false;
        }

        private static class SubShaders
        {
            public static readonly SubShaderDescriptor Toon = new SubShaderDescriptor
            {
                pipelineTag = UniversalTarget.kPipelineTag,
                customTags = UniversalTarget.kToonMaterialTypeTag,
                generatesPreview = true,
                passes = new PassCollection
                {
                    ToonPasses.Toon,
                    ToonPasses.DepthNormals,
                    CorePasses.ShadowCaster,
                    CorePasses.DepthOnly
                },
            };

            public static SubShaderDescriptor ToonDots
            {
                get
                {
                    PassDescriptor toon = ToonPasses.Toon;
                    PassDescriptor depthNormals = ToonPasses.DepthNormals;
                    PassDescriptor shadowCaster = CorePasses.ShadowCaster;
                    PassDescriptor depthOnly = CorePasses.DepthOnly;

                    toon.pragmas = CorePragmas.DOTSForward;
                    depthNormals.pragmas = CorePragmas.DOTSInstanced;
                    shadowCaster.pragmas = CorePragmas.DOTSInstanced;
                    depthOnly.pragmas = CorePragmas.DOTSInstanced;

                    return new SubShaderDescriptor
                    {
                        pipelineTag = UniversalTarget.kPipelineTag,
                        customTags = UniversalTarget.kToonMaterialTypeTag,
                        generatesPreview = true,
                        passes = new PassCollection
                        {
                            toon,
                            depthNormals,
                            shadowCaster,
                            depthOnly
                        }
                    };
                }
            }
        }
        private static class ToonBlockFields
        {
            [GenerateBlocks("Universal Render Pipeline")]
            public struct SurfaceDescription
            {
                public static string name = "SurfaceDescription";
                public static BlockFieldDescriptor ShadowColor = new BlockFieldDescriptor
                (
                    name,
                    "ShadowColor",
                    "Shadow Color",
                    "SURFACEDESCRIPTION_SHADOWCOLOR",
                    new ColorControl(Color.grey, false),
                    ShaderStage.Fragment
                );
                public static BlockFieldDescriptor NumLightLevels = new BlockFieldDescriptor
                (
                    name,
                    "NumLightLevels",
                    "Number of Light Levels",
                    "SURFACEDESCRIPTION_NUMLIGHTLEVELS",
                    new FloatControl(1.0f),
                    ShaderStage.Fragment
                );
                public static BlockFieldDescriptor TerminatorBias = new BlockFieldDescriptor
                (
                    name,
                    "TerminatorBias",
                    "Terminator Bias",
                    "SURFACEDESCRIPTION_TERMINATORBIAS",
                    new FloatControl(0.0f),
                    ShaderStage.Fragment
                );
            }
        }

        private static class ToonPasses
        {
            public static readonly PassDescriptor Toon = new PassDescriptor
            {
                // Definition
                displayName = "Toon",
                referenceName = "SHADERPASS_TOON",
                lightMode = "UniversalForward",
                useInPreview = true,

                // defines = toonDefines,

                // Template
                passTemplatePath = GenerationUtils.GetDefaultTemplatePath("PassMesh.template"),
                sharedTemplateDirectories = GenerationUtils.GetDefaultSharedTemplateDirectories(),

                // Port Mask
                validVertexBlocks = CoreBlockMasks.Vertex,
                validPixelBlocks = ToonBlockMasks.FragmentToon,

                // Fields
                structs = CoreStructCollections.Default,
                requiredFields = ToonRequiredFields.Toon,
                fieldDependencies = CoreFieldDependencies.Default,

                // Conditional State
                renderStates = CoreRenderStates.Default,
                pragmas = CorePragmas.Forward,
                keywords = ToonKeywords.Toon,
                includes = ToonIncludes.Toon,
            };

            public static readonly PassDescriptor DepthNormals = new PassDescriptor
            {
                // Definition
                displayName = "DepthNormals",
                referenceName = "SHADERPASS_DEPTHNORMALSONLY",
                lightMode = "DepthNormals",
                useInPreview = false,

                // Template
                passTemplatePath = GenerationUtils.GetDefaultTemplatePath("PassMesh.template"),
                sharedTemplateDirectories = GenerationUtils.GetDefaultSharedTemplateDirectories(),

                // Port Mask
                validVertexBlocks = CoreBlockMasks.Vertex,
                validPixelBlocks = ToonBlockMasks.FragmentDepthNormals,

                // Fields
                structs = CoreStructCollections.Default,
                requiredFields = ToonRequiredFields.DepthNormals,
                fieldDependencies = CoreFieldDependencies.Default,

                // Conditional State
                renderStates = CoreRenderStates.DepthNormalsOnly,
                pragmas = CorePragmas.Instanced,
                includes = CoreIncludes.DepthNormalsOnly,
            };
        }

        private static class ToonBlockMasks
        {
            public static readonly BlockFieldDescriptor[] FragmentDepthNormals =
            {
                BlockFields.SurfaceDescription.NormalOS,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.NormalWS,
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.AlphaClipThreshold,
            };

            public static readonly BlockFieldDescriptor[] FragmentToon =
            {
                BlockFields.SurfaceDescription.BaseColor,
                ToonBlockFields.SurfaceDescription.ShadowColor,
                BlockFields.SurfaceDescription.NormalOS,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.NormalWS,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Specular,
                BlockFields.SurfaceDescription.Occlusion,
                ToonBlockFields.SurfaceDescription.NumLightLevels,
                ToonBlockFields.SurfaceDescription.TerminatorBias,
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.AlphaClipThreshold
            };
        }

        private static class ToonRequiredFields
        {
            public static readonly FieldCollection DepthNormals = new FieldCollection
            {
                // needed for meta vertex position
                StructFields.Attributes.uv1,
                StructFields.Varyings.normalWS,
                // needed for vertex lighting
                StructFields.Varyings.tangentWS
            };

            public static readonly FieldCollection Toon = new FieldCollection
            {
                // needed for meta vertex position
                StructFields.Attributes.uv0,
                StructFields.Attributes.uv1,
                StructFields.Attributes.uv2,
                StructFields.Varyings.positionWS,
                StructFields.Varyings.normalWS,
                // needed for vertex lighting
                StructFields.Varyings.tangentWS,
                StructFields.Varyings.viewDirectionWS,
                UniversalStructFields.Varyings.lightmapUV,
                UniversalStructFields.Varyings.sh,
                // fog and vertex lighting, vert input is dependency
                UniversalStructFields.Varyings.fogFactorAndVertexLight,
                // shadow coord, vert input is dependency
                UniversalStructFields.Varyings.shadowCoord
            };
        }

        private static class ToonKeywords
        {
            public static readonly KeywordCollection Toon = new KeywordCollection
            {
                new KeywordDescriptor
                {
                    displayName = "Screen Space Ambient Occlusion",
                    referenceName = "_SCREEN_SPACE_OCCLUSION",
                    type = KeywordType.Boolean,
                    definition = KeywordDefinition.MultiCompile,
                    scope = KeywordScope.Global,
                },
                CoreKeywordDescriptors.Lightmap,
                CoreKeywordDescriptors.DirectionalLightmapCombined,
                CoreKeywordDescriptors.MainLightShadows,
                CoreKeywordDescriptors.MainLightShadowsCascade,
                CoreKeywordDescriptors.AdditionalLights,
                CoreKeywordDescriptors.AdditionalLightShadows,
                CoreKeywordDescriptors.ShadowsSoft,
                CoreKeywordDescriptors.LightmapShadowMixing,
                CoreKeywordDescriptors.ShadowsShadowmask,
                CoreKeywordDescriptors.SampleGI,
            };
        }

        private static class ToonIncludes
        {
            public static readonly IncludeCollection Toon = new IncludeCollection
            {
                CoreIncludes.CorePregraph,
                CoreIncludes.ShaderGraphPregraph,
                CoreIncludes.CorePostgraph,
                {toon, IncludeLocation.Pregraph},
                {toonPass, IncludeLocation.Postgraph}
            };
            private const string toonPass =
                "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ToonPass.hlsl";
            private const string toon =
                "Packages/com.unity.render-pipelines.universal/Shaders/Stylized/Toon/Toon.hlsl";
        }
    }
}
