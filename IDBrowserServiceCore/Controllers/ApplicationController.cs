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
        private readonly ServiceSettings serviceSettings;

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="serviceSettings">ServiceSettings</param>
        public ApplicationController(ServiceSettings serviceSettings)
        {
            this.serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
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
                OpenIdConfigurationAddress = serviceSettings.OpenIdConfigurationAddress
            };

            return applicationConfig;
        }
    }
}
