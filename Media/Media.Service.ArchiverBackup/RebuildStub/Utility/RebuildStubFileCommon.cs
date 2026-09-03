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
using Cloud.Sdk.Data.Dao;
using LeaveStubType = AvePoint.GCommon.Contract.StorageOptimization.Object.LeaveStubType;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public static class RebuildStubFileCommon
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(RebuildStubFileCommon));
        private readonly static object readFileLock = new object();

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

        public static byte[] GetFileContent(LeaveStubType leaveStubType, CultureInfo cultureInfo, ProcessRebuildStubfileContent psc)//string contentTypeId, string urlPath, string siteUrl)
        {
            //mConfig.currentRule.LeaveStubMessage = "xxxx";

            if (leaveStubType == LeaveStubType.Aspx)
            {
                return Encoding.UTF8.GetBytes(StubPageStyleContent.Replace(StubReplaceFlag, CustomizeStubDynamicValues(psc.MessageContent, psc, cultureInfo)));
            }
            else if (leaveStubType == LeaveStubType.Html)
            {
                return Encoding.UTF8.GetBytes(HtmlStubPageStyleContent.Replace(StubReplaceFlag, CustomizeHtmlStubDynamicValues(psc.MessageContent, psc, cultureInfo)));
            }
            else if (leaveStubType == LeaveStubType.Link)
            {
                return Encoding.UTF8.GetBytes(CustomizeLinkStubDynamicValues(psc.MessageContent, psc, cultureInfo));
            }
            //LeaveStubType txt.
            else
            {
                return Encoding.UTF8.GetBytes(GetFinalCustomizeMessage(psc, leaveStubType, cultureInfo));
            }
        }

        public static string GetStubFileNameSuffix(LeaveStubType leaveStubType)
        {
            string mSuffix = string.Empty;

            if (leaveStubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIX;
            }
            else if (leaveStubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIX;
            }
            else if (leaveStubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIX;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIX;
            }

            return mSuffix;
        }

        public static string GetStubFileNameSuffixWithDot(LeaveStubType leaveStubType)
        {
            string mSuffix = string.Empty;

            if (leaveStubType == LeaveStubType.Aspx)
            {
                mSuffix = ASPXSTUBSUFFIXWITHDOT;
            }
            else if (leaveStubType == LeaveStubType.Html)
            {
                mSuffix = HTMLSTUBSUFFIXWITHDOT;
            }
            else if (leaveStubType == LeaveStubType.Link)
            {
                mSuffix = LINKSTUBSUFFIXWITHDOT;
            }
            else
            {
                mSuffix = TXTSTUBSUFFIXWITHDOT;
            }

            return mSuffix;
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

        public static byte[] GetEndUserStubLinkMasterKey(string tenantGroupId)
        {
            if (tenantGroupId == mTenantGroupId && mEndUserStubLinkMasterKey != null)
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
                    mTenantGroupId = tenantGroupId;
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
        public static string CustomizeStubDynamicValues(string OriginalString, ProcessRebuildStubfileContent psc, CultureInfo cultureInfo)
        {
            LeaveStubType stubType = LeaveStubType.Aspx;
            return GetFinalCustomizeMessage(psc, stubType, cultureInfo);

        }
        public static string CustomizeHtmlStubDynamicValues(string OriginalString, ProcessRebuildStubfileContent psc, CultureInfo cultureInfo)
        {
            LeaveStubType stubType = LeaveStubType.Html;
            return GetFinalCustomizeMessage(psc, stubType, cultureInfo);

        }
        public static string CustomizeLinkStubDynamicValues(string OriginalString, ProcessRebuildStubfileContent psc, CultureInfo cultureInfo)
        {
            StringBuilder targetString = new StringBuilder();
            targetString.AppendLine("[InternetShortcut]");
            targetString.AppendLine(string.Format("URL={0}", psc.ReCenterLink));
            return targetString.ToString();
        }

        private static string GetFinalCustomizeMessage(ProcessRebuildStubfileContent psc, LeaveStubType leaveStubType, CultureInfo cultureInfo)
        {
            string tempString = psc.MessageContent;
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
                        var tempStringInside = string.Format("<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>", psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkContent", cultureInfo));
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, tempStringInside);

                    }
                    else if (leaveStubType == LeaveStubType.Html)
                    {
                        var tempStringInside = string.Format("<a href=\"{0}\"><u style=\"word-break: keep-all;\">{1}</u></a>", psc.ReCenterLink, ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARStubMessageKeyRecenterLinkHtmlContent", cultureInfo));
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, tempStringInside);
                    }
                    else
                    {
                        tempString = tempString.Replace(RMConstants.STUBRESTORELINKMAPPING, psc.ReCenterLink);
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
            }
            return tempString;
        }

        //public static Task<ProcessRebuildStubfileContent> SetStubContentValue(IAveFile aveFile, string stubTemplateId, string md5, string tenantId, string tenantGroupId, bool isOneDriverSite, LeaveStubType leaveStubType, string rebuildJobId, AveBPOSAccountInfo accountInfo)
        //{
        //    return SetStubContentValueAsync(aveFile, stubTemplateId, md5, tenantId, tenantGroupId, isOneDriverSite, leaveStubType, rebuildJobId, accountInfo);
        //}
        public static async Task<ProcessRebuildStubfileContent> SetStubContentValueAsync(IAveFile stubFile, string fileName, string fileUrl, StubSettingDto stubSettingDto, string md5, string tenantId, string tenantGroupId, bool isOneDriverSite, LeaveStubType leaveStubType, string rebuildJobId, AveBPOSAccountInfo accountInfo, long archiveTime)
        {
            StubSettingParaDto stubSettingParaDto = new StubSettingParaDto()
                {
                    StubType = stubSettingDto.StubType,
                    StubContent = stubSettingDto.StubContent,
                    IsDeclareStubAsRecords = stubSettingDto.IsDeclareStubAsRecords,
                };
            ProcessRebuildStubfileContent psc = new ProcessRebuildStubfileContent(stubSettingParaDto.StubContent, leaveStubType);
            if (psc.FileNameSelected)
            {
                psc.SetValue(RebuildStubDynamicValueType.FileName, fileName, true);
            }
            if (psc.FullPathToFileSelected)
            {
                psc.SetValue(RebuildStubDynamicValueType.Url, fileUrl, true);
            }
            if (psc.RuleNameSelected)
            {
                //psc.SetValue(StubDynamicValueType.RuleName, mConfig.currentRule.Name, true);
            }
            if (psc.DateOfArchivalSelected)
            {
                IAveTimeZone timeZone = stubFile.Web.RegionalSettings.TimeZone;
                DateTime dateTime = timeZone.UTCToLocalTime(new DateTime(archiveTime));
                string dateTimeStr = stubFile.Web.RegionalSettings.Time24 ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : dateTime.ToString("yyyy-MM-dd hh:mm:ss tt");
                psc.SetValue(RebuildStubDynamicValueType.ArchivalDate, dateTimeStr + timeZone.Description, true);
            }
            if (psc.ReCenterLinkSelected)
            {
                if (!string.IsNullOrEmpty(md5))
                {
                    psc.SetValue(RebuildStubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(stubFile, tenantId, tenantGroupId, md5, isOneDriverSite, leaveStubType, rebuildJobId, accountInfo), true);
                }
                else
                {
                    psc.SetValue(RebuildStubDynamicValueType.ReCenterLink, string.Empty, true);
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
                var stubSetting = await RebuildStubFileCommon.GetStubTemplatesByIdAsync(rule.StubTemplateId);
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
            ProcessRebuildStubfileContent psc = new ProcessRebuildStubfileContent(stubSettingParaDto.StubContent, rule.LeaveStubType);
            if (psc.ReCenterLinkSelected)
            {
                hasRestoreLink = true;
            }

            return hasRestoreLink;
        }

        public static string GetReCenterRestoreLink(IAveFile file, string tenantId, string tenantGroupId, string md5, bool isOneDriverSite, LeaveStubType leaveStubType, string rebuildJobId, AveBPOSAccountInfo accountInfo)
        {
            string userId = string.Empty;
            if (isOneDriverSite)
            {
                userId = ArchiverRebuildStubCommonStaticMethod.GetADUserID(file.Web.Site.Owner.Email, accountInfo);
            }
            RebuildStubLinkDetails stubLinkDetails = new RebuildStubLinkDetails(tenantId, file.Web.Site.Url, file.ServerRelativeUrl.Substring(0, file.ServerRelativeUrl.LastIndexOf(".")), md5, rebuildJobId, userId, leaveStubType);
            var reCenterUrl = ArchiverRebuildStubCommonStaticMethod.GetReCenterHost(tenantGroupId);
            if (string.IsNullOrEmpty(reCenterUrl))
            {
                throw new Exception("Can not get ReCenter host.");
            }

            return string.Format($"{reCenterUrl.TrimEnd('/')}/?archiver={new RebuildStubLinkProcessor(tenantGroupId).ConvertToString(stubLinkDetails)}");
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

    public class ProcessRebuildStubfileContent
    {
        private string fileName;
        private string fullPathToFile;
        private string ruleName;
        private string dateOfArchival;
        private string reCenterLink;
        private string messageContent;
        public string FileName { get { return fileName; } }
        public string FullPathToFile { get { return fullPathToFile; } }
        public string RuleName { get { return ruleName; } }
        public string DateOfArchival { get { return dateOfArchival; } }
        public string ReCenterLink { get { return reCenterLink; } }
        public string MessageContent { get { return messageContent; } }

        public bool FileNameSelected;
        public bool FullPathToFileSelected;
        public bool RuleNameSelected;
        public bool DateOfArchivalSelected;
        public bool ReCenterLinkSelected;
        //txt stub file GetValue

        public ProcessRebuildStubfileContent(string message, LeaveStubType leaveStubType)
        {
            messageContent = message;
            if (leaveStubType == LeaveStubType.Link)
            {
                ReCenterLinkSelected = true;
            }
            else
            {
                if (CheckHasStubTags(messageContent, RMConstants.STUBFILENAMEMAPPING) == StubCustomizeTag.FileName)
                {
                    FileNameSelected = true;
                }
                if (CheckHasStubTags(messageContent, RMConstants.STUBFILEPATHMAPPING) == StubCustomizeTag.FilePath)
                {
                    FullPathToFileSelected = true;
                }
                if (CheckHasStubTags(messageContent, RMConstants.STUBRULENAMEMAPPING) == StubCustomizeTag.Rulename)
                {
                    RuleNameSelected = true;
                }
                if (CheckHasStubTags(messageContent, RMConstants.STUBARCHIVEDTIMEMAPPING) == StubCustomizeTag.Archivedtime)
                {
                    DateOfArchivalSelected = true;
                }
                if (CheckHasStubTags(messageContent, RMConstants.STUBRESTORELINKMAPPING) == StubCustomizeTag.RestoreLink)
                {
                    ReCenterLinkSelected = true;
                }
            }
        }

        public void SetValue(RebuildStubDynamicValueType type, string value, bool stubContent)
        {
            switch (type)
            {
                case RebuildStubDynamicValueType.FileName:
                    {
                        if (stubContent)
                        {
                            this.fileName = value;
                            break;
                        }
                        break;
                    }
                case RebuildStubDynamicValueType.Url:
                    {
                        if (stubContent)
                        {
                            this.fullPathToFile = value;
                            break;
                        }
                        break;
                    }
                case RebuildStubDynamicValueType.RuleName:
                    {
                        if (stubContent)
                        {
                            this.ruleName = value;
                            break;
                        }
                        break;
                    }
                case RebuildStubDynamicValueType.ArchivalDate:
                    {
                        if (stubContent)
                        {
                            this.dateOfArchival = value;
                            break;
                        }
                        break;
                    }
                case RebuildStubDynamicValueType.ReCenterLink:
                    {
                        if (stubContent)
                        {
                            this.reCenterLink = value;
                            break;
                        }
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
            if (stubContent.Contains(stubMapping))
            {
                return result;
            }
            return StubCustomizeTag.None;
        }
    }

    public enum RebuildStubDynamicValueType
    {
        FileName,
        Url,
        RuleName,
        ArchivalDate,
        ReCenterLink
    }

    public enum RebuildStubArchiverLinkFileType
    {
        ArchiveStubLinkFile = 1,
        DocAveArchiverMoveToLinkFile = 2,
    }

    public class RebuildStubLinkProcessor
    {
        private string tenantGroupId = string.Empty;

        public RebuildStubLinkProcessor(string tenantGroupId)
        {
            this.tenantGroupId = tenantGroupId;
        }

        public string ConvertToString(RebuildStubLinkDetails linkDetails)
        {
            string returnStr;
            var masterKey = RebuildStubFileCommon.GetEndUserStubLinkMasterKey(tenantGroupId);
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
                    var encryptBytes = AuthenticatedEncryption.Encrypt(masterKey, ms.ToArray());
                    returnStr = System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(encryptBytes));
                }
                //returnStr = Encoding.UTF8.GetString(temp);
            }

            return returnStr;
        }

        public RebuildStubLinkDetails ConvertToLinkDetails(string str)
        {
            byte[] inPut = Convert.FromBase64String(System.Web.HttpUtility.UrlDecode(str));
            var decrypBytes = AuthenticatedEncryption.Decrypt(RebuildStubFileCommon.GetEndUserStubLinkMasterKey(tenantGroupId), inPut);
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
            var ld = new RebuildStubLinkDetails(decrypString);
            return ld;
        }

        private void AppendBytes(ref byte[] source, byte[] additional, int startIndex, int length)
        {
            int oldLen = source.Length;
            Array.Resize<byte>(ref source, source.Length + length);
            Array.Copy(additional, startIndex, source, oldLen, length);
        }
    }
    public class RebuildStubLinkDetails
    {
        private readonly string SplitChar = "|#|";
        public string TenantID { get; }
        public string SiteUrl { get; }
        public string FileServerRelativeUrl { get; }
        public string PathMD5 { get; }
        public string JobID { get; }
        public string User { get; }
        public string StubType { get; }

        public RebuildStubLinkDetails(string tenantID, string siteUrl, string fileServerRelativeUrl, string pathMD5, string jobID, string user, LeaveStubType stubType)
        {
            this.TenantID = tenantID;
            this.SiteUrl = siteUrl;
            this.FileServerRelativeUrl = fileServerRelativeUrl;
            this.PathMD5 = pathMD5;
            this.JobID = jobID;
            this.User = user;
            this.StubType = stubType.ToString();
        }

        public RebuildStubLinkDetails(string hybridString)
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
            else if (array.Length == 7)
            {
                this.TenantID = array[0];
                this.SiteUrl = array[1];
                this.FileServerRelativeUrl = array[2];
                this.PathMD5 = array[3];
                this.JobID = array[4];
                this.User = array[5];
                this.StubType = array[6];
            }
            else
            {
                throw new Exception("Invalid data format.");
            }
        }

        public override string ToString()
        {
            return $"{TenantID}{SplitChar}{SiteUrl}{SplitChar}{FileServerRelativeUrl}{SplitChar}{PathMD5}{SplitChar}{JobID}{SplitChar}{User}{SplitChar}{StubType}";
        }
    }
}
