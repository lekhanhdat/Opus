using AvePoint.RA.CommonUtil;
using RACommon.SQLiteDatabase;
using RAMultiGeo.SyncCommonData.OtherDCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAMultiGeo.Helper
{
    internal class SQLiteHelper
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SQLiteHelper));

        private static string SQLiteFilePath;
        private static SQLiteClient dbHelper = null;
        public static SQLiteClient DbHelper => dbHelper ??= new SQLiteClient(SQLiteFilePath);

        public static void SetSQLiteFilePath(string filePath)
        {
            if(dbHelper == null)
                SQLiteFilePath = filePath;
        }

        public static void Dispose()
        {
            try
            {
                DbHelper?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to dispose DbHelper.", ex);
            }
        }
    }
}
