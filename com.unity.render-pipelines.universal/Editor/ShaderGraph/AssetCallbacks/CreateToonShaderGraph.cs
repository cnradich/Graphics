using System;
using UnityEditor.ShaderGraph;


// ReSharper disable once CheckNamespace
namespace UnityEditor.Rendering.Universal.ShaderGraph
{
    internal static class CreateToonShaderGraph
    {
        [MenuItem("Assets/Create/Shader/Universal Render Pipeline/Toon Shader Graph", false, 300)]
        public static void CreateToonGraph()
        {
            UniversalTarget target = (UniversalTarget) Activator.CreateInstance(typeof(UniversalTarget));
            target.TrySetActiveSubTarget(typeof(UniversalToonSubTarget));

            BlockFieldDescriptor[] blockDescriptors =
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.Smoothness,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Occlusion,
            };

            GraphUtil.CreateNewGraphWithOutputs(new Target[] {target}, blockDescriptors);
        }
    }
}
