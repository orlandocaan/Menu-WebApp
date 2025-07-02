using BetaClientesVM.Menu;
using Newtonsoft.Json;
using Portal_BetaClientes.Class;
using Portal_BetaClientes.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebGrease.Css.Ast.Selectors;

namespace Portal_BetaClientes.Controllers
{

    public class DefaultController : Controller
    {
        Datos objDatos = new Datos();

        MenuController menuController = new MenuController();

        /// <summary>
        /// Método para mostrar la vista de inicio de la aplicación.
        /// </summary>
        /// <returns>Nos retorna una View</returns>
        [SessionExpireFilter]
        public async Task<ActionResult> Index()
        {
			List<MCarouselVM> listCarousel = await menuController.GetCarousel();
            await ObtenerListaUsuarios();

            return View(listCarousel);
        }


        /// <summary>
        /// Método para mostrar la vista de inicio de sesión.
        /// </summary>
        /// <returns>ActionResult que representa la vista de inicio de sesión.</returns>
        public ActionResult Login()
        {
            if (Session["LoginRequiredMessage"] != null)
            {
                ViewBag.ErrorMessage = Session["LoginRequiredMessage"].ToString();
                Session.Remove("LoginRequiredMessage");
            }

            return View();
        }


        /// <summary>
        /// Método para autenticar las credenciales ingresadas por el usuario en la ventana de inicio de sesión.
        /// </summary>
        /// <param name="username">Nombre de usuario ingresado por usuario</param>
        /// <param name="password">Contraseña ingrsada por el usuario</param>
        /// <returns>Devuelve una vista según el resultado de la autenticación de las credenciales.</returns>
        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            try
            {
                Users objUsuario = new Users();
                objUsuario = await ValidaCredencialesDelUsuario(1, username, password);
                if (objUsuario!=null && objUsuario.UserId>0)
                {
                    Session["UserID"]           =    objUsuario.UserId;
                    Session["UserName"]         =    objUsuario.UserName;
                    Session["Name"]             =    objUsuario.Name;
                    Session["LastName"]         =    objUsuario.LastName;
                    Session["Email"]            =    objUsuario.Email;
                    Session["Img"]              =    objUsuario.Img;
                    Session["RoleId"]           =    objUsuario.RoleId;
                    Session["NombreEmpresa"]    =    objUsuario.NombreEmpresa;
                    Session["NombrePlanta"]     =    objUsuario.NombrePlanta;
                    Session["IdPlanta"]         =    objUsuario.IdPlanta;
                    Session["IdEmpresa"]        =    objUsuario.IdEmpresa;
                    Session["IdErp"]            =    objUsuario.IdErp;

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error al iniciar sesión, verifica tu nombre de usuario o contraseña.";
                    return View();
                }

            }catch(Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/Login()", "0", "Error", "Portal_BetaClientes");
                return View();
            }
        }


