using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInstanceRender 
{
}


/// <summary>
/// 先做最简单的一个Mesh生成
/// </summary>
public class ObjectInstanceRender
{
    public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode;
    public Matrix4x4 matrix;
    public Material material;  //材质
    public Mesh mesh;
    public Transform transform;
    public bool receiveShadows;
    public List<Matrix4x4> matrixs = new List<Matrix4x4>();
    public ObjectInstanceRender(GameObject go)
    {
        mesh = go.GetComponent<MeshFilter>().sharedMesh;
        var r = go.GetComponent<MeshRenderer>();
        material = r.sharedMaterial;
        shadowCastingMode = r.shadowCastingMode;
        receiveShadows = r.receiveShadows;
    }
    public void set(Matrix4x4 matrix4x4)
    {
        matrixs.Add(matrix4x4);
    }
    public void Remove(Matrix4x4 matrix4x4)
    {
        matrixs.Remove(matrix4x4);
    }
    public void clear()
    {
        matrixs.Clear();
    }
}

