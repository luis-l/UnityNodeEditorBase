using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores state data such as current pan, zoom, and nodes.
/// </summary>
public class NodeCanvas
{
    public static float zoomDelta = 0.1f;
    public static float minZoom = 1f;
    public static float maxZoom = 4f;
    public static float panSpeed = 1.2f;

    public Vector2 panOffset = Vector2.zero;
    public Vector2 zoom = Vector2.one;

    public List<EditorNode> nodes = new List<EditorNode>();

    public EditorNode CreateBaseNode()
    {
        var icon = TextureLib.GetTexture("UnityLogo");
        var node = new EditorNode("Basic Node", icon, EditorNode.kDefaultSize);

        nodes.Add(node);
        return node;
    }

    public T CreateNode<T>() where T : EditorNode, new()
    {
        T node = new T();
        nodes.Add(node);

        return node;
    }

    public void Remove(EditorNode node)
    {
        nodes.Remove(node);
    }

    /// <summary>
    /// Put the node at the end of the node list.
    /// </summary>
    /// <param name="node"></param>
    public void PushToEnd(EditorNode node)
    {
        if (nodes.Remove(node)) {
            nodes.Add(node);
        }
    }

    public float ZoomScale
    {
        get { return zoom.x; }
    }
}
