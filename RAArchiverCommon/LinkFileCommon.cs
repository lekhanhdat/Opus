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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;
using ADDTAGRESOURCE = Merged18NResources.Archive.ResourceFileForArchiver;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.IO;
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using System.IO.Compression;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using Media.Common;
using System.Collections.Concurrent;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.RA.Contract.RMWeb.Setting;
using StubSettingDto = AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto;
using PnP.Framework.Modernization.Extensions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;
using AvePoint.GCommon.GraphAPI;
using AvePoint.Media.Service.DomainModel;
using Newtonsoft.Json;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public static class LinkFileCommon
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ScheduleConfiguration));
        private readonly static object readFileLock = new object();
        private readonly static object updateListLock = new object();

        private static string StubPageStyleContent = GetArchiverLeaveStubContent();
        private static byte[] mEndUserStubLinkMasterKey = null;
        private static string HtmlStubPageStyleContent = GetArchiverHtmlLeaveStubContent();
        private static string mTenantGroupId = string.Empty;

        public const string LinkFileFieldName = "ArchiverLinkFileType";
        public const string LinkFileFieldID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
        public const string LinkFileVersionString = "1";
        public const string LinkFieldValueDelimiter = "#|";
        public static string StringForSearch = "<div hidden>04fa8c3c-da63-4d38-8606-2aa6649ac133</div>";
        public const string StubReplaceFlag = "STUBCONTENT";
        public const string StubIDContentReplaceFlag = "STUBIDCONTENT";

        public const string ASPXSTUBSUFFIX = "aspx";
        public const string TXTSTUBSUFFIX = "txt";
        public const string HTMLSTUBSUFFIX = "html";
        public const string LINKSTUBSUFFIX = "url";
        public const string ASPXSTUBSUFFIXWITHDOT = ".aspx";
        public const string TXTSTUBSUFFIXWITHDOT = ".txt";
        public const string HTMLSTUBSUFFIXWITHDOT = ".html";
        public const string LINKSTUBSUFFIXWITHDOT = ".url";

        public static List<string> StubFileNameSuffixList = new List<string>() { ASPXSTUBSUFFIXWITHDOT, TXTSTUBSUFFIXWITHDOT, HTMLSTUBSUFFIXWITHDOT, LINKSTUBSUFFIXWITHDOT };
        private static IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();
        private static ConcurrentDictionary<string, StubSettingDto> StubTemplates = new ConcurrentDictionary<string, StubSettingDto>();

        public static byte[] GetFileContent(CultureInfo cultureInfo, ProcessStubfileContent psc)//string contentTypeId, string urlPath, string siteUrl)
        {
            //mConfig.currentRule.LeaveStubMessage = "xxxx";
            string StubPageStyleContentAspx = StubPageStyleContent;
            string StubPageStyleContentHtml = HtmlStubPageStyleContent;
            if (psc.StubType == LeaveStubType.Aspx)
            {
                if (!string.IsNullOrEmpty(psc.ReCenterLink))
                {
                    StubPageStyleContentAspx = StubPageStyleContentAspx.Replace(StubIDContentReplaceFlag, psc.StubId);
                }
                return Encoding.UTF8.GetBytes(StubPageStyleContentAspx.Replace(StubReplaceFlag, CustomizeStubDynamicValues(psc.MessageContent, psc, cultureInfo)));
            }
            else if (psc.StubType == LeaveStubType.Html)
            {
                if (!string.IsNullOrEmpty(psc.ReCenterLink))
                {
                    StubPageStyleContentHtml = StubPageStyleContentHtml.Replace(StubIDContentReplaceFlag, psc.StubId);
                }
                return Encoding.UTF8.GetBytes(StubPageStyleContentHtml.Replace(StubReplaceFlag, CustomizeHtmlStubDynamicValues(psc.MessageContent, psc, cultureInfo)));
            }
            else if (psc.StubType == LeaveStubType.Link)
            {
                return Encoding.UTF8.GetBytes(CustomizeLinkStubDynamicValues(psc.MessageContent, psc, cultureInfo));
            }
            //LeaveStubType txt.
            else
            {
                return Encoding.UTF8.GetBytes(GetFinalCustomizeMessage(psc, psc.StubType, cultureInfo));
            }
        }

        public static string GetStubFileNameSuffix(ScheduleConfiguration mConfig)
        {
            string mSuffix = string.Empty;

            if (mConfig.currentRule.LeaveStubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIX;
            }
            else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIX;
            }
            else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIX;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIX;
            }

            return mSuffix;
        }

        public static string GetStubFileNameSuffix(LeaveStubType stubType)
        {
            string mSuffix = string.Empty;

            if (stubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIX;
            }
            else if (stubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIX;
            }
            else if (stubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIX;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIX;
            }

            return mSuffix;
        }

        public static string GetStubFileNameSuffixWithDot(ScheduleConfiguration mConfig)
        {
            string mSuffix = string.Empty;

            if (mConfig.currentRule.LeaveStubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIXWITHDOT;
            }
            else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIXWITHDOT;
            }
            else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIXWITHDOT;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIXWITHDOT;
            }

            return mSuffix;
        }

        public static string GetStubFileNameSuffixWithDot(LeaveStubType stubType)
        {
            string mSuffix = string.Empty;

            if (stubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIXWITHDOT;
            }
            else if (stubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIXWITHDOT;
            }
            else if (stubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIXWITHDOT;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIXWITHDOT;
            }

            return mSuffix;
        }

        public static bool IsLeaveStubRule(Rule rule)
        {
            if (rule == null)
            {
                return false;
            }
            var isLeaveStub = (rule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument
                    || (rule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub
                    || (rule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
            return isLeaveStub;
        }

        public static bool IsStubFileType(string fileName)
        {
            foreach (string stubType in StubFileNameSuffixList)
            {
                if (fileName.EndsWith(stubType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddLinkField2List(IAveList list)
        {
            try
            {
                if (!list.Fields.ContainsField(LinkFileFieldName))
                {
                    InterAddLinkField2List(list);
                }
                else if (list.Fields[LinkFileFieldName].ID != new Guid(LinkFileFieldID))
                {
                    list.Fields.Delete(LinkFileFieldName);
                    list.Update();
                    InterAddLinkField2List(list);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Add ArchiverLinkFileType field error:{0}", e.ToString());
                throw;
            }
        }

        private static void InterAddLinkField2List(IAveList list)
        {
            lock (updateListLock)
            {
                list.Reload();
                if (!list.Fields.ContainsField(LinkFileCommon.LinkFileFieldName))
                {
                    try
                    {
                        mLog.Info($"Add linkFileFieldXml to list:{list.RootFolder.ServerRelativeUrl}");
                        IAveField linkField = list.Fields.AddFieldAsXml(ScheduleConfiguration.linkFileFieldXml);
                        list.Update();
                    }
                    catch (Exception ie)
                    {
                        mLog.Warn($"AddLinkField2List error {ie.ToString()}");
                        int i = 0;
                        while (i < 3)
                        {
                            i++;
                            try
                            {
                                list.Reload();
                                IAveField linkField = list.Fields.AddFieldAsXml(ScheduleConfiguration.linkFileFieldXml);
                                list.Update();
                                break;
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn($"AddLinkField2List error {ex.ToString()}");
                                if (i == 3)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }
            }

        }

        public static string GenerateLinkFieldValue(string jobid)
        {
            //
            //JobId#|Version#|TypeId#|TimeTicks
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(jobid);
            stringBuilder.Append(LinkFieldValueDelimiter);

            stringBuilder.Append(LinkFileVersionString);
            stringBuilder.Append(LinkFieldValueDelimiter);

            stringBuilder.Append((int)ArchiverLinkFileType.ArchiveStubLinkFile);
            stringBuilder.Append(LinkFieldValueDelimiter);

            stringBuilder.Append(DateTime.UtcNow.Ticks);

            return stringBuilder.ToString();
        }

        public static void SetLinkFieldValue(IAveListItem listItem, ScheduleConfiguration mConfig)
        {
            try
            {
                int retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        retryCount++;
                        AddLinkField2List(listItem.ParentList);
                        listItem[LinkFileCommon.LinkFileFieldName] = LinkFileCommon.GenerateLinkFieldValue(mConfig.JobId);
                        listItem.SystemUpdate();
                        break;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("retry item update hidden column value failed,{0}", e.ToString());
                        if (retryCount == 2)
                        {
                            throw;
                        }
                        listItem.ParentList.ParentWeb.ReloadWeb();
                        listItem.ParentList.Reload();
                        listItem = listItem.ParentList.GetItemByUniqueId(listItem.UniqueId);
                        //AddLinkField2List(listItem.ParentList);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while setting link field value, Item Name:{0}, Error Message:{1}", listItem.Url, e.ToString());
                throw;
            }
        }

        public static void RemoveArchiveStub(IAveFile aveFile, ScheduleConfiguration mConfig)
        {
            //log
            try
            {
                //mLog.Info(string.Format("Begin remove stub file : {0}.", aveDoc.Name + "." + fileNameSUFFIX));
                var stubUrl = aveFile.ServerRelativeUrl + GetStubFileNameSuffixWithDot(mConfig);
                var stubFile = aveFile.Web.GetFile(stubUrl);
                if (stubFile.Exists)
                {
                    if (stubFile.Item != null
                        && stubFile.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                        && stubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName] != null
                        && stubFile.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString().Length > 0)
                    {
                        try
                        {
                            stubFile.Delete();
                        }
                        catch (Exception exp)
                        {
                            mLog.Info($"delete file {stubUrl} exception: {exp.Message}. retry action.");
                            mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(stubFile.Item);
                            stubFile.Delete();
                        }
                        mLog.Info($"Delete stub file : {GetStubFileNameSuffixWithDot(mConfig)} successful.");
                    }
                    else
                    {
                        mLog.Warn($"The file {GetStubFileNameSuffixWithDot(mConfig)} is not a stub.");
                        throw new StubNameConflictException();
                    }
                }
                else
                {
                    mLog.Info(string.Format("stub : {0} does not exist in library.", GetStubFileNameSuffixWithDot(mConfig)));
                }
            }
            catch (StubNameConflictException)
            {
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("Error in remove archive stub. stub name : {0}, reason : {1}.", aveFile.Name, ex.ToString()));
            }
        }

        private static IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();

        public static void DeleteStubFileRecord(string siteId, string archivedItemId)
        {
            try
            {
                StubFileRecordDao.DeleteStubFileRecordEntitiesInBatch(TenantLocalValue.LogonGroupId, siteId, archivedItemId);
            }
            catch (Exception ex)
            {
                mLog.Error($"Error in delete stub file record for file {archivedItemId}, ex: {ex}.");
            }
        }

        public static void FlushStubFileRecordCache()
        {
            try
            {
                StubFileRecordDao.FlushDeleteCache(TenantLocalValue.LogonGroupId);
            }
            catch (Exception ex)
            {
                mLog.Error($"Error in flush stub file record cache, ex: {ex}.");
            }
        }

        public static string ReplaceStubTags(string stubContent, bool isSaveToDB)
        {
            if (string.IsNullOrEmpty(stubContent))
            {
                return string.Empty;
            }
            string stubFileName = $"[{I18NEntity.GetString("StorageOptimization.Gui_9FE3A6A6-DB1B-478A-9C84-3793B070A958")}]";
            string stubFilePath = $"[{I18NEntity.GetString("StorageOptimization.Gui_FB4CF4C0-AA67-43A7-9C37-97719E9B97A3")}]";
            string stubArchivedTime = $"[{I18NEntity.GetString("StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B")}]";
            string stubRuleName = $"[{I18NEntity.GetString("StorageOptimization.Gui_AE414513-8007-44BC-98B9-8E6B1212C257")}]";
            string stubRestoreLink = $"[{I18NEntity.GetString("RM_AR_CP_Stub_Panel_RestoreLink")}]";

            if (isSaveToDB)
            {
                if (stubContent.Contains(stubFileName))
                {
                    stubContent = stubContent.Replace(stubFileName, RMConstants.STUBFILENAMEMAPPING);
                }
                if (stubContent.Contains(stubFilePath))
                {
                    stubContent = stubContent.Replace(stubFilePath, RMConstants.STUBFILEPATHMAPPING);
                }
                if (stubContent.Contains(stubArchivedTime))
                {
                    stubContent = stubContent.Replace(stubArchivedTime, RMConstants.STUBARCHIVEDTIMEMAPPING);
                }
                if (stubContent.Contains(stubRuleName))
                {
                    stubContent = stubContent.Replace(stubRuleName, RMConstants.STUBRULENAMEMAPPING);
                }
                if (stubContent.Contains(stubRestoreLink))
                {
                    stubContent = stubContent.Replace(stubRestoreLink, RMConstants.STUBRESTORELINKMAPPING);
                }
            }
            else
            {
                if (stubContent.Contains(RMConstants.STUBFILENAMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBFILENAMEMAPPING, stubFileName);
                }
                if (stubContent.Contains(RMConstants.STUBFILEPATHMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBFILEPATHMAPPING, stubFilePath);
                }
                if (stubContent.Contains(RMConstants.STUBARCHIVEDTIMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBARCHIVEDTIMEMAPPING, stubArchivedTime);
                }
                if (stubContent.Contains(RMConstants.STUBRULENAMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBRULENAMEMAPPING, stubRuleName);
                }
                if (stubContent.Contains(RMConstants.STUBRESTORELINKMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBRESTORELINKMAPPING, stubRestoreLink);
                }
            }
            return stubContent;
        }

        /// <summary>
        /// 自定义Message需要获取Stub页面样式
        /// </summary>
        /// <returns></returns>
        private static string GetArchiverLeaveStubContent()
        {
            lock (readFileLock)
            {
                string stubPageStylePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "StubTemplate", "ArchiverCommonStubPageStyle.aspx");
                using (FileStream fs = new FileStream(stubPageStylePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] content = new byte[fs.Length];
                        using (var ms = new MemoryStream())
                        {
                            int read;
                            while ((read = br.Read(content, 0, content.Length)) > 0)
                            {
                                ms.Write(content, 0, read);
                            }
                            return Encoding.UTF8.GetString(ms.ToArray(), 0, ms.ToArray().Length);
                        }
                    }
                }
            }
        }
        private static string GetArchiverHtmlLeaveStubContent()
        {
            lock (readFileLock)
            {
                string stubPageStylePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "StubTemplate", "ArchiverCommonHtmlStubPageStyle.html");
                using (FileStream fs = new FileStream(stubPageStylePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte[] content = new byte[fs.Length];
                        using (var ms = new MemoryStream())
                        {
                            int read;
                            while ((read = br.Read(content, 0, content.Length)) > 0)
                            {
                                ms.Write(content, 0, read);
                            }
                            return Encoding.UTF8.GetString(ms.ToArray(), 0, ms.ToArray().Length);
                        }
                    }
                }
            }
        }
        public static string AppendSearchString(string OriginalString)
        {
            if (OriginalString.EndsWith("</asp:Content>"))
            {
                string temp = OriginalString.Substring(0, OriginalString.LastIndexOf("</asp:Content>"));
                return string.Format($"{temp} {StringForSearch} </asp:Content>");
            }
            else
            {
                return string.Format($"{OriginalString} {StringForSearch}");
            }
        }

        public static byte[] GetEndUserStubLinkMasterKey(ScheduleConfiguration mConfig)
        {
            if (mConfig.TenantGroupId == mTenantGroupId && mEndUserStubLinkMasterKey != null)
            {
                return mEndUserStubLinkMasterKey;
            }
            else
            {
                ISettingProfilesDao _SettingProfileDao = null;
                ISettingProfilesDao SettingProfileDao = PlatformWindsorManager.GetService(ref _SettingProfileDao);
                try
                {
                    mEndUserStubLinkMasterKey = SettingProfileDao.GetEndUserStubLinkMasterKey();
                    mTenantGroupId = mConfig.TenantGroupId;
                    return mEndUserStubLinkMasterKey;
                }
                catch (Exception e)
                {
                    mLog.Error(string.Format("Error in get EndUserStubLinkMasterKey. reason : {0}.", e.ToString()));
                    return null;
                }
            }
        }
        public static string AppendHtmlSearchString(string OriginalString)
        {
            if (OriginalString.EndsWith("</asp:Content>"))
            {
                string temp = OriginalString.Substring(0, OriginalString.LastIndexOf("</asp:Content>"));
                return string.Format($"{temp} {StringForSearch} </body></html>");
            }
            else
            {
                return string.Format($"{OriginalString} {StringForSearch}");
            }
        }
        public static string CustomizeStubDynamicValues(string OriginalString, ProcessStubfileContent psc, CultureInfo cultureInfo)
        {
            LeaveStubType stubType = LeaveStubType.Aspx;
            //if (OriginalString.EndsWith("</asp:Content>"))
            //{
            //    StringBuilder targetString = new StringBuilder();
            //    string temp = OriginalString.Substring(0, OriginalString.LastIndexOf("</asp:Content>"));
            //    //string tempUtility = System.Web.HttpUtility.HtmlEncode(temp);
            //    targetString.Append(StringForSearch + GetFinalCustomizeMessage(psc) + "</asp:Content>");
            //    return targetString.ToString();
            //}
            //else
            //{
            //    StringBuilder targetString = new StringBuilder();
            //    //string tempUtility = System.Web.HttpUtility.HtmlEncode(OriginalString);
            //    targetString.Append(StringForSearch + GetFinalCustomizeMessage(psc));
            //    return targetString.ToString();
            //}
            return GetFinalCustomizeMessage(psc, stubType, cultureInfo);

        }
        public static string CustomizeHtmlStubDynamicValues(string OriginalString, ProcessStubfileContent psc, CultureInfo cultureInfo)
        {
            LeaveStubType stubType = LeaveStubType.Html;
            //if (OriginalString.EndsWith("</asp:Content>"))
            //{
            //    StringBuilder targetString = new StringBuilder();
            //    string temp = OriginalString.Substring(0, OriginalString.LastIndexOf("</asp:Content>"));
            //    //string tempUtility = System.Web.HttpUtility.HtmlEncode(temp);
            //    targetString.Append(StringForSearch + GetFinalCustomizeMessage(psc) + "</body></html>");
            //    return targetString.ToString();
            //}
            //else
            //{
            //    StringBuilder targetString = new StringBuilder();
            //    //string tempUtility = System.Web.HttpUtility.HtmlEncode(OriginalString);
            //    targetString.Append(StringForSearch + GetFinalCustomizeMessage(psc));
            //    return targetString.ToString();
            //}
            return GetFinalCustomizeMessage(psc, stubType, cultureInfo);

        }
        public static string CustomizeLinkStubDynamicValues(string OriginalString, ProcessStubfileContent psc, CultureInfo cultureInfo)
        {
            StringBuilder targetString = new StringBuilder();
            targetString.AppendLine("[InternetShortcut]");
            targetString.AppendLine(string.Format("URL={0}", psc.ReCenterLink));
            return targetString.ToString();
        }
        /*private static string AppendDynamicValues(ProcessStubfileContent psc, CultureInfo cultureInfo, LeaveStubType stubType)
        {
            StringBuilder tempString = new StringBuilder();
            StringBuilder tempStringInside = new StringBuilder();
            //StringBuilder tempStringRight = new StringBuilder();
            //< div style = "text-align:left; width:700px; margin:0 auto;" >< b > Policy / Rule Name:</ b > leavestub </ div >
            //string tableLabel = "<table style=\"border-collapse:collapse;border:0.5pxsolid black;margin:auto\">";
            //string tableLabelEnd = "</table>";
            string brLabel = "<br>";
            string divLabel = "<div>";
            string divLabelWithStyle = "<div style = \"text-align:left; width:740px; margin:0 auto;white-space:normal; word-break:break-all;overflow:hidden;\">";
            string divLabelEnd = "</div>";
            string bLabel = "<b>";
            string bLabelEnd = "</b>";
            string space = " ";
            //string thLable = "<th style=\"border: 0.5px solid black; \">";
            //string thLableEnd = "</th>";
            //string thLableRight = "<th style=\"border: 0.5px solid black;font-weight:lighter; \">";
            //string trLable = "<tr style=\"border: 0.5px solid black; \">";
            //string trLableEnd = "</tr>";
            string fileName = string.Format($"{divLabel}{bLabel}{ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyFileName", cultureInfo)}{space}{bLabelEnd}");
            string fullPathToFile = string.Format($"{divLabel}{bLabel}{ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyFullPathToFile", cultureInfo)}{space}{bLabelEnd}");
            string ruleName = string.Format($"{divLabel}{bLabel}{ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRuleName", cultureInfo)}{space}{bLabelEnd}");
            string dateOfArchival = string.Format($"{divLabel}{bLabel}{ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyDateOfArchival", cultureInfo)}{space}{bLabelEnd}");
            string reCenterLink = string.Format($"{divLabel}{bLabel}{ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLink", cultureInfo)}{space}{bLabelEnd}");
            if (psc.FileNameSelected)
            {
                tempStringInside.Append(fileName);
                tempStringInside.Append(psc.FileName);
                tempStringInside.Append(divLabelEnd);
            }
            if (psc.FullPathToFileSelected)
            {
                tempStringInside.Append(fullPathToFile);
                tempStringInside.Append(psc.FullPathToFile);
                tempStringInside.Append(divLabelEnd);
            }
            if (psc.RuleNameSelected)
            {
                string tempRuleString = System.Web.HttpUtility.HtmlEncode(psc.RuleName);
                tempStringInside.Append(ruleName);
                tempStringInside.Append(tempRuleString);
                tempStringInside.Append(divLabelEnd);
            }
            if (psc.DateOfArchivalSelected)
            {
                tempStringInside.Append(dateOfArchival);
                tempStringInside.Append(psc.DateOfArchival + "(UTC)");
                tempStringInside.Append(divLabelEnd);
            }
            if (psc.ReCenterLinkSelected)
            {
                tempStringInside.Append(reCenterLink);
                if (stubType == LeaveStubType.Aspx)
                {
                    tempStringInside.Append(string.Format("<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>", psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkContent", cultureInfo)));
                }
                else if (stubType == LeaveStubType.Html)
                {
                    tempStringInside.Append(string.Format("<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>", psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkHtmlContent", cultureInfo)));
                }
                else
                {
                    tempStringInside.Append(string.Format("<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>", psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkHtmlContent", cultureInfo)));
                }
                tempStringInside.Append(divLabelEnd);
            }
            string temp = divLabelWithStyle + brLabel + tempStringInside + divLabelEnd;
            tempString.Append(temp);
            return tempString.ToString();
        }*/

        public static bool IsDeclareLinkFile(ScheduleConfiguration mConfig)
        {
            if (mConfig.currentRule != null && (mConfig.currentRule.DeclareLinkFile || mConfig.currentRule.DeclareStubOption == DeclareStubType.Declare || mConfig.currentRule.DeclareStubOption == DeclareStubType.None || mConfig.currentRule.DeclareStubOption == DeclareStubType.AddRecordLabel))
            {
                return true;
            }
            return false;
        }

        private static string GetFinalCustomizeMessage(ProcessStubfileContent psc, LeaveStubType leaveStubType, CultureInfo cultureInfo)
        {
            string tempString = psc.MessageContent;
            string tempStringInside = "<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>";
            if (!string.IsNullOrEmpty(psc.MessageContent))
            {
                if (psc.FileNameSelected)
                {
                    tempString = tempString.Replace(RMConstants.STUBFILENAMEMAPPING, psc.FileName);
                }
                if (psc.ReCenterLinkSelected)
                {
                    if (leaveStubType == LeaveStubType.Aspx)
                    {
                        string tempStringRestoreLink = string.Format(tempStringInside, psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkContent", cultureInfo));
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, tempStringRestoreLink);

                    }
                    else if (leaveStubType == LeaveStubType.Html)
                    {
                        string tempStringRestoreLink =string.Format(tempStringInside, psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkHtmlContent", cultureInfo));
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, tempStringRestoreLink);

                    }
                    else
                    {
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, psc.ReCenterLink+" ");
                    }
                }
                if (psc.FullPathToFileSelected)
                {
                    tempString = tempString.Replace(RMConstants.STUBFILEPATHMAPPING, psc.FullPathToFile);
                }
                if (psc.DateOfArchivalSelected)
                {
                    tempString = tempString.Replace(RMConstants.STUBARCHIVEDTIMEMAPPING, psc.DateOfArchival);
                }
                if (psc.RuleNameSelected)
                {
                    tempString = tempString.Replace(RMConstants.STUBRULENAMEMAPPING, psc.RuleName);
                }
                if (psc.ExternalLinkSelected)
                {
                    tempString = ReplaceExternalLinkTag(psc.ExternalLinks, tempString, tempStringInside, leaveStubType);
                }
            }
            return tempString;
        }
        private static string ReplaceExternalLinkTag(List<string> linkTags,string tempString,string hrefString,LeaveStubType type)
        {
            string result = tempString;
            foreach (var tag in linkTags)
            {
                var temp = tag.Replace("[","").Replace("]","").Split('|');
                if (temp.Length > 0)
                {
                    string linkString = string.Empty;
                    if (type == LeaveStubType.Txt)
                    {
                        linkString = temp[0] + '|' + temp[1];
                    }
                    else
                    {
                        linkString = string.Format(hrefString, temp[1], temp[0]);
                    }
                    result = result.Replace(tag,linkString);
                }
                else
                {
                    mLog.Warn($"current tag not contain |,please check stub template,tag:{tag}");
                }
            }
            return result;
        }
        public static Task<ProcessStubfileContent> SetStubContentValue(IAveFile aveFile, ScheduleConfiguration mConfig)
        {
            return SetStubContentValueAsync(aveFile, mConfig, string.Empty);
        }



        public static async Task<ProcessStubfileContent> SetStubContentValueAsync(IAveFile aveFile, ScheduleConfiguration mConfig, string md5,string stubId = null,string fileServerRelatedUrl = "")
        {
            var stubSetting = WrapperConfiguration.IsAOSPLeaveStub? GenerateStubSettingForAOSP(mConfig.currentRule.AOSPStubContent, mConfig.currentRule.AOSPStubType) : await LinkFileCommon.GetStubTemplatesByIdAsync(mConfig.currentRule.StubTemplateId);
            if (stubSetting == null)
            {
                throw new Exception($"Cannot find the stub template by {mConfig.currentRule.StubTemplateId}");
            }
            ProcessStubfileContent psc = new ProcessStubfileContent(stubSetting.StubContent, (LeaveStubType)stubSetting.StubType);
            if (psc.FileNameSelected)
            {
                psc.SetValue(StubDynamicValueType.FileName, aveFile.Name);
            }
            if (psc.FullPathToFileSelected)
            {
                psc.SetValue(StubDynamicValueType.Url, ArchiverCommonStaticMethod.MakeFullUrl(aveFile.ParentFolder.ParentWeb.Site.Url, string.IsNullOrEmpty(fileServerRelatedUrl)?aveFile.ServerRelativeUrl: fileServerRelatedUrl));
            }
            if (psc.RuleNameSelected)
            {
                psc.SetValue(StubDynamicValueType.RuleName, mConfig.currentRule.Name);
            }
            if (psc.DateOfArchivalSelected)
            {
                IAveTimeZone timeZone = aveFile.Web.RegionalSettings.TimeZone;
                DateTime dateTime = timeZone.UTCToLocalTime(DateTime.UtcNow);
                string dateTimeStr = aveFile.Web.RegionalSettings.Time24 ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : dateTime.ToString("yyyy-MM-dd hh:mm:ss tt");
                psc.SetValue(StubDynamicValueType.ArchivalDate, dateTimeStr.ToString() + timeZone.Description);
            }
            if (psc.ReCenterLinkSelected)
            {
                if (!string.IsNullOrEmpty(md5))
                {
                    psc.StubId = stubId;
                    psc.SetValue(StubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(aveFile, mConfig, md5, stubId));
                }
                else
                {
                    psc.SetValue(StubDynamicValueType.ReCenterLink, string.Empty);
                }
            }

            return psc;
        }
        private static StubSettingDto GenerateStubSettingForAOSP(string settingString,int stubType)
        {
            return new StubSettingDto()
            {
                StubContent = settingString,
                StubType = stubType,
            };
        }
        public static async Task<ProcessStubfileContent> SetStubContentValueAsync(ArchiverBasicIndex archiverFileIndex, IAveFile aveFile, ScheduleConfiguration mConfig, string stubId = null)
        {
            var stubSetting = WrapperConfiguration.IsAOSPLeaveStub ? GenerateStubSettingForAOSP(mConfig.currentRule.AOSPStubContent, mConfig.currentRule.AOSPStubType) :await LinkFileCommon.GetStubTemplatesByIdAsync(mConfig.currentRule.StubTemplateId);
            if (stubSetting == null)
            {
                throw new Exception($"Cannot find the stub template by {mConfig.currentRule.StubTemplateId}");
            }

            var realBackupFileName = archiverFileIndex.Name;
            if (mConfig.isConvertSameTypeStub)
            {
                realBackupFileName = archiverFileIndex.Name.StartsWith(mConfig.JobId + "_") ? archiverFileIndex.Name.Replace(mConfig.JobId + "_", "") : archiverFileIndex.Name;
            }

            ProcessStubfileContent psc = new ProcessStubfileContent(stubSetting.StubContent, (LeaveStubType)stubSetting.StubType);

            if (psc.FileNameSelected)
            {
                psc.SetValue(StubDynamicValueType.FileName, realBackupFileName);
            }
            if (psc.FullPathToFileSelected)
            {
                psc.SetValue(StubDynamicValueType.Url, archiverFileIndex.Url);
            }
            if (psc.RuleNameSelected)
            {
                psc.SetValue(StubDynamicValueType.RuleName, mConfig.RuleNameByJobIdDic[archiverFileIndex.JobId]);
            }
            if (psc.DateOfArchivalSelected)
            {
                IAveTimeZone timeZone = aveFile.Web.RegionalSettings.TimeZone;
                DateTime dateTime = timeZone.UTCToLocalTime(new DateTime(archiverFileIndex.ArchiveTime));
                string dateTimeStr = aveFile.Web.RegionalSettings.Time24 ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : dateTime.ToString("yyyy-MM-dd hh:mm:ss tt");
                psc.SetValue(StubDynamicValueType.ArchivalDate, dateTimeStr.ToString() + timeZone.Description);
            }
            if (psc.ReCenterLinkSelected)
            {
                if (!string.IsNullOrEmpty(archiverFileIndex.PathMD5))
                {
                    if (mConfig.IsConvertStubJob && string.IsNullOrEmpty(stubId))
                    {
                        stubId = string.Concat(Guid.NewGuid().ToString().Replace("-", ""), DateTime.UtcNow.Ticks.ToString());
                        mLog.Debug($"File {aveFile.Item.Url} convert from no restore link to have restore link, create new stubId: {stubId}");
                        if (mConfig.StubCache.TryGetValue(aveFile.UniqueId.ToString(), out var result))
                        {
                            result.StubRealId = stubId;
                        }
                    }
                    psc.StubId = stubId;
                    psc.SetValue(StubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(archiverFileIndex, aveFile, mConfig, archiverFileIndex.PathMD5, psc.StubId));
                }
                else
                {
                    psc.SetValue(StubDynamicValueType.ReCenterLink, string.Empty);
                }
            }

            return psc;
        }

        public static async Task<bool> CheckHasRestoreLinkSettings(Rule rule)
        {
            bool hasRestoreLink = false;
            StubSettingParaDto stubSettingParaDto = null;
            if (!string.IsNullOrEmpty(rule.StubTemplateId))
            {
                var stubSetting = await LinkFileCommon.GetStubTemplatesByIdAsync(rule.StubTemplateId);
                if (stubSetting != null)
                {
                    stubSettingParaDto = new StubSettingParaDto()
                    {
                        StubType = stubSetting.StubType,
                        StubContent = stubSetting.StubContent,
                        IsDeclareStubAsRecords = stubSetting.IsDeclareStubAsRecords,
                    };
                }
            }
            if (stubSettingParaDto == null)
            {
                throw new Exception($"Cannot find the stub template by {rule.StubTemplateId}");
            }
            ProcessStubfileContent psc = new ProcessStubfileContent(stubSettingParaDto.StubContent, rule.LeaveStubType);
            if (psc.ReCenterLinkSelected)
            {
                hasRestoreLink = true;
            }

            return hasRestoreLink;
        }

        public static string GetReCenterRestoreLink(IAveFile file, ScheduleConfiguration mConfig, string md5, string stubId)
        {
            string userId = string.Empty;
            if (mConfig.IsOneDriverSite)
            {
                try
                {
                    userId = ArchiverCommonStaticMethod.GetADUserID(file.Web.Site.Owner.Email, mConfig.aveObjectModelFactory.AccountInfo);
                }
                catch (Exception e)
                {
                    mLog.Debug($"get user failed,error:{e}");
                }
            }
            StubLinkDetails stubLinkDetails = new StubLinkDetails(mConfig.aveObjectModelFactory.AccountInfo.TenantId, file.Web.Site.Url, file.ServerRelativeUrl, md5, mConfig.CurrentIndexJobID, userId, mConfig.currentRule.LeaveStubType);
            stubLinkDetails.StubId = stubId;
            stubLinkDetails.StubProductSource = WrapperConfiguration.IsAOSPLeaveStub? StubProductSource.AOSP: StubProductSource.Opus;
            mLog.Debug($"the stub of file size is:{file.Length}");
            stubLinkDetails.FileSize = file.Length.ToString();
            var reCenterUrl = ArchiverCommonStaticMethod.GetReCenterHost(mConfig.TenantGroupId);
            if (string.IsNullOrEmpty(reCenterUrl))
            {
                throw new RALeaveStubException("Can not get ReCenter host.");
            }
            return string.Format($"{reCenterUrl.TrimEnd('/')}/?Id=({stubId})&archiver={new StubLinkProcessor(mConfig).ConvertToString(stubLinkDetails)}");
        }

        public static string GetReCenterRestoreLink(ArchiverBasicIndex archiverFileIndex, IAveFile file, ScheduleConfiguration mConfig, string md5, string stubId)
        {
            string userId = string.Empty;
            if (mConfig.IsOneDriverSite)
            {
                try
                {
                    userId = ArchiverCommonStaticMethod.GetADUserID(file.Web.Site.Owner.Email, mConfig.aveObjectModelFactory.AccountInfo);
                }
                catch (Exception e)
                {
                    mLog.Debug($"get user failed,error:{e}");
                }
            }

            var serverRelativeUrl = AveUrlUtility.GetServerRelativeUrl(archiverFileIndex.Url);
            var siteUrl = AveUrlUtility.GetSiteUrl(archiverFileIndex.Url);
            mLog.Debug($"build stub link for file: {archiverFileIndex.NodeGuid}, size:{archiverFileIndex.ContentLength}, stub id: {stubId}");

            StubLinkDetails stubLinkDetails = new StubLinkDetails(mConfig.aveObjectModelFactory.AccountInfo.TenantId, siteUrl, serverRelativeUrl, md5, archiverFileIndex.JobId, userId, mConfig.currentRule.LeaveStubType);
            stubLinkDetails.StubId = stubId;
            stubLinkDetails.StubProductSource = WrapperConfiguration.IsAOSPLeaveStub ? StubProductSource.AOSP : StubProductSource.Opus;
            stubLinkDetails.FileSize = archiverFileIndex.ContentLength.ToString();
            var reCenterUrl = ArchiverCommonStaticMethod.GetReCenterHost(mConfig.TenantGroupId);
            if (string.IsNullOrEmpty(reCenterUrl))
            {
                throw new RALeaveStubException("Can not get ReCenter host.");
            }
            return string.Format($"{reCenterUrl.TrimEnd('/')}/?Id=({stubId})&archiver={new StubLinkProcessor(mConfig).ConvertToString(stubLinkDetails)}");
        }

        public static bool IsRestoreLink(ScheduleConfiguration mConfig)
        {
            if (mConfig.IsILMode)
            {
                if (mConfig.IsOneDriverSite)
                {
                    if (mConfig.currentRule != null && mConfig.currentRule.OneDriveRule != null)
                    {
                        return mConfig.currentRule.OneDriveRule.IsRestoreLink;
                    }
                }
                else if(mConfig.IsTeams && mConfig.jobtype != Contract.JobMonitor.JobType.TeamsRecordsDisposal)
                {
                    if (mConfig.currentRule != null && mConfig.currentRule.TeamsRule != null)
                    {
                        return mConfig.currentRule.TeamsRule.IsRestoreLink;
                    }
                }
                else
                {
                    if (mConfig.currentRule != null)
                    {
                        return mConfig.currentRule.IsRestoreLink;
                    }
                }
                return false;
            }
            else
            {
                return mConfig.currentRule.IsRestoreLink;
            }
        }

        public static bool IsRecordRestoreLink(ScheduleConfiguration mConfig)
        {
            if (mConfig.IsOneDriverSite)
            {
                if (mConfig.currentRule != null && mConfig.currentRule.OneDriveRule != null)
                {
                    return mConfig.currentRule.OneDriveRule.IsRestoreLink;
                }
            }
            else if (mConfig.IsTeams && mConfig.jobtype != Contract.JobMonitor.JobType.TeamsRecordsDisposal)
            {
                if (mConfig.currentRule != null && mConfig.currentRule.TeamsRule != null)
                {
                    return mConfig.currentRule.TeamsRule.IsRestoreLink;
                }
            }
            else
            {
                if (mConfig.currentRule != null)
                {
                    return mConfig.currentRule.IsRestoreLink;
                }
            }
            return false;
        }

        public static string GetDocumnetPathMD5(string sitePath, string folderPath, string fileName)
        {
            string parentPath = string.Empty;
            if (sitePath.Equals(folderPath, StringComparison.OrdinalIgnoreCase))
            {
                parentPath = sitePath;
            }
            else
            {
                parentPath = sitePath + "\\" + folderPath;
            }
            string fullPath = parentPath + "\\" + fileName;
            var pathmd5 = HashCodeHelper.ToMD5HashCode(fullPath);
            mLog.Info($"Path MD5 is {pathmd5}");
            return pathmd5;
        }

        public static async Task<StubSettingDto> GetStubTemplatesByIdAsync(string StubTemplateId)
        {
            if (string.IsNullOrEmpty(StubTemplateId))
            {
                mLog.Error($"Stub template id is null");
                return null;
            }
            if (StubTemplates.ContainsKey(StubTemplateId))
            {
                return StubTemplates[StubTemplateId];
            }
            else
            {
                var stubSettings = await StubSettingService.GetStubTemplateByIdAsync(StubTemplateId);
                if (stubSettings == null)
                {
                    mLog.Error($"Cannot find stub template by id {StubTemplateId}");
                    return null;
                }
                else
                {
                    StubTemplates[StubTemplateId] = stubSettings;
                    return stubSettings;
                }
            }
        }


    }

    public class StubBasicInfo
    {
        [JsonProperty("sn")]
        public int StubNumber { get; set; }
        [JsonProperty("ci")]
        public string ContainerId { get; set; }
        [JsonProperty("si")]
        public string StubId { get; set; }
        [JsonProperty("st")]
        public string StubTemplateId { get; set; }
        [JsonProperty("lc")]
        public int LCID { get; set; }
        [JsonProperty("fn")]
        public string FileName { get; set; }
        [JsonProperty("fp")]
        public string FullPathToFile { get; set; }
        [JsonProperty("rn")]
        public string RuleName { get; set; }
        [JsonProperty("ad")]
        public string DateOfArchival { get; set; }
        [JsonProperty("rl")]
        public string ReCenterLink { get; set; }

        public string ToJsonString()
        {
            return SerializerHelper.SerializeByJsonConvert(this);
        }
        public async Task<ProcessStubfileContent> ToProcessStubFileContentAsync()
        {
            var templateDto = WrapperConfiguration.IsAOSPLeaveStub? WrapperConfiguration.AOSPStubSettingDto : await LinkFileCommon.GetStubTemplatesByIdAsync(this.StubTemplateId);
            if (templateDto == null)
            {
                throw new Exception($"Cannot find the stub template by {this.StubTemplateId}");
            }
            return new ProcessStubfileContent(this, templateDto.StubContent, (LeaveStubType)templateDto.StubType);
        }
        public static StubBasicInfo FromJsonString(string jsonString)
        {
            return SerializerHelper.DeserializeByJsonConvert<StubBasicInfo>(jsonString);
        }
    }

    public class ProcessStubfileContent
    {
        public LeaveStubType StubType { get; private set; }
        public string FileName { get; private set; }
        public string FullPathToFile { get; private set; }
        public string RuleName { get; private set; }
        public string DateOfArchival { get; private set; }
        public string ReCenterLink { get; private set; }
        public string MessageContent { get; private set; }
        public List<string> ExternalLinks { get; private set; }

        public bool FileNameSelected { get; private set; }
        public bool FullPathToFileSelected { get; private set; }
        public bool RuleNameSelected { get; private set; }
        public bool DateOfArchivalSelected { get; private set; }
        public bool ReCenterLinkSelected { get; private set; }
        public bool ExternalLinkSelected { get; private set; }
        public string StubId { get; set; }
        //txt stub file GetValue

        public ProcessStubfileContent(string message, LeaveStubType leaveStubType)
        {
            MessageContent = message;
            this.StubType = leaveStubType;
            if (leaveStubType == LeaveStubType.Link)
            {
                ReCenterLinkSelected = true;
            }
            else
            {
                if (CheckHasStubTags(MessageContent, RMConstants.STUBFILENAMEMAPPING) == StubCustomizeTag.FileName)
                {
                    FileNameSelected = true;
                }
                if (CheckHasStubTags(MessageContent, RMConstants.STUBFILEPATHMAPPING) == StubCustomizeTag.FilePath)
                {
                    FullPathToFileSelected = true;
                }
                if (CheckHasStubTags(MessageContent, RMConstants.STUBRULENAMEMAPPING) == StubCustomizeTag.Rulename)
                {
                    RuleNameSelected = true;
                }
                if (CheckHasStubTags(MessageContent, RMConstants.STUBARCHIVEDTIMEMAPPING) == StubCustomizeTag.Archivedtime)
                {
                    DateOfArchivalSelected = true;
                }
                if (CheckHasStubTags(MessageContent, RMConstants.STUBRESTORELINKMAPPING) == StubCustomizeTag.RestoreLink)
                {
                    ReCenterLinkSelected = true;
                }
                if (CheckHasStubTags(MessageContent, RMConstants.STUBEXTERNALLINKMAPPING) == StubCustomizeTag.ExternalLink)
                {
                    ExternalLinkSelected = true;
                    this.ExternalLinks = GetExternalLinksFromTemplate(message);
                }
            }
        }

        public ProcessStubfileContent(StubBasicInfo stubBasicInfo, string message, LeaveStubType leaveStubType)
            : this(message, leaveStubType)
        {
            StubId = stubBasicInfo.StubId;
            FileName = stubBasicInfo.FileName;
            FullPathToFile = stubBasicInfo.FullPathToFile;
            RuleName = stubBasicInfo.RuleName;
            DateOfArchival = stubBasicInfo.DateOfArchival;
            ReCenterLink = stubBasicInfo.ReCenterLink;
            MessageContent = message;
        }

        public StubBasicInfo GetStubBasicInfo(string stubTemplateId, int cultureLCID, int stubFileNum, string containerId)
        {
            return new StubBasicInfo()
            {
                LCID = cultureLCID,
                StubNumber = stubFileNum,
                ContainerId = containerId,
                StubId = this.StubId,
                StubTemplateId = stubTemplateId,
                FileName = this.FileName,
                FullPathToFile = this.FullPathToFile,
                RuleName = this.RuleName,
                DateOfArchival = this.DateOfArchival,
                ReCenterLink = this.ReCenterLink,
            };
        }

        public void SetValue(StubDynamicValueType type, string value)
        {
            switch (type)
            {
                case StubDynamicValueType.FileName:
                    {
                        this.FileName = value;
                        break;
                    }
                case StubDynamicValueType.Url:
                    {
                        this.FullPathToFile = value;
                        break;
                    }
                case StubDynamicValueType.RuleName:
                    {
                        this.RuleName = value;
                        break;
                    }
                case StubDynamicValueType.ArchivalDate:
                    {
                        this.DateOfArchival = value;
                        break;
                    }
                case StubDynamicValueType.ReCenterLink:
                    {
                        this.ReCenterLink = value;
                        break;
                    }
            }
        }

        private StubCustomizeTag CheckHasStubTags(string stubContent, string stubMapping)
        {
            StubCustomizeTag result = StubCustomizeTag.None;
            if (stubMapping == RMConstants.STUBFILENAMEMAPPING)
            {
                result = StubCustomizeTag.FileName;
            }
            if (stubMapping == RMConstants.STUBFILEPATHMAPPING)
            {
                result = StubCustomizeTag.FilePath;
            }
            if (stubMapping == RMConstants.STUBARCHIVEDTIMEMAPPING)
            {
                result = StubCustomizeTag.Archivedtime;
            }
            if (stubMapping == RMConstants.STUBRULENAMEMAPPING)
            {
                result = StubCustomizeTag.Rulename;
            }
            if (stubMapping == RMConstants.STUBRESTORELINKMAPPING)
            {
                result = StubCustomizeTag.RestoreLink;
            }
            if (stubMapping == RMConstants.STUBEXTERNALLINKMAPPING)
            {
                result = StubCustomizeTag.ExternalLink;
            }
            if (stubContent.Contains(stubMapping))
            {
                return result;
            }
            return StubCustomizeTag.None;
        }

        private string _ExternalLinkPattern = @"\[[^\]]+\]";
        private List<string> GetExternalLinksFromTemplate(string stubContent)
        {
            List<string> result = new List<string>();
            MatchCollection matches = Regex.Matches(stubContent, _ExternalLinkPattern);
            foreach (Match match in matches)
            {
                string link = match.Groups[0].Value;
                if (!string.IsNullOrEmpty(link) && link.Contains('|'))
                {
                    result.Add(link);
                }
            }
            return result;
        }
    }

    public enum StubDynamicValueType
    {
        FileName,
        Url,
        RuleName,
        ArchivalDate,
        ReCenterLink,
        ExternalLink,
    }

    public enum ArchiverLinkFileType
    {
        ArchiveStubLinkFile = 1,
        DocAveArchiverMoveToLinkFile = 2,
    }

    public class StubLinkProcessor
    {
        private ScheduleConfiguration Config = null;

        public StubLinkProcessor(ScheduleConfiguration mConfig)
        {
            Config = mConfig;
        }

        public string ConvertToString(StubLinkDetails linkDetails)
        {
            string returnStr;
            var masterKey = LinkFileCommon.GetEndUserStubLinkMasterKey(Config);
            var linkBytes = Encoding.UTF8.GetBytes(linkDetails.ToString());

            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                {
                    gs.Write(linkBytes, 0, linkBytes.Length);
                }
                byte[] temp = new byte[mso.Length];
                mso.Position = 0;
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = mso.Read(temp, 0, temp.Length)) > 0)
                    {
                        ms.Write(temp, 0, read);
                    }
                    //var encryptBytes = AuthenticatedEncryption.Encrypt(masterKey, ms.ToArray()); // [RECO-20916]: Fortify scan issue, Privacy Violation: Heap Inspection
                    returnStr = System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(AuthenticatedEncryption.Encrypt(masterKey, ms.ToArray())));
                }
                //returnStr = Encoding.UTF8.GetString(temp);
            }

            return returnStr;
        }

        public StubLinkDetails ConvertToLinkDetails(string str)
        {
            byte[] inPut = Convert.FromBase64String(System.Web.HttpUtility.UrlDecode(str));
            var decrypBytes = AuthenticatedEncryption.Decrypt(LinkFileCommon.GetEndUserStubLinkMasterKey(Config), inPut);
            byte[] decompressedData = new byte[0];
            byte[] temp = new byte[4096];
            using (var mso = new MemoryStream(decrypBytes))
            {
                using (var gs = new GZipStream(mso, CompressionMode.Decompress))
                {
                    int readLen;
                    while ((readLen = gs.Read(temp, 0, 4096)) != 0)
                    {
                        AppendBytes(ref decompressedData, temp, 0, readLen);
                    }
                }
            }

            string decrypString = Encoding.UTF8.GetString(decompressedData);
            var ld = new StubLinkDetails(decrypString);
            return ld;
        }

        private void AppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }
    }
    public class StubLinkDetails
    {
        private readonly string SplitChar = "|#|";
        public string TenantID { get; }
        public string SiteUrl { get; }
        public string FileServerRelativeUrl { get; }
        public string PathMD5 { get; }
        public string JobID { get; }
        public string User { get; }
        public string StubType { get; }
        public string StubId { get; set; }
        public string FileSize { get; set; }
        public StubProductSource StubProductSource { get; set; }

        public StubLinkDetails(string tenantID, string siteUrl, string fileServerRelativeUrl, string pathMD5, string jobID, string user, LeaveStubType stubType)
        {
            this.TenantID = tenantID;
            this.SiteUrl = siteUrl;
            this.FileServerRelativeUrl = fileServerRelativeUrl;
            this.PathMD5 = pathMD5;
            this.JobID = jobID;
            this.User = user;
            this.StubType = stubType.ToString();
        }

        public StubLinkDetails(string hybridString)
        {
            var array = hybridString.Split(new string[] { SplitChar }, StringSplitOptions.None);
            if (array.Length == 6)
            {
                this.TenantID = array[0];
                this.SiteUrl = array[1];
                this.FileServerRelativeUrl = array[2];
                this.PathMD5 = array[3];
                this.JobID = array[4];
                this.User = array[5];
            }
            else if (array.Length == 8)
            {
                this.TenantID = array[0];
                this.SiteUrl = array[1];
                this.FileServerRelativeUrl = array[2];
                this.PathMD5 = array[3];
                this.JobID = array[4];
                this.User = array[5];
                this.StubType = array[6];
                this.StubId = array[7];
                this.FileSize = array[8];
            }
            else
            {
                throw new Exception("Invalid data format.");
            }
        }

        public override string ToString()
        {
            return $"{TenantID}{SplitChar}{SiteUrl}{SplitChar}{FileServerRelativeUrl}{SplitChar}{PathMD5}{SplitChar}{JobID}{SplitChar}{User}{SplitChar}{StubType}{SplitChar}{StubId}{SplitChar}{FileSize}{SplitChar}{(int)StubProductSource}";
        }
    }
}
