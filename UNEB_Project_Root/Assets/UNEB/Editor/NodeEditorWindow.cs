
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using NodeEditorFramework.Utilities;
using UNEB.Utility;

namespace UNEB
{
    public class NodeEditorWindow : EditorWindow
    {
        [MenuItem("Window/Node Editor")]
        static void Init()
        {
            var w = EditorWindow.CreateInstance<NodeEditorWindow>();
            w.titleContent = new GUIContent("Node Editor");
            w.Show();
        }

        public const float kToolbarHeight = 20f;
        public const float kToolbarButtonWidth = 50f;

        [SerializeField]
        public NodeGraph graph;
        
        public NodeEditor editor;
        public ActionManager actions;
        public ActionTriggerSystem triggers;
        public NodeEditorState state;
        private SaveManager _saveManager;

        public enum Mode { Edit, View };
        private Mode _mode = Mode.Edit;

        void OnEnable()
        {
            GUIScaleUtility.CheckInit();
            TextureLib.LoadStandardTextures();

            actions = new ActionManager(this);
            editor = new NodeEditor(this);
            triggers = new ActionTriggerSystem(actions);
            state = new NodeEditorState();

            _saveManager = new SaveManager(this);

            editor.graph = graph;

            // Make sure that changes from the undo system are immediately
            // updated in the window. If not, the undo changes will be
            // visually delayed.
            actions.OnUndo += Repaint;
            actions.OnRedo += Repaint;

            // Always start in edit mode.
            // The only way it can be in view mode is if the window is 
            // already opened and the user selects a some graph.
            _mode = Mode.Edit;

            editor.HomeView();
        }

        void OnDisable()
        {
            _saveManager.Cleanup();
        }

        void OnDestroy()
        {
            cleanup();
        }

        void OnGUI()
        {
            // Asset removed.
            if (!graph && !_saveManager.IsInNographState()) {
                _saveManager.InitState();
            }

            editor.Draw();
            drawToolbar();

            // This must go after draw calls or there can be 
            // GUI layout errors.
            triggers.Update();
        }

        public void SetGraph(NodeGraph g, Mode mode = Mode.Edit)
        {
            graph = g;
            editor.graph = g;

            // Reset Undo and Redo buffers.
            actions.Reset();

            _mode = mode;
        }

        private void cleanup()
        {
            if (actions != null) {
                actions.OnUndo -= Repaint;
                actions.OnRedo -= Repaint;
            }

            actions.Reset();
            _saveManager.Cleanup();
        }

        private void drawToolbar()
        {
            EditorGUILayout.BeginHorizontal("Toolbar");

            if (DropdownButton("File", kToolbarButtonWidth)) {
                createFileMenu();
            }

            if (DropdownButton("Edit", kToolbarButtonWidth)) {
                createEditMenu();
            }

            if (DropdownButton("View", kToolbarButtonWidth)) {
                createViewMenu();
            }

            if (DropdownButton("Settings", kToolbarButtonWidth + 10f)) {
                createSettingsMenu();
            }

            if (DropdownButton("Tools", kToolbarButtonWidth)) {
                createToolsMenu();
            }

            // Make the toolbar extend all throughout the window extension.
            GUILayout.FlexibleSpace();
            drawGraphName();
            
            EditorGUILayout.EndHorizontal();
        }

        private void drawGraphName()
        {
            string graphName = "None";
            if (graph != null) {
                graphName = graph.name;
            }

            GUILayout.Label(graphName);
        }

        private void createFileMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Create New"), false, _saveManager.RequestNew);
            menu.AddItem(new GUIContent("Load"), false, _saveManager.RequestLoad);

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Save"), false, _saveManager.RequestSave);
            menu.AddItem(new GUIContent("Save As"), false, _saveManager.RequestSaveAs);

            menu.DropDown(new Rect(5f, kToolbarHeight, 0f, 0f));
        }

        private void createEditMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Undo"), false, actions.UndoAction);
            menu.AddItem(new GUIContent("Redo"), false, actions.RedoAction);

            menu.DropDown(new Rect(55f, kToolbarHeight, 0f, 0f));
        }

        private void createViewMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Home"), false, editor.HomeView);
            menu.AddItem(new GUIContent("Zoom In"), false, () => { editor.Zoom(-1); });
            menu.AddItem(new GUIContent("Zoom Out"), false, () => { editor.Zoom(1); });

            menu.DropDown(new Rect(105f, kToolbarHeight, 0f, 0f));
        }

        private void createSettingsMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Show Guide"), editor.bDrawGuide, editor.ToggleDrawGuide);
            
            menu.DropDown(new Rect(155f, kToolbarHeight, 0f, 0f));
        }

        private void createToolsMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add Test Nodes"), false, addTestNodes);
            menu.AddItem(new GUIContent("Clear Nodes"), false, clearNodes);

            menu.DropDown(new Rect(215f, kToolbarHeight, 0f, 0f));
        }

        public bool DropdownButton(string name, float width)
        {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        private void addTestNodes()
        {
            if (graph) {

                for (int x = 0; x < 10; x++) {
                    for (int y = 0; y < 10; y++) {

                        var node = SaveManager.CreateNode<BasicNode>(graph);

                        float xpos = x * Node.kDefaultSize.x * 1.5f;
                        float ypos = y * Node.kDefaultSize.y * 1.5f;
                        node.bodyRect.position = new Vector2(xpos, ypos);
                    }
                }
            }
        }

        private void clearNodes()
        {
            if (graph) {

                foreach (var node in graph.nodes) {
                    ScriptableObject.DestroyImmediate(node, true);
                }

                actions.Reset();
                graph.nodes.Clear();
            }
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

                rect.y += kToolbarHeight;
                rect.height -= kToolbarHeight;

                return rect;
            }
        }

        public Mode GetMode()
        {
            return _mode;
        }

        /// <summary>
        /// Opens up the node editor window from asset selection.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(1)]
        private static bool OpenGraphAsset(int instanceID, int line)
        {
            var graphSelected = EditorUtility.InstanceIDToObject(instanceID) as NodeGraph;

            if (graphSelected != null) {

                NodeEditorWindow windowToUse = null;

                // Try to find an editor window without a graph...
                var windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
                foreach (var w in windows) {

                    // The canvas is already opened
                    if (w.graph == graphSelected) {
                        return false;
                    }

                    // Found a window with no active canvas.
                    if (w.graph == null) {
                        windowToUse = w;
                        break;
                    }
                }

                // No windows available...just make a new one.
                if (!windowToUse) {
                    windowToUse = EditorWindow.CreateInstance<NodeEditorWindow>();
                    windowToUse.titleContent = new GUIContent("Node Editor");
                    windowToUse.Show();
                }

                windowToUse.SetGraph(graphSelected);
                windowToUse._saveManager.InitState();
                windowToUse.Repaint();

                return true;
            }

            return false;
        }
    }
}