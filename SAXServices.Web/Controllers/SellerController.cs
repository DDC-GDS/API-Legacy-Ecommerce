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
    public class SellerController : ApiController
    {
        IHandlerBase<Seller> _sellerHandler;

        public SellerController() : this(new SellerHandler()) { }

        public SellerController(IHandlerBase<Seller> handler)
        {
            this._sellerHandler = handler;
        }

        // GET: api/Seller
        public JsonResult<IEnumerable<Seller>> Get()
        {
            var sellers = this._sellerHandler.GetAll();
            return Json(sellers);
        }

        // GET: api/Seller/5
        public JsonResult<Seller> Get(int id)
        {
            var ent = this._sellerHandler.GetByID(id);
            return Json(ent);
        }

        // POST: api/Seller
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Seller/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Seller/5
        public void Delete(int id)
        {
        }
    }
}
