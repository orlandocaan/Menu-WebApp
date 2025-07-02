using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal_BetaClientes.ViewModels
{
    public class BITACORA_ERRORES
    {
        public string REPORTE_ERROR { get; set; }
        public string SOURCE { get; set; }
        public string USER_LOGIN { get; set; }
        public DateTime FECHA { get; set; }
        public string TYPE { get; set; }
        public string RECEIVER { get; set; }
    }
}