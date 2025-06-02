namespace IDBrowserServiceCore.Settings
{
    public class OpenIdSettings
    {
        public string ConfigurationAddress { get; set; }
        public string AccountManagementAddress { get; set; }
        public bool DisableServerCertificateValidation { get; set; }
        public string Audience { get; set; }

        public OpenIdSettings()
        {

        }
    }
}
