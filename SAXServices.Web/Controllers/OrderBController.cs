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

namespace SAXServices.Web.Controllers
{
    public class OrderBController : ApiController
    {
        IHandlerBase<OrderB> _orderBHandler;

        public OrderBController() : this(new OrderBHandler()) { }

        public OrderBController(IHandlerBase<OrderB> handler)
        {
            this._orderBHandler = handler;
        }

        // GET: api/OrderB
        public JsonResult<IEnumerable<OrderB>> Get()
        {
            var orders = this._orderBHandler.GetAll();
            return Json(orders);
        }

        // GET: api/OrderB/5
        public JsonResult<OrderB> Get(int id)
        {
            var orders = this._orderBHandler.GetByID(id);
            return Json(orders);
        }

        //POST: api/OrderB
        public JsonResult<ResponseDC> Post([FromBody]OrderB order)
        {
            var response = new ResponseDC();
            string message;
            bool bOk = true;
            Log logeo = new Log();
            try
            {              
                logeo.InicioServicio("OrderB.Post: " + JsonConvert.SerializeObject(order));
                
                bOk= this._orderBHandler.Save(order, out message);
                if (bOk)
                {
                    response.Result = "OK";
                    response.Message = message;
                    response.datos = "order_id: " + order.Order_id;
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

            logeo.FinServicio("OrderB.Post: " + response.Result + " / " + response.Message   );

            return Json(response);
        }

        // PUT: api/Order/5
        public JsonResult<ResponseDC> Put(int id, [FromBody]OrderB value)
        {
            return Json(new ResponseDC());
        }

        // DELETE: api/Order/5
        public void Delete(int id)
        {
        }
    }
}
