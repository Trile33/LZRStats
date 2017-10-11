using LZRStats.DAL;
using LZRStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LZRStats.Controllers
{
    public class PlayerController : Controller
    {
        DatabaseContext db = new DatabaseContext();
        // GET: Players
        public ActionResult Index()
        {
            return View(db.Players.ToList());
        }
    }
}