using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAXServices.Contracts
{
    public enum ProductType
    {
        simple,
        variable
    }

    public class Product
    {
        public string Product_Id { get; set; }

        public ProductType Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Manage_Stock { get; set; }

        public decimal Stock { get; set; }
        
        public decimal StockDisponible { get; set; }

        public string Category { get; set; }

        public List<ProductVariation> Variations { get; set; }

        public DateTime LastUpdate { get; set; }
    }

    public class ProductVariation
    {
        public string ParentProduct_Id { get; set; }

        public string ProductVariation_Id { get; set; }

        public string Description { get; set; }

        public bool Manage_Stock { get; set; }

        public decimal Stock { get; set; }
        public decimal StockDisponible { get; set; }

        public string AttributeName { get; set; }

        public string AttributeValue { get; set; }

        public int Order { get; set; }
        
        public List<ProductVariation> Variations { get; set; }
    }
}
