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




using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSiteInfo
    {
        private AveSPSite mAveSite = null;

        public AveSPSiteInfo(AveSPSite aveSPSite)
        {
            mAveSite = aveSPSite;
        }

        public virtual AveSiteInfo GetSiteInfo()
        {
            return mAveSite.SPSite.SiteSerializer.GetObjectData() as AveSiteInfo;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.SiteInfo"))
            {
                output.WriteMetadata(AveMetadataType.SiteBasicInfo, GetSiteInfo());
            }
        }
    }

    public class AveSPSiteSettingInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mAveSite = null;
        private int mSettingTypes = -1;


        public AveSPSiteSettingInfo(AveSPSite aveSPSite)
        {
            mAveSite = aveSPSite;
        }

        public AveSPSiteSettingInfo(AveSPSite aveSPSite, int settingTypes)
        {
            mAveSite = aveSPSite;
            mSettingTypes = settingTypes;
        }

        public virtual AveSiteSettingInfo GetSiteSettingInfo()
        {
            mAveSite.SPSite.SiteSettingSerializer.SetBackupTypes(mSettingTypes);
            return mAveSite.SPSite.SiteSettingSerializer.GetObjectData() as AveSiteSettingInfo;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.SiteSettingInfo"))
            {
                var settingInfo = GetSiteSettingInfo();
                OutputSiteSettingInfo(settingInfo);
                output.WriteMetadata(AveMetadataType.SiteProperty, settingInfo);
            }
        }
        private static void OutputSiteSettingInfo(AveSiteSettingInfo settingInfo)
        {
            try
            {
                mLog.Info($"[SAAS-38254]Get site setting info, AllowDesigner:{settingInfo.AllowDesigner}, AllowMasterPageEditing{settingInfo.AllowMasterPageEditing}, AllowRevertFromTemplate:{settingInfo.AllowRevertFromTemplate}, ShowUrlStructure:{settingInfo.ShowURLStructure}");
            }
            catch (System.Exception e)
            {
                mLog.Warn($"An error occurred when get site setting infos:{0}", e);
            }
        }
    }
}