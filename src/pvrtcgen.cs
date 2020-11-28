using System;

public enum PvrtcType
{
	Opaque2bit = 0,
	Transparent2bit = 1,
	Opaque4bit = 2,
	Transparent4bit = 3,
}

public static class PvrtcGen
{
	public const string pvrtcGenVersionNumber = "1.0";

	// This static class can be used to generate (somewhat) random PVRTC texture byte arrays
	// Outputs data with or without PVR header
	// There are some sanity and safety checks used for parameters
	// This code only works on little endian platforms!

	private static int randomSeed = 0;
	
	// Used only for testing purposes
	public static void InitRandomSeed(int newSeedValue)
	{
		randomSeed = newSeedValue;
	}

	public static byte[] GeneratePvrtcByteArray(int sideSize, PvrtcType pvrtcType, bool includeHeader = true)
	{
		return GeneratePvrtcByteArray(sideSize, sideSize, pvrtcType, includeHeader);
	}

	public static byte[] GeneratePvrtcByteArray(int width, int height, PvrtcType pvrtcType, bool includeHeader = true)
	{
		byte[] returnArray = null;

		if (!CheckThatWidthOrHeightIsValid(width) || !CheckThatWidthOrHeightIsValid(height))
		{
			// Incorrect resolution
			return returnArray;
		}

		byte[] header = new byte[0];
		
		if (includeHeader)
		{
			header = PvrtcGen.GeneratePvrFileFormatHeader(width, height, pvrtcType);
		}

		int amountOfBlocks = 0;

		if (PvrtcType.Opaque2bit == pvrtcType || PvrtcType.Transparent2bit == pvrtcType)
		{
			// every 8x4 pixel section is a block
			amountOfBlocks = width / 8 * height / 4;
		}
		else if (PvrtcType.Opaque4bit == pvrtcType || PvrtcType.Transparent4bit == pvrtcType)
		{
			// every 4x4 pixel section is a block
			amountOfBlocks = width / 4 * height / 4;
		}

		// Every block requires 8 bytes of data
		int bytesForTextureData = amountOfBlocks * 8;

		// Allocate memory for header + texture data, nothing for metadata
		returnArray = new byte[header.Length + bytesForTextureData];

		// Copy Header to returnArray
		Buffer.BlockCopy(header, 0, returnArray, 0, header.Length);

		// Here we would write metadata, but currently it is unimplemented
		// GeneratePvrMetaData()

		// Fill the texture data
		PvrtcGen.FillRestOfArrayWithRandomBytes(returnArray, header.Length, pvrtcType);

		return returnArray;
	}

	private static byte[] GeneratePvrFileFormatHeader(int wantedWidth, int wantedHeight, PvrtcType pvrtcType)
	{
		// Currently only v3.0.0 headers are supported, their size is ALWAYS 52 bytes
		// See http://cdn.imgtec.com/sdk-documentation/PVR+File+Format.Specification.pdf

		// Version, just static number
		uint version = 0x03525650; // 4 bytes

		// Flags, currently means pre-multiplied with alpha
		uint flags = 0; // 4 bytes

		// Pixel Format, we only support 4 predefined values
		ulong pixelFormat = (ulong)pvrtcType; // 8 bytes

		// Colo(u)r Space, options are linear RGB (0) or sRGB (1)
		uint colorSpace = 0; // 4 bytes

		// Channel Type, 
		uint channelType = 0; // 4 bytes

		// Height, in pixels 
		uint height = (uint)wantedHeight; // 4 bytes

		// Width, in pixels 
		uint width = (uint)wantedWidth; // 4 bytes

		// Depth, depth of 3D texture in pixels, value 1 is used since we only do 2d textures
		uint depth = 1; // 4 bytes

		// Number of surfaces, value 1 is used since we only do 2d textures
		uint numberOfSurfaces = 1; // 4 bytes

		// Number of faces, value 1 is used since we only do 2d textures
		uint numberOfFaces = 1; // 4 bytes

		// MIP-Map Count, number of mip map levels, always 1 in this case since mipmap generation is NOT supported
		uint mipmapLevel = 1; // 4 bytes

		// Meta Data Size, always 0 since no metadata is generated
		uint metaDataSize = 0; // 4 bytes

		byte[] returnArray = new byte[52];

		PvrtcGen.WriteValuesToByteArray(returnArray, version, flags, pixelFormat, colorSpace, channelType, height, width, depth, numberOfSurfaces, numberOfFaces, mipmapLevel, metaDataSize);

		return returnArray;
	}

