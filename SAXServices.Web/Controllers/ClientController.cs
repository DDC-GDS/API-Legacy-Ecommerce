using Newtonsoft.Json;
using SAXServices.BL;
using SAXServices.Contracts;
using SAXServices.Web.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Services.Description;

namespace SAXServices.Web.Controllers
{
    public class ClientController : ApiController
    {
        IHandlerBaseClient<Client> _clientHandler;

        public ClientController() : this(new ClientHandler()) { }

        public ClientController(IHandlerBaseClient <Client> handler)
        {
            this._clientHandler = handler;
        }

        // GET: api/Client
        public JsonResult<List<Client>> Get()
        {
            
            var response = new ResponseDC();
            List<Client> clientes = new List<Client>();            
            string mensaje = "";
            bool bOk;
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Client.Get");

                bOk = this._clientHandler.GetAll(out mensaje, out clientes);
                if (bOk)
                {
                    response.Result = "OK";
                    if (clientes.Count == 0)
                    {
                        mensaje += " No se encontraron clientes";
                        Client cliente = new Client { Name = mensaje };
                        clientes.Add (cliente); 
                    }
                        

                    response.Message = mensaje;
                    response.datos = clientes;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    Client cliente = new Client { Name = response.Result + " / " +response.Message  };
                    clientes.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                Client cliente = new Client { Name = response.Result + " / " + response.Message};
                clientes.Add(cliente);
            }
            logeo.FinServicio("Client.Get" + response.Result + " / " + response.Message);

            return Json(clientes);
        }

        // GET: api/Client/5
        public JsonResult<Client> Get(int id)
        {
            var response = new ResponseDC();
            String mensaje = "";
            Client cliente = null;
            bool bOk;
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Client.Get: " + id);

                bOk = this._clientHandler.GetByID(id,out mensaje, out cliente);
                if (bOk)
                {
                    response.Result = "OK";
                    if (cliente==null)
                    {
                        mensaje += "No se encontró el cliente";
                        cliente = new Client { Name = mensaje  };
                    }

                    response.Message = mensaje;
                    response.datos = cliente;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    cliente = new Client { Name = response.Result + " / " + response.Message };
                    
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                cliente = new Client { Name = response.Result + " / " + response.Message };
            }
            
            logeo.FinServicio("Client.Get: " + response.Result + " / " + response.Message);

            return Json(cliente);           
        }

        // POST: api/Client
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Client/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Client/5
        public void Delete(int id)
        {
        }
    }
}
