using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Mvc;
using System;

namespace IDBrowserServiceCore.Controllers
{
    /// <summary>
    /// Controller for application
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class ApplicationController : Controller
    {
        private readonly OpenIdSettings _openIdSettings;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="openIdSettings">OpenIdSettings</param>
        public ApplicationController(OpenIdSettings openIdSettings)
        {
            _openIdSettings = openIdSettings ?? throw new ArgumentNullException(nameof(openIdSettings));
        }

        /// <summary>
        /// Returns the application configuration.
        /// </summary>
        /// <returns>ApplicationConfig</returns>
        [HttpGet]
        public ApplicationConfig GetApplicationConfig()
        {
            var applicationConfig = new ApplicationConfig()
            {
                UserIsAuthenticated = HttpContext.User.Identity.IsAuthenticated,
                OpenIdConfigurationAddress = _openIdSettings.ConfigurationAddress,
                OpenIdAccountManagementAddress = _openIdSettings.AccountManagementAddress,
            };

            return applicationConfig;
        }
    }
}