        /// <summary>
        /// Método para validar las credenciales del usuario
        /// </summary>
        /// <param name="accion">Numero de acción a ejecutar dentro del StoredProcedure</param>
        /// <param name="username">Nombre de usuario proporcionado en pantalla Login</param>
        /// <param name="password">Contraseña proporcionada en pantalla de Login</param>
        /// <returns>Nos retorna un objeto de Users</returns>
        private async Task<Users> ValidaCredencialesDelUsuario(int accion, string username, string password)
        {
            try
            {
                Users objUser = new Users();
                
                BetaClientesVM.Funciones.UsersVM objUsuario = new BetaClientesVM.Funciones.UsersVM
                {
                    Accion = accion,
                    UserName = username,
                    Password = password
                };

               DataTable dt = await objDatos.ValidarInicioSesion("ApiLogin/ValidarCredencialesLogin", JsonConvert.SerializeObject(objUsuario));

               if (dt!= null)
                {
                    objUser = dt.AsEnumerable().Select(row => new Users
                    {
                        UserId          =     Convert.ToInt32(row.Field<object>("UserId") ?? 0),
                        UserName        =     row.Field<string>("UserName") ?? "",
                        Name            =     row.Field<string>("Name") ?? "",
                        LastName        =     row.Field<string>("LastName") ?? "",
                        Email           =     row.Field<string>("Email") ?? "",
                        RoleId          =     Convert.ToInt32(row.Field<object>("RoleId") ?? 0),
                        NombreEmpresa   =     row.Field<string>("NombreEmpresa") ?? "",
                        NombrePlanta    =     row.Field<string>("NombrePlanta") ?? "",
                        IdEmpresa       =     Convert.ToInt32(row.Field<object>("IdEmpresa") ?? 0),
                        IdPlanta        =     Convert.ToInt32(row.Field<object>("IdPlanta") ?? 0),
                        IdErp           =     row.Field<string>("IdErp") ?? "",
                    }).FirstOrDefault(); 

                }

                return objUser;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ValidaCredencialesDelUsuario()", "0", "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Este método hace unas consulta sobre la tabla de menús y nos devuelve la información de los registros que son padres
        /// </summary>
        /// <param name="accion">Numero de acción a ejecutar en StoredProcedure</param>
        /// <param name="idUsuario">Id del usuario con sesión iniciada</param>
        /// <param name="roleCode">Rol del usuario</param>
        /// <param name="language">Lenguaje del sistema</param>
        /// <returns>Retorna una lista de MenuDinamico que corresponde a los registros que son padres</returns>
        public async Task<List<MenuDinamico>> ObtenerListaPadre(int accion, int idUsuario, int roleCode, string language)
        {
            try
            {
                List<MenuDinamico> lista = new List<MenuDinamico>();
                DataTable dtMenu = await objDatos.DTDatosMenu($"ApiDatos/DTMenuDinamico?accion={accion}&idUsuario={idUsuario}&roleCode={roleCode}&language={language}");
                if (dtMenu.Rows.Count > 0)
                {
                    lista = dtMenu.AsEnumerable()
                        .Select(dataRow => new MenuDinamico
                        {
                            Item = dataRow.Field<string>("Item"),
                            Parent = dataRow.Field<string>("Parent"),
                            Type = Convert.ToInt32(dataRow.Field<object>("Type") ?? 0),
                            FunctionId = Convert.ToInt32(dataRow.Field<object>("FunctionId") ?? 0),
                            Functions_FunctionsId = Convert.ToInt32(dataRow.Field<object>("Functions_FunctionsId") ?? 0),
                            Label = dataRow.Field<string>("Label"),
                            Icon = dataRow.Field<string>("Icon")
                        }).ToList();
                }
                return lista;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ObtenerListaPadre()", "0", "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Este método consulta la información de los registros que son hijos
        /// </summary>
        /// <param name="accion">Numero de acción a ejecutar en StoredProcedure</param>
        /// <param name="idUsuario">Id del usuario con sesión iniciada</param>
        /// <param name="roleCode">Rol del usuario</param>
        /// <param name="language">Lenguaje del sistema</param>
        /// <returns>Retorna una lista de MenuDinamico que corresponde a los registros que son padres</returns>
        public async Task<List<MenuDinamico>> ObtenerListaHijos(int accion, int idUsuario, int roleCode, string language)
        {
            try
            {
                List<MenuDinamico> lista = new List<MenuDinamico>();
                DataTable dtMenu = await objDatos.DTDatosMenu($"ApiDatos/DTMenuDinamico?accion={accion}&idUsuario={idUsuario}&roleCode={roleCode}&language={language}");
                if (dtMenu.Rows.Count > 0)
                {
                    lista = dtMenu.AsEnumerable()
                        .Select(dataRow => new MenuDinamico
                        {
                            Item = dataRow.Field<string>("Item"),
                            Parent = dataRow.Field<string>("Parent"),
                            Type = Convert.ToInt32(dataRow.Field<object>("Type") ?? 0),
                            FunctionId = Convert.ToInt32(dataRow.Field<object>("FunctionId") ?? 0),
                            Functions_FunctionsId = Convert.ToInt32(dataRow.Field<object>("Functions_FunctionsId") ?? 0),
                            Code = Convert.ToDecimal(dataRow.Field<object>("Code") ?? 0),
                            Url = dataRow.Field<string>("Url"),
                            Label = dataRow.Field<string>("Label")
                        }).ToList();
                }
                return lista;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ObtenerListaHijos()", "0", "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Método para actualizar la información basica del usuario
        /// </summary>
        /// <param name="objUsers">Objeto de Users con los datos del usuario</param>
        /// <returns>Nos retorna un resultado por Json(status=boolean, message=string)</returns>
        [HttpPost]
        public async Task<ActionResult> ActualizarInformacionUsuario(Users objUsers)
        {
            bool status = false;
            string message = "";
            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                BetaClientesVM.Funciones.UsersVM objUsersVM = new BetaClientesVM.Funciones.UsersVM
                {
                    Accion = 2,
                    UserId = idUsuario,
                    Name = objUsers.Name,
                    LastName = objUsers.LastName,
                    Email = objUsers.Email,
                    Password = objUsers.Password
                };

                if (await objDatos.ActualizarInformacion("ApiDatos/AdministrarUsuarios", JsonConvert.SerializeObject(objUsersVM)))
                {
                    status = true;
                    message = "Información actualizada correctamente";
                }
                else
                {
                    status = false;
                    message = "Error al actualizar la información";
                }

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ActualizarInformacionUsuario()", "0", "Error", "Portal_BetaClientes");
            }

            return Json(new { success = status, message = message });

        }


        /// <summary>
        /// Método para cerrar la sesión del usuario
        /// </summary>
        /// <returns>Nos redirecciona a la View Login default</returns>
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Contents.RemoveAll();
            return RedirectToAction("Login", "Default");
        }



        /// <summary>
        /// Método para cargar el combo de los Roles
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
        private async Task<ActionResult> ObtenerListaUsuarios()
        {
            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                List<MUserVM> lista = new List<MUserVM>();
                lista = await menuController.GetUsers();
                ViewBag.listaUsuarios = lista;

                return PartialView("_Layout", ViewBag.listaUsuarios);
            }
            catch (Exception ex)
            {
                //await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                ViewBag.listaUsuarios = null;
                return PartialView("_Layout", ViewBag.listaUsuarios);
            }
        }




    }
}