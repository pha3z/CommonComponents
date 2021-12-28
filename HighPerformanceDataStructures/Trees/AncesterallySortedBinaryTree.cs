using Faeric.HighPerformanceDataStructures;
using Faeric.Redzen.Sorting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// When items are created in this tree, they are always created at an index greater than their parent,
    /// ensuring the tree is always ordered in the underlying array such that a child can never occur before its parent.
    /// Consequently, repeated removals and additions will cause growing holes.
    /// To remove holes, Compact() shifts all items left and fixes up their parent and child indexes.
    /// Autocompaction is possible by specifying a count of removals before automatic compaction.
    /// <br/><br/>What's FAST: Because the tree is always ordered, you can iterate over all nodes
    /// in a straight-ahead manner guaranteeing you will always visit ancestors before their children.
    /// <br/><br/>What's SLOW: Because compacting the tree requires copying all of the struct values to new positions in underlying array,
    /// the compaction can get expensive with large structs, large trees, or frequent operation.
    /// If you need frequent removals, you might be better off with
    /// with a traditional tree that works on reference type nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AncesterallySortedBinaryTree<T>
        where T : struct
    {
        //Using 0 as the default value allows to keep the default values when the underlying array is created
        //since the CLR sets default values. Its a small performance grab, but its something.
        public const short EMPTY_REF = -1;

        /// <summary>
        /// Causes an implicit call to Compact() so that you can iterate over nodes with a promise of no holes (invalid nodes).
        /// </summary>
        /// <returns></returns>
        public IReadOnlySortedRefList<BinaryNode<T>> GetNodes()
        {
            Compact();
            return _nodes;
        }

        public ref BinaryNode<T> Node(int index) => ref _nodes[index];

        RefList<BinaryNode<T>> _nodes;
        //FastList<short> _holes;

        public int NodeCount => _nodes.Count - _holeCount;
        short _holeCount = 0;
        short _compactAfterNRemovals;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compactAfterNRemovals">0 means never automatically compact. If you set this to 0 and you CreateValues directly after parents in an optimize manner, you will need do compaction periodically or you will get memory leaks.</param>
        /// <param name="capacity"></param>
        public AncesterallySortedBinaryTree(short compactAfterNRemovals = 0, int capacity = 8)
        {
            if (compactAfterNRemovals < 0)
                throw new Exception($"{nameof(compactAfterNRemovals)}  must be 0 (disables auto compaction) or a positive value. Got {compactAfterNRemovals}.");

            _nodes = new RefList<BinaryNode<T>>(
                capacity,
                defaultValue: 
                    new BinaryNode<T>()
                    {
                        Parent = EMPTY_REF,
                        Left = EMPTY_REF,
                        Right = EMPTY_REF
                    });
            
            _compactAfterNRemovals = compactAfterNRemovals;
            _ = ref _nodes.AddByRef_Unsafe();
        }

        public void Clear()
        {
            _holeCount = 0;

            //We have to reset all the indexes and T Value to avoid leaked state.
            //Note: Doing this on a Clear() op is the most logical time to do it.
            //Because a Clear() op is going to be relatively rare (may never even happen in many use cases)
            //And the cost of reseting the values is the same as what would happen if you created a new tree (since the CLR explicitly sets values to defaults for a new data structure).
            for(int i = 0; i < _nodes.Capacity; i++)
            {
                _nodes[i].Left = EMPTY_REF;
                _nodes[i].Right = EMPTY_REF;
                _nodes[i].Parent = EMPTY_REF;
                _nodes[i].Value = default;
            }

            _nodes.Clear();
            _ = ref _nodes.AddByRef_Unsafe();
        }

        /*
        public short CreateLeftChild(short parent)
        {
            short i = GrabAnyHole();
            _nodes[i] = new BinaryNode<T>(parent);
            _nodes[parent].Left = i;
            return i;
        }*/

        /*
        public short CreateRightChild(short parent)
        {
            short i = GrabAnyHole();
            _nodes[i] = new BinaryNode<T>(parent);
            _nodes[parent].Right = i;
            return i;
        }*/

        /// <summary>
        /// Inserts a lefthand child for the given parent. Guarantees the child is inserted somewhere after the parent in the underlying array.
        /// <br/><br/>NOTE: This means you will get memory leaks as holes in the beginning of the tree grow continuously,
        /// unless you compact the tree periodically.
        /// <br/>Holes can only appear from removals, which makes removal counting a good metric for when to perform a compaction.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateLeftChild(short parent)
        {
            short i = GrabNextHoleAfterParent(parent);
            _nodes[parent].Left = i;
            _nodes[i].Parent = parent;
            return i;
        }

        /// <summary>
        /// Inserts a righthand child for the given parent. Guarantees the child is inserted somewhere after the parent in the underlying array.
        /// <br/><br/>NOTE: This means you will get memory leaks as holes in the beginning of the tree grow continuously,
        /// unless you compact the tree periodically.
        /// <br/>Holes can only appear from removals, which makes removal counting a good metric for when to perform a compaction.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>Index of the new child</returns>
        public short CreateRightChild(short parent)
        {
            short i = GrabNextHoleAfterParent(parent);
            _nodes[parent].Right = i;
            _nodes[i].Parent = parent;
            return i;
        }

        public short CreateChild(short parent, Hand hand )
        {
            if (hand == Hand.LEFT)
                return CreateLeftChild(parent);
            else
                return CreateRightChild(parent);

        }

         
        /// <summary>
        /// Removes the parent's reference to the indexed node.  Adds the index to the internal holes list so that its free for reuse.
        /// <br/>Also recurses and removes children of the removed node.
        /// </summary>
        /// <param name="i"></param>
        public void Remove(short i)
        {
            short iParent = _nodes[i].Parent;

            //If the left child is our removal candidate, then mark it removed
            //If the left child doesn't match, we assume the right child must be the match.
            //NOTE: The assumption works as long as the tree is valid.  If some other operation has a bug or creates an invalid state
            //this assumption might fail.  But we make the assumption because it eliminates an extra check and should always work, provided the tree is used correctly.
            if (_nodes[iParent].Left == i)
                _nodes[iParent].Left = EMPTY_REF;
#if DEBUG
            else if (_nodes[iParent].Right == i)
                _nodes[iParent].Right = EMPTY_REF;
            else
                throw new Exception($"Attempted to remove a node not referenced as a child of its parent. This can occur if the tree was improperly mutated somewhere else, producing an invalid state.");
#else
            else
                _nodes[iParent].Right = EMPTY_REF;
#endif

            RemoveRecursive(i);
            if (_compactAfterNRemovals != 0 && _holeCount > _compactAfterNRemovals)
                Compact();
        }

        void RemoveRecursive(short i)
        {
            _holeCount++;

            //Remove children
            if (_nodes[i].Left != EMPTY_REF)
            {
                RemoveRecursive(_nodes[i].Left);
                _nodes[i].Left = EMPTY_REF;
            }
            if (_nodes[i].Right != EMPTY_REF)
            {
                RemoveRecursive(_nodes[i].Right);
                _nodes[i].Right = EMPTY_REF;
            }

            _nodes[i].Parent = EMPTY_REF;
            _nodes[i].Value = default;
        }

        /// <summary>
        /// Naively grabs a hole from the holes list regardless of its position in the tree array
        /// </summary>
        /// <returns></returns>
        /*short GrabAnyHole()
        {
            if(_holes.Count > 0)
            {
                short lastHole = _holes[_holes.Count - 1];
                _holes.RemoveLast();
                return lastHole;
            }

            if(_nodes.Count == _nodes.Capacity)
                _nodes.EnsureCapacityMatch(_nodes.Capacity * 2);
            return (short)(_nodes.Count);
        }*/

        /// <summary>
        /// Grabs the next hole after parent. Can be used to ensure children are always inserted after parent
        /// <br/>NOTE: This means you will get memory leaks as holes in the beginning of the tree grow continuously,
        /// unless you compact the tree periodically.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        short GrabNextHoleAfterParent(short parent)
        {
            if(_holeCount == 0)
            {
                short count = (short)(_nodes.Count);
                _nodes.AddByRef();
                return count;
            }

            for (short i = (short)(parent + 1); i < _nodes.Count; i++)
            {
                if (_nodes[i].Parent == EMPTY_REF)
                    return i;
            }

            throw new Exception($"Unreachable code reached in {nameof(GrabNextHoleAfterParent)}. {nameof(_holeCount)} was zero, but no holes found.");

        }

        /// <summary>
        /// If you ever remove children and create new ones in an sorted manner, you will eventually
        /// end up with holes that keep growing. You will need to periodically compact the tree.<br/><br/>
        /// If you want to avoid calling this method manually, you can create the BinaryTree with a count for automatic compaction.
        /// <br/><br/>NOTE: There is no cost to calling this function if there are no holes. It short-circuits and returns immediately.
        /// </summary>
        public void Compact()
        {
            if (_holeCount == 0)
                return; 

            //Because the tree is already sorted, all we have to do shift values to the left.

            //Find the first hole and the number of immediate holes after it
            //These form a gap. Shift everything left to the fill the gap.
            short gapStart = FindFirstHole(1); //Start immediately after root
            while (gapStart != -1)
            {
                short gapEndEx = FindFirstNonHole(gapStart);
#if DEBUG
                if (gapEndEx == -1)
                    throw new Exception("This shouldn't happen.  It means we started searching from the end of the node list.");
#endif

                short iParentOfNextNode = _nodes[gapEndEx].Parent;
                if (_nodes[iParentOfNextNode].Left == gapEndEx)
                    _nodes[iParentOfNextNode].Left = gapStart;
                else
                    _nodes[iParentOfNextNode].Right = gapStart;

                int shiftAmount = gapEndEx - gapStart;
                ShiftNodesLeft(gapEndEx, shiftAmount);
                _nodes.RemoveLastN(shiftAmount);
               
                gapStart = FindFirstHole((short)(gapStart + 1));
            }

            _holeCount = 0;
        }

        void ShiftNodesLeft(short startIdx, int shiftAmount)
        {
            int newStartIdx = startIdx - shiftAmount;
            int newStartIdxMinusOne = newStartIdx - 1;

            int newCount = _nodes.Count - shiftAmount;
            for (int i = newStartIdx; i < newCount; i++)
            {
                //Copy alues from node j to node i.
                int j = i + shiftAmount;
                
                _nodes[i].Parent = 
                    _nodes[j].Parent > newStartIdxMinusOne
                        ? (short)(_nodes[j].Parent - shiftAmount)
                        : _nodes[i].Parent = _nodes[j].Parent;
                
                _nodes[i].Left = 
                    _nodes[j].Left == EMPTY_REF 
                        ? EMPTY_REF 
                        : (short)(_nodes[j].Left - shiftAmount);
                _nodes[i].Right = 
                    _nodes[j].Right == EMPTY_REF 
                        ? EMPTY_REF 
                        : (short)(_nodes[j].Right - shiftAmount);
                _nodes[i].Value = _nodes[j].Value;
            }

            int clearAfter = newCount - 1;
            for (int i = _nodes.Count - 1; i > clearAfter; i--)
            {
                _nodes[i].Parent = EMPTY_REF;
                _nodes[i].Left = EMPTY_REF;
                _nodes[i].Right = EMPTY_REF;
                _nodes[i].Value = default;
            }

        }

        short FindFirstHole(short startIdx)
        {
#if DEBUG
            if (startIdx == 0)
                throw new Exception($"{nameof(FindFirstHole)}() should not start at root.");
#endif

            for (short i = startIdx; i < _nodes.Count; i++)
            {
                //We've found a hole.
                if (_nodes[i].Parent == -1)
                    return i;
            }

            return -1;
        }

        short FindFirstNonHole(short startIdx)
        {
            for (short i = startIdx; i < _nodes.Count; i++)
            {
                //We've found a none hole.
                if (_nodes[i].Parent != -1)
                    return i;
            }

            return -1;
        }


        /*
        short GrabNextHoleAfterParent(short parent)
        {
            short nextHole = GetFirstHoleAfterParent(parent);
            if(nextHole > - 1)
            {
                _holes.RemoveBySwap(nextHole);
                return nextHole;
            }

            if (_nodes.Count == _nodes.Capacity)
                _nodes.EnsureCapacityMatch(_nodes.Capacity * 2);
            return (short)(_nodes.Count);
        }

        
        short GetFirstHoleAfterParent(short parent)
        {
            short match = short.MaxValue;
            int diff = match - parent;

            for (int i = 0; i < _holes.Count; i++)
            {
                short lastHole = _holes[i];
                if (lastHole > parent)
                {
                    int tDiff = lastHole - parent;
                    if (tDiff < diff)
                    {
                        match = lastHole;
                        diff = tDiff;
                    }
                }
            }

            if (match == short.MaxValue)
                return -1;
            else
                return match;
        }*/

    }
}
