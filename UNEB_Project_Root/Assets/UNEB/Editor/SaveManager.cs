
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using Bonsai.Utility;

namespace UNEB
{
    /// <summary>
    /// Handles the saving and loading of tree assets.
    /// </summary>
    public class SaveManager
    {
        public enum SaveState { NoGraph, TempGraph, SavedGraph };

        // The FSM used to structure the logic control of saving and loading.
        private StateMachine<SaveState> _saveFSM;

        // The events that dictate the flow of the manager's FSM.
        private enum SaveOp { None, New, Load, Save, SaveAs };
        private SaveOp _saveOp = SaveOp.None;

        private NodeEditorWindow _window;

        private const string kRootUNEB = "UNEB";

        // Path that stores temporary graphs.
        private const string kTempGraphDirectory = "TempGraphsUNEB";
        private const string kTempFileName = "TempNodeGraphUNEB";

        public SaveManager(NodeEditorWindow w)
        {
            _window = w;

            _saveFSM = new StateMachine<SaveState>();

            var noGraph = new StateMachine<SaveState>.State(SaveState.NoGraph);
            var tempGraph = new StateMachine<SaveState>.State(SaveState.TempGraph);
            var savedGraph = new StateMachine<SaveState>.State(SaveState.SavedGraph);

            _saveFSM.AddState(noGraph);
            _saveFSM.AddState(tempGraph);
            _saveFSM.AddState(savedGraph);

            // Actions to take when starting out on a window with no graph.
            _saveFSM.AddTransition(noGraph, tempGraph, isNewRequested, createNewOnto_Window_WithTempOrEmpty);
            _saveFSM.AddTransition(noGraph, savedGraph, isLoadRequested, loadOnto_EmptyWindow);

            // Actions to take when the window has a temp graph.
            _saveFSM.AddTransition(tempGraph, tempGraph, isNewRequested, createNewOnto_Window_WithTempOrEmpty);
            _saveFSM.AddTransition(tempGraph, savedGraph, isSaveOrSaveAsRequested, saveTempAs);
            _saveFSM.AddTransition(tempGraph, savedGraph, isLoadRequested, loadOnto_Window_WithTempgraph);

            // Actions to take when the window has a valid graph (already saved).
            _saveFSM.AddTransition(savedGraph, savedGraph, isSaveRequested, save);
            _saveFSM.AddTransition(savedGraph, savedGraph, isSaveAsRequested, saveCloneAs);
            _saveFSM.AddTransition(savedGraph, savedGraph, isLoadRequested, loadOnto_Window_WithSavedgraph);
            _saveFSM.AddTransition(savedGraph, tempGraph, isNewRequested, createNewOnto_Window_WithSavedgraph);

            // Consume the save operation even after the transition is made.
            _saveFSM.OnStateChangedEvent += () => { _saveOp = SaveOp.None; };

            InitState();

            NodeConnection.OnConnectionCreated -= saveConnection;
            NodeConnection.OnConnectionCreated += saveConnection;
        }

        /// <summary>
        /// This hanldes setting up the proper state based on the window's graph.
        /// </summary>
        internal void InitState()
        {
            // If the window has a valid graph and editable.
            if (_window.graph != null && _window.GetMode() == NodeEditorWindow.Mode.Edit) {

                string path = getCurrentGraphPath();

                // If the graph is temp.
                if (path.Contains(kTempGraphDirectory)) {
                    SetState(SaveState.TempGraph);
                }

                // If the graph is saved (not a temp).
                else {
                    SetState(SaveState.SavedGraph);
                }
            }

            // Window is fresh, no graph yet set.
            else {
                SetState(SaveState.NoGraph);
            }
        }

        /// <summary>
        /// Get the path from open file dialog.
        /// </summary>
        /// <returns></returns>
        private string getGraphFilePath()
        {
            string path = EditorUtility.OpenFilePanel("Open Node Graph", "Assets/", "asset");

            // If the path is outside the project's asset folder.
            if (!path.Contains(Application.dataPath)) {

                // If the selection was not cancelled...
                if (!string.IsNullOrEmpty(path)) {
                    _window.ShowNotification(new GUIContent("Please select a Graph asset within the project's Asset folder."));
                    return null;
                }
            }

            return path;
        }

        /// <summary>
        /// Assumes that the path is already valid.
        /// </summary>
        /// <param name="path"></param>
        private void loadGraph(string path)
        {
            int assetIndex = path.IndexOf("/Assets/");
            path = path.Substring(assetIndex + 1);

            var graph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
            _window.SetGraph(graph);
        }

