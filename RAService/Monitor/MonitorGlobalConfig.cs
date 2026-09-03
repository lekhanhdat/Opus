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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor.Rule;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Monitor
{
    public class MonitorGlobalConfig
    {

        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        /// <summary>
        /// 默认关闭Monitor, 如需要监控需要开启开关, 之后可以根据License类型控制, Enterprise类型的默认监控.
        /// </summary>
        /// <returns></returns>
        public static bool CheckIfNeedMonitor()
        {
            return RMKeyValueDao.IsMonitorEnabled();
        }
       
        public static MonitorRuleBase GetRuleByType(MonitorType type)
        {
            MonitorRuleBase rule = null;
            switch (type)
            {
                case MonitorType.JobMonitor:
                    rule = GetJobMonitorRule();
                    break;
                case MonitorType.AgentStatus:
                    //rule = new MonitorRuleBase
                    break;
                default:
                    break;
            }
            return rule;
        }
        private static MonitorJobRule GetJobMonitorRule() 
        {
            var longRunningRange = RMKeyValueDao.GetMonitorLongRunningJobRange();
            var queryRange = RMKeyValueDao.GetMonitorQueryScope();
            return new MonitorJobRule()
            {
                QueryScope = new TimeSpan(queryRange),
                //JobTypes = new List<JobType>() { JobType.DataSynchronisation },
                LongRunningDate = new TimeSpan(longRunningRange)
            };
        }
    }
}
