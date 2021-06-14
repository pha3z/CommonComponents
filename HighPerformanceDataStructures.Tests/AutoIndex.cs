using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Faeric.HighPerformanceDataStructures.Tests
{
    [TestClass]
    public class AutoIndex
    {
        private AutoIndex<int> CreateTestList()
        {
            var list = new AutoIndex<int>(2);

            IsTrue(list.Count == 0);
            for (int i = 0; i < 3; i++)
                list.Add(i);

            IsTrue(list.Capacity == 4);
            IsTrue(list.Count == 3);

            return list;
        }

        [TestMethod]
        public void Add()
        {
            CreateTestList();
        }

        [TestMethod]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(-1)]
        public void AddInOrderedPosition(int idx)
        {
            var list = CreateTestList();
            list.AddInOrderedPosition(idx, (a, b) => a > b);

            IsTrue(list.Count == 4);

            if (idx == 3)
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
                IsTrue(list[2] == 2);
                IsTrue(list[3] == 3);
            }
            else if (idx == 4)
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
                IsTrue(list[2] == 2);
                IsTrue(list[3] == 4);

                list.AddInOrderedPosition(3, (a, b) => a > b);
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
                IsTrue(list[2] == 2);
                IsTrue(list[3] == 3);
                IsTrue(list[4] == 4);
            }
            else if (idx == -1)
            {
                IsTrue(list[0] == -1);
                IsTrue(list[1] == 0);
                IsTrue(list[2] == 1);
                IsTrue(list[3] == 2);

            }
        }

        [TestMethod]
        public void RemoveBySwap()
        {
            var list = CreateTestList();

            list.RemoveBySwap(2);
            IsTrue(list[2] == 2);
        }

        [TestMethod][DataRow(0)][DataRow(1)][DataRow(2)]
        public void Remove_RetainingOrder(int idx)
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.Remove_RetainingOrder(idx);
            IsTrue(list.Count == initialCount - 1);

            if (idx == 0)
            {
                IsTrue(list[0] == 1);
                IsTrue(list[1] == 2);
            }
            else if (idx == 1)
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 2);
            }
            else
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
            }
        }

        [TestMethod]
        public void RemoveLast()
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.RemoveLast();
            IsTrue(list.Count == initialCount - 1);
        }

        [TestMethod][DataRow(0)][DataRow(1)][DataRow(2)]
        public void RemoveFirstN(int N)
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.RemoveFirstN(N);
            IsTrue(list.Count == initialCount - N);

            if(list.Count > 0 && N > 0)
                IsTrue(list[0] == 2);
        }

        [TestMethod][DataRow(0)][DataRow(1)][DataRow(2)]
        public void RemoveLastN(int N)
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.RemoveLastN(N);
            IsTrue(list.Count == initialCount - N);
            if (list.Count > 0)
                IsTrue(list.Last == 2 - N);
        }

        

        [TestMethod][DataRow(0)][DataRow(1)][DataRow(2)]
        public void RemoveFirstMatch(int idx)
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.RemoveFirstMatch(x => x == idx);

            IsTrue(list.Count == initialCount - 1);

            if (idx == 0)
            {
                IsTrue(list[0] == 2);
                IsTrue(list[1] == 1);
            }
            else if (idx == 1)
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 2);
            }
            else
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
            }
        }


        [TestMethod][DataRow(0)][DataRow(1)][DataRow(2)]
        public void RemoveFirstMatch_RetainingOrder(int idx)
        {
            var list = CreateTestList();

            int initialCount = list.Count;
            list.RemoveFirstMatch_RetainingOrder(x => x == idx);

            IsTrue(list.Count == initialCount - 1);

            if (idx == 0)
            {
                IsTrue(list[0] == 1);
                IsTrue(list[1] == 2);
            }
            else if (idx == 1)
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 2);
            }
            else
            {
                IsTrue(list[0] == 0);
                IsTrue(list[1] == 1);
            }
        }

        [TestMethod]
        public void Clear()
        {
            var list = CreateTestList();

            list.Clear();
            IsTrue(list.Count == 0);
            IsTrue(list.Capacity == 4);

            list.Add(5);

            list.Clear(2);
            IsTrue(list.Count == 0);
            IsTrue(list.Capacity == 2);
        }

        [TestMethod]
        public void Any()
        {
            var list = CreateTestList();

            IsFalse(list.Any(x => x == -1));
            IsTrue(list.Any(x => x == 0));
            IsTrue(list.Any(x => x == 1));
            IsTrue(list.Any(x => x == 2));
            IsFalse(list.Any(x => x == 3));

            list.Clear();
            IsFalse(list.Any(x => x == -1));
            IsFalse(list.Any(x => x == 0));
            IsFalse(list.Any(x => x == 1));
            IsFalse(list.Any(x => x == 2));
            IsFalse(list.Any(x => x == 3));
        }

        [TestMethod]
        public void InsertionSort()
        {
            throw new NotImplementedException();
        }

    }
}
