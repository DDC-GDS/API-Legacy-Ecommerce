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

namespace SAXServices.Web.Controllers
{
    public class OrderController : ApiController
    {
        IHandlerBase<Order> _orderHandler;

        public OrderController() : this(new OrderHandler()) { }

        public OrderController(IHandlerBase<Order> handler)
        {
            this._orderHandler = handler;
        }

        // GET: api/Order
        public JsonResult<IEnumerable<Order>> Get()
        {
            var orders = this._orderHandler.GetAll();
            return Json(orders);
        }

        // GET: api/Order/5
        public JsonResult<Order> Get(int id)
        {
            var orders = this._orderHandler.GetByID(id);
            return Json(orders);
        }

        //POST: api/Order
        public JsonResult<ResponseDC> Post([FromBody]Order order)
        {
            var response = new ResponseDC();
            string message;
            bool bOk = true;
            try
            {
                bOk = this._orderHandler.Save(order, out message);
                if (bOk)
                {
                    response.Result = "OK";
                    response.Message = message;
                    response.datos = "order_id: " + message;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = message;
                    response.datos = null;
                }

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
            }

            return Json(response);
        }

        // PUT: api/Order/5
        public JsonResult<ResponseDC> Put(int id, [FromBody]Order value)
        {
            return Json(new ResponseDC());
        }

        // DELETE: api/Order/5
        public void Delete(int id)
        {
        }
    }
}
