using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace AvePoint.ObjectModel.Common.Cache
{
    public abstract class AveObjectSqliteDBBase
    {
        private static IRALogger _logger = RALogger.GetInstance(typeof(AveObjectSqliteDBBase));
        protected string dbName;
        private string dbdirPath;
        public string dbFilePath;
        public AveObjectSqliteDBBase(string tenantGroupId, string jobid, string aveObjectId = null)
        {
            dbdirPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "Archiver",
                tenantGroupId, jobid, "DB");
            if (!Directory.Exists(dbdirPath))
            {
                DirectoryInfo dbdir = new DirectoryInfo(dbdirPath);
                try
                {
                    dbdir.Create();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to create database directory {dbdirPath}.Ex: {ex}");
                    throw;
                }
            }

            var guidObj = Guid.Empty;
            if (aveObjectId == null || !Guid.TryParse(aveObjectId, out guidObj))
            {
                guidObj = Guid.NewGuid();
            }

            dbName = guidObj.ToString() + "_data.db";
            dbFilePath = Path.Combine(dbdirPath, dbName);

            try
            {
                // should only 1 sqlite db file each aveObject
                if (File.Exists(dbFilePath))
                {
                    File.Delete(dbFilePath);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to delete existing database file {dbFilePath}.Ex: {e}");
            }

            //InitializeDb();
        }

        public virtual SQLiteConnection GetConnection()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = dbFilePath;
            return new SQLiteConnection(builder.ToString());
        }

        public void InitializeDb() => ExecuteWithConnection(connection =>
        {
            using (var command = connection.CreateCommand())
                CreateSchemaIfNotExists(command);
        });

        public abstract void CreateSchemaIfNotExists(IDbCommand command);

        protected void ExecuteWithConnection(Action<SQLiteConnection> action)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                action(connection);
                try
                {
                    Directory.SetLastAccessTimeUtc(dbdirPath, DateTime.UtcNow);
                }
                catch (Exception e)
                {
                    _logger.Warn($@"fail set dir last accesstime utc,ex:{e}");
                }
            }
        }
    }
}
