using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Faeric.HighPerformanceDataStructures.Tests
{
    [TestClass]
    public class RefQueueTests
    {
        public struct MyStruct
        {
            public int A;
        }

        RefQueue<MyStruct> CreateTestQ()
        {
            var q = new RefQueue<MyStruct>(2);

            IsTrue(q.Count == 0);
            for (int i = 0; i < 3; i++)
                q.Enqueue(
                    new MyStruct()
                    {
                        A = i
                    });

            IsTrue(q.Capacity == 4);
            IsTrue(q.Count == 3);

            return q;
        }

        [TestMethod]
        public void Enqueue()
        {
            var q = CreateTestQ();

            MyStruct a = new MyStruct();
            a.A = 10;

            q.Enqueue(a);
            IsTrue(q.Capacity == 4);
            IsTrue(q.Count == 4);

            a.A = 15;
            q.Enqueue(a);
            IsTrue(q.Capacity == 8);
            IsTrue(q.Count == 5);
        }

    }
}
