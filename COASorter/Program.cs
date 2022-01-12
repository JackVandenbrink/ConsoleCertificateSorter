using System;
using System.IO;
using System.Reflection;
using Salaros.Configuration;
using Salaros;


//iText 7
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

using System.Diagnostics;
using System.Collections.Generic;

namespace COASorter
{



	class Program
	{

		



		static void Main(string[] args)
		{
			string executableDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

			Console.WriteLine("Looking for configuration file in: " + executableDirectory);

			ConfigWrapper configuration = new ConfigWrapper(executableDirectory);

			string[] inputFiles = Directory.GetFiles(configuration.GetInputDirectory(),"",SearchOption.AllDirectories);

			List<PDFWrapper> pdfInputFiles = new List<PDFWrapper>();

			foreach (string file in inputFiles)
			{
				if (file.Contains(".pdf"))
				{
					pdfInputFiles.Add(new PDFWrapper(file));
				}
			}

			foreach(PDFWrapper pdf in pdfInputFiles)
			{
				if (pdf.Scan())
				{
					// Log what will be changed
					Console.WriteLine("File: " + pdf.ToString() + " Copied to: " + pdf.GetDesiredPathAppend());

					// Create string for output path
					string outputPath = configuration.GetOutputDirectory() + pdf.GetDesiredPathAppend();

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

					string outputPath = configuration.GetErrorDirectory() + Path.GetFileName(pdf.GetSourcePath());
					Directory.CreateDirectory(configuration.GetErrorDirectory());
					File.Copy(pdf.GetSourcePath(), outputPath,false);
				}
			}

			// Using this to break and check values
			Console.ReadKey(true);
		}
	}


	class PDFWrapper
	{
		private string mPath;
		private PdfDocument mPDFDoc;
		private string mPDFText;

		private string mDesiredFilePath;

		private string mReleaseYear;
		private string mLotNumber;
		private string mProductCode;
		private string mProductName;

		private string CERTIFICATE_VALIDATION_TEXT = "CERTIFICATE OF ANALYSIS";

		private string RELEASE_DATE_KEY = "RELEASE DATE ";
		private string EXPIRY_DATE_KEY = "EXPIRY DATE ";
		private string PRODUCT_CODE_KEY = "PRODUCT ";
		private string LOT_NUMBER_KEY = "LOT NUMBER ";


		public PDFWrapper(string _path)
		{
			mPath = _path;
		}

		public override string ToString()
		{
			return Path.GetFileName(mPath);
		}

		public string GetSourcePath()
		{
			return mPath;
		}

		public string GetDesiredPathAppend()
		{
			return mDesiredFilePath;
		}
		public bool Scan()
		{
			mPDFDoc = new PdfDocument(new PdfReader(mPath));
			mPDFText = PdfTextExtractor.GetTextFromPage(mPDFDoc.GetFirstPage());

			// Close the file as the text has now been read
			mPDFDoc.Close();

			return CollectData();
		}

		private bool CollectData()
		{

			// Example of the raw text contained within this file
			//"CERTIFICATE OF ANALYSIS\nPage 1 of   1\nPRODUCT PP2016\nMACCONKEY NO SALT\nLOT NUMBER 4308074\nEXPIRY DATE 2021.06.26\nRELEASE DATE 2021.04.06\n
			// If text does not follow this style it will be moved to error folder and handled manually

			if (!mPDFText.Contains(CERTIFICATE_VALIDATION_TEXT))
				return false;

			mProductCode = ScanCertificateField(PRODUCT_CODE_KEY,"\n");
			if (mProductCode == null)
				return false;

			mProductName = ScanCertificateField(mProductCode + "\n", "\n");
			if (mProductName == null)
				return false;

			mLotNumber = ScanCertificateField(LOT_NUMBER_KEY, "\n");
			if (mLotNumber == null)
				return false;


			mReleaseYear = ScanCertificateField(RELEASE_DATE_KEY, "\n");
			if (mReleaseYear == null)
				return false;

			mReleaseYear = mReleaseYear.Substring(0, 4);

			// Clean product name from forward slashes
			mProductName = mProductName.Replace("/", "-");

			// Create desired file path

			mDesiredFilePath = "/" + mProductName + "/" + mReleaseYear + "/" + "COA_" + mProductCode + "_" + mLotNumber + ".pdf";
			
			return true;
		}

		private string ScanCertificateField(string _keyStart, string _keyEnd)
		{
			int subStrMin = 0;
			int subStrMax = 0;

			subStrMin = mPDFText.IndexOf(_keyStart);

			if (subStrMin == -1)
				return null;

			subStrMin += _keyStart.Length;

			subStrMax = mPDFText.IndexOf(_keyEnd, subStrMin);

			if (subStrMax == -1)
				return null;

			return mPDFText.Substring(subStrMin, subStrMax - subStrMin);
		}
	}

	class ConfigWrapper
	{
		// Config Filename
		const string CONFIG_NAME = "config.cfg";

		// Config defaults
		const string CONFIG_FILE_HEADER_STRING = "#Configuration File";

		// Config keys
		const string INPUT_DIRECTORY_KEY = "Input_Directory";
		const string OUTPUT_DIRECTORY_KEY = "Output_Directory";
		const string ERROR_DIRECTORY_KEY = "Error_Directory";
		const string MOVE_INSTEAD_OF_COPY_KEY = "Move_Instead_Of_Copy";

		// Config default values
		const string INPUT_DIRECTORY_VALUE = "Input\\";
		const string OUTPUT_DIRECTORY_VALUE = "Output\\";
		const string ERROR_DIRECTORY_VALUE = "Error\\";
		const string MOVE_INSTEAD_OF_COPY_VALUE = "false";

		private static string mExePath;
		private static string mPath;
		private ConfigParser mConfigParser;


		public ConfigWrapper(string _path)
		{
			mExePath = _path;
			mPath = mExePath + "\\" + CONFIG_NAME;
			//
			if (!File.Exists(_path))
			{
				CreateEmptyConfigurationFile(mPath);				
			}


			mConfigParser = new ConfigParser(mPath);
		}

		public string GetInputDirectory()
		{
			string outString = null;

			GetStringValue(INPUT_DIRECTORY_KEY,ref outString);
			
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


