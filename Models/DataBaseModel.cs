using System;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductRow productRow = new ProductRow();

                        productRow.MarketId = reader["marketId"].ToString();
                        productRow.CurrencyCode = reader["currencyCode"].ToString();
                        productRow.ValidFrom = convertToDateFormatString(reader["validFrom"].ToString());
                        productRow.ValidUntil = convertToDateFormatString(reader["validUntil"].ToString());
                        productRow.UnitPrice = roundPrice(reader["unitPrice"].ToString());

                        //Do not add productRow if it already exist with cheaper price

                        foreach(ProductRow row in List[productRow.MarketId])
                        {
                            if(row.MarketId == productRow.MarketId && row.CurrencyCode == productRow.CurrencyCode && row.ValidFrom == productRow.ValidFrom && row.ValidUntil == productRow.ValidUntil)
                            {
                                if(createPrice(row.UnitPrice) < createPrice(productRow.UnitPrice))
                                {
                                    row.UnitPrice = productRow.UnitPrice;
                                    continue;
                                }
                            }
                        }
                        List[productRow.MarketId].Add(productRow);
                    }

                }

                Dictionary<string, List<ProductRow>> orderedList = new Dictionary<string, List<ProductRow>>();

                foreach(KeyValuePair<string, List<ProductRow>> entry in List)
                {
                    List<ProductRow> sortedList = entry.Value.OrderBy(x=>x.ValidFrom).ToList();
                    orderedList[entry.Key] = sortedList;
                }
                return orderedList;
            }
        }

        private string convertToDateFormatString(string date)
        {
            if(date != "NULL")
            {   
                DateTime parsedDate = DateTime.Parse(date);
                return parsedDate.ToString();
            }
            return date;    
        }

        private string roundPrice(string price)
        {
            int index = price.IndexOf(".");
            price = price.Substring(0, index + 3);
            return price;
        }

        private int createPrice(string price)
        {
            price = price.Replace(".", ",");
            var intPrice = Int32.Parse(price);
            return intPrice;
        }
    }
}