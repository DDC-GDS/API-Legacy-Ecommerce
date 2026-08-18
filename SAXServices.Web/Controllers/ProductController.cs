using SAXServices.BL;
using SAXServices.Contracts;
using SAXServices.Web.Contracts;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;

namespace SAXServices.Web.Controllers
{
    public class ProductController : ApiController
    {
        IHandlerBaseProduct<Product> _productHandler;

        public ProductController() : this(new ProductHandler()) { }

        public ProductController(IHandlerBaseProduct<Product> handler)
        {
            this._productHandler = handler;
        }          

        // GET: api/Product
        public JsonResult<List<Product>> Get()
        {
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;
            List<Product> productos = new List<Product>();
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Product.Get");

                bOk = this._productHandler.GetAll(out mensaje, out productos );
                if (bOk)
                {
                    response.Result = "OK";
                    if (productos.Count == 0)
                    {
                        mensaje += ". No se encontraron productos";
                        Product producto = new Product { Name = mensaje };
                        productos.Add ( producto );
                    }
                        

                    response.Message = mensaje;
                    response.datos = productos;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    Product producto = new Product { Name = response.Result + " / " + response.Message    };
                    productos.Add(producto);
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                Product producto = new Product { Name = response.Result + " / " + response.Message };
                productos.Add(producto);
            }

            logeo.FinServicio("Product.Get" + response.Result + " / " + response.Message);

            return Json(productos);
        }

        // GET: api/Product/5
        public JsonResult<Product> Get(string id)
        {
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;
            Product producto = new Product();
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Product.Get: " + id);

                bOk = this._productHandler.GetByName(id, out mensaje, out producto);
                if (bOk)
                {
                    response.Result = "OK";
                    if (producto != null)
                    {
                        if (producto.Name == null)
                        {
                            mensaje += "No se encontró el producto: " + id;
                            producto.Name = mensaje;
                        }
                    }
                    else
                    {
                        mensaje += "No se encontró el producto: " + id;
                        producto.Name = mensaje;
                    }
                    
                    response.Message = mensaje;
                    response.datos = producto;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    producto = new Product { Name = response.Result + " / " + response.Message };
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                producto = new Product { Name = response.Result + " / " + response.Message };
            }

            logeo.FinServicio("Product.Get: " + response.Result + " / " + response.Message);
            
            return Json(producto);            
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
