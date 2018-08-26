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

        public Dictionary<string, List<ProductRow>> addValuesToListList(List<List<string>> matchingValues, Dictionary<string, List<ProductRow>> dictonary) 
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
            return dictonary;
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

        public Dictionary<string, List<ProductRow>> setProductsList(Dictionary<string, List<ProductRow>> list, List<string> allMarketId)
        {
            Boolean isNull;
            string unitPrice = null;

            Dictionary<string, List<ProductRow>> newItems = new Dictionary<string, List<ProductRow>>();

            for (int j = 0; j < list.Count; j++)
            {
                KeyValuePair<string, List<ProductRow>> entry = list.ElementAt(j);
                isNull = false;
                for(int i = 0; i < entry.Value.Count; i++)
                {  
                    if(checkIfNull(entry.Value[i].ValidUntil))
                        {
                            isNull = true;
                            unitPrice = entry.Value[i].UnitPrice;
                            entry.Value[i].ValidUntil = DateTime.MaxValue.ToString();
                        }
                    if(i > 0)
                    {          
                        int index = i - 1;
                        if(checkIfNull(entry.Value[index].ValidUntil)) 
                        {
                            isNull = true;
                            unitPrice = entry.Value[index].UnitPrice;
                            entry.Value[index].ValidUntil = DateTime.MaxValue.ToString();
                        }                       

                        if(ifSmallerDate(entry.Value[i].ValidFrom, entry.Value[index].ValidUntil) && ifSmallerPrice(entry.Value[i].UnitPrice, entry.Value[index].UnitPrice))
                        {
                            entry.Value[index].ValidUntil = entry.Value[i].ValidFrom;
                        } else if(ifSmallerDate(entry.Value[i].ValidFrom, entry.Value[index].ValidUntil) && ifBiggerPrice(entry.Value[i].UnitPrice, entry.Value[index].UnitPrice)) 
                        {
                            entry.Value.Remove(entry.Value[i]);
                        } else if(ifBiggerDate(entry.Value[i].ValidFrom, entry.Value[index].ValidUntil) && convertToDateFormat(entry.Value[i].ValidFrom) != convertToDateFormat(entry.Value[index].ValidUntil).AddDays(1) && isNull == true)
                        {
                            ProductRow newProductRow = new ProductRow();
                            newProductRow.MarketId = entry.Key.ToString();
                            newProductRow.ValidFrom = entry.Value[index].ValidUntil;
                            if(i == entry.Value.Count - 1)
                            {
                                ProductRow productRowNew = creteNewProductRow(newProductRow, entry.Value[i], entry.Key.ToString(), unitPrice);
           
                                if (!newItems.ContainsKey(entry.Key)) {
                                    newItems[entry.Key] = new List<ProductRow>();
                                }
                                newItems[entry.Key].Add(productRowNew);
                            } else {
                                newProductRow.ValidUntil = entry.Value[i + 1].ValidFrom;
                            }
                            newProductRow.UnitPrice = unitPrice;

                            if (!newItems.ContainsKey(entry.Key)) {
                                newItems[entry.Key] = new List<ProductRow>();
                            }
                            newItems[entry.Key].Add(newProductRow);             
                        } 
                    }        
                }
            }

            foreach(string marketId in  allMarketId)
            {
                if (list.ContainsKey(marketId) && newItems.ContainsKey(marketId))
                {
                    list[marketId] = list[marketId].Concat(newItems[marketId]).ToList();
                }
            }

            foreach(string marketId in allMarketId)
            {
                list[marketId] = list[marketId].Distinct().ToList();
            }

            Dictionary<String, List<ProductRow>> newSortedList = sortLists(list);

            foreach(KeyValuePair<string, List<ProductRow>> entry in newSortedList)
            {
                changeValidUntil(entry.Value);
            }                                 
            return newSortedList;         
        }

         private List<ProductRow> changeValidUntil(List<ProductRow> productList)
        {
            for(int i = 0; i < productList.Count; i++)
            {
                if(productList[i].ValidUntil == DateTime.MaxValue.ToString())
                {

                    if(i == productList.Count - 1)
                    {
                        productList[i].ValidUntil = "";
                    } else if(createDouble(productList[i + 1].UnitPrice) < createDouble(productList[i].UnitPrice))
                    {
                        productList[i].ValidUntil = productList[i + 1].ValidFrom;
                    } else if(createDouble(productList[i + 1].UnitPrice) > createDouble(productList[i].UnitPrice))
                    {
                        productList.Remove(productList[i + 1]);
                    } 
                    if(i == productList.Count - 1)
                    {
                        productList[i].ValidUntil = "";
                    }
                }        
            }
            return productList;
        }

        private  Dictionary<String, List<ProductRow>> sortLists(Dictionary<String, List<ProductRow>> Dictionary)
        {
            Dictionary<String, List<ProductRow>> sortedDictionary = new Dictionary<String, List<ProductRow>>();
            foreach(KeyValuePair<string, List<ProductRow>> entry in Dictionary)
            {
                List<ProductRow> sortedList = entry.Value.OrderBy(x=>x.ValidFrom).ToList();
                sortedDictionary[entry.Key] = sortedList;
            }
            return sortedDictionary;
        }

        private ProductRow creteNewProductRow(ProductRow newProductRow, ProductRow row, string key, string unitPrice)
        {
            newProductRow.ValidUntil = row.ValidFrom;
            ProductRow productRowNew = new ProductRow();
            productRowNew.MarketId = key;
            productRowNew.ValidFrom = row.ValidUntil;
            productRowNew.ValidUntil = DateTime.MaxValue.ToString();
            productRowNew.UnitPrice = unitPrice;

            return productRowNew;
        }

        public Boolean checkIfNull(string value) 
        {
            if(value == "NULL")
            {
                return true;
            }
            return false;
        }

        private DateTime convertToDateFormat(string date)
        { 
            DateTime parsedDate = DateTime.Parse(date);
            return parsedDate; 
        }

        private double createDouble(string price)
        {
            price = price.Replace(".", ",");
            var doublePrice = Convert.ToDouble(price);
            return doublePrice;
        }

        private Boolean ifSmallerDate(string validFrom, string validUntil)
        {
            if(convertToDateFormat(validFrom) < convertToDateFormat(validUntil))
            {
                return true;
            }
            return false;
        }

        private Boolean ifBiggerDate(string validFrom, string validUntil)
        {
            if(convertToDateFormat(validFrom) > convertToDateFormat(validUntil))
            {
                return true;
            }
            return false;
        }

        private Boolean ifSmallerPrice(string priceOne, string priceTwo)
        {
            if(createDouble(priceOne) < createDouble(priceTwo))
            {
                return true;
            }
            return false;
        }

        private Boolean ifBiggerPrice(string priceOne, string priceTwo)
        {
            if(createDouble(priceOne) > createDouble(priceTwo))
            {
                return true;
            }
            return false;
        }
    }
}