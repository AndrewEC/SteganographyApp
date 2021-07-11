namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class IndexGeneratorTest
    {
        [Test]
        public void TestDifferentSeedsDontMatch()
        {
            var generator = new IndexGenerator(100, 5);
            int[] nums = Generate(generator, 20, 20);

            generator = new IndexGenerator(101, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsFalse(HaveSameElements(nums, nums2));
        }

        [Test]
        public void TestDifferentGenerationsDontMatch()
        {
            var generator = new IndexGenerator(100, 6);
            int[] nums = Generate(generator, 20, 20);

            generator = new IndexGenerator(101, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsFalse(HaveSameElements(nums, nums2));
        }

        [Test]
        public void TestSameSeedsMatch()
        {
            var generator = new IndexGenerator(100, 5);
            int[] nums = Generate(generator, 20, 20);

            generator = new IndexGenerator(100, 5);
            int[] nums2 = Generate(generator, 20, 20);

            Assert.IsTrue(AreInRange(nums, 0, 20));
            Assert.IsTrue(AreInRange(nums2, 0, 20));
            Assert.IsTrue(HaveSameElements(nums, nums2));
        }

        private bool HaveSameElements(int[] first, int[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }
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

        private int[] Generate(IndexGenerator generator, int length, int max)
        {
            int[] nums = new int[length];
            int count = 0;
            while (count < length)
            {
                nums[count++] = generator.Next(max);
            }
            return nums;
        }
    }
}
