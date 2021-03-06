# MorphyKnot
This example shows a work around of accessing ComputeBuffer in Surface Shader (built-in) and Shader Graph (URP). 


## Install

1. Press :star2: *Star* :star2: on the top right corner.
2. Clone/Download this repository.
3. Open with UnityEditor.

This repo contains the following two variations:
* MorphyKnot-BRP: Built-in render pipeline
    * Requires Unity version > 2019
* MorphyKnot-URP: Universal render pipeline
    * Requires Unity version > 2021

## Demos

### Built-in Render Pipeline
![](https://github.com/Ending2015a/MorphyKnot/blob/master/assets/brp.gif)


### Universal Render Pipeline
![](https://github.com/Ending2015a/MorphyKnot/blob/master/assets/urp.gif)
![](https://github.com/Ending2015a/MorphyKnot/blob/master/assets/urp-morph.gif)

[Watch this on YouTube in 4K quality](https://youtu.be/VzoO9lDk2UU)

## Explain

### Built-in Render Pipeline


First, you add these lines into your surface shader to enable the customizatoin of vertex shader and instancing procedure. Adding the `multi_compile_instancing` generates two variants of the surface shader for instancing mode ON and OFF.
```
#pragma surface surf Standard fullforwardshadows addshadow vertex:vert
#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
#pragma editor_sync_compilation
#pragma multi_compile_instancing
#pragma target 5.0
```

Then, define your compute buffers for instancing ON and the default properties for instancing OFF. Note that you have to block all the codes that only available when GPU instancing is ON using macro `UNITY_*_INSTANCING_ENABLED`. Otherwise, Unity will throw some compiler errors. The `*` depends on what drwa calls you are using. (`DrawMeshInstancedProcedural` in this repo)
```hlsl
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
struct Particle
{
    float3 position;
    float4 color;
};
StructuredBuffer<Particle> _Particles;
float _Scale;
#else
float4 _Color;
#endif
// This will set the Object to World matrix for each particle instance
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
```

In your vertex shader, you can access the `instanceID` pre-defined in the [built-in structure type `appdata_full`](https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/CGIncludes/UnityCG.cginc#L82) in instancing ON block, and perform some additional pre-particle computations here. Then assign the results to a custom structure `Input`. This structure `Input` will be passed to Surface shader.
```hlsl
struct Input
{
    float4 color;
};
void vert(inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input, o);
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    o.color = _Particles[v.instanceID].color;
#else
    // fallback to this default color when instancing is off
    o.color = _Color;
#endif
}
```

Finally, assign the properties in your custom structure to surface output
```hlsl
void surf (Input IN, inout SurfaceOutputStandard o)
{
   o.Albedo = IN.color.rgb;
   o.Emission = IN.color.rgb * 0.3;
   o.Metallic = _Metallic;
   o.Smoothness = _Glossiness;
   o.Alpha = IN.color.a;
}
```

BTW, don't forget to turn on `Enable GPU Instancing` for your material.

### Universal Render Pipeline

In URP, we have to do these procedures in shader graph. First create the shader graph:

![](https://github.com/Ending2015a/MorphyKnot/blob/master/assets/graph.png)

* Note that the `Instance ID` node is only available for newer version (I don't know the exact version, but if your shader graph has no `Instance ID` node please upgrade the Unity.)
* Type the following string into `Inject` node to enable per-particle configuration
```hlsl
#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
#pragma editor_sync_compilation

Out = In;
```
* Create a custom node file `ParticleGetter.cginc` with the following contents. You can access the compute buffers here and pass them as the output of this node
```hlsl

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
    in float3 InPosition, in float4 InColor, in float InstanceID,
    out float3 Position, out float3 Albedo, out float Alpha)
{
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    float4 color = _Particles[(uint)InstanceID].color;
#else
    // Fallback to this default color
    float4 color = InColor;
#endif
    Position = InPosition;
    Albedo = color.rgb;
    Alpha = color.a;
}
```
