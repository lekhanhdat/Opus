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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;
using System.Reflection;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Restore
{
    public class AvePublishing
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static AveObjectModelFactory mFactory = null;
        private static IAvePublishingWeb mPublishingWeb = null;
        private static IAvePublishingSite mPublishingSite = null;

        private static AveObjectModelFactory Factory
        {
            get
            {
                if (mFactory == null)
                {
                    mFactory = WrapperRuntime.CurrentContext.ModelFactory;//AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.ServerObjectModel);
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

        private static IAvePublishingSite PublishingSite
        {
            get
            {
                if (mPublishingSite == null)
                {
                    mPublishingSite = Factory.CreatePublishingSite();
                }
                return mPublishingSite;
            }
        }

        public static bool IsPublishingSite(IAveSite site)
        {
            if (AveEnv.IsPublishing)
            {
                return PublishingSite.IsPublishingSite(site);
            }           
            return false;
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
        public static void SetWelcomePage(IAveWeb web, string welcomePageUrl)
        {
            try
            {
                if (AveEnv.IsPublishing && IsPublishingWeb(web))
                {
                    IAvePublishingWeb currentPublishingWeb = GetPublishingWeb(web);
                    IAveFile welcomePage = web.GetFile(welcomePageUrl);
                    currentPublishingWeb.DefaultPage = welcomePage;
                    currentPublishingWeb.Update();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.SetWelcomePageFailed, e);
            }
        }
        public static Guid AverageRatings
        {
            get
            {
                if (AveEnv.IsPublishing)
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
                if (AveEnv.IsPublishing)
                {
                    //代表 Microsoft.SharePoint.Publishing.FieldId.RatingsCount
                    return new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
                }
                return Guid.Empty;
            }
        }

        public static void DeclareItemAsRecord(IAveListItem item)
        {
            if (AveSPEnv.IsPublishing)
            {
                IAveORecords records = Factory.CreateRecords();
                records.DeclareItemAsRecord(item);
            }
        }

        public static void UnlockItem(IAveListItem item)
        {
            if (AveEnv.IsPublishing)
            {
                IAveORecords records = Factory.CreateRecords();
                
                if (records.IsLocked(item))
                {
                    records.UndeclareItemAsRecord(item);
                }
                if (item.File != null && item.Level == AveFileLevel.Checkout)
                {
                    records.UnlockItem(item, item.Name);
                }
                if (item.Properties.ContainsKey("_vti_ItemHoldRecordStatus"))
                {
                    //清空_vti_ItemHoldRecordStatus属性值，不然在删除的时候可能删不了
                    item.Properties["_vti_ItemHoldRecordStatus"] = null;
                    item.SystemUpdate(false);
                }
            }
        }

        public static void LockItem(IAveListItem item, IAveListItem holdItem, string comments)
        {
            if (AveSPEnv.IsPublishing)
            {
                IAveOHold hold = Factory.CreateHold();
                hold.SetHold(item, holdItem, comments);
            }
        }

        public static IAveList GetHoldsList(IAveWeb web)
        {
            IAveList list = null;
            if (AveSPEnv.IsPublishing)
            {
                IAveOHold hold = Factory.CreateHold();
                list = hold.GetHoldsList(web);
            }
            return list;
        }

        public static void SetSiteLockProperty(IAveSite site)
        {
            if (AveSPEnv.IsPublishing)
            {
                IAveOHold hold = Factory.CreateHold();
                hold.SetSiteLockProperty(site);
            }
        }

        public static void ProvisionWeb(IAveWeb web)
        {
            IAveOHold hold = Factory.CreateHold();
            hold.ProvisionWeb(web);
        }

        public static void ProvisionList(IAveList list)
        {
            IAveOHold hold = Factory.CreateHold();
            hold.ProvisionList(list);
        }
    }

    public class AveEcmDocumentRouting
    {

        private static AveObjectModelFactory mFactory = null;
        private readonly static object mLock = new object();

        private static AveObjectModelFactory Factory
        {
            get
            {
                if (mFactory == null)
                {
                    lock (mLock)
                    {
                        if (mFactory == null)
                        {
                            mFactory = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.ServerObjectModel);
                        }
                    }
                }
                return mFactory;
            }
        }
        public static bool RouteFileToFinalDestination(IAveWeb web, IAveList dropOffLibrary, IAveUser routUser, IAveFile file, out string routDestination)
        {
            return Factory.EcmDocumentRouting().RouteFileToFinalDestination(web, dropOffLibrary, routUser, file, out routDestination);
        }

        /// <summary>
        /// According to Routing Rule to update edit template name of content type to 'DropOffZoneRoutingForm'
        /// </summary>
        /// <param name="web"></param>
        public static void UpdateDropOffLibContentType(IAveWeb web)
        {
            Factory.EcmDocumentRouting().UpdateDropOffLibContentType(web);
        }
    }

}
