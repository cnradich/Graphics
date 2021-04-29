using UnityEngine;
using UnityEditor.Rendering;

namespace UnityEditor.Rendering.HighDefinition
{
    class ToonsToHDToonUpgrader : MaterialUpgrader
    {
        string Toon_Color = "Toon/Color";
        //string Toon_Texture = "Toon/Texture";
        string Toon_Transparent = "Toon/Transparent";
        string Toon_Cutout = "Toon/Transparent Cutout";

        public ToonsToHDToonUpgrader(string sourceShaderName, string destShaderName, MaterialFinalizer finalizer = null)
        {
            RenameShader(sourceShaderName, destShaderName, finalizer);

            if (sourceShaderName == Toon_Color)
                RenameColor("_Color", "_ToonColor");
            else // all other toon have a texture
                RenameTexture("_MainTex", "_ToonColorMap");

            if (sourceShaderName == Toon_Cutout)
            {
                RenameFloat("_Cutoff", "_AlphaCutoff");
                SetFloat("_AlphaCutoffEnable", 1f);
            }
            else
                SetFloat("_AlphaCutoffEnable", 0f);


            SetFloat("_SurfaceType", (sourceShaderName == Toon_Transparent) ? 1f : 0f);
        }

        public override void Convert(Material srcMaterial, Material dstMaterial)
        {
            //dstMaterial.hideFlags = HideFlags.DontUnloadUnusedAsset;

            base.Convert(srcMaterial, dstMaterial);

            HDShaderUtils.ResetMaterialKeywords(dstMaterial);
        }
    }
}
