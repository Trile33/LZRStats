﻿using LZRStats.PdfExtraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;

namespace LZRStats.Controllers
{
    public class PDFImportController : Controller
    {
        protected HtmlInputFile file;
        // GET: PDFImport
        public ActionResult PDFImport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PDFImport(HttpPostedFileBase file)
        {
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                PdfExtractor.ExtractFromPdf(path);
                //file.SaveAs(path);
            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("PDFImport");
        }
    }
}