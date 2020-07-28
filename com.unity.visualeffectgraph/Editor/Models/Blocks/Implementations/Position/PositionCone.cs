using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Position")]
    class PositionCone : PositionBase
    {
        public enum HeightMode
        {
            Base,
            Volume
        }

        [VFXSetting, Tooltip("Controls whether particles are spawned on the base of the cone, or throughout the entire volume.")]
        public HeightMode heightMode;

        public override string name { get { return "Position (Cone)"; } }
        protected override float thicknessDimensions { get { return 2.0f; } }

        public class InputProperties
        {
            [Tooltip("Sets the cone used for positioning the particles.")]
            public ArcCone ArcCone = ArcCone.defaultValue;
        }

        public class CustomProperties
        {
            [Range(0, 1), Tooltip("Sets the position along the height to emit particles from when ‘Custom Emission’ is used.")]
            public float HeightSequencer = 0.0f;
            [Range(0, 1), Tooltip("Sets the position on the arc to emit particles from when ‘Custom Emission’ is used.")]
            public float ArcSequencer = 0.0f;
        }

        public override IEnumerable<VFXNamedExpression> parameters
        {
            get
            {
                foreach (var p in GetExpressionsFromSlots(this).Where(      e => e.name != "Thickness"
                                                                        ||  e.name != "ArcCone_center"))
                    yield return p; //TODOPAUL, exclude unused slot

                yield return new VFXNamedExpression(CalculateVolumeFactor(positionMode, 0, 1), "volumeFactor");

                VFXExpression center = inputSlots[0][0].GetExpression();
                VFXExpression direction = inputSlots[0][1].GetExpression();
                VFXExpression up = inputSlots[0][2].GetExpression();

                var left = VFXOperatorUtility.Cross(direction, up);

                //Test building a matrix
                var zero = VFXOperatorUtility.ZeroExpression[VFXValueType.Float];
                var one = VFXOperatorUtility.OneExpression[VFXValueType.Float];
                var m0 = new VFXExpressionCombine(direction.x, direction.y, direction.z, zero);
                var m1 = new VFXExpressionCombine(up.x, up.y, up.z, zero);
                var m2 = new VFXExpressionCombine(left.x, left.y, left.z, zero);
                var m3 = new VFXExpressionCombine(center.x, center.y, center.z, one);

                yield return new VFXNamedExpression(direction, "transformMatrix_a");
                yield return new VFXNamedExpression(up, "transformMatrix_b");
                yield return new VFXNamedExpression(left, "transformMatrix_c");
                yield return new VFXNamedExpression(center, "transformMatrix_d");

                VFXExpression radius0 = inputSlots[0][3].GetExpression();
                VFXExpression radius1 = inputSlots[0][4].GetExpression();
                VFXExpression height = inputSlots[0][5].GetExpression();
                VFXExpression tanSlope = (radius1 - radius0) / height;
                VFXExpression slope = new VFXExpressionATan(tanSlope);
                yield return new VFXNamedExpression(new VFXExpressionCombine(new VFXExpression[] { new VFXExpressionSin(slope), new VFXExpressionCos(slope) }), "sincosSlope");
            }
        }

        protected override bool needDirectionWrite
        {
            get
            {
                return true;
            }
        }

        public override string source
        {
            get
            {
                string outSource = "";

                if (spawnMode == SpawnMode.Random)
                    outSource += @"float theta = ArcCone_arc * RAND;";
                else
                    outSource += @"float theta = ArcCone_arc * ArcSequencer;";

                outSource += @"
float rNorm = sqrt(volumeFactor + (1 - volumeFactor) * RAND);

float2 sincosTheta;
sincos(theta, sincosTheta.x, sincosTheta.y);
float2 pos = (sincosTheta * rNorm);
";

                if (heightMode == HeightMode.Base)
                {
                    outSource += @"
float hNorm = 0.0f;
";
                }
                else if (spawnMode == SpawnMode.Random)
                {
                    float distributionExponent = positionMode == PositionMode.Surface ? 2.0f : 3.0f;
                    outSource += $@"
float hNorm = 0.0f;
if (abs(ArcCone_radius0 - ArcCone_radius1) > VFX_EPSILON)
{{
    // Uniform distribution on cone
    float heightFactor = ArcCone_radius0 / max(VFX_EPSILON,ArcCone_radius1);
    float heightFactorPow = pow(heightFactor, {distributionExponent});
    hNorm = pow(heightFactorPow + (1.0f - heightFactorPow) * RAND, rcp({distributionExponent}));
    hNorm = (hNorm - heightFactor) / (1.0f - heightFactor); // remap on [0,1]
}}
else
    hNorm = RAND; // Uniform distribution on cylinder
";
                }
                else
                {
                    outSource += @"
float hNorm = HeightSequencer;
";
                }

                outSource += @"
direction.xzy = normalize(float3(pos * sincosSlope.x, sincosSlope.y));
float3 finalPos = lerp(float3(pos * ArcCone_radius0, 0.0f), float3(pos * ArcCone_radius1, ArcCone_height), hNorm).xzy;

float3x4 trsMatrix =
{
    transformMatrix_a.x, transformMatrix_b.x, transformMatrix_c.x, transformMatrix_d.x,
    transformMatrix_a.y, transformMatrix_b.y, transformMatrix_c.y, transformMatrix_d.y,
    transformMatrix_a.z, transformMatrix_b.z, transformMatrix_c.z, transformMatrix_d.z
};

finalPos = mul(trsMatrix, float4(finalPos, 1.0f)).xyz;
position += finalPos;
";
                return outSource;
            }
        }
    }
}
