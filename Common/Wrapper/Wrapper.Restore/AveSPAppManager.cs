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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using AvePoint.Wrapper.Common;


namespace AvePoint.Wrapper.Restore
{
    public class AveSPAppManager : RestoreableObject
    {
        private AveSPWeb aveSPWeb = null;                
        private IAveAppInstance appInstance;
        private IAveRestoreStream receiver = null;

        public AveSPAppManager(AveSPWeb web)
        {
            aveSPWeb = web;            
        }
        public IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }
        
        public int RestoreAppSelf(AveAppPackageInfo appPackageInfo)
        {            
            IAveAppSerializer appSerializer = aveSPWeb.ParentSite.ObjectModelFactory.CreateAppSerializer(aveSPWeb.SPWeb, (int)mRestoreOption.mAveRestoreMode);
            appInstance = appSerializer.SetObjectData(appPackageInfo);
            if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceIdSkippedAppData.Contains(appPackageInfo.InstanceId))
            {
                throw new AveWrapperSkipException(WrapperReportResourceKey.Wrapper_SkippedApp.ToString(), WrapperRestoreReportResource.Wrapper_SkippedApp);
            }
            WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAppInstanceIdMapping(appPackageInfo.InstanceId.ToString().ToLower(), appInstance.Id.ToString());
            return appSerializer.GetRestoreOption();
        }

        public void SetStream(IAveRestoreStream stream)
        {
            receiver = stream;
        }

        public IAveAppInstance AppInstance
        {
            get { return this.appInstance; }
        }
    }
}
