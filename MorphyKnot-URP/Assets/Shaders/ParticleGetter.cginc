
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
struct Particle
{
    float3 position;
    float4 color;
};
StructuredBuffer<Particle> _Particles;
float _Scale;
#endif

void ConfigureProcedural()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float3 position = _Particles[unity_InstanceID].position;
    unity_ObjectToWorld = 0.0;
    unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
    unity_ObjectToWorld._m00_m11_m22 = _Scale;

    unity_WorldToObject = 0.0;
    unity_WorldToObject._m03_m13_m23_m33 = float4(-position, 1.0);
    unity_WorldToObject._m00_m11_m22 = 1.0/_Scale;
    #endif
}


void ParticleGetter_float(
    in float3 InPosition,
    in float4 InColor,
    in float InstanceID,
    out float3 Position,
    out float3 Albedo,
    out float Alpha)
{
    float4 color = InColor;
    
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    color = _Particles[(uint)InstanceID].color;
    #endif
    
    Position = InPosition;
    Albedo = color.rgb;
    Alpha = color.a;
}
