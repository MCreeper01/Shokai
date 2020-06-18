// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_UTILS_INCLUDED_
#define _VLB_SHADER_UTILS_INCLUDED_

#include "ShaderMaths.cginc"

#if VLB_DEPTH_BLEND

// DECLARE _CameraDepthTexture
#ifdef VLB_SRP_API

CBUFFER_START(_PerCamera)
float4 _ScaledScreenParams; // only used by LWRP for render scale
CBUFFER_END

TEXTURE2D(_CameraDepthTexture);
//SAMPLER(sampler_CameraDepthTexture);
inline float VLB_SampleDepthTexture(float4 uv)
{
    float2 screenParams = VLB_GET_PROP(_CameraBufferSizeSRP) * (_ScaledScreenParams.x > 0 ? _ScaledScreenParams.xy : _ScreenParams.xy);
    uint2 pixelCoords = uint2( (uv.xy/uv.w) * screenParams );
    return LOAD_TEXTURE2D_LOD(_CameraDepthTexture, pixelCoords, 0).r;
}

#else // !VLB_SRP_API

#ifndef UNITY_DECLARE_DEPTH_TEXTURE // handle Unity pre 5.6.0
#define UNITY_DECLARE_DEPTH_TEXTURE(tex) sampler2D_float tex
#endif
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#define VLB_SampleDepthTexture(uv) (SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (uv)/(uv.w)))
//#define VLB_SampleDepthTexture(uv) (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(uv)))

#endif // VLB_SRP_API


#ifdef VLB_SRP_API
#define VLB_LinearEyeDepth(depth) LinearEyeDepth((depth), _ZBufferParams)
#include "ShaderSRP.cginc"
#else
#define VLB_LinearEyeDepth(depth) LinearEyeDepth(depth)
#endif // VLB_SRP_API

inline float SampleSceneZ_Eye(float4 uv)    { return VLB_LinearEyeDepth(VLB_SampleDepthTexture(uv)); }
//inline float SampleSceneZ_01(float4 uv)     { return Linear01Depth(VLB_SampleDepthTexture(uv)); }

#define VLB_CAMERA_NEAR_PLANE _ProjectionParams.y // https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html

inline float4 DepthFade_VS_ComputeProjPos(float4 vertex_in, float4 vertex_out)
{
    float4 projPos = ComputeScreenPos(vertex_out);
    projPos.z = -VLBObjectToViewPos(vertex_in).z; // = COMPUTE_EYEDEPTH
    return projPos;
}

inline float DepthFade_PS_BlendDistance(float4 projPos, float distance)
{
    float sceneZ = max(0, SampleSceneZ_Eye(projPos) - VLB_CAMERA_NEAR_PLANE);
    float partZ = max(0, projPos.z - VLB_CAMERA_NEAR_PLANE);
    return saturate((sceneZ - partZ) / distance);
}

inline float DepthFade_PS_BlendDistance(float4 projPos, float3 geomPosObjectSpace, float distance)
{
    float sceneZ = max(0, SampleSceneZ_Eye(projPos));
    float partZ = max(0, -VLBObjectToViewPos(geomPosObjectSpace).z);
    return saturate((sceneZ - partZ) / distance);
}

inline float DepthFade_PS_BlendDistance(float sceneZ, float3 geomPosObjectSpace, float distance)
{
    float partZ = max(0, -VLBObjectToViewPos(geomPosObjectSpace).z);
    return saturate((sceneZ - partZ) / distance);
}
#endif


#if VLB_NOISE_3D
uniform sampler3D _VLB_NoiseTex3D;
uniform float4 _VLB_NoiseGlobal;

float3 Noise3D_GetUVW(float3 posWorldSpace, float3 posLocalSpace)
{
    float4 noiseLocal = VLB_GET_PROP(_NoiseLocal);
    float4 noiseParam = VLB_GET_PROP(_NoiseParam);
    float3 velocity = lerp(noiseLocal.xyz, _VLB_NoiseGlobal.xyz, noiseParam.y);
    float scale = lerp(noiseLocal.w, _VLB_NoiseGlobal.w, noiseParam.z);

    float3 posRef = lerp(posWorldSpace, posLocalSpace, noiseParam.w); // 0 -> World Space ; 1 -> Local Space

	//return frac(posRef.xyz * scale + (_Time.y * velocity)); // frac doesn't give good results on VS
	return (posRef.xyz * scale + (_Time.y * velocity));
}

float Noise3D_GetFactorFromUVW(float3 uvw)
{
    float3 noiseParam = VLB_GET_PROP(_NoiseParam);
    float intensity = noiseParam.x;
	float noise = tex3D(_VLB_NoiseTex3D, uvw).a;
    return lerp(1, noise, intensity);
}
#endif // VLB_NOISE_3D


inline float ComputeAttenuation(float pixDistZ, float fallOffStart, float fallOffEnd, float lerpLinearQuad)
{
    // Attenuation
    float distFromSourceNormalized = invLerpClamped(fallOffStart, fallOffEnd, pixDistZ);

    // Almost simple linear attenuation between Fade Start and Fade End: Use smoothstep for a better fall to zero rendering
    float attLinear = smoothstep(0, 1, 1 - distFromSourceNormalized);

    // Unity's custom quadratic attenuation https://forum.unity.com/threads/light-attentuation-equation.16006/
    float attQuad = 1.0 / (1.0 + 25.0 * distFromSourceNormalized * distFromSourceNormalized);

    const float kAttQuadStartToFallToZero = 0.8;
    attQuad *= saturate(smoothstep(1.0, kAttQuadStartToFallToZero, distFromSourceNormalized)); // Near the light's range (fade end) we fade to 0 (because quadratic formula never falls to 0)

    return lerp(attLinear, attQuad, lerpLinearQuad);
}



#if VLB_COLOR_GRADIENT_MATRIX_HIGH || VLB_COLOR_GRADIENT_MATRIX_LOW
#if VLB_COLOR_GRADIENT_MATRIX_HIGH
#define FLOAT_PACKING_PRECISION 64
#else
#define FLOAT_PACKING_PRECISION 8
#endif
inline half4 UnpackToColor(float packedFloat)
{
    half4 color;

    color.a = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.b = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.g = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.r = packedFloat;

    return color / (FLOAT_PACKING_PRECISION - 1);
}

inline float GetAtMatrixIndex(float4x4 mat, uint idx) { return mat[idx % 4][floor(idx / 4)]; }

inline half4 DecodeGradient(float t, float4x4 colorMatrix)
{
#define kColorGradientMatrixSize 16
    float sampleIndexFloat = t * (kColorGradientMatrixSize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, kColorGradientMatrixSize - 2);
    half4 colorA = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 0));
    half4 colorB = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 1));
    return lerp(colorA, colorB, ratioPerSample);
}
#elif VLB_COLOR_GRADIENT_ARRAY
inline half4 DecodeGradient(float t, float4 colorArray[kColorGradientArraySize])
{
    uint arraySize = kColorGradientArraySize;
    float sampleIndexFloat = t * (arraySize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, arraySize - 2);
    half4 colorA = colorArray[sampleIndexInt + 0];
    half4 colorB = colorArray[sampleIndexInt + 1];
    return lerp(colorA, colorB, ratioPerSample);
}
#endif // VLB_COLOR_GRADIENT_*

#endif // _VLB_SHADER_UTILS_INCLUDED_
