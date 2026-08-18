using System;
using System.Collections.Generic;
using SAXServices.Contracts;
using SAXServices.DAL;

namespace SAXServices.BL
{
    public class PriceListHandler : IHandlerBase<PriceList>
    {
        ICRUDDAL<PriceList> _priceListDAL;

        public PriceListHandler():this(new PriceListDAL()){ }

        public PriceListHandler(ICRUDDAL<PriceList> dal)
        {
            this._priceListDAL = dal;
        }

        public IEnumerable<PriceList> GetAll()
        {
            return this._priceListDAL.Get();
        }

        public IEnumerable<PriceList> GetByDate(DateTime fecha)
        {
            return this._priceListDAL.GetByDate(fecha);
        }

        public PriceList GetByID(int Id)
        {
            throw new NotImplementedException();
        }

        public PriceList GetByName(string name)
        {
            var result = this._priceListDAL.GetByName(name);
            return result;
        }

        public bool Save(PriceList order, out string message)
        {
            throw new NotImplementedException();
        }

        private PriceList MockPriceList(int i)
        {
            var priceItems = new List<PriceListItem>();

            for (int j = 0; j < i; j++)
            {
                priceItems.Add(new PriceListItem { Price = j, ProductVariation_Id = "Talle"+j, Product_Id = "ART"+j });
            }

            var priceList = new PriceList { Name = "PriceList" + i, Items = priceItems, Modificado = false };

            return priceList;
        }
    }
}
