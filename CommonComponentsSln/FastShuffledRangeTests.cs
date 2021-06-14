using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using static System.Console;
using System.Linq;
using System.Collections.Generic;

namespace CommonComponentsSln
{
    [TestClass]
    public class FastShuffledRangeTests
    {
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(10)]
        [DataRow(20)]
        public void VariousRangeTests(int count)
        {
            var sourceString = new StringBuilder();
            for (int i = 0; i < count; i++)
                sourceString.Append(i.ToString() + ",");

            WriteLine("source_: " + sourceString);
            var shuffled = new FastShuffledRange(0, count);

            var shuffledNumbers = new List<int>();
            int next;
            for (int i = 0; i < shuffled.Length; i++)
            {
                next = shuffled.NextValue();
                shuffledNumbers.Add(next);
            }

            WriteLine("shuffled: " + string.Join(',', shuffledNumbers));

            Assert.IsTrue(shuffled.Length == count);
            Assert.IsTrue(shuffledNumbers.Distinct().Count() == count);
        }
    }
}
