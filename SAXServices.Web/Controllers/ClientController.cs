using SAXServices.BL;
using SAXServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

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
        public JsonResult<IEnumerable<Client>> Get()
        {
            var clients = this._clientHandler.GetAll();
            return Json(clients);
        }

        // GET: api/Client/5
        public JsonResult<Client> Get(int id)
        {
            var client = this._clientHandler.GetByID(id);
            return Json(client);
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
