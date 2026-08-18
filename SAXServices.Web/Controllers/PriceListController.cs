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
    public class PriceListController : ApiController
    {
        IHandlerBasePriceList<PriceList> _priceListHandler;

        public PriceListController() : this(new PriceListHandler()) { }

        public PriceListController(IHandlerBasePriceList<PriceList> handler)
        {
            this._priceListHandler = handler;
        }

        // GET: api/PriceList
        public JsonResult<IEnumerable<PriceList>> Get()
        {
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;            
            List<PriceList> listaError = new List<PriceList>();
            IEnumerable<PriceList> listas;
            Log logeo = new Log();
            
            try
            {
                logeo.InicioServicio("PriceList.Get");

                bOk = this._priceListHandler.GetAll(out mensaje, out listas);                
                if (bOk)
                {
                    response.Result = "OK";
                    response.Message = mensaje;
                    response.datos = listas;
                   
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    
                    PriceList error = new PriceList  { 
                                            Name  = response.Result + " / " + response.Message};
                    listaError .Add(error);
                    listas = listaError; 
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                PriceList error = new PriceList  {
                                            Name = response.Result + " / " + response.Message};
                listaError.Add(error);
                listas = listaError;
            }
            
            logeo.FinServicio("PriceList.Get: " + response.Result + " / " + response.Message);

            return Json(listas);
            
        }

        // GET: api/PriceList/BKMayorista
        public JsonResult<PriceList> Get(string id)
        {
            var response = new ResponseDC();
            string mensaje;
            bool bOk = true;
            PriceList priceList = new PriceList();
            Log logeo = new Log();
            
            try
            {
                logeo.InicioServicio("PriceList.Get: " + id);

                bOk = this._priceListHandler.GetByName(id,out mensaje,out priceList);
                if (bOk)
                {
                    response.Result = "OK";
                    if (priceList.Name == null) {
                        mensaje += "No se encontraron datos para la lista: " + id;
                        priceList.Name = mensaje; 
                    }


                    response.Message = mensaje;
                    response.datos = priceList;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    priceList  = new PriceList{
                                                Name = response.Result + " / " + response.Message};
                    
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                priceList  = new PriceList{
                                            Name = response.Result + " / " + response.Message};                
            }

            logeo.FinServicio("PriceList.Get: " + response.Result + " / " + response.Message);

            return Json(priceList);            
        }

        // GET: api/PriceList/fecha
        public JsonResult<IEnumerable<PriceList>> Get(DateTime fecha)
        {
            var response = new ResponseDC();
            String mensaje;
            bool bOk = true;
            List<PriceList> listaError = new List<PriceList>();
            IEnumerable<PriceList> listas;
            Log logeo = new Log();

            try
            {
                logeo.InicioServicio("PriceList.Get(date): " + fecha);
                             
                bOk  = this._priceListHandler.GetByDate(fecha, out mensaje, out listas);
                if (bOk)
                {
                    response.Result = "OK";
                    response.Message = mensaje;
                    response.datos = listas;
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = mensaje;
                    response.datos = null;
                    PriceList error = new PriceList{
                                            Name = response.Result + " / " + response.Message};
                    listaError.Add(error);
                    listas = listaError; 
                    
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Result = "ERROR";
                PriceList error = new PriceList{
                                        Name = response.Result + " / " + response.Message};
                listaError.Add(error);
                listas = listaError;
            }

            logeo.FinServicio("PriceList.Get(date): " + response.Result + " / " + response.Message);

            return Json(listas);
            
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
