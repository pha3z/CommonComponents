﻿using Faeric.HighPerformanceDataStructures;
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
        public FishList<LtNode<T>> Nodes;

        /// <summary>A direct reference to Nodes.Items. To eliminate extra indirection. Might not actually matter in practice. Not benchmarked.</summary>
        LtNode<T>[] _items;

        T _defaultValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="defaultValue">The initial array will be filled with this value. If the array capacity changes, new empty values will also be filled with this value.</param>
        public LtTree(int capacity = 8, T defaultValue = default)
        {
            _defaultValue = defaultValue;
            Nodes = new FishList<LtNode<T>>(capacity);
            _items = Nodes.Items;
            Reset();

            Console.WriteLine("halt here");
        }

        /// <summary>
        /// All nodes are removed except for the root node at position 0. For housekeeping, all node slots are reset to constructor-supplied defaultValue to avoid leaking state.
        /// </summary>
        public void Reset()
        {
            for (short i = 0; i < Nodes.RightBoundItem; i++)
            {
                _items[i].Principal = EMPTY_REF;
                _items[i].Child = EMPTY_REF;
                _items[i].StackSibling = EMPTY_REF;
                _items[i].OverlaySibling = EMPTY_REF;
                _items[i].Value = _defaultValue;
            }

            Nodes.Clear();

            //Add a node for the root.
            Nodes.AddByRef_Unchecked();
        }


        /// <summary>
        /// Inserts a child for the given parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateChild(short parent)
        {
            short i = (short)Nodes.InsertNewElementReturningIndex();
            _items[parent].Child = i;
            _items[i].Principal = parent;
            return i;
        }

        /// <summary>
        /// Inserts an OverlaySibling for the given node.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateOverlaySibling(short parent)
        {
            short i = (short)Nodes.InsertNewElementReturningIndex();
            _items[parent].OverlaySibling = i;
            _items[i].Principal = parent;
            return i;
        }

        /// <summary>
        /// Inserts a stack sibling.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns>Index of the new child</returns>
        public short CreateStackSibling(short principal)
        {
            short i = (short)Nodes.InsertNewElementReturningIndex();
            _items[principal].StackSibling = i;
            _items[i].Principal = principal;
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
            short i = _items[parent].Child;

            while (_items[i].OverlaySibling != EMPTY_REF)
            {
                if (_items[i].OverlaySibling == node)
                    _items[i].OverlaySibling = _items[node].OverlaySibling;

                i++;
            }

            i = _items[parent].Child;
            while (_items[i].StackSibling != EMPTY_REF)
            {
                if (_items[i].StackSibling == node)
                    _items[i].StackSibling = _items[node].StackSibling;

                i++;
            }

            RemoveChildIfPresent(node);
            RemoveNode(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChildIfPresent(short parent)
        {
            if (_items[parent].Child != EMPTY_REF)
                RemoveChild(parent);
        }

        public void RemoveChild(short parent)
        {
            short child = _items[parent].Child;
            _items[parent].Child = EMPTY_REF;

            if (_items[child].Child != EMPTY_REF)
                RemoveChild(child);

            RemoveStackSiblings(child);
            RemoveOverlaySiblings(child);
            RemoveNode(child);
        }

        /// <summary>Removes siblings and their descedents.
        /// NOTE: For efficiency, this method assumes the input parameter points to the first child. 
        /// Only its immediately linked following siblings (and their descedents) will be removed. 
        /// Preceeding siblings will not be removed.</summary>
        /// <param name="i"></param>
        void RemoveStackSiblings(short i)
        {
            short sib = _items[i].StackSibling;
            while (sib != EMPTY_REF)
            {
                RemoveChildIfPresent(sib);
                i = Nodes[sib].StackSibling;
                RemoveNode(sib);
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
                RemoveNode(sib);
                sib = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveNode(short i)
        {
            _items[i].Principal = EMPTY_REF;
            _items[i].Child = EMPTY_REF;
            _items[i].StackSibling = EMPTY_REF;
            _items[i].OverlaySibling = EMPTY_REF;
            _items[i].Value = _defaultValue;
            Nodes.Remove(i);
        }
    }
}
