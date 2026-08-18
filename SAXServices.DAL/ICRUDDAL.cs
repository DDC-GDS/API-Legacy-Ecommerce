using SAXServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.DAL
{
    public interface ICRUDDAL<T>
    {
        IEnumerable<T> Get();

        bool Save(T element,out String mensaje);

        bool Delete(T element);
        T GetByName(string name);

        T GetById(int id);
        IEnumerable<T> GetByDate(DateTime fecha);       

      
    }

    public interface  ICRUDDALClient<T>: ICRUDDAL<T>
    {
        Boolean Get(out String mensaje, out List<Client> clientes);

        Boolean GetById(int name, out String mensaje, out Client cliente);

        Boolean GetByCuit(string cuit, out String mensaje, out Client cliente);

        Boolean Save(T element);       

        //T GetByCuit(string cuit);
    }
    public interface ICRUDDALClientWeb<T>    {        bool Save(T element, out String mensaje, String listaPrecios);    }

    public interface ICRUDDALPriceList<T>: ICRUDDAL<T>
    {
        Boolean Get(out String mensaje,out IEnumerable<PriceList> listas);
                
        Boolean  GetByName(String name, out String mensaje, out PriceList listaPrecio);

        Boolean  GetByDate(DateTime fecha, out String mensaje, out IEnumerable<PriceList> listas);
                
    }

    public interface ICRUDDALProduct<T>: ICRUDDAL<T>
    {
        Boolean Get(out String mensaje, out List<Product> listas);

        Boolean GetByName(String name, out String mensaje, out Product producto);
    }

    public interface ICRUDDALSeller<T> : ICRUDDAL<T>
    {
        Boolean Get(out String mensaje, out List<Seller> vendedor);

        Boolean GetById(int name, out String mensaje, out Seller vendedor);
    }
}
