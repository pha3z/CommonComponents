using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Faeric.HighPerformanceDataStructures.Tests
{
    [TestClass]
    public class IndexTest
    {
        Index<int> CreateTestList()
        {
            var list = new Index<int>(2);

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
    }
}
