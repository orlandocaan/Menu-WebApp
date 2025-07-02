using Microsoft.Ajax.Utilities;
using Portal_BetaClientes.Class;
using Portal_BetaClientes.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using PMLLogica;
using Newtonsoft.Json.Linq;
using BetaClientesVM.PML;
using System.Net.NetworkInformation;
using System.IO;
using BetaClientesVM.Funciones;
using System.Runtime.InteropServices;

namespace Portal_BetaClientes.Controllers
{
    [SessionExpireFilter]
    public class PMLController : Controller
    {

        /// <summary>
        /// Controlador para la gestión de PML (Procesamiento de la lógica).
        /// </summary>
        private readonly Datos objDatos = new Datos();
        private readonly PMLLogica.PML.PMLGestorDatos objPMLLogica;


        /// <summary>
        /// Constructor por defecto de PMLController.
        /// Inicializa el objeto PMLLogica.DatosApiClientes con la URL de la API Beta y el ID de usuario de la sesión actual.
        /// </summary>
        public PMLController()
        {
            if (System.Web.HttpContext.Current.Session["UserID"] != null)
            {
                objPMLLogica = new PMLLogica.PML.PMLGestorDatos(Convert.ToInt32(System.Web.HttpContext.Current.Session["UserID"]),
                                                                Convert.ToInt32(System.Web.HttpContext.Current.Session["IdPlanta"]),
                                                                Convert.ToInt32(System.Web.HttpContext.Current.Session["IdEmpresa"]),
                                                                SystemConfigurationProperties.UrlApiBeta,
                                                                Convert.ToString(System.Web.HttpContext.Current.Session["IdErp"]));
            }
        }



        /// <summary>
        /// Método que  nos permite cargar la vista de los equipos index
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Nos retorna una View()</returns>
        public async Task<ActionResult> EquiposIndex()
        {
            await ComboTipoEquipos();
            await ComboAreas();
            await ComboTurnos();
            ViewBag.MensajeError = TempData["MensajeError"];
            return View();

        }

        /// <summary>
        /// Método que  nos permite cargar la vista de los Cursos index
        /// GFLT 18/04/2024
        /// </summary>
        /// <returns>Nos retorna una View()</returns>
        public async Task<ActionResult> CursosIndex()
        {
            return View();
        }

        /// <summary>
        /// Método que  nos permite cargar la vista de los Cursos index
        /// GFLT 18/04/2024
        /// </summary>
        /// <returns>Nos retorna una View()</returns>
        public async Task<ActionResult> TurnosIndex()
        {
            return View();
        }

        
        public async Task<ActionResult> ColaboradoresIndex()
        {
            await ComboPuestos();
			return View();
		}


