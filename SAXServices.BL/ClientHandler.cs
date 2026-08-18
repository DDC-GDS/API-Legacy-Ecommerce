using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAXServices.BL
{
    public class ClientHandler :  IHandlerBaseClient<Client>
    {
        ICRUDDALClient<Client> _clientDAL;

        public ClientHandler():this(new ClientDAL()){ }

        public ClientHandler(ICRUDDALClient<Client> dal)
        {
            this._clientDAL = dal;
        }

        public Client GetByID(int id)
        {
            return this._clientDAL.GetById(id);
        }

        public IEnumerable<Client> GetAll()
        {
            return this._clientDAL.Get();
        }

        public Client GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Client> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public bool Save(Client order, out string message)
        {
            throw new NotImplementedException();
        }

        public Client GetByCuit(string  cuit)
        {
            return this._clientDAL.GetByCuit(cuit);
        }
    }
}
