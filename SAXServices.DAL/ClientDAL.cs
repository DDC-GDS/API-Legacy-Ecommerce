using System;
using System.Linq;
using System.Collections.Generic;
using SAXServices.Contracts;
using System.Configuration;
using System.Globalization;
using System.Data.SqlClient;


namespace SAXServices.DAL
{
    public class ClientDAL : CRUDDALBase, ICRUDDALClient<Client>, ICRUDDALClientWeb<ClienteWeb> 
    {
        private enum enumTipoFiscalBalcony {
            CF = 1,     //(consumidor final)
            RI,         //(responsable inscripto)
            MO,         //(monotributo)
            EX          //(exento )
        }
              

        public Boolean  Get(out String mensaje, out List<Client> clientes)
        {
            mensaje = "";
            clientes = new List<Client>();
            var result = new List<Client>();

            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetClientData(connection, 0, ref result, out mensaje);
            }
            if (mensaje.Equals(""))
            {
                mensaje = "Cantidad de clientes: " + result.Count.ToString(); 
                clientes = result; 
                return true;
            }
                
            else
                return false;            
        }

        private void GetClientData(ConnectionStringSettings connection, int id, ref List<Client> result, out String mensaje)
        {
            mensaje = "";
            try { 
              if (OpenDBConnection(connection.ConnectionString))
                {
                var sucItems = new Dictionary<int, List<ClientSuc>>();

                //Paso 1: Busco todos los productos/precios
                var sSql = "SELECT cs.[Cliente_Id], cs.[Sucursal], cs.[vendedor], lp.[Descripcion] as Lista_precio " +
                    "FROM [dbo].[Clientes_Sucursal] cs " +
                    "INNER JOIN Listas_Precio lp ON lp.Id_lista_precios = cs.ListaPrecio ";


                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "WHERE Cliente_Id={0}", id);

                using (var sqlCommand = new SqlCommand(sSql, oConexion))
                {
                    var rsp = sqlCommand.ExecuteReader();

                    while (rsp.Read())
                    {
                        if (!sucItems.ContainsKey((int)rsp["Cliente_Id"]))
                        {
                            sucItems.Add((int)rsp["Cliente_Id"], new List<ClientSuc>());
                        }

                        var sucursal = new ClientSuc
                        {
                            Client_ID = (int)rsp["Cliente_Id"],
                            SucName = rsp["Sucursal"].ToString(),
                            Seller_id = (int)(decimal)rsp["vendedor"],
                            PriceList = new List<string>()
                        };

                        sucursal.PriceList.Add(connection.Name + rsp["Lista_precio"].ToString());

                        sucItems[(int)rsp["Cliente_Id"]].Add(sucursal);
                    }

                    rsp.Close();
                }


                ///Paso 2: Busco las cabeceras de listas de precios y armo las listas
                sSql = "SELECT c.[Cliente_ID], c.[Nombre], c.[ID_Vendedor], lp.[Descripcion] as Lista_precio, ISNULL(ge.[Descripcion], '') as GrupoEconomico, c.cuit FROM [dbo].[Clientes] c " +
                        //"INNER JOIN Listas_Precio lp ON lp.Id_lista_precios = c.Lista_Precios " +
                        //19/05/2020 ITO:ECM-42 Envío de clientes sin lista de precios.
                        "LEFT JOIN Listas_Precio lp ON c.Lista_Precios = lp.Id_lista_precios " +
                        "LEFT JOIN Grupo_Economico ge ON ge.Id = c.IdGrupo_Economico ";

                if (id > 0) sSql += String.Format(CultureInfo.CurrentCulture, "WHERE Cliente_Id={0}", id);

                using (var sqlCommand = new SqlCommand(sSql, oConexion))
                {
                    var rsp = sqlCommand.ExecuteReader();

                    while (rsp.Read())
                    {
                        Client client;

                        //Pregunto si el cliente ya existe.
                        if (!result.Exists(c => c.Client_ID == (int)rsp["Cliente_ID"]))
                        {
                            string cuit;
                            if (rsp["CUIT"] == System.DBNull.Value)
                            {
                                cuit = null;
                            }
                            else
                            {
                                cuit = (string)rsp["CUIT"];
                            }

                            List<ClientSuc> sucursales;
                            int idCliente = (int)rsp["Cliente_ID"];
                            String nombre = rsp["Nombre"].ToString();

                            int idVendedor =0;
                            if (rsp["ID_Vendedor"] != DBNull.Value)
                                idVendedor = (int)rsp["ID_Vendedor"];

                            String grupo = rsp["GrupoEconomico"].ToString();

                            if (sucItems.ContainsKey((int)rsp["Cliente_ID"]))
                                sucursales = sucItems[(int)rsp["Cliente_ID"]];
                            else
                                sucursales = new List<ClientSuc>();

                            client = new Client(idCliente ,
                                                cuit,
                                                nombre,
                                                new List<string>(),
                                                idVendedor,                                                
                                                grupo,                                                
                                                sucursales);

                            result.Add(client);
                        }
                        else //El cliente ya existe. Sólo tengo que cargar las listas de precios
                        {
                            client = result.First(c => c.Client_ID == (int)rsp["Cliente_ID"]);

                            if (sucItems.ContainsKey((int)rsp["Cliente_ID"]))
                            {
                                foreach (var suc in client.Sucs)
                                {
                                    var sucAux = sucItems[(int)rsp["Cliente_ID"]].Find(s => s.SucName == suc.SucName);
                                    if (sucAux != null) suc.PriceList.AddRange(sucAux.PriceList);
                                }
                            }
                        }
                        //19/05/2020 ITO:ECM-42 Envío de clientes sin lista de precios.
                        if (rsp["Lista_precio"].ToString().Length > 0)
                        {
                            client.PriceList.Add(connection.Name + rsp["Lista_precio"].ToString());
                        }
                    }

                    rsp.Close();
                }

                CloseDBConnection();
            }
            }
            catch (Exception ex)
            {
               mensaje = "Error al buscar clientes. " + ex.Message;                
            }
        }

        
        public Boolean  GetById(int id,out String mensaje, out Client cliente)
        {
            mensaje = "";
            cliente = null;
            var result = new List<Client>();
            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                GetClientData(connection, id, ref result, out mensaje);
            }
            if (mensaje.Equals(""))
            {
                cliente  = result.FirstOrDefault();
                return true;
            }
            else
                return false;                       
        }

        public Boolean GetByCuit(string cuit, out String  mensaje, out Client cliente)
        {
            Client result = null;
            mensaje = "";
            cliente = null;
            var lista = new List<Client>();
            var connections = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings connection in connections)
            {
                if (OpenDBConnection(connection.ConnectionString))
                {
                    // Busco cliente por cuit
                    var sSql = "SELECT * FROM Clientes cs ";

                    if (!cuit.Equals("")) sSql += String.Format(CultureInfo.CurrentCulture, "WHERE CUIT='{0}'", cuit);

                    using (var sqlCommand = new SqlCommand(sSql, oConexion))
                    {
                        var rsp = sqlCommand.ExecuteReader();
                        if (rsp.HasRows)
                        {
                            result = new Client();
                            rsp.Read();
                            this.GetClientData(connection, (int)rsp["CLIENTE_ID"], ref lista, out mensaje);
                            /*result.Client_ID = (int)rsp["Cliente_Id"];
                            result.CUIT = (string)rsp["CUIT"];
                            result.Name = (string)rsp["Nombre"];*/
                            //result = lista.FirstOrDefault();
                            
                        }
                        rsp.Close();
                    }
                    CloseDBConnection();
                }
            }                       
            if (mensaje.Equals(""))
            {
                cliente = lista.FirstOrDefault(); 
                return true;
            }
            else
                return false;
        }
                

        /**Cliente WEB***/
        public bool Save(ClienteWeb element, out String mensaje, String listaPrecios )
        {
            Boolean result = false;
            mensaje = "";
            var connections = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connection in connections)
            {
                if (OpenDBConnection(connection.ConnectionString))
                {
                    if (validarCliente(element, out mensaje))
                        if (saveCliente(element, out mensaje, listaPrecios))
                            result = true;
                }
                CloseDBConnection();
            }                       
            return result;
            
            /*ID	IVA_Descripcion	PorcIVA	PorcIVANI	LetraFactura	Id_Tabla
CF	Consumidor Final	21	0	B	1
EP	Exportacion	0	0	E	2
EX	Exento	0	0	B	3
MO	Monotributo	21	0	A	4
RI	Responsable Inscripto	21	0	A	5
IN	Inscripto 10,5%	10,5	0	A	6
FE	Exento A	0	NULL	A	7
EF	Exento B	0	NULL	B	8*/
                             
        }


        private bool validarCliente(ClienteWeb cliente, out String mensaje)
        {
            Boolean validarCliente = true ;
            mensaje = null;


            if (cliente.nombre.Equals("") || cliente.domicilioFacturacion.calle.Equals("") || cliente.domicilioFacturacion.codigoPostal.Equals("") || cliente.domicilioFacturacion.ciudad.Equals("")){
                mensaje += " / Complete todos los campos obligatorios: Razon Social,Domicilio,Codigo Postal, y CUIT";
                validarCliente = false;
            }

            if (cliente.documentoTipo.Equals("") || cliente.documentoNro.Equals("")){
                mensaje += " / Debe completar Tipo de documento y Numero";
                validarCliente = false;
            }

            return validarCliente;
        }
        private bool saveCliente(ClienteWeb cliente, out String mensaje, String listaPrecios)
        {
            mensaje = "";
            SqlDataReader dr;
            int idListaPrecio;
            try
            {
                /*tomo id lista de precios*/
                SqlCommand sqlCommand = new SqlCommand("SELECT Id_lista_precios FROM Listas_Precio WHERE descripcion = '" +  listaPrecios + "'", oConexion);
                sqlCommand.CommandType = System.Data.CommandType.Text;
                dr =sqlCommand.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    idListaPrecio = (int)dr["Id_lista_precios"];
                }
                else
                {
                    dr.Close(); 
                    mensaje = "No se encontró la lista de precios";
                    return false;
                }
                dr.Close();

                
                /*armo command para guardar cliente*/
                sqlCommand = new SqlCommand("spClientesWeb_Insert", oConexion);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                String dcilio = "";
                String dcilioEntrega = "";

                SqlParameter parametro = crearParametro("nombre", System.Data.DbType.String, cliente.nombre + " " + cliente.apellido);
                sqlCommand.Parameters.Add(parametro);

                if (cliente.documentoTipo.ToUpper().Equals("CUIT"))
                {
                    parametro = crearParametro("cuit", System.Data.DbType.String, cliente.documentoNro);
                    sqlCommand.Parameters.Add(parametro);
                    parametro = crearParametro("tipoDocumentoAfip", System.Data.DbType.String, ((int)enumTipoDocumento.CUIT) );
                    sqlCommand.Parameters.Add(parametro);
                    parametro = crearParametro("numeroDocumento", System.Data.DbType.String, cliente.documentoNro.Substring(2, cliente.documentoNro.Length - 2));
                    sqlCommand.Parameters.Add(parametro);
                }
                else if (cliente.documentoTipo.ToUpper().Equals("DNI"))
                {
                    /*DESA-2235 Pilar*/
                    //da error porque falta el cuit--> lo toma como el cliente web                    
                    /*------------------------------------------DESA-2235 Pilar*/
                    parametro = crearParametro("tipoDocumentoAfip", System.Data.DbType.String, ((int)enumTipoDocumento.DNI) );
                    sqlCommand.Parameters.Add(parametro);
                    parametro = crearParametro("numeroDocumento", System.Data.DbType.String, cliente.documentoNro);
                    sqlCommand.Parameters.Add(parametro);
                }

                parametro = crearParametro("consumidorFinal", System.Data.DbType.Boolean, cliente.tipoFiscal.CompareTo(((int)enumTipoFiscalBalcony.CF)) == 0);
                sqlCommand.Parameters.Add(parametro);

                if (cliente.tipoFiscal.CompareTo(((int)enumTipoFiscalBalcony.CF) ) == 0)
                    parametro = crearParametro("condicionIva", System.Data.DbType.String, "CF");
                else if (cliente.tipoFiscal.CompareTo(enumTipoFiscalBalcony.RI) == 0)
                    parametro = crearParametro("condicionIva", System.Data.DbType.String, "RI");
                else if (cliente.tipoFiscal.CompareTo(enumTipoFiscalBalcony.MO) == 0)
                    parametro = crearParametro("condicionIva", System.Data.DbType.String, "MO");
                else if (cliente.tipoFiscal.CompareTo(enumTipoFiscalBalcony.EX) == 0)
                    parametro = crearParametro("condicionIva", System.Data.DbType.String, "EX");
                else
                    parametro = crearParametro("condicionIva", System.Data.DbType.String, "CF");
                sqlCommand.Parameters.Add(parametro);

                if (cliente.domicilioFacturacion != null)
                {
                    dcilio = "Pcia: " + cliente.domicilioFacturacion.provinciaNombre + " / ciudad:" + cliente.domicilioFacturacion.ciudad + " / calle: " + cliente.domicilioFacturacion.calle + " nro: " + cliente.domicilioFacturacion.numero
                            + " / observaciones: " + cliente.domicilioFacturacion.comentarios;
                    parametro = crearParametro("domicilio", System.Data.DbType.String, " COMPLETAR");
                    sqlCommand.Parameters.Add(parametro);
                    parametro = crearParametro("codigoPostal", System.Data.DbType.String, cliente.domicilioFacturacion.codigoPostal);
                    sqlCommand.Parameters.Add(parametro);
                    parametro = crearParametro("telefono", System.Data.DbType.String, cliente.domicilioFacturacion.telefono);
                    sqlCommand.Parameters.Add(parametro);
                }

                if (cliente.domicilioEnvio != null)
                {
                    dcilioEntrega = "Pcia: " + cliente.domicilioEnvio.provinciaNombre + " / ciudad:" + cliente.domicilioEnvio.ciudad + " /  cp:" + cliente.domicilioEnvio.codigoPostal + " / calle: " + cliente.domicilioEnvio.calle + " nro: " + cliente.domicilioEnvio.numero
                           + " / observaciones: " + cliente.domicilioEnvio.comentarios;
                    parametro = crearParametro("domicilioEntrega", System.Data.DbType.String, " COMPLETAR");
                    sqlCommand.Parameters.Add(parametro);                    
                    parametro = crearParametro("telefonoEntrega", System.Data.DbType.String, cliente.domicilioEnvio.telefono);
                    sqlCommand.Parameters.Add(parametro);
                }                
                parametro = crearParametro("observaciones", System.Data.DbType.String, "Cliente de Balcony - alias: " + cliente.nickName +  " - domicilio cliente: " + dcilio + " - domicilio entrega: " + dcilioEntrega);
                sqlCommand.Parameters.Add(parametro);

                parametro = crearParametro("listaPrecios", System.Data.DbType.Int32 , idListaPrecio);
                sqlCommand.Parameters.Add(parametro);

                parametro = crearParametro("@ReturnValue", System.Data.DbType.Int32 , 0,System.Data.ParameterDirection.ReturnValue);
                sqlCommand.Parameters.Add(parametro);

                sqlCommand.ExecuteNonQuery();  
                int resultado = (int)sqlCommand.Parameters["@ReturnValue"].Value;
                if (resultado > 0){
                    mensaje = resultado.ToString() ;
                    //    GuardaNovedadesAltasDemonio
                    return true;
                }
                else{
                    mensaje = "Error al insertar cliente.";
                    return false;
                }
            }
            catch(Exception e)
            {
                mensaje = e.Message;
                return false;
            }

        }

        /*NO implementados*/
        public IEnumerable<Client> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Client GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Client element)
        {
            throw new NotImplementedException();
        }
        public bool Save(Client element)
        {
            throw new NotImplementedException();
        }
               
        
        IEnumerable<Client> ICRUDDAL<Client>.Get()
        {
            throw new NotImplementedException();
        }

        bool ICRUDDAL<Client>.Save(Client element, out string mensaje)
        {
            throw new NotImplementedException();
        }

        Client ICRUDDAL<Client>.GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
