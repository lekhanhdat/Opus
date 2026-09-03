using AvePoint.GCommon.Utility;
using System;
using System.Data.SQLite;
using System.IO;

namespace AvePoint.RA.DB.SQLiteDB.Reporting.DBManager
{
    public static class SharePointReportSQLiteDBManager
    {
        private const string SQLITE_DB_PREFIX = "SiteMetrics";

        public static string GetDBPath(string jobId, string dirPath)
        {
            var dbName = $"{SQLITE_DB_PREFIX}-{jobId.ToUpper()}.db";
            return SecurityUtils.SafeCombinePath(dirPath, dbName);
        }

        public static string CreateDatabase(string jobId, string dirPath = "")
        {
            var dbPath = GetDBPath(jobId, dirPath);

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
            EnsureDBFolderPath(Path.GetDirectoryName(dbPath));
            SQLiteConnection.CreateFile(dbPath);
            return dbPath;
        }

        private static void EnsureDBFolderPath(string folderPath = "")
        {
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                return;
            }
        }
    }
}