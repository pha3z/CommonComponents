using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static System.Console;


namespace Faeric.HighPerformanceDataStructures.Tests
{

    [TestClass]
    public class TrinaryIndexTest
    {
        void EmptySlotEraser(ref MyStruct s) => s.A = -1;
        bool EmptySlotTester(ref MyStruct s) => s.A < 0;

        TrinaryIndex< MyStruct > CreateTestIndex()
        {
            var index = new TrinaryIndex<MyStruct>(
                initialCapacity: 2,
                emptySlotEraser: EmptySlotEraser,
                emptySlotTest: EmptySlotTester);

            IsTrue(index.Count == 0);
            for (int i = 0; i < 3; i++)
            {
                int idx = index.Add_Uninitialized_byIndex();
                index[idx].A = i;
                
            }

            IsTrue(index.Capacity == 6);
            IsTrue(index.Count == 3);

            return index;
        }

        /// <summary>
        /// Suggested sizes for testing: 14, 15, 16
        /// </summary>
        TrinaryIndex<MyStruct> CreateSnapshot(int size)
        {
            var index = new TrinaryIndex<MyStruct>(
                initialCapacity: size,
                emptySlotEraser: EmptySlotEraser,
                emptySlotTest: EmptySlotTester);

            for (int i = 0; i < size; i++)
            {
                int idx = index.Add_Uninitialized_byIndex();
                index[idx].A = i;

            }

            return index;
        }

        void Print(int[] data)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sb.Append(" " + data[i]);

            WriteLine(sb.ToString());
        }

        void Print(TrinaryIndex<MyStruct> index)
        {
            StringBuilder sb = new StringBuilder();
            
            for(int i = 0; i < index.Capacity; i++)
            {
                if (index[i].A < 0)
                    sb.Append(" . ");
                else
                    sb.Append(" " + index[i].A);
            }

            sb.Append($"\t\tCnt: {index.Count}");
            WriteLine(sb.ToString());
        }

        void ShouldMatch(TrinaryIndex<MyStruct> index, int[] data)
        {
            IsTrue(index.Count == data.Length);

            index.ResetIterator();
            int i = 0;
            while (index.HasNext())
            {
                MyStruct foo = index.Next();
                IsTrue(foo.A == data[i]);
                i++;
            }

        }

        void ShouldMatchSet(TrinaryIndex<MyStruct> index, int[] data)
        {
            IsTrue(index.Count == data.Length);

            index.ResetIterator();
            int i = 0;
            while (index.HasNext())
            {
                MyStruct foo = index.Next();
                IsTrue(data.Contains(foo.A));
                i++;
            }

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
            IsTrue(index.Capacity == 6);

            s.A = 15;
            index.Add(s);
            IsTrue(index[4].A == 15);
            IsTrue(index.Count == 5);

            index.Add(s);
            index.Add(s);
            IsTrue(index.Capacity == 12);

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
            IsTrue(index.Capacity == 6);

            index.Add_Uninitialized_ByRef();
            index.Add_Uninitialized_ByRef();
            IsTrue(index.Capacity == 12);
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
            IsTrue(index.Capacity == 6);

            i = index.Add_Uninitialized_byIndex();
            ref MyStruct s2 = ref index[i];
            s2.A = 15;
            IsTrue(index[4].A == 15);
            IsTrue(index.Count == 5);
            IsTrue(index.Capacity == 6);

            i = index.Add_Uninitialized_byIndex();
            i = index.Add_Uninitialized_byIndex();
            IsTrue(index.Capacity == 12);

        }

