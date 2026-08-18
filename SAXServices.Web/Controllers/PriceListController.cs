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
    public class PriceListController : ApiController
    {
        IHandlerBase<PriceList> _priceListHandler;

        public PriceListController() : this(new PriceListHandler()) { }

        public PriceListController(IHandlerBase<PriceList> handler)
        {
            this._priceListHandler = handler;
        }

        // GET: api/PriceList
        public JsonResult<IEnumerable<PriceList>> Get()
        {
            var prices = this._priceListHandler.GetAll();
            return Json(prices);
        }

        // GET: api/PriceList/BKMayorista
        public JsonResult<PriceList> Get(string id)
        {
            var price = this._priceListHandler.GetByName(id);
            return Json(price);
        }

        // GET: api/PriceList/BKMayorista
        public JsonResult<IEnumerable<PriceList>> Get(DateTime fecha)
        {
            var price = this._priceListHandler.GetByDate(fecha);
            return Json(price);
        }

        // POST: api/PriceList
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/PriceList/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/PriceList/5
        public void Delete(int id)
        {
        }
    }
}
