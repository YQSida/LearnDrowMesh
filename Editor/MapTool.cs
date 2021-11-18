using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;



[EditorTool("HexagonMap Tool")]
public class MapTool : EditorTool
{
    [SerializeField] Texture2D m_ToolIcon;
    GUIContent m_IconContent;
    [Shortcut("Tools/Custom", KeyCode.G)]
    public static void SetToolModeCustom()
    {
        EditorTools.SetActiveTool(typeof(MapTool));
    }
    HexEventSystem hs;
    private void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "HexagonMap Tool",
            tooltip = "HexagonMap Tool"
        };
        hs = new HexEventSystem();
    }

    public override void OnToolGUI(EditorWindow window)
    {

        // hook mouse input.
        int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        if (Event.current.type == EventType.Layout)
            HandleUtility.AddDefaultControl(controlId);
        hs.OnEvent(Event.current);
        HandleUtility.Repaint();
    }

}
interface IHexEnter
{
    void OnHexEnter(Hex hex);
}
interface IHexDrag
{
    void OnHexDrag(Hex hex);
}
interface IHexMouseDown
{
    void OnHexDown(Hex hex);
}

interface IHexShiftAndDrag
{
    void OnHexShiftAndDrag(Hex hex);
}
public class HexEventSystem
{

    private bool _isMoveDown = false;
    private bool _isLeftControlDrag = false;
    Event _e;
    Hex hex;
    public static List<EditorWindow> windos = new List<EditorWindow>();
    public void OnEvent(Event e)
    {
        _e = e;
        if (_e == null)
            return;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
    
        if (_e.type == EventType.MouseDown && _e.button == 0)
        {
            _isMoveDown = true;         
        }
        if (_isMoveDown && _e.isKey && _e.keyCode == KeyCode.LeftControl)
        {
            _isLeftControlDrag = true;
        }
        if (_e.type == EventType.MouseUp && _e.button == 0)
        {
            _isMoveDown = false;
            _isLeftControlDrag = false;

        }
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            hex = null;
            if (TerrianManager._hexDic.TryGetValue(Hex.WorldSpaceToAxiaCoord(hit.point), out hex))
            {
                for (int i = 0; i < windos.Count; i++)
                {
                    if (windos[i] is IHexEnter l)
                    {
                        l.OnHexEnter(hex);
                    }
                    if (_e.type == EventType.MouseDown && _e.button == 0)
                    {
                        if (windos[i] is IHexMouseDown s)
                        {
                            s.OnHexDown(hex);
                        }
                    }
                    if (_isMoveDown && _e.isKey && _e.keyCode == KeyCode.LeftControl)
                    {
                        if (windos[i] is IHexShiftAndDrag s)
                        {
                            s.OnHexShiftAndDrag(hex);
                        }
                    }
                }
            }
        }
    }
}
