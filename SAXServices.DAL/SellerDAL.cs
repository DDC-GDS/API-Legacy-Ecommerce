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
    public class SellerDAL : CRUDDALBase, ICRUDDALSeller<Seller>
    {
        
        public Boolean Get(out String mensaje, out List<Seller> vendedores)
        {
            mensaje = "";
            vendedores = new List<Seller>();
            var connections = ConfigurationManager.ConnectionStrings;
                      
            foreach (ConnectionStringSettings connection in connections)
            {
                GetSellerData(connection, 0, ref vendedores,out mensaje);
            }
            if (mensaje.Equals(""))
            {
                mensaje = "Cantidad de vendedores: " + vendedores.Count.ToString();
                return true;
            }
                
            else
                return false;           
        }

        private void GetSellerData(ConnectionStringSettings connection, int id, ref List<Seller> result, out String mensaje)
        {
            mensaje = "";
            try { 
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
            catch (Exception ex)
            {
                mensaje = "Error al buscar vendedores. " + ex.Message;
                
            }
        }

       

        public Boolean  GetById(int id,out String mensaje, out Seller vendedor)
        {
            var result = new List<Seller>();
            mensaje = "";
            vendedor = null;
            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetSellerData(connection, id, ref result, out mensaje);
            }
            if (mensaje.Equals(""))
            {
                vendedor = result.FirstOrDefault();
                return true;
            }                
            else
                return false;
        }

        /*No implementados*/
        public Seller GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(Seller element, out String mensaje)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Seller element)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Seller> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }
                
        IEnumerable<Seller> ICRUDDAL<Seller>.Get()
        {
            throw new NotImplementedException();
        }

        Seller ICRUDDAL<Seller>.GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
