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
using AvePoint.Common;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AvePublishing
    {
        private static AveObjectModelFactory mFactory = null;
        private static IAvePublishingWeb mPublishingWeb = null;

        private static AveObjectModelFactory Factory
        {
            get
            {
                if (mFactory == null)
                {
                    mFactory = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.ServerObjectModel);
                }
                return mFactory;
            }
        }

        private static IAvePublishingWeb PublishingWeb
        {
            get
            {
                if (mPublishingWeb == null)
                {
                    mPublishingWeb = Factory.CreatePublishingWeb();
                }
                return mPublishingWeb;
            }
        }

        public static Guid AverageRatings
        {
            get
            {
                if (AveSPEnv.IsPublishing)
                {
                    //代表Microsoft.SharePoint.Publishing.FieldId.AverageRatings;
                    return new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742");
                }
                return Guid.Empty;
            }
        }

        public static Guid RatingsCount
        {
            get
            {
                if (AveSPEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
                }
                return Guid.Empty;
            }
        }

        public static bool IsPublishingWeb(IAveWeb web)
        {
            if (AveEnv.IsPublishing)
            {
                return PublishingWeb.IsPublishingWeb(web);
            }
            return false;
        }

        public static IAvePublishingWeb GetPublishingWeb(IAveWeb web)
        {
            if (AveEnv.IsPublishing)
            {
                return PublishingWeb.GetPublishingWeb(web);
            }
            return null;
        }

        public static AveWebSettingInfo ProcessWebSettingInfo(AveWebSettingInfo webSettingInfo, IAveWeb web)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AvePublishing.ProcessWebSettingInfo"))
            {
                if (AveSPEnv.IsPublishing)
                {
                    IAvePublishingWeb pWeb = GetPublishingWeb(web);
                    if (pWeb != null)
                    {
                        webSettingInfo.InheritAlertCss = pWeb.AlternateCssUrl.IsInheriting;
                        webSettingInfo.InheritAlertCssUrl = pWeb.AlternateCssUrl.Value;
                        webSettingInfo.CInheriting = pWeb.CustomMasterUrl.IsInheriting;
                        webSettingInfo.CPageUrl = pWeb.CustomMasterUrl.Value;
                        webSettingInfo.MInheriting = pWeb.MasterUrl.IsInheriting;
                        webSettingInfo.MPageUrl = pWeb.MasterUrl.Value;
                    }
                }
                return webSettingInfo;
            }
        }
    }
}