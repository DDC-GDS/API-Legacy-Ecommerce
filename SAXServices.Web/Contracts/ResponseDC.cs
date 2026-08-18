using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAXServices.Web.Contracts
{
    public class ResponseDC
    {
        public string  Result { get; set; }

        public string Message { get; set; }

        public object datos { get; set; }
    }
}