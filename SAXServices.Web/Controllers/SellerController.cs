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
    public class SellerController : ApiController
    {
        IHandlerBaseSeller<Seller> _sellerHandler;

        public SellerController() : this(new SellerHandler()) { }

        public SellerController(IHandlerBaseSeller<Seller> handler)
        {
            this._sellerHandler = handler;
        }

        // GET: api/Seller
        public JsonResult<List<Seller>> Get()
        {          
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;
            List<Seller> vendedores = new List<Seller>();
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Seller.Get");

                bOk = this._sellerHandler.GetAll(out mensaje, out vendedores);
                if (bOk)
                {
                    response.Result = "OK";
                    if (vendedores.Count == 0)
                    {
                        mensaje += "No se encontraron vendedores";
                        Seller vendedor = new Seller { Name = mensaje,
                                                       LastName ="",
                                                       Seller_Id = -1};                        
                        vendedores.Add (vendedor); 
                    }

                    response.Message = mensaje;
                    response.datos = vendedores;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;

                    Seller vendedor = new Seller
                    {
                        Name = response.Result + " / " + response.Message   ,
                        LastName = "",
                        Seller_Id = -1
                    };
                    vendedores.Add(vendedor);
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                Seller vendedor = new Seller
                {
                    Name = response.Result + " / " + response.Message,
                    LastName = "",
                    Seller_Id = -1
                };
                vendedores.Add(vendedor);
            }
            
            logeo.FinServicio("Seller.Get" + response.Result + " / " + response.Message);

            return Json(vendedores );
        }

        // GET: api/Seller/5
        public JsonResult<Seller> Get(int id)
        {            
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;
            Seller  vendedor= new Seller();
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("Seller.Get: " + id);

                bOk = this._sellerHandler.GetById(id, out mensaje, out vendedor);
                if (bOk)
                {
                    response.Result = "OK";
                    if (vendedor != null)
                    {
                        if (vendedor.Name.Equals(""))
                        {
                            mensaje += "No se encontró el vendedor: " + id;
                            vendedor = new Seller
                            {
                                Name = mensaje ,
                                LastName = "",
                                Seller_Id = -1
                            };
                        }
                    }
                    else
                    {
                        mensaje += "No se encontró el vendedor: " + id;
                        vendedor = new Seller
                        {
                            Name = mensaje ,
                            LastName = "",
                            Seller_Id = -1
                        };
                    }
                
                    response.Message = mensaje;
                    response.datos = vendedor;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    vendedor = new Seller
                    {
                        Name = response.Result + " / " + response.Message,
                        LastName = "",
                        Seller_Id = -1
                    };
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                vendedor = new Seller
                {
                    Name = response.Result + " / " + response.Message,
                    LastName = "",
                    Seller_Id = -1
                };
            }
                        
            logeo.FinServicio("Seller.Get: " + response.Result + " / " + response.Message);
            return Json(vendedor);
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
