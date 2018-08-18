using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using products.Models;


namespace products.Controllers
{
    public class SkuController : Controller
    {
        public IActionResult Index()
        {
            var databaseModel = new DataBaseModel();
            //databaseModel.createDbAndInsertValues();
            var catalogEntryCodes = databaseModel.getCatalogEntryCodeFromDB();
            ViewData["CatalogEntryCodes"] = catalogEntryCodes;
            return View();
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}