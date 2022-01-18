using System;
using System.Collections.Generic;
using System.Text;

using Utils;
using System.IO;
using System.Diagnostics;

namespace Application
{
	class Application
	{
		public string ExecutablePath { get; }
		public ConfigWrapper ConfigurationFile { get; }


		public Application()
		{
			ExecutablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);



			ConfigurationFile = new ConfigWrapper(ExecutablePath);
		}
		public void Run()
		{
			Console.WriteLine("Loaded Config from: " + ExecutablePath);


			string[] inputFiles = Directory.GetFiles(ConfigurationFile.GetInputDirectory(), "", SearchOption.AllDirectories);

			List<PDFWrapper> pdfInputFiles = new List<PDFWrapper>();

			foreach (string file in inputFiles)
			{
				if (file.Contains(".pdf"))
				{
					pdfInputFiles.Add(new PDFWrapper(file));
				}
			}

			foreach (PDFWrapper pdf in pdfInputFiles)
			{
				if (pdf.Scan())
				{
					// Log what will be changed
					Console.WriteLine("File: " + pdf.ToString() + " Copied to: " + pdf.GetDesiredPathAppend());

					// Create string for output path
					string outputPath = ConfigurationFile.GetOutputDirectory() + pdf.GetDesiredPathAppend();

					//Create directory to hold the file if it does not exist already
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

					// Check to make sure file doesnt already exist before copying it
					if (!File.Exists(outputPath))
					{
						File.Copy(pdf.GetSourcePath(), outputPath, false);
					}

				}
				else
				{
					// Scan returned an error.
					// Copy this file to the error folder

					string outputPath = ConfigurationFile.GetErrorDirectory() + Path.GetFileName(pdf.GetSourcePath());
					Directory.CreateDirectory(ConfigurationFile.GetErrorDirectory());
					File.Copy(pdf.GetSourcePath(), outputPath, false);
				}
			}

			// Using this to break and check values
			Console.ReadKey(true);


		}
	}
}
