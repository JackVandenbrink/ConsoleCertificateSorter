using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using Salaros.Configuration;
using Salaros;

using System.IO;

namespace Utils
{
	class ConfigWrapper
	{
		// Config Filename
		private const string CONFIG_NAME = "config.cfg";

		// Config keys
		private const string INPUT_DIRECTORY_KEY = "Input_Directory";
		private const string OUTPUT_DIRECTORY_KEY = "Output_Directory";
		private const string ERROR_DIRECTORY_KEY = "Error_Directory";
		private const string MOVE_INSTEAD_OF_COPY_KEY = "Move_Instead_Of_Copy";

		// Config default values
		private const string INPUT_DIRECTORY_VALUE = "Input\\";
		private const string OUTPUT_DIRECTORY_VALUE = "Output\\";
		private const string ERROR_DIRECTORY_VALUE = "Error\\";
		private const string MOVE_INSTEAD_OF_COPY_VALUE = "false";

		private static string mExePath;
		private static string mPath;
		private ConfigParser mConfigParser;


		public ConfigWrapper(string _path)
		{
			mExePath = _path;
			mPath = mExePath + "\\" + CONFIG_NAME;
			//
			if (!File.Exists(mPath))
			{
				Console.WriteLine("No configuration file found at:\t" + mPath);
				Console.WriteLine("Creating template configuration file and directories.\n");
				CreateEmptyConfigurationFile(mPath);
			}

			Console.WriteLine("Loaded Config from: " + mPath);

			mConfigParser = new ConfigParser(mPath);
		}

		public string GetInputDirectory()
		{
			string outString = null;

			GetStringValue(INPUT_DIRECTORY_KEY, ref outString);

			return outString;

		}

		public string GetOutputDirectory()
		{
			string outString = null;

			GetStringValue(OUTPUT_DIRECTORY_KEY, ref outString);

			return outString;
		}

		public string GetErrorDirectory()
		{
			string outString = null;

			GetStringValue(ERROR_DIRECTORY_KEY, ref outString);

			return outString;
		}


		public bool GetStringValue(string _key, ref string _outString)
		{
			_outString = mConfigParser.GetValue("Strings", _key);
			return _outString != null;
		}

		static void CreateEmptyConfigurationFile(string _configPath)
		{
			string[] lines =
			{
				"[Strings]",				
				INPUT_DIRECTORY_KEY + "=" + mExePath + "\\" + INPUT_DIRECTORY_VALUE,
				OUTPUT_DIRECTORY_KEY + "=" + mExePath + "\\" + OUTPUT_DIRECTORY_VALUE,
				ERROR_DIRECTORY_KEY + "=" + mExePath + "\\" + ERROR_DIRECTORY_VALUE,
				"",
				"[Boolean]",
				"# If true, files will be deleted from the input folder as they are processed.",
				MOVE_INSTEAD_OF_COPY_KEY + "=" + MOVE_INSTEAD_OF_COPY_VALUE

			};

			StreamWriter textFile = File.CreateText(_configPath);
			foreach (string line in lines)
			{
				textFile.WriteLine(line);
			}
			textFile.Close();


			Directory.CreateDirectory(mExePath + "\\" + INPUT_DIRECTORY_VALUE);
			Directory.CreateDirectory(mExePath + "\\" + OUTPUT_DIRECTORY_VALUE);
		}

	}
}
