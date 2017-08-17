using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
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

        public T CreateNode<T>() where T : EditorNode, new()
        {
            T node = new T();
            nodes.Add(node);

            return node;
        }

        public EditorNode CreateNode(System.Type type)
        {
            if (typeof(EditorNode).IsAssignableFrom(type)) {

                var node = System.Activator.CreateInstance(type) as EditorNode;

                nodes.Add(node);
                return node;
            }

            else {
                Debug.LogError(type + " is not of type: " + typeof(EditorNode));
                return null;
            }
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
}