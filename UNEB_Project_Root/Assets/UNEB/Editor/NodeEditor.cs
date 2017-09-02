
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;
using UNEB.Utility;

namespace UNEB
{
    public class NodeEditor
    {
        /// <summary>
        /// Callback for when a node is modified within the editor
        /// </summary>
        public static Action<NodeGraph, Node> onUpdateNode;

        /// <summary>
        /// The rect bounds defining the recticle at the grid center.
        /// </summary>
        public static readonly Rect kReticleRect = new Rect(0, 0, 8, 8);

        public static float zoomDelta = 0.1f;
        public static float minZoom = 1f;
        public static float maxZoom = 4f;
        public static float panSpeed = 1.2f;

        /// <summary>
        /// The associated graph to visualize and edit.
        /// </summary>
        public NodeGraph graph;
        private NodeEditorWindow _window;

        private Texture2D _gridTex;
        private Texture2D _backTex;
        private Texture2D _circleTex;
        private Texture2D _knobTex;
        private Texture2D _headerTex;

        public Color backColor;
        public Color knobColor;
        public Color guideColor;

        // To keep track of zooming.
        private Vector2 _zoomAdjustment;
        private Vector2 _zoom = Vector2.one;
        public Vector2 panOffset = Vector2.zero;

        /// <summary>
        /// Enables and disables drawing the guide to the grid center.
        /// </summary>
        public bool bDrawGuide = false;

        public NodeEditor(NodeEditorWindow w)
        {
            backColor = ColorExtensions.From255(59, 62, 74);
            knobColor = ColorExtensions.From255(126, 186, 255);
            
            guideColor = Color.gray;
            guideColor.a = 0.3f;

            _gridTex = TextureLib.GetTexture("Grid");
            _backTex = TextureLib.GetTexture("Square");
            _circleTex = TextureLib.GetTexture("Circle");

            _window = w;
        }

        #region Drawing

        public void Draw()
        {
            if (Event.current.type == EventType.Repaint) {
                drawGrid();
                updateTextures();
            }

            if (graph)
                drawGraphContents();

            drawMode();
        }

        private void drawGraphContents()
        {
            Rect graphRect = _window.Size;
            var center = graphRect.size / 2f;

            _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, false);

            drawGridOverlay();
            drawConnectionPreview();
            drawConnections();
            drawNodes();

            GUIScaleUtility.EndScale();
        }

        private void drawGrid()
        {
            var size = _window.Size.size;
            var center = size / 2f;

            float zoom = ZoomScale;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / _gridTex.width;
            float yOffset = ((center.y - size.y) * zoom + panOffset.y) / _gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(size.x * zoom) / _gridTex.width;
            float tileAmountY = Mathf.Round(size.y * zoom) / _gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(_window.Size, _gridTex, new Rect(tileOffset, tileAmount));
        }

        // Handles drawing things over the grid such as axes.
        private void drawGridOverlay()
        {
            drawAxes();
            drawGridCenter();

            if (bDrawGuide) {
                drawGuide();
                _window.Repaint();
            }
        }

        private void drawGridCenter()
        {
            var rect = kReticleRect;

            rect.size *= ZoomScale;
            rect.center = Vector2.zero;
            rect.position = GraphToScreenSpace(rect.position);
            
            DrawTintTexture(rect, _circleTex, Color.gray);
        }

        private void drawAxes()
        {
            // Draw axes. Make sure to scale based on zoom.
            Vector2 up = Vector2.up * _window.Size.height * ZoomScale;
            Vector2 right = Vector2.right * _window.Size.width * ZoomScale;
            Vector2 down = -up;
            Vector2 left = -right;

            // Make sure the axes follow the pan.
            up.y -= panOffset.y;
            down.y -= panOffset.y;
            right.x -= panOffset.x;
            left.x -= panOffset.x;

            up = GraphToScreenSpace(up);
            down = GraphToScreenSpace(down);
            right = GraphToScreenSpace(right);
            left = GraphToScreenSpace(left);

            DrawLine(right, left, Color.gray);
            DrawLine(up, down, Color.gray);
        }

