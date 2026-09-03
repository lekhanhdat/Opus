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
using AvePoint.Cryptography;
using AvePoint.GCommon;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using System;
using System.Reflection;
using System.Web;

namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class SPMoveFolderRestore : AveSPFolder, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveSPSite aveSPSite;
        internal AveSPWeb aveSPWeb;
        internal AveSPList aveSPList;
        internal AveSPFolder aveSPFolder;
        Guid aveSPWebId;
        Guid aveSPListId;
        IAveSite site;
        IAveWeb web;
        IAveList list;
        ScheduleConfiguration mConfig;
        IAveRestoreStream importStream;
        private bool isStubRestore = false;
        private DateTime mInitialTime = DateTime.MinValue;//用于记录Site的生存时间
        private string mSiteUrl = string.Empty;

        public SPMoveFolderRestore()
        {

        }

        public void Init(IAveRestoreStream stream, ScheduleConfiguration config, bool isStubRestore = false)
        {
            importStream = stream;
            mConfig = config;
            this.isStubRestore = isStubRestore;
        }

        public AveSPFolder GetDestFolder()
        {
            return aveSPFolder;
        }

        public void RestoreParentInfo(string desUrl, string subFolderUrl)
        {
            AveBPOSAccountInfo user = null;
            string siteUrl = string.Empty;
            string userName = string.Empty;
            AveObjectModelFactory factory = null;
            AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo = null;
            if (isStubRestore)
            {
                factory = mConfig.aveObjectModelFactory;
                userName = mConfig.user.UserName;
            }
            else
            {
                factory = mConfig.recordManagerRestoreOMFactory;
                userName = mConfig.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName;
                bposInfo = mConfig.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo;
            }
            if (bposInfo != null && bposInfo.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken)
            {
                user = bposInfo.ConvertToAveBPOSAccountInfo();
                siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);//获取的是Web URL，而不是实际的SC URL
            }
            else if (!string.IsNullOrEmpty(userName))
            {
                if (bposInfo != null)
                {
                    user = bposInfo.ConvertToAveBPOSAccountInfo();
                }
                else
                {
                    if (isStubRestore)
                    {
                        user = new AveBPOSAccountInfo() { Domain = "", UserName = userName, Password = mConfig.user.Password, TenantGroupId = string.Empty };
                    }
                    else
                    {
                        user = new AveBPOSAccountInfo() { Domain = "", UserName = userName, Password = CspCommunicationWrapper.UnWrapKeyToSecureString(mConfig.currentRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password), TenantGroupId = string.Empty };
                    }
                }
                mLog.Info("RestoreParentInfo: Init BPOS Factory , tenantId is:{0}.TenantGroupId:{1}.", user.TenantId, user.TenantGroupId);
                siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);
            }
            else
            {
                user = mConfig.user;
                siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);
            }

            if (site == null)
            {
                mInitialTime = DateTime.Now;
                site = factory.CreateSite(siteUrl);
                mSiteUrl = siteUrl;
                web = site.OpenWeb();
            }
            else if ((string.Compare(desUrl, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                        || mInitialTime.AddHours(23) < DateTime.Now)
            {
                site.Dispose();
                mInitialTime = DateTime.Now;
                site = factory.CreateSite(siteUrl);
                mSiteUrl = desUrl;
                web = site.OpenWeb();
            }
            list = web.GetList(desUrl);
            string listUrl = list.ParentWeb.Url + "/" + list.RootFolder.Url;
            //string subFolderUrl = desUrl.Substring(listUrl.Length).Trim('/');
            RestoreSiteInfo(site, user);
            RestoreWebInfo();
            RestoreListInfo();
            RestoreFolderInfo(subFolderUrl);
        }

        public void RestoreSiteInfo(IAveSite site, AveBPOSAccountInfo user)
        {
            var siteInfo = importStream.ReadMetadata().GetMetadata<AveSiteInfo>();
            if (aveSPSite == null)
            {
                if (user != null)//(site.IsOnlineSite)
                {
                    aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.ClientObjectModel, user);
                }
                else
                {
                    aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.Server13ObjectModel, null);
                }
                aveSPSite.RestoreSiteSelf(siteInfo);
            }
            importStream.Reset();
        }

        private void RestoreWebInfo()
        {
            var webInfo = importStream.ReadMetadata().GetMetadata<AveWebInfo>();
            if (aveSPWeb == null || aveSPWebId == null || aveSPWebId != aveSPFolder.SPFolder.ParentWeb.ID)
            {
                aveSPWeb = new AveSPWeb(aveSPSite, web.ServerRelativeUrl);
                aveSPWebId = web.ID;
                aveSPWeb.RestoreWebSelf(webInfo);
            }
            importStream.Reset();
        }

        private void RestoreListInfo()
        {
            var listInfo = importStream.ReadMetadata().GetMetadata<AveListInfo>();
            var fieldXML = importStream.ReadMetadata().GetMetadata<string>();
            var contentTypeInfo = importStream.ReadMetadata().GetMetadata<AveContentTypeCollectionInfo>();
            if (aveSPList == null || aveSPListId == null || aveSPListId != list.ID)
            {
                aveSPList = new AveSPList(aveSPWeb, list.Title);
                //change list title to find the right list  //SAAS-29158 RECO-348
                listInfo.Title = list.Title;
                listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                aveSPListId = list.ID;
                listInfo.RootWebOnly = false;
                aveSPList.RestoreListSelf(listInfo);
            }
            //SAAS-15676  由于结构原因导致Field和ContentType每次job只Reload一次,如果原端list改变则需要重新load
            if (listInfo.Id != mConfig.tempListId)
            {
                aveSPList.AveFields.RestoreFields(fieldXML);
                aveSPList.AveFields.LoadFields(fieldXML);
                aveSPList.AveContentTypes.LoadContentTypes(contentTypeInfo);
                mConfig.tempListId = listInfo.Id;
            }
            importStream.Reset();
        }

        /// <summary>
        /// destFolderUrl  e.g: dest url substring list url
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="destFolderUrl"></param>
        /// <returns></returns>
        private AveSPFolder GetSubSPFolder(AveSPFolder parentFolder, string destFolderUrl)
        {
            if (string.IsNullOrEmpty(destFolderUrl))
            {
                return parentFolder;
            }
            if (!destFolderUrl.Contains("/"))
            {
                AveSPFolder subFolder = new AveSPFolder(parentFolder, destFolderUrl);
                if (subFolder.ParentFolder != null && (subFolder.ParentFolder.SPFolder == null || !subFolder.ParentFolder.SPFolder.Exists))
                {
                    subFolder.ParentFolder.InitSPFolder(true);
                }
                return subFolder;
            }
            int pos = destFolderUrl.IndexOf("/");
            if (pos > -1)
            {
                string subDest = destFolderUrl.Substring(0, pos);
                string subLastDest = destFolderUrl.Substring(pos + 1);
                AveSPFolder subFolder = new AveSPFolder(parentFolder, subDest);
                if (subFolder.ParentFolder != null && (subFolder.ParentFolder.SPFolder == null || !subFolder.ParentFolder.SPFolder.Exists))
                {
                    subFolder.ParentFolder.InitSPFolder(true);
                }
                return this.GetSubSPFolder(subFolder, subLastDest);
            }
            return parentFolder;
        }

        private void RestoreFolderInfo(string destSubFolderUrl)
        {
            aveSPFolder = new AveSPFolder(aveSPList, list.RootFolder.Name);
            AveSPFolder subFolder = GetSubSPFolder(aveSPFolder, destSubFolderUrl);
            aveSPFolder = subFolder.ParentFolder;
            //if (aveSPFolder.ServerRelativeUrl == aveSPList.RootFolder.ServerRelativeUrl)
            //{
            //    aveSPFolder.Name = string.Empty;
            //}
            importStream.Reset();
        }



        public void Dispose()
        {
            DisposeObj(site);
            DisposeObj(web);
            DisposeObj(aveSPSite);
            DisposeObj(aveSPWeb);
        }

        private void DisposeObj(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
