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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Office;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.Wrapper.Restore
{
    public class AveAudienceManager : AvePoint.Wrapper.Restore.IAveAudienceManager
    {
        private AveSPSite mAveParentSite = null;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        IAveOAudienceManager manager;

        public AveAudienceManager(AveSPSite aveSite)
        {
            mAveParentSite = aveSite;

            try
            {
                var context = mAveParentSite.ObjectModelFactory.CreateServiceContext();
                IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = mAveParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier();
                var serviceContext = context.GetContext(mAveParentSite.SPSite.WebApplication.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
                //serviceContext = SPServiceContext.GetContext(mAveSite.SPSite.WebApplication.ServiceApplicationProxyGroup, SPSiteSubscriptionIdentifier.Default);
                //从sharepoint API中看,只有Farm对象是null的时候才会返回null.这种情况现在没有case可以发生.
                manager = mAveParentSite.ObjectModelFactory.CreateAudienceManager(serviceContext);
            }
            catch (Exception e)
            {
                mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.DataProtection_GranularRestore_ItemLevel,
                        new EventIds.SharePoint.RestoreAudienceMappingFailedEventMessage(ParentSite.SPSite.Url, e));
            }

        }

        private Dictionary<string, string> GetAudienceNameIdMapping()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveAudienceManager.GetAudienceNameIdMapping"))
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
                        mLog.Warn("An error occurred while get audience. error", e);
                        return null;
                    }
                }

                return null;


            }


        }

        public void GenerateIDMapping(Dictionary<string, string> sourceMapping)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveAudienceManager.GenerateIDMapping"))
            {
                Dictionary<string, string> destMapping = GetAudienceNameIdMapping();

                if (destMapping != null)
                {
                    foreach (string key in sourceMapping.Keys)
                    {
                        if (destMapping.ContainsKey(key) && sourceMapping[key] != destMapping[key])
                        {
                            mAveParentSite.MappingManager.SiteMappingManager.AddAudienceIDMapping(sourceMapping[key], destMapping[key]);
                        }
                    }
                }
                if (mLog.IsDebugEnabled)
                {
                    StringBuilder info = new StringBuilder();
                    info.AppendLine("Audience information.");
                    if (sourceMapping != null)
                    {
                        info.AppendLine("Source:");
                        foreach (KeyValuePair<string, string> pair in sourceMapping)
                        {
                            info.AppendFormat("{0}\t{1}\r\n", pair.Key, pair.Value);
                        }
                    }
                    if (destMapping != null)
                    {
                        info.AppendLine("Destination:");
                        foreach (KeyValuePair<string, string> pair in destMapping)
                        {
                            info.AppendFormat("{0}\t{1}\r\n", pair.Key, pair.Value);
                        }
                    }
                    mLog.Debug(info.ToString());
                }

            }

        }
        public static string ReplaceAudienceId(Dictionary<string, string> audienceIdMapping, string oldValue)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveAudienceManager.ReplaceAudienceId"))
            {

                if (string.IsNullOrEmpty(oldValue))
                {
                    return oldValue;
                }
                if (oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return oldValue;
                }
                string tempValue = oldValue.Substring(0, oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(tempValue))
                {
                    return oldValue;
                }
                string newValue = oldValue;
                string[] tValues = tempValue.Split(',');
                foreach (string tValue in tValues)
                {
                    if (audienceIdMapping.ContainsKey(tValue))
                    {
                        newValue = newValue.Replace(tValue, audienceIdMapping[tValue]);
                    }
                }
                return newValue;

            }

        }

        #region IAveAudienceManager Members


        IAveSPSite IAveAudienceManager.ParentSite
        {
            get { return mAveParentSite; }
        }

        #endregion
    }
}
