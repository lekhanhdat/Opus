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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Graph;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Cloud.Sdk.Data.AosModern;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.Azure.Cosmos.Linq;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class ArchiverCommonStaticMethod
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverCommonStaticMethod));
        public static Guid HoldRecordStatus = new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
        public readonly static object mLock = new object();
        public static bool IsNestleCustomize { get; private set; }
        public static bool IsNestleCustomizeSearchFilter { get; private set; }
        public static int NestleCustomizeSearchFilterDays { get; private set; }
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public static string LastMovedDateProp = "opso_" + "LastMovedDate".ToMd5().ToString();

        //for check record
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "list property")]
        public static bool CheckListRecord(IAveList list)
        {
            try
            {
                return (GetBoolIprPropertyCore(list, "ecm_ListFieldsReadyForIPR") || IsHoldOrRecordsEnabled(list));
            }
            catch (Exception ex)
            {
                mLog.Info(ex.ToString());
                return false;
            }
        }

        public static bool IsContainerNode(int cacheNodeType)
        {
            switch (cacheNodeType)
            {
                case (int)CacheNodeType.Exception:
                case (int)CacheNodeType.WebApplication:
                case (int)CacheNodeType.SiteCollection:
                case (int)CacheNodeType.Web:
                case (int)CacheNodeType.APP:
                case (int)CacheNodeType.List:
                case (int)CacheNodeType.Folder:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CheckisRecord(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckisRecord"))
            {
                bool isRecord = false;
                int result = 0;
                try
                {
                    object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                    mLog.Info($"start CheckisRecord.value:{obj?.ToString()}");
                    if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                }
                catch (Exception ex)
                {
                    //mLog.Info(ex.ToString());
                    mLog.Info($"CheckisRecord failed,error:{ex}");
                    result = 0;
                }
                if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
                {
                    isRecord = true;
                }
                return isRecord;
            }
        }

        public static Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return new Guid(HashCodeHelper.ToMD5HashCode(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()));
        }

        /// <summary>
        /// 此方法返回True，表示只是Declare ，不是Hold + Declare;所以不建议通过此方法去判断文件是否只是Declare
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool CheckIsRecordOnly(IAveListItem item)
        {
            bool isRecordOnly = false;
            int result = 0;
            try
            {
                object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (Exception ex)
            {
                mLog.Info("This Item is not On Hold " + ex.Message);
                result = 0;
            }
            if (((result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0) && !((result & 0x1000) != 0))
            {
                //进入这里说明 次Item 仅仅是Record的而不是hold 的
                isRecordOnly = true;
            }
            return isRecordOnly;
        }
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

        private static string mReCenterHost = string.Empty;
        private readonly static object mReCenterHostLock = new object();

        public static string GetReCenterHost(string tenantGroupId)
        {
            if (!string.IsNullOrEmpty(mReCenterHost))
            {
                return mReCenterHost;
            }
            else
            {
                lock (mReCenterHostLock)
                {
                    if (string.IsNullOrEmpty(mReCenterHost))
                    {
                        mReCenterHost = RMAosApiClient.GetRECENTERServiceUrl(tenantGroupId);
                    }
                    return mReCenterHost;
                }
            }
        }

        private static string mOneDriveADUserID = string.Empty;
        private readonly static object mOneDriveADUserIDLock = new object();

        public static string GetADUserID(string mail, AveBPOSAccountInfo accountInfo)
        {
            if (!string.IsNullOrEmpty(mOneDriveADUserID))
            {
                return mOneDriveADUserID;
            }
            else
            {
                lock (mOneDriveADUserIDLock)
                {
                    if (string.IsNullOrEmpty(mOneDriveADUserID))
                    {
                        try
                        {
                            //GetGraphUser need use upn, if upn different with email, it will throw exception.
                            var mGraphUser = GraphHelper.GetGraphUser(mail, accountInfo);
                            mOneDriveADUserID = mGraphUser.Id;
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Failed to get Azure AD user id:{mail} and FilterGraphUserByEmail.error :{e.ToString()}");
                            //AppProfileInfo appProfile = GetBPOSInfoAsync(accountInfo.TenantId).GetAwaiter().GetResult();
                            //var groupSite = new RMGraphGroupManager(appProfile);
                            //var mGraphUser = groupSite.FilterGraphUserByEmail(mail).GetAwaiter().GetResult();
                            var mGraphUser = GraphHelper.GetGraphByEmail(mail, accountInfo);
                            if (mGraphUser != null)
                            {
                                mOneDriveADUserID = mGraphUser.Id;

                            }
                            if (!string.IsNullOrEmpty(mOneDriveADUserID))
                            {
                                mLog.Info($"Success to get Azure AD user id by FilterGraphUserByEmail:{mail}.mOneDriveADUserID:{mOneDriveADUserID}.");
                                return mOneDriveADUserID;
                            }
                            throw;
                        }
                    }
                    return mOneDriveADUserID;
                }
            }
        }
        public static async Task<AppProfileInfo> GetBPOSInfoAsync(string O365TenantId, bool useCache = true, bool enableMultipleAppProfile = true)
        {
            mLog.Info($"Get bpos info, O365TenantId:{O365TenantId} enableMultipleAppProfile:{enableMultipleAppProfile}");
            IRMAppProfileDao RMAppProfileDao = null;
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            var tenantId = O365TenantId;
            AppProfileInfo appProfile = null;
            if (enableMultipleAppProfile)
            {
                RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));
                var bestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
                if (bestApp != null)
                {
                    mLog.Info($"bestApp is not null,LogonGroupId is {TenantLocalValue.LogonGroupId}");
                    appProfile = RMAosApiClient.GetProfileByAppId(TenantLocalValue.LogonGroupId, O365TenantId, bestApp.AppClientId.ToString(), (IdentityProviderType)bestApp.AppType, useCache);
                }
                if (appProfile == null)
                {
                    mLog.Warn("App profile no longer exist, try to get app profiles from aos again.");
                    var authenticationProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, new List<string>() { tenantId });
                    mLog.Info($"Get app profiles from aos finished. Count:{authenticationProfiles?.Count}");
                    if (authenticationProfiles != null && authenticationProfiles.Count > 0)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(tenantId), authenticationProfiles.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                        var newBestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
                        if (newBestApp != null)
                        {
                            mLog.Info($"newBestApp is not null,LogonGroupId is {TenantLocalValue.LogonGroupId}");
                            appProfile = RMAosApiClient.GetProfileByAppId(TenantLocalValue.LogonGroupId, O365TenantId, bestApp?.AppClientId.ToString(), (IdentityProviderType)newBestApp.AppType, useCache);
                        }
                    }
                }
            }
            else
            {
                appProfile = RMAosApiClient.GetAppProfile(TenantLocalValue.LogonGroupId, O365TenantId, useCache);
            };

            return appProfile;
        }
        private static RMAppProfileInfo Convert2RMAppProfileInfo(RMAosAuthenticationProfile aosAuthenticationProfile)
        {
            return new RMAppProfileInfo()
            {
                AppClientId = new Guid(aosAuthenticationProfile.AppClientId),
                TenantId = new Guid(aosAuthenticationProfile.TenantId),
                UsedTimes = 0,
                AppType = aosAuthenticationProfile.AppType
            };
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
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.IsBlockDeleteOnlyRecord"))
            {
                return IsBlockDeleteOnlyRecord(GetHoldAndRecordStatus(item));
            }
        }

        private static Dictionary<string, Dictionary<string, AveComplianceTagInfo>> retentionSiteCache = new();

        public static bool IsHaveRecordLabel(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.HaveRetentionLabel"))
            {
                var currentLabelInfo = item.GetComplianceInfo(false);
                if (!string.IsNullOrEmpty(currentLabelInfo?.ComplianceTag ?? string.Empty))
                {
                    mLog.Info("[IsHaveRecordLabel]CurrentLabelInfo is not null or empty.");
                    var aveSite = item.Web.Site;
                    if (!retentionSiteCache.ContainsKey(aveSite.Url))
                    {
                        mLog.Info("[IsHaveRecordLabel]retentionSiteCache does not contains this site url.");
                        retentionSiteCache[aveSite.Url] = aveSite.GetAvailableTagsForSite().ToDictionary(r => r.TagName, StringComparer.OrdinalIgnoreCase);
                    }
                    var availableTags = retentionSiteCache[aveSite.Url];
                    if (availableTags.TryGetValue(currentLabelInfo?.ComplianceTag ?? string.Empty, out var currentLabel))
                    {
                        mLog.Info($"[IsHaveRecordLabel]CurrentLabel.BlockDelete: {currentLabel.BlockDelete}. CurrentLabel.BlockEdit: {currentLabel.BlockEdit}.");
                        if(currentLabel.BlockDelete && currentLabel.BlockEdit)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    mLog.Info("[IsHaveRecordLabel]CurrentLabelInfo is null or empty.");
                }
                return false;
            }
        }

        public static bool IsBlockDeleteOnlyRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.RecordMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.DeleteBlockedMask)) != 0L) && ((holdAndRecordStatus & (int)(HoldAndRecordStatusMask.EditBlockedMask)) == 0L);
        }

        public static void SetBlockEditAndDelete(IAveSite site)
        {
            var blockEditandDelete = RecordRestrictions.BlockDelete | RecordRestrictions.BlockEdit;
            SetRecordRestrictions(site, blockEditandDelete);
        }

        public static string GetRecordRestrictions(IAveSite site)
        {
            var originalRestrictionsSetting = string.Empty;
            if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions"))
            {
                originalRestrictionsSetting = site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString();
            }
            return originalRestrictionsSetting;
        }

        public static void SetRecordRestrictions(IAveSite site, RecordRestrictions option)
        {
            EnableCustomerScript(site);
            string? orginWebOption = RecordRestrictions.None.ToString();
            if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions"))
            {
                orginWebOption = site.RootWeb.AllProperties["ecm_siterecordrestrictions"]?.ToString();
            }
            try
            {
                site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = option.ToString();
                site.RootWeb.Update();
            }
            catch (Exception e)
            {
                mLog.Warn($"SetRecordRestrictions error {e}");
                mLog.Warn($"SetRecordRestrictions update web property {"ecm_siterecordrestrictions"} error, reset property to {orginWebOption}");
                site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = orginWebOption;
            }
            site.RootWeb.ReloadWeb();
            if (!site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions"))
            {
                mLog.Warn("Add web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
                throw new Exception("RM_UI_Failed_EnableCustomScript");
            }
            else
            {
                var webOption = site.RootWeb.AllProperties["ecm_siterecordrestrictions"]?.ToString();
                if (null == webOption || !string.Equals(webOption, (RecordRestrictions.BlockDelete | RecordRestrictions.BlockEdit).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Warn("Update web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
                    throw new Exception("RM_UI_Failed_EnableCustomScript");
                }
            }
        }

        public static void EnableCustomerScript(IAveSite site)
        {
            try
            {
                if (site.DenyAddAndCustomizePagesStatus)
                {
                    site.DenyAddAndCustomizePagesStatus = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Error in update deny add and customize page status, reason : {ex.ToString()}.");
            }
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

        public static bool IsOnHold(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.HoldMask) != 0L);
        }

        /// <summary>
        /// 此方法返回True，表示只是Declare ，不是Hold + Declare;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsRecordOnly(IAveListItem item)
        {
            var status = GetHoldAndRecordStatus(item);
            return IsRecordOnly(status);
        }

        public static bool IsRecordOnly(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.HoldMask) == 0L);
        }

        /// <summary>
        /// 此方法返回True，表示只是Hold ，不是Hold + Declare;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsHoldOnly(IAveListItem item)
        {
            var status = GetHoldAndRecordStatus(item);
            return IsHoldOnly(status);
        }

        public static bool IsHoldOnly(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.HoldMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) == 0L);
        }
        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetHoldAndRecordStatus"))
            {
                int result = 0;
                try
                {
                    if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
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
                catch (Exception ex)
                {
                    mLog.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
                }
                return result;
            }
        }
        public static bool CheckIsHoldOnly(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckIsHoldOnly"))
            {
                bool isHoldOnly = false;
                int result = 0;
                try
                {
                    object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                    if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                }
                catch (Exception ex)
                {
                    mLog.Info("This Item is not On Hold " + ex.Message);
                    result = 0;
                }
                if (((result & 0x1000) != 0 || (result & 1) != 0 || (result & 0x100) != 0) && !((result & 0x10) != 0))
                {
                    //进入这里说明 次Item 仅仅是Hold 的而不是Declare 的
                    isHoldOnly = true;
                }
                return isHoldOnly;
            }
        }
        private static bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetBoolIprPropertyCore"))
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

        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.IsHoldOrRecordsEnabled"))
            {
                if (list == null || list.Fields == null)
                {
                    throw new ArgumentNullException("list");
                }
                if (list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
                {
                    return (list.Fields[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")] != null);
                }
                else
                {
                    return false;
                }
            }
        }

        public static object DeSerializer(string text, Type oType)
        {
            object result = null;
            using (TextReader reader = new StringReader(text))
            {
                XmlSerializer s = new XmlSerializer(oType);
                result = s.Deserialize(reader);
            }
            return result;
        }

        public static string EscapeName(Hashtable syncCommonMapping, string name, bool isFile = true)
        {
            string fileNameExcludeExtension = name.Substring(0, name.LastIndexOf('.'));
            string fileExtension = name.Substring(name.LastIndexOf('.'));
            StringBuilder sbName = BaseEscapeName(syncCommonMapping, fileNameExcludeExtension);
            string newName = sbName.ToString() + fileExtension;
            return newName;
        }

        public static StringBuilder BaseEscapeName(Hashtable syncCommonMapping, string name)
        {
            StringBuilder sbName = new StringBuilder();
            //if (name.Length < illegalCharArray.Length)
            //{
            foreach (char nameChar in name)
            {
                //SharePoint允许文件或文件夹名含有'.',但是不能以'.'开头或结尾
                if (syncCommonMapping != null && syncCommonMapping.ContainsKey(nameChar))
                {
                    sbName.Append(syncCommonMapping[nameChar]);
                }
                else
                {
                    sbName.Append(nameChar);
                }
            }
            return sbName;
        }

        public static string GetBreakInheritSHA1String(string siteurl, string serverRelativeUrl)
        {
            string GetFullPath(string siteUrl, string itemUrl)
            {
                if (itemUrl.StartsWith("http:") || itemUrl.StartsWith("https:"))
                {
                    return itemUrl;
                }
                var stringBuilder = new StringBuilder(512);
                var siteUri = new Uri(siteUrl);
                stringBuilder.Append(siteUri.Scheme);
                stringBuilder.Append("://");
                stringBuilder.Append(siteUri.Host);
                return stringBuilder.ToString() + itemUrl;
            }
            return GetBreakInheritSHA1String(GetFullPath(siteurl, serverRelativeUrl));
        }

        public static string GetBreakInheritSHA1String(string fullUrl)
        {
            return Encrypt(fullUrl);
        }

        private static string Encrypt(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
            try
            {
                byte[] data = System.Text.Encoding.Default.GetBytes(url.ToLower(CultureInfo.CurrentCulture));
                byte[] sha1data = hash.ComputeHash(data);
                hash.Clear();
                StringBuilder sbr = new StringBuilder();
                for (int i = 0; i < sha1data.Length - 1; i++)
                {
                    sbr.Append(sha1data[i].ToString("x").PadLeft(2, '0'));
                }
                return sbr.ToString();
            }
            finally
            {
                if (hash != null)
                {
                    hash.Clear();
                }
            }
        }

        /// <summary>
        /// for sp make full url
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public static string MakeFullUrl(string siteUrl, string strUrl)
        {
            if (siteUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (siteUrl == strUrl)
            {
                return siteUrl;
            }
            if (strUrl.StartsWith("http:") || strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(siteUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(siteUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        public static bool StsCompareStrings(string str1, string str2)
        {
            System.Globalization.CompareInfo compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            return 0 == compareInfo.Compare(str1, str2, System.Globalization.CompareOptions.IgnoreCase);
        }

        public static void UpdateSiteRecordDeclarationSettings(IAveSite updateSite, string declareSetting)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateSiteRecordDeclarationSettings"))
            {
                try
                {
                    lock (mLock)
                    {
                        mLog.Info("Current Site:{0} need update RecordDeclarationSettings is:{1}.", updateSite.Url, declareSetting);
                        if (!updateSite.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions") || updateSite.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString() != declareSetting)
                        {
                            mLog.Info("Current DenyAddAndCustomizePagesStatus is:{0}.", updateSite.DenyAddAndCustomizePagesStatus);
                            if (updateSite.DenyAddAndCustomizePagesStatus)
                            {
                                updateSite.DenyAddAndCustomizePagesStatus = false;
                            }
                            updateSite.RootWeb.AllProperties["ecm_siterecordrestrictions"] = declareSetting;
                            updateSite.RootWeb.Update();
                            mLog.Info("Current Site ecm_siterecordrestrictions doesn't BlockDeleteEdit and need change to BlockDeleteEdit.Current is:{0}.", updateSite.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("An error occur when UpdateSiteRecordDeclarationSettings.Message:{0}.", ex.ToString());
                }
            }
        }

        public static string GetMessageFromCallStack(string callStack)
        {
            string keyword1 = "CallStack";
            if (string.IsNullOrEmpty(callStack))
            {
                return callStack;
            }
            if (callStack.IndexOf(keyword1) > 0)
            {
                return callStack.Substring(0, callStack.IndexOf(keyword1));
            }
            return callStack;
        }

        public static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                DirectoryInfo dbdir = new DirectoryInfo(dirPath);
                try
                {
                    dbdir.Create();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        public static void VerifyAvePoint(StorageDeviceDto storage)
        {
            if ((storage.IsAveStorage || storage.IsSystemStorage))
            {
                ILicenseHelperService LicenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
                if (!LicenseHelperService.IsAvePointStorage().GetAwaiter().GetResult())
                {
                    throw new LicenseMismatchOfAvePointStorageException();
                }
            }
        }
        public static void VerifyFSRetainAvePoint(StorageDeviceDto storage)
        {
            if ((storage.IsAveStorage || storage.IsSystemStorage || storage.Type == (int)AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType.Google))
            {
                throw new FSNotSurpportAvePointStorageException();
            }
        }
        public static void InitExchangeOnlineSetting()
        {
            IsNestleCustomize = GetIsNestleCustomize();
            IsNestleCustomizeSearchFilter = GetIsNestleCustomizeSearchFilter();
            NestleCustomizeSearchFilterDays = GetNestleCustomizeSearchFilterDays();
            mLog.Info($"ArchiverCommon InitExchangeOnlineSetting IsNestleCustomize:{IsNestleCustomize}.IsNestleCustomizeSearchFilter:{IsNestleCustomizeSearchFilter}.NestleCustomizeSearchFilterDays:{NestleCustomizeSearchFilterDays}.");
        }
        private static bool GetIsNestleCustomize()
        {
            var key = RMKeyValueDao.GetValueByKey("IsNestleCustomize");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private static bool GetIsNestleCustomizeSearchFilter()
        {
            var key = RMKeyValueDao.GetValueByKey("IsNestleCustomizeSearchFilter");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private static int GetNestleCustomizeSearchFilterDays()
        {
            var key = RMKeyValueDao.GetValueByKey("NestleCustomizeSearchFilterDays");
            int.TryParse(key?.Value, out int result);
            return result;
        }
    }

    public static class ArchiverTypeConvert
    {
        public static StubSettingParaDto ConvertStubSettingDtoToStubSettingParaDto(StubSettingDto stubSettingDto)
        {
            StubSettingParaDto stubSettingPara = new StubSettingParaDto();
            stubSettingPara.IsDeclareStubAsRecords = stubSettingDto.IsDeclareStubAsRecords;
            stubSettingPara.StubContent = stubSettingDto.StubContent;
            stubSettingPara.StubType = stubSettingDto.StubType;
            return stubSettingPara;
        }

        public static String ConvertNodeLevelToI18n(int cacheNodeType)
        {
            string I18nStr = "";
            if (cacheNodeType == (int)CacheNodeType.Exception)
            {
                I18nStr = "RM_Archiver_JobDetailExceptionLevel";
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItem)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_Item";
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItemVersion)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_ItemVersion";
            }
            else if (cacheNodeType == (int)CacheNodeType.ArchiveBy365Item)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_Item";
            }
            else if (cacheNodeType > (int)CacheNodeType.ItemVersion)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_Attachment";
            }
            else if (cacheNodeType > (int)CacheNodeType.Item)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_ItemVersion";
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {

                I18nStr = "RM_JS_Rule_ObjectLevel_Item";

            }
            else if (cacheNodeType > (int)CacheNodeType.List)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_Folder";

            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_List";

            }
            else if (cacheNodeType >= (int)CacheNodeType.Web)
            {
                if (cacheNodeType == (int)CacheNodeType.APP)
                {
                    I18nStr = "RM_JS_Rule_ObjectLevel_App";
                }
                else
                {
                    I18nStr = "RM_JS_Rule_ObjectLevel_Site";
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                I18nStr = "RM_JS_Rule_ObjectLevel_SiteCollection";
            }
            return I18nStr;
        }

        public static LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
                IsSystemStorage = storageDevice.Id == RecordsConstants.AVEPOINT_DEFAULT_STORAGEID || storageDevice.IsSystemStorage
            };

            var logical = new LogicalDeviceDto();
            logical.Name = storageDevice.Name;
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }
    }
    public static class LogExtension
    {
        public static void LogToXml(this RALogger logger, string prefix, object o)
        {
            try
            {
                var result = new StringBuilder();
                using (var writer = XmlWriter.Create(result))
                {
                    var serializer = new DataContractSerializer(o.GetType());
                    serializer.WriteObject(writer, o);
                }
                logger.Info(prefix + result);
            }
            catch (Exception ex)
            {
                string output = ex.ToString();
            }
        }
    }

}