        /// <summary>
        /// Carga los datos necesarios para un elemento seleccionable de los tipos de equipos en la vista.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="selected">El valor seleccionado por defecto, si lo hay.</param>
        /// <returns>Un objeto SelectList para manipular los Tipos de Equipos.</returns>
        private async Task ComboTipoEquipos(object selected = null)
        {

            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaTipoEquipo  =   new List<BetaClientesVM.Funciones.ComboVM>();
                listaTipoEquipo                                         =   await objPMLLogica.ComboTipoEquipos();
                ViewBag.cmbTipoEquipo                                   =   new SelectList(listaTipoEquipo, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboTipoEquipos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbTipoEquipo = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Método para cargar el combo de las Áreas
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular las Áreas.</returns>
        private async Task ComboAreas(object selected = null)
        {

            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaAreas   =   new List<BetaClientesVM.Funciones.ComboVM>();
                listaAreas                                          =   await objPMLLogica.ComboAreas();
                ViewBag.cmbArea                                     =   new SelectList(listaAreas, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboAreas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbArea = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Método para cargar el combo de los Turnos
        /// GFLT 22/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
        private async Task ComboTurnos(object selected = null)
        {

            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaTurnos   =   new List<BetaClientesVM.Funciones.ComboVM>();
                listaTurnos                                          =   await objPMLLogica.ComboTurnos();
                ViewBag.cmbTurno                                     =   new SelectList(listaTurnos, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboTurnos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbTurno = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }




        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarEquiposActivos()
        {
            try
            {
                return PartialView("_ListaEquipos", await objPMLLogica.ListaEquiposPlanta());
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarEquiposActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaEquipos", null);
            }
        }


        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarCursos()
        {
            try
            {
                return PartialView("_PMLListaCursos", await objPMLLogica.ListaCursos());
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarCursos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_PMLListaCursos", null);
            }
        }


        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Equipos.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="objEquiposVM">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarEquipos(PMLEquiposVM objEquiposVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarEquipos(objEquiposVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarEquipos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }


		/// <summary>
		/// Método para consultar los equipos activos y devolver una PartialView que los muestra.
		/// JGPJ 15/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los equipos activos.</returns>
		[HttpGet]
		public async Task<PartialViewResult> ConsultarColaboradoresActivos()
		{
			try
			{
				return PartialView("_ListaColaboradores", await objPMLLogica.ListaColaboradores());
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarColaboradoresActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return PartialView("_ListaColaboradores", null);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Colaboradores.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objColaboradoresVM">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarColaboradores(PMLColaboradoresVM objColaboradoresVM)
		{
			try
			{
				var datos = await objPMLLogica.GestionarColaboradores(objColaboradoresVM);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarColaboradores()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}


		/// <summary>
		/// Método para cargar el combo de los Puestos
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="selected">Valor seleccionado por defecto (opcional).</param>
		/// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
		private async Task ComboPuestos(object selected = null)
		{
			int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaPuestos = new List<BetaClientesVM.Funciones.ComboVM>();
                listaPuestos                                        = await objPMLLogica.ListaPuestos();
				ViewBag.cmbPuestos                                  = new SelectList(listaPuestos, "ValorEntero", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboTurnos()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbPuestos = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

		/// <summary>
		/// Método para consultar los cursos y devolver una PartialView que los muestra.
		/// OCA 17/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los cursos activos para asignar.</returns>
		[HttpGet]
		public async Task<PartialViewResult> CursosAsignados() {
			try
			{
                PMLCursosVM cursos = new PMLCursosVM();
				return PartialView("_CursosAsignados", cursos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarColaboradoresActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return PartialView("_CursosAsignados", null);
			}
		}

		/// <summary>
		/// Este método nos permite cargar la vista de Calificaciones Cursos.
		/// JGPJ 16/04/2024
		/// </summary>
		/// <param name="IdColaborador">El identificador del equipo para el cual se cargará el plan de limpieza.</param>
		/// <returns>Retorna la vista de Plan de Limpieza de un equipo.</returns>
		[HttpGet]
		public async Task<ActionResult> CalificacionesCursos(int idColaborador)
		{
			try
			{
				//BetaClientesVM.PML.PMLEquiposVM objEquipos = await objPMLLogica.ConsultarDatosEquipo(idEquipo);
				//if (objEquipos.Equi_IdEquipo == null || objEquipos.Equi_IdEquipo == 0)
				//{
				//	TempData["MensajeError"] = "No se ha podido recuperar la información del equipo.";
				//	return RedirectToAction("ColaboradoresIndex", "PML");
				//}


				return View();
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/CalificacionesCursos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				TempData["MensajeError"] = "Ha ocurrido una excepción al intentar cargar la vista de CalificacionesCursos().";
				return RedirectToAction("ColaboradoresIndex", "PML");
			}
		}

		/// <summary>
		/// Método para consultar los equipos activos y devolver una PartialView que los muestra.
		/// JGPJ 15/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los equipos activos.</returns>
		[HttpGet]
		public async Task<PartialViewResult> ConsultarCalificaciones(int idColaborador)
		{
			try
			{
				List<PMLCalificacionesVM> calificaciones = await objPMLLogica.ListaCalificaciones(idColaborador);
                
				//Ordenamos las calificaciones por fecha de curso descendente
				calificaciones = calificaciones.OrderByDescending(x => x.Cal_FechaCurso).ToList();
                
				return PartialView("_ListaCalificaciones", calificaciones);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarCalificaciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return PartialView("_ListaCalificaciones", null);
			}
		}

        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Cursos.
        /// GFLT 22/04/2024
        /// </summary>
        /// <param name="objCursosVM">Objeto de datos objCursosVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarCursos(PMLCursosVM objCursosVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarCursos(objCursosVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarCursos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }

        /// <summary>
        /// Método para consultar los elementos de Area y devolver una PartialView que los muestra.
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los elementos de Area.</returns>
        [HttpGet]
        public async Task<PartialViewResult> BuscadorArea()
        {
            try
            {
                return PartialView("_BuscadorArea", await objPMLLogica.BuscadorAreas());
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/BuscadorArea()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_BuscadorArea", null);
            }

        }

        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpPost]
        public async Task<PartialViewResult> BuscadorEquipos(int? area)
        {
            try
            {
                return PartialView("_BuscadorEquipos", await objPMLLogica.BuscadorEquipos(area));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/BuscadorEquipos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("c", null);
            }

        }

        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Turnos.
        /// GFLT 22/04/2024
        /// </summary>
        /// <param name="objTurnosVM">Objeto de datos objTurnosVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarTurnos(PMLTurnosVM objTurnosVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarTurnos(objTurnosVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarTurnos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }

        /// <summary>
        /// Método para consultar los Turnos activos y devolver una PartialView que los muestra.
        /// GFLT 22/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los Turnos activos.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarTurnos()
        {
            try
            {
                return PartialView("_PMLListaTurnos", await objPMLLogica.ConsultarTurnos());
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarTurnos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_PMLListaTurnos", null);
            }
        }

        /// <summary>
        /// Este método nos permite cargar la vista de Plan de Limpieza de un equipo.
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="IdEquipo">El identificador del equipo para el cual se cargará el plan de limpieza.</param>
        /// <returns>Retorna la vista de Plan de Limpieza de un equipo.</returns>
        [HttpGet]
        public async Task<ActionResult> PlanDeLimpiezaEquipo(int idEquipo)
        {
            try
            {
                BetaClientesVM.PML.PMLEquiposVM objEquipos  =   await objPMLLogica.ConsultarDatosEquipo(idEquipo);
                if (objEquipos.Equi_IdEquipo == null || objEquipos.Equi_IdEquipo == 0)
                {
                    TempData["MensajeError"] = "No se ha podido recuperar la información del equipo.";
                    return RedirectToAction("EquiposIndex", "PML");
                }


                BetaClientesVM.PML.PMLFrecuenciaVM objFrecuencia = new BetaClientesVM.PML.PMLFrecuenciaVM
                {
                    Frec_IdEquipo = objEquipos.Equi_IdEquipo
                };


                ViewBag.NombreEquipo = objEquipos.Equi_Nombre;


                //Cargamos los combos
                await ComboTipoProductos();
                await ComboUDM();
                await ComboTipoLimpieza();

                return View(objFrecuencia);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/PlanDeLimpiezaEquipo()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                TempData["MensajeError"] = "Ha ocurrido una excepción al intentar cargar la vista de PlanDeLimpiezaEquipo().";
                return RedirectToAction("EquiposIndex", "PML");
            }
        }


        /// <summary>
        /// Método para cargar el combo de los Tipos de Productos
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los tipos de productos</returns>
        private async Task ComboTipoProductos(object selected = null)
        {
            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaTurnos = new List<BetaClientesVM.Funciones.ComboVM>();
                listaTurnos = await objPMLLogica.ComboTipoProductos();
                ViewBag.cmbTipoProductos = new SelectList(listaTurnos, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboTipoProductos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbTipoProductos = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Método para cargar el combo de Unidades de medida (UDM)
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular las unidades de medida (UDM)</returns>
        private async Task ComboUDM(object selected = null)
        {
            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaTurnos = new List<BetaClientesVM.Funciones.ComboVM>();
                listaTurnos = await objPMLLogica.ComboUnidadesDeMedida();
                ViewBag.cmbUnidadesDeMedida = new SelectList(listaTurnos, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboProductos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbUnidadesDeMedida = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }



        /// <summary>
        /// Método para cargar el combo de Tipos de Limpieza
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular lo s tipos de limpieza</returns>
        private async Task ComboTipoLimpieza(object selected = null)
        {
            try
            {
                List<BetaClientesVM.Funciones.ComboVM> listaTurnos = new List<BetaClientesVM.Funciones.ComboVM>();
                listaTurnos = await objPMLLogica.ComboTiposDeLimpieza();
                ViewBag.cmbTiposLimpieza = new SelectList(listaTurnos, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboProductos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbTiposLimpieza = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Método que consulta la lista de frecuencias de limpieza de un equipo.
        /// JGPJ 16/04/2024
        /// </summary>
        /// <param name="idEquipo">El ID del equipo a consultar.</param>
        /// <returns>Una vista parcial con la lista de frecuencias de limpieza.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarFrecuencias(int idEquipo)
        {
            try
            {
                return PartialView("_ListaFrecuencia", await objPMLLogica.ListaFrecuenciaEquipo(idEquipo));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarEquiposActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaFrecuencia", null);
            }
        }


        /// <summary>
        /// Este método nos permite Obtener los datos del combo de productos
        /// JGPJ 22-04-2024
        /// </summary>
        /// <param name="idEquipo"></param>
        /// <returns>Un objeto JsonResult que contiene los datos del combo de productos en formato JSON.</returns>
        [HttpGet]
        public async Task<JsonResult> ObtenerDatosComboProducto(int idTipoProducto)
        {
            try
            {
                return Json(await objPMLLogica.ComboProductos(idTipoProducto), JsonRequestBehavior.AllowGet);
            }catch(Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ObtenerDatosComboProducto()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Este método nos permite obtener los datos del combo de productos
        /// JGPJ 22/04/2024
        /// </summary>
        /// <param name="objFrecuenciaVM">Objeto de datos de PMLFrecuenciaVM</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarFrecuencia(PMLFrecuenciaVM objFrecuenciaVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarFrecuencia(objFrecuenciaVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarFrecuencia()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }



        /// <summary>
        /// Método para cargar la vista de las Áreas.
        /// Este método asincrónico carga la vista principal de las áreas del proyecto,
        /// </summary>
        /// <returns>
        /// Retorna una tarea que eventualmente resultará en una acción de resultado que
        /// representa la página de índice de las áreas.
        /// </returns>
        public async Task<ActionResult> AreasIndex()
        {
            await ComboColaboradoresResponsable();
            await ComboColaboradoresSupervisor();

            return View();
        }



        /// <summary>
        /// Método para cargar el combo de los cobaloradores (General)
        /// JGPJ 23/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los colaboradores</returns>
        private async Task ComboColaboradoresGeneral(object selected = null)
        {
            try
            {

                List<BetaClientesVM.Funciones.ComboVM> listaColaboradores   =     new List<BetaClientesVM.Funciones.ComboVM>();
                listaColaboradores                                          =     await objPMLLogica.ComboColaboradores(null);
                ViewBag.cmbColaboradores                                    =     new SelectList(listaColaboradores, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboColaboradores()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbColaboradores = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Este método nos permite consultar los colaboradores Supervisores
        /// JGPJ 23/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los colaboradores con puesto de Supervisor</returns>
        private async Task ComboColaboradoresSupervisor(object selected = null)
        {
            try
            {

                List<BetaClientesVM.Funciones.ComboVM> listaColaboradores   =      new List<BetaClientesVM.Funciones.ComboVM>();
                listaColaboradores                                          =      await objPMLLogica.ComboColaboradores(11);
                ViewBag.cmbColaboradoresSupervisor                          =      new SelectList(listaColaboradores, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboColaboradores()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbColaboradoresSupervisor = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }



        /// <summary>
        /// Este método nos permite consultar los colaboradores responsables
        /// JGPJ 23/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los colaboradores con puesto de Responsable</returns>
        private async Task ComboColaboradoresResponsable(object selected = null)
        {
            try
            {

                List<BetaClientesVM.Funciones.ComboVM> listaColaboradores   =   new List<BetaClientesVM.Funciones.ComboVM>();
                listaColaboradores                                          =   await objPMLLogica.ComboColaboradores(13);
                ViewBag.cmbColaboradoresResponsable                         =   new SelectList(listaColaboradores, "ValorEntero", "TextoOpcion", selected);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ComboColaboradores()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                ViewBag.cmbColaboradoresResponsable = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
            }
        }


        /// <summary>
        /// Método para consultar las Areas y SubAreas asociadas a la planta
        /// JGPJ 23-04-2024
        /// </summary>
        /// <returns>Una PartialView que muestra las Areas activas.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarAreas()
        {
            try
            {
                return PartialView("_ListaAreasPadre", await objPMLLogica.ListaAreasYSubAreas(null));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarAreas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaAreasPadre", null);
            }
        }



        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Equipos.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <param name="objEquiposVM">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarAreasYSubAreas(PMLAreasVM objAreasVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarAreasYSubAreas(objAreasVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarAreasYSubAreas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }



        /// <summary>
        /// Método para consultar las Areas y SubAreas asociadas a la planta
        /// JGPJ 23-04-2024
        /// </summary>
        /// <returns>Una PartialView que muestra las Areas activas.</returns>
        [HttpGet]
        public async Task<PartialViewResult> ConsultarSubAreas(int idAreaPadre)
        {
            try
            {
                return PartialView("_ListaSubAreas", await objPMLLogica.ListaAreasYSubAreas(idAreaPadre));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarSubAreas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaSubAreas", null);
            }
        }


        /// <summary>
        /// Método que devuelve la vista de la bitácora del programa maestro.
        /// JGPJ 24-04-2024
        /// </summary>
        /// <returns>ActionResult de la vista de la bitácora del programa maestro.</returns>
        public async Task<ActionResult> BitacoraProgramaMaestro()
        {
            ViewBag.MensajeError = TempData["MensajeError"];
            return View();
        }



        /// <summary>
        /// Este método nos permite consultar la información  sobre los reportes programa maestro de limpieza
        /// </summary>
        /// <param name="objBitacoraVM">Objeto de datos PMLBitacoraProgramaMaestroVM datos enviados por formulario</param>
        /// <returns>Nos retorna una partial View donde recorremos la lista de los programas creados en el rango de fechas</returns>
        ///      
        [HttpPost]
        public async Task<PartialViewResult> ConsultarProgramasMaestro(PMLProgramaVM objProgramaVM)
        {
            try
            {
                return PartialView("_ListaProgramaMaestro", await objPMLLogica.ListaProgramaMaestro(objProgramaVM));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarProgramasMaestro()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaProgramaMaestro", null);

            }

        }


        /// <summary>
        /// Método para cargar la vista de ReporteGeneralPML
        /// </summary>
        /// <returns>Nos devuelve una View</returns>
        public async Task<ActionResult> ReporteGeneralPML()
        {
            await ComboTurnos();
            return View();
        }



		/// <summary>
		/// Método para consultar los cursos y devolver una PartialView que los muestra.
		/// OCA 17/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los cursos activos para asignar.</returns>
		[HttpGet]
		public async Task<PartialViewResult> ConsultarCursosSinBotones()
		{
			try
			{
				List<PMLCursosVM> cursos = new List<PMLCursosVM>();
				cursos = await objPMLLogica.ListaCursos();

                return PartialView("_ListaCursos", cursos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarColaboradoresActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaCursos", null);
            }
        }



		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla PML_Calificaciones.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objCalificacionesVM">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarCalificaciones(PMLCalificacionesVM objCalificacionesVM)
		{
			try
			{
				var datos = await objPMLLogica.GestionarCalificaciones(objCalificacionesVM);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarCalificaciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}


        /// <summary>
        /// Este método nos permitirá insertar un nuevo programa maestro de limpieza
        /// JGPJ 26-04-2024
        /// </summary>
        /// <param name="objProgramaVM">Objeto de datos de PMLProgramaVM</param>
        /// <returns>ActionResult con la vista del objeto PMLProgramaVM.</returns>
        [HttpPost]
        public async Task<ActionResult> ReporteGeneralPML(PMLProgramaVM objProgramaVM)
        {
            var respuestaOperacion = await objPMLLogica.GestionarProgramaMaestro(objProgramaVM);

            if (respuestaOperacion.Resultado > 0)
            {
                return View(await objPMLLogica.ConsultarProgramaMaestroEspecifico(objProgramaVM.Pro_IdPrograma));
            }
            else
            {
                ViewBag.MensajeError = respuestaOperacion.Mensaje;
                return View(objProgramaVM);
            }

        }


        /// <summary>
        /// Este método nos permite consultar la información  sobre los reportes programa maestro de limpieza
        /// </summary>
        /// <param name=""></param>
        /// <returns>Nos retorna una PartialView con la información de los equipos que reciben limpieza</returns>
        [HttpPost]
        public async Task<PartialViewResult> ConsultarDetallesDelPrograma(PMLDetalleProgramaVM objDetallesVM)
        {
            try
            {
                return PartialView("_DetallesProgramaPML", await objPMLLogica.ConsultarDetallesDelPrograma(objDetallesVM));
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarEquiposActivos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_DetallesProgramaPML", null);
            }
        }


        /// <summary>
        /// Este método nos devuelve una PartialView del buscador de colaboradores para el programa maestro de limpieza
        /// </summary>
        /// <param name="idEquipo">Recibimos el id del campo sobre el que bajaremos la información</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PartialViewResult> BuscadorColaboradorCalificado(int accion, int? idArea, int? idEquipo)
        {
            ViewBag.IdEquipo = idEquipo;
            return PartialView("_BuscadorColaboradorCalificado", await objPMLLogica.ListaBuscadorColaboradorCalificado(accion, idArea, idEquipo));
        }



        /// <summary>
        /// Este método nos permite insertar un nuevo programa maestro de limpieza
        /// </summary>
        /// <param name="listaProgramasVM">Lista del objeto PMLAreasVM</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor ó igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> ListaGestionarProgramas(PMLProgramaVM objProgramaVM)
        {
            try
            {
                var datos = await objPMLLogica.ListaGestionarProgramas(objProgramaVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ListaGestionarProgramas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }


        /// <summary>
        /// Este método nos permite consultar la información del programa maestro de limpieza
        /// </summary>
        /// <param name="idPrograma">Id del programa a consultar</param>
        /// <returns>Nos devuelve una View()</returns>
        [HttpGet]
        public async Task<ActionResult> DetallesReporteGeneralPML(int idPrograma)
        {
            try
            {
                PMLProgramaVM objPrograma = await objPMLLogica.ConsultarProgramaMaestroEspecifico(idPrograma);
                if (objPrograma.Pro_IdPrograma==0)
                {
                    TempData["MensajeError"] = "No se ha podido recuperar la información del programa";
                    return RedirectToAction("BitacoraProgramaMaestro", "PML");
                }

                await ComboTurnos(objPrograma.Pro_IdTurno);
                objPrograma.ListaDetalles = await objPMLLogica.ConsultarDetallesDelPrograma(new PMLDetalleProgramaVM { Accion= 3, DetPro_IdPrograma = idPrograma });

                return View(objPrograma);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/DetallesReporteGeneralPML()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                TempData["MensajeError"] = "No se ha podido recuperar la información del programa.";
                return RedirectToAction("BitacoraProgramaMaestro", "PML");
            }
        }


        /// <summary>
        /// Este método nos permite consultar los colaboradores activos
        /// </summary>
        /// <param name="idArea">Id del area a consultar</param>
        /// <param name="idPuesto">Id del puesto a consultar</param>
        /// <returns>Nos retorna una List<PMLColaboradoresVM></returns>
        [HttpGet]
        public async Task<PartialViewResult> BuscadorColaborador(int? idArea, int? idPuesto)
        {
            return PartialView("_BuscadorColaborador", await objPMLLogica.ListaColaboradoresAreas(idArea, idPuesto));
        }


        /// <summary>
        /// Este metodo nos permite consultar los equipos agendados para la fecha del programa que no fueron programados la fecha de captura del programa
        /// </summary>
        /// <param name="objDetalles">Objeto de datos de PMLDetalleProgramaVM</param>
        /// <returns>Nos retorna una List<PMLDetalleProgramaVM></returns>
        [HttpPost]
        public async Task<PartialViewResult> AgregarNuevoEquipoPrograma(PMLDetalleProgramaVM objDetalles)
        {
            return PartialView("_AgregarNuevoEquipoPrograma", await objPMLLogica.ConsultarDetallesDelPrograma(objDetalles));
        }



        /// <summary>
        ///Este método nos permite gestionar los registros de la tabla de los detalles 
        /// </summary>
        /// <param name="objCalificacionesVM">Objeto de PMLDetalleProgramaVM</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor ó igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarDetallesPrograma(PMLDetalleProgramaVM objDetallesVM)
        {
            try
            {
                var datos = await objPMLLogica.GestionarDetallesPrograma(objDetallesVM);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarDetallesPrograma()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }



        /// <summary>
        ///Este método nos permite gestionar los registros de la tabla de los detalles 
        /// </summary>
        /// <param name="objCalificacionesVM">Objeto de PMLDetalleProgramaVM</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor ó igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> ListaGestionarDetallesPrograma(List<PMLDetalleProgramaVM> listaDetalles)
        {
            try
            {
                var datos = await objPMLLogica.ListaGestionarDetallesPrograma(listaDetalles);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarDetallesPrograma()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }



        /// <summary>
        /// Este método nos permite actualizar la información de los detalles del programa
        /// </summary>
        /// <param name="listaDetalles"></param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor ó igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> ActualizarDetallesPrograma(List<PMLDetalleProgramaVM> listaDetalles)
        {
            try
            {
                var datos = await objPMLLogica.ActualizarDetallesPrograma(listaDetalles);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ActualizarDetallesPrograma()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }








		/// <summary>
		/// Método para consultar los equipos activos y devolver una PartialView que los muestra.
		/// OCA 25/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los equipos activos.</returns>
		[HttpGet]
		public async Task<PartialViewResult> DLCalificaciones(int idColaborador)
		{
			try
			{
				List<PMLCalificacionesVM> calificaciones = await objPMLLogica.ListaCalificaciones(idColaborador);

				//Ordenamos las calificaciones por fecha de curso descendente
				calificaciones = calificaciones.OrderByDescending(x => x.Cal_FechaCurso).ToList();
				return PartialView("_DLCalificaciones", calificaciones);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/ConsultarCalificaciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return PartialView("_ListaCalificaciones", null);
			}
		}
	}
}