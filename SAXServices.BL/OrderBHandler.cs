using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.BL
{
    public class OrderBHandler : IHandlerBase<OrderB>
    {
        ICRUDDAL<OrderB> _orderDAL;
        ICRUDDALClient<Client> _clientDAL;
        ICRUDDALClientWeb <ClienteWeb > _clientWebDAL;
        ICRUDDAL<Seller> _sellerDAL;
        ICRUDDAL<Product> _productDAL;
        ICRUDDAL<PriceList> _priceListtDAL;

        /*DESA-2235 Pilar*/
        private const string productoDescuento = "##DESC";
        private const string productoCostoEnvio = "##COSTOENVIO";        
        /*--------------------------------------------------------DESA-2235 Pilar*/

        public OrderBHandler():this(new OrderBDAL(), 
                                    new ClientDAL(),
                                    new ClientDAL(),
                                    new SellerDAL(), 
                                    new ProductDAL(),
                                    new PriceListDAL()){ }

        public OrderBHandler(ICRUDDAL<OrderB> orderDAL, 
                            ICRUDDALClient<Client> clientDAL,
                            ICRUDDALClientWeb<ClienteWeb> clientWebDAL,
                            ICRUDDAL<Seller> sellerDAL, 
                            ICRUDDAL<Product> productDAL,
                            ICRUDDAL<PriceList> priceListDAL)
        {
            this._orderDAL = orderDAL;
            this._clientDAL = clientDAL;
            this._clientWebDAL = clientWebDAL;
            this._sellerDAL = sellerDAL;
            this._productDAL = productDAL;
            this._priceListtDAL = priceListDAL;
        }

        public IEnumerable<OrderB> GetAll()
        {
            return this._orderDAL.Get();
        }

        public IEnumerable<OrderB> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public OrderB GetByID(int Id)
        {
            return this._orderDAL.GetById(Id);
        }

        public OrderB GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(OrderB order, out string mensaje)
        {
            bool result = false;

            result = ValidateOrder(order, out mensaje);

            if (result)
            {
                result = this._orderDAL.Save(order,out mensaje);

                if (result)
                {                    
                    mensaje += String.Format(CultureInfo.CurrentCulture, " La orden {0} se guardó en forma exitosa", order.NroOrdenCompra);
                }
                else
                    mensaje += String.Format(CultureInfo.CurrentCulture, " No se pudo generar la orden {0}", order.NroOrdenCompra);
            }            

            return result;
        }

        /// <summary>
        /// Ejecuta todas las validaciones de la Orden de Pedido antes de guardarla
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool ValidateOrder(OrderB order, out string mensaje)
        {
            try {
                IEnumerable<PriceList> prices = null;
                
                ///Validación 0: Orden mal formada. Si esta no se cumple, no se sigue verificando nada.
                if (order == null || order.Detail == null || order.Detail.Count <= 0)
                {
                    mensaje = "La orden es null o el detalle está vacío. Verifique.";
                    return false;
                }

                var sb = new StringBuilder();
                var resultado = true;
                                
                order.Fecha_Vto = order.Fecha_Emision;
                
                ///Validación 1: Cliente inexistente            
                var client = this._clientDAL.GetByCuit(order.cliente.documentoNro); 
                /*DESA-2053 el cliente no existe*/
                if (client == null){                    
                    resultado = this._clientWebDAL.Save(order.cliente, out mensaje, order.Detail[0].PriceList_Name);
                    if (resultado)
                    {
                        /*Se pudo crear el nuevo cliente*/
                        order.Client_ID = Int32.Parse(mensaje);
                        sb.AppendLine("CUIT de cliente inexistente. Se creo el cliente: " + order.cliente.apellido + " " + order.cliente.nombre + " - CUIT:" + order.cliente.documentoNro);
                    }
                    /*DESA-2235 PILAR*/
                    else
                    {
                        /*Dió error la creación --> tomo cliente genérico web*/
                        client = this._clientDAL.GetById(Int32.Parse(ConfigurationManager.AppSettings["clienteWeb"]));
                        order.Client_ID = client.Client_ID;
                        resultado = (client != null);
                        mensaje = "Datos del cliente inválidos. Se tomó el cliente web: " + client.Name;
                    }
                    /*-------------------------------------------------------DESA-2235 PILAR*/
                    sb.AppendLine(mensaje);
                }
                else
                    order.Client_ID = client.Client_ID;

                if (resultado)
                {
                    var clients = this._clientDAL.Get();
                    ///Validación 2: Suc cliente inválida                
                    client = clients.First(c => c.Client_ID == order.Client_ID);

                    if (client != null && !String.IsNullOrEmpty(order.SucName) && client.Sucs.Where(s => s.SucName == order.SucName).Count() <= 0)
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "La Sucursal {0} de Cliente no es válida para el cliente {1}. Verifique.", order.SucName, order.Client_ID));
                        resultado = false;
                    }

                    ///Validación 2.1: Lista de Precios Existente
                    List<string> priceListNames = null;
                    if (client != null && !String.IsNullOrEmpty(order.SucName) && client.Sucs.Where(s => s.SucName == order.SucName).Count() > 0)
                    {
                        var suc = client.Sucs.Find(s => s.SucName == order.SucName);
                        priceListNames = suc.PriceList;
                    }
                    else
                    {
                        if (client != null)
                        {
                            priceListNames = client.PriceList;
                        }
                    }


                    prices = this._priceListtDAL.Get().Where(p => priceListNames.Contains(p.Name));
                }

                if (resultado) {
                    //Validación 6: Valor de acción del usuario (Anula o Agrega)
                    //19/05/2020 ITO:ECM-40 Modificación en el envío de las observaciones de los pedidos.
                    //if (order.UserAction.Trim().Substring(0,1) != "A" && order.UserAction.Trim().Substring(0,1)  != "G")
                    //26/09/2022 Posibilidad de no enviar observacion --> "V"
                    if (order.UserAction.Trim().Substring(0, 1) != "A" && order.UserAction.Trim().Substring(0, 1) != "G" && order.UserAction.Trim().Substring(0, 1) != "V")
                    {
                        sb.AppendLine("Acción inválida del usuario. No se indica si Anula o Agrega al pedido");
                        resultado = false;
                    }
                }
                //DESA-2235 PILAR '
                var products = this._productDAL.Get();
                //--------------------------------------DESA-2235 PILAR '

                ///Validaciones del cuerpo de la orden
                foreach (var detail in order.Detail)
                {
                        
                        //Validación 1.1: Cantidad > 0
                    if (detail.Quantity <= 0) {
                            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "El producto {0} no puede indicarse con cantidad menor o igual a 0.", detail.Product_Id));
                            resultado = false;                            
                    }


                    /*DESA-2235 Pilar si es descuento o costo envío le asigno el idProducto correspondiente*/
                    if (detail.Product_Id.ToUpper().Contains(productoDescuento))
                    {
                        detail.Product_Id = ConfigurationManager.AppSettings["descuento"];
                        detail.ProductVariation_Id = ConfigurationManager.AppSettings["descuento"];
                    }
                    else if (detail.Product_Id.ToUpper().Contains(productoCostoEnvio))
                    {
                        detail.Product_Id = ConfigurationManager.AppSettings["costoEnvio"];
                        detail.ProductVariation_Id = ConfigurationManager.AppSettings["costoEnvio"];
                    }
                    /*--------------------DESA-2235 Pilar*/


                    ///Validación 2: Producto inexistente
                    //DESA-2235 PILAR '
                    //                    var products = this._productDAL.Get();
                    //------------------------------------DESA-2235 PILAR '                    
                    if (products.Where(p => p.Product_Id.ToUpper().Equals(detail.Product_Id.ToUpper())).Count() <= 0)
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "Id de Producto {0} Inválido o inexistente. Verifique.", detail.Product_Id));
                       resultado = false;
                       break; 
                    }

                    ///Validación 3: Datos del producto
                    var product = products.First(p => p.Product_Id.ToUpper().Equals(detail.Product_Id.ToUpper()));      

                    detail.Product_Name = product.Name;
                    detail.Product_Description = product.Description;
                        //09/12/2019 ITO : DESA-952 Separar los pedidos por marca
                    detail.Category = product.Category;

                        ///Validación 4: Variante válida.
                        ///Puede ser que la variante sea el producto en si, en ese caso se omite la validación.
                    ProductVariation variation = null;
                    if (product != null && product.Product_Id.ToUpper() != detail.ProductVariation_Id.ToUpper())
                    {
                        if (product.Variations != null && product.Variations.Count > 0)
                            variation = ObtainVariation(detail.ProductVariation_Id, product.Variations);

                        if (variation == null)
                        {
                            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "La variante {0} no es válida para el producto {1}. Verifique.", detail.ProductVariation_Id, detail.Product_Id));
                            resultado = false;
                        }
                    }   

                    if (product != null && variation != null)
                    { 
                        detail.AttributeValue = variation.AttributeValue;

                        ProductVariation aux = null;
                        if (product.Variations != null && product.Variations.Count > 0)
                        {
                            aux = ObtainVariation(variation.ParentProduct_Id, product.Variations);
                        }

                        if (aux != null)
                        {
                            detail.Product_Name = variation.ParentProduct_Id;
                            detail.Product_Description += aux.Description;
                        }
                    }

                    if (resultado)
                    {
                        PriceList priceList;

                        /*DESA-2235 Pilar si es descuento o costo envío le asigno el idProducto correspondiente*/
                        if (!detail.Product_Id.ToUpper().Equals(ConfigurationManager.AppSettings["descuento"].ToUpper()) && !detail.Product_Id.ToUpper().Equals(ConfigurationManager.AppSettings["costoEnvio"].ToUpper()))
                        //---------------------------------------------------DESA-2235 Pilar'
                        {
                            priceList = prices.FirstOrDefault(l => l.Items.Exists(p => p.Product_Id.ToUpper().Equals(detail.Product_Id.ToUpper()) && p.ProductVariation_Id.ToUpper().Equals(detail.ProductVariation_Id.ToUpper())));
                            if (priceList == null)
                            {
                                sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "No existe el producto {0} variante {1} en las listas de precios del cliente {2}. Verifique.", detail.Product_Id, detail.ProductVariation_Id, client.Name));
                                resultado = false;
                            }
                            else
                            {
                                detail.PriceList_Name = priceList.Name;
                                detail.Price = priceList.Items.Find(p => p.Product_Id.ToUpper().Equals(detail.Product_Id.ToUpper()) && p.ProductVariation_Id.ToUpper().Equals(detail.ProductVariation_Id.ToUpper())).Price;
                            }
                        }
                    }
                }
                
                mensaje = sb.ToString();
                return resultado;
            }
            catch(Exception ex) {
                mensaje = ex.Message;
                return false;
            }
        }

        private ProductVariation ObtainVariation(string productVariation_Id, List<ProductVariation> variations)
        {
            ProductVariation result = null;

            foreach (var variation in variations)
            {
                if (variation.ProductVariation_Id.ToUpper() == productVariation_Id.ToUpper()) result = variation;

                if (result != null) break;

                if (variation.Variations != null && variation.Variations.Count > 0)
                {
                    result = ObtainVariation(productVariation_Id, variation.Variations);
                }
            }

            return result;
        }
    }
}
