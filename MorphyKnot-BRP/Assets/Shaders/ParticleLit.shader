Shader "Custom/ParticleLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        #pragma multi_compile_instancing
        #pragma target 5.0

        struct Particle
        {
            float3 position;
            float4 color;
        };

        struct Input
        {
            float4 color;
        };

        half _Glossiness;
        half _Metallic;
        float _Scale;

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        StructuredBuffer<Particle> _Particles;
        #else
        float4 _Color;
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

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            o.color = _Particles[v.instanceID].color;
            #else
            // fallback to this default color
            // when instancing is off
            o.color = _Color;
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = IN.color.rgb;
            o.Emission = IN.color.rgb * 0.3;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
