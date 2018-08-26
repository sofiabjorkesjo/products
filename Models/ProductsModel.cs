using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

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
    }
}