        /// <summary>
        /// Gets the file path to save the canavs at.
        /// </summary>
        /// <returns></returns>
        private string getSaveFilePath()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewNodeGraph", "asset", "Select a destination to save the graph.");

            if (string.IsNullOrEmpty(path)) {
                return "";
            }

            return path;
        }

        #region Save Operations

        /// <summary>
        /// Creates and adds a node to the graph.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="bt"></param>
        /// <returns></returns>
        public static Node CreateNode(Type t, NodeGraph g)
        {
            try {

                var node = ScriptableObject.CreateInstance(t) as Node;
                AssetDatabase.AddObjectToAsset(node, g);

                // Optional, set reference to graph: node.graph = g

                node.Init();
                g.Add(node);
                return node;
            }

            catch (Exception e) {
                throw new UnityException(e.Message);
            }
        }

        /// <summary>
        /// Creates and adds a node to the graph.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bt"></param>
        /// <returns></returns>
        public static Node CreateNode<T>(NodeGraph g) where T : Node
        {
            var node = ScriptableObject.CreateInstance<T>();
            AssetDatabase.AddObjectToAsset(node, g);

            // Optional, set reference to graph: node.graph = g

            node.Init();
            g.Add(node);
            return node;
        }

        /// <summary>
        /// Creates a graph asset and saves it.
        /// </summary>
        /// <param name="path">The full path including name and extension.</param>
        /// <returns></returns>
        public static NodeGraph CreateNodeGraph(string path)
        {
            // We create a graph asset in the data base in order to add node assets
            // under the graph. This way things are organized in the editor.
            //
            // The drawback is that we need to create a temp asset for the tree
            // and make sure it does not linger if the temp asset is discarded.
            //
            // This means that we need to have a persistent directoy to store temp
            // assets.

            var graph = ScriptableObject.CreateInstance<NodeGraph>();

            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return graph;
        }

        /// <summary>
        /// Creates a new temporary node graph.
        /// </summary>
        /// <returns></returns>
        private NodeGraph createNew()
        {
            string tempPath = getTempFilePath();

            if (!string.IsNullOrEmpty(tempPath)) {

                _window.ShowNotification(new GUIContent("New Graph Created"));
                return CreateNodeGraph(tempPath);
            }

            return null;
        }

        // Create a new temp graph on an empty window or with a temp graph.
        private bool createNewOnto_Window_WithTempOrEmpty()
        {
            _window.SetGraph(createNew());
            return true;
        }

        // Saves the current active graph then loads a new graph.
        private bool createNewOnto_Window_WithSavedgraph()
        {
            // Save the old graph to avoid loss.
            AssetDatabase.SaveAssets();

            _window.SetGraph(createNew());

            return true;
        }

        // Load a graph to a window that has no graph active.
        private bool loadOnto_EmptyWindow()
        {
            loadGraph(getGraphFilePath());
            return true;
        }

        // Load a graph to a window that has a temp graph active.
        private bool loadOnto_Window_WithTempgraph()
        {
            string path = getGraphFilePath();

            if (!string.IsNullOrEmpty(path)) {

                // Get rid of the temporary graph.
                AssetDatabase.DeleteAsset(getCurrentGraphPath());
                loadGraph(path);
                return true;
            }

            return false;
        }

        // Load a graph to a window that has a saved graph active.
        private bool loadOnto_Window_WithSavedgraph()
        {
            string path = getGraphFilePath();

            if (!string.IsNullOrEmpty(path)) {

                // Save the old graph.
                save();
                loadGraph(path);
                return true;
            }

            return false;
        }

        // Makes the temporary graph into a saved graph.
        private bool saveTempAs()
        {
            string newPath = getSaveFilePath();
            string currentPath = getCurrentGraphPath();

            //If asset exists on path, delete it first.
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(newPath) != null) {
                AssetDatabase.DeleteAsset(newPath);
            }

            string result = AssetDatabase.ValidateMoveAsset(currentPath, newPath);

            if (result.Length == 0) {
                AssetDatabase.MoveAsset(currentPath, newPath);
                save();
                return true;
            }

