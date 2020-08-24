using System.ComponentModel;

namespace Intersect.Configuration.Server.Services
{
    public class SmtpConfiguration
    {
        public string FromAddress { get; set; }

        [DefaultValue("")]
        public string FromName { get; set; }

        public string Host { get; set; }

        [DefaultValue(587)]
        public int Port { get; set; }

        [DefaultValue(true)]
        public bool UseSsl { get; set; }

        [DefaultValue("")]
        public string Username { get; set; }

        [DefaultValue("")]
        public string Password { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(FromAddress) && !string.IsNullOrEmpty(Host);
    }
}
