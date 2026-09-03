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
using System.Collections.Generic;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System.Text;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.Wrapper.Backup
{
    public class AveAudienceManager 
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
		private AveSPSite mAveParentSite;
        IAveOAudienceManager manager;
        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
            set { mAveParentSite = value; }
        }

        public AveAudienceManager(AveSPSite aveSite)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.AudienceManager.Constructor"))
            {
                mAveParentSite = aveSite;
                try
                {
                    var context = mAveParentSite.ObjectModelFactory.CreateServiceContext();
                    var siteSubscriptionIdentifier = mAveParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier();
                    var serviceContext = context.GetContext(mAveParentSite.SPSite.WebApplication.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
                    //serviceContext = SPServiceContext.GetContext(mAveSite.SPSite.WebApplication.ServiceApplicationProxyGroup, SPSiteSubscriptionIdentifier.Default);
                    //从sharepoint API中看,只有Farm对象是null的时候才会返回null.这种情况现在没有case可以发生.
                    manager = mAveParentSite.ObjectModelFactory.CreateAudienceManager(serviceContext);
                }
                catch (Exception e)
                {
                    mLog.Log(EventSources.DocAveAgentService,EventCategorys.DocAveAgentService.DataProtection_GranularBackup_ItemLevel,
                        new EventIds.SharePoint.BackupAudienceMappingFailedEventMessage(ParentSite.SPSite.Url,e));
                }
            }
        }

        private Dictionary<string, string> GetAudienceNameIdMapping()
        {
            if (manager != null)
            {
                try
                {
                    Dictionary<string, string> nameIdMapping = new Dictionary<string, string>();
                    foreach (IAveOAudience aud in manager.Audiences)
                    {
                        nameIdMapping.Add(aud.AudienceName, aud.AudienceID.ToString());
                    }
                    return nameIdMapping;
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while get audience. error:{0}", e);
                    return null;
                }
            }

            return null;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.AudienceManager.Export"))
            {
                var nameIdMapping = GetAudienceNameIdMapping();
                if (nameIdMapping != null && nameIdMapping.Count > 0)
                {
                    mLog.Info("Export Audience Cache. item count:{0}", nameIdMapping.Count);
                    output.WriteMetadata(AveMetadataType.AudienceCache.ToString(), nameIdMapping);
                }
            }
        }
    }
}