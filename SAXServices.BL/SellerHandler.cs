using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.BL
{
    public class SellerHandler : IHandlerBase<Seller>
    {
        ICRUDDAL<Seller> _sellerDAL;

        public SellerHandler():this(new SellerDAL()){ }

        public SellerHandler(ICRUDDAL<Seller> dal)
        {
            this._sellerDAL = dal;
        }

        public IEnumerable<Seller> GetAll()
        {
            return this._sellerDAL.Get();
        }

        public IEnumerable<Seller> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Seller GetByID(int Id)
        {
            return this._sellerDAL.GetById(Id);
        }

        public Seller GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(Seller order, out string message)
        {
            throw new NotImplementedException();
        }
    }
}
