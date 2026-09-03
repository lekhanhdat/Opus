using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Opus.RelatedRecords.Utilities
{
    internal static class AveListItemExtension
    {
        public static string GetSingleUserFieldValue(this SPListItem item, string fieldName)
        {
            string userName = string.Empty;
            try
            {
                string fieldValue = item[fieldName].ToString();
                if (fieldValue.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                {
                    fieldValue = fieldValue.Substring("i:0#.w|".Length);
                }
                if (fieldValue.IndexOf(";#") == -1)
                {
                    userName = fieldValue;
                }
                else
                {
                    var userValues = fieldValue.Split(new string[] { ";#" }, StringSplitOptions.None);
                    userName = userValues[1];
                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Get single user field value failed! Item url: {item.Url}, fieldName: {fieldName}, error message: {ex}.");
            }
            return userName;
        }

        public static Guid GetGuidFieldValue(this SPListItem item, string fieldName)
        {
            var fieldVal = item.GetFieldValue(fieldName);
            return Guid.TryParse(fieldVal, out var result) ? result : Guid.Empty;
        }

        public static long GetUTCDateWithTimeZone(this SPListItem item, string fieldName)
        {
            if (item.Fields.ContainsField(fieldName))
            {
                try
                {
                    DateTime dateTime = DateTime.MinValue;
                    var date = (DateTime)item[fieldName];
                    if (date.Kind == DateTimeKind.Utc)
                    {
                        dateTime = date;
                    }
                    else if (date.Kind == DateTimeKind.Local)
                    {
                        dateTime = date.ToUniversalTime();
                    }
                    else
                    {
                        dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, AveTimeZoneUtility.ToTimeZoneInfoId(item.ParentList.ParentWeb.RegionalSettings.TimeZone.ID), "UTC");
                    }
                    return dateTime.Ticks;
                }
                catch (Exception e)
                {
                    Logger.LogError($"An error occurred while getting utc time for field:{fieldName} error:{e}");
                    var dateTime = Convert.ToDateTime(item.GetFieldValue(fieldName));
                    return dateTime.Ticks;
                }
            }
            return 0;
        }

        public static string GetFieldValue(this SPListItem item, string fieldName, string defaultValue = "")
        {
            return item.Fields.ContainsField(fieldName) ? item[fieldName]?.ToString() : defaultValue;
        }

        public static bool IsBlockEditAndDeleteRecord(this SPListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        public static bool IsBlockDeleteOnlyRecord(this SPListItem item)
        {
            return IsBlockDeleteOnlyRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockDeleteOnlyRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.RecordMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.DeleteBlockedMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.EditBlockedMask)) == 0L);
        }

        private static int GetHoldAndRecordStatus(SPListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(new Guid(key)))
                        {
                            object obj2 = item[new Guid(key)];
                            if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                            {
                                result = 0;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occur in get hold and declare status, reason : {ex}");
            }
            return result;
        }

        private static string key = "3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E";
        private static bool IsHoldOrRecordsEnabled(SPList list)
        {
            if (list == null || list.Fields == null)
            {
                throw new ArgumentNullException("list");
            }
            if (list.Fields.Contains(new Guid(key)))
            {
                return (list.Fields[new Guid(key)] != null);
            }
            else
            {
                return false;
            }
        }

        private static bool GetBoolIprPropertyCore(SPList list, string propName)
        {
            bool? nullable = null;
            if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
            {
                object obj = list.RootFolder.Properties[propName];
                if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
            }
            return (nullable == true);
        }
    }
}
