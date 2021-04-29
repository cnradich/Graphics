//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit / Render Pipeline / Generate Shader Includes ] instead
//

#ifndef TOON_CS_HLSL
#define TOON_CS_HLSL
//
// UnityEngine.Rendering.HighDefinition.Toon+SurfaceData:  static fields
//
#define DEBUGVIEW_TOON_SURFACEDATA_COLOR (300)
#define DEBUGVIEW_TOON_SURFACEDATA_NORMAL (301)
#define DEBUGVIEW_TOON_SURFACEDATA_NORMAL_VIEW_SPACE (302)

//
// UnityEngine.Rendering.HighDefinition.Toon+BSDFData:  static fields
//
#define DEBUGVIEW_TOON_BSDFDATA_COLOR (350)

// Generated from UnityEngine.Rendering.HighDefinition.Toon+SurfaceData
// PackingRules = Exact
struct SurfaceData
{
    float3 color;
    float3 normalWS;
};

// Generated from UnityEngine.Rendering.HighDefinition.Toon+BSDFData
// PackingRules = Exact
struct BSDFData
{
    float3 color;
};

//
// Debug functions
//
void GetGeneratedSurfaceDataDebug(uint paramId, SurfaceData surfacedata, inout float3 result, inout bool needLinearToSRGB)
{
    switch (paramId)
    {
        case DEBUGVIEW_TOON_SURFACEDATA_COLOR:
            result = surfacedata.color;
            needLinearToSRGB = true;
            break;
        case DEBUGVIEW_TOON_SURFACEDATA_NORMAL:
            result = surfacedata.normalWS * 0.5 + 0.5;
            break;
        case DEBUGVIEW_TOON_SURFACEDATA_NORMAL_VIEW_SPACE:
            result = surfacedata.normalWS * 0.5 + 0.5;
            break;
    }
}

//
// Debug functions
//
void GetGeneratedBSDFDataDebug(uint paramId, BSDFData bsdfdata, inout float3 result, inout bool needLinearToSRGB)
{
    switch (paramId)
    {
        case DEBUGVIEW_TOON_BSDFDATA_COLOR:
            result = bsdfdata.color;
            needLinearToSRGB = true;
            break;
    }
}


#endif
