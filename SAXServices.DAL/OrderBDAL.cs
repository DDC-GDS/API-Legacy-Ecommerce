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
    public class OrderBDAL : CRUDDALBase, ICRUDDAL<OrderB>
    {
        private const string NO_VARIANTES = ".";
        
        public bool Delete(OrderB element)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OrderB> Get()
        {
            var result = new List<OrderB>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetOrderData(connection, 0, ref result);
            }

            return result;
        }

        public IEnumerable<OrderB> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public OrderB GetById(int id)
        {
            var result = new List<OrderB>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetOrderData(connection, id, ref result);
            }

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Obtiene los datos de las ordenes de compra en una base.
        /// Si se indica el id obtiene la orden puntual asociada a ese ID
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="result"></param>
        private void GetOrderData(ConnectionStringSettings connection, int id, ref List<OrderB> result)
        {
            if (OpenDBConnection(connection.ConnectionString))
            {
                var orderDetailList = new Dictionary<int, List<OrderBDetail>>();

                //Paso 1: Busco los detalles de todas las OP
                var sSql = "select ocd.[ID], ocd.[Producto_ID], ocd.[Tamaño], ocd.[Cantidad], ocd.[precio_unitario], ocd.[Estado], oc.[N_Ord_Com] " +
                    "from [dbo].[Orden_De_Compra_Cliente_Detalle] ocd " +
                    "INNER JOIN [dbo].[Orden_De_Compra_Cliente] oc ON oc.Numero_Orden = ocd.ID " +
                    "WHERE oc.Pedido_WEB = 1 ";
                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND oc.[N_Ord_Com] = 'EC_{0}'", id);//Agrego el EC_ para buscar

                sSql +=" UNION ALL " +
                "select ocd.[ID], ocd.[Producto_ID], ocd.[Tamaño], ocd.[Cantidad], ocd.[precio_unitario], ocd.[Estado], och.[N_Ord_Com] " +
                "from [dbo].[Orden_De_Compra_Cliente_Detalle] ocd " +
                "INNER JOIN [dbo].[Orden_De_Compra_Cliente_H] och ON och.Numero_Orden = ocd.ID " +
                "WHERE och.Pedido_WEB = 1 ";
                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND och.[N_Ord_Com] = 'EC_{0}'", id);//Agrego el EC_ para buscar

                sSql += " UNION ALL " +
                    "select ocdh.[ID], ocdh.[Producto_ID], ocdh.[Tamaño], ocdh.[Cantidad], ocdh.[precio_unitario], ocdh.[Estado], oc.[N_Ord_Com] " +
                    "from [dbo].[Orden_De_Compra_Cliente_Detalle_H] ocdh " +
                    "INNER JOIN[dbo].[Orden_De_Compra_Cliente] oc ON oc.Numero_Orden = ocdh.ID " +
                    "WHERE oc.Pedido_WEB = 1 ";
                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND oc.[N_Ord_Com] = 'EC_{0}'", id);//Agrego el EC_ para buscar

                sSql += " UNION ALL " +
                    "select ocdh.[ID], ocdh.[Producto_ID], ocdh.[Tamaño], ocdh.[Cantidad], ocdh.[precio_unitario], ocdh.[Estado], och.[N_Ord_Com] " +
                    "from [dbo].[Orden_De_Compra_Cliente_Detalle_H] ocdh " +
                    "INNER JOIN[dbo].[Orden_De_Compra_Cliente_H] och ON och.Numero_Orden = ocdh.ID " +
                    "WHERE och.Pedido_WEB = 1";
                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND och.[N_Ord_Com] = 'EC_{0}'", id);//Agrego el EC_ para buscar

                using (var sqlCommand = new SqlCommand(sSql, oConexion))
                {
                    //aqui
                    SqlCommand sqlCommand2 = new SqlCommand("select [CodParametro],[Parametro] from [dbo].[Parametros] where [CodParametro] = 'CANT_AGRUPA'", oConexion);
                    var rsp2 = sqlCommand2.ExecuteReader();
                    rsp2.Read();
                    int longitud = Int32.Parse(rsp2["Parametro"].ToString());
                    rsp2.Close();

                    var rsp = sqlCommand.ExecuteReader();

                    while (rsp.Read())
                    {
                        var order_Id = int.Parse(rsp["N_Ord_Com"].ToString().Replace("EC_","")); //Agrego sacar el EC_ con el skip 3.

                        if (!orderDetailList.ContainsKey((int)rsp["ID"]))
                        {
                            orderDetailList.Add((int)rsp["ID"], new List<OrderBDetail>());
                        }
//                      aqui
//                      var product_id = String.Concat(rsp["Producto_ID"].ToString().TakeWhile(c => Char.IsNumber(c)));
                        var product_id = rsp["Producto_ID"].ToString().Length > longitud ? rsp["Producto_ID"].ToString().Substring(0, longitud) : rsp["Producto_ID"].ToString();

                        orderDetailList[(int)rsp["ID"]].Add(
                            new OrderBDetail
                            {
                                AttributeValue = rsp["Tamaño"].ToString(),
                                //Order_id = order_Id,
                                Quantity = (decimal)rsp["Cantidad"],
                                State = (int)rsp["Estado"],
                                Price = (decimal)rsp["precio_unitario"],
                                Product_Id = product_id,
                                Product_Name = product_id,
                                ProductVariation_Id = product_id + rsp["Tamaño"].ToString(),
                            });
                    }

                    rsp.Close();
                }

                ///Paso 2: Busco las cabeceras de las OP y armo los pedidos
                sSql = "SELECT oc.[Numero_Orden],oc.[ID_Cliente],oc.[Fecha_Emision],oc.[Estado],oc.[fecha_vto],oc.[id_vendedor],oc.[suc_cli],oc.[usrGeneracion],oc.[N_Ord_Com] " +
                    "FROM [dbo].[Orden_De_Compra_Cliente] oc "+
                    "WHERE oc.Pedido_WEB = 1 ";

                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND oc.[N_Ord_Com] = 'EC_{0}'", id);//Agrego EC_ para buscar

                sSql += " UNION ALL ";
                sSql += "SELECT och.[Numero_Orden],och.[ID_Cliente],och.[Fecha_Emision],och.[Estado],och.[fecha_vto],och.[id_vendedor],och.[suc_cli],och.[usrGeneracion],och.[N_Ord_Com] " +
                        "FROM [dbo].[Orden_De_Compra_Cliente_H] och "+
                        "WHERE och.Pedido_WEB = 1 ";

                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "AND och.[N_Ord_Com] = 'EC_{0}'", id);//Agrego el EC_ para buscar

                using (var sqlCommand = new SqlCommand(sSql, oConexion))
                {
                    var rsp = sqlCommand.ExecuteReader();

                    while (rsp.Read())
                    {
                        OrderB order;

                        var order_Id = int.Parse(rsp["N_Ord_Com"].ToString().Replace("EC_", ""));//Agrego sacar el EC_ con el skip 3.
                        var id_sax = (int)rsp["Numero_Orden"];

                        //Pregunto si la orden ya existe.
                        if (!result.Exists(o => o.Order_id == order_Id))
                        {
                            order = new OrderB
                            {
                                Order_id = order_Id,
                                Client_ID = (int)rsp["ID_Cliente"],
                                Fecha_Emision = (DateTime)rsp["Fecha_Emision"],
                                Fecha_Vto = (DateTime)rsp["fecha_vto"],
                                Seller_Id = (int)rsp["id_vendedor"],
                                SucName = rsp["suc_cli"].ToString(),
                                User_ID = rsp["usrGeneracion"].ToString(),
                                Detail = new List<OrderBDetail>()
                            };

                            result.Add(order);
                        }
                        else //La Orden ya existe. A nivel datos tengo que actualizar estados
                        {
                            order = result.FirstOrDefault(o => o.Order_id  == order_Id);
                        }

                        if (orderDetailList.ContainsKey(id_sax)) order.Detail.AddRange(orderDetailList[id_sax]);
                        order.CalculateState((int)rsp["Estado"]);
                    }

                    rsp.Close();
                }

                CloseDBConnection();
            }
        }

        public OrderB GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(OrderB element, out String mensaje)
        {
            var connections = ConfigurationManager.ConnectionStrings;
            var result = true;
            mensaje = "0";
            foreach (ConnectionStringSettings connection in connections)
            {                
                if (element.Detail.Exists(d => d.PriceList_Name.Substring(0, connection.Name.Length).ToString() == connection.Name))
                {
                    //09/12/2019 ITO : DESA-952 Separar los pedidos por marca
                    var serviceMarca = ConfigurationManager.AppSettings[connection.Name];
                    if (serviceMarca == "true")
                    {
                        var marcas = element.Detail.GroupBy(d => d.Category,
                          (codigo) => new
                          {
                              Key = codigo
                          });
                        foreach (var marca in marcas)
                        {

                            var newOrder = new OrderB
                            {
                                Client_ID = element.Client_ID,
                                SucName = element.SucName,
                                User_ID = element.User_ID,
                                Fecha_Emision = element.Fecha_Emision,
                                Fecha_Vto = element.Fecha_Vto,
                                NroOrdenCompra = element.NroOrdenCompra,
                                Seller_Id = element.Seller_Id,
                                UserAction = element.UserAction,
                                CanalVentas = element.CanalVentas,
                                TipoEnvio = element.TipoEnvio,
                                CodigoPedidoEC = element.CodigoPedidoEC,
                                Detail = element.Detail.Where(d => d.PriceList_Name.Substring(0, connection.Name.Length).ToString() == connection.Name && d.Category == marca.Key).ToList()
                            };

                            result &= SaveOrder(connection, newOrder, out mensaje);
                        }
                    }
                    else
                    {
                        var newOrder = new OrderB
                    {
                        Client_ID = element.Client_ID,
                        SucName = element.SucName,
                        User_ID = element.User_ID,
                        Fecha_Emision = element.Fecha_Emision,
                        Fecha_Vto = element.Fecha_Vto,
                        NroOrdenCompra = element.NroOrdenCompra ,
                        Seller_Id = element.Seller_Id,
                        UserAction = element.UserAction,
                        CanalVentas = element.CanalVentas ,
                        TipoEnvio =element.TipoEnvio, 
                        CodigoPedidoEC = element.CodigoPedidoEC, 
                        Detail = element.Detail.Where(d => d.PriceList_Name.Substring(0, connection.Name.Length).ToString() == connection.Name).ToList()
                    };
                    result &= SaveOrder(connection, newOrder,out mensaje);
                    }

                }
            }
            if (result) 
                element.Order_id = Int32.Parse(mensaje);
            else 
                element.Order_id = 0; 
            return result;
        }

        private bool SaveOrder(ConnectionStringSettings connection, OrderB order, out String mensaje)
        {
            try
            {
                mensaje = "0";
                if (OpenDBConnection(connection.ConnectionString))
                {
                    string sSql;

                    //Paso 1: Traigo el proximo numero de pedido
                    var sNumero_Orden = "1";

                    //02/03/2020 ITO:ECM-36 Numero de los Pedidos web.
                    //               var sSql =
                    //                  "SELECT IsNull(MAX(Numero_Orden), 0) + 1 Proximo_Numero " +
                    //                  "FROM Orden_De_Compra_Cliente";
                    sSql = "SELECT IsNull(MAX(Num), 0) + 1 as Proximo_Numero FROM(SELECT(numero_orden) AS Num FROM orden_de_compra_cliente UNION SELECT(numero_orden) AS Num FROM orden_de_compra_cliente_h) u";

                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        var rsp = sqlCommand.ExecuteReader();

                        if (rsp.Read())
                        {
                            sNumero_Orden = rsp["Proximo_Numero"].ToString();
                        }

                        if (rsp != null)
                        {
                            if (!rsp.IsClosed)
                            {
                                rsp.Close();
                            }
                        }
                    }

                    //Paso 1.1: Traigo el depósito y la condición de pago del cliente
                    string tipoDeposito = null;
                    int id_CP = 0;

                    if (String.IsNullOrEmpty(order.SucName))
                    {
                        sSql = "SELECT Tipo_Deposito,ISNULL(ID_Condicion_pago, 0) as ID_CP " +
                        "FROM [dbo].[Clientes] " +
                        "WHERE Cliente_ID = " + order.Client_ID;
                    }
                    else
                    {
                        sSql = "SELECT cs.deposito as Tipo_Deposito, ISNULL(c.ID_Condicion_pago, 0) as ID_CP " +
                        "FROM Clientes_Sucursal cs " +
                        "INNER JOIN [dbo].[Clientes] c ON cs.Cliente_ID = c.Cliente_ID " +
                        "WHERE cs.Cliente_ID = " + order.Client_ID + " AND cs.Sucursal = " + order.SucName;
                    }

                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        var rsp = sqlCommand.ExecuteReader();

                        if (rsp.Read())
                        {
                            tipoDeposito = rsp["Tipo_Deposito"].ToString();
                            id_CP = (int)rsp["ID_CP"];
                        }

                        if (rsp != null)
                        {
                            if (!rsp.IsClosed)
                            {
                                rsp.Close();
                            }
                        }
                    }


                    //Paso 2: Grabo la cabecera del pedido
                    var log = String.Format(CultureInfo.CurrentCulture, "{0}{1}", DateTime.Now.ToString(), order.User_ID);

                    //05/05/2020 ITO:ECM-40 Modificación en el envío de las observaciones de los pedidos.
                    var str20200506075501 = "";
                    var strUserActionTmp = order.UserAction.Trim().Substring(0, 1);
                    if (order.UserAction.Trim().Length > 1)
                    {
                        str20200506075501 = order.UserAction.Trim().Substring(1);
                        strUserActionTmp = strUserActionTmp.Substring(0, 1);
                    }
                    //26/09/2022
                    var strPedidoObservacion = "";
                    switch (strUserActionTmp.Trim())
                    {
                        case "A":
                            strPedidoObservacion = "Anula pedido anterior" + str20200506075501;
                            break;
                        case "G":
                            strPedidoObservacion = "Agrega al pedido anterior" + str20200506075501;
                            break;
                        case "V":
                            strPedidoObservacion = "";
                            break;
                    }

                    strPedidoObservacion += " - TipoEnvio:" + order.TipoEnvio;

                    OpenDBTransaction();

                    sSql = "INSERT INTO Orden_De_Compra_Cliente " +
                        "([ID_Cliente]" +
                        ",[Numero_Orden]" +
                        ",[Fecha_Emision]" +
                        ",[Observaciones]" +
                        ",[fecha_vto]" +
                        ",[Log]" +
                        ",[ID_CP]" +
                        ",[Tipo_Deposito]" +
                        ",[DESCUENTO]" +
                        ",[id_vendedor]" +
                        ",[usrGeneracion]" +
                        ",[suc_cli]" +
                        ",[N_Ord_Com]" +
                        ",[Estado]" +
                        ",[Pedido_WEB]" +
                        ",[ID_Moneda]" +
                        ",[Moneda_Cotizacion]" +
                        ")" +
                        "VALUES" +
                        "(" + order.Client_ID +
                        "," + sNumero_Orden +
                        ",'" + order.Fecha_Emision.ToString("dd-MM-yyyy") + "'" +
                        //05/05/2020 ITO:ECM-40 Modificación en el envío de las observaciones de los pedidos.
                        //",'" + (order.UserAction.Trim() == "A" ? "Anula pedido anterior" : "Agrega al pedido anterior") + "'" +
                        //26/09/2022 Posibilidad de no enviar observacion --> "V"
                        //",'" + (strUserActionTmp.Trim() == "A" ? "Anula pedido anterior" : "Agrega al pedido anterior") + str20200506075501 + "'" +
                        ",'" + strPedidoObservacion + "'" +
                        ",'" + order.Fecha_Vto.ToString("dd-MM-yyyy") + "'" +
                        ",'" + log + "'" +
                        "," + id_CP +
                        "," + (String.IsNullOrEmpty(tipoDeposito) ? "NULL" : tipoDeposito) +
                        ",0" +
                        //18/02/2020 ITO : ECM-19 Vendedor No Obligatorio.
                        //"," + order.Seller_Id +
                        "," + (String.IsNullOrEmpty(order.Seller_Id.ToString()) ? 0 : order.Seller_Id) +
                        ",'" + log + "'" +
                        ",'" + (String.IsNullOrEmpty(order.SucName) ? "" : order.SucName) + "'" +
                        ",'" + order.CanalVentas.Trim() + "_" + order.NroOrdenCompra + "/codigo pedido:" + order.CodigoPedidoEC.Trim() + "'" +
                       ",2" + //Estado
                        ",1" +
                        ",1" +
                        ",1" +
                        ")";

                                        
                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        sqlCommand.Transaction = oTran;
                        sqlCommand.ExecuteNonQuery();
                    }

                    //Paso 3: Actualizo el ultimo comprobante generado
                    sSql = "UPDATE comprobantes_tipo SET ULTIMO_NUMERO = " + sNumero_Orden + "  WHERE id = 'CC'";

                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        sqlCommand.Transaction = oTran;
                        sqlCommand.ExecuteNonQuery();
                    }

                    //Paso 4: Grabo el detalle del pedido
                    for (int i = 1; i <= order.Detail.Count; i++)
                    {
                        try
                        {
                            //JORGE: Considerar productos sin talle color (catálogos)
                            if (order.Detail[i - 1].ProductVariation_Id == order.Detail[i - 1].Product_Id &&
                               !order.Detail[i - 1].Product_Id.Equals(ConfigurationManager.AppSettings["descuento"]) && !order.Detail[i - 1].Product_Id.Equals(ConfigurationManager.AppSettings["costoEnvio"])) //DESA-2235 PILAR
                            {
                                order.Detail[i - 1].Product_Name += NO_VARIANTES;
                                order.Detail[i - 1].AttributeValue = NO_VARIANTES;
                            }
                            //DESA-2235 PILAR
                            else if (order.Detail[i - 1].Product_Id.Equals(ConfigurationManager.AppSettings["descuento"]) || order.Detail[i - 1].Product_Id.Equals(ConfigurationManager.AppSettings["costoEnvio"]))
                                order.Detail[i - 1].AttributeValue = ConfigurationManager.AppSettings["tamanioGenerico"];
                            //------------------------------------------------------------------------DESA-2235 PILAR

                            sSql = "INSERT INTO [dbo].[Orden_De_Compra_Cliente_Detalle]" +
                                "([ID]" +
                                ",[Linea]" +
                                ",[Producto_ID]" +
                                ",[Tamaño]" +
                                ",[Cantidad]" +
                                ",[Descripcion_concepto]" +
                                ",[precio_unitario]" +
                                //19/02/2020 ITO : ECM-27 Envio de Precio_unitario.
                                ",[MM_precio_unitario]" +
                                ",[Cantidad_Pendiente]" +
                                ",[impo_Desc]" +
                                ",[Porc_Desc]" +
                                ",[Estado]" +
                                ",[ID_Almacen]" +
                                ",[PorcentajeIVA]" +
                                ")" +
                                "VALUES" +
                                "(" + sNumero_Orden +
                                "," + i +
                                ",'" + order.Detail[i - 1].Product_Name + "'" +
                                ",'" + order.Detail[i - 1].AttributeValue + "'" +
                                "," + order.Detail[i - 1].Quantity.ToString().Replace(",", ".") +
                                "," + String.Format(CultureInfo.CurrentCulture, "'{0} : {1} : {2}'", order.Detail[i - 1].Product_Name, order.Detail[i - 1].Product_Description, order.Detail[i - 1].AttributeValue) +
                                "," + order.Detail[i - 1].Price.ToString().Replace(",", ".") +
                                //19/02/2020 ITO : ECM-27 Envio de Precio_unitario.
                                "," + order.Detail[i - 1].Price.ToString().Replace(",", ".") +
                                "," + order.Detail[i - 1].Quantity.ToString().Replace(",", ".") +
                                ",0" +
                                ",0" +
                                ",2" + //Estado
                                ",1" +
                                ",21" +
                                ")";

                            using (var sqlCommand = new SqlCommand(sSql, oConexion))
                            {
                                sqlCommand.Transaction = oTran;
                                sqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            RollBackDBTransaction();
                            mensaje = "Error al guardar producto en la orden. Producto: " + order.Detail[i - 1].Product_Name + ". " + Environment.NewLine +  ex.Message;                            
                            return false;
                        }
                    }
                    CommitDBTransaction();
                    CloseDBConnection();
                    mensaje = sNumero_Orden;
                }
                return true;
            }
            catch (Exception ex)
            {
                RollBackDBTransaction();
                mensaje = "Error al guardar la orden. " +  ex.Message;                
                return false;
            }
            
        }
    }
}
