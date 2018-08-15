using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace products.Models
{
    public class DataBaseModel
    {
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
                        createTable.CommandText = "CREATE TABLE products(priceValueId VARCHAR(50), created VARCHAR(50), modified VARCHAR(50), catalogEntryCode VARCHAR(50), marketId VARCHAR(50), currencyCode VARCHAR(50), validFrom VARCHAR(50), validUntil VARCHAR(50), unitPrice VARCHAR(50))";
                        createTable.ExecuteNonQuery();

                        using (var transaction = connection.BeginTransaction())
                        {
                            using (StreamReader sr = new StreamReader("price_detail.csv"))
                            {
                                string line;

                                while((line = sr.ReadLine()) != null) {
                                    var delimitedLine = line.Split();
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
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }        
        }
    }
}