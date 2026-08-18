using SAXServices.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace SAXServices.DAL
{
    public class PriceListDAL : CRUDDALBase, ICRUDDALPriceList<PriceList>
    {
        private const string NO_VARIANTES = ".";
        
        public Boolean  Get(out String mensaje,out IEnumerable<PriceList> listas)
        {
            mensaje = "Error PriceListDAL";                                
            var connections = ConfigurationManager.ConnectionStrings;

            listas = new List<PriceList>();
            foreach (ConnectionStringSettings connection in connections)
            {
                listas=GetPriceList(connection, DateTime.Now,out mensaje);
                return (mensaje.Equals(""));
                //result.AddRange(GetPriceList(connection, DateTime.Now,mensaje));
            }
            return false;
        }

        public Boolean GetByName(String name, out String mensaje, out PriceList listaPrecio)
        {
            var connections = ConfigurationManager.ConnectionStrings;
            IEnumerable<PriceList> listas = new List<PriceList>();
            mensaje = "Error PriceListDAL";
            listaPrecio = null;
            try
            {

                foreach (ConnectionStringSettings connection in connections)
                {
                    if (name.Substring(0, connection.Name.Length) == connection.Name && name.Length > connection.Name.Length)
                    {
                        int nameLongitud = name.Length - connection.Name.Length;
                        String listaNombre = name.Substring(connection.Name.Length,
                                                            nameLongitud);

                        listas = GetPriceList(connection, DateTime.Now, out mensaje, listaNombre);
                        if (mensaje.Equals(""))
                        {
                            listaPrecio = new PriceList();
                            if (listas.Count() > 0)
                            {
                                listaPrecio = listas.First();
                                mensaje = "Cantidad de productos: " + listaPrecio.Items.Count.ToString();
                            }
                            else
                                mensaje = "No se encontraron productos para la lista";

                            return true;
                        }
                    }
                    else mensaje = "Error en nombre de lista - PriceListDAL";
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error en nombre de lista: " +  ex.Message;                
            }
            return false;
        }

        public Boolean GetByDate(DateTime fecha, out String mensaje, out IEnumerable<PriceList> listas)
        {            
            mensaje = "Error PriceListDAL";
            var connections = ConfigurationManager.ConnectionStrings;
            listas = new List<PriceList>();

            foreach (ConnectionStringSettings connection in connections)
            {
                //result.AddRange(GetPriceList(connection, fecha));
                listas = GetPriceList(connection, fecha , out mensaje);
                return (mensaje.Equals(""));
            }
            return false;           
        }

        private IEnumerable<PriceList> GetPriceList(ConnectionStringSettings connection, DateTime fechaVigencia, out String mensaje, String listaNombre = "")
        {
            var result = new List<PriceList>();
            var listaPrecios = new Dictionary <int,PriceList>();
            String product_id = "";

            try{

                if (OpenDBConnection(connection.ConnectionString))
                {
                    var priceListItems = new Dictionary<int, List<PriceListItem>>();

                    //DESA-2553 Pilar
                    String whereNombre = "";
                    String whereHNombre = "";
                    if (!listaNombre.Equals(""))
                    {
                        whereNombre = " AND lista.Descripcion like '" + listaNombre + "' ";
                        whereHNombre = " AND lista_h.Descripcion like '" + listaNombre + "' ";
                    }
                    //-------------------------------------------------------------------------------------------/DESA-2553 Pilar

                    //Paso 1: Busco todos los productos/precios

                    var sSql = String.Format(CultureInfo.CurrentCulture,
                        "SELECT lista.descripcion,lista.modificado,pp.[id_lista_precios],pp.[Precio],pp.[Producto_ID],pp.[Tamaño],pp.[FVigencia],pp.[FHasta],c.Abreviado as Color " +
                        //DESA-2262 Pilar - agrego join con listas_precio para acotar el resultado de precios a las listas web activas, y join con productos para tomar el color,
                        //saco comparación  ON pp.Producto_Id LIKE '%' + c.Abreviado " el like para optimizar la consulta
                        //"FROM [dbo].[Productos_Precios] pp " +
                        "FROM(SELECT[Id_lista_precios], [Descripcion], [Modificado], [web] FROM[dbo].[Listas_Precio] WHERE Web = 1 AND activo =1) as lista " +
                        "INNER JOIN [dbo].[Productos_Precios] pp on lista.id_lista_precios = pp.id_lista_precios " +
                        "INNER JOIN Productos p on p.id = pp.Producto_ID " +
                        "INNER JOIN Colores c ON c.id = p.color " +
                        //"INNER JOIN Colores c ON pp.Producto_Id LIKE '%' + c.Abreviado " +
                        //-------------------------------------------------------------------------------------------DESA-2262 Pilar                    
                        "INNER JOIN Productos_Stock st ON pp.Producto_Id = st.Producto_ID " + //Filtrar por Productos tipo Web
                        "WHERE Convert(date, pp.FVigencia) <= '{0}' and Convert(date, pp.FHasta) >= '{0}' and st.Web = 1 and st.activo = 1 and p.existe=1 " + whereNombre +                         
                    "UNION " +
                        "SELECT lista_h.descripcion,lista_h.modificado,pph.[id_lista_precios],pph.[Precio],pph.[Producto_ID],pph.[Tamaño],pph.[FVigencia],pph.[FHasta],c.Abreviado as Color " +
                        //DESA-2262 Pilar - agrego join con listas_precio para acotar el resultado de precios a las listas web activas, y join con productos para tomar el color,
                        //saco comparación  ON pp.Producto_Id LIKE '%' + c.Abreviado " el like para optimizar la consulta
                        //"FROM [dbo].[Productos_Precios] pp " +
                        "FROM(SELECT[Id_lista_precios], [Descripcion], [Modificado], [web] FROM[dbo].[Listas_Precio] WHERE Web = 1 AND activo =1) as lista_h " +
                        "INNER JOIN [dbo].[Productos_Precios_H] pph on lista_h.id_lista_precios = pph.id_lista_precios " +
                        "INNER JOIN Productos p on p.id = pph.Producto_ID " +
                        "INNER JOIN Colores c ON c.id = p.color " +
                         //"INNER JOIN Colores c ON pph.Producto_Id LIKE '%' + c.Abreviado " +
                         //-------------------------------------------------------------------------------------------DESA-2262 Pilar                    
                         "INNER JOIN Productos_Stock st ON pph.Producto_Id = st.Producto_ID " + //Filtrar por Productos tipo Web
                        "WHERE st.Web = 1  and Convert(date, pph.FVigencia) <= '{0}' and Convert(date, pph.FHasta) >= '{0}' and st.activo = 1 and p.existe=1 " + whereHNombre, fechaVigencia.ToString("yyyyMMdd", CultureInfo.CurrentCulture));

                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        //aqui
                        SqlCommand sqlCommand2 = new SqlCommand("select [CodParametro],[Parametro] from [dbo].[Parametros] where [CodParametro] = 'CANT_AGRUPA'", oConexion);
                        var rsp2 = sqlCommand2.ExecuteReader();
                        sqlCommand.CommandTimeout = 60;
                        rsp2.Read();
                        int longitud = Int32.Parse(rsp2["Parametro"].ToString());
                        rsp2.Close();

                        var rsp = sqlCommand.ExecuteReader();

                        while (rsp.Read())
                        {
                            if (!priceListItems.ContainsKey((int)rsp["id_lista_precios"]))
                            {
                                priceListItems.Add((int)rsp["id_lista_precios"], new List<PriceListItem>());
                            }
                            //                      aqui
                            //                      var product_id = String.Concat(rsp["Producto_ID"].ToString().TakeWhile(c => Char.IsNumber(c)));

                            //DESA-2367 Pilar
                            if (connection.Name.Equals("AL"))
                            {
                                product_id = rsp["Producto_ID"].ToString().Substring(0, rsp["Producto_ID"].ToString().IndexOf("-") + 1);
                            }
                            //----------------------------------------------DESA-2367 Pilar
                            else
                            {
                                product_id = rsp["Producto_ID"].ToString().Length > longitud ? rsp["Producto_ID"].ToString().Substring(0, longitud) : rsp["Producto_ID"].ToString();
                            }

                            var color = rsp["Color"].ToString().Trim();

                            priceListItems[(int)rsp["id_lista_precios"]].Add(
                                new PriceListItem
                                {
                                    Price = (decimal)rsp["Precio"],
                                    Product_Id = product_id,
                                    ProductVariation_Id = (color == NO_VARIANTES ? product_id : product_id + color + rsp["Tamaño"].ToString()),
                                    FechaVigencia = (DateTime)rsp["FVigencia"],
                                    FechaHasta = (DateTime)rsp["FHasta"]
                                });

                            if (!listaPrecios.ContainsKey((int)rsp["id_lista_precios"]))
                            {
                                listaPrecios.Add((int)rsp["id_lista_precios"], new PriceList
                                {
                                    Name = connection.Name + rsp["Descripcion"].ToString(),
                                    Modificado = (bool)rsp["Modificado"],
                                    Items = null
                                });
                            }
                        }

                        rsp.Close();
                    }


                    ///Paso 2: Busco las cabeceras de listas de precios y armo las listas
                    //              sSql = "SELECT [Id_lista_precios], [Descripcion], [Modificado] FROM [dbo].[Listas_Precio]";
                    //DESA-2262 Pilar
                    /*sSql = "SELECT [Id_lista_precios], [Descripcion], [Modificado], [web] FROM [dbo].[Listas_Precio]";
                    sSql += " where [web] = 1";
                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        var rsp = sqlCommand.ExecuteReader();

                        while (rsp.Read())                
                        {
                            if (priceListItems.ContainsKey((int)rsp["Id_lista_precios"]))
                            {
                                result.Add(
                                    new PriceList
                                    {
                                        Name = connection.Name + rsp["Descripcion"].ToString(),
                                        Modificado = (bool)rsp["Modificado"],
                                        Items = priceListItems[(int)rsp["Id_lista_precios"]]
                                    });
                            }
                        }

                        /*rsp.Close();
                    }*/

                    foreach (var lista in listaPrecios)
                    {

                        result.Add(
                                    new PriceList
                                    {
                                        Name = lista.Value.Name,
                                        Modificado = lista.Value.Modificado,
                                        Items = priceListItems[lista.Key]
                                    }
                                  );
                    }

                    CloseDBConnection();
                }
                mensaje = "";
                return result;
            }
            catch (Exception ex)
            {
                CloseDBConnection();
                mensaje = "Error al buscar listas de precio. " + ex.Message;
                return null;            
            }            
        }

        private void If(bool v)
        {
            throw new NotImplementedException();
        }      

        public bool Save(PriceList element, out String mensaje)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PriceList> Get()
        {
            throw new NotImplementedException();
        }

        public PriceList GetById(int id)
        {
            throw new NotImplementedException();
        }
        public Boolean Delete(PriceList element)
        {
            throw new NotImplementedException();
        }

        PriceList ICRUDDAL<PriceList>.GetByName(string name)
        {
            throw new NotImplementedException();
        }

        IEnumerable<PriceList> ICRUDDAL<PriceList>.GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }
    }
}
