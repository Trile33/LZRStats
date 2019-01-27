using LZRStats.DocumentExtractor;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;

namespace LZRStats.Controllers
{
    public class ImportController : Controller
    {
        protected HtmlInputFile file;
        // GET: PDFImport
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/PDFFiles"), fileName);
                file.SaveAs(path);

                var errors = DocExtractor.ExtractFromFile(path, fileName);
                ViewBag.Message = $"File {fileName} imported successfully!";

                return View("Index");
            }
            // redirect back to the index action to show the form once again
            ViewBag.Message = "File import failed!";

            return View("Index");
        }
    }
}