using SAXServices.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.DAL
{
    public class SellerDAL : CRUDDALBase, ICRUDDAL<Seller>
    {
        public bool Delete(Seller element)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Seller> Get()
        {
            var result = new List<Seller>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetSellerData(connection, 0, ref result);
            }

            return result;
        }

        private void GetSellerData(ConnectionStringSettings connection, int id, ref List<Seller> result)
        {
            if (OpenDBConnection(connection.ConnectionString))
            {
                ///Paso 1: Busco los vendedores
                var sSql =
                    "SELECT e.[id], e.[Apellido], e.[Nombre] " +
                    "FROM[dbo].[EMPLEADOS] e " +
                    "WHERE e.Es_Vendedor = 1 and e.habilitado = 1 ";

                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND id={0}", id);

                using (var sqlCommand = new SqlCommand(sSql, oConexion))
                {
                    var rsp = sqlCommand.ExecuteReader();

                    while (rsp.Read())
                    {
                        Seller seller;

                        //Pregunto si el vendedor ya existe.
                        if (!result.Exists(s => s.Seller_Id == (int)rsp["id"]))
                        {
                            seller = new Seller
                            {
                                Seller_Id = (int)rsp["id"],
                                //LastUpdate = rsp["Last_Update"],                                 
                                Name = rsp["Nombre"].ToString(),
                                LastName = rsp["Apellido"].ToString()
                            };

                            result.Add(seller);
                        }
                    }

                    rsp.Close();
                }

                CloseDBConnection();
            }
        }

        public IEnumerable<Seller> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Seller GetById(int id)
        {
            var result = new List<Seller>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetSellerData(connection, id, ref result);
            }

            return result.FirstOrDefault();
        }

        public Seller GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(Seller element, out String mensaje)
        {
            throw new NotImplementedException();
        }
    }
}
