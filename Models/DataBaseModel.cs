using System;
using System.IO;
using Microsoft.Data.Sqlite;


namespace products.Models
{
    public class DataBaseModel
    {
        public void readDataSet() {
            try
            {
                using (StreamReader sr = new StreamReader("price_detail.csv"))
                {
                    string line;
                    while((line = sr.ReadLine()) != null) {
                        Console.WriteLine(line);
                    };
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public void createDbAndInsertValues() {
            if(!File.Exists("products.db")) {
                var connectionString = new SqliteConnectionStringBuilder();
                connectionString.DataSource = "./products.db";

                using(var connection = new SqliteConnection(connectionString.ConnectionString)) 
                {
                    connection.Open();

                    var createTable = connection.CreateCommand();
                    createTable.CommandText = "CREATE TABLE products(PriceValueId VARCHAR(50), created VARCHAR(50), modified VARCHAR(50), CatalogEntryCode VARCHAR(50), MarketId VARCHAR(50), CurrencyCode VARCHAR(50), ValidFrom VARCHAR(50), ValidUntil VARCHAR(50), UnitPrice VARCHAR(50))";
                    createTable.ExecuteNonQuery();

                    using(var transaction = connection.BeginTransaction())
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = "INSERT INTO products VALUES('test', 'test', 'test', 'test', 'test', 'test', 'test', 'test', 'test')";
                        insertCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
        }
    }
}