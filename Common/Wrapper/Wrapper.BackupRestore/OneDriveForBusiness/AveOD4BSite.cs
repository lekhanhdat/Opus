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
using System;
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.AveO365LightWeightRequest;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class AveOD4BSite : AveOD4BBase, IAveBackupRestoreSite
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveBRSiteInfo mInternalInfo = null;

        public AveOD4BSite(string url, RequestConfig config)
            : base(config)
        {
            this.Url = url;
            Validate();
            //if (config.ConnectionType == ConnectionType.ServiceAccount)
            //{
            //    EnsureSiteAdmin(config.AdminUrl, config.Username);
            //}
        }

        private bool Validate()
        {
            return this.ID != Guid.Empty;
        }

        protected override string Level
        {
            get
            {
                return "Site";
            }
        }

        public string Url { get; private set; }

        public Guid ID
        {
            get
            {
                VerifyCacheData("SiteBasicInfo");
                return this.mInternalInfo.Id;
            }
        }

        private void EnsureSiteAdmin(string adminUrl, string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }
            string realAdminUrl = adminUrl;
            if (string.IsNullOrEmpty(realAdminUrl))
            {
                realAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(null, this.Url);
            }
            mLog.Info("Admin Url is:{0}", realAdminUrl);
            Controller.EnsureSiteAdmin(realAdminUrl, this.Url, userName);
        }

        protected override void EnsureExportMethods()
        {
            ExportMethods[BackupOption.BasicInfo] = ExportBasicInfo;
        }

        private ProcessResult ExportBasicInfo(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();

            VerifyCacheData("SiteBasicInfo");

            var siteInfo = InfoConverter<AveSiteInfo>.ConvertToCommonInfo(this.mInternalInfo);
            stream.WriteMetadata(AveMetadataType.SiteBasicInfo, siteInfo);
            return result;
        }

        protected override void FillCacheData(ProcessResult result)
        {
            this.mInternalInfo = base.mController.GetOD4BSiteInfo(this.Url);
            base.mInternCache.Add("SiteBasicInfo", new CacheItem() { Value = this.mInternalInfo, Result = result });
        }

        protected override void AddFakeData(ProcessResult result)
        {
            base.mInternCache.Add("SiteBasicInfo", new CacheItem() { Value = null, Result = result });
        }

        public List<IAveBackupRestoreWeb> GetWebs()
        {
            return new List<IAveBackupRestoreWeb>() { new AveOD4BWeb(Controller, this.Url) };
        }

        protected override List<AveBRChangeObject> GetChangedObjects()
        {
            return new List<AveBRChangeObject>();
        }

        public void Dispose()
        {
            base.mInternCache.Clear();
        }
    }
}
