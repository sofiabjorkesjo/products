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
        ProductsModel productModel = new ProductsModel();

        [Route("")]
        [Route("/Sku")]
        public IActionResult Index()
        {
            databaseModel.createProductsTable();
            databaseModel.insertValuesToProductsTable("price_detail.csv");
            List<string> catalogEntryCodes = databaseModel.getCatalogEntryCodes();
            ViewData["CatalogEntryCodes"] = catalogEntryCodes;

            return View();
        }
        
        [Route("/Sku/{catalogEntryCode}")]
        public IActionResult Sku(string catalogEntryCode)
        {
            Dictionary<string, List<ProductRow>> list = databaseModel.getProducts(catalogEntryCode);
            List<string> allMarketId = new List<string>();
        
            foreach(KeyValuePair<string, List<ProductRow>> entry in list)
            {
                allMarketId.Add(entry.Key);
                ViewData[entry.Key] = entry.Value; 
            }
            
            ViewData["allMarketId"] = allMarketId;

            // List<string> list2 = databaseModel.getAllMarketId(catalogEntryCode);
            // Dictionary<string, List<ProductRow>> dictonary = productModel.createDictionary(list2);
            // List<List<string>> matchingValues =  databaseModel.getMatchingValues(catalogEntryCode);
            // productModel.addValuesToListList(matchingValues, dictonary);


            return View();


            
           
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
