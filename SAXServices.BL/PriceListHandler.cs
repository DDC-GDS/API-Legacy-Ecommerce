using System;
using System.Collections.Generic;
using SAXServices.Contracts;
using SAXServices.DAL;

namespace SAXServices.BL
{
    public class PriceListHandler : IHandlerBasePriceList<PriceList>
    {
        ICRUDDALPriceList<PriceList> _priceListDAL;

        public PriceListHandler():this(new PriceListDAL()){ }

        public PriceListHandler(ICRUDDALPriceList<PriceList> dal)
        {
            this._priceListDAL = dal;
        }

        public Boolean  GetAll(out String mensaje, out IEnumerable<PriceList> listas)
        {            
            return this._priceListDAL.Get(out mensaje,out listas);
        }

        public Boolean  GetByDate(DateTime fecha,out String mensaje, out IEnumerable<PriceList> listas)
        {
            return this._priceListDAL.GetByDate(fecha,out mensaje,out listas);
        }
                
        public Boolean  GetByName(string name,out string mensaje,out PriceList listaPrecio)
        {                        
            var result = this._priceListDAL.GetByName(name,out mensaje,out listaPrecio);
            return result;
        }


        //NO implementados

        public Boolean  GetByID(int Id)
        {
            throw new NotImplementedException();
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

        PriceList IHandlerBase<PriceList>.GetByName(string name)
        {
            throw new NotImplementedException();
        }

        PriceList IHandlerBase<PriceList>.GetByID(int Id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PriceList> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PriceList> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }
    }
}
