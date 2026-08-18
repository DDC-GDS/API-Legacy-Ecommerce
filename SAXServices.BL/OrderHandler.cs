using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.BL
{
    public class OrderHandler : IHandlerBase<Order>
    {
        ICRUDDAL<Order> _orderDAL;
        ICRUDDALClient<Client> _clientDAL;
        ICRUDDAL<Seller> _sellerDAL;
        ICRUDDAL<Product> _productDAL;
        ICRUDDALPriceList<PriceList> _priceListtDAL;

        public OrderHandler():this(
            new OrderDAL(), 
            new ClientDAL(), 
            new SellerDAL(), 
            new ProductDAL(),
            new PriceListDAL()){ }

        public OrderHandler(
            ICRUDDAL<Order> orderDAL, 
            ICRUDDALClient<Client> clientDAL, 
            ICRUDDAL<Seller> sellerDAL, 
            ICRUDDAL<Product> productDAL,
            ICRUDDALPriceList <PriceList> priceListDAL)
        {
            this._orderDAL = orderDAL;
            this._clientDAL = clientDAL;
            this._sellerDAL = sellerDAL;
            this._productDAL = productDAL;
            this._priceListtDAL = priceListDAL;
        }

        public IEnumerable<Order> GetAll()
        {
            return this._orderDAL.Get();
        }

        public IEnumerable<Order> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public Order GetByID(int Id)
        {
            return this._orderDAL.GetById(Id);
        }

        public Order GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool Save(Order order, out string message)
        {
            bool result = false;

            result = ValidateOrder(order, out message);

            if (result)
            {
                result = this._orderDAL.Save(order, out message);

                if (result) message = String.Format(CultureInfo.CurrentCulture, "La orden {0} se guardó en forma exitosa", order.Order_id);
                else message = String.Format(CultureInfo.CurrentCulture, "No se pudo generar la orden {0}", order.Order_id);
            }

            return result;
        }

        /// <summary>
        /// Ejecuta todas las validaciones de la Orden de Pedido antes de guardarla
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool ValidateOrder(Order order, out string message)
        {
            ///Validación 0: Orden mal formada. Si esta no se cumple, no se sigue verificando nada.
            if (order == null || order.Detail == null || order.Detail.Count <= 0)
            {
                message = "La orden es null o el detalle está vacío. Verifique.";
                return false;
            }

            var sb = new StringBuilder();
            var result = true;

            /////Validación 1: Fecha Emisión > FechaVencimiento
            //if (order.Fecha_Emision > order.Fecha_Vto)
            //{
            //    sb.AppendLine("La fecha de Emisión es mayor a la de vencimiento. Verifique.");
            //    result = false;
            //}

            order.Fecha_Vto = order.Fecha_Emision;

            ///Validación 2: Nro de Orden incorrecto o existente
            var orders = this._orderDAL.Get();
            if (order.Order_id <= 0 || orders.Where(o=> o.Order_id == order.Order_id).Count() > 0)
            {
                sb.AppendLine("Id de Orden Inválida o ya existente. Verifique.");
                result = false;
            }

            ///Validación 3: Cliente inexistente
            var clients = this._clientDAL.Get();
            if (clients.Where(c => c.Client_ID == order.Client_ID).Count() <= 0)
            {
                sb.AppendLine("Id de Cliente Inválido o inexistente. Verifique.");
                result = false;
            }
            else
            {
                ///Validación 4: Suc cliente inválida
                var client = clients.First(c => c.Client_ID == order.Client_ID);
                if (client != null && !String.IsNullOrEmpty(order.SucName) && client.Sucs.Where(s => s.SucName == order.SucName).Count() <= 0)
                {
                    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "La Sucursal {0} de Cliente no es válida para el cliente {1}. Verifique.", order.SucName, order.Client_ID));
                    result = false;
                }

                ///Validación 4.1: Lista de Precios Existente
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


                var prices = this._priceListtDAL.Get().Where(p => priceListNames.Contains(p.Name));

                //18/02/2020 ITO : ECM-19 Vendedor No Obligatorio.
                //Validación 5: Vendedor inexistente
                //           var sellers = this._sellerDAL.Get();
                //           if (sellers.Where(s => s.Seller_Id == order.Seller_Id).Count() <= 0)
                //           {
                //                sb.AppendLine("Id de Vendedor Inválido o inexistente. Verifique.");
                //                result = false;
                //            }

                //Validación 6: Valor de acción del usuario (Anula o Agrega)
                //19/05/2020 ITO:ECM-40 Modificación en el envío de las observaciones de los pedidos.
                //if (order.UserAction.Trim().Substring(0,1) != "A" && order.UserAction.Trim().Substring(0,1)  != "G")
                //26/09/2022 Posibilidad de no enviar observacion --> "V"
                if (order.UserAction.Trim().Substring(0, 1) != "A" && order.UserAction.Trim().Substring(0, 1) != "G" && order.UserAction.Trim().Substring(0, 1) != "V")

                {
                    sb.AppendLine("Acción inválida del usuario. No se indica si Anula o Agrega al pedido");
                    result = false;
                }

                ///Validaciones del cuerpo de la orden
                foreach (var detail in order.Detail)
                {
                    ///Validación 1: Order_id válido.
                    if (order.Order_id != detail.Order_id)
                    {
                        sb.AppendLine("El id de orden de un detalle no coincide con el id de la orden cabecera. Verifique.");
                        result = false;
                    }

                    //Validación 1.1: Cantidad > 0
                    if (detail.Quantity <= 0)
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "El producto {0} no puede indicarse con cantidad menor o igual a 0.", detail.Product_Id));
                        result = false;
                    }

                    ///Validación 2: Producto inexistente
                    var products = this._productDAL.Get();
                    if (products.Where(p => p.Product_Id == detail.Product_Id).Count() <= 0)
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "Id de Producto {0} Inválido o inexistente. Verifique.", detail.Product_Id));
                        result = false;
                    }

                    ///Validación 3: Datos del producto
                    var product = products.First(p => p.Product_Id == detail.Product_Id);
                    //if (product != null && product.Name != detail.Product_Name)
                    //{
                    //    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "El Nombre {0} de producto no coincide con el Id {1}. Verifique.", detail.Product_Description, detail.Product_Id));
                    //    result = false;
                    //}

                    //if (product != null && product.Description != detail.Product_Description)
                    //{
                    //    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "La Descripción {0} de producto no coincide con el Id {1}. Verifique.", detail.Product_Description, detail.Product_Id));
                    //    result = false;
                    //}

                    detail.Product_Name = product.Name;
                    detail.Product_Description = product.Description;
                    //09/12/2019 ITO : DESA-952 Separar los pedidos por marca
                    detail.Category = product.Category;

                    ///Validación 4: Variante válida.
                    ///Puede ser que la variante sea el producto en si, en ese caso se omite la validación.
                    ProductVariation variation = null;
                    if (product != null && product.Product_Id != detail.ProductVariation_Id)
                    {
                        if (product.Variations != null && product.Variations.Count > 0)
                            variation = ObtainVariation(detail.ProductVariation_Id, product.Variations);

                        if (variation == null)
                        {
                            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "La variante {0} no es válida para el producto {1}. Verifique.", detail.ProductVariation_Id, detail.Product_Id));
                            result = false;
                        }
                    }

                    /////Validación 5: Atributo de variante inválido
                    /////Puede ser que la variante sea el producto en si, en ese caso se omite la validación.
                    //if (product != null && variation != null && variation.AttributeValue != detail.AttributeValue)
                    //{
                    //    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "El valor de atributo {0} de la variante {1} no es correcto para el producto {2}. Verifique.", detail.AttributeValue, detail.ProductVariation_Id, detail.Product_Id));
                    //    result = false;
                    //}

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

                    ///Validación 6: Lista de Precios Existente
                    //var prices = this._priceListtDAL.Get();
                    //if (prices.Where(p => p.Name == detail.PriceList_Name).Count() <= 0)
                    //{
                    //    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "Lista de precios {0} Inválido o inexistente. Verifique.", detail.PriceList_Name));
                    //    result = false;
                    //}

                    //var priceList = prices.First(p => p.Name == detail.PriceList_Name);
                    //if (priceList != null && !priceList.Items.Exists(p => p.Product_Id == detail.Product_Id && p.ProductVariation_Id == detail.ProductVariation_Id))
                    //{
                    //    sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "No existe le producto {0} variante {1} en la lista de precios {2}. Verifique.", detail.Product_Id, detail.ProductVariation_Id, detail.PriceList_Name));
                    //    result = false;
                    //}

                    var priceList = prices.FirstOrDefault(l => l.Items.Exists(p => p.Product_Id == detail.Product_Id && p.ProductVariation_Id == detail.ProductVariation_Id));
                    if (priceList == null)
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, "No existe el producto {0} variante {1} en las listas de precios del cliente {2}. Verifique.", detail.Product_Id, detail.ProductVariation_Id, client.Name));
                        result = false;
                    }
                    else
                    {
                        detail.PriceList_Name = priceList.Name;
                        detail.Price = priceList.Items.Find(p => p.Product_Id == detail.Product_Id && p.ProductVariation_Id == detail.ProductVariation_Id).Price;
                    }
                }
            }

            message = sb.ToString();
            return result;
        }

        private ProductVariation ObtainVariation(string productVariation_Id, List<ProductVariation> variations)
        {
            ProductVariation result = null;

            foreach (var variation in variations)
            {
                if (variation.ProductVariation_Id == productVariation_Id) result = variation;

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
