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

    public interface  ICRUDDALClient<T>
    {
        IEnumerable<T> Get();

        bool Save(T element);

        bool Delete(T element);
        T GetByName(string name);

        T GetById(int id);
        IEnumerable<T> GetByDate(DateTime fecha);

        T GetByCuit(string cuit);
    }
    public interface ICRUDDALClientWeb<T>
    {

        bool Save(T element, out String mensaje, String listaPrecios);

    }
}
