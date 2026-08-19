using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdminAppLauncher
{
    public class AppConfig
    {
        public CredentialConfig Credentials { get; set; }
        public List<AppEntry> Applications { get; set; }

        public AppConfig()
        {
            Credentials = new CredentialConfig();
            Applications = new List<AppEntry>();
        }
    }

    public class CredentialConfig
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }

        [XmlIgnore]
        public string Password { get; set; }

        public CredentialConfig()
        {
            Domain = "";
            Username = "";
            EncryptedPassword = "";
            Password = "";
        }
    }

    public class AppEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }

        public AppEntry()
        {
            Name = "";
            Path = "";
            Arguments = "";
            WorkingDirectory = "";
        }

        public AppEntry Clone()
        {
            return new AppEntry
            {
                Name = this.Name,
                Path = this.Path,
                Arguments = this.Arguments,
                WorkingDirectory = this.WorkingDirectory
            };
        }
    }
}
