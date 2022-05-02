using Faeric.HighPerformanceDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate void ActionRef<T>(ref T item);

    public struct LtTreeVisitor<T> where T : struct
    {
        LtTree<T> _tree;
        ActionRef<T> _fnPassRef;
        Action<LtTree<T>, short> _fnPassIndex;

        public LtTreeVisitor(LtTree<T> tree)
        {
            _tree = tree;
            _fnPassRef = null;
            _fnPassIndex = null;
        }

        public void VisitAll(ActionRef<T> func)
        {
            _fnPassRef = func;
            VisitChildByRef(0, 0);
        }

        public void VisitAll(Action<LtTree<T>, short> func)
        {
            _fnPassIndex = func;
            VisitChildByIndex(0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth">Only increases when recursing to a first child/param>
        void VisitChildByRef(short iNode, int depth)
        {
            ref var node = ref _tree.Nodes[iNode];
            _fnPassRef(ref node.Value);

            depth = depth + 1;

            //Visit stack siblings
            short stackSib = node.StackSibling;
            while (stackSib != LtTree<T>.EMPTY_REF)
            {
                VisitChildByRef(stackSib, depth);
                stackSib = _tree.Nodes[stackSib].StackSibling;
            }

            //Visit child
            if (node.Child != LtTree<T>.EMPTY_REF)
            {
                VisitChildByRef(node.Child, depth);
            }

            //Visit overlays
            short overlay = node.OverlaySibling;
            while (overlay != LtTree<T>.EMPTY_REF)
            {
                VisitChildByRef(overlay, depth);
                overlay = _tree.Nodes[overlay].OverlaySibling;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth">Only increases when recursing to a first child/param>
        void VisitChildByIndex(short iNode, int depth)
        {
            ref var node = ref _tree.Nodes[iNode];
            _fnPassIndex(_tree, iNode);

            depth = depth + 1;

            //Visit stack siblings
            short stackSib = node.StackSibling;
            while (stackSib != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndex(stackSib, depth);
                stackSib = _tree.Nodes[stackSib].StackSibling;
            }

            //Visit child
            if (node.Child != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndex(node.Child, depth);
            }

            //Visit overlays
            short overlay = node.OverlaySibling;
            while (overlay != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndex(overlay, depth);
                overlay = _tree.Nodes[overlay].OverlaySibling;
            }
        }
    }
}
