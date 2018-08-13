using System;
using System.IO;

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
  
    }
}