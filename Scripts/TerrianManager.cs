using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// MESH学习  地板生成
/// </summary>
public class TerrianManager : MonoBehaviour
{

    public enum DrawType
    {
        Odd,
        Alxs,
    }

    private MeshFilter _MeshFilter;
    private MeshRenderer _MeshRenderer;
    private MeshCollider _MeshCollider;
    private List<Vector3> _Vectors = new List<Vector3>();                     // 顶点数据
    private List<int> _Indices = new List<int>();                             //三角形点数据
    private List<Color> _uvs = new List<Color>();
    //--------------------------------------------------------------------------------
    public static Dictionary<AxialHex, Hex> _hexDic = new Dictionary<AxialHex, Hex>();
    AxialHex center = new AxialHex(0, 0);
    //-------------------------------------------------------------------------------
    public int N;
    public float space;
    public float Height = 0;
    public float noiseParam = 0.1f;
    public float Offset = 0;
    public float Updatepinlv = 1f;
    public SpriteRenderer prefab;
    public string MapName;
    public DrawType M_DrawType = DrawType.Alxs;
    public OBJPool Pool;
    // Start is called before the first frame update
    [ContextMenu("DrawMeshInScent")]
    public void Start()
    {
        Hex.space = space;
        _MeshFilter = GetComponent<MeshFilter>();
        _MeshRenderer = GetComponent<MeshRenderer>();
        _MeshCollider = GetComponent<MeshCollider>();
        Generate();

    }
    public void ClearnObj()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Pool.RecycleObj(transform.GetChild(i).gameObject);
        }
    }
    private Mesh mesh;
    float lastUpdata;
    public void Refresh()
    {
        if (Time.time - lastUpdata > Updatepinlv)
        {
            Hex.space = space;
            _Vectors.Clear();
            _Indices.Clear();
            _uvs.Clear();
            foreach (var item in _hexDic)
            {
                GenerateOneHexinds();

                _Vectors.AddRange(item.Value.CornerPonts);
                for (int i = 0; i < item.Value.CornerPonts.Count; i++)
                {
                    _uvs.Add(item.Value.MCloor);
                }
                if (item.Value.Dirty)
                {
                    if (item.Value.Obj)
                        Pool.RecycleObj(item.Value.Obj);
                    if (item.Value.SP != null)
                    {


                        //这里可以使用对象池提升性能
                        var obj = Pool.GetObj(item.Value.Center + new Vector3(0, 0.01f, 0));
                        var sp = obj.GetComponent<SpriteRenderer>();
                        sp.sprite = item.Value.SP as Sprite;
                        item.Value.Obj = sp.gameObject;
                    }
                    else
                    {
                        Pool.RecycleObj(item.Value.Obj);
                    }
                }
            }
            /// <summary>
            /// 连接边
            /// </summary>
            GenerateRect();
            /// <summary>
            /// 连接角
            /// </summary>
            GeneratesanTriangle();
            if (mesh == null)
                mesh = new Mesh();                                    //把数据传递给Mesh,生成真正的网格
            mesh.vertices = _Vectors.ToArray();
            mesh.triangles = _Indices.ToArray();                  //mesh.uv=uvs.ToArray();
            mesh.colors = _uvs.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _MeshFilter.mesh = mesh;
            _MeshCollider.sharedMesh = mesh;                      //碰撞体专用的mesh,只负责物体的碰撞外形
            lastUpdata = Time.time;
        }
    }
    public void Generate()
    {
        ClearMeshData();
        AddMeshData();
        ClearnObj();
        Mesh mesh = new Mesh();                               //把数据传递给Mesh,生成真正的网格
        mesh.vertices = _Vectors.ToArray();
        mesh.triangles = _Indices.ToArray();                  //mesh.uv=uvs.ToArray();
        Debug.LogError(mesh.vertices.Length);
        Debug.LogError(mesh.colors.Length);
        mesh.colors = _uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        _MeshFilter.mesh = mesh;
        _MeshCollider.sharedMesh = mesh;                      //碰撞体专用的mesh,只负责物体的碰撞外形

    }

    void AddMeshData()
    {

        GenerateHexData();
        //GenerateRectData();


    }
    void ClearMeshData()
    {
        _Vectors.Clear();
        _Indices.Clear();
        _hexDic.Clear();
        _uvs.Clear();
    }

    void GenerateHexData()
    {
        ///创建出hex
        switch (M_DrawType)
        {
            case DrawType.Odd:
                oddDrow();
                break;
            case DrawType.Alxs:
                axilsDrow();
                break;
            default:
                break;
        }

        /// < summary >
        /// 连接边
        /// </ summary >
        GenerateRect();
        /// < summary >
        /// 连接角
        /// </ summary >
        GeneratesanTriangle();
    }
    void oddDrow()
    {
        for (int z = 0; z < N; z++)
        {
            for (int x = 0; x < N; x++)
            {
                var hex = new Hex(new OddHex(x, z), 0);
                GenerateOneHexinds();
                for (int i = 0; i < hex.CornerPonts.Count; i++)
                {
                    _uvs.Add(hex.MCloor);
                }
                _Vectors.AddRange(hex.CornerPonts);
                _hexDic.Add(hex.AxialHex, hex);
            }
        }
    }

    void axilsDrow()
    {
        foreach (var item in center.GetRang((ushort)N))
        {
            var hex = new Hex(item.AxiaToOdd(), 0);
            GenerateOneHexinds();
            for (int i = 0; i < hex.CornerPonts.Count; i++)
                _uvs.Add(hex.MCloor);
            _Vectors.AddRange(hex.CornerPonts);
            _hexDic.Add(hex.AxialHex, hex);
        }
    }


    int index = 0;
    void GenerateOneHexinds()
    {
        index = _Vectors.Count;
        _Indices.Add(index); _Indices.Add(index + 2); _Indices.Add(index + 1);
        _Indices.Add(index); _Indices.Add(index + 3); _Indices.Add(index + 2);
        _Indices.Add(index); _Indices.Add(index + 4); _Indices.Add(index + 3);
        _Indices.Add(index); _Indices.Add(index + 5); _Indices.Add(index + 4);

    }
    void GenerateRect()
    {
        foreach (var item in _hexDic)
        {
            var hex = item.Value;
            int _index = 0;
            for (int i = 0; i < 3; i++, _index--)
            {
                Hex dir;
                _hexDic.TryGetValue(hex.AxialHex.getNeighbor(_index), out dir);
                if (null != dir)
                {
                    var index = _Vectors.Count;
                    _Vectors.Add(hex.CornerPonts[hex.AxialHex.round(_index)]);
                    _Vectors.Add(hex.CornerPonts[hex.AxialHex.round(_index - 1)]);
                    _Vectors.Add(dir.CornerPonts[dir.AxialHex.round(_index - 3)]);
                    _Vectors.Add(dir.CornerPonts[dir.AxialHex.round(_index - 4)]);
                    for (int j = 0; j < 4; j++)
                    {
                        _uvs.Add(Color.white);
                    }
                    _Indices.Add(index); _Indices.Add(index + 2); _Indices.Add(index + 1);
                    _Indices.Add(index); _Indices.Add(index + 3); _Indices.Add(index + 2);
                }
            }
        }
    }
    void GeneratesanTriangle()
    {
        foreach (var item in _hexDic)
        {
            var hex = item.Value;

            for (int i = 0; i < 2; i++)
            {
                Hex dir1;
                if (!_hexDic.TryGetValue(hex.AxialHex.getNeighbor(i), out dir1))
                    continue;
                Hex dir2;
                if (!_hexDic.TryGetValue(hex.AxialHex.getNeighbor(i + 1), out dir2))
                    continue;
                var index = _Vectors.Count;
                _Vectors.Add(hex.CornerPonts[i]);
                _Vectors.Add(dir2.CornerPonts[(i + 4) % 6]);
                _Vectors.Add(dir1.CornerPonts[(i + 2) % 6]);
                for (int j = 0; j < 3; j++)
                {
                    _uvs.Add(Color.white);
                }
                _Indices.Add(index); _Indices.Add(index + 1); _Indices.Add(index + 2);
            }
        }
    }
}

