
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;

public class NodeEditor
{
    /// <summary>
    /// The associated canvas to visualize and edit.
    /// </summary>
    public NodeCanvas canvas;
    private NodeEditorWindow _window;

    private Texture2D _gridTex;
    private Texture2D _knobTex;
    private Texture2D _backTex;

    private Color _backColor;
    private Color _knobColor;

    // To keep track of zooming.
    private Vector2 _zoomAdjustment;

    public NodeEditor(NodeEditorWindow w)
    {
        _backColor = ColorExtensions.From255(49, 52, 64);
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

        drawCanvasContents();
    }

    private void drawCanvasContents()
    {
        Rect canvasRect = _window.Size;
        var center = canvasRect.size / 2f;

        _zoomAdjustment = GUIScaleUtility.BeginScale(ref canvasRect, center, canvas.ZoomScale, true, false);

        drawConnectionPreview();
        drawConnections();
        drawNodes();

        GUIScaleUtility.EndScale();
    }

    private void drawGrid()
    {
        var size = _window.Size.size;
        var center = size / 2f;

        float zoom = canvas.ZoomScale;

        // Offset from origin in tile units
        float xOffset = -(center.x * zoom + canvas.panOffset.x) / _gridTex.width;
        float yOffset = ((center.y - size.y) * zoom + canvas.panOffset.y) / _gridTex.height;

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
        foreach (EditorNode node in canvas.nodes) {
            drawNode(node);
            drawKnobs(node);
        }
    }

    private void drawKnobs(EditorNode node)
    {
        float left = node.bodyRect.xMin;
        float right = node.bodyRect.xMax;
        float top = node.bodyRect.yMin;

        // Draw the inputs.
        float yKnob = node.HeaderTop + EditorKnob.kMinHalfSize.y;

        foreach (var input in node.Inputs) {

            input.bodyRect.center = new Vector2(left, yKnob);
            drawKnob(input);

            yKnob += EditorNode.kKnobOffset + EditorKnob.kMinSize.y;
        }

        // Draw the outputs.
        yKnob = node.HeaderTop + EditorKnob.kMinHalfSize.y;

        foreach (var output in node.Outputs) {

            output.bodyRect.center = new Vector2(right, yKnob);
            drawKnob(output);

            yKnob += EditorNode.kKnobOffset + EditorKnob.kMinSize.y;
        }
    }

    private void drawKnob(EditorKnob knob)
    {
        // Convert the body rect from canvas to screen space.
        var screenRect = knob.bodyRect;
        screenRect.position = CanvasToScreenSpace(screenRect.position);

        GUI.DrawTexture(screenRect, _knobTex);
    }

    private void drawConnections()
    {
        foreach (EditorNode node in canvas.nodes) {

            foreach (var output in node.Outputs) {

                foreach (EditorInputKnob input in output.Inputs) {

                    Vector2 start = CanvasToScreenSpace(output.bodyRect.center);
                    Vector2 end = CanvasToScreenSpace(input.bodyRect.center);

                    DrawBezier(start, end, Color.white);
                }
            }
        }
    }

    private void drawConnectionPreview()
    {
        var output = _window.state.selectedOutput;

        if (output != null) {
            Vector2 start = CanvasToScreenSpace(output.bodyRect.center);
            DrawBezier(start, Event.current.mousePosition, Color.gray);
        }
    }

    private void drawNode(EditorNode node)
    {
        // Convert the node rect from canvas to screen space.
        Rect screenRect = node.bodyRect;
        screenRect.position = CanvasToScreenSpace(screenRect.position);

        // The node contents are grouped together within the node body.
        BeginGroup(screenRect, backgroundStyle, _backColor);
        
        // Make the body of node local to the group coordinate space.
        Rect localRect = node.bodyRect;
        localRect.position = Vector2.zero;

        // Draw the contents inside the node body, automatically laidout.
        GUILayout.BeginArea(localRect, GUIStyle.none);

        // Draw header
        GUILayout.Box(node.name, node.HeaderStyle);

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
        canvas.panOffset += delta * canvas.ZoomScale * NodeCanvas.panSpeed;
    }

    public void Zoom(float zoomDirection)
    {
        float scale = (zoomDirection < 0f) ? (1f - NodeCanvas.zoomDelta) : (1f + NodeCanvas.zoomDelta);
        canvas.zoom *= scale;

        float cap = Mathf.Clamp(canvas.zoom.x, NodeCanvas.minZoom, NodeCanvas.maxZoom);
        canvas.zoom.Set(cap, cap);
    }

    /// <summary>
    /// Convertes the screen position to canvas space.
    /// </summary>
    public Vector2 ScreenToCanvasSpace(Vector2 screenPos)
    {
        var canvasRect = _window.Size;
        var center = canvasRect.size / 2f;
        return (screenPos - center) * canvas.ZoomScale - canvas.panOffset;
    }

    /// <summary>
    /// Returns the mouse position in canvas space.
    /// </summary>
    /// <returns></returns>
    public Vector2 MousePosition()
    {
        return ScreenToCanvasSpace(Event.current.mousePosition);
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
    /// Converts the canvas position to screen space.
    /// This only works for geometry inside the GUIScaleUtility.BeginScale()
    /// </summary>
    /// <param name="canvasPos"></param>
    /// <returns></returns>
    public Vector2 CanvasToScreenSpace(Vector2 canvasPos)
    {
        return canvasPos + _zoomAdjustment + canvas.panOffset;
    }

    /// <summary>
    /// Converts the canvas position to screen space.
    /// This only works for geometry inside the GUIScaleUtility.BeginScale().
    /// </summary>
    /// <param name="canvasPos"></param>
    public void CanvasToScreenSpace(ref Vector2 canvasPos)
    {
        canvasPos += _zoomAdjustment + canvas.panOffset;
    }

    /// <summary>
    /// Converts the canvas position to screen space.
    /// This works for geometry NOT inside the GUIScaleUtility.BeginScale().
    /// </summary>
    /// <param name="canvasPos"></param>
    public void CanvasToScreenSpaceZoomAdj(ref Vector2 canvasPos)
    {
        canvasPos = CanvasToScreenSpace(canvasPos) / canvas.ZoomScale;
    }

    /// <summary>
    /// Executes the callback on the first node that is detected under the mouse.
    /// </summary>
    /// <param name="callback"></param>
    public bool OnMouseOverNode(Action<EditorNode> callback)
    {
        for (int i = canvas.nodes.Count - 1; i >= 0; --i) {

            EditorNode node = canvas.nodes[i];

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
    public bool OnMouseOverOutput(Action<EditorOutputKnob> callback)
    {
        foreach (var node in canvas.nodes) {

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
    public bool OnMouseOverInput(Action<EditorInputKnob> callback)
    {
        foreach (var node in canvas.nodes) {

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
    public bool OnMouseOverNode_OrInput(Action<EditorNode> callback)
    {
        foreach (var node in canvas.nodes) {

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