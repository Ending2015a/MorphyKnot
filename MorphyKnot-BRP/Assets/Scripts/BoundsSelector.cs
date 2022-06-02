using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsSelector : MonoBehaviour
{
    public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

    public Vector3 center { get=>bounds.center; set=>bounds.center=value; }
    public Vector3 extents { get=>bounds.extents; set=>bounds.extents=value; }
    public Vector3 max { get=>bounds.max; set=>bounds.max=value; }
    public Vector3 min { get=>bounds.min; set=>bounds.min=value; }
    public Vector3 size { get=>bounds.size; set=>bounds.size=value; }

    public Color gizmoColor = Color.yellow;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(center, size);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}