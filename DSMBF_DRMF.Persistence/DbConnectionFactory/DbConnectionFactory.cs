using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace DSMBF_DRMF.Persistence
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private string connectionName = String.Empty;

        public DbConnectionFactory(string connectionName) { this.connectionName = (string.IsNullOrEmpty(connectionName) ? "DefaultDb" : connectionName); }

        private ConnectionStringSettingsCollection GetConnectionStrings()
        {
            ConnectionStringSettingsCollection connStringSettingCollection = ConfigurationManager.ConnectionStrings;

            if (connStringSettingCollection.Count == 0)
            {
                throw new Exception("Application configuration does not contains connection string section. Please configure connection sections.");
            }

            return connStringSettingCollection;
        }

        public IDbConnection GetConnection(string connectionString, string provider)
        {
            System.Data.Common.DbProviderFactory factory = System.Data.Common.DbProviderFactories.GetFactory(provider);
            System.Data.Common.DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = connectionString;
            return conn;
        }

        public IDbConnection GetConnection(string connectionId)
        {
            if (String.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentNullException();
            }

            ConnectionStringSettingsCollection connStringSettingCollection = GetConnectionStrings();

            string connectionString = connStringSettingCollection[connectionId].ConnectionString;
            string provider = connStringSettingCollection[connectionId].ProviderName;

            if (String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(provider))
            {
                throw new ArgumentOutOfRangeException();
            }

            return GetConnection(connectionString, provider);
        }

        public IDbConnection GetConnection()
        {
            ConnectionStringSettingsCollection connStringSettingCollection = GetConnectionStrings();

            string connectionString = connStringSettingCollection[connectionName].ConnectionString;
            string provider = connStringSettingCollection[connectionName].ProviderName;

            if (String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(provider))
            {
                throw new Exception("Application configuration does not contains connection string section");
            }

            return GetConnection(connectionString, provider);
        }

        public string GetConnectionString(string connectionId = "")
        {
            if (string.IsNullOrEmpty(connectionId))
                connectionId = connectionName;

            ConnectionStringSettingsCollection connStringSettingCollection = GetConnectionStrings();

            string connectionString = connStringSettingCollection[connectionId].ConnectionString;
            return connectionString;
        }
    }

    public interface IDbConnectionFactory
    {
        IDbConnection GetConnection();
        IDbConnection GetConnection(string connectionId);
        IDbConnection GetConnection(string connectionString, string provider);
        string GetConnectionString(string connectionId = "");
    }
}
