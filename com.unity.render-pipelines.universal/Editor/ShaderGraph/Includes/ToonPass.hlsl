// #include <UnityInstancing.cginc>
// #include "../../../ShaderLibrary/Input.hlsl"
// #include "../../../ShaderLibrary/SurfaceData.hlsl"

// struct Attributes
// {
// };
//
// struct Varyings
// {
// };
//
// struct PackedVaryings
// {
// };
//
// struct SurfaceDescriptionInputs
// {
// };
//
// struct SurfaceDescription
// {
//     float3 BaseColor;
//     float3 ShadowColor;
//     float4 NormalOS;
//     float4 NormalTS;
//     float4 NormalWS;
//     float3 Emission;
//     float3 Specular;
//     float Occlusion;
//     float Alpha;
//     float AlphaClipThreshold;
// };
//
// Varyings BuildVaryings(Attributes attributes);
// PackedVaryings PackVaryings(Varyings varyings);
// Varyings UnpackVaryings(PackedVaryings packedVaryings);
// SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings Varyings);
// SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs inputs);

void BuildInputData(Varyings input, SurfaceDescription surfaceDescription, out ToonInputData inputData)
{
    inputData.keyLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS.xyz));
    inputData.aoFactor = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(input.positionCS));
    inputData.cameraDistance = distance(input.positionWS, GetCameraPositionWS());
    inputData.positionWS = input.positionWS;
    inputData.positionCS = input.positionCS;

    #ifdef _NORMALMAP
    #if _NORMAL_DROPOFF_TS
            // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
            float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
            float3 bitangent = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
            inputData.normalWS = TransformTangentToWorld(surfaceDescription.NormalTS, half3x3(input.tangentWS.xyz, bitangent, input.normalWS.xyz));
    #elif _NORMAL_DROPOFF_OS
            inputData.normalWS = TransformObjectToWorldNormal(surfaceDescription.NormalOS);
    #elif _NORMAL_DROPOFF_WS
            inputData.normalWS = surfaceDescription.NormalWS;
    #endif
    #else
    inputData.normalWS = input.normalWS;
    #endif
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.bakedGI = 1;
    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}


PackedVaryings vert(Attributes input)
{
    const Varyings output = BuildVaryings(input);
    const PackedVaryings packedOutput = PackVaryings(output);
    return packedOutput;
}

half4 frag(PackedVaryings packedInput) : SV_TARGET
{
    const Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

    const SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    #if _AlphaClip
        half alpha = surfaceDescription.Alpha;
        clip(alpha - surfaceDescription.AlphaClipThreshold);
    #elif _SURFACE_TYPE_TRANSPARENT
        half alpha = surfaceDescription.Alpha;
    #else
    half alpha = 1;
    #endif

    #ifdef _ALPHAPREMULTIPLY_ON
        surfaceDescription.BaseColor *= surfaceDescription.Alpha;
    #endif

    ToonInputData inputData;
    BuildInputData(unpacked, surfaceDescription, inputData);

    ToonSurfaceData surface = (ToonSurfaceData)0;
    surface.baseColor = surfaceDescription.BaseColor;
    surface.shadowColor = surfaceDescription.ShadowColor;
    surface.specular = surfaceDescription.Specular;
    surface.occlusion = surfaceDescription.Occlusion;
    surface.emission = surfaceDescription.Emission;
    surface.alpha = saturate(alpha);
    surface.numLightLevels = surfaceDescription.NumLightLevels;
    surface.terminatorBias = clamp(surfaceDescription.TerminatorBias, -1, 1);

    half4 color = Toon(inputData, surface);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}
