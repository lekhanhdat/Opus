/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Records.Core.Utilities;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class CommonUtil
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(CommonUtil));
     
        public static string MakeSPFullUrl(string siteurl, string strUrl)
        {
            if (strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            Uri siteUri = new Uri(siteurl);
            string Protocol = siteUri.Scheme + ":";
            strUrl = strUrl.Trim();
            StringBuilder builder = new StringBuilder(0x200);
            if (strUrl.StartsWith("/"))
            {
                builder.Append(Protocol);
                builder.Append("//");
                builder.Append(siteUri.Host);
                if ((AveSPUtility.StsCompareStrings(Protocol, "http:") && (siteUri.Port != 80)) || (AveSPUtility.StsCompareStrings(Protocol, "https:") && (siteUri.Port != 443)))
                {
                    builder.Append(":");
                    builder.Append(siteUri.Port);
                }
                builder.Append(strUrl);
            }
            else
            {
                builder.Append(siteurl);
                if (strUrl != "")
                {
                    builder.Append("/");
                    builder.Append(strUrl);
                }
            }
            if (builder[builder.Length - 1] == '/')
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return builder.ToString();
        }

        public static string CombinePath(string path, string path1, string path2 = "", string path3 = "")
        {
            var p = string.IsNullOrEmpty(path) ? "" : path;
            var p1 = string.IsNullOrEmpty(path1) ? "" : path1;
            var p2 = string.IsNullOrEmpty(path2) ? "" : path2;
            var p3 = string.IsNullOrEmpty(path3) ? "" : path3;

            return System.IO.Path.Combine(p, p1.TrimStart('\\'), p2.TrimStart('\\'), p3.TrimStart('\\'));
        }

     
     
        public static MoveDestStub GenerateDestStubInfo(IAveFile file)
        {
            MoveDestStub stub = new MoveDestStub();
            stub.ListId = file.ParentFolder.ParentList.ID;
            stub.WebId = file.Web.ID;
            stub.FolderId = file.ParentFolder != null ? file.ParentFolder.UniqueId : Guid.Empty;
            stub.FullPath = MakeSPFullUrl(file.Web.Site.Url, file.ServerRelativeUrl);
            stub.ItemRowId = file.Item.ID;
            stub.ItemId = file.UniqueId;
            stub.LeafName = file.Name;
            stub.DateModified = ToUniversalTimeWithTimeZone((DateTime)(file.Item == null ? file.TimeLastModified : file.Item["Modified"]), file.Web).Ticks;
            stub.DirPath = file.ServerRelativeUrl;
            stub.ParentId = file.ParentFolder.UniqueId;
            stub.DestFlag = (int)RecordFlag.SP;//1 means SP, 2 means FS
            stub.UIVersion = file.UIVersion;
            return stub;
        }

        public static MoveDestStub GenerateDestStubInfo(IAveFolder folder)
        {
            bool isRootFolder = false;
            if (folder.Item == null && folder.ParentList != null)
            {
                isRootFolder = string.Equals(folder.ServerRelativeUrl, folder.ParentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
            }
            MoveDestStub stub = new MoveDestStub();
            stub.ListId = folder.ParentList.ID;
            stub.WebId = folder.ParentWeb.ID;
            stub.FolderId = folder.ParentFolder != null ? folder.ParentFolder.UniqueId : Guid.Empty;
            stub.FullPath = MakeSPFullUrl(folder.ParentWeb.Site.Url, folder.ServerRelativeUrl);
            stub.ItemRowId = folder.Item.ID;
            stub.ItemId = folder.UniqueId;
            stub.LeafName = folder.Name;
            stub.DateModified = (folder.Item != null ? ToUniversalTimeWithTimeZone((DateTime)folder.Item["Modified"], folder.ParentWeb) : (isRootFolder ? ToUniversalTimeWithTimeZone((DateTime)folder.ParentList.LastItemModifiedDate, folder.ParentWeb) : DateTime.UtcNow)).Ticks;
            stub.DirPath = folder.ServerRelativeUrl;
            stub.ParentId = folder.ParentFolder != null ? folder.ParentFolder.UniqueId : Guid.Empty;
            stub.DestFlag = (int)RecordFlag.SP;//1 means SP, 2 means FS
            return stub;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationUrl">目的端容器的Url或者Path</param>
        /// <param name="moveParentPath">目的端文件的Parent Path,如果是目的端root 路径，传string.Empty 即可</param>
        /// <param name="fileName">目的端文件名字</param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GeneralJobReportDesUrl(int sourceType, RecordFlag destFlag, Hashtable illegalCharMap, string destinationUrl, string moveParentPath, string fileName, string version = "")
        {
            StringBuilder strBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(destinationUrl)) throw new Exception("Destination url is null");
            if (string.IsNullOrEmpty(fileName)) throw new Exception("File name is null");
            string slash = "/";
            if (destFlag == RecordFlag.FS)
            {
                slash = "\\";
            }
            if (destFlag == RecordFlag.SP)
            {
                if (sourceType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder)
                {
                    fileName = BaseEscapeName(illegalCharMap, fileName).ToString();
                }
                else if (sourceType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile)
                {
                    fileName = EscapeFileName(fileName, illegalCharMap);
                }
                moveParentPath = EscapeFolderPath(moveParentPath, illegalCharMap);
            }
            strBuilder.Append(destinationUrl);
            if (!string.IsNullOrEmpty(moveParentPath))
            {
                strBuilder.Append(slash);
                strBuilder.Append(moveParentPath);
            }
            strBuilder.Append(slash);
            strBuilder.Append(fileName);
            if (!string.IsNullOrEmpty(version))
            {
                strBuilder.Append(slash);
                strBuilder.Append(version);
            }
            string desUrl = strBuilder.ToString();
            if(destFlag == RecordFlag.FS)
            {
                desUrl = desUrl.Replace("/", slash);
            }
            else
            {
                desUrl = desUrl.Replace("\\", slash);
            }
            return desUrl;
        }

        public static string GeneralSPFullPath(string libraryUrl, string parentFolderUrl, string fileName)
        {
            StringBuilder strBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(libraryUrl)) throw new Exception("LibraryUrl url is null");
            if (string.IsNullOrEmpty(fileName)) throw new Exception("File name is null");
            string slash = "/";
            strBuilder.Append(libraryUrl);
            if (!string.IsNullOrEmpty(parentFolderUrl))
            {
                strBuilder.Append(slash);
                strBuilder.Append(parentFolderUrl);
            }
            strBuilder.Append(slash);
            strBuilder.Append(fileName);
            return strBuilder.ToString();
        }

        /// <summary>
        /// Convert 1.2 to 514
        /// </summary>
        /// <param name="versionLabel"></param>
        /// <returns></returns>
        public static int ConvertVersionLabelToUIVersion(string versionLabel)
        {
            return Convert.ToInt32(versionLabel.Split('.')[0]) * 512 + Convert.ToInt32(versionLabel.Split('.')[1]);
        }

        public static string ConvertUIVersionToVersionLabel(int uiVersion)
        {
            return uiVersion / 512 + "." + uiVersion % 512;
        }

        //将从sharepoint取到的时间转换成UTC时间。
        private static DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                TimeZoneInfo webZone = GeneralSettingConfig.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(web.RegionalSettings.TimeZone.ID));
                datetime = TimeZoneInfo.ConvertTimeToUtc(datetime, webZone);
            }
            return datetime;
        }

        public static string ConvertNodeTypeToReportType(int nodeType)
        {
            string reportType = string.Empty;
            switch (nodeType)
            {
                case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile:
                    //reportType = GCommon.Contract.Tree.Object.NodeLevel.FSFile;
                    break;
                case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder:
                    //reportType = GCommon.Contract.Tree.Object.NodeLevel.FSFolder;
                    break;
                case (int)GCommon.Contract.Tree.Object.NodeLevel.Item:
                case (int)GCommon.Contract.Tree.Object.NodeLevel.ItemVersion:
                    reportType = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                default:
                    break;
            }
            return reportType;
        }

        #region Escape file system name/Check file system length

        public static void ThrowIfOverLength(string name, int limitedLength)
        {
            if (limitedLength > 0 && !string.IsNullOrEmpty(name) && name.Length > limitedLength)
            {
                throw new Exception(I18NString.NameOverLength);
            }
        }
        public static string EscapeFolderPath(string folderPath, Hashtable illegalCharMap)
        {
            try
            {
                if (String.IsNullOrEmpty(folderPath))
                {
                    return string.Empty;
                }
                StringBuilder stBuilder = new StringBuilder();
                foreach(string subFolderName in folderPath.Split('/'))
                {
                    stBuilder.Append(EscapeName(illegalCharMap, subFolderName, false));
                    stBuilder.Append('/');
                }

                return stBuilder.ToString().TrimEnd('/');
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Error in escape folder name, reason : {0}.", ex.ToString()));
                return folderPath;
            }
        }

        public static string EscapeFileName(string fileName, Hashtable illegalCharMap)
        {
            try
            {
                if (String.IsNullOrEmpty(fileName))
                {
                    return string.Empty;
                }

                string extension = Path.GetExtension(fileName);
                string nameWithOutExtension = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrEmpty(nameWithOutExtension))
                {
                    return EscapeName(illegalCharMap, fileName);
                }

                return EscapeName(illegalCharMap, nameWithOutExtension) + extension;
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("Error in escape file name, reason : {0}.", ex.ToString()));
                return fileName;
            }
        }

        private static String EscapeName(Hashtable illegalCharMap, string name, bool isFile = true)
        {
            StringBuilder sbName = BaseEscapeName(illegalCharMap, name);
            //ADO-168282
            //if (!isFile)
            //{
            //    if (sbName[0].Equals('~') && syncCommonMapping != null && syncCommonMapping.ContainsKey('~'))
            //    {
            //        sbName.Replace('~', (char)syncCommonMapping['~'], 0, 1);
            //    }
            //    else
            //    {
            //        sbName.Replace('~', '_', 0, 1);
            //    }
            //}
            string newName = sbName.ToString();
            while (newName.Contains(".."))
            {
                newName = newName.Replace("..", ".");
            }
            return newName;
        }

        private static StringBuilder BaseEscapeName(Hashtable illegalCharMap, string name)
        {
            StringBuilder sbName = new StringBuilder();
            //if (name.Length < illegalCharArray.Length)
            //{
            foreach (char nameChar in name)
            {
                if (illegalCharMap != null && illegalCharMap.ContainsKey(nameChar))
                {
                    sbName.Append(illegalCharMap[nameChar]);
                }
                else
                {
                    sbName.Append(nameChar);
                }
            }
            return sbName;
        }
        #endregion

        #region Record and Hold
        /// <summary>
        /// 此方法返回True 时，表示是Declare，不一定是不是Hold；但是返回False 时，一定不是Declare。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsRecord(IAveListItem item)
        {
            return IsRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsRecord(int holdAndRecordStatus)
        {
            return (holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L;
        }

        /// <summary>
        /// 此方法返回True 时，表示是Block Edit and Delete 类型的Declare, 但是返回false 的时候，不代表不是declare 文件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsBlockEditAndDeleteRecord(IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        /// <summary>
        /// 此方法返回True 时，表示是Block Delete 类型的Declare, 但是返回false 的时候，不代表不是declare 文件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsBlockDeleteOnlyRecord(IAveListItem item)
        {
            return IsBlockDeleteOnlyRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockDeleteOnlyRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.RecordMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.DeleteBlockedMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.EditBlockedMask)) == 0L);
        }

        /// <summary>
        /// 此方法返回True 时，表示是hold，不一定是不是Declare；但是返回False 时，一定不是hold。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsOnHold(IAveListItem item)
        {
            return ((GetHoldAndRecordStatus(item) & (int)HoldAndRecordStatusMask.HoldMask) != 0L);
        }

        /// <summary>
        /// 此方法返回True，表示只是Declare ，不是Hold + Declare;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsRecordOnly(IAveListItem item)
        {
            var status = GetHoldAndRecordStatus(item);
            return ((status & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((status & (int)HoldAndRecordStatusMask.HoldMask) == 0L);
        }

        /// <summary>
        /// 此方法返回True，表示只是Hold ，不是Hold + Declare;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsHoldOnly(IAveListItem item)
        {
            var status = GetHoldAndRecordStatus(item);
            return ((status & (int)HoldAndRecordStatusMask.HoldMask) != 0L) && ((status & (int)HoldAndRecordStatusMask.RecordMask) == 0L);
        }


        internal static Guid HoldRecordStatus
        {
            get
            {
                return new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
            }
        }

        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprProperty(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(HoldRecordStatus))
                        {
                            object obj2 = item[HoldRecordStatus];
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
            catch(Exception ex)
            {
                logger.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
            }
            return result;
        }

        private static bool GetBoolIprProperty(IAveList list, string propName)
        {
            return (GetBoolIprPropertyCore(list, propName) == true);
        }

        private static bool? GetBoolIprPropertyCore(IAveList list, string propName)
        {
            bool? nullable = null;
            if (((list != null) && (list.RootFolder != null)) && (list.RootFolder.Properties != null))
            {
                object obj2 = list.RootFolder.Properties[propName];
                if (obj2 != null)
                {
                    nullable = new bool?(obj2.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
                }
            }
            return nullable;
        }

        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            if ((list == null) || (list.Fields == null))
            {
                throw new ArgumentNullException("list");
            }
            return (list.Fields.TryGetFieldByStaticName(ConstString.ItemHoldRecordStatus) != null);
        }
        #endregion
    }

    internal enum HoldAndRecordStatusMask
    {
        EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
        RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
        DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
        HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
    }

    [Flags]
    public enum RecordRestrictions
    {
        None,
        BlockDelete,
        BlockEdit
    }

}
