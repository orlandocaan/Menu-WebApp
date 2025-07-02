using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal_BetaClientes.ViewModels
{
    public class Users
    {
        public int UserId { get; set; }

        [Display(Name="Nombre de Usuario")]
        public string UserName { get; set; }

        [Display(Name="Contraseña")]
        public string Password { get; set; }

        [Display(Name="Nombre")]
        public string Name { get; set; }

        [Display(Name="Apellidos")]
        public string LastName { get; set; }

        [Display(Name="Correo Electrónico")]
        public string Email { get; set; }
        public bool Enabled { get; set; }

        [Display(Name="Imagen de Perfil")]
        public byte[] Img { get; set; }
        public bool FirstLogin { get; set; }

        [Display(Name="Rol de Usuario")]
        public int RoleId { get; set; }

        public string NombreEmpresa { get; set; }

        public string NombrePlanta { get; set; }

        public int IdEmpresa { get; set; }

        public int IdPlanta { get; set; }

        public string IdErp { get; set; }

    }
}