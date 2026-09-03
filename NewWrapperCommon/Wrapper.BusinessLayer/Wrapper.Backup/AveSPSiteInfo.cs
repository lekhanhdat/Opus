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
using System.Collections.Generic;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSiteInfo
    {
        private AveSPSite mAveSite = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSiteInfo(AveSPSite aveSPSite)
        {
            mAveSite = aveSPSite;
        }

        public virtual AveSiteInfo GetSiteInfo()
        {
            return mAveSite.SPSite.SiteSerializer.GetObjectData() as AveSiteInfo;
        }

        /// <summary>
        /// Get basic Site Info
        /// </summary>
        /// <returns></returns>
        public virtual AveSiteInfo GetSiteBasicInfo()
        {
            return mAveSite.SPSite.SiteSerializer.GetSiteBasicInfo();
        }

        /// <summary>
        /// Get site info with all web templates
        /// </summary>
        /// <param name="output"></param>
        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.SiteInfo"))
            {
                output.WriteMetadata(AveMetadataType.SiteBasicInfo, GetSiteInfo());
            }
        }

        /// <summary>
        /// only contains the basic information.
        /// </summary>
        /// <param name="output"></param>
        public void ExportBasicInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.SiteInfo"))
            {
                output.WriteMetadata(AveMetadataType.SiteBasicInfo, GetSiteBasicInfo());
            }
        }
    }

    public class AveSPSiteSettingInfo 
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mAveSite = null;

        public AveSPSiteSettingInfo(AveSPSite aveSPSite)
        {
            mAveSite = aveSPSite;
        }

        public virtual AveSiteSettingInfo GetSiteSettingInfo()
        {
            return mAveSite.SPSite.SiteSettingSerializer.GetObjectData() as AveSiteSettingInfo;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.SiteSettingInfo"))
            {
                var setting = GetSiteSettingInfo();
                ExportRelatedUsers(setting,output);
                output.WriteMetadata(AveMetadataType.SiteProperty, setting);
            }
        }
        /// <summary>
        /// 暂时不加option控制。
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="output"></param>
        private void ExportRelatedUsers(AveSiteSettingInfo setting, IAveBackupStream output)
        {
            var users = GetRelatedUsers(setting);
            if (users != null && users.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.UserCache, users);
            }
        }

        private List<AveUserInfo> GetRelatedUsers(AveSiteSettingInfo setting)
        {
            var userList = new List<AveUserInfo>();
            if (setting.OwnerID.IsAvailable && setting.OwnerID.Value.HasValue)
            {
                userList.Add(mAveSite.DataCache.GetUserInfo(setting.OwnerID.Value.Value));
            }
            if (setting.SecondaryContactID.IsAvailable && setting.SecondaryContactID.Value.HasValue)
            {
                userList.Add(mAveSite.DataCache.GetUserInfo(setting.SecondaryContactID.Value.Value));
            }
            return userList;
        }
    }
}