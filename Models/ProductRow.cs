using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace products.Models
{
    public class ProductRow
    {
        private string priceValueId;
        private string created;
        private string modified;
        private string marketId;
        private string currencyCode;
        private string validFrom;
        private string validUntil;
        private string unitPrice;

        public string PriceValueId
        {
            get
            {
                return priceValueId;
            }
            set 
            {
                priceValueId = value;
            }
        }

        public string Created
        {
            get
            {
                return created;
            }
            set 
            {
                created = value;
            }
        }

        public string Modified
        {
            get
            {
                return modified;
            }
            set 
            {
                modified = value;
            }
        }

        public string MarketId
        {
            get
            {
                return marketId;
            }
            set 
            {
                marketId = value;
            }
        }

        public string CurrencyCode
        {
            get
            {
                return currencyCode;
            }
            set 
            {
                currencyCode = value;
            }
        }

        public string ValidFrom
        {
            get
            {
                return validFrom;
            }
            set 
            {
                validFrom = value;
            }
        }

        public string ValidUntil
        {
            get
            {
                return validUntil;
            }
            set 
            {
                validUntil= value;
            }
        }

        public string UnitPrice
        {
            get
            {
                return unitPrice;
            }
            set 
            {
                unitPrice = value;
            }
        }

    }
}