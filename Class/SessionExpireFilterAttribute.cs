using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal_BetaClientes.Class
{
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Validar si existe una sesión y si hay un usuario autenticado.
            if (filterContext.HttpContext.Session == null || filterContext.HttpContext.Session["userID"] == null)
            {
                // Establecer un mensaje de error para mostrar en la vista de inicio de sesión.
                filterContext.HttpContext.Session["LoginRequiredMessage"] = "Inicie sesión para continuar.";

                // Si la session se encuentra inactiva, redireccionar a la página de login.
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "Controller", "Default" },
                        { "Action", "Login" }
                    });
            }
            else
            {
                // Si la session esta activa continuamos ocn la ejecucion de la accion del controlador.
                base.OnActionExecuting(filterContext);
            }

        }








    }
}