        /// <summary>
        /// Shows where the center of the grid is.
        /// </summary>
        private void drawGuide()
        {
            Vector2 gridCenter = GraphToScreenSpace(Vector2.zero);
            DrawLine(gridCenter, Event.current.mousePosition, guideColor);
        }

        private void drawNodes()
        {
            // Calculate the viewing rect in graph space.
            var view = _window.Size;
            view.size *= ZoomScale;
            view.center = -panOffset;

            // Render nodes within the view space for performance.
            foreach (Node node in graph.nodes) {

                if (view.Overlaps(node.bodyRect)) {
                    drawNode(node);
                    drawKnobs(node);
                }
            }
        }

        private void drawKnobs(Node node)
        {
            foreach (var input in node.Inputs) {
                drawKnob(input);
            }

            foreach (var output in node.Outputs) {
                drawKnob(output);
            }
        }

        private void drawKnob(NodeConnection knob)
        {
            // Convert the body rect from graph to screen space.
            var screenRect = knob.bodyRect;
            screenRect.position = GraphToScreenSpace(screenRect.position);

            GUI.DrawTexture(screenRect, _knobTex);
        }

        private void drawConnections()
        {
            foreach (var node in graph.nodes) {
                foreach (var output in node.Outputs) {
                    foreach (var input in output.Inputs) {

                        Vector2 start = GraphToScreenSpace(output.bodyRect.center);
                        Vector2 end = GraphToScreenSpace(input.bodyRect.center);

                        DrawBezier(start, end, knobColor);
                    }
                }
            }
        }

        private void drawConnectionPreview()
        {
            var output = _window.state.selectedOutput;

            if (output != null) {
                Vector2 start = GraphToScreenSpace(output.bodyRect.center);
                DrawBezier(start, Event.current.mousePosition, Color.gray);
            }
        }

        private void drawNode(Node node)
        {
            // Convert the node rect from graph to screen space.
            Rect screenRect = node.bodyRect;
            screenRect.position = GraphToScreenSpace(screenRect.position);

            // The node contents are grouped together within the node body.
            BeginGroup(screenRect, backgroundStyle, backColor);

            // Make the body of node local to the group coordinate space.
            Rect localRect = node.bodyRect;
            localRect.position = Vector2.zero;

            // Draw the contents inside the node body, automatically laidout.
            GUILayout.BeginArea(localRect, GUIStyle.none);

            node.HeaderStyle.normal.background = _headerTex;

            EditorGUI.BeginChangeCheck();
            node.OnNodeGUI();
            if (EditorGUI.EndChangeCheck()) 
                if (onUpdateNode != null) onUpdateNode(graph, node);

            GUILayout.EndArea();
            GUI.EndGroup();
        }

        /// <summary>
        /// Draw the window mode in the background.
        /// </summary>
        public void drawMode()
        {
            if (!graph) {
                GUI.Label(_modeStatusRect, new GUIContent("No Graph Set"), ModeStatusStyle);
            }

            else if (_window.GetMode() == NodeEditorWindow.Mode.Edit) {
                GUI.Label(_modeStatusRect, new GUIContent("Edit"), ModeStatusStyle);
            }

            else {
                GUI.Label(_modeStatusRect, new GUIContent("View"), ModeStatusStyle);
            }
        }

        /// <summary>
        /// Draws a bezier between the two end points in screen space.
        /// </summary>
        public static void DrawBezier(Vector2 start, Vector2 end, Color color)
        {
            Vector2 endToStart = (end - start);
            float dirFactor = Mathf.Clamp(endToStart.magnitude, 20f, 80f);

            endToStart.Normalize();
            Vector2 project = Vector3.Project(endToStart, Vector3.right);

            Vector2 startTan = start + project * dirFactor;
            Vector2 endTan = end - project * dirFactor;

            UnityEditor.Handles.DrawBezier(start, end, startTan, endTan, color, null, 3f);
        }

