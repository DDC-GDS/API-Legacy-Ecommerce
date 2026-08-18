using SAXServices.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SAXServices.DAL
{
    public class ProductDAL : CRUDDALBase, ICRUDDAL<Product>
    {
        private const string NO_VARIANTES = ".";

        public bool Delete(Product element)
        {
            throw new NotImplementedException();
        }

        private void GetProducts(ConnectionStringSettings connection, string id, ref List<Product> result)
        {
            if (OpenDBConnection(connection.ConnectionString))
            {
                var productVariations = new Dictionary<string, List<ProductVariation>>();
                //'21/01/2020 ITO:ECM-8 Nombre de categorías en los productos - GRIMAN
                var categoria = ConfigurationManager.AppSettings["CAT"];
                var ContCategoria = "";
                if (categoria == "true")
                {
                    ContCategoria = ",N2.Nivel2_Descripcion as Category";
                }
                else
                {
                    ContCategoria = ",CASE WHEN N2.N2_ID = 'CA' THEN N2.Nivel2_Descripcion ELSE N1.Descripcion END as Category";
                }
                //Paso 1: Busco todos los productos
                var sSql = "SELECT "
                    + "ps.[Producto_ID]" +
                    ",ps.[Tamaño]" +
                    ",ps.[Stock_PT]" +
                    /*DESA-2053 17/4/2024  PILAR ",CASE WHEN ps.[LOG] IS NULL THEN CONVERT(datetime, '01/01/1990', 103) " +
                    "ELSE CONVERT(datetime, SUBSTRING(ps.[LOG], 1, 19), 103) END as Last_Update" +*/
                    ",ps.[LOG] as Last_Update" +   /*DESA-2053 17/4/2024  PILAR*/
                    ",p.[Descripcion]" +
                    ",t.[Descripcion] as Talle" +
                    ",c.Abreviado as Color" +
                    ",c.Descripcion as ColorDesc" +

                    //'21/01/2020 ITO:ECM-8 Nombre de categorías en los productos - GRIMAN
                    //",CASE WHEN N2.N2_ID = 'CA' THEN N2.Nivel2_Descripcion " +
                    //"ELSE N1.Descripcion END as Category" +

                     ContCategoria +

                    ",t.[Orden] as Orden" +
                    " FROM [dbo].[Productos_Stock] ps" +
                    " INNER JOIN [dbo].[Productos] p ON ps.Producto_ID = p.ID" +
                    " INNER JOIN [dbo].[Tamaños] t on t.id = ps.Tamaño" +
                    /*DESA-2261 */
                    //" INNER JOIN [dbo].[Colores] c ON ps.Producto_Id LIKE '%' + c.Abreviado " +
                    " INNER JOIN [dbo].[Colores] c ON p.color =  c.ID " +
                    /*DESA-2261 */
                    " LEFT JOIN [dbo].[Producto_Nivel_1] N1 ON N1.N1_ID = P.N1_ID" +
                    " LEFT JOIN [dbo].[Producto_Nivel_2] N2 ON N2.N2_ID = P.N2_ID" +
                    " WHERE ps.Activo = 1 AND p.Existe = 1  AND ps.Web = 1 ";
                    //DESA-961 Envío de productos por regla de negocio
                    //" AND LEFT(ps.Producto_ID, 2) IN ('15', '17', '60', '80') "; //JORGE: Según requerimiento DESA-116

                        if (!String.IsNullOrEmpty(id)) sSql += String.Format(CultureInfo.CurrentCulture, "AND Producto_ID LIKE '{0}%'", id);

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
                                Product product;
                                ProductVariation currentColor;
                                String product_id;

                                //  aqui
                                // var product_id = String.Concat(rsp["Producto_ID"].ToString().TakeWhile(c=>Char.IsNumber(c)));
                                //DESA-2235 Pilar agrego if
                                //product_id = rsp["Producto_ID"].ToString().Length > longitud ? rsp["Producto_ID"].ToString().Substring(0, longitud) : rsp["Producto_ID"].ToString();
                                if (!rsp["Producto_ID"].ToString().ToUpper().Equals(ConfigurationManager.AppSettings["descuento"].ToUpper()) && 
                                    !rsp["Producto_ID"].ToString().ToUpper().Equals(ConfigurationManager.AppSettings["costoEnvio"].ToUpper()))
                                {
                                    if (connection.Name.Equals("AL")){
                                        product_id = rsp["Producto_ID"].ToString().Substring(0, rsp["Producto_ID"].ToString().IndexOf("-")+1);
                                    }
                                    else{
                                        product_id = rsp["Producto_ID"].ToString().Length > longitud ? rsp["Producto_ID"].ToString().Substring(0, longitud) : rsp["Producto_ID"].ToString();
                                    }
                                }
                                else
                                    product_id = rsp["Producto_ID"].ToString();
                                //-------------------------------------------------------DESA-2235 Pilar
                                var color = rsp["Color"].ToString().Trim();
                                var description = rsp["Descripcion"].ToString();

                                if (description.Contains('-'))
                                {
                                    var arrDesc = description.Reverse().SkipWhile(c => c != '-').Reverse().ToArray();
                                    description = new string(arrDesc);
                                }

                                //Paso 1: Pregunto si el producto ya existe.
                                if (!result.Exists(p => p.Product_Id == product_id))
                                {
                                    /*DESA-2053 17/4/2024  PILAR*/
                            DateTime  logDia;
                            String    diaTiempo;
                            try {
                                if (rsp["Last_Update"]==DBNull.Value)
                                {
                                    logDia = new DateTime (1990,01,01);
                                }
                                else
                                {
                                    diaTiempo = rsp["Last_Update"].ToString();
                                    diaTiempo  = diaTiempo.Substring(0, 19);
                                    logDia = DateTime.Parse(diaTiempo);
                                }                                
                                
                            }
                            catch (Exception ex)
                            {
                                logDia = DateTime.Now; 
                            }
                            /*************DESA - 2053 17 / 4 / 2024  PILAR*/

                            product = new Product
                            {
                                Product_Id = product_id,
                                LastUpdate = logDia,  /*DESA-2053 17/4/2024  PILAR (DateTime)rsp["Last_Update"],*/
                                Name = product_id,
                                Description = description,
                                Category = rsp["Category"].ToString(),
                                Manage_Stock = color == NO_VARIANTES,
                                Stock = (color == NO_VARIANTES ? (decimal)rsp["Stock_PT"] : 0),
                                Type = (color == NO_VARIANTES ? ProductType.simple : ProductType.variable),
                                Variations = new List<ProductVariation>()
                            };

                            result.Add(product);
                        }
                        else //si ya existe, lo obtengo
                        {
                            product = result.First(p => p.Product_Id == product_id);
                            product.Stock += (color == NO_VARIANTES ? (decimal)rsp["Stock_PT"] : 0);                            
                        }

                        //Paso 2: Pregunto si el producto tiene colores
                        if (color != NO_VARIANTES)
                        {
                            //Paso 2: Pregunto si el color existe para ese producto
                            if (!product.Variations.Exists(v => v.AttributeValue == color))
                            {
                                currentColor = new ProductVariation
                                {
                                    ParentProduct_Id = product_id,
                                    ProductVariation_Id = product_id+color,
                                    AttributeName = "Color",
                                    AttributeValue = color,
                                    Description = rsp["ColorDesc"].ToString(),
                                    Manage_Stock = false,
                                    Stock = 0,
                                    Variations = new List<ProductVariation>()
                                };

                                product.Variations.Add(currentColor);
                            }
                            else //si ya existe, lo obtengo
                            {
                                currentColor = product.Variations.First(v => v.AttributeValue == color);
                            }

                            //Paso 3: Pregunto si el talle existe
                            if (!currentColor.Variations.Exists(v => v.AttributeValue == rsp["Tamaño"].ToString()))
                            {
                                var currentTalle = new ProductVariation
                                {
                                    ParentProduct_Id = product_id + color,
                                    ProductVariation_Id = product_id + color + rsp["Tamaño"].ToString(),
                                    AttributeName = "Talle",
                                    AttributeValue = rsp["Tamaño"].ToString(),
                                    Description = rsp["Talle"].ToString(),
                                    Order = (int)rsp["Orden"],
                                    Manage_Stock = true,
                                    
                                    /*DESA-2200 Pilar*/
                                    //Stock = rsp["Stock_PT"] == null ? (decimal)rsp["Stock_PT"] : (decimal)0.00,
                                    Stock = rsp["Stock_PT"] != null ? (decimal)rsp["Stock_PT"] : (decimal)0.00,
                                    /*-----------------------------DESA-2200 Pilar*/

                                    Variations = new List<ProductVariation>()
                                };

                                currentColor.Variations.Add(currentTalle);
                            }
                            else //si el talle existe, sólo sumo Stock
                            {
                                var currentTalle = currentColor.Variations.First(v => v.AttributeValue == rsp["Tamaño"].ToString());
                                /*DESA-2200 Pilar*/
                                //currentTalle.Stock += rsp["Stock_PT"] == null ? (decimal)rsp["Stock_PT"] : (decimal)0.00;
                                currentTalle.Stock += rsp["Stock_PT"] != null ? (decimal)rsp["Stock_PT"] : (decimal)0.00;
                                /*--------------------------------DESA-2200 Pilar*/
                            }
                        }
                    }

                    rsp.Close();
                }

                CloseDBConnection();
            }
        }


        public IEnumerable<Product> Get()
        {
            var result = new List<Product>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetProducts(connection, String.Empty, ref result);
            }

            return result;
        }

        public IEnumerable<Product> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Product GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Product GetByName(string name)
        {
            var connections = ConfigurationManager.ConnectionStrings;

            var result = new List<Product>();

            foreach (ConnectionStringSettings connection in connections)
            {
                GetProducts(connection, name, ref result);
            }

            return result.FirstOrDefault();
        }

        public bool Save(Product element,out String mensaje)
        {
            throw new NotImplementedException();
        }
    }
}
