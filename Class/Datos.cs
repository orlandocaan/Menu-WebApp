using BetaClientesVM.Menu;
using Newtonsoft.Json;
using Portal_BetaClientes.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Portal_BetaClientes.Class
{
    public class Datos
    {
		

        /// <summary>
        /// Este método nos permitirá escribir en la bitácora de errores de la aplicación
        /// </summary>
        /// <param name="reporteError">Par escribir sobre columna REPORTE_ERROR</param>
        /// <param name="source">Para escribir sobre la columna SOURCE</param>
        /// <param name="userLogin">Para escribir sobre USER_LOGIN</param>
        /// <returns>Retorna un valor Boolean</returns>
        public async Task<bool> EscribirEnBitacora(string reporteError, string source, string userLogin, string type, string receiver )
        {            
            try
            {
                using (var httpClient = new HttpClient())
                {

                    BITACORA_ERRORES objBitacora = new BITACORA_ERRORES
                    {
                        REPORTE_ERROR = reporteError,
                        SOURCE = source,
                        USER_LOGIN = userLogin,
                        TYPE = type,
                        RECEIVER = receiver
                    };

                    var jsonData = JsonConvert.SerializeObject(objBitacora);

                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url = $"{SystemConfigurationProperties.UrlApiBeta}/{"Datos/BitacoraErroresCliente"}";
                    HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<bool>(content);
                    }
                    else
                    {
                        return false;
                    }
                }

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }




        /// <summary>
        /// Realiza una solicitud HTTP POST a una URL específica de una API con los datos JSON proporcionados y devuelve un valor entero que indica el resultado de la operación.
        /// </summary>
        /// <param name="srtUrl">La ruta relativa de la URL de la API.</param>
        /// <param name="jsonData">Los datos JSON que se enviarán en la solicitud POST.</param>
        /// <returns>Nos retorna un elemento DataTable</returns>
        public async Task<DataTable> ValidarInicioSesion(string srtUrl, string jsonData)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent                    =    new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url                         =    $"{SystemConfigurationProperties.UrlApiBeta}/{srtUrl}";
                    HttpResponseMessage resultResponse =    await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<DataTable>(content);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ValidarInicioSesion()", "0", "Error", "Portal_BetaClientes");
                return null;
            }
        }


        /// <summary>
        /// Método que establece comunicación con la api mediante una petición GET
        /// </summary>
        /// <param name="url">Url del método a conectar</param>
        /// <returns>Nos retorna un elemento DataTable</returns>
        public async Task<DataTable> DTDatosMenu(string url)
        {
            try
            {
                DataTable dt = new DataTable();
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(SystemConfigurationProperties.UrlApiBeta + url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dt = JsonConvert.DeserializeObject<DataTable>(responseBody);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/DTDatosMenu()", "0", "Error", "Portal_BetaClientes");
                return null;
            }

        }



        /// <summary>
        /// Método que establece comunicación con la api mediante una petición GET
        /// </summary>
        /// <param name="url">Url de comunicación con la api (Controller/Método)</param>
        /// <returns>Nos retorna la información como un DataTable</returns>
        public async Task<DataTable> DTObtenerDatosApi(string url)
        {
            try
            {
                DataTable dt = new DataTable();
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(SystemConfigurationProperties.UrlApiBeta + url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dt = JsonConvert.DeserializeObject<DataTable>(responseBody);
                    }
                }

                return dt;
            }catch(Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/DTObtenerDatosApi()", "0", "Error", "Portal_BetaClientes");
                return null;
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="srtUrl"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<bool> ActualizarInformacion(string srtUrl, string jsonData)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url = $"{SystemConfigurationProperties.UrlApiBeta}/{srtUrl}";
                    HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        int resultado = JsonConvert.DeserializeObject<int>(content);
                        return resultado == 1 ? true : false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/ActualizarInformacion()", "0", "Error", "Portal_BetaClientes");
                return false;
            }
        }


        /// <summary>
        /// Método para realizar un Insert a la base de datos
        /// </summary>
        /// <param name="srtUrl">Url de nuestro controlador/método</param>
        /// <param name="jsonData">Objeto pasado como json</param>
        /// <returns>Nos retorna un resultado int en base al resultado (si retorna 0 ocurrió error, si retorna mayor o igual a 1 éxito)</returns>
        public async Task<int> InsertarInformacion(string srtUrl, string jsonData)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    string url = $"{SystemConfigurationProperties.UrlApiBeta}/{srtUrl}";
                    HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
                    if (resultResponse.IsSuccessStatusCode)
                    {
                        string content = await resultResponse.Content.ReadAsStringAsync();
                        int resultado = JsonConvert.DeserializeObject<int>(content);
                        return resultado;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await EscribirEnBitacora(ex.Message, "Portal_Beta_Clientes/Class/Datos/InsertarInformacion()", "0", "Error", "Portal_BetaClientes");
                return 0;
            }
        }

		/// <summary>
		/// Método asincrónico para obtener datos de una API.
		/// JGPJ 15/04/2024 
		/// </summary>
		/// <param name="stUrl">URL a conectar de la API</param>
		/// <param name="jsonData">Objeto de datos pasado por JSON</param>
		/// <returns>Un objeto DataTable que contiene los datos obtenidos si la solicitud es exitosa; de lo contrario, devuelve null.</returns>
		public async Task<DataTable> DTObtenerDatosApi(string stUrl, string jsonData)
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
					string url = $"{SystemConfigurationProperties.UrlApiBeta}/{stUrl}";
					HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
					if (resultResponse.IsSuccessStatusCode)
					{
						string content = await resultResponse.Content.ReadAsStringAsync();
						return JsonConvert.DeserializeObject<DataTable>(content);
					}
					else
					{
						return null;
					}
				}
			}
			catch (Exception ex)
			{
				await EscribirEnBitacora(ex.Message, "Portal_BetaClientes/Datos/DTObtenerDatosApi()", System.Web.HttpContext.Current.Session["UserID"].ToString(), "Error", "Portal_BetaClientes");
				return null;
			}

		}

		/// <summary>
		/// Método asincrónico para obtener datos de una API.
		/// JGPJ 15/04/2024 
		/// </summary>
		/// <param name="stUrl">URL a conectar de la API</param>
		/// <param name="jsonData">Objeto de datos pasado por JSON</param>
		/// <returns>Un objeto DataTable que contiene los datos obtenidos si la solicitud es exitosa; de lo contrario, devuelve null.</returns>
		public async Task<List<MUserVM>> ListUsersApi(string stUrl, string jsonData)
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
					string url = $"{SystemConfigurationProperties.UrlApiBeta}/{stUrl}";
					HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
					if (resultResponse.IsSuccessStatusCode)
					{
						string content = await resultResponse.Content.ReadAsStringAsync();
						return JsonConvert.DeserializeObject<List<MUserVM>>(content);
					}
					else
					{
						return new List<MUserVM> ();
					}
				}
			}
			catch (Exception ex)
			{
				await EscribirEnBitacora(ex.Message, "Portal_BetaClientes/Datos/DTObtenerDatosApi()", System.Web.HttpContext.Current.Session["UserID"].ToString(), "Error", "Portal_BetaClientes");
				return new List<MUserVM>();
			}
		

		}

		/// <summary>
		/// Método para gestionar las operaciones CRUD en la API (Insertar, Actualizar, Eliminar).
		/// Este método envía una solicitud POST a la URL especificada con los datos JSON proporcionados.
		/// Si la solicitud es exitosa, devuelve el resultado como un entero.
		/// Si no es exitosa, devuelve 0.
		/// JGPJ 15/04/2024
		/// </summary>
		/// <param name="stUrl">La parte específica de la URL para la solicitud.</param>
		/// <param name="jsonData">Los datos en formato JSON para enviar en la solicitud.</param>
		/// <returns>Un entero que representa el resultado de la operación. Devuelve 0 si hay algún error.</returns>
		public async Task<int> GestionarCRUD(string stUrl, string jsonData)
		{
			try
			{

				using (var httpClient = new HttpClient())
				{
					var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
					string url = $"{SystemConfigurationProperties.UrlApiBeta}/{stUrl}";
					HttpResponseMessage resultResponse = await httpClient.PostAsync(url, httpContent);
					if (resultResponse.IsSuccessStatusCode)
					{
						string content = await resultResponse.Content.ReadAsStringAsync();
						return JsonConvert.DeserializeObject<int>(content);
					}
					else
					{
						return 0;
					}
				}
			}
			catch (Exception ex)
			{
				await EscribirEnBitacora(ex.Message, "Portal_BetaClientes/Datos/GestionarCRUD()", System.Web.HttpContext.Current.Session["UserID"].ToString(), "Error", "Portal_BetaClientes");
				return 0;
			}

		}
	}
}