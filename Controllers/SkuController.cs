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
        [Route("")]
        [Route("/Sku")]
        public IActionResult Index()
        {
            var databaseModel = new DataBaseModel();
            //databaseModel.createDbAndInsertValues();
            var catalogEntryCodes = databaseModel.getCatalogEntryCodesFromDB();
            ViewData["CatalogEntryCodes"] = catalogEntryCodes;
            return View();
        }
        
        [Route("/Sku/{catalogEntryCode}")]
        public IActionResult Sku(string catalogEntryCode)
        {
            var databaseModel = new DataBaseModel();
            var list = databaseModel.getValuesFromDB(catalogEntryCode);
            var allMarketId = new List<string>();
        
            foreach(KeyValuePair<string, List<ProductRow>> entry in list)
            {
                allMarketId.Add(entry.Key);
                ViewData[entry.Key] = entry.Value; 
            }
            
            ViewData["allMarketId"] = allMarketId;
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
