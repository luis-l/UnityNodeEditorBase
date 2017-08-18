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
        public Vector2 panOffset = Vector2.zero;
        public Vector2 zoom = Vector2.one;

        public List<Node> nodes = new List<Node>();

        public T CreateNode<T>() where T : Node, new()
        {
            T node = new T();
            nodes.Add(node);

            return node;
        }

        public Node CreateNode(System.Type type)
        {
            if (typeof(Node).IsAssignableFrom(type)) {

                var node = System.Activator.CreateInstance(type) as Node;

                nodes.Add(node);
                return node;
            }

            else {
                Debug.LogError(type + " is not of type: " + typeof(Node));
                return null;
            }
        }

        public void Remove(Node node)
        {
            nodes.Remove(node);
        }

        /// <summary>
        /// Put the node at the end of the node list.
        /// </summary>
        /// <param name="node"></param>
        public void PushToEnd(Node node)
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