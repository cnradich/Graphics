using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.ShaderGraph;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

// We share the name of the properties in the UI to avoid duplication
using static UnityEditor.Rendering.HighDefinition.LitSurfaceInputsUIBlock.Styles;
using static UnityEditor.Rendering.HighDefinition.SurfaceOptionUIBlock.Styles;
using static UnityEditor.Rendering.HighDefinition.RefractionUIBlock.Styles;

namespace UnityEditor.Rendering.HighDefinition.ShaderGraph
{
    class HDToonSurfaceOptionPropertyBlock : SurfaceOptionPropertyBlock
    {
        class Styles
        {
            public static GUIContent shadowMatte = new GUIContent("Shadow Matte", "When enabled, shadow matte inputs are exposed on the master node.");
        }

        HDToonData toonData;

        public HDToonSurfaceOptionPropertyBlock(SurfaceOptionPropertyBlock.Features features, HDToonData toonData) : base(features)
            => this.toonData = toonData;

        protected override void CreatePropertyGUI()
        {
            base.CreatePropertyGUI();

            // HDToon specific properties:
            AddProperty(Styles.shadowMatte, () => toonData.enableShadowMatte, (newValue) => toonData.enableShadowMatte = newValue);
        }
    }
}
