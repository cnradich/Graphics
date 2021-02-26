#ifndef UNIVERSAL_TOON_INCLUDED
#define UNIVERSAL_TOON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/Shaders/Stylized/Toon/ToonTypes.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

/**
 * Scales the input linearly.
 */
float scale(float lowerBound, float upperBound, float input)
{
    return (input - lowerBound) / (upperBound - lowerBound);
}

float Scale(float lowerBound, float upperBound, float input)
{
    lowerBound = max(0, lowerBound);
    upperBound = min(1, upperBound);
    return (input - lowerBound) / (upperBound - lowerBound);
}

/// <summary>
/// Normalize a value currently in the 1 to -1 space.
/// </summary>
/// <example>
/// nono(1);
/// => 1
/// nono(0);
/// => 0.5
/// nono(-1);
/// => 0
/// </example>
/// <remarks>
/// The name "nono" is an acronym for "Normalize One to Negative One".
/// </remarks>
float nono(float value)
{
    return clamp(value / 2 + 0.5, 0, 1);
}

float lerp3(float x, float y, float z, float s)
{
    return s < 0.5 ? lerp(x, y, s * 2) : lerp(y, z, s * 2 - 1);
}

/// <param name="numHalfToneLevels">test 1</param>
/// <param name="falloff">test 2</param>
/// <param name="incidenceAngleCos">The cosine of the angle of incidence.</param>
half AttenuateSteppedLighting(half numHalfToneLevels, half falloff, half incidenceAngleCos)
{
    // return clamp(incidenceAngleCos, 0, 1);
    // return nono(incidenceAngleCos);
    numHalfToneLevels = max(1, numHalfToneLevels);
    for (int i = 0; i <= numHalfToneLevels; i++)
    {
        half lowerBound = 1.0h / numHalfToneLevels * i;
        half upperBound = 1.0h / numHalfToneLevels * (i + 1);
        if (incidenceAngleCos <= upperBound)
        {
            half upper = lowerBound + falloff;
            return clamp(lerp(lowerBound, upperBound, clamp(scale(lowerBound, upper, incidenceAngleCos), 0, 1)), 0, 1);
        }
    }
    return 1;
}

half ToonifyShadowMask(half shadowAttenuation, half camDist)
{
    return clamp(scale(0, 0.05f * camDist, shadowAttenuation), 0, 1);
}

half GetCastShadowValue(half shadowAttenuation, half cameraDistance)
{
    #ifdef _DISABLE_RECEIVE_SHADOWS_ON
    return 1;
    #else
    return ToonifyShadowMask(shadowAttenuation, cameraDistance);
    #endif
}

half GetAmbientOcclusionValue(half ambientOcclusion)
{
    #ifdef _DISABLE_AMBIENT_OCCLUSION_ON
    return 1;
    #else
    return clamp(ambientOcclusion, 0.f, 1.f);
    #endif
}

/// <summary>
///
/// TODO:
/// * Implement specular
/// * Implement highlight
/// * Implement rimlight
/// * Implement point lights
/// </summary>
/// <param name="inputData"></param>
/// <param name="surfaceData"></param>
half4 Toon(ToonInputData inputData, ToonSurfaceData surfaceData)
{
    // NOTE: When dealing with angles, we generally just use the cosine of the angle. This value is convenient to work
    //       with.

    // Glossary:
    // * Half tone - The lit area between the highlight and the terminator. In reality, this is a smooth continuous
    //               falloff, gradually getting darker as the incidence angle grows. For a cartoony effect, the
    //               half tone is instead one or more discreet levels of uniform color with hard edges.
    // * Core shadow - Area of the object where direct light cannot affect it (opposite side of the light)
    // * Cast shadow - The shadow cast by an object onto the surface of itself or another.
    // * Terminator - The divide between the lit surface and the core-shaded surface
    // * Incidence angle - The angle between a ray incident (light) and the surface normal

    // The length of falloff between light levels. Larger values produce a soft effect, and very small values produce
    // a hard edge (cartoony) effect with anti-aliasing. A value of 0 produces an aliased hard-edge
    static const half standardFalloffLength = 0.005;

    const half3 keyLitColor = surfaceData.baseColor * inputData.keyLight.color;
    const half3 coreShadowColor = surfaceData.shadowColor * inputData.keyLight.color;

    // The falloff "length" is measured in object space, so the further our camera is the less effective the falloff is
    // at producing the desired anti-aliasing effect. So we adjust the falloff based on the camera distance
    const half viewAdjustedFalloff = standardFalloffLength * inputData.cameraDistance;
    // 1 : 0 degrees. -1 : 180 degrees.
    const half incidenceAngleCos = dot(inputData.normalWS, inputData.keyLight.direction);
    // We can adjust the position of the terminator by modifying the incidence angle
    const half virtualIncidenceAngleCos = scale(surfaceData.terminatorBias, 1, incidenceAngleCos);
    const half attenuatedLightValue_keyLight =
        AttenuateSteppedLighting(surfaceData.numLightLevels, viewAdjustedFalloff, virtualIncidenceAngleCos);
    // In order to preserve some semblance of shading when an object is completely cast in shadow, cast shadows do not
    // "occlude" light as much as core shadows. Think of it as sort of an "ambient lighting" effect.
    // `nono` coincidentally works for this so we'll use it.
    const half castShadowLightValue_keyLight =
        nono(GetCastShadowValue(inputData.keyLight.shadowAttenuation, inputData.cameraDistance));
    const half compositeLightValue_keyLight = min(attenuatedLightValue_keyLight, castShadowLightValue_keyLight);
    const half aoValue = GetAmbientOcclusionValue(inputData.aoFactor.directAmbientOcclusion);

    half3 color = lerp(coreShadowColor, keyLitColor, compositeLightValue_keyLight) * aoValue;
    return half4(color, surfaceData.alpha);
}

#endif
