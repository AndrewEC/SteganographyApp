﻿namespace SteganographyApp.Common.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class IndexGeneratorTest
    {
        [Test]
        public void TestDifferentSeedsDontMatch()
        {
            var generator = new Xor128Prng(100, 5);
            int[] nums = Generate(generator, 20, 20);

            generator = new Xor128Prng(101, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsFalse(HaveSameElements(nums, nums2));
        }

        [Test]
        public void TestDifferentGenerationsDontMatch()
        {
            var generator = new Xor128Prng(100, 6);
            int[] nums = Generate(generator, 20, 20);

            generator = new Xor128Prng(101, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsFalse(HaveSameElements(nums, nums2));
        }

        [Test]
        public void TestSameSeedsMatch()
        {
            var generator = new Xor128Prng(100, 5);
            int[] nums = Generate(generator, 20, 20);

            generator = new Xor128Prng(100, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsTrue(HaveSameElements(nums, nums2));
        }

        private bool HaveSameElements(int[] first, int[] second)
        {
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreInRange(int[] arr, int min, int max)
        {
            foreach (int item in arr)
            {
                if (item < min || item > max)
                {
                    return false;
                }
            }
            return true;
        }

        private int[] Generate(Xor128Prng generator, int length, int max) => Enumerable.Range(0, length)
            .Select(i => generator.Next(max)).ToArray();
    }
}
