using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Crawler.Tests
{
    [TestClass]
    public class SplitInPagesTests
    {
        [TestInitialize]
        public void Setup()
        {

        }

        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            int[] original = { 1, 2, 3, 4, 5, 6, 7 };
            var result = new List<int>(original.Length);

            //Act
            foreach (var sublist in original.SplitInPages(2))
                foreach (var item in sublist)
                    result.Add(item);

            //Assert
            Assert.AreEqual(original.Length, result.Count);

            for (var i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], result[i]);
            }
        }
    }
}
