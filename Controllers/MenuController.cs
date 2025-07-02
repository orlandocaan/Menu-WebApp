using BetaClientesVM.Funciones;
using BetaClientesVM.Menu;
using BetaClientesVM.PML;
using Newtonsoft.Json;
using Portal_BetaClientes.Class;
using Portal_BetaClientes.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Portal_BetaClientes.Controllers
{
    public class MenuController : Controller
    {
        /// <summary>
        /// Controlador para la gestión de PML (Procesamiento de la lógica).
        /// </summary>
        private readonly Datos objDatos = new Datos();
        private readonly int idUsuario = Convert.ToInt32(System.Web.HttpContext.Current.Session["UserID"]);
        private readonly int idPlanta = Convert.ToInt32(System.Web.HttpContext.Current.Session["IdPlanta"]);
        private readonly int idEmpresa = Convert.ToInt32(System.Web.HttpContext.Current.Session["IdEmpresa"]);


        /// <summary>
        /// Constructor por defecto de PMLController.
        /// Inicializa el objeto PMLLogica.Datos con la URL de la API Beta y el ID de usuario de la sesión actual.
        /// </summary>

		// GET: Menu
		[SessionExpireFilter]
		public async Task<ActionResult> UsersIndex()
		{
			await ComboRoles();
			await ComboCompanies();
			await ComboPlantas(0);
			MUserVM userModel = new MUserVM();
			return View(userModel);
		}

        [SessionExpireFilter]
        public async Task<ActionResult> LenguajesIndex()
        {
            List<MLanguageVM> langcodesModel = await ConsultarLangcodes();
            MLabelVM labelModel = new MLabelVM();
            var viewModel = Tuple.Create(labelModel, langcodesModel);

            return View(viewModel);
        }

        // GET: Menu
        [SessionExpireFilter]
        public ActionResult RolesIndex()
        {
            MRoleVM roleModel = new MRoleVM();
            return View(roleModel);
        }

		[SessionExpireFilter]
		public async Task<ActionResult> PlantasIndex()
		{
			await ComboCompanies();
			MPlantaVM plantaModel = new MPlantaVM();
			return View(plantaModel);
		}

		[SessionExpireFilter]
		public async Task<ActionResult> MenusIndex()
		{
			await ComboFolder();
			await ComboFuncion();
			MMenuVM menuModel = new MMenuVM();

			List<MMenuVM> menuFolderModel = await GetMenu(35);
			List<MMenuVM> menuDocumentModel = await GetMenu(36);

			var viewModel = Tuple.Create(menuModel, menuFolderModel, menuDocumentModel);
			return View(viewModel);
		}

		[SessionExpireFilter]
		public async Task<ActionResult> CompaniesIndex()
		{
			MCompanieVM companieModel = new MCompanieVM();

			List<MCompanieVM> listCompanieModel = await GetCompanies();

			var viewModel = Tuple.Create(companieModel, listCompanieModel);
			return View(viewModel);
		}

		[SessionExpireFilter]
		public async Task<ActionResult> SettingsIndex()
		{
			MSettingVM settingModel = new MSettingVM();

			List<MSettingVM> listSettingModel = await GetSettings();

			var viewModel = Tuple.Create(settingModel, listSettingModel);
			return View(viewModel);
		}

		[SessionExpireFilter]
		public async Task<ActionResult> CatalogosIndex()
		{
			MCatalogoTipoVM catalogModel = new MCatalogoTipoVM();

			List<MCatalogoTipoVM> listCatalogModel = await GetCatalogTypes();

			var viewModel = Tuple.Create(catalogModel, listCatalogModel);
			return View(viewModel);
		}

		[SessionExpireFilter]
		public async Task<ActionResult> CarouselIndex()
		{
			List<MCarouselVM> listCarousel = await GetCarousel();
			MCarouselVM carousel = new MCarouselVM();

			var viewModel = Tuple.Create(carousel, listCarousel);

			return View(viewModel);
		}

        [SessionExpireFilter]
        public async Task<ActionResult> PruebaIndex()
        {

            List<MUserVM> userModel = await GetUsers();

            return View(userModel);
        }


        [SessionExpireFilter]
        public async Task<ActionResult> TicketsIndex()
        {

            return View();
        }


        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
		public async Task<ActionResult> ConsultarUsuarios()
		{
			try
			{
				List<MUserVM> users = await GetUsers();
				return PartialView("_ListaUsuarios", users);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarUsuarios()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return PartialView("_ListaUsuarios", null);
			}
		}

		/// <summary>
		/// Método para cargar el combo de los Roles
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="selected">Valor seleccionado por defecto (opcional).</param>
		/// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
		private async Task ComboRoles(object selected = null)
		{
			int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaRoles = new List<BetaClientesVM.Funciones.ComboVM>();
				listaRoles = await ListRoles();
				ViewBag.cmbRoles = new SelectList(listaRoles, "ValorEntero", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbRoles = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

  

        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Users.
        /// OCA 06/05/2024
        /// </summary>
        /// <param name="objUser">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
		public async Task<JsonResult> GestionarUsuarios(MUserVM objUser)
		{
			try
			{
				var datos = await ManageUser(objUser);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarUsuario()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Users.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objUser">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarContraseña(MUserVM objUser)
		{
			try
			{
				var datos = await ManagePassword(objUser);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarContraseña()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Este método carga el ViewBag.cmbPlantas de la vista parcial _ComboPlantas.
		/// OCA 16/05/2024
		/// </summary>
		/// <param name="idEmpresa">Tipo int necesario para la consulta</param>
		[HttpPost]
		public async Task<ActionResult> CargarComboPlantas(int idEmpresa)
		{
			await ComboPlantas(idEmpresa);
			return PartialView("_ComboPlantas"); // Nombre de la vista parcial que renderiza el DropDownListFor
		}

		/// <summary>
		/// Método para cargar el combo de las Plantas
		/// OCA 16/05/2024
		/// </summary>
		/// <param name="selected">Valor seleccionado por defecto (opcional).</param>
		/// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
		private async Task ComboPlantas(int idEmpresa, object selected = null)
		{
			int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaPlantas = new List<BetaClientesVM.Funciones.ComboVM>();
				listaPlantas = await ListPlantas(idEmpresa);
				ViewBag.cmbPlantas = new SelectList(listaPlantas, "ValorEntero", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboPlantas()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbPlantas = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarLenguajes()
        {
            try
            {
                List<MLabelVM> lenguages = await GetLanguages();
                return PartialView("_ListaLenguajes", lenguages);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarLenguajes()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaLenguajes", null);
            }
        }

		/// <summary>
		/// Método para consultar los equipos activos y devolver una PartialView que los muestra.
		/// JGPJ 15/04/2024
		/// </summary>
		/// <returns>Una PartialView que muestra los equipos activos.</returns>
		[HttpGet]
		public async Task<List<MLanguageVM>> ConsultarLangcodes()
		{
			try
			{
				List<MLanguageVM> listLangcodes = new List<MLanguageVM>();
				listLangcodes = await GetLangcodes();
				return listLangcodes;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboRoles()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarRoles()
        {
            try
            {
                List<MRoleVM> users = await GetRoles();
                return PartialView("_ListaRoles", users);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarUsuarios()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaRoles", null);
            }
        }

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Roles.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objRole">Objeto de datos MRoleVMdatos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarRoles(MRoleVM objRole)
		{
			try
			{
				var datos = await ManageRoles(objRole);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarContraseña()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Método para cargar el combo de las Empresas
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="selected">Valor seleccionado por defecto (opcional).</param>
		/// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
		private async Task ComboCompanies(object selected = null)
		{
			int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaCompanies = new List<BetaClientesVM.Funciones.ComboVM>();
				listaCompanies = await ListCompanies();
				ViewBag.cmbCompanies = new SelectList(listaCompanies, "ValorEntero", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboCompanies()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbCompanies = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarPlantas(int? idEmpresa)
        {
            try
            {
                List<MPlantaVM> planta = await GetPlantas(idEmpresa);
                return PartialView("_ListaPlantas", planta);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarUsuarios()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaPlantas", null);
            }
        }

        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Roles.
        /// OCA 06/05/2024
        /// </summary>
        /// <param name="objPlanta">Objeto de datos MRoleVMdatos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarPlantas(MPlantaVM objPlanta)
        {
            objPlanta.IdUsuario = Convert.ToInt32(Session["UserID"]);
            try
            {
                var datos = await ManagePlantas(objPlanta);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarPlanta()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }

		/// <summary>
		/// Método para cargar el combo de las carpetas
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="selected">Valor seleccionado por defecto (opcional).</param>
		/// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
		private async Task ComboFolder(object selected = null)
		{
			int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaFolder = new List<BetaClientesVM.Funciones.ComboVM>();
				listaFolder = await ListFolder();
				ViewBag.cmbFolder = new SelectList(listaFolder, "TextoOpcion", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboFolder()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbFolder = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

		
        /// <summary>
        /// Método para cargar el combo de los Puestos
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="selected">Valor seleccionado por defecto (opcional).</param>
        /// <returns>Nos retorna un objeto SelectList para manipular los turnos.</returns>
        private async Task ComboFuncion(object selected = null)
        {
            int idUsuario = Convert.ToInt32(Session["UserID"]);

			try
			{
				List<BetaClientesVM.Funciones.ComboVM> listaFuncion = new List<BetaClientesVM.Funciones.ComboVM>();
				listaFuncion = await ListFunction();
				ViewBag.cmbFuncion = new SelectList(listaFuncion, "ValorEntero", "TextoOpcion", selected);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ComboFuncion()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				ViewBag.cmbFuncion = new SelectList(null, "ValorEntero", "TextoOpcion", selected);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Users.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objMenu">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarMenu(MMenuVM objMenu)
		{
			try
			{	  
				objMenu.Property = objMenu.Parent == null ? "ParentName" : "Name";
				objMenu.Icon = (objMenu.Type == 1 && objMenu.Icon == null) ? "fas fa-folder-plus" : objMenu.Icon;
				objMenu.Parent = (objMenu.Type == 1 && objMenu.Parent == null) ? "" : objMenu.Parent;

				var datos = await ManageMenu(objMenu);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarUsuario()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Companies.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objCompanie">Objeto de datos MCompanieVM enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarEmpresas(MCompanieVM objCompanie)
		{
			objCompanie.IdUsuario = Convert.ToInt32(Session["UserID"]);
			try
			{
				var datos = await ManageCompanies(objCompanie);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarEmpresas()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}
            




        // GET: Menu/Index
        [SessionExpireFilter]
        public async Task<ActionResult> FuncionesIndex()
        {
			MFunctionsVM objFunctions = new MFunctionsVM();
            return View(objFunctions);
        }


        /// <summary>
        /// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Functions, Labels, Permissions.
        /// GFLT 07/05/2024
        /// </summary>
        /// <param name="objFunction">Objeto de datos MFunctionsVM datos enviados por formulario</param>
        /// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
        [HttpPost]
        public async Task<JsonResult> GestionarFunciones(MFunctionsVM objFunction)
        {
            try
            {
                var datos = await ManageFunction(objFunction);
                return Json(datos);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/PMLController/GestionarUsuario()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
                return Json(errorResponse);
            }
        }

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Settings.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objSetting">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarConfiguraciones(MSettingVM objSetting)
		{
			try
			{
				var datos = await ManageSettings(objSetting);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarConfiguraciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

        /// <summary>
        /// Método para consultar las funciones y devolver una PartialView.
        /// GFLT 06/05/2024
        /// </summary>
        /// <param name="FunctionId">El ID de la función que se desea consultar.</param>
        /// <returns>Una PartialView que muestra las funciones.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarFunciones()
        {
            try
            {
                // Utiliza el parámetro FunctionId según sea necesario en tu lógica para consultar las funciones
                List<MFunctionsVM> users = await GetFunctions();

                return PartialView("_ListaFunctions", users);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarFunciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaFunctions", null);
            }
        }


        /// <summary>
        /// Método para consultar los usuarios y devolver una PartialView.
        /// GFLT 06/05/2024
        /// </summary>
        /// <param name="FunctionsId">El ID de la función que se desea consultar.</param>
        /// <returns>Una PartialView que muestra las usuarios sin asignar.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarUsuariosSinAsignar(int? FunctionsId)
        {
            try
            {
                int functionId = FunctionsId ?? 0; // Si FunctionsId es nulo, establece la variable en 0

                // Utiliza la variable functionId según sea necesario en tu lógica para consultar las funciones
                List<MUserVM> users = await GetUsersForFunctions(functionId);

                return PartialView("_ListaUsuariosSinFuncion", users);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarFunciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaUsuariosSinFuncion", null);
            }
        }


        /// <summary>
        /// Método para consultar las usuarios y devolver una PartialView.
        /// GFLT 06/05/2024
        /// </summary>
        /// <param name="FunctionsId">El ID de la función que se desea consultar.</param>
        /// <returns>Una PartialView que muestra las usuarios asignar.</returns>
        [HttpGet]
        public async Task<ActionResult> ConsultarUsuariosAsignados(int? FunctionsId)
        {
            try
            {
                int functionId = FunctionsId ?? 0; // Si FunctionsId es nulo, establece la variable en 0

                // Utiliza la variable functionId según sea necesario en tu lógica para consultar las funciones
                List<MUserVM> users = await GetUsersWithFunctions(functionId);

                return PartialView("_ListaUsuariosConFuncion", users);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarFunciones()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_ListaUsuariosConFuncion", null);
            }
        }


		/// <summary>
		/// Este método carga el model de la vista parcial _ListaCatOpciones.
		/// OCA 16/05/2024
		/// </summary>
		/// <param name="idCatalogo">Tipo int necesario para la consulta</param>
		[HttpPost]
		public async Task<ActionResult> ListaCatOpciones(int idCatalogo)
		{
			List<MCatalogoOpcionVM> objCatOpcion= await GetCatalogOptions(idCatalogo);
			return PartialView("_ListaCatOpciones", objCatOpcion); 
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Catalogo.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objCatalogo">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarCatalogos(MCatalogoTipoVM objCatalogo)
		{
			try
			{
				objCatalogo.IdUsuario = idUsuario;
				objCatalogo.Catt_Actualizable = true;
				var	datos = await ManageCatalogs(objCatalogo);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarCatalogos()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Catalogo.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objCatalogo">Objeto de datos PMLEquiposVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarCatalogoOpcion(MCatalogoOpcionVM objCatalogo)
		{
			try
			{
				var datos = await ManageCatalogOptions(objCatalogo);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarCatalogoOpcion()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}

		/// <summary>
		/// Este método se encarga de gestionar las operaciones de inserción, actualización y eliminación de la tabla Carousel.
		/// OCA 06/05/2024
		/// </summary>
		/// <param name="objCarousel">Objeto de datos MCarouselVM datos enviados por formulario</param>
		/// <returns>JsonResult que contiene el estado de la operación, un mensaje y un resultado numérico (respuesta 0 error, mayor 0 igual a 1 correcto).</returns>
		[HttpPost]
		public async Task<JsonResult> GestionarCarrusel(MCarouselVM objCarousel)
		{
			try
			{

				var datos = await ManageCarousel(objCarousel);
				return Json(datos);
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/GestionarUsuario()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
				var errorResponse = new { status = false, message = ex.Message, resultado = 0 };
				return Json(errorResponse);
			}
		}


        /// <summary>
        /// Método para consultar los equipos activos y devolver una PartialView que los muestra.
        /// JGPJ 15/04/2024
        /// </summary>
        /// <returns>Una PartialView que muestra los equipos activos.</returns>
        [HttpPost]
        public async Task<ActionResult> ConsultarConversasion(int idUserSecundary)
        {
            try
            {
                List<MMessagesVM> listMessages = await GetMessages(idUsuario, idUserSecundary);
                return PartialView("_Conversations", listMessages);
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/MenuController/ConsultarUsuarios()", Convert.ToInt32(Session["UserID"]).ToString(), "Error", "Portal_BetaClientes");
                return PartialView("_Conversations", null);
            }
        }

        /// -------------------------------------------------- Metodos de conexion -------------------------------------------------- ///



        /// <summary>
        /// Este Método nos permite obtener los lenguajes soportados de label
        /// OCA 02/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MLabelVM</returns>
        public async Task<List<MMenuVM>> GetDataMenu(int accion)
		{
			try
			{
				List<MMenuVM> parents = new List<MMenuVM>();

                MLabelVM objLanguage = new MLabelVM
                {
                    Accion = accion
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetDataMenu", JsonConvert.SerializeObject(objLanguage));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    parents = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MMenuVM
                                   {
                                       Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
                                       Item = dataRow.Field<string>("Item"),
                                       Parent = dataRow.Field<string>("Parent"),
                                       Type = Convert.ToInt32(dataRow.Field<object>("Type") ?? 0),
                                       FunctionId = Convert.ToInt32(dataRow.Field<object>("FunctionId") ?? 0)
                                   }).ToList();
                }

                return parents;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetDataMenu()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MUserVM>> GetUsers()
		{
			try
			{

                MUserVM objUser = new MUserVM
                {
                    Accion = 3
                };


				return await objDatos.ListUsersApi("ApiMenu/GetUsers", JsonConvert.SerializeObject(objUser));
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetUsers()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

        /// <summary>
        /// Este Método nos permite consultar los Puestos
        /// OCA 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de ComboVM</returns>
        public async Task<List<ComboVM>> ListRoles()
        {
            try
            {
                List<ComboVM> listaColaboradores = new List<ComboVM>();

                MRoleVM objRol = new MRoleVM
                {
                    Accion = 4
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetRoles", JsonConvert.SerializeObject(objRol));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaColaboradores = dtConsulta.AsEnumerable()
                                      .Select(dataRow => new ComboVM
                                      {
                                          TextoOpcion = dataRow.Field<string>("Code") ?? "",
                                          ValorEntero = Convert.ToInt32(dataRow.Field<object>("RoleID")),
                                      }).ToList();
                }

                return listaColaboradores;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ListaRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

		/// <summary>
		/// Este Método nos permite consultar los Puestos
		/// OCA 16/05/2024
		/// </summary>
		/// <returns>Nos retorna una Lista de ComboVM</returns>
		public async Task<List<ComboVM>> ListPlantas(int idCompanie)
		{
			try
			{
				List<ComboVM> listPlantas = new List<ComboVM>();

				MPlantaVM objPlanta = new MPlantaVM
				{
					Accion = 30,
					Plan_IdEmpresa = idCompanie
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetPlantas", JsonConvert.SerializeObject(objPlanta));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					listPlantas = dtConsulta.AsEnumerable()
									  .Select(dataRow => new ComboVM
									  {
										  TextoOpcion = dataRow.Field<string>("Plan_Nombre") ?? "",
										  ValorEntero = Convert.ToInt32(dataRow.Field<object>("Plan_IdPlanta")),
									  }).ToList();
				}

				return listPlantas;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ListaPlantas()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objUser">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageUser(MUserVM objUser)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				if (objUser.Password == null)
				{
					objUser.Password = await GetDefaultPassword();
				}

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageUsers", JsonConvert.SerializeObject(objUser));


                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageUser()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }

        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objUser">Objeto de datos de MUserVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> ManagePassword(MUserVM objUser)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                resultado = await objDatos.GestionarCRUD("ApiMenu/ManagePassword", JsonConvert.SerializeObject(objUser));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageUser()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }

        /// <summary>
        /// Este Método nos permite obtener los lenguajes soportados de label
        /// OCA 02/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MLabelVM</returns>
        public async Task<List<MLabelVM>> GetLanguages()
        {
            try
            {
                List<MLabelVM> languages = new List<MLabelVM>();

                MLabelVM objLabel = new MLabelVM
                {
                    Accion = 20
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetLanguages", JsonConvert.SerializeObject(objLabel));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    languages = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MLabelVM
                                   {
                                       Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
                                       langcode = dataRow.Field<string>("langcode"),
                                       Class = dataRow.Field<string>("Class"),
                                       Property = dataRow.Field<string>("Property"),
                                       text = dataRow.Field<string>("text"),
                                       RecordId = Convert.ToInt32(dataRow.Field<object>("RecordId") ?? 0)
                                   }).ToList();
                }

				return languages;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetLanguages()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

        /// <summary>
        /// Este Método nos permite obtener los lenguajes soportados de label
        /// OCA 08/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MLanguajeVM</returns>
        public async Task<List<MLanguageVM>> GetLangcodes()
        {
            try
            {
                List<MLanguageVM> languages = new List<MLanguageVM>();

                MLanguageVM objLang = new MLanguageVM
                {
                    Accion = 21
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetLangcodes", JsonConvert.SerializeObject(objLang));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    languages = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MLanguageVM
                                   {
                                       Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
                                       langCode = dataRow.Field<string>("langCode")
                                   }).ToList();
                }

				return languages;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetLangcodes()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

        /// <summary>
        /// Este Método nos permite obtener los lenguajes soportados de label
        /// OCA 02/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MLabelVM</returns>
        public async Task<List<MRoleVM>> GetRoles()
        {
            try
            {
                List<MRoleVM> users = new List<MRoleVM>();

                MRoleVM objUser = new MRoleVM
                {
                    Accion = 25
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetRoles", JsonConvert.SerializeObject(objUser));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    users = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MRoleVM
                                   {
                                       RoleID = Convert.ToInt32(dataRow.Field<object>("RoleID") ?? 0),
                                       Code = dataRow.Field<string>("Code"),
                                       Description = dataRow.Field<string>("Description")
                                   }).ToList();
                }

                return users;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objRole">Objeto de datos de MUserVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> ManageRoles(MRoleVM objRole)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                resultado = await objDatos.GestionarCRUD("ApiMenu/ManageRoles", JsonConvert.SerializeObject(objRole));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageUser()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }

        /// <summary>
        /// Este Método nos permite consultar los Puestos
        /// OCA 17/04/2024
        /// </summary>
        /// <returns>Nos retorna una Lista de ComboVM</returns>
        public async Task<List<ComboVM>> ListCompanies()
        {
            try
            {
                List<ComboVM> listaCompanies = new List<ComboVM>();

                MCompanieVM objCompanie = new MCompanieVM
                {
                    Accion = 29
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCompanies", JsonConvert.SerializeObject(objCompanie));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    listaCompanies = dtConsulta.AsEnumerable()
                                      .Select(dataRow => new ComboVM
                                      {
                                          TextoOpcion = dataRow.Field<string>("Name") ?? "",
                                          ValorEntero = Convert.ToInt32(dataRow.Field<object>("Id")),
                                      }).ToList();
                }

                return listaCompanies;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ListaRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

        /// <summary>
        /// Este Método nos permite obtener los lenguajes soportados de label
        /// OCA 02/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MLabelVM</returns>
        public async Task<List<MCompanieVM>> GetCompanies()
        {
            try
            {
                List<MCompanieVM> companie = new List<MCompanieVM>();

                MCompanieVM objCompanie = new MCompanieVM
                {
                    Accion = 29
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCompanies", JsonConvert.SerializeObject(objCompanie));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					companie = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MCompanieVM
								   {
									   Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
									   Code = dataRow.Field<string>("Code"),
									   RFC = dataRow.Field<string>("RFC"),
									   Name = dataRow.Field<string>("Name"),
									   Enabled = Convert.ToBoolean(dataRow.Field<object>("Enabled") ?? false),
									   DateTimeCreate = Convert.ToDateTime(dataRow.Field<object>("DateTimeCreate") ?? DateTime.Now),
									   UserCreate = Convert.ToInt32(dataRow.Field<object>("UserCreate") ?? 0),
									   DateTimeModify = Convert.ToDateTime(dataRow.Field<object>("DateTimeModify") ?? DateTime.Now),
									   UserModify = Convert.ToInt32(dataRow.Field<object>("UserModify") ?? 0),
									   ShortName = dataRow.Field<string>("ShortName"),
									   IdErp = dataRow.Field<string>("IdErp")
								   }).ToList();
				}

                return companie;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetPlantas()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objCompanie">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageCompanies(MCompanieVM objCompanie)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageCompanies", JsonConvert.SerializeObject(objCompanie));

				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageCompanies()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MPlantaVM>> GetPlantas(int? idCompanie)
		{
			try
			{
				List<MPlantaVM> planta = new List<MPlantaVM>();

				MPlantaVM objPlanta = new MPlantaVM
				{
					Accion = 30,
					Plan_IdEmpresa = idCompanie ?? 0,
				};

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetPlantas", JsonConvert.SerializeObject(objPlanta));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    planta = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MPlantaVM
                                   {
                                       Plan_IdPlanta = Convert.ToInt32(dataRow.Field<object>("Plan_IdPlanta") ?? 0),
                                       Plan_Nombre = dataRow.Field<string>("Plan_Nombre"),
                                       Plan_Descripcion = dataRow.Field<string>("Plan_Descripcion"),
                                       Plan_Activo = Convert.ToBoolean(dataRow.Field<object>("Plan_Activo") ?? false),
                                       Plan_IdEmpresa = Convert.ToInt32(dataRow.Field<object>("Plan_IdEmpresa") ?? 0),
                                       Plan_Logo = dataRow.Field<string>("Plan_Logo"),
                                       Plan_URL = dataRow.Field<string>("Plan_URL"),
                                       Plan_Telefono = dataRow.Field<string>("Plan_Telefono"),
                                       Plan_Fax = dataRow.Field<string>("Plan_Fax"),
                                       Plan_CP = dataRow.Field<string>("Plan_CP"),
                                       Plan_Calle = dataRow.Field<string>("Plan_Calle"),
                                       Plan_Colonia = dataRow.Field<string>("Plan_Colonia"),
                                       Plan_Ciudad = dataRow.Field<string>("Plan_Ciudad"),
                                       Plan_Email = dataRow.Field<string>("Plan_Email"),
                                       Plan_UsuarioCrea = Convert.ToInt32(dataRow.Field<object>("Plan_UsuarioCrea") ?? 0),
                                       Plan_UsuarioMod = Convert.ToInt32(dataRow.Field<object>("Plan_UsuarioMod") ?? 0),
                                       Plan_FechaCrea = Convert.ToDateTime(dataRow.Field<object>("Plan_FechaCrea") ?? DateTime.Now),
                                       Plan_FechaMod = Convert.ToDateTime(dataRow.Field<object>("Plan_FechaMod") ?? DateTime.Now),
                                       Contacto = dataRow.Field<string>("Plan_Calle") + ", " + dataRow.Field<string>("Plan_Colonia") + ", " + dataRow.Field<string>("Plan_CP") + ", " + dataRow.Field<string>("Plan_Ciudad")

                                   }).ToList();
                }

                return planta;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetPlantas()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

        /// <summary>
        /// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objRole">Objeto de datos de MUserVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> ManagePlantas(MPlantaVM objPlanta)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                resultado = await objDatos.GestionarCRUD("ApiMenu/ManagePlantas", JsonConvert.SerializeObject(objPlanta));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageUser()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

		/// <summary>
		/// Este Método nos permite consultar los Puestos
		/// OCA 17/04/2024
		/// </summary>
		/// <returns>Nos retorna una Lista de ComboVM</returns>
		public async Task<List<ComboVM>> ListFolder()
		{
			try
			{
				List<ComboVM> listFolder = new List<ComboVM>();

				MMenuVM objMenu = new MMenuVM
				{
					Accion = 37
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetDataMenu", JsonConvert.SerializeObject(objMenu));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					listFolder = dtConsulta.AsEnumerable()
									  .Select(dataRow => new ComboVM
									  {
										  TextoOpcion = dataRow.Field<string>("Item") ?? "",
										  ValorEntero = Convert.ToInt32(dataRow.Field<object>("Id")),
									  }).ToList();
				}

				listFolder.Insert(0, new ComboVM { ValorEntero = 0, TextoOpcion = "Tipo Folder" });

				return listFolder;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ListFolder()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Este Método nos permite consultar los Puestos
		/// OCA 17/04/2024
		/// </summary>
		/// <returns>Nos retorna una Lista de ComboVM</returns>
		public async Task<List<ComboVM>> ListFunction()
		{
			try
			{
				List<ComboVM> listFunction = new List<ComboVM>();

				MMenuVM objMenu = new MMenuVM
				{
					Accion = 38
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetDataMenu", JsonConvert.SerializeObject(objMenu));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					listFunction = dtConsulta.AsEnumerable()
									  .Select(dataRow => new ComboVM
									  {
										  TextoOpcion = dataRow.Field<string>("text") ?? "",
										  ValorEntero = Convert.ToInt32(dataRow.Field<object>("RecordId")),
									  }).ToList();
				}

				listFunction.Insert(0, new ComboVM { ValorEntero = 0, TextoOpcion = "Sin Función" });

				return listFunction;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ListaRoles()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MMenuVM>> GetMenu(int accion)
		{
			try
			{
				List<MMenuVM> menu = new List<MMenuVM>();

				MMenuVM objMenu = new MMenuVM
				{
					Accion = accion,
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetDataMenu", JsonConvert.SerializeObject(objMenu));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					menu = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MMenuVM
								   {
									   Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
									   Item = dataRow.Field<string>("Item"),
									   Parent = dataRow.Field<string>("Parent"),
									   Type = Convert.ToInt32(dataRow.Field<object>("Type") ?? 0),
									   FunctionId = Convert.ToInt32(dataRow.Field<object>("FunctionId") ?? 0),
									   Icon = dataRow.Field<string>("Icon"),
									   Descripcion = dataRow.Field<string>("Descripcion")
								   }).ToList();
				}

				return menu;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetMenu()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objMenu">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageMenu(MMenuVM objMenu)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageMenu", JsonConvert.SerializeObject(objMenu));

				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageMenu()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<string> GetDefaultPassword()
		{
			try
			{
				MSettingVM setting = new MSettingVM();

				MSettingVM objSetting = new MSettingVM
				{
					Accion = 46
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetSettings", JsonConvert.SerializeObject(objSetting));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					setting = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MSettingVM
								   {
									   Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
									   Class = dataRow.Field<string>("Class"),
									   RecordId = Convert.ToInt32(dataRow.Field<object>("RecordId") ?? 0),
									   property = dataRow.Field<string>("property"),
									   value = dataRow.Field<string>("value")
								   }).FirstOrDefault();
				}

				return setting.value;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetSettings()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MSettingVM>> GetSettings()
		{
			try
			{
				List<MSettingVM> setting = new List<MSettingVM>();

				MSettingVM objSetting = new MSettingVM
				{
					Accion = 45,
					Class = Portal_BetaClientes.Class.SettingsClasses.SystemConfiguration,
					RecordId = 0
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetSettings", JsonConvert.SerializeObject(objSetting));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					setting = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MSettingVM
								   {
									   Id = Convert.ToInt32(dataRow.Field<object>("Id") ?? 0),
									   Class = dataRow.Field<string>("Class"),
									   RecordId = Convert.ToInt32(dataRow.Field<object>("RecordId") ?? 0),
									   property = dataRow.Field<string>("property"),
									   value = dataRow.Field<string>("value")
								   }).ToList();
				}

				return setting;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetSettings()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objSetting">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageSettings(MSettingVM objSetting)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				objSetting.Class = Portal_BetaClientes.Class.SettingsClasses.SystemConfiguration;
				objSetting.RecordId = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageSettings", JsonConvert.SerializeObject(objSetting));

				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageMenu()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

		public byte[] GetImageBytes(string Img)
		{
			string imagePath = Server.MapPath("~/images/SystemConfigImages/" + Img);

			if (System.IO.File.Exists(imagePath))
			{
				// Lee el archivo de imagen y conviértelo en un array de bytes
				return System.IO.File.ReadAllBytes(imagePath);
			}
			else
			{
				// Maneja el caso en que la imagen no exista, como devolver un array de bytes vacío o lanzar una excepción
				// Por ejemplo, lanzar una excepción personalizada o devolver un mensaje de error.
				throw new FileNotFoundException("La imagen no fue encontrada.", Img);
			}
		}

		public async Task<object> UploadImage(MSettingVM objSetting)
		{
			string imagePath = Server.MapPath("~/images/SystemConfigImages/" + objSetting.ImgName);
			bool estatus = false;
			string mensaje = "";
			int resultado = 0;

			try
			{
			
				System.IO.File.WriteAllBytes(imagePath, objSetting.Img);
				estatus = true;
				mensaje = "La ejecución se realizó correctamente.";

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				estatus = false;
				mensaje = "Ocurrió un error durante la ejecución";

				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/UploadImage()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}




        /// <summary>
        /// Este Método nos permite obtener todas las Funciones 
        /// GFLT 07/05/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MFunctionsVM</returns>
        public async Task<List<MFunctionsVM>> GetFunctions()
        {
            try
            {
                List<MFunctionsVM> functions = new List<MFunctionsVM>();

                MFunctionsVM objfunction = new MFunctionsVM
                {
                    Accion = 9,

                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetFunctions", JsonConvert.SerializeObject(objfunction));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    functions = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MFunctionsVM
                                   {
                                       FunctionsId = Convert.ToInt32(dataRow.Field<object>("FunctionsId") ?? 0),
                                       Code = dataRow.Field<string>("Code"),
                                       Type = Convert.ToInt32(dataRow.Field<object>("Type") ?? 0),
                                       Url = dataRow.Field<string>("Url"),
                                       Label_Text = dataRow.Field<string>("Label_Text")
                                   }).ToList();
                }

                return functions;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuLogica/DatosApiClientes/GetFunctions()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Método que nos permitirá gestionar las Funciones datos del CRUD.
        /// GFLT 07/05/2024
        /// </summary>
        /// <param name="objFunction">Objeto de datos de MUserVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> ManageFunction(MFunctionsVM objFunction)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;


                resultado = await objDatos.GestionarCRUD("ApiMenu/ManageFunction", JsonConvert.SerializeObject(objFunction));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuLogica/DatosApiClientes/ManageFunction()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }


        /// <summary>
        /// Este Método nos permite obtener los usuarios sin funciones
        /// GFLT 08/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MUserVM</returns>
        public async Task<List<MUserVM>> GetUsersForFunctions(int functionId)
        {
            try
            {
                List<MUserVM> users = new List<MUserVM>();

                MUserVM objUser = new MUserVM
                {
                    Accion = 15,

                    FunctionsId = functionId
                };

				return await objDatos.ListUsersApi("ApiMenu/GetUsers", JsonConvert.SerializeObject(objUser));
			}
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuLogica/DatosApiClientes/GetUsers()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Este Método nos permite obtener los usuarios con funciones
        /// GFLT 08/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MUserVM</returns>
        public async Task<List<MUserVM>> GetUsersWithFunctions(int functionId)
        {
            try
            {
                List<MUserVM> users = new List<MUserVM>();

                MUserVM objUser = new MUserVM
                {
                    Accion = 14,

                    FunctionsId = functionId
                };

				return await objDatos.ListUsersApi("ApiMenu/GetUsers", JsonConvert.SerializeObject(objUser));
			}
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuLogica/DatosApiClientes/GetUsers()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MCatalogoTipoVM>> GetCatalogTypes()
		{
			try
			{
				List<MCatalogoTipoVM> catalog = new List<MCatalogoTipoVM>();

				MCatalogoTipoVM objCatalog = new MCatalogoTipoVM
				{
					Accion = 50
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCatalogTypes", JsonConvert.SerializeObject(objCatalog));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					catalog = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MCatalogoTipoVM
								   {
									   Catt_IdCatalogo = Convert.ToInt32(dataRow.Field<object>("Catt_IdCatalogo") ?? 0),
									   Catt_Nombre = dataRow.Field<string>("Catt_Nombre"),
									   Catt_Actualizable = dataRow.Field<bool>("Catt_Actualizable"),
									   Catt_FechaCrea = dataRow.Field<DateTime>("Catt_FechaCrea")
								   }).ToList();
				}

				return catalog;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetCatalogTypes()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MCatalogoOpcionVM>> GetCatalogOptions(int idCatalogo)
		{
			try
			{
				List<MCatalogoOpcionVM> catalog = new List<MCatalogoOpcionVM>();

				MCatalogoOpcionVM objCatalog = new MCatalogoOpcionVM
				{
					Accion = 51,
					Catv_IdCatalogo = idCatalogo
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCatalogOptions", JsonConvert.SerializeObject(objCatalog));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					catalog = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MCatalogoOpcionVM
								   {
									   Catv_IdOpcion = Convert.ToInt32(dataRow.Field<object>("Catv_IdOpcion") ?? 0),
									   Catv_IdCatalogo = Convert.ToInt32(dataRow.Field<object>("Catv_IdCatalogo") ?? 0),
									   Catv_Nombre = dataRow.Field<string>("Catv_Nombre"),
									   Catv_Orden = Convert.ToInt32(dataRow.Field<object>("Catv_Orden") ?? 0),
								   }).ToList();
				}

				return catalog;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetCatalogOptions()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objCatalog">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageCatalogs(MCatalogoTipoVM objCatalog)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageCatalogs", JsonConvert.SerializeObject(objCatalog));


				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageCatalogTypes()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}


		/// <summary>
		/// Método que nos permitirá gestionar los equipos de la planta datos del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objCatalog">Objeto de datos de MUserVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageCatalogOptions(MCatalogoOpcionVM objCatalog)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageCatalogOptions", JsonConvert.SerializeObject(objCatalog));


				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageCatalogOptions()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<int> InsertCatalogType(MCatalogoTipoVM objCatalog)
		{
			try
			{
				objCatalog.IdUsuario = idUsuario;
				objCatalog.Catt_Actualizable = true;
				MCatalogoTipoVM catalog = new MCatalogoTipoVM();

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCatalogTypes", JsonConvert.SerializeObject(objCatalog));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					catalog = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MCatalogoTipoVM
								   {
									   Catt_IdCatalogo = Convert.ToInt32(dataRow.Field<object>("IdCodigo") ?? 0)
								   }).FirstOrDefault();
				}

				return catalog.Catt_IdCatalogo;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/InsertCatalogType()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return 0;
			}
		}

		/// <summary>
		/// Este Método nos permite obtener los lenguajes soportados de label
		/// OCA 02/04/2024
		/// </summary>
		/// <returns>Nos retorna un objeto de MLabelVM</returns>
		public async Task<List<MCarouselVM>> GetCarousel()
		{
			try
			{
				List<MCarouselVM> carousel = new List<MCarouselVM>();

				MCarouselVM objCarousel = new MCarouselVM
				{
					Accion = 58,
				};

				DataTable dtConsulta = new DataTable();
				dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetCarousel", JsonConvert.SerializeObject(objCarousel));

				if (dtConsulta != null && dtConsulta.Rows.Count > 0)
				{
					carousel = dtConsulta.AsEnumerable()
								   .Select(dataRow => new MCarouselVM
								   {
									   Crsl_IdCarousel = Convert.ToInt32(dataRow.Field<object>("Crsl_IdCarousel") ?? 0),
									   Crsl_Title = dataRow.Field<string>("Crsl_Title"),
									   Crsl_Message = dataRow.Field<string>("Crsl_Message"),
									   Crsl_Img = dataRow.Field<string>("Crsl_Img"),
									   Crsl_Active = dataRow.Field<bool>("Crsl_Active"),
									   Crsl_DateEnd = dataRow.Field<DateTime>("Crsl_DateEnd"),
									   Crsl_DateStart = dataRow.Field<DateTime>("Crsl_DateStart")
								   }).ToList();
				}

				return carousel;
			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetCarousel()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return null;
			}
		}

		/// <summary>
		/// Método que nos permitirá gestionar los datos que se muestran en Carrusel del CRUD.
		/// OCA 16/04/2024
		/// </summary>
		/// <param name="objCarousel">Objeto de datos de MCarouselVM</param>
		/// <returns>
		/// Un objeto Task que representa la operación asincrónica. 
		/// La tarea devolverá un objeto anónimo que contiene tres propiedades:
		///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
		///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
		///   - resultado: Un entero que contiene el resultado de la operación.
		/// </returns>
		public async Task<object> ManageCarousel(MCarouselVM objCarousel)
		{
			try
			{
				bool estatus = false;
				string mensaje = "";
				int resultado = 0;

				resultado = await objDatos.GestionarCRUD("ApiMenu/ManageCarousel", JsonConvert.SerializeObject(objCarousel));

				if (resultado == 0)
				{
					estatus = false;
					mensaje = "Ocurrió un error durante la ejecución";
				}
				else
				{
					estatus = true;
					mensaje = "La ejecución se realizó correctamente.";
				}

				return new { status = estatus, message = mensaje, resultado = resultado };

			}
			catch (Exception ex)
			{
				await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/ManageUser()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
				return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
			}
		}

        /// <summary>
        /// Este Método nos permite obtener la Conversasion entre dos usuarios
        /// OCA 02/04/2024
        /// </summary>
        /// <returns>Nos retorna un objeto de MMessagesVM</returns>
        public async Task<List<MMessagesVM>> GetMessages(int idUser1, int idUser2)
        {
            try
            {
                List<MMessagesVM> conversation = new List<MMessagesVM>();

                MMessagesVM objM = new MMessagesVM
                {
                    Accion = 62,
                    UserId1 = idUser1,
                    UserId2 = idUser2,
                };

                DataTable dtConsulta = new DataTable();
                dtConsulta = await objDatos.DTObtenerDatosApi("ApiMenu/GetMessages", JsonConvert.SerializeObject(objM));

                if (dtConsulta != null && dtConsulta.Rows.Count > 0)
                {
                    conversation = dtConsulta.AsEnumerable()
                                   .Select(dataRow => new MMessagesVM
                                   {
                                       IdMessage = Convert.ToInt32(dataRow.Field<object>("IdMessage") ?? 0),
                                       IdConversation = Convert.ToInt32(dataRow.Field<object>("IdConversation") ?? 0),
                                       UserId = Convert.ToInt32(dataRow.Field<object>("UserId") ?? 0),
                                       Content = dataRow.Field<string>("Content"),
                                       DateSend = dataRow.Field<DateTime>("DateSend"),
                                       UserName = dataRow.Field<string>("UserName"),
                                   }).ToList();
                }

                return conversation;
            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/GetMessages()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return null;
            }
        }

        /// <summary>
        /// Método que nos permitirá gestionar los datos que se muestran en Messages.
        /// OCA 16/04/2024
        /// </summary>
        /// <param name="objMessage">Objeto de datos de MMessagesVM</param>
        /// <returns>
        /// Un objeto Task que representa la operación asincrónica. 
        /// La tarea devolverá un objeto anónimo que contiene tres propiedades:
        ///   - status: Un booleano que indica si la operación se realizó correctamente (true) o si ocurrió un error (false).
        ///   - message: Un string que proporciona un mensaje descriptivo sobre el resultado de la operación.
        ///   - resultado: Un entero que contiene el resultado de la operación.
        /// </returns>
        public async Task<object> ManageConversation(MMessagesVM objMessage)
        {
            try
            {
                bool estatus = false;
                string mensaje = "";
                int resultado = 0;

                resultado = await objDatos.GestionarCRUD("ApiMenu/ManageConversation", JsonConvert.SerializeObject(objMessage));

                if (resultado == 0)
                {
                    estatus = false;
                    mensaje = "Ocurrió un error durante la ejecución";
                }
                else
                {
                    estatus = true;
                    mensaje = "La ejecución se realizó correctamente.";
                }

                return new { status = estatus, message = mensaje, resultado = resultado };

            }
            catch (Exception ex)
            {
                await objDatos.EscribirEnBitacora(ex.Message, "MenuController/Datos/StartConversation()", idUsuario.ToString(), "Error", "Portal_BetaClientes");
                return new { status = false, message = "Error en el servidor: " + ex.Message, resultado = 0 };
            }
        }

		[HttpGet]
        public async Task<ActionResult> ObtenerListaUsuarios()
        {
            List<MUserVM> lista = await GetUsers();
            return Json(lista, JsonRequestBehavior.AllowGet);
        }
    }
}