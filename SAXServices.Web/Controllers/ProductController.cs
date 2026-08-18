using SAXServices.BL;
using SAXServices.Contracts;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;

namespace SAXServices.Web.Controllers
{
    public class ProductController : ApiController
    {
        IHandlerBase<Product> _productHandler;

        public ProductController() : this(new ProductHandler()) { }

        public ProductController(IHandlerBase<Product> handler)
        {
            this._productHandler = handler;
        }          

        // GET: api/Product
        public JsonResult<IEnumerable<Product>> Get()
        {
            var products = this._productHandler.GetAll();
            return Json(products);
        }

        // GET: api/Product/5
        public JsonResult<Product> Get(string id)
        {
            var ent = this._productHandler.GetByName(id);
            return Json(ent);
        }

        // POST: api/Product
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Product/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Product/5
        public void Delete(int id)
        {
        }
    }
}
