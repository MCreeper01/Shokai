// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_MATHS_INCLUDED_
#define _VLB_SHADER_MATHS_INCLUDED_

#if UNITY_VERSION < 540
    #define VLBMatrixWorldToObject  _World2Object
    #define VLBMatrixObjectToWorld  _Object2World
    #define VLBMatrixMV             UNITY_MATRIX_MV
    inline float4 VLBObjectToClipPos(in float3 pos) { return mul(UNITY_MATRIX_MVP, float4(pos, 1.0)); }
#else
    #if VLB_CUSTOM_INSTANCED_OBJECT_MATRICES
        #define VLBMatrixWorldToObject  UNITY_ACCESS_INSTANCED_PROP(Props, _WorldToLocalMatrix)
        #define VLBMatrixObjectToWorld  UNITY_ACCESS_INSTANCED_PROP(Props, _LocalToWorldMatrix)
        #define VLBMatrixMV             mul(unity_MatrixV, VLBMatrixObjectToWorld)
        inline float4 VLBObjectToClipPos(in float3 pos) { return mul(mul(unity_MatrixVP, VLBMatrixObjectToWorld), float4(pos, 1.0)); }
    #else
        #define VLBMatrixWorldToObject  unity_WorldToObject
        #define VLBMatrixObjectToWorld  unity_ObjectToWorld
        #define VLBMatrixMV             UNITY_MATRIX_MV
        #define VLBObjectToClipPos      UnityObjectToClipPos
    #endif
#endif

inline float3 VLBObjectToViewPos(in float3 pos)     { return mul(VLBMatrixMV, float4(pos, 1.0)).xyz; }
inline float3 VLBObjectToViewPos(   float4 pos)     { return VLBObjectToViewPos(pos.xyz); }

inline float3 VLBWorldToObjectPos(in float3 pos)    { return mul(VLBMatrixWorldToObject, float4(pos, 1.0)).xyz; }
//inline float3 VLBObjectToWorldPos(in float3 pos)    { return mul(VLBMatrixObjectToWorld, float4(pos, 1.0)).xyz; } // unused

inline float dot2(float2 v) { return dot(v,v); }
inline float lerpClamped(float a, float b, float t) { return lerp(a, b, saturate(t)); }
inline float invLerp(float a, float b, float t) { return (t - a) / (b - a); }
inline float invLerpClamped(float a, float b, float t) { return saturate(invLerp(a, b, t)); }
inline float fromABtoCD_Clamped(float valueAB, float A, float B, float C, float D)  { return lerpClamped(C, D, invLerpClamped(A, B, valueAB)); }

// Get signed distance of pos from the plane (normal ; d).
// Normal should be normalized.
// If we want to disable this feature, we could set normal and d to 0 (no discard in this case).
inline float DistanceToPlane(float3 pos, float3 normal, float d) { return dot(normal, pos) + d; }


inline float parabola( float x, float k )
{
    return pow( 4.0*x*(1.0-x), k );
}

// https://en.wikipedia.org/wiki/Smoothstep
inline float smootherstep(float edge0, float edge1, float x)
{
    // Scale, and clamp x to 0..1 range
    x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    // Evaluate polynomial
    return x*x*x*(x*(x * 6 - 15) + 10);
}

#endif
