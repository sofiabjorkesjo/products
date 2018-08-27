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
            List<string> allMarketId = databaseModel.getAllMarketId(catalogEntryCode);
            Dictionary<string, List<ProductRow>> dictonary = productModel.createDictionary(allMarketId);
            List<List<string>> matchingValues =  databaseModel.getMatchingValues(catalogEntryCode);
            Dictionary<string, List<ProductRow>> dictonaryList = productModel.addValuesToListList(matchingValues, dictonary);
            Dictionary<string, List<ProductRow>> sortDictionary = productModel.sortDictionary(dictonaryList);
            Dictionary<string, List<ProductRow>> orderdDictionary = productModel.setProductsList(sortDictionary, allMarketId);
        
            foreach(KeyValuePair<string, List<ProductRow>> entry in orderdDictionary)
            {
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
