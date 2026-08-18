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

    public interface IHandlerBaseClient<T>
    {
        T GetByName(string name);

        T GetByID(int Id);

        IEnumerable<T> GetAll();
        IEnumerable<T> GetByDate(DateTime fecha);

        bool Save(T order, out string message);
        
        T GetByCuit(string cuit);
        
    }
}