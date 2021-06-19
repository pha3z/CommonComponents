using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Faeric.HighPerformanceDataStructures.Tests
{
    public struct MyStruct
    {
        public int A;
    }


    [TestClass]
    public class IndexTest
    {
        void EmptySlotEraser(ref MyStruct s) => s.A = -1;
        bool EmptySlotTester(ref MyStruct s) => s.A < 0;

        Index< MyStruct > CreateTestIndex()
        {
            var index = new Index<MyStruct>(
                initialCapacity: 2,
                emptySlotEraser: EmptySlotEraser,
                emptySlotTest: EmptySlotTester);

            IsTrue(index.Count == 0);
            for (int i = 0; i < 3; i++)
            {
                int idx = index.Add_Uninitialized_byIndex();
                index[idx].A = i;
                
            }

            IsTrue(index.Capacity == 4);
            IsTrue(index.Count == 3);

            return index;
        }

        [TestMethod]
        public void Add()
        {
            var index = CreateTestIndex();

            var s = new MyStruct()
            {
                A = 10
            };

            index.Add(s);
            IsTrue(index[3].A == 10);
            IsTrue(index.Count == 4);
            IsTrue(index.Capacity == 4);

            s.A = 15;
            index.Add(s);
            IsTrue(index[4].A == 15);
            IsTrue(index.Count == 5);
            IsTrue(index.Capacity == 8);

        }

        [TestMethod]
        public void Add_Uninitialized_ByRef()
        {
            var index = CreateTestIndex();

            ref MyStruct s = ref index.Add_Uninitialized_ByRef();
            s.A = 10;
            IsTrue(index[3].A == 10);

            ref MyStruct t = ref index.Add_Uninitialized_ByRef();
            t.A = 15;
            IsTrue(index[4].A == 15);
            IsTrue(index.Count == 5);
            IsTrue(index.Capacity == 8);
        }

        [TestMethod]
        public void Add_Uninitialized_ByIndex()
        {
            var index = CreateTestIndex();

            int i = index.Add_Uninitialized_byIndex();
            ref MyStruct s = ref index[i];
            s.A = 10;
            IsTrue(index[3].A == 10);
            IsTrue(index.Count == 4);
            IsTrue(index.Capacity == 4);

            i = index.Add_Uninitialized_byIndex();
            ref MyStruct s2 = ref index[i];
            s2.A = 15;
            IsTrue(index[4].A == 15);
            IsTrue(index.Count == 5);
            IsTrue(index.Capacity == 8);

        }

        [TestMethod]
        public void RemoveByIndex()
        {
            var index = CreateTestIndex();

            index.RemoveAt(2);

            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 4);
        }

        [TestMethod]
        public void RemoveByMatch()
        {
            var index = CreateTestIndex();

            var s = new MyStruct()
            {
                A = 1
            };

            bool AreMatched(ref MyStruct a)
                => a.A == s.A;

            index.Remove(AreMatched);

            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 4);
        }

        [TestMethod]
        public void RemoveLast()
        {
            var index = CreateTestIndex();

            index.RemoveLast();
            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 4);

            index.RemoveLast();
            IsTrue(index.Count == 1);

            index.RemoveLast();
            IsTrue(index.Count == 0);
        }

        [TestMethod]
        public void TrimExcess()
        {
            var index = CreateTestIndex();

            index.TrimExcess(2);
            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 2);
        }

        [TestMethod]
        public void LastByRef()
        {
            var index = CreateTestIndex();

            ref MyStruct s = ref index.LastByRef;
            IsTrue(s.A == 2);
        }

        [TestMethod]
        public void LastIndex()
        {
            var index = CreateTestIndex();

            int i = index.LastIndex;
            IsTrue(i == 2);
        }

        [TestMethod]
        public void Clear()
        {
            var index = CreateTestIndex();

            index.Clear();
            IsTrue(index.Count == 0);
            IsTrue(index.Capacity == 4);
        }

        [TestMethod]
        public void IterateByRef()
        {
            var index = CreateTestIndex();

            index.ResetIterator();

            for (int i = 0; i < index.Count; i++)
            {
                IsTrue(index.HasNext());
                ref MyStruct s = ref index.NextByRef();
                IsTrue(s.A == i);
            }

            IsFalse(index.HasNext());
        }

        [TestMethod]
        public void IterateByRef_OneHole()
        {
            var index = CreateTestIndex();

            index.RemoveAt(1);

            index.ResetIterator();

            for (int i = 0; i < index.Count; i++)
            {
                IsTrue(index.HasNext());
                ref MyStruct s = ref index.NextByRef();

                if (i == 0)
                    IsTrue(s.A == 0);
                else if (i == 1)
                    IsTrue(s.A == 2);
            }

            IsFalse(index.HasNext());
        }
    }
}
