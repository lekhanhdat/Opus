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
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.ServiceVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Common
{

    public class LoggerInitializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(LoggerInitializer));

        public static void SyncLogLocationInfo()
        {
            try
            {
                AveStaticEnv.Setup();
                LogUploadService logUploadService = LogUploadService.GetInstance();
                var xriString = AvePoint.GCommon.Utility.Cloud.GCommonRoleConfiguration.JobLogStorageXri;
                logUploadService.ApplyXriString(xriString);
                logUploadService.ApplyIdentifier(string.Format("{0}_{1}", AveEnv.AgentName, AveEnv.AgentRoleId));
                string version = ServiceVersionHelper.GetVersion(true).ProductVersion;
                logUploadService.ApplyVersion(version);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
