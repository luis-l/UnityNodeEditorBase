
using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;

public class NodeEditorWindow : EditorWindow
{
    [MenuItem("Window/Node Editor")]
    static void Init()
    {
        var w = EditorWindow.CreateInstance<NodeEditorWindow>();
        w.titleContent = new GUIContent("Node Editor");
        w.Show();
    }

    public float toolbarHeight = 20f;
    public float toolbarButtonWidth = 50f;

    public NodeEditor editor;
    public NodeCanvas canvas;
    public InputManager input;

    void OnEnable()
    {
        GUIScaleUtility.CheckInit();

        editor = new NodeEditor();
        canvas = new NodeCanvas();
        input = new InputManager();

        editor.canvas = canvas;
        editor.window = this;
        input.window = this;

        // NOTE: You can also serialize data members so the window remembers
        // what data to use when the window is re-opened after an engine restart/reload.
        // For example you can have a Node Graph structure saved as a Scriptable Object asset,
        // and you can serialize a reference to it in the window class so next time the engine starts up/reloads
        // it is still opened in the window.
    }

    void OnGUI()
    {
        editor.Draw();

        // Draw tool bar last so it renders on top of everything.
        DrawToolbar();

        input.ProcessInput(Event.current);
    }

    protected virtual void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal("Toolbar");

        if (DropdownButton("File", toolbarButtonWidth)) {
            CreateFileMenu();
        }

        if (DropdownButton("Edit", toolbarButtonWidth)) {

        }

        if (DropdownButton("View", toolbarButtonWidth)) {

        }

        // Make the toolbar extend all throughout the window extension.
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
    }

    protected virtual void CreateFileMenu()
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create New"), false, () => { Debug.Log("Create New"); });
        menu.AddItem(new GUIContent("Load"), false, () => { Debug.Log("Load"); });

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Save"), false, () => { Debug.Log("Save"); });
        menu.AddItem(new GUIContent("Save As"), false, () => { Debug.Log("Save As"); });

        menu.DropDown(new Rect(5f, toolbarHeight, 0f, 0f));
    }

    public bool DropdownButton(string name, float width)
    {
        return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
    }

    /// <summary>
    /// The size of the window.
    /// </summary>
    public Rect Size
    {
        get { return new Rect(Vector2.zero, position.size); }
    }

    /// <summary>
    /// The rect used to filter input.
    /// This is so the toolbar is not ignored by editor inputs.
    /// </summary>
    public Rect InputRect
    {
        get
        {
            var rect = Size;

            rect.y += toolbarHeight;
            rect.height -= toolbarHeight;

            return rect;
        }
    }
}
