using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Portal_BetaClientes.Class
{
    public class SystemConfigurationProperties
    {
        public static string defaultPassword = "defaultPassword";
        public static string customerImage = "customerImage";
        public static string customerLogo = "customerLogo";
        public static string defaultLanguage = "defaultLanguage";
        public static string defaultUserImage = "defaultUserImage";
        public static string smtpServer = "smtpServer";
        public static string smtpUser = "smtpUser";
        public static string smtpPassword = "smtpPassword";
        public static string smtpAnonymous = "smtpAnonymous";
        public static string systemGeneralName = "Beta_PortalExterno";
        public static string FacilityName = "Facility";

        public static string customerImagePath = "customerImagePath";

        public static string FilesRootPath = "FilesRootPath";
        public static string UrlApiBeta = ConfigurationManager.AppSettings["UrlApiBeta"];

    }
}