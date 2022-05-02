using Faeric.HighPerformanceDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate void ActionRef<T>(ref T item);
    public delegate bool PredicateRef<T>(ref T item);

    public struct LtTreeVisitor<T> where T : struct
    {
        LtTree<T> _tree;
        ActionRef<T> _fnPassRef;
        Action<short> _fnPassIndex;
        PredicateRef<T> _fnTestNode;

        public LtTreeVisitor(LtTree<T> tree)
        {
            _tree = tree;
            _fnPassRef = null;
            _fnPassIndex = null;
            _fnTestNode = null;
        }

        /// <summary>
        /// Find the deepest descendent matching functional node test.<br/><br/>
        /// Overlays are ignored.<br/><br/>
        /// NOTE: The first deep match is returned. If your layout has overlapping siblings or overlapping cousins (children of other branches), the first deep match will be returned and the others will never be tested.
        /// </summary>
        /// <param name="nodeTest"></param>
        /// <returns>Index of deepest descendent matching the node test.</returns>
        public short FirstDeepMatch(PredicateRef<T> nodeTest)
        {
            return FindDeepestMatchingDescedentRecursively(0, 0);
        }

        public void VisitAll(ActionRef<T> func)
        {
            _fnPassRef = func;
            VisitChildByRef(0, 0);
        }

        public void VisitAll(Action<short> func)
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
            _fnPassIndex(iNode);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iNode"></param>
        /// <param name="depth"></param>
        /// <returns>-1 if iNode fails functional test. Otherwise returns index to deepest descendent matching the test</returns>
        short FindDeepestMatchingDescedentRecursively(short iNode, int depth)
        {
            ref var node = ref _tree.Nodes[iNode];

            if (_fnTestNode(ref node.Value))
            {
                //Test descendents
                if (node.Child != LtTree<T>.EMPTY_REF)
                {
                    var result = FindDeepestMatchingDescedentRecursively(node.Child, depth);
                    if (result != -1)
                        return result;
                }

                return iNode;
            }

            depth = depth + 1;

            //Visit stack siblings
            short stackSib = node.StackSibling;
            while (stackSib != LtTree<T>.EMPTY_REF)
            {
                var result = FindDeepestMatchingDescedentRecursively(stackSib, depth);
                if (result != -1)
                    return result;

                stackSib = _tree.Nodes[stackSib].StackSibling;
            }

            return -1;


            //Overlays are ignored for Match TEsts
            /*short overlay = node.OverlaySibling;
            while (overlay != LtTree<T>.EMPTY_REF)
            {
                result = FindDeepestMatchingDescedentRecursive(overlay, depth);
                overlay = _tree.Nodes[overlay].OverlaySibling;
            }*/
        }

        /*
        public void Where(FuncRef<T> testNode)
        {
            _fnTestNode = testNode;
        }

        public void Visit(Action<LtTree<T>, short> func)
        {
            _fnPassIndex = func;
            VisitChildByIndex(0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth">Only increases when recursing to a first child/param>
        void VisitChildByIndexWithCondition(short iNode, int depth)
        {
            ref var node = ref _tree.Nodes[iNode];
            if (_fnTestNode(ref node.Value))
                _fnPassIndex(_tree, iNode);

            depth = depth + 1;

            //Visit stack siblings
            short stackSib = node.StackSibling;
            while (stackSib != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndexWithCondition(stackSib, depth);
                stackSib = _tree.Nodes[stackSib].StackSibling;
            }

            //Visit child
            if (node.Child != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndexWithCondition(node.Child, depth);
            }

            //Visit overlays
            short overlay = node.OverlaySibling;
            while (overlay != LtTree<T>.EMPTY_REF)
            {
                VisitChildByIndexWithCondition(overlay, depth);
                overlay = _tree.Nodes[overlay].OverlaySibling;
            }
        }
        */
    }
}
