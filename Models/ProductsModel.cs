using System;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections;

namespace products.Models
{
    public class ProductsModel
    {
        public Dictionary<string, List<ProductRow>> createDictionary(List<string> list) 
        {
            List<ProductRow> productRowList;
            Dictionary<string, List<ProductRow>> List = new Dictionary<string, List<ProductRow>>();

            foreach(string marketId in list)
            {
                productRowList = new List<ProductRow>();
                List.Add(marketId, productRowList);
            }
            return List;
        }

        public void addValuesToListList(List<List<string>> matchingValues, Dictionary<string, List<ProductRow>> dictonary) 
        {
            foreach(List<string> list in matchingValues)
            {
                ProductRow productRow = new ProductRow();
                productRow.MarketId = list[0];
                productRow.CurrencyCode = list[1];
                productRow.ValidFrom = list[2];
                productRow.ValidUntil = list[3];
                productRow.UnitPrice = list[4];

                dictonary[list[0]].Add(productRow);             
            }
            sortDictionary(dictonary);
        }

        public Dictionary<string, List<ProductRow>> sortDictionary(Dictionary<string, List<ProductRow>> dictonary)
        {
            Dictionary<string, List<ProductRow>> orderedList = new Dictionary<string, List<ProductRow>>();

            foreach(KeyValuePair<string, List<ProductRow>> entry in dictonary)
            {
                List<ProductRow> sortedList = entry.Value.OrderBy(x=>x.ValidFrom).ToList();
                orderedList[entry.Key] = sortedList;
            }
            return orderedList;
        }

   
    }
}