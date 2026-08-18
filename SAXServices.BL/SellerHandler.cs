using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.BL
{
    public class SellerHandler : IHandlerBaseSeller<Seller>
    {
        ICRUDDALSeller<Seller> _sellerDAL;

        public SellerHandler():this(new SellerDAL()){ }

        public SellerHandler(ICRUDDALSeller<Seller> dal)
        {
            this._sellerDAL = dal;
        }

        public Boolean GetAll(out String mensaje, out List<Seller> vendedores)
        {
            return this._sellerDAL.Get(out mensaje, out vendedores);
        }

       
        public Boolean GetById(int Id,out String mensaje, out Seller vendedor)
        {
            return this._sellerDAL.GetById(Id,out mensaje, out vendedor);
        }

        
        
        /*No implementados*/
        public IEnumerable<Seller> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Seller GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(Seller order, out string message)
        {
            throw new NotImplementedException();
        }

        public Seller GetByID(int Id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Seller> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
