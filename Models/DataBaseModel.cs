using System;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace products.Models
{
    public class DataBaseModel
    {
        //Create SQLite DB, reads dataset and insert values to DB
        public void createDbAndInsertValues() {
            try
            {
                if(!File.Exists("products.db")) {
                    var connectionString = new SqliteConnectionStringBuilder();
                    connectionString.DataSource = "./products.db";

                    using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
                    {
                        connection.Open();

                        var createTable = connection.CreateCommand();
                        createTable.CommandText = "CREATE TABLE products(priceValueId VARCHAR(50) PRIMARY KEY, created VARCHAR(50), modified VARCHAR(50), catalogEntryCode VARCHAR(50), marketId VARCHAR(50), currencyCode VARCHAR(50), validFrom VARCHAR(50), validUntil VARCHAR(50), unitPrice VARCHAR(50))";
                        createTable.ExecuteNonQuery();

                        using (var transaction = connection.BeginTransaction())
                        {
                            using (StreamReader sr = new StreamReader("price_detail.csv"))
                            {
                                string line;

                                while((line = sr.ReadLine()) != null) {
                                    var delimitedLine = line.Split('\t');
                                    var priceValueId = delimitedLine[0];
                                    var created = delimitedLine[1];
                                    var modified = delimitedLine[2];
                                    var catalogEntryCode = delimitedLine[3];
                                    var marketId = delimitedLine[4];
                                    var currencyCode = delimitedLine[5];
                                    var validForm = delimitedLine[6];
                                    var validUntil = delimitedLine[7];
                                    var unitPrice = delimitedLine[8];

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
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }        
        }

        public void deleteRow()
        {
            var connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = "./products.db";

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "DELETE FROM products WHERE priceValueId='PriceValueId'";
                selectCmd.ExecuteReader();
               
            }
        }

        public List<string> getCatalogEntryCodesFromDB() 
        {
            var connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = "./products.db";
            List<string> catalogEntryCodes = new List<string>();

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
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

        public List<ProductRow> getValuesFromDB(string catalogEntryCode)
        {
            var connectionString = new SqliteConnectionStringBuilder();
            connectionString.DataSource = "./products.db";

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                var productRowList = new List<ProductRow>();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $"SELECT * FROM products WHERE catalogEntryCode='{catalogEntryCode}';";

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var productRow = new ProductRow();
                        
                        productRow.MarketId = reader["marketId"].ToString();
                        productRow.ValidFrom = reader["validFrom"].ToString();
                        productRow.ValidUntil = reader["validUntil"].ToString();
                        productRow.UnitPrice = reader["unitPrice"].ToString();

                        productRowList.Add(productRow); 
                    }
                }  
                return productRowList;
            }
        }
    }
}