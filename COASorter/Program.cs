using System;
using System.IO;
using System.Reflection;
using Salaros.Configuration;
using Salaros;


//iText 7
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;


using System.Collections.Generic;

namespace COASorter
{



	class Program
	{

		// Config filename
		const string CONFIG_NAME = "config.cfg";



		static void Main(string[] args)
		{
			string configurationPath = AppDomain.CurrentDomain.BaseDirectory + CONFIG_NAME;
			ConfigWrapper configuration = new ConfigWrapper(configurationPath);

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
					// PDF has been verified now find appropriate fields to rename the file and move it

					Console.WriteLine("Scanned File: " + pdf.ToString());

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

		private string mFolder;
		private string mFileName;

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
			//"CERTIFICATE OF ANALYSIS\nPage 1 of   1\nPRODUCT TM0268\nGC SUGAR SET\nLOT NUMBER 1437598\nEXPIRY DATE 2017.04.05\n
			//"CERTIFICATE OF ANALYSIS\nDelivery/Customer information\nDate Printed (dd.mm.yyyy)\nPRODUCT MP1350\n20.09.2017\nMUELLER HINTON AGAR\nDelivery No.\nCustomer \nLOT NUMBER 1912183\nCustomer Order number\nEXPIRY DATE 2016.11.01\n



			return true;
		}
	}

	class ConfigWrapper
	{

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


		private string mPath;
		private ConfigParser mConfigParser;


		public ConfigWrapper(string _path)
		{
			mPath = _path;

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
				INPUT_DIRECTORY_KEY + "=" + AppDomain.CurrentDomain.BaseDirectory + INPUT_DIRECTORY_VALUE,
				OUTPUT_DIRECTORY_KEY + "=" + AppDomain.CurrentDomain.BaseDirectory + OUTPUT_DIRECTORY_VALUE,
				ERROR_DIRECTORY_KEY + "=" + AppDomain.CurrentDomain.BaseDirectory + ERROR_DIRECTORY_VALUE,
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


			Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + INPUT_DIRECTORY_VALUE);
			Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + OUTPUT_DIRECTORY_VALUE);
		}

	}

}


