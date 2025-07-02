using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal_BetaClientes.ViewModels
{
    public class MenuDinamico
    {
        public string Item { get; set; }
        public string Parent { get; set; }
        public int Type { get; set; }
        public int FunctionId { get; set; }
        public int Functions_FunctionsId { get; set; }
        public decimal Code { get; set; }
        public string Url { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
    }
}