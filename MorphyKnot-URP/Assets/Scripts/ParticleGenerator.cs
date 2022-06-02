using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices; // Marshal

struct MyParticle
{
    public Vector3 position;
    public Vector4 color;
};

public class ParticleGenerator : MonoBehaviour
{

    public Camera camera;
    public ComputeShader graphShader;
    public Material material;

    public BoundsSelector boundary;

    public ComputeBuffer particleBuffer {get; private set;}

    public Vector3 paletteA;
    public Vector3 paletteB;
    public Vector3 paletteC;
    public Vector3 paletteD;


    [SerializeField, Range(10, 1000)]
    public int resolutionX = 100;

    [SerializeField, Range(1, 1000)]
    public int resolutionY = 100;

    [SerializeField, Range(0.1f, 50.0f)]
    public float particleSize = 4.0f;

    const int maxResolution = 1000;

    private Mesh m_mesh;

    const int Threads = 8;
    static readonly int resolutionXId = Shader.PropertyToID("_ResolutionX");
    static readonly int resolutionYId = Shader.PropertyToID("_ResolutionY");
    static readonly int particleId = Shader.PropertyToID("_Particles");
    static readonly int timeId = Shader.PropertyToID("_Time");
    static readonly int scaleId = Shader.PropertyToID("_Scale");
    static readonly int minId = Shader.PropertyToID("_Min");
    static readonly int maxId = Shader.PropertyToID("_Max");
    static readonly int paletteAId = Shader.PropertyToID("_PaletteA");
    static readonly int paletteBId = Shader.PropertyToID("_PaletteB");
    static readonly int paletteCId = Shader.PropertyToID("_PaletteC");
    static readonly int paletteDId = Shader.PropertyToID("_PaletteD");

    private int m_csGraph;

    void Awake()
    {
        // Generate primitive sphere
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_mesh = Object.Instantiate(obj.GetComponent<MeshFilter>().mesh);
        Object.Destroy(obj);

        m_csGraph = graphShader.FindKernel("Graph");
    }

    // Start is called before the first frame update
    void Start()
    {
        camera.transform.LookAt(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateParticles();
        Render();
    }

    void OnEnable()
    {
        CreateParticles();
    }

    void OnDisable()
    {
        DestroyParticles();
    }

    void CreateParticles()
    {
        particleBuffer = new ComputeBuffer(maxResolution * maxResolution, Marshal.SizeOf(typeof(MyParticle)));
    }

    void DestroyParticles()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Release();
            particleBuffer = null;
        }
    }
    

    void UpdateParticles()
    {
        int kernel = m_csGraph;
        int groupx = Mathf.CeilToInt((float)resolutionX / (float)Threads);
        int groupy = Mathf.CeilToInt((float)resolutionY / (float)Threads);
        float scale = 0.002f;
        graphShader.SetInt(resolutionXId, resolutionX);
        graphShader.SetInt(resolutionYId, resolutionY);
        graphShader.SetFloat(scaleId, scale);
        graphShader.SetFloat(timeId, Time.time);
        graphShader.SetVector(minId, boundary.bounds.min);
        graphShader.SetVector(paletteAId, paletteA);
        graphShader.SetVector(paletteBId, paletteB);
        graphShader.SetVector(paletteCId, paletteC);
        graphShader.SetVector(paletteDId, paletteD);
        graphShader.SetBuffer(kernel, particleId, particleBuffer);
        // launch kernel: compute the new particle positions and colors
        graphShader.Dispatch(kernel, groupx, groupy, 1);
    }

    void Render()
    {
        float scale = particleSize / 500f;
        material.SetBuffer(particleId, particleBuffer);
        material.SetFloat(scaleId, scale);
        Graphics.DrawMeshInstancedProcedural(m_mesh, 0, material, boundary.bounds, resolutionX * resolutionY);
    }
}
