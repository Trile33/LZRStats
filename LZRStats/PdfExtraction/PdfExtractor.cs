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

        public static string ExtractFromPdf(string filePath)
        {
            filePath = "C:\\Users\\aleksandart\\Downloads\\05-02-skywalkers.pdf";
            PDDocument doc = PDDocument.load(filePath);
            PDFTextStripper stripper = new PDFTextStripper();
            var textFromFile = stripper.getText(doc);
            return textFromFile;
        }
    }
}