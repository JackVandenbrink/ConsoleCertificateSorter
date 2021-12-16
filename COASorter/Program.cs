using System;
using System.IO;
using System.Reflection;
using Salaros.Configuration;
using Salaros;
using System.Globalization;

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


			configFile = new ConfigParser(configPath, CreateParserSettings());
	
			Console.ReadKey(true);



			
			Console.WriteLine("Hello World!");
		}

		static ConfigParserSettings CreateParserSettings()
		{
			string[] commentChars = { "#" };


			return new ConfigParserSettings {
				CommentCharacters = commentChars,
				MultiLineValues = MultiLineValues.NotAllowed | MultiLineValues.AllowValuelessKeys,
				Culture = new CultureInfo("en-US")
				};
		}

		static void CreateEmptyConfigurationFile(string _configPath)
		{
			string[] lines =
			{
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


