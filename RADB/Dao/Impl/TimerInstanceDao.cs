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
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TimerInstanceDao : ITimerInstanceDao
    {
        private const string TableName = "RMTimerInstances";
        private RALogger logger = RALogger.GetInstance(typeof(TimerInstanceDao));


        public void RefreshTimer(string name, long activityTimePeriod)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var timerInstance = context.RMTimerInstances.Where(t => t.Name == name).FirstOrDefault();

                bool existsPrimaryTimer = false;
                bool switchPrimaryTimer = false;
                if (timerInstance == null || !timerInstance.IsPrimary)
                {
                    var primaryTimers = context.RMTimerInstances.Where(t => t.IsPrimary);
                    foreach (var pTimer in primaryTimers)
                    {
                        if (pTimer.TimeStamp < activityTimePeriod)
                        {
                            pTimer.IsPrimary = false;
                            logger.Info($"Primary timer timeout : {name}");
                        }
                        else
                        {
                            existsPrimaryTimer = true;
                        }
                    }

                    if (timerInstance == null)
                    {
                        switchPrimaryTimer = !existsPrimaryTimer;
                        timerInstance = new RMTimerInstance()
                        {
                            Name = name,
                            IsPrimary = switchPrimaryTimer,
                            TimeStamp = DateTime.UtcNow.Ticks
                        };
                        context.RMTimerInstances.Add(timerInstance);

                        logger.Info($"Add timer instance: {name}, IsPrimary: {!existsPrimaryTimer}");
                    }
                    else if (!timerInstance.IsPrimary)
                    {
                        switchPrimaryTimer = !existsPrimaryTimer;
                        timerInstance.IsPrimary = switchPrimaryTimer;
                    }

                    if (switchPrimaryTimer)
                    {
                        logger.Info($"Switch primary timer: {name}");
                        ReleaseTask(TaskType.ObserveAOSNotification);
                    }
                }

                timerInstance.TimeStamp = DateTime.UtcNow.Ticks;
                context.SaveChanges();
            }
        }

        public void ReleaseTask(params TaskType[] taskTypes)
        {
            var TaskJobDao = new RMTaskJobDao();
            foreach(var taskType in taskTypes)
            {
                try
                {
                    var task = RMTaskFactory.GetDefaultTask(taskType);
                    TaskJobDao.ReleaseTimer(task);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while releasing task: {taskType}. {ex}");
                }
            }
        }

        public bool IsPrimaryTimer(string name)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@Name", name)
                };
                string sql = $"SELECT COUNT(1) FROM {TableName} WHERE IsPrimary=1 AND Name=@Name ;";
                var count = context.Database.SqlQuery<int>(sql, sqlParams).FirstOrDefault();
                return count > 0;
            }
        }


    }
}
