using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Include material common properties names
using static UnityEngine.Rendering.HighDefinition.HDMaterialProperties;

namespace UnityEditor.Rendering.HighDefinition
{
    /// <summary>
    /// GUI for HDRP toon shaders (does not include shader graphs)
    /// </summary>
    class ToonGUI : HDShaderGUI
    {
        MaterialUIBlockList uiBlocks = new MaterialUIBlockList
        {
            new SurfaceOptionUIBlock(MaterialUIBlock.ExpandableBit.Base, features: SurfaceOptionUIBlock.Features.Toon),
            new ToonSurfaceInputsUIBlock(MaterialUIBlock.ExpandableBit.Input),
            new TransparencyUIBlock(MaterialUIBlock.ExpandableBit.Transparency),
            new EmissionUIBlock(MaterialUIBlock.ExpandableBit.Emissive),
            new AdvancedOptionsUIBlock(MaterialUIBlock.ExpandableBit.Advance, AdvancedOptionsUIBlock.Features.Instancing | AdvancedOptionsUIBlock.Features.AddPrecomputedVelocity)
        };

        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            using (var changed = new EditorGUI.ChangeCheckScope())
            {
                uiBlocks.OnGUI(materialEditor, props);
                ApplyKeywordsAndPassesIfNeeded(changed.changed, uiBlocks.materials);
            }
        }

        protected override void SetupMaterialKeywordsAndPass(Material material) => SetupToonKeywordsAndPass(material);

        // All Setup Keyword functions must be static. It allow to create script to automatically update the shaders with a script if code change
        public static void SetupToonKeywordsAndPass(Material material)
        {
            material.SetupBaseToonKeywords();
            material.SetupBaseToonPass();

            if (material.HasProperty(kEmissiveColorMap))
                CoreUtils.SetKeyword(material, "_EMISSIVE_COLOR_MAP", material.GetTexture(kEmissiveColorMap));

            // All the bits exclusively related to lit are ignored inside the BaseLitGUI function.
            BaseLitGUI.SetupStencil(material, receivesSSR: false, useSplitLighting: false);

            if (material.HasProperty(kAddPrecomputedVelocity))
            {
                CoreUtils.SetKeyword(material, "_ADD_PRECOMPUTED_VELOCITY", material.GetInt(kAddPrecomputedVelocity) != 0);
            }
        }
    }
} // namespace UnityEditor
