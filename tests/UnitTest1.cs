using NUnit.Framework;

namespace tests
{
	public class PvrtcGenTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void GenerationTest()
		{
			// Arrange
			PvrtcGen.InitRandomSeed(1337 /* Magic value*/);
			int width = 256;
			int height = 256;
			PvrtcType pvrtcType = PvrtcType.Transparent4bit;
			bool generateHeader = true;

			byte[] pvrtcBytes = PvrtcGen.GeneratePvrtcByteArray(width, height, pvrtcType, generateHeader);

			// Act

			// Assert
			Assert.AreEqual(256 * 256 / 2 + 52, pvrtcBytes.Length);
		}
	}
}