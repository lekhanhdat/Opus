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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.ControlMigrations.Upgrade.Impl
{
    public class RMTaskUpgradeDao
    {
        private List<TaskType> excludeTasks = new List<TaskType>() { TaskType.UpgradeDB};
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal IRMTaskJobDao taskJobDao;
        public void Upgrade(Core.RMSysDBContext context)
        {
            try
            {
                taskJobDao = new RMTaskJobDao();
                var timerInstanceDao = new TimerInstanceDao();
                var isPimaryTimer = timerInstanceDao.IsPrimaryTimer(Dns.GetHostName());
                var taskTypes = Enum.GetValues(typeof(TaskType));
                foreach (var taskType in taskTypes)
                {
                    TaskType type = (TaskType)taskType;
                    
                    if (!excludeTasks.Contains(type))
                    {
                        try
                        {
                            var task = RMTaskFactory.GetDefaultTask(type);
                            if (!context.Task.Any(t => t.Type == type))
                            {
                                taskJobDao.CreateTask(task);
                            }
                            else if (isPimaryTimer)
                            {
                                if (type == TaskType.ObserveAOSNotification)
                                {
                                    taskJobDao.ReleaseTimer(task);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error occurred while creating task: {type}. {ex}");
                        }
                    }
                    else
                    {
                        logger.Info($"skip to upgrade task:{type}");
                    }
                   
                }

            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while upgrade task:{ex.ToString()}");
            }
           
        }
    }
}
