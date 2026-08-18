using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAXServices.Contracts
{
    public class OrderB
    {
        public OrderB()
        {
            this.State = -1;
        }

        public int Order_id { get; set; }

        public string NroOrdenCompra { get; set; }

        public string CanalVentas { get; set; }

        public string TipoEnvio { get; set; }

        public int Client_ID { get; set; }
                
        public string SucName { get; set; }

        public string User_ID { get; set; }

        public DateTime Fecha_Emision { get; set; }

        public DateTime Fecha_Vto { get; set; }

        public int Seller_Id { get; set; }

        public int State { get; set; }

        public string StateDescription { get; private set; }

        /// <summary>
        /// A: Anula Pedido anterior
        /// G: aGrega al Pedido anterior
        /// </summary>
        public string UserAction { get; set; }

        public List<OrderBDetail> Detail { get; set; }

        /// <summary>
        /// Calcula el estado de una orden de Pedido
        /// </summary>
        /// <param name="newState">
        /// Estado de una OP en SAX
        /// Estados Posibles:
        /// 0 - FINALIZADAS (OPCH)
        /// 1 - PENDIENTE DE ENTREGA (OPC)
        /// 2 - PENDIENTE DE APROBACION (OPC)
        /// 3 - ANULADAS (OPCH)
        /// </param>
        public void CalculateState(int newState)
        {
            switch (newState)
            {
                case 0:
                    if (this.State <= newState) {
                        this.State = newState;
                        this.StateDescription = "FINALIZADA";
                    }
                    break;
                case 1:
                    if (this.State != newState)
                    {
                        this.State = newState;
                        this.StateDescription = "PENDIENTE DE ENTREGA";
                    }
                    break;
                case 2:
                    if (this.State != 1)
                    {
                        this.State = newState;
                        this.StateDescription = "PENDIENTE DE APROBACION";
                    }
                   break;
                case 3:
                    if (!(this.State > -1 && this.State < newState))
                    {
                        this.State = newState;
                        this.StateDescription = "ANULADAS";
                    }
                    break;
                default:
                    throw new Exception("Estado de la OC inválido en SAX");
            }
        }

        public ClienteWeb cliente { get; set; }
    }

    public class OrderBDetail
    {        

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        //09/12/2019 ITO : DESA-952 Separar los pedidos por marca
        public string Category { get; set; }

        public string Product_Description { get; set; }

        public string ProductVariation_Id { get; set; }

        public string AttributeValue { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public int State { get; set; }

        public string StateDescription
        {
            get
            {
                switch (this.State)
                {
                    case 0: return "FINALIZADA";
                    case 1: return "PENDIENTE DE ENTREGA";
                    case 2: return "PENDIENTE DE APROBACION";
                    case 3: return "ANULADAS";
                    default: throw new Exception("Estado de la OC inválido en SAX");
                }
            }
        }

        public string PriceList_Name { get; set; }
     }
}
