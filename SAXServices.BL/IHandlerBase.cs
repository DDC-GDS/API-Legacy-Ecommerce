using SAXServices.Contracts;
using System.Collections.Generic;
using System;

namespace SAXServices.BL
{
    public interface IHandlerBase<T>
    {
        T GetByName(string name);

        T GetByID(int Id);

        IEnumerable<T> GetAll();
        IEnumerable<T> GetByDate(DateTime fecha);

        bool Save(T order, out string message);
       
    }

    public interface IHandlerBaseClient<T>: IHandlerBase<T>
    {
        Boolean  GetByID(int Id, out String mensaje, out Client clientes);

        Boolean  GetAll(out String mensaje, out List<Client> clientes);
        
        Boolean  GetByCuit(string cuit, out String mensaje, out Client cliente);
        
    }

    public interface IHandlerBasePriceList<T>: IHandlerBase<T> 
    {
        Boolean  GetByName(String name, out String mensaje, out PriceList listaPrecio);              

        Boolean GetAll(out String mensaje, out IEnumerable<PriceList> listas);

        Boolean GetByDate(DateTime fecha, out String mensaje, out IEnumerable<PriceList> listaPrecio);              

    }

    public interface IHandlerBaseProduct<T> : IHandlerBase<T>
    {
        Boolean GetAll(out String mensaje, out List<Product> listas);

        Boolean GetByName(String name, out String mensaje, out Product producto);

    }

    public interface IHandlerBaseSeller<T> : IHandlerBase<T>
    {
        Boolean GetAll(out String mensaje, out List<Seller> vendedores);

        Boolean GetById(int name, out String mensaje, out Seller vendedor);

    }
}