        /// <summary>
        /// Draws a line between the two end points.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            var handleColor = Handles.color;
            Handles.color = color;

            Handles.DrawLine(start, end);
            Handles.color = handleColor;
        }

        /// <summary>
        /// Draws a GUI texture with a tint.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="c"></param>
        public static void DrawTintTexture(Rect r, Texture t, Color c)
        {
            var guiColor = GUI.color;
            GUI.color = c;

            GUI.DrawTexture(r, t);
            GUI.color = guiColor;
        }

        public static void BeginGroup(Rect r, GUIStyle style, Color color)
        {
            var old = GUI.color;

            GUI.color = color;
            GUI.BeginGroup(r, style);

            GUI.color = old;
        }

        // TODO: Call after exiting playmode.
        private void updateTextures()
        {
            _knobTex = TextureLib.GetTintTex("Circle", knobColor);
            _headerTex = TextureLib.GetTintTex("Square", ColorExtensions.From255(79, 82, 94));
        }

        #endregion

        #region View Operations

        public void ToggleDrawGuide()
        {
            bDrawGuide = !bDrawGuide;
        }

        public void HomeView()
        {
            if (!graph || graph.nodes.Count == 0) {
                panOffset = Vector2.zero;
                return;
            }

            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            foreach (var node in graph.nodes) {

                Rect r = node.bodyRect;

                if (r.xMin < xMin) {
                    xMin = r.xMin;
                }

                if (r.xMax > xMax) {
                    xMax = r.xMax;
                }

                if (r.yMin < yMin) {
                    yMin = r.yMin;
                }

                if (r.yMax > yMax) {
                    yMax = r.yMax;
                }
            }

            // Add some padding so nodes do not appear on the edge of the view.
            xMin -= Node.kDefaultSize.x;
            xMax += Node.kDefaultSize.x;
            yMin -= Node.kDefaultSize.y;
            yMax += Node.kDefaultSize.y;
            var nodesArea = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            // Center the pan in the bounding view.
            panOffset = -nodesArea.center;

            // Calculate the required zoom based on the ratio between the window view and node area rect.
            var winSize = _window.Size;
            float zoom = 1f;

            // Use the view width to determine zoom to fit the entire node area width.
            if (nodesArea.width > nodesArea.height) {

                float widthRatio = nodesArea.width / winSize.width;
                zoom = widthRatio;

                if (widthRatio < 1f) {
                    zoom = 1 / widthRatio;
                }
            }

            // Use the height to determine zoom.
            else {

                float heightRatio = nodesArea.height / winSize.height;
                zoom = heightRatio;

                if (heightRatio < 1f) {
                    zoom = 1 / heightRatio;
                }
            }

            ZoomScale = zoom;
        }

        #endregion

        #region Space Transformations and Mouse Utilities

        public void Pan(Vector2 delta)
        {
            panOffset += delta * ZoomScale * panSpeed;
        }

        public void Zoom(float zoomDirection)
        {
            float scale = (zoomDirection < 0f) ? (1f - zoomDelta) : (1f + zoomDelta);

            _zoom *= scale;

            float cap = Mathf.Clamp(_zoom.x, minZoom, maxZoom);
            _zoom.Set(cap, cap);
        }

        public float ZoomScale
        {
            get { return _zoom.x; }
            set
            {
                float z = Mathf.Clamp(value, minZoom, maxZoom);
                _zoom.Set(z, z);
            }
        }

        /// <summary>
        /// Convertes the screen position to graph space.
        /// </summary>
        public Vector2 ScreenToGraphSpace(Vector2 screenPos)
        {
            var graphRect = _window.Size;
            var center = graphRect.size / 2f;
            return (screenPos - center) * ZoomScale - panOffset;
        }

        /// <summary>
        /// Returns the mouse position in graph space.
        /// </summary>
        /// <returns></returns>
        public Vector2 MousePosition()
        {
            return ScreenToGraphSpace(Event.current.mousePosition);
        }

        /// <summary>
        /// Tests if the rect is under the mouse.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool IsUnderMouse(Rect r)
        {
            return r.Contains(MousePosition());
        }

        /// <summary>
        /// Converts the graph position to screen space.
        /// This only works for geometry inside the GUIScaleUtility.BeginScale()
        /// </summary>
        /// <param name="graphPos"></param>
        /// <returns></returns>
        public Vector2 GraphToScreenSpace(Vector2 graphPos)
        {
            return graphPos + _zoomAdjustment + panOffset;
        }

        /// <summary>
        /// Converts the graph position to screen space.
        /// This only works for geometry inside the GUIScaleUtility.BeginScale().
        /// </summary>
        /// <param name="graphPos"></param>
        public void graphToScreenSpace(ref Vector2 graphPos)
        {
            graphPos += _zoomAdjustment + panOffset;
        }

        /// <summary>
        /// Converts the graph position to screen space.
        /// This works for geometry NOT inside the GUIScaleUtility.BeginScale().
        /// </summary>
        /// <param name="graphPos"></param>
        public void graphToScreenSpaceZoomAdj(ref Vector2 graphPos)
        {
            graphPos = GraphToScreenSpace(graphPos) / ZoomScale;
        }

        /// <summary>
        /// Executes the callback on the first node that is detected under the mouse.
        /// </summary>
        /// <param name="callback"></param>
        public bool OnMouseOverNode(Action<Node> callback)
        {
            if (!graph) {
                return false;
            }

            for (int i = graph.nodes.Count - 1; i >= 0; --i) {

                Node node = graph.nodes[i];

                if (IsUnderMouse(node.bodyRect)) {
                    callback(node);
                    return true;
                }
            }

            // No node under mouse.
            return false;
        }

        /// <summary>
        /// Tests if the mouse is over an output.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool OnMouseOverOutput(Action<NodeOutput> callback)
        {
            if (!graph) {
                return false;
            }

            foreach (var node in graph.nodes) {

                foreach (var output in node.Outputs) {

                    if (IsUnderMouse(output.bodyRect)) {
                        callback(output);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tests if the mouse is over an input.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool OnMouseOverInput(Action<NodeInput> callback)
        {
            if (!graph) {
                return false;
            }

            foreach (var node in graph.nodes) {

                foreach (var input in node.Inputs) {

                    if (IsUnderMouse(input.bodyRect)) {
                        callback(input);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tests if the mouse is over the node or the input.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool OnMouseOverNode_OrInput(Action<Node> callback)
        {
            if (!graph) {
                return false;
            }

            foreach (var node in graph.nodes) {

                if (IsUnderMouse(node.bodyRect)) {
                    callback(node);
                    return true;
                }

                // Check inputs
                else {

                    foreach (var input in node.Inputs) {
                        if (IsUnderMouse(input.bodyRect)) {
                            callback(node);
                            return true;
                        }
                    }
                }
            }

            // No node under mouse.
            return false;
        }

        #endregion

        #region Styles

        private GUIStyle _backgroundStyle;
        private GUIStyle backgroundStyle
        {
            get
            {
                if (_backgroundStyle == null) {
                    _backgroundStyle = new GUIStyle(GUI.skin.box);
                    _backgroundStyle.normal.background = _backTex;
                }

                return _backgroundStyle;
            }
        }


        private static Rect _modeStatusRect = new Rect(20f, 20f, 250f, 150f);
        private static GUIStyle _modeStatusStyle;
        private static GUIStyle ModeStatusStyle
        {
            get
            {
                if (_modeStatusStyle == null) {
                    _modeStatusStyle = new GUIStyle();
                    _modeStatusStyle.fontSize = 36;
                    _modeStatusStyle.fontStyle = FontStyle.Bold;
                    _modeStatusStyle.normal.textColor = new Color(1f, 1f, 1f, 0.2f);
                }

                return _modeStatusStyle;
            }
        }

        #endregion
    }
}
