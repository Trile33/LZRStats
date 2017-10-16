using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LZRStats.PdfExtraction
{
    public class PdfExtractor
    {

        public static string ExtractFromPdf(string PDFFilePath)
        {
            PDDocument doc = PDDocument.load(PDFFilePath);
            PDFTextStripper stripper = new PDFTextStripper();
            return stripper.getText(doc);
        }
    }
}