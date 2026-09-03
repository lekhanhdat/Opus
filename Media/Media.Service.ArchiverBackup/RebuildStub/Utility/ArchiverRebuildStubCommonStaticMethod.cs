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

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class ArchiverRebuildStubCommonStaticMethod
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverRebuildStubCommonStaticMethod));
        public readonly static object mLock = new object();

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
                            var mGraphUser = GraphHelper.GetGraphUser(mail, accountInfo);
                            mOneDriveADUserID = mGraphUser.Id;
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Failed to get Azure AD user id:{mail} and FilterGraphUserByEmail.error :{e.ToString()}");
                            AppProfileInfo appProfile = GetBPOSInfoAsync(accountInfo.TenantId).GetAwaiter().GetResult();
                            var groupSite = new RMGraphGroupManager(appProfile);
                            var mGraphUser = groupSite.FilterGraphUserByEmail(mail).GetAwaiter().GetResult();
                            mOneDriveADUserID = mGraphUser.Id;
                            if (!string.IsNullOrEmpty(mOneDriveADUserID))
                            {
                                mLog.Info($"Success to get Azure AD user id by FilterGraphUserByEmail:{mail}.");
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
                AppType = aosAuthenticationProfile.AppType,
            };
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
    }
}

