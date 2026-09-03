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
    public class AveSPWebInfo
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPWeb mAveSPWeb = null;

        public AveSPWebInfo(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.WebInfo"))
            {
                output.WriteMetadata(AveMetadataType.WebBasicInfo, GetWebInfo());
            }
        }

        public AveWebInfo GetWebInfo()
        {
            return mAveSPWeb.SPWeb.WebSerializer.GetObjectData() as AveWebInfo;
        }
    }

    public class AveSPWebSettingInfo
    {
        private AveSPWeb mAveSPWeb = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveBackupOption mOption = new AveBackupOption();

        public AveSPWebSettingInfo(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public AveSPWebSettingInfo(AveSPWeb aveSPWeb, AveBackupOption option)
        {
            this.mAveSPWeb = aveSPWeb;
            this.mOption = option;
        }

        public AveWebSettingInfo GetWebSettingInfo()
        {
            var webSettingInfo = mAveSPWeb.SPWeb.WebSettingSerializer.GetObjectData(mOption);
            SetUserResource(webSettingInfo);
            return webSettingInfo;
        }

        private void SetUserResource(AveWebSettingInfo webSettingInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.SetUserResource"))
            {
                var spweb = mAveSPWeb.SPWeb;
                if (spweb != null)
                {
                    webSettingInfo.TitleResource = spweb.TitleResource.GetUserResourceInfo(spweb);
                    webSettingInfo.DescriptionResource = spweb.DescriptionResource.GetUserResourceInfo(spweb);
                }
            }
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.WebSettingInfo"))
            {
                AveWebSettingInfo webSettingInfo = GetWebSettingInfo();
                if (webSettingInfo.MetaDataNavigationRelativeTerm != null && webSettingInfo.MetaDataNavigationRelativeTerm.Count > 0)
                {
                    if (this.mOption.BackupMetadataNavigation)
                    {
                        output.WriteMetadata(AveMetadataType.MetadataService, webSettingInfo.MetaDataNavigationRelativeTerm);
                    }
                    webSettingInfo.MetaDataNavigationRelativeTerm = null;
                }
                output.WriteMetadata(AveMetadataType.WebProperty, webSettingInfo);
            }
        }
    }
}