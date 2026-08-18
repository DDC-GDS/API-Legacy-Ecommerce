using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.DAL
{
    public class BdDAL
    {
        public  SqlParameter crearParametro(String nombre, DbType tipo, Object valor) {
            SqlParameter parametro = new SqlParameter();
            parametro.ParameterName = nombre;
            parametro.DbType = tipo;
            parametro.Value = valor;
            return parametro;
        }

     }
}
