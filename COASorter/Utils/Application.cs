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
		private string ExecutablePath { get; }
		private ConfigWrapper Configuration { get; }
		private List<PDFWrapper> PDFFiles { get; }
		private int CounterCopies { get; set; }
		private int CounterDuplicates { get; set; }
		private int CounterErrors { get; set; }
		public Application()
		{
			ExecutablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			Configuration = new ConfigWrapper(ExecutablePath);
			PDFFiles = new List<PDFWrapper>();

			CounterCopies = 0;
			CounterDuplicates = 0;
			CounterErrors = 0;
		}
		public void Run()
		{


			Console.WriteLine("\n\nSearching for input files. . .");
			try
			{
				string[] inputFiles = Directory.GetFiles(Configuration.GetInputDirectory(), "", SearchOption.AllDirectories);
				foreach (string file in inputFiles)
				{
					if (file.Contains(".pdf"))
					{
						PDFFiles.Add(new PDFWrapper(file));
					}
				}
			}
			catch (System.ArgumentNullException)
			{
				Console.WriteLine("Exception Encountered: ArgumentNullException\nPlease check Input directory");
				QuitApplication();
				return;
			}
			catch (System.IO.DirectoryNotFoundException)
			{
				Console.WriteLine("Exception Encountered: DirectoryNotFoundException\nInput directory does not exist");
				QuitApplication();
				return;
			}

			//



			Console.WriteLine("PDF files found: " + PDFFiles.Count);

			Console.WriteLine("\n\nPress any key to begin sorting.");
			Console.WriteLine("\nYou can abort this procedure at any time by holding the 'Q' key.");
			Console.ReadKey(true);

			foreach (PDFWrapper pdf in PDFFiles)
			{
				// Check if abort key is pressed

				if (Console.KeyAvailable)
				{
					if (Console.ReadKey(true).Key == ConsoleKey.Q)
					{
						Console.WriteLine("\nProcess Aborted.");
						break;
					}
				}	

				if (pdf.Scan())
				{
					// Log what will be changed
					Console.WriteLine("File: " + pdf.ToString() + "  \tCopied to: " + pdf.GetDesiredPathAppend());

					// Create string for output path
					string outputPath = Configuration.GetOutputDirectory() + pdf.GetDesiredPathAppend();

					//Create directory to hold the file if it does not exist already
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

					// Check to make sure file doesnt already exist before copying it
					if (!File.Exists(outputPath))
					{
						File.Copy(pdf.GetSourcePath(), outputPath, false);
						CounterCopies++;
					}
					else
					{
						CounterDuplicates++;
					}

				}
				else
				{
					// Scan returned an error.
					// Copy this file to the error folder

					Console.WriteLine("File: " + pdf.ToString() + "\t Error: Unexpected contents. Moving/Copying to error directory.");

					string outputPath = Configuration.GetErrorDirectory() + Path.GetFileName(pdf.GetSourcePath());
					Directory.CreateDirectory(Configuration.GetErrorDirectory());
					if (!File.Exists(outputPath))
					{
						File.Copy(pdf.GetSourcePath(), outputPath, false);
						
					}
					CounterErrors++;
				}
			}

			Console.WriteLine("\n\n");
			Console.WriteLine("Finished.\n\n");
			Console.WriteLine("Files Copied/Moved:\t\t" + CounterCopies);
			Console.WriteLine("Duplicates Found:\t\t" + CounterDuplicates);
			Console.WriteLine("Errors Encountered:\t\t" + CounterErrors);

			// Using this to break and check values

			QuitApplication();

		}
	
		private void QuitApplication()
		{
			Console.WriteLine("\n\nPress any key to quit. . .");
			Console.ReadKey(true);
		}
	}
}
