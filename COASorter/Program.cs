using System;
using System.IO;
using System.Reflection;
using Salaros.Configuration;

namespace COASorter
{



	class Program
	{

		const string CONFIG_NAME = "config.cfg";

		static void Main(string[] args)
		{
			string execPath = AppDomain.CurrentDomain.BaseDirectory;
			string configPath = execPath + CONFIG_NAME;
			ConfigParser configFile;

			try
			{
				configFile = new ConfigParser(configPath, CreateParserSettings());
			}
			catch (Salaros.Configuration.ConfigParserException)
			{
				Console.WriteLine("Config file values are missing");
				Console.WriteLine("[KEY=VALUE]");
				Console.WriteLine("Values are missing.");
				Console.ReadKey(true);
				return;
			}

			
			if (configFile.Lines.Count == 0)
			{
				Console.WriteLine("No configuration lines were found\n Creating empty configuration file");
				CreateEmptyConfigurationFile(configPath);
				Console.ReadKey(true);
				return;
			}
			
			Console.WriteLine("Hello World!");
		}

		static ConfigParserSettings CreateParserSettings()
		{
			string[] commentChars = { "#" };
			string keyValueSeparator = "=";
			

			return new ConfigParserSettings {CommentCharacters = commentChars, KeyValueSeparator = keyValueSeparator };
		}

		static void CreateEmptyConfigurationFile(string _configPath)
		{
			string[] lines =
			{
				"# Configuration File",
				"InputDirectory=",
				"OutputDirectory="
			};

			StreamWriter textFile = File.CreateText(_configPath);
			foreach (string line in lines)
			{
				textFile.WriteLine(line);
			}
			textFile.Close();
		}
	}

}


