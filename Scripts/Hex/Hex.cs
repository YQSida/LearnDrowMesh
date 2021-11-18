using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;
/// <summary>
/// 定义Hex中的一些常量
/// </summary>

public class Step
{
    public AxialHex step;
    public Step ParentStep=null;
    public Step(AxialHex _step, Step parent=null)
    {
        step = _step;
        ParentStep = parent;
    }
}



public partial class Hex
{
    public const bool _UseConst = false;
    public static float space = 0.2f;
    private const float _SQUR3 = 1.732050808f;     //根号3
    private const float _SIZE = 1f;                //Hex的边 外圆半径  
    private const float _W = 3.464101616f;         //Hex的宽 内院半径
    private const float _H = 2f;                   //Hex两个对角顶点的长度
    private const float _DisCenterW = 3.464101616f; //Hex 横向两个中心点间的距离
    private const float _DisCenterH = 1.5f;        //Hex 纵向两个中心点间的距离

    public static float Size => 1f;                   //通过公式计算时所用
    public static float W => _UseConst ? _W : Size * _SQUR3 * 2;
    public static float H => _UseConst ? _H : Size * 2;
    public static float DisCenterW => _UseConst ? _DisCenterW : W;
    public static float DisCenterH => _UseConst ? _DisCenterH : H * 3 / 4;

    public static Dictionary<string, Sprite> CatcheSprite = new Dictionary<string, Sprite>();
}

[Serializable]
public class MapData
{
 public  Hex[] Datas;

    public Dictionary<AxialHex, Hex> ToDic()
    {
        Dictionary<AxialHex, Hex> dic = new Dictionary<AxialHex, Hex>();
        foreach (var hex in Datas)
        {
            dic[hex.AxialHex] = hex;
        }
        return dic;
    }
    public MapData(Dictionary<AxialHex, Hex> map)
    {
        Datas = new Hex[map.Count];
        int i = 0;
        foreach (var item in map)
        {
            Datas[i] = item.Value;
            i++;
        }
    }
}
/// <summary>
/// 存储hex的各种转化
/// </summary>
[Serializable]
public partial class Hex
{
   
    public OddHex OddHex;
    public AxialHex AxialHex;
    public float Y { get { return Center.y; } set { Center.y = value; } }
    public Vector3 Center;
    public bool _Obstace = false;
    public bool Obstacle
    {
        get { return _Obstace; }
        set { if (value != _Obstace){ Dirty = true;_Obstace = value; }}
    }
    public bool Dirty=false;
    private Sprite _sp;
    public Sprite SP {
      get
        {

            if (sp_path != null && sp_path.Length > 0)
            {
                if (!CatcheSprite.ContainsKey(sp_path))
                {
                    CatcheSprite[sp_path] = AssetDatabase.LoadAssetAtPath<Sprite>(sp_path);
                }
                if (CatcheSprite[sp_path] == null)
                    Debug.LogError($"{sp_path}没有这个资源");
                return CatcheSprite[sp_path];
            }
            return null;
        }
    }
    public string sp_path;
    public string SP_Path
    {
        get { return sp_path; }
        set { if (sp_path != value){ Dirty = true; sp_path = value; } }
    }
    public GameObject Obj;
     
    public Color MCloor => Obstacle? Color.gray:Color.green;
    public Hex(AxialHex axialHex, float y = 0)
    {
        Init(axialHex, y);
    }
    public Hex(OddHex oddHex, float y = 0)
    {
        Init(oddHex.OddToAxial(), y);
    }
    private void Init(AxialHex axialHex, float y = 0)
    {
        AxialHex = axialHex;
        OddHex = axialHex.AxiaToOdd();
        Center = oddCoordToWorldSpace() ;
        Y = y;
        InitPoints();
    }

    public List<Vector3> CornerPonts = new List<Vector3>();

    public Vector3 AxialCoordToWorldSpace()
    {
        var x = (Size + space) * _SQUR3*(  AxialHex.Q +  0.5f * AxialHex.R);
        var z = (Size + space) * (3f / 2 * AxialHex.R);
        return new Vector3(x, 0, z);
    }
    public Vector3 oddCoordToWorldSpace()
    {
        var x = (Size + space) * _SQUR3 * (OddHex.Col +0.5f * (OddHex.Row & 1));
        var z = (Size + space) * 3f/ 2 * OddHex.Row;
        return new Vector3(x, 0, z);

    }

