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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Upgrade;

namespace RADBInitializationUpgrade
{
    public class RMControlDBUpgrader : CommonUpgradeTaskBase
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMControlDBUpgrader));

        public override bool IsNeedUpgrade() => true;

        protected override Task<UpgradeStatus> RunDataCenterUpgradeAsync()
        {
            try
            {
                s_logger.Info($"Start initialize control DB.");

                RMDBInitializer.InitializeControlDatabase();

                s_logger.Info($"Successful initialize control DB.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while upgrade control db mode from service. Error: {e}");
                return Task.FromResult(UpgradeStatus.Failed);
            }

            return Task.FromResult(UpgradeStatus.Finished);
        }
    }
}
