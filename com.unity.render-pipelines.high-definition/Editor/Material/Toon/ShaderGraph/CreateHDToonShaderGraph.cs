using System;
using UnityEditor.ShaderGraph;

namespace UnityEditor.Rendering.HighDefinition.ShaderGraph
{
    static class CreateToonShaderGraph
    {
        [MenuItem("Assets/Create/Shader/HD Render Pipeline/Toon Shader Graph", false, 208)]
        public static void CreateHDToonGraph()
        {
            var target = (HDTarget)Activator.CreateInstance(typeof(HDTarget));
            target.TrySetActiveSubTarget(typeof(HDToonSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Alpha,
            };

            GraphUtil.CreateNewGraphWithOutputs(new[] {target}, blockDescriptors);
        }
    }
}
