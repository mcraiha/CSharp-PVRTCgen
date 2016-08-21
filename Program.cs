using System;
using System.IO;
using System.Collections.Generic;

namespace ConsoleApplication
{
	public class Program
	{
		public const string programVersion = "1.0";

		// Default values
		private static int width = 8;
		private static int height = 8;
		private static PvrtcType pvrtcType = PvrtcType.Transparent4bit;
		private static bool generateHeader = true;
		private static string outputFilename = null;

		// Error handling
		private static bool errorInParsingParameters = false;
		private static string lastError = "";

		private static Dictionary<string /* Key */, Tuple<string /* Help text */, Action /* Action */>> simpleOptions = new Dictionary<string /* Key */, Tuple<string /* Help text */, Action /* Action */>>()
		{
			{ "-help",      new Tuple<string, Action>("Prints this help",           				PrintHelp) },
			{ "-version",   new Tuple<string, Action>("Prints version numbers",      				PrintVersion) },
			{ "-test",      new Tuple<string, Action>("Outputs test PVRTC file to output stream", 	RunTest) },
		};

		private static Dictionary<string /* Key */, Tuple<string /* Help text */, Action<string> /* Action with parameter */>> normalOptions = new Dictionary<string /* Key */, Tuple<string /* Help text */, Action<string> /* Action with parameter */>>()
		{
			{ "-resolution", new Tuple<string, Action<string>>("Set resolution, e.g. 512 or 512x256. Values must be Power of Two.", SetResolution) },
			{ "-format", new Tuple<string, Action<string>>("Set format, valid values are Opaque2bit, Opaque4bit, Transparent2bit and Transparent4bit. Default value is Transparent4bit.", SetFormat)},
			{ "-header", new Tuple<string, Action<string>>("Generate header, valid values are yes, no, true, false. Default value is yes.", SetHeader)},
			{ "-file", new Tuple<string, Action<string>>("Filename (and path) for generated texture file.", SetFilename)},
		};

		public static void Main(string[] args)
		{
			// Check that there is at least one paramater
			if (args.Length < 1)
			{
				Console.WriteLine("You have to give parameters! (use -help to show possible parameters)");
				return;
			}		

			// Parse commandline/terminal commands, check simple options first
			Action simpleCommand = null;

			for (int i = 0; i < args.Length; i++)
			{
				// Check if simpleoptions includes the parameter
				if (simpleOptions.ContainsKey(args[i]))
				{
					// Since it contains the parameter, do the simple command from it
					simpleCommand = simpleOptions[args[i]].Item2;
					break;
				}
			}

			if (simpleCommand != null)
			{
				// With simple command we just execute that command and exit out
				simpleCommand.Invoke();
				return;
			}

			// If there were no simple options, do more complex stuff next
			for (int i = 0; i < args.Length; i++)
			{
				// Check if normalOptions includes the parameter
				if (normalOptions.ContainsKey(args[i]))
				{
					// Check that there is at least one more parameter
					if (i < (args.Length - 1))
					{
						normalOptions[args[i]].Item2(args[i + 1]);
					}
					else
					{
						// Complain about missing parameter and exit
						Console.WriteLine("Missing parameter for " + args[i]);
						return;
					}

					// Check after every loop that given parameter was valid
					if (errorInParsingParameters)
					{
						// Complain about error and exit
						Console.WriteLine(lastError);
						return;
					}
				}
			}

			// Do actual generate since everything seems to be OK
			if (outputFilename != null)
			{
				byte[] pvrtcBytes = PvrtcGen.GeneratePvrtcByteArray(width, height, pvrtcType, generateHeader);

				// Write file if we have something to write
				if (pvrtcBytes != null)
				{
					File.WriteAllBytes(outputFilename, pvrtcBytes);
				}
				else
				{
					Console.WriteLine("Could not generate PVRTC data, most likely an incorrect resolution value");
				}
			}
			else
			{
				Console.WriteLine("Missing -file parameter");
			}
		}


		#region Simple commands

		private static void PrintHelp()
		{
			// Print Simple options first
			foreach(var pair in simpleOptions)
			{
				Console.WriteLine(pair.Key + "\t\t" + pair.Value.Item1);
			}

			// Then print normal options
			foreach(var pair in normalOptions)
			{
				Console.WriteLine(pair.Key + "\t\t" + pair.Value.Item1);
			}
		}

		private static void PrintVersion()
		{
			Console.WriteLine("Program version: " + programVersion);
			Console.WriteLine("PvrtcGen version: " + PvrtcGen.pvrtcGenVersionNumber);
		}

		private static void RunTest()
		{
			// Generates output with following settings:
			PvrtcGen.InitRandomSeed(1337 /* Magic value*/);
			Program.width = 256;
			Program.height = 256;
			Program.pvrtcType = PvrtcType.Transparent4bit;
			Program.generateHeader = true;
		}

		#endregion // Simple commands


		#region Normal commands

		private static void SetResolution(string resolution)
		{
			string resolutionLower = resolution.ToLower();

			char splitChar = 'x';

			// Check if we two part (e.g. 512x256) resolution or one part
			if (resolutionLower.Contains(splitChar.ToString()))
			{
				// Two part

				// Split first
				string[] splitted = resolutionLower.Split(splitChar);
				if (splitted.Length == 2)
				{
					// Check that splitted values are valid
					int parsedWidth = 0;
					int parsedHeight = 0;

					if (int.TryParse(splitted[0], out parsedWidth) && int.TryParse(splitted[0], out parsedHeight))
					{
						// Valid result, set both width and height
						Program.width = parsedWidth;
						Program.height = parsedHeight;
						return;
					}
				}
			}
			else
			{
				// One part

				// Try to parse a int
				int parsedInt = 0;
				if (int.TryParse(resolutionLower, out parsedInt))
				{
					// Valid result, set it to both width and height
					Program.width = parsedInt;
					Program.height = parsedInt;
					return;
				}
			}

			// Error out
			Program.errorInParsingParameters = true;
			Program.lastError = "Cannot convert " + resolution + " to resolution!";
			return;
		}

		private static void SetFormat(string format)
		{
			PvrtcType possibleValue;
			// Check if format matches any enum value
			{
				Program.pvrtcType = possibleValue;
			}
			else
			{
				// Error out
				Program.errorInParsingParameters = true;
				Program.lastError = "Cannot convert " + format + " to texture format!";
			}
		}

		private static void SetHeader(string header)
		{
			if ("true" == headerToLower || "yes" == headerToLower)
			{
				Program.generateHeader = true;
			}
			else if ("false" == headerToLower || "no" == headerToLower)
			{
				Program.generateHeader = false;
			}
			else
			{
				// Error out
				Program.errorInParsingParameters = true;
				Program.lastError = header + " is NOT yes, no, true, false value!";
			}
		}

		private static void SetFilename(string filename)
		{
			// Check that path is valid
			string fullPath = "";

			try
			{
				fullPath = Path.GetFullPath(filename);
			}
			catch(Exception e)
			{
				// Error out
				Program.errorInParsingParameters = true;
				Program.lastError = "Error with filename " + filename + " is " + e.ToString();
				return;
			}

			// Check if file already exists (we do NOT overwrite any files!)
			if (File.Exists(fullPath))
			{
				// Error out
				Program.errorInParsingParameters = true;
				Program.lastError = "Can NOT overwrite file " + filename;
				return;
			}

			Program.outputFilename = fullPath;
		}

		#endregion // Normal commands
	}
}