    /// <summary>
    /// 四舍五入获取一个六边地址
    /// </summary>
    /// <returns></returns>
    public static AxialHex RoundToAxial(float q, float r, float s)
    {
        var _Q = Mathf.RoundToInt(q);
        var _R = Mathf.RoundToInt(r);
        var _S = Mathf.RoundToInt(s);
        var q_diff = Mathf.Abs(_Q - q);
        var r_diff = Mathf.Abs(_R - r);
        var s_diff = Mathf.Abs(_S - s);
        if (q_diff > r_diff && q_diff > s_diff)
            _Q = 0 - _R - _S;
        else if (r_diff > s_diff)
            _R = 0 - _Q - _S;
        return new AxialHex(_Q, _R);
    }

    public static AxialHex WorldSpaceToAxiaCoord(Vector3 worldSpace)
    {
        var q = (_SQUR3 / 3 * worldSpace.x - 1f / 3 * worldSpace.z) / (Size + space);
        var r = (2f / 3 * worldSpace.z) / (Size + space);


        return RoundToAxial(q, r, 0 - q - r);
    }
    public void InitPoints()
    {
        CornerPonts.Clear();
        for (int i = 1; i <= 6; i++)
        {
            var angle = 60 * i - 30;
            var red = Mathf.Deg2Rad * angle;
            var new_x = Center.x + (Size) * Mathf.Cos(red);
            var new_z = Center.z + (Size) * Mathf.Sin(red);
            CornerPonts.Add(new Vector3(new_x, Y, new_z));
        }
    }
    public static Dictionary<AxialHex, bool> Fined=new Dictionary<AxialHex, bool>();
    public static List<Step> roots = new List<Step>();
    public  static List<AxialHex> FindTarGet(AxialHex self,  AxialHex target , Step step=null)
    {
        if (step == null)
        {
            Fined .Clear();
            roots.Clear();
            foreach (var item in TerrianManager._hexDic)
            {
                Fined.Add(item.Value.AxialHex, true);

            }
        }
        var  New_step = new Step(self, step);
        List<Step> l = new List<Step>();
        List<AxialHex> result = new List<AxialHex>();
        roots =new List<Step>() { New_step };
        while (roots.Count != 0)
        {
            l.Clear();
            foreach (var item in roots)
            {
                for (int i = 0; i <= 5; i++)
                {
                    var f = item.step.getNeighbor(i);

                    Hex hex;
                    if (TerrianManager._hexDic.TryGetValue(f, out hex) && hex.Obstacle == false )
                    {
                        if (f == target)
                        {
                            result.Clear();
                            result.Add(f);
                            New_step = item;
                            while (New_step.ParentStep != null)
                            {
                                result.Add(New_step.step);
                                New_step = New_step.ParentStep;
                            }
                            return result;
                        }
                      
                      
                       
                        l.Add(new Step(f, item));
                    }
                    Debug.LogError($"{f.Q},{f.R}");

                    Fined[f] = false;
                }
            }
            roots = Findchllds(l);
        }
       
        return null;
    }
    public static List<Step> Findchllds(List<Step> l)
    {
        roots.Clear();
        List<Step> m = new List<Step>();
        foreach (var item in l)
        {
            for (int i = 0; i <= 5; i++)
            {
                var f = item.step .getNeighbor(i);

                Hex hex;
                if (TerrianManager._hexDic.TryGetValue(f, out hex) && hex.Obstacle == false&&Fined[f])
                {
                   
                    roots.Add(new Step(f,item));
                }
                
            }
           
        }
        return roots;
    }
}

