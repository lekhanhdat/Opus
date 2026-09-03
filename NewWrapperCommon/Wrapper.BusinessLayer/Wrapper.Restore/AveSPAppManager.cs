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
using AvePoint.Wrapper.Common;


namespace AvePoint.Wrapper.Restore
{
    public class AveSPAppManager : RestoreableObject, IAveSPAppManager
    {
        private AveSPWeb aveSPWeb = null;
        private IAveAppInstance appInstance;
        private IAveAppSerializer serializer;
        private IAveRestoreStream receiver;

        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPAppManager(AveSPWeb web)
        {
            this.aveSPWeb = web;

            serializer = this.aveSPWeb.SPWeb.AppSerializer;
        }

        public void RestoreAppSelf(AveAppPackageInfo appPackageInfo)
        {

            this.serializer.SetStream(receiver);
            this.serializer.SetRestoreOption(mRestoreOption);
            this.appInstance = this.serializer.SetObjectData(appPackageInfo);

            aveSPWeb.ParentSite.MappingManager.SiteMappingManager.AppInstanceIdMapping.Add(appPackageInfo.InstanceId, appInstance.Id);
            
        }

        public void SetStream(IAveRestoreStream stream)
        {
            receiver = stream;
        }

        public IAveAppInstance AppInstance
        {
            get { return this.appInstance; }
        }

        public AveRestoreMode GetRestoreOption()
        {
            return (AveRestoreMode)this.serializer.GetRestoreOption();
        }
    }
}
