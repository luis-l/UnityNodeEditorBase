
using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;

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

        public NodeEditor editor;
        public NodeCanvas canvas;
        public ActionManager actions;
        public ActionTriggerSystem triggers;
        public NodeEditorState state;

        void OnEnable()
        {
            GUIScaleUtility.CheckInit();

            actions = new ActionManager(this);
            editor = new NodeEditor(this);
            canvas = new NodeCanvas();
            triggers = new ActionTriggerSystem(actions);
            state = new NodeEditorState();

            editor.canvas = canvas;

            // Make sure that changes from the undo system are immediately
            // updated in the window. If not, the undo changes will be
            // visually delayed.
            actions.OnUndo += Repaint;
            actions.OnRedo += Repaint;

            // NOTE: You can also serialize data members so the window remembers
            // what data to use when the window is re-opened or after an engine restart/reload.
            // For example you can have a Node Graph structure saved as a Scriptable Object asset,
            // and you can serialize a reference to it in the window class so next time the engine starts up/reloads
            // it is still opened in the window.
        }

        void OnDisable()
        {
            if (actions != null) {
                actions.OnUndo -= Repaint;
                actions.OnRedo -= Repaint;
            }
        }

        void OnGUI()
        {
            editor.Draw();
            drawToolbar();

            // Input and events should be processed after drawing.
            triggers.Update();
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

            // Make the toolbar extend all throughout the window extension.
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void createFileMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Create New"), false, () => { Debug.Log("Create New"); });
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Load"), false, openLoadFileWindow);
            menu.AddItem(new GUIContent("Save"), false, openSaveFileWindow);

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

            menu.AddItem(new GUIContent("Zoom In"), false, () => { editor.Zoom(-1); });
            menu.AddItem(new GUIContent("Zoom Out"), false, () => { editor.Zoom(1); });

            menu.DropDown(new Rect(105f, kToolbarHeight, 0f, 0f));
        }

        private void openLoadFileWindow()
        {
            EditorUtility.OpenFilePanel("Open Node Graph", "Assets/", "asset");
        }

        private void openSaveFileWindow()
        {
            EditorUtility.SaveFilePanelInProject("Save Node Graph", "New Graph", "asset", "Select a destination to save the graph.");
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

                rect.y += kToolbarHeight;
                rect.height -= kToolbarHeight;

                return rect;
            }
        }
    }
}