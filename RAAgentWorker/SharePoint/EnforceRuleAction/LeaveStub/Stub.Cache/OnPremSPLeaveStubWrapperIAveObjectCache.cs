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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubWrapperIAveObjectCache
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory CurrentModelFactory;
        private AveBPOSAccountInfo BposInfo;
        private static OnPremSPLeaveStubWrapperIAveObjectCache actions = null;
        private static object mLock = new object();
        public IAveSite StubIAveSite { get; set; }
        public IAveWeb StubIAveWeb { get; set; }
        public IAveList StubIAveList { get; set; }

        public static OnPremSPLeaveStubWrapperIAveObjectCache GetInstance(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            if (actions == null)
            {
                lock (mLock)
                {
                    if (actions == null)
                    {
                        actions = new OnPremSPLeaveStubWrapperIAveObjectCache(objectModelFactory, bposInfo);
                    }
                }
            }
            return actions;
        }

        private OnPremSPLeaveStubWrapperIAveObjectCache(AveObjectModelFactory objectModelFactory, AveBPOSAccountInfo bposInfo)
        {
            CurrentModelFactory = objectModelFactory;
            BposInfo = bposInfo;
        }

        public void InitStubIAveObjectContainer(string mSiteUrl, Guid webGuid, Guid listGuid)
        {
            using (var performance = new AgentPerformanceScope("ArchiveBackUp.InitDeletionContainer", addToStatistics: true))
            {
                lock (mLock)
                {
                    if (null == StubIAveSite || string.Compare(StubIAveSite.Url, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (string.IsNullOrEmpty(mSiteUrl))
                        {
                            mLog.Error("mSiteUrl is null when InitStubIAveObjectContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init StubSite when InitStubIAveObjectContainer.");
                            StubIAveSite = CurrentModelFactory.CreateSite(mSiteUrl);
                        }
                    }

                    if (null == StubIAveWeb || !StubIAveWeb.ID.Equals(webGuid))
                    {
                        if (webGuid.Equals(Guid.Empty))
                        {
                            mLog.Error("webGuid is null when InitStubIAveObjectContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init StubWeb when InitStubIAveObjectContainer.webGuid:{0}.", webGuid);
                            StubIAveWeb = StubIAveSite.OpenWeb(webGuid);
                        }
                    }

                    if (null == StubIAveList || !listGuid.Equals(StubIAveList.ID))
                    {
                        if (listGuid.Equals(Guid.Empty))
                        {
                            mLog.Error("listGuid is null when InitStubIAveObjectContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init StubList when InitStubIAveObjectContainer.listGuid:{0}.", listGuid);
                            StubIAveList = StubIAveWeb.Lists[listGuid];
                        }
                    }
                }
            }
        }
    }
}