	private static void WriteValuesToByteArray(byte[] byteArray, params object[] numberArray)
	{
		int currentPos = 0;

		byte[] numberAsBytes = null;
		int sizeOfUint = sizeof(uint);
		int sizeOfUlong = sizeof(ulong);

		for (int i = 0; i < numberArray.Length; i++)
		{
			if (numberArray[i].GetType() == typeof(uint))
			{
				numberAsBytes = BitConverter.GetBytes((uint)numberArray[i]);

				Buffer.BlockCopy(numberAsBytes, 0, byteArray, currentPos, sizeOfUint);
				currentPos += sizeOfUint;
			}
			else if (numberArray[i].GetType() == typeof(ulong))
			{
				numberAsBytes = BitConverter.GetBytes((ulong)numberArray[i]);

				Buffer.BlockCopy(numberAsBytes, 0, byteArray, currentPos, sizeOfUlong);
				currentPos += sizeOfUlong;
			}
		}
	}

	private static byte[] GeneratePvrMetaData()
	{
		// Currently unused since this does NOT have any usage for random textures
		byte[] returnArray = new byte[0];
		return returnArray;
	}

	private static void FillRestOfArrayWithRandomBytes(byte[] byteArray, int startPos, PvrtcType pvrtcType)
	{
		Random rand = new Random();

		if (randomSeed != 0)
		{
			// Use chosen seed if required
			rand = new Random(randomSeed);
		}

		int sizeOfRandomByteArray = 16; // 16 bytes is two blocks, just go with it since we ALWAYS have at least 2 blocks

		byte[] randomGeneratedByteArray = new byte[sizeOfRandomByteArray]; 

		int currentPos = startPos;
		int endPos = byteArray.Length;

		while (currentPos < endPos)
		{
			// All byte combinations are valid for PVRTC
			rand.NextBytes(randomGeneratedByteArray);

			// But with Opaque ones we want to force opaque flags so that output only contains opaque alpha
			if (PvrtcType.Opaque2bit == pvrtcType || PvrtcType.Opaque4bit == pvrtcType)
			{
				PvrtcGen.TurnRandomByteArrayIntoOpaque(randomGeneratedByteArray);
			}

			Buffer.BlockCopy(randomGeneratedByteArray, 0, byteArray, currentPos, sizeOfRandomByteArray);
			currentPos += sizeOfRandomByteArray;
		}
	}

	private static void TurnRandomByteArrayIntoOpaque(byte[] randomGeneratedByteArray)
	{
		byte modeMask = 254; 	// 1111 1110, because we want to set mode bit to 0
		byte colorMask = 128; 	// 1000 0000, because we want to set alpha bit to 0
		// For every 8 bytes we want to edit three bits (two color bits and one mode bit)
		for (int i = 0; i < randomGeneratedByteArray.Length; i += 8)
		{
			randomGeneratedByteArray[i+4] &= modeMask; // Mode
			randomGeneratedByteArray[i+5] |= colorMask; // Color B
			randomGeneratedByteArray[i+7] |= colorMask; // Color A
		}
	}

	private static bool CheckThatWidthOrHeightIsValid(int widthOrHeight)
	{
		// Only sizes between [8 4096] are supported, must also be power of two 
		if (widthOrHeight < 8 || widthOrHeight > 4096)
		{
			return false;
		}

		if (!PvrtcGen.IsPowerOfTwo((ulong)widthOrHeight))
		{
			return false;
		}

		return true;
	}

	private static bool IsPowerOfTwo(ulong x)
	{
		return (x & (x - 1)) == 0;
	}
}