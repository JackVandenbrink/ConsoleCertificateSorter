using System;
using System.Collections.Generic;
using System.Text;

//iText 7
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;


using System.IO;


namespace Utils
{
	class PDFWrapper
	{
		public string mPath { get; private set; }
		private PdfDocument mPDFDoc;
		private string mPDFText;

		private string mDesiredFilePath;

		private string mReleaseYear;
		private string mLotNumber;
		private string mProductCode;
		private string mProductName;

		private string CERTIFICATE_VALIDATION_TEXT = "CERTIFICATE OF ANALYSIS";

		private string RELEASE_DATE_KEY = "RELEASE DATE ";
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

			mProductCode = ScanCertificateField(PRODUCT_CODE_KEY, "\n");
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

			mDesiredFilePath = string.Format("{0}/{1}/COA_{2}_{3}.pdf",
												mProductName,mReleaseYear,mProductCode,mLotNumber);

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
}
