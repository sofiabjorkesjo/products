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
    public class DataBaseModel
    {
        private SqliteConnectionStringBuilder connectToDB()
        {
            SqliteConnectionStringBuilder connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = "./products.db";

            return connectionString;
        }

        public void createProductsTable()
        {
            if(!File.Exists("products.db")) 
            {
                SqliteConnectionStringBuilder connectionString = connectToDB();
                using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
                {
                    connection.Open();

                    var createTable = connection.CreateCommand();
                    createTable.CommandText = "CREATE TABLE products(priceValueId VARCHAR(50) PRIMARY KEY, created VARCHAR(50), modified VARCHAR(50), catalogEntryCode VARCHAR(50), marketId VARCHAR(50), currencyCode VARCHAR(50), validFrom VARCHAR(50), validUntil VARCHAR(50), unitPrice VARCHAR(50))";
                    createTable.ExecuteNonQuery();
                }
            }
        }

        
        //Reads dataset and insert values to DB
        public void insertValuesToProductsTable(string filename) {
            try
            {    
                SqliteConnectionStringBuilder connectionString = connectToDB();

                using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (StreamReader sr = new StreamReader(filename))
                        {
                            string line;

                            while((line = sr.ReadLine()) != null) {
                                string[] delimitedLine = line.Split('\t');
                                string priceValueId = delimitedLine[0];
                                string created = delimitedLine[1];
                                string modified = delimitedLine[2];
                                string catalogEntryCode = delimitedLine[3];
                                string marketId = delimitedLine[4];
                                string currencyCode = delimitedLine[5];
                                string validForm = delimitedLine[6];
                                string validUntil = delimitedLine[7];
                                string unitPrice = delimitedLine[8];

                                var insertCmd = connection.CreateCommand();
                                insertCmd.CommandText = $"INSERT INTO products VALUES('{priceValueId}', '{created}', '{modified}', '{catalogEntryCode}', '{marketId}', '{currencyCode}', '{validForm}', '{validUntil}', '{unitPrice}')";
                                insertCmd.ExecuteNonQuery();                       
                            };  
                            transaction.Commit();
                            deleteRow(); 
                        }
                    }
                }      
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }        
        }

        private void deleteRow()
        {
            SqliteConnectionStringBuilder connectionString = connectToDB();

            using(SqliteConnection connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "DELETE FROM products WHERE priceValueId='PriceValueId'";
                selectCmd.ExecuteReader();             
            }
        }

        public List<string> getCatalogEntryCodes() 
        {
            SqliteConnectionStringBuilder connectionString = connectToDB();
            List<string> catalogEntryCodes = new List<string>();

            using(SqliteConnection connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT DISTINCT catalogEntryCode FROM products";
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string res = reader["catalogEntryCode"].ToString();
                        catalogEntryCodes.Add(res);
                    }
                }
            }
            return catalogEntryCodes;
        }

        public Dictionary<string, List<ProductRow>> getProducts(string catalogEntryCode)
        {
            SqliteConnectionStringBuilder connectionString = connectToDB();

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                 List<ProductRow> productRowList;
            
                //get all unique marketId and create list for each one of them

                Dictionary<string, List<ProductRow>> List = new Dictionary<string, List<ProductRow>>();

                var selectCmdDistinct = connection.CreateCommand();
                selectCmdDistinct.CommandText = $"SELECT DISTINCT marketId FROM products WHERE catalogEntryCode='{catalogEntryCode}';";

                using (var reader = selectCmdDistinct.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string res = reader["marketId"].ToString();
                        productRowList = new List<ProductRow>();
                        List.Add(res, productRowList);
                    }
                }

                //Get values from DB that matches the catalogEntyrCode and them to matching list

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $"SELECT * FROM products WHERE catalogEntryCode='{catalogEntryCode}';";

                List<string> allMarkets = new List<string>();

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductRow productRow = new ProductRow();

                        allMarkets.Add(reader["marketId"].ToString());

                        productRow.MarketId = reader["marketId"].ToString();
                        productRow.CurrencyCode = reader["currencyCode"].ToString();
                        productRow.ValidFrom = reader["validFrom"].ToString();
                        productRow.ValidUntil = reader["validUntil"].ToString();
                        productRow.UnitPrice = roundPrice(reader["unitPrice"].ToString());

                        List[productRow.MarketId].Add(productRow);
                    }

                }

                Dictionary<string, List<ProductRow>> orderedList = new Dictionary<string, List<ProductRow>>();

                foreach(KeyValuePair<string, List<ProductRow>> entry in List)
                {
                    List<ProductRow> sortedList = entry.Value.OrderBy(x=>x.ValidFrom).ToList();
                    orderedList[entry.Key] = sortedList;
                }
                
                Boolean isNull;
                string unitPrice = null;

                Dictionary<string, List<ProductRow>> newItems = new Dictionary<string, List<ProductRow>>();

                for (int j = 0; j < orderedList.Count; j++)
                {
                    KeyValuePair<string, List<ProductRow>> entry = orderedList.ElementAt(j);
                    isNull = false;
                    for(int i = 0; i < entry.Value.Count; i++)
                    {  
                        if(entry.Value[i].ValidUntil == "NULL")
                            {
                                isNull = true;
                                unitPrice = entry.Value[i].UnitPrice;
                                entry.Value[i].ValidUntil = DateTime.MaxValue.ToString();
                            }
                        if(i > 0)
                        {          
                            int index = i - 1;
                            if(entry.Value[index].ValidUntil == "NULL") 
                            {
                                isNull = true;
                                unitPrice = entry.Value[index].UnitPrice;
                                entry.Value[index].ValidUntil = DateTime.MaxValue.ToString();
                            }                       

                            if(convertToDateFormat(entry.Value[i].ValidFrom) < convertToDateFormat(entry.Value[index].ValidUntil) && createDouble(entry.Value[i].UnitPrice) < createDouble(entry.Value[index].UnitPrice))
                            {
                                entry.Value[index].ValidUntil = entry.Value[i].ValidFrom;
                            } else if(convertToDateFormat(entry.Value[i].ValidFrom) < convertToDateFormat(entry.Value[index].ValidUntil) && createDouble(entry.Value[i].UnitPrice) > createDouble(entry.Value[index].UnitPrice)) 
                            {
                                entry.Value.Remove(entry.Value[i]);
                            } else if(convertToDateFormat(entry.Value[i].ValidFrom) > convertToDateFormat(entry.Value[index].ValidUntil) && convertToDateFormat(entry.Value[i].ValidFrom) != convertToDateFormat(entry.Value[index].ValidUntil).AddDays(1) && isNull == true)
                            {
                                ProductRow productRow2 = new ProductRow();
                                productRow2.MarketId = entry.Key.ToString();
                                productRow2.ValidFrom = entry.Value[index].ValidUntil;
                                if(i == entry.Value.Count - 1)
                                {
                                    productRow2.ValidUntil = entry.Value[i].ValidFrom;

                                    ProductRow productRow3 = new ProductRow();
                                    productRow3.MarketId = entry.Key.ToString();
                                    productRow3.ValidFrom = entry.Value[i].ValidUntil;
                                    productRow3.ValidUntil = DateTime.MaxValue.ToString();
                                    productRow3.UnitPrice = unitPrice;
                                    if (!newItems.ContainsKey(entry.Key)) {
                                        newItems[entry.Key] = new List<ProductRow>();
                                    }
                                    newItems[entry.Key].Add(productRow3);

                                } else {
                                    productRow2.ValidUntil = entry.Value[i + 1].ValidFrom;
                                }
                                productRow2.UnitPrice = unitPrice;

                                if (!newItems.ContainsKey(entry.Key)) {
                                    newItems[entry.Key] = new List<ProductRow>();
                                }

                                newItems[entry.Key].Add(productRow2);             
                            } 
                        }        
                    }
                }

                foreach(string marketId in allMarkets)
                {
                    if (orderedList.ContainsKey(marketId) && newItems.ContainsKey(marketId))
                    {
                        orderedList[marketId] = orderedList[marketId].Concat(newItems[marketId]).ToList();
                    }
                }

                foreach(string marketId in allMarkets)
                {
                    orderedList[marketId] = orderedList[marketId].Distinct().ToList();
                }

                Dictionary<String, List<ProductRow>> newSortedList = sortLists(orderedList);

                foreach(KeyValuePair<string, List<ProductRow>> entry in newSortedList)
                {
                    changeValidUntil(entry.Value);
                }                                 
                return newSortedList;
            }
        }

        private DateTime convertToDateFormat(string date)
        { 
            DateTime parsedDate = DateTime.Parse(date);
            return parsedDate; 
        }

        private string roundPrice(string price)
        {
            int index = price.IndexOf(".");
            price = price.Substring(0, index + 3);
            return price;
        }

        private double createDouble(string price)
        {
            price = price.Replace(".", ",");
            var doublePrice = Convert.ToDouble(price);
            return doublePrice;
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

       


    }
}