using Faeric.HighPerformanceDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.Layout.HighPerformanceDataStructures
{
    public delegate void ActionRef<T>(ref T item);

    public struct LtTreeVisitor<T> where T : struct
    {
        LtTree<T> _tree;
        ActionRef<T> _fn;

        public LtTreeVisitor(LtTree<T> tree)
        {
            _tree = tree;
            _fn = null;
        }

        public void VisitAll(ActionRef<T> func)
        {
            _fn = func;
            VisitChild(0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth">Only increases when recursing to a first child/param>
        void VisitChild(short iNode, int depth)
        {
            ref var node = ref _tree.Nodes[iNode];
            _fn(ref node.Value);

            depth = depth + 1;

            //Visit stack siblings
            short stackSib = node.StackSibling;
            while (stackSib != LtTree<T>.EMPTY_REF)
            {
                VisitChild(stackSib, depth);
                stackSib = _tree.Nodes[stackSib].StackSibling;
            }

            //Visit child
            if (node.Child != LtTree<T>.EMPTY_REF)
            {
                VisitChild(node.Child, depth);
            }

            //Visit overlays
            short overlay = node.OverlaySibling;
            while (overlay != LtTree<T>.EMPTY_REF)
            {
                VisitChild(overlay, depth);
                overlay = _tree.Nodes[overlay].OverlaySibling;
            }
        }
    }
}
