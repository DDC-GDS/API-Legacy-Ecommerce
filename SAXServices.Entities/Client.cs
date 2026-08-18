using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAXServices.Contracts
{
    public enum enumTipoDocumento    {
        CUIT          = 80,
        CUIL          = 86,
        LE            = 89,
        LC            = 90,
        CI_extranjera = 91,
        Pasaporte     = 94,
        DNI           = 96
    }
    /*TIPO DOC afip
            id Codigo_doc  Tipo_doc
            1   86  CUIL
            2   89  LE
            3   90  LC
            4   91  CI extranjera
            5   94  Pasaporte
            6   96  DNI
            7   80  CUIT*/
    public enum enumTipoDomicilio    {
        personal =1,
        fiscal,
        envio
    }

    public class Client
    {
        public Client() { }
        public Client(int pCienteId,string pCuit,string pName,List<String> pPriceList,int pSellerId, string pEnterpriseGroup, List<ClientSuc> pSucs  )
        {
            Client_ID = pCienteId;
            CUIT = pCuit;
            Name = pName;
            PriceList = pPriceList;
            Seller_id = pSellerId;
            EnterpriseGroup = pEnterpriseGroup;
            Sucs = pSucs; 
        }
        public int Client_ID { get; set; }
        public string  CUIT { get; set; }

        public string Name { get; set; }

        public List<string> PriceList { get; set; }

        public int Seller_id { get; set; }

        public string EnterpriseGroup { get; set; }

        public List<ClientSuc> Sucs { get; set; }
    }

    public class ClientSuc
    {
        public int Client_ID { get; set; }

        public string SucName { get; set; }

        public int Seller_id { get; set; }

        public List<string> PriceList { get; set; }
    }

    public class ClienteWeb
    {
        public int tipoFiscal { get; set; }
        public String documentoTipo { get; set; }
        public String documentoNro { get; set; }
        public String nickName { get; set; }
        public String nombre { get; set; }
        public String apellido { get; set; }
        public ClienteWebDomicilio domicilioFacturacion { get; set; }
        public ClienteWebDomicilio domicilioEnvio { get; set; }
        public String listaPrecios { get; set; }

    }
    
    public  class ClienteWebDomicilio {       
      public String  provinciaId { get; set; }
      public String provinciaNombre { get; set; }
      public String calle { get; set; }
      public String numero { get; set; }
      public String comentarios { get; set; }
      public String ciudad { get; set; }
      public String codigoPostal { get; set; }
      public String telefono { get; set; }
    }
    

}
