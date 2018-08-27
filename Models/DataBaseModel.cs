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

        public List<string> getAllMarketId(string catalogEntryCode) 
        {
            SqliteConnectionStringBuilder connectionString = connectToDB();
            List<string> list = new List<string>();

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                //get all unique marketId and create list for each one of them

                var selectCmdDistinct = connection.CreateCommand();
                selectCmdDistinct.CommandText = $"SELECT DISTINCT marketId FROM products WHERE catalogEntryCode='{catalogEntryCode}';";

                using (var reader = selectCmdDistinct.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string res = reader["marketId"].ToString();
                        list.Add(res);
                    }
                }
            }
            return list;
        }

        public List<List<string>> getMatchingValues(string catalogEntryCode)
        {
            SqliteConnectionStringBuilder connectionString = connectToDB();

            using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
            {
                connection.Open();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $"SELECT * FROM products WHERE catalogEntryCode='{catalogEntryCode}';";

                List<string> product;
                List<List<string>> allMarkets = new List<List<string>>();

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        product = new List<string>();

                        product.Add(reader["marketId"].ToString());
                        product.Add(reader["currencyCode"].ToString());
                        product.Add(reader["validFrom"].ToString());
                        product.Add(reader["validUntil"].ToString());
                        product.Add(reader["unitPrice"].ToString());

                        allMarkets.Add(product);
                    }
                }
                return allMarkets;
            }
        }
    }
}