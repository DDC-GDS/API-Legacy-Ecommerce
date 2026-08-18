using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAXServices.Contracts
{ 
    public class PriceList
    {
        public string Name { get; set; }

        public List<PriceListItem> Items { get; set; }

        public bool Modificado { get; set; }
    }

    public class PriceListItem
    {
        public string Product_Id { get; set; }

        public string ProductVariation_Id{ get; set; }

        public decimal Price { get; set; }

        public DateTime FechaVigencia { get; set; }

        public DateTime FechaHasta { get; set; }
    }
}