/// <summary>
/// 行列坐标
/// </summary>
[Serializable]
public partial struct OddHex
{
    public int Col;
    public int Row;
    public OddHex(int col, int row)
    {
        Col = col;
        Row = row;
    }
    public AxialHex OddToAxial()
    {
        var q = Col - (Row - (Row & 1)) / 2;
        var r = Row;
        return new AxialHex((int)q, r);
    }
    public OddHex getNeighbor(int dir)
    {
        return OddToAxial().getNeighbor(dir).AxiaToOdd();
    }
    public int Distance(OddHex a)
    {
        return OddToAxial().Distance(a.OddToAxial());
    }

}
/// <summary>
/// 轴坐标
/// </summary>
[Serializable]
public partial struct AxialHex
{
    public int Q;         //Hex Q 
    public int R;         //Hex R
    public int S => 0 - Q - R;
    public AxialHex(int q, int r)
    {
        Q = q;
        R = r;
    }
    public OddHex AxiaToOdd()
    {
        var col = Q + (R - (R &1)) / 2;
        var row = R;
        return new OddHex(col, row);
    }
    /// <summary>
    /// 获取两个六边形之间的类曼哈顿距离
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public int Distance(AxialHex a)
    {
        var subtract = this - a;
        return Mathf.Max(Mathf.Abs(subtract.Q), Mathf.Abs(subtract.R), Mathf.Abs(subtract.S));
    }
    /// <summary>
    /// 获取邻居
    /// </summary>
    public AxialHex getNeighbor(int dir)
    {
        return this + Dirs[round(dir)];
    }
    public int round(int dir)
    {
        var d = dir % 6;
        if (d < 0)
            d += 6;
        return d;
    }
    /// <summary>
    /// 获取两个cube之间的连线
    /// </summary>
    /// <returns></returns>
    public static List<AxialHex> GetLine(AxialHex a, AxialHex b)
    {
        List<AxialHex> l = new List<AxialHex>();
        var N = b.Distance(a);

        for (int i = 0; i <= N; i++)
        {
            var lerpQ = Lerp(a.Q, b.Q , 1f / N * i);
            var lerpR = Lerp(a.R, b.R , 1f / N * i);
            var LerpS = Lerp(a.S, b.S , 1f / N * i);
            l.Add(Hex.RoundToAxial(lerpQ, lerpR , LerpS));
        }
        return l;
    }

    /// <summary>
    /// 获取指定距离范围内的点
    /// </summary>
    /// <param name="N"></param>
    /// <returns></returns>
    public List<AxialHex> GetRang(ushort N)
    {
        List<AxialHex> l = new List<AxialHex>();
        for (int q = -N; q <= N; q++)
        {
            for (int r = Mathf.Max(-N, -q - N); r <= Mathf.Min(N, -q + N); r++)
            {
                l.Add(new AxialHex(q, r) + this);
            }
        }
        return l;
    }

    public List<AxialHex> GetRing(ushort N)
    {
        List<AxialHex> l = new List<AxialHex>();
        for (int q = -N; q <= N; q++)
        {
            for (int r = Mathf.Max(-N, -q - N); r <= Mathf.Min(N, -q + N); r++)
            {
                if(Mathf.Max(Mathf.Abs(q),Mathf.Abs(r),Mathf.Abs(-q-r))==N)
                 l.Add(new AxialHex(q, r) + this);
            }
        }
        return l;
    }
}
/// <summary>
/// 静态 扩展
/// </summary>

public partial struct OddHex
{
    public static int Distance(OddHex a, OddHex b)
    {
        return a.OddToAxial().Distance(b.OddToAxial());
    }

}
public partial struct AxialHex
{

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static AxialHex[] Dirs = { new AxialHex(1,0) , new AxialHex(0,1) , new AxialHex(-1,1),
                               new AxialHex(-1,0), new AxialHex(0,-1) , new AxialHex(+1 ,-1)
                             };
    public static int Distance(AxialHex a, AxialHex b)
    {
        return a.Distance(b);
    }

    public override int GetHashCode()
    {
        return Q.GetHashCode() ^ (R.GetHashCode() << 2);
    }
    public bool Equals(AxialHex a)
    {
        return a.Q == Q && a.R == R;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is AxialHex)) return false;
        return Equals((AxialHex)obj);
    }
    public static bool operator ==(AxialHex a, AxialHex b)
    {
        return a.Q == b.Q && a.R == b.R;
    }
    public static bool operator !=(AxialHex a, AxialHex b)
    {
        return a.Q != b.Q || a.R != b.R;
    }
    public static AxialHex operator +(AxialHex a, AxialHex b)
    {
        return new AxialHex(a.Q + b.Q, a.R + b.R);
    }
    public static AxialHex operator -(AxialHex a, AxialHex b)
    {
        return new AxialHex(a.Q - b.Q, a.R - b.R);
    }
    public static AxialHex operator -(AxialHex a)
    {
        return new AxialHex(-a.Q, -a.R);
    }
}