            else {
                Debug.LogError(result);
                return false;
            }
        }

        // Copies the current active graph to a new location.
        private bool saveCloneAs()
        {
            string newPath = getSaveFilePath();

            if (!string.IsNullOrEmpty(newPath)) {

                string currentPath = getCurrentGraphPath();

                AssetDatabase.CopyAsset(currentPath, newPath);
                AssetDatabase.SetMainObject(_window.graph, currentPath);

                save();
                return true;
            }

            return false;
        }

        // Saves the current graph (not a temp graph).
        private bool save()
        {
            _window.graph.OnSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _window.ShowNotification(new GUIContent("Graph Saved"));
            return true;
        }

        // Helper method for the NodeConnection.OnConnectionCreated callback.
        private void saveConnection(NodeConnection conn)
        {
            if (!AssetDatabase.Contains(conn)) {
                AssetDatabase.AddObjectToAsset(conn, _window.graph);
            }
        }

        #endregion

        /// <summary>
        /// Handles deleting temporary graph or saving valid graph.
        /// </summary>
        internal void Cleanup()
        {
            // Only save/delete things if we are in edit mode.
            if (_window.GetMode() != NodeEditorWindow.Mode.Edit) {
                return;
            }

            SaveState state = _saveFSM.CurrentState.Value;

            if (state == SaveState.TempGraph) {
                AssetDatabase.DeleteAsset(getCurrentGraphPath());
            }

            else if (state == SaveState.SavedGraph) {
                save();
            }
        }

        /*
         * These are conditions used the save FSM to know when to transition.
         *  */
        private bool isNewRequested() { return _saveOp == SaveOp.New; }
        private bool isLoadRequested() { return _saveOp == SaveOp.Load; }
        private bool isSaveRequested() { return _saveOp == SaveOp.Save; }
        private bool isSaveAsRequested() { return _saveOp == SaveOp.SaveAs; }
        private bool isSaveOrSaveAsRequested() { return isSaveAsRequested() || isSaveRequested(); }

        /*
         * These are the events that drive the save manager.
         * Whenever one of this is fired, the save operation is set 
         * and the save FSM updated.
         * */
        internal void RequestNew() { _saveOp = SaveOp.New; _saveFSM.Update(); }
        internal void RequestLoad() { _saveOp = SaveOp.Load; _saveFSM.Update(); }
        internal void RequestSave() { _saveOp = SaveOp.Save; _saveFSM.Update(); }
        internal void RequestSaveAs() { _saveOp = SaveOp.SaveAs; _saveFSM.Update(); }

        private string getTempFilePath()
        {
            string tempRoot = getTempDirPath();

            if (string.IsNullOrEmpty(tempRoot)) {
                return "";
            }

            string filename = kTempFileName + _window.GetInstanceID().ToString().Ext("asset");
            return tempRoot.Dir(filename);
        }

        internal void SetState(SaveState state)
        {
            _saveFSM.SetCurrentState(state);
        }

        internal bool IsInNographState()
        {
            return _saveFSM.CurrentState.Value == SaveState.NoGraph;
        }

        internal SaveState CurrentState()
        {
            return _saveFSM.CurrentState.Value;
        }

        private string getCurrentGraphPath()
        {
            return AssetDatabase.GetAssetPath(_window.graph);
        }

        private string getTempDirPath()
        {
            string[] dirs = Directory.GetDirectories(Application.dataPath, kTempGraphDirectory, SearchOption.AllDirectories);

            // Return first occurance containing targetFolderName.
            if (dirs.Length != 0) {
                return getTempPathRelativeToAssets(dirs[0]);
            }

            // Could not find anything. Make the folder
            string rootPath = getPathToRootUNEB();

            if (!string.IsNullOrEmpty(rootPath)) {
                var dirInfo = Directory.CreateDirectory(rootPath.Dir(kTempGraphDirectory));
                return getTempPathRelativeToAssets(dirInfo.FullName);
            }

            else {
                return "";
            }
        }

        private static string getPathToRootUNEB()
        {
            // Find the UNEB project root directory within the Unity project.
            var dirs = Directory.GetDirectories(Application.dataPath, kRootUNEB, SearchOption.AllDirectories);

            if (dirs.Length != 0) {
                return dirs[0];
            }
            else {
                Debug.LogError("Could not find project root: /" + kRootUNEB + '/');
                return "";
            }
        }

        // Assumes that the fullTempPath is valid.
        private static string getTempPathRelativeToAssets(string fullTempPath)
        {
            int index = fullTempPath.IndexOf("Assets");
            return fullTempPath.Substring(index);
        }
    }
}