using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;


public class HexEventWindow:EditorWindow
{
    protected virtual void OnEnable()
    {
        HexEventSystem.windos.Add(this);
    }
    protected virtual void OnDisable()
    {
        HexEventSystem.windos.Remove(this);
    }
}



public class LoadWindow : EditorWindow
{
    public string[] Maps;
    public int[] MapInt ;
    public string[] paths;
    public int Select = 0;
    public TerrianManager Mgr;
    public static void Open(TerrianManager mgr)
    {
       
       var window= GetWindow<LoadWindow>("LoadWindow");
        window.Mgr = mgr;
    }
    private void OnEnable()
    {
        initDir();     
    }
    private void OnDisable()
    {     
    }
    public void OnGUI()
    {
        Select = EditorGUILayout.IntPopup(Select, Maps, MapInt);
        if (GUILayout.Button("load"))
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(paths[Select]);
            Debug.LogError(text.text);
            TerrianManager._hexDic = JsonUtility.FromJson<MapData>(text.text).ToDic();
            Mgr.ClearnObj();
            Mgr.Refresh();
            Mgr.MapName = Maps[Select];
            Close();
        }
        for (int i = 0; i < paths.Length; i++)
        {
            if (i % 5 == 0)
                EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Maps[i]))
            {
                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(paths[i]);
                Debug.LogError(text.text);
                TerrianManager._hexDic = JsonUtility.FromJson<MapData>(text.text).ToDic();
                Mgr.ClearnObj();
                Mgr.Refresh();
                Mgr.MapName = Maps[i];
                Close();
            }
            if (i % 5 == 4)
                EditorGUILayout.EndHorizontal();
        }
    }

    void initDir()
    {
        Maps = Directory.GetFiles(Application.dataPath + "/MapDatas", "*.txt", SearchOption.TopDirectoryOnly);
        MapInt = new int[Maps.Length];
        paths = new string[Maps.Length];
        for (int i = 0; i < Maps.Length; i++)
        {
            Maps[i] = Maps[i].Replace(Application.dataPath+ "/MapDatas\\", "");
            paths[i] = $"Assets/MapDatas/{Maps[i]}";
            MapInt[i] = i;
        }
    }

}


public class CreateMapWindow:EditorWindow
{
    public TerrianManager Mgr;
    public int Side;
    public string name="new_Map";
    public static void Open(TerrianManager mgr)
    {
        var window = GetWindow<CreateMapWindow>("CreateMapWindow");
        window.Mgr = mgr;
    }
    public void OnGUI()
    {
        name = EditorGUILayout.TextField("name", name);
        Side = EditorGUILayout.IntSlider("Side", Side, 1, 20);
        if (GUILayout.Button("Create"))
        {
            Mgr.N = Side;
            Mgr.Generate();
            Mgr.MapName = $"{name}.txt";
            string str = JsonUtility.ToJson(new MapData(TerrianManager._hexDic));
            Debug.LogError(str);
            FileStream fileStream = new FileStream(Application.dataPath + $"/MapDatas/{Mgr.MapName}", FileMode.Create);
            StreamWriter sw = new StreamWriter(fileStream);
            sw.Write(str);
            sw.Close();
            fileStream.Close();
            Close();
        }
    }
}




public class MeshEditor : HexEventWindow, IHexMouseDown,IHexShiftAndDrag,IHexEnter
{
    [MenuItem("MyTool/MeshEditor")]
    public static void Open()
    {
        GetWindow<MeshEditor>("MeshEditor");
    }

    public enum DrowType
    {
        Range,
        Line,
        Path,
        Ring,
    }
    public enum DrowObjType
    {
        None,
        Normal,
        Obstacle,
        Obj,

    }
    private Object obj;
    public int layer;
    public int OffsetY;
    public DrowType drowType;
    public DrowObjType drowObjType;
    public TerrianManager Mgr;
    public Texture2D _Image;
     
    public Vector2 rect=Vector2.zero;
    public string ImagePath;
    public string[] ImagePaths;
  
    private void OnEnable()
    {
        isSpaceDown = false;
        isMoveDown = false;
        isDrag = false;
        ImagePath = "/Image/ColorTiles/";
        string[] files = Directory.GetFiles(Application.dataPath+ImagePath, "*.png", SearchOption.TopDirectoryOnly);
    
        ImagePaths = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            ImagePaths[i] = "Assets"+files[i].Replace(Application.dataPath, "");
        }
        HexEventSystem.windos.Add(this);
    }
    private void OnDisable()
    {
        HexEventSystem.windos.Remove(this);
    }

    public void OnGUI()
    {
        Mgr = ((GameObject)obj).GetComponent<TerrianManager>();
        Handles.color = Color.yellow;
        if (Tools.current != Tool.Custom)
            EditorGUILayout.HelpBox("Press \"G\" switch to HexagonMap Tool", MessageType.Warning);
        EditorGUILayout.BeginVertical();
        obj = EditorGUILayout.ObjectField(obj, typeof(Object), GUILayout.Width(200)) ;
        layer = EditorGUILayout.IntSlider("Layer",layer,0,8);
        OffsetY = EditorGUILayout.IntSlider("OffsetY",OffsetY,-5,10);
        drowObjType = (DrowObjType)EditorGUILayout.EnumPopup(
             "DrowObjType:",
             drowObjType);
        drowType = (DrowType)EditorGUILayout.EnumPopup(
             "Drow:",
             drowType);
     
        if (GUILayout.Button("Save"))
        {
            Mgr = ((GameObject)obj).GetComponent<TerrianManager>();
          
            string str=JsonUtility.ToJson(new MapData(TerrianManager._hexDic));
            Debug.LogError(str);
            FileStream fileStream = new FileStream(Application.dataPath + $"/MapDatas/{Mgr.MapName}", FileMode.Create);
            StreamWriter sw = new StreamWriter(fileStream);
            sw.Write(str);
            sw.Close();
            fileStream.Close();
        }
      
        if (GUILayout.Button("Load"))
        {
            Mgr = ((GameObject)obj).GetComponent<TerrianManager>();
            LoadWindow.Open(Mgr);
        }
        if (GUILayout.Button("Create"))
        {
            Mgr = ((GameObject)obj).GetComponent<TerrianManager>();
            CreateMapWindow.Open(Mgr);
        }
        //-------------------------------------------------------ScrollView
        rect = EditorGUILayout.BeginScrollView(rect);
        for (int i = 0; i < ImagePaths.Length; i++)
        {
            if (i% 5 == 0)
                EditorGUILayout.BeginHorizontal();
            
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ImagePaths[i]);
            if (GUILayout.Button(texture, GUILayout.Width(100), GUILayout.Height(100)))
            {
                Debug.LogError("选择了这个分支");
              
             
                _Image = texture;
                ImagePath = ImagePaths[i];
            }
            if (i % 5 == 4)
                EditorGUILayout.EndHorizontal();
        }      
        EditorGUILayout.EndScrollView();
        ///-------------------------------------------------------EndScrollView
        EditorGUILayout.EndVertical();
      
    }

    Hex hex;
    AxialHex _ZeroAxial = new AxialHex(0, 0);

    private bool isSpaceDown = false;
    private bool isMoveDown = false;
    private bool isDrag = false;
    void DrawHex(Hex hex)
    {
        Handles.color = Color.yellow;
        Handles.DrawPolyLine(hex.CornerPonts[0], hex.CornerPonts[1], hex.CornerPonts[2],
                            hex.CornerPonts[3], hex.CornerPonts[4], hex.CornerPonts[5],hex.CornerPonts[0]); ;
    }
    List<AxialHex> Path=new List<AxialHex>();
    long time;
   

    List<AxialHex> GetDrawAxiaHexL()
    {
        List<AxialHex> l = null;
        switch (drowType)
        {
            case DrowType.Range:
                l = hex.AxialHex.GetRang((ushort)(layer));
        
                break;
            case DrowType.Ring:
                l = hex.AxialHex.GetRing((ushort)(layer));    ;
                break;
            case DrowType.Line:
                l = AxialHex.GetLine(_ZeroAxial, hex.AxialHex);         
                break;
            case DrowType.Path:
                l = AxialHexFindPath.BFS_FindPath(_ZeroAxial, hex.AxialHex);
                break;
            default:
                l = null;
                break;
        }
        return l;
    }

    public void OnHexDown(Hex hex)
    {
        this.hex = hex;
        DrowHex(GetDrawAxiaHexL());
        void DrowHex(List<AxialHex> _l)
        {
            if (_l == null || _l.Count <= 0)
                return;
            foreach (var axial in _l)
            {
                hex = null;
                if (TerrianManager._hexDic.TryGetValue(axial, out hex))
                {
                    DrawHex(hex);
            
                        TerrianManager._hexDic[axial].Y += OffsetY;
                        TerrianManager._hexDic[axial].InitPoints();
                        switch (drowObjType)
                        {
                            case DrowObjType.None:
                                TerrianManager._hexDic[axial].Obstacle = false;
                                break;
                            case DrowObjType.Normal:
                                TerrianManager._hexDic[axial].Obstacle = false;
                                TerrianManager._hexDic[axial].SP_Path = null;
                                break;
                            case DrowObjType.Obstacle:
                                TerrianManager._hexDic[axial].Obstacle = false;
                                break;
                            case DrowObjType.Obj:
                                TerrianManager._hexDic[axial].SP_Path = ImagePath;
                                break;
                            default:
                                break;
                        }     
                    
                }
            }
         Mgr.Refresh();
        }
    }

    public void OnHexShiftAndDrag(Hex hex)
    {
        this.hex = hex;
        DrowHex(GetDrawAxiaHexL());
        void DrowHex(List<AxialHex> _l)
        {
            if (_l == null || _l.Count <= 0)
                return;
            foreach (var axial in _l)
            {
                hex = null;
                if (TerrianManager._hexDic.TryGetValue(axial, out hex))
                {
                    DrawHex(hex);
                        TerrianManager._hexDic[axial].Y = OffsetY;
                        TerrianManager._hexDic[axial].InitPoints();
                        switch (drowType)
                        {
                            case DrowType.Range:
                                foreach (var item in _l)
                                {
                                    if (TerrianManager._hexDic.ContainsKey(item))
                                    {
                                        switch (drowObjType)
                                        {
                                            case DrowObjType.None:
                                                TerrianManager._hexDic[item].Obstacle = false;
                                                break;
                                            case DrowObjType.Normal:
                                                TerrianManager._hexDic[item].Obstacle = false;
                                                TerrianManager._hexDic[axial].SP_Path = null;
                                                break;
                                            case DrowObjType.Obstacle:
                                                TerrianManager._hexDic[item].Obstacle = true;
                                                break;
                                            case DrowObjType.Obj:
                                                TerrianManager._hexDic[item].SP_Path = ImagePath;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                break;
                            case DrowType.Line:
                                break;
                            case DrowType.Path:
                                break;
                            case DrowType.Ring:
                                foreach (var item in _l)
                                {
                                    if (TerrianManager._hexDic.ContainsKey(item))
                                        switch (drowObjType)
                                        {
                                            case DrowObjType.None:
                                                break;
                                            case DrowObjType.Normal:
                                                TerrianManager._hexDic[item].Obstacle = false;
                                                TerrianManager._hexDic[axial].SP_Path = null;
                                                break;
                                            case DrowObjType.Obstacle:
                                                TerrianManager._hexDic[item].Obstacle = true;
                                                break;
                                            case DrowObjType.Obj:
                                                TerrianManager._hexDic[item].SP_Path = ImagePath;
                                                break;
                                            default:
                                                break;
                                        }
                                }
                                break;
                            default:
                                break;
                        }
                        Mgr.Refresh();                 
                }
            }
            Mgr.Refresh();
        }
    }

    public void OnHexEnter(Hex hex)
    {
        this.hex = hex;
        DrowHex(GetDrawAxiaHexL());
        void DrowHex(List<AxialHex> _l)
        {
            if (_l == null || _l.Count <= 0)
                return;
            foreach (var axial in _l)
            {
                hex = null;
                if (TerrianManager._hexDic.TryGetValue(axial, out hex))
                {
                    DrawHex(hex);
                }
            }    
        }
    }
}
