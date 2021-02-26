#ifndef UNIVERSAL_TOON_TYPES_INCLUDED
#define UNIVERSAL_TOON_TYPES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct ToonInputData
{
    Light keyLight;
    AmbientOcclusionFactor aoFactor;
    half cameraDistance;
    float3 positionWS;
    float3 positionCS;
    half3 normalWS;
    half3 viewDirectionWS;
    float4 shadowCoord;
    half bakedGI;
    half fogCoord;
    half3 vertexLighting;
    float2 normalizedScreenSpaceUV;
    half4 shadowMask;
};

struct ToonSurfaceData
{
    half3 baseColor;
    half3 shadowColor;
    half3 specular;
    half3 normalTS; // TODO don't need this right?
    half3 emission;
    half occlusion;
    half alpha;
    half numLightLevels;
    half terminatorBias;
};

#endif
