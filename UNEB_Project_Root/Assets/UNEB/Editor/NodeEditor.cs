
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
        /// The associated graph to visualize and edit.
        /// </summary>
        public NodeGraph graph;
        private NodeEditorWindow _window;

        private Texture2D _gridTex;
        private Texture2D _knobTex;
        private Texture2D _backTex;

        public Color backColor;
        private Color _knobColor;

        // To keep track of zooming.
        private Vector2 _zoomAdjustment;

        public static float zoomDelta = 0.1f;
        public static float minZoom = 1f;
        public static float maxZoom = 4f;
        public static float panSpeed = 1.2f;

        public Vector2 panOffset = Vector2.zero;
        public Vector2 zoom = Vector2.one;

        public NodeEditor(NodeEditorWindow w)
        {
            backColor = ColorExtensions.From255(59, 62, 74);
            _knobColor = ColorExtensions.From255(126, 186, 255);

            TextureLib.LoadStandardTextures();

            _gridTex = TextureLib.GetTexture("Grid");
            _backTex = TextureLib.GetTexture("Square");
            _knobTex = TextureLib.GetTintTex("Circle", _knobColor);

            _window = w;
        }

        #region Drawing

        public void Draw()
        {
            if (Event.current.type == EventType.Repaint) {
                drawGrid();
            }

            if (graph)
                drawGraphContents();
        }

        private void drawGraphContents()
        {
            Rect graphRect = _window.Size;
            var center = graphRect.size / 2f;

            _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, false);

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

        private void drawNodes()
        {
            foreach (Node node in graph.nodes) {
                drawNode(node);
                drawKnobs(node);
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
            screenRect.position = graphToScreenSpace(screenRect.position);

            GUI.DrawTexture(screenRect, _knobTex);
        }

        private void drawConnections()
        {
            foreach (Node node in graph.nodes) {

                foreach (var output in node.Outputs) {

                    foreach (NodeInput input in output.Inputs) {

                        Vector2 start = graphToScreenSpace(output.bodyRect.center);
                        Vector2 end = graphToScreenSpace(input.bodyRect.center);

                        DrawBezier(start, end, _knobColor);
                    }
                }
            }
        }

        private void drawConnectionPreview()
        {
            var output = _window.state.selectedOutput;

            if (output != null) {
                Vector2 start = graphToScreenSpace(output.bodyRect.center);
                DrawBezier(start, Event.current.mousePosition, Color.gray);
            }
        }

        private void drawNode(Node node)
        {
            // Convert the node rect from graph to screen space.
            Rect screenRect = node.bodyRect;
            screenRect.position = graphToScreenSpace(screenRect.position);

            // The node contents are grouped together within the node body.
            BeginGroup(screenRect, backgroundStyle, backColor);

            // Make the body of node local to the group coordinate space.
            Rect localRect = node.bodyRect;
            localRect.position = Vector2.zero;

            // Draw the contents inside the node body, automatically laidout.
            GUILayout.BeginArea(localRect, GUIStyle.none);

            node.OnNodeGUI();

            GUILayout.EndArea();
            GUI.EndGroup();
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
        public static void DrawLine(Vector2 start, Vector2 end)
        {
            Handles.DrawLine(start, end);
        }

        public static void BeginGroup(Rect r, GUIStyle style, Color color)
        {
            var old = GUI.color;

            GUI.color = color;
            GUI.BeginGroup(r, style);

            GUI.color = old;
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
            zoom *= scale;

            float cap = Mathf.Clamp(zoom.x, minZoom, maxZoom);
            zoom.Set(cap, cap);
        }

        public float ZoomScale
        {
            get { return zoom.x; }
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
        public Vector2 graphToScreenSpace(Vector2 graphPos)
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
            graphPos = graphToScreenSpace(graphPos) / ZoomScale;
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

        #endregion
    }
}