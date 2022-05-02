using Faeric.HighPerformanceDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// A root node is automatically created. The root node should not be removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LtTree<T>
        where T : struct
    {
        public const short EMPTY_REF = -1;

        /// <summary>May contain holes. If you want to iterate this, check each node to see if its a hole.</summary>
        public LtNode<T>[] Nodes;
        FastList<short> _holes;

        public short NodeCount => (short)(Nodes.Length - _holes.Count);

        T _defaultValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="defaultValue">The initial array will be filled with this value. If the array capacity changes, new empty values will also be filled with this value.</param>
        public LtTree(int capacity = 8, T defaultValue = default)
        {
            _defaultValue = defaultValue;
            Nodes = new LtNode<T>[capacity];
            _holes = new FastList<short>(capacity);

            Clear();

            Console.WriteLine("halt here");
        }

        /// <summary>
        /// Clears all nodes except the root node. The root node is reset to default value. Node Count will be 1 after invoking Clear().
        /// </summary>
        public void Clear()
        {
            //Iterate in reverse order so that the last elements will be the first holes.
            //Don't invoke Clear on the root
            //Stop early before 0 because we don't want to add a hole for the root index.
            for (short i = (short)(Nodes.Length - 1); i > 0; i--)
                FreeNode(i);

            ResetRootNode();
        }


        /// <summary>
        /// Inserts a child for the given parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateChild(short parent)
        {
            short i = CheckoutNextAvailableSlot();
            Nodes[parent].Child = i;
            Nodes[i].Principal = parent;
            return i;
        }

        /// <summary>
        /// Inserts an OverlaySibling for the given node.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateOverlaySibling(short parent)
        {
            short i = CheckoutNextAvailableSlot();
            Nodes[parent].OverlaySibling = i;
            Nodes[i].Principal = parent;
            return i;
        }

        /// <summary>
        /// Inserts a stack sibling.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns>Index of the new child</returns>
        public short CreateStackSibling(short principal)
        {
            short i = CheckoutNextAvailableSlot();
            Nodes[principal].StackSibling = i;
            Nodes[i].Principal = principal;
            return i;
        }

        /// <summary>
        /// Removes the node and all descendents. The next sibling links are updated to maintain integrity.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNodeButKeepSiblings(short node, short parent)
        {
            //The node requested for removal may not be the first sibling.
            //So we get the first sibling by way of the parent.
            short i = Nodes[parent].Child;

            while (Nodes[i].OverlaySibling != EMPTY_REF)
            {
                if (Nodes[i].OverlaySibling == node)
                    Nodes[i].OverlaySibling = Nodes[node].OverlaySibling;

                i++;
            }

            i = Nodes[parent].Child;
            while (Nodes[i].StackSibling != EMPTY_REF)
            {
                if (Nodes[i].StackSibling == node)
                    Nodes[i].StackSibling = Nodes[node].StackSibling;

                i++;
            }

            RemoveChildIfPresent(node);
            FreeNode(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChildIfPresent(short parent)
        {
            if (Nodes[parent].Child != EMPTY_REF)
                RemoveChild(parent);
        }

        public void RemoveChild(short parent)
        {
            short child = Nodes[parent].Child;
            Nodes[parent].Child = EMPTY_REF;

            if (Nodes[child].Child != EMPTY_REF)
                RemoveChild(child);

            RemoveStackSiblings(child);
            RemoveOverlaySiblings(child);
            FreeNode(child);
        }

        /// <summary>Removes siblings and their descedents.
        /// NOTE: For efficiency, this method assumes the input parameter points to the first child. 
        /// Only its immediately linked following siblings (and their descedents) will be removed. 
        /// Preceeding siblings will not be removed.</summary>
        /// <param name="i"></param>
        void RemoveStackSiblings(short i)
        {
            short sib = Nodes[i].StackSibling;
            while (sib != EMPTY_REF)
            {
                RemoveChildIfPresent(sib);
                i = Nodes[sib].StackSibling;
                FreeNode(sib);
                sib = i;
            }
        }

        /// <summary>Removes siblings and their descedents.
        /// NOTE: For efficiency, this method assumes the input parameter points to the first child. 
        /// Only its immediately linked following siblings (and their descedents) will be removed. 
        /// Preceeding siblings will not be removed.</summary>
        /// <param name="i"></param>
        void RemoveOverlaySiblings(short i)
        {
            short sib = Nodes[i].OverlaySibling;
            while (sib != EMPTY_REF)
            {
                RemoveChildIfPresent(sib);
                i = Nodes[sib].OverlaySibling;
                FreeNode(sib);
                sib = i;
            }
        }


        short CheckoutNextAvailableSlot()
        {
            short iNode;
            if (_holes.Count != 0)
                iNode = _holes.PopLast();
            else
            {
                iNode = (short)Nodes.Length;
                var newNodeArray = new LtNode<T>[Nodes.Length * 2];
                Array.Copy(Nodes, newNodeArray, Nodes.Length);
                Nodes = newNodeArray;
                _holes.EnsureCapacityMinimum(newNodeArray.Length);

                for (short i = iNode; i < newNodeArray.Length; i++)
                    FreeNode(i);

            }

            return iNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FreeNode(short i)
        {
            Nodes[i].Principal = EMPTY_REF;
            Nodes[i].Child = EMPTY_REF;
            Nodes[i].StackSibling = EMPTY_REF;
            Nodes[i].OverlaySibling = EMPTY_REF;
            Nodes[i].Value = _defaultValue;

            _holes.Add(i);
        }

        void ResetRootNode()
        {
            Nodes[0].Principal = EMPTY_REF;
            Nodes[0].Child = EMPTY_REF;
            Nodes[0].StackSibling = EMPTY_REF;
            Nodes[0].OverlaySibling = EMPTY_REF;
            Nodes[0].Value = _defaultValue;
        }
    }
}
