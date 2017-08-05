
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
    private Texture2D _nodeBaseTex;

    // To keep track of zooming.
    private Vector2 _zoomAdjustment;

    public NodeEditor(NodeEditorWindow w)
    {
        TextureLib.LoadStandardTextures();

        _gridTex = TextureLib.GetTexture("Grid");
        _nodeBaseTex = TextureLib.GetTexture("GrayGradient");
        _window = w;
    }

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
            drawKnobs(node);
            drawNode(node);
        }
    }

    private void drawKnobs(EditorNode node)
    {
        var bodyRect = node.bodyRect;
        var output = node.output;
        var input = node.input;

        if (input != null) {

            // Top / Down
            // float x = bodyRect.x + (bodyRect.width - input.bodyRect.width) / 2f;
            // float y = bodyRect.y - input.bodyRect.height;

            // Left / Right
            float x = bodyRect.x - input.bodyRect.width;
            float y = bodyRect.y + (bodyRect.height - input.bodyRect.height) / 2f;

            input.bodyRect.position = new Vector2(x, y);
            drawKnob(input);
        }

        if (output != null) {

            // Top / Down
            // float x = bodyRect.x + (bodyRect.width - output.bodyRect.width) / 2f;
            // float y = bodyRect.y + bodyRect.height;

            // Left / Right
            float x = bodyRect.x + bodyRect.width;
            float y = bodyRect.y + (bodyRect.height - input.bodyRect.height) / 2f;

            output.bodyRect.position = new Vector2(x, y);
            drawKnob(output);
        }
    }

    private void drawKnob(EditorKnob knob)
    {
        // Convert the body rect from canvas to screen space.
        var screenRect = knob.bodyRect;
        screenRect.position = CanvasToScreenSpace(screenRect.position);

        GUI.DrawTexture(screenRect, _nodeBaseTex);
    }

    private void drawConnections()
    {
        foreach (EditorNode node in canvas.nodes) {

            if (node.output != null) {
                foreach (EditorInputKnob input in node.output.Inputs) {
                    DrawLine(node.output.bodyRect.center, input.bodyRect.center);
                }
            }
        }
    }

    private void drawConnectionPreview()
    {
        var output = _window.state.selectedOutput;

        if (output != null) {
            DrawLine(output.bodyRect.center, MousePosition());
        }
    }

    /// <summary>
    /// Draws a line using canvas space coordinates.
    /// </summary>
    public void DrawLine(Vector2 start, Vector2 end)
    {
        CanvasToScreenSpace(ref start);
        CanvasToScreenSpace(ref end);

        Handles.DrawLine(start, end);
    }

    private void drawNode(EditorNode node)
    {
        // Convert the node rect from canvas to screen space.
        Rect screenRect = node.bodyRect;
        screenRect.position = CanvasToScreenSpace(screenRect.position);

        // The node contents are grouped together within the node body.
        GUI.BeginGroup(screenRect, backgroundStyle);

        // Make the body of node local to the group coordinate space.
        Rect localRect = node.bodyRect;
        localRect.position = Vector2.zero;

        // Draw the contents inside the node body, automatically laidout.
        GUILayout.BeginArea(localRect, GUIStyle.none);
        GUILayout.Box(node.IconNameContent, node.IconNameStyle);

        GUILayout.EndArea();
        GUI.EndGroup();
    }

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

            if (node.output == null) {
                continue;
            }

            if (IsUnderMouse(node.output.bodyRect)) {
                callback(node.output);
                return true;
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

            if (node.input == null) {
                continue;
            }

            if (IsUnderMouse(node.input.bodyRect)) {
                callback(node.input);
                return true;
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

            bool bCondition = IsUnderMouse(node.bodyRect) ||
                (node.input != null && IsUnderMouse(node.input.bodyRect));

            if (bCondition) {
                callback(node);
                return true;
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
                _backgroundStyle.normal.background = _nodeBaseTex;
            }

            return _backgroundStyle;
        }
    }

    #endregion
}