using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AdminAppLauncher
{
    public static class ConfigManager
    {
        public static string ConfigPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml"); }
        }

        public static bool ConfigExists()
        {
            return File.Exists(ConfigPath);
        }

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException("配置文件不存在，请先运行配置模式。", ConfigPath);

            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            using (FileStream fs = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read))
            {
                AppConfig config = (AppConfig)serializer.Deserialize(fs);
                if (config == null)
                    throw new InvalidOperationException("配置文件格式无效。");

                if (config.Credentials == null)
                    config.Credentials = new CredentialConfig();

                if (config.Applications == null)
                    config.Applications = new System.Collections.Generic.List<AppEntry>();

                if (string.IsNullOrEmpty(config.Credentials.EncryptedPassword))
                    throw new InvalidOperationException("配置文件中未设置管理员密码，请重新配置。");

                config.Credentials.Password = DecryptPassword(config.Credentials.EncryptedPassword);
                config.Credentials.EncryptedPassword = "";

                return config;
            }
        }

        public static void Save(AppConfig config)
        {
            AppConfig saveConfig = new AppConfig();
            saveConfig.Credentials.Domain = config.Credentials.Domain;
            saveConfig.Credentials.Username = config.Credentials.Username;
            saveConfig.Credentials.EncryptedPassword = EncryptPassword(config.Credentials.Password);
            saveConfig.Applications = config.Applications;

            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (FileStream fs = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = new UTF8Encoding(false);
                using (XmlWriter writer = XmlWriter.Create(fs, settings))
                {
                    serializer.Serialize(writer, saveConfig, ns);
                }
            }
        }

        private static string EncryptPassword(string plainPassword)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptPassword(string encryptedBase64)
        {
            byte[] bytes = Convert.FromBase64String(encryptedBase64);
            byte[] decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
