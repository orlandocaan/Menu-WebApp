using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal_BetaClientes.ViewModels
{
	public class Areas
	{
		public int Area_IdArea { get; set; }
		public int Area_IdAreaPadre { get; set; }
		public int Area_IdPlanta { get; set; }
		public string Area_Nombre { get; set; }
		public bool Area_Activo { get; set; }
		public int Area_UsuarioCrea { get; set; }
		public int? Area_UsuarioMod { get; set; }
		public DateTime Area_FechaCrea { get; set; }
		public DateTime? Area_FechaMod { get; set; }
	}
}