        [TestMethod]
        public void RemoveByIndex()
        {
            var index = CreateTestIndex();

            index.RemoveAt(2);

            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 6);
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
            IsTrue(index.Capacity == 6);
        }

        [TestMethod]
        public void RemoveLast()
        {
            var index = CreateTestIndex();

            index.RemoveLast();
            IsTrue(index.Count == 2);
            IsTrue(index.Capacity == 6);

            index.RemoveLast();
            IsTrue(index.Count == 1);

            index.RemoveLast();
            IsTrue(index.Count == 0);
        }

        [TestMethod]
        public void TrimExcess()
        {
            var index = CreateTestIndex();

            index.EnsureCapacity(19);
            IsTrue(index.Count == 3);
            IsTrue(index.Capacity == 19);

            index.TrimExcess(2);
            IsTrue(index.Count == 3);
            IsTrue(index.Capacity == 6);
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
            IsTrue(index.Capacity == 6);
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


        [TestMethod]
        public void SnapTest1()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            index.RemoveAt(15);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
        }

        [TestMethod]
        public void SnapTest2()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            index.RemoveAt(15);
            index.RemoveAt(14);
            index.RemoveAt(13);
            index.RemoveAt(12);
            index.RemoveAt(11);

            Print(index);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void SnapTest3()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            index.RemoveAt(15);
            index.RemoveAt(14);
            index.RemoveAt(13);
            index.RemoveAt(12);
            index.RemoveAt(11);

            Print(index);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10 });

            //There is no guarantee on what index will be returned from the free slots
            //So you cannot directly compare order.
            int i = index.Add_Uninitialized_byIndex();
            index[i].A = 32;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 33;

            Print(index);
            var result = new int[] { 0, 1, 32, 33, 6, 7, 8, 9, 10 };
            Print(result);
            ShouldMatchSet(index, result);
        }

        [TestMethod]
        public void SnapTest4()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            index.RemoveAt(15);
            index.RemoveAt(14);
            index.RemoveAt(13);
            index.RemoveAt(12);
            index.RemoveAt(11);

            Print(index);
            ShouldMatch(index, new int[] {0, 1, 6, 7, 8, 9, 10 });

            //There is no guarantee on what index will be returned from the free slots
            //So you cannot directly compare order.
            int i = index.Add_Uninitialized_byIndex();
            index[i].A = 32;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 33;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 34;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 35;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 36;

            Print(index);
            var result = new int[] {0, 1, 32, 33, 34, 35, 36, 6, 7, 8, 9, 10 };
            Print(result);
            ShouldMatchSet(index, result);
        }

        [TestMethod]
        public void SnapTest5()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            index.RemoveAt(15);
            index.RemoveAt(14);
            index.RemoveAt(13);
            index.RemoveAt(12);
            index.RemoveAt(11);

            Print(index);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10 });

            //There is no guarantee on what index will be returned from the free slots
            //So you cannot directly compare order.
            int i = index.Add_Uninitialized_byIndex();
            index[i].A = 32;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 33;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 34;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 35;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 36;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 37;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 38;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 39;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 40;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 41;


            Print(index);
            var result = new int[] { 0, 1, 32, 33, 34, 35, 6, 7, 8, 9, 10, 36, 37, 38, 39, 40, 41};
            Print(result);
            ShouldMatchSet(index, result);
        }

        [TestMethod]
        public void SnapTest6()
        {
            var index = CreateSnapshot(16);

            Print(index);

            index.RemoveAt(2);
            index.RemoveAt(3);
            index.RemoveAt(4);
            index.RemoveAt(5);

            Print(index);
            
            index.RemoveAt(14);
            index.RemoveAt(13);
            index.RemoveAt(12);
            index.RemoveAt(11);

            Print(index);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10, 15 });

            index.RemoveAt(15);
            Print(index);
            ShouldMatch(index, new int[] { 0, 1, 6, 7, 8, 9, 10 });

            //There is no guarantee on what index will be returned from the free slots
            //So you cannot directly compare order.
            int i = index.Add_Uninitialized_byIndex();
            index[i].A = 32;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 33;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 34;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 35;
            i = index.Add_Uninitialized_byIndex();
            index[i].A = 36;

            Print(index);
            var result = new int[] { 0, 1, 32, 33, 34, 35, 36, 6, 7, 8, 9, 10 };
            Print(result);
            ShouldMatchSet(index, result);
        }
    }
}
