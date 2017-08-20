using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNEB
{
    /// <summary>
    /// Represents a graph of nodes.
    /// </summary>
    public class NodeGraph : ScriptableObject
    {
        [HideInInspector]
        public List<Node> nodes = new List<Node>();

        /// <summary>
        /// Add a node to the graph.
        /// It is recommended that the save manager adds the nodes.
        /// </summary>
        /// <param name="n"></param>
        public void AddNode(Node n)
        {
            nodes.Add(n);
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

        /// <summary>
        /// Gets called right before the graph is saved.
        /// Can be used to setup things before saving like sorting nodes.
        /// </summary>
        public virtual void OnSave() { }
    }
}