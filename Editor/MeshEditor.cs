using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.UI;
using System.IO;

public class D : EditorTool
{
    public override void OnToolGUI(EditorWindow window)
    {

        // hook mouse input.
        int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(controlId);
        if(Event.current.type == EventType.DragPerform)
            HandleUtility.AddDefaultControl(controlId);
        HandleUtility.Repaint();
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




public class MeshEditor : EditorWindow
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
      
    }

    public void OnGUI()
    {
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
    void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        Repaint();
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
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
    private void OnSceneGUI(SceneView sceneView)
    {
        Mgr = ((GameObject)obj).GetComponent<TerrianManager>();
        Handles.color = Color.yellow;
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isMoveDown = true;
        }
        if (e.type == EventType.MouseUp && e.button == 0)
        {   
            isMoveDown = false;
        }
        if (e.isKey)
        {
       
            if (e.keyCode == KeyCode.G)
            {
                EditorTools.SetActiveTool(typeof(D));
              
            }
        };
        isDrag = (isMoveDown && e.type == EventType.MouseMove);
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (obj != null)
            {
                if (Mgr)
                {
                    Handles.SphereHandleCap(0, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity, 0.5f, EventType.Repaint);
                    hex = null;
                    if (TerrianManager._hexDic.TryGetValue(Hex.WorldSpaceToAxiaCoord(hit.point), out hex))
                    {

                        List<AxialHex> l = null;
                        switch (drowType)
                        {
                            case DrowType.Range:
                                l = hex.AxialHex.GetRang((ushort)(layer));

                                DrowHex(l);
                                break;
                            case DrowType.Ring:
                                l = hex.AxialHex.GetRing((ushort)(layer));

                                DrowHex(l);
                                break;
                            case DrowType.Line:
                                l = AxialHex.GetLine(_ZeroAxial, hex.AxialHex);
                                DrowHex(l);
                                break;
                            case DrowType.Path:
                                if (e.type == EventType.MouseDown && e.button == 0)
                                {
                                    Path = AxialHexFindPath.BFS_FindPath(_ZeroAxial, hex.AxialHex);
                                }
                                if (Path != null && Path.Count > 0)
                                    DrowHex(Path);
                                break;
                            default:
                                l = null;
                                break;
                        }

                    }
                    //if (e.type == EventType.MouseDown && e.button == 0)
                    //{
                    //    Mgr.Refresh();
                    //}
                }
            }
        }

        void DrowHex(List<AxialHex> l)
        {
            foreach (var axial in l)
            {
                hex = null;
                if (TerrianManager._hexDic.TryGetValue(axial, out hex))
                {
                    DrawHex(hex);
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {

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
                    if (isMoveDown && e.isKey && e.keyCode == KeyCode.LeftControl)
                    {
                        TerrianManager._hexDic[axial].Y = OffsetY;
                        TerrianManager._hexDic[axial].InitPoints();
                        switch (drowType)
                        {
                            case DrowType.Range:
                                foreach (var item in l)
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
                                foreach (var item in l)
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
             
            }
            if (e.type == EventType.MouseDown && e.button == 0)
                Mgr.Refresh();
        }
        // 刷新界面，才能让球一直跟随
        sceneView.Repaint();
        HandleUtility.Repaint();
    }

}
