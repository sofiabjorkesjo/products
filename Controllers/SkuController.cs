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
        DataBaseModel databaseModel = new DataBaseModel();

        [Route("")]
        [Route("/Sku")]
        public IActionResult Index()
        {
            databaseModel.createDB();
            List<string> catalogEntryCodes = databaseModel.getCatalogEntryCodesFromDB();
            ViewData["CatalogEntryCodes"] = catalogEntryCodes;
            return View();
        }
        
        [Route("/Sku/{catalogEntryCode}")]
        public IActionResult Sku(string catalogEntryCode)
        {
            Dictionary<string, List<ProductRow>> list = databaseModel.getValuesFromDB(catalogEntryCode);
            List<string> allMarketId = new List<string>();
        
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
