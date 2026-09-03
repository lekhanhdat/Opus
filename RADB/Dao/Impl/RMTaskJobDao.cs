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
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System.Collections;
using System.Data;
using AvePoint.RA.DB.Core;
using System.Data.Entity;
using System.Transactions;
using AvePoint.RA.CommonUtil;
using System.Reflection;

namespace AvePoint.RA.DB.Dao.Impl
{
    
    public class RMTaskJobDao : BaseDao<RMTask>, IRMTaskJobDao
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        private TaskBase AssembleTask(RMTask task)
        {
            try
            {
                TaskBase domain = RMTaskFactory.GetDefaultTask(task.Type);
                domain.Id = task.Id;
                domain.Type = task.Type;
                domain.NextRunTime = task.NextRunTime;
                domain.RowVersion = task.RowVersion1;
                domain.Status = task.Status;
                domain.ProfileId = task.ProfileId;
                if (task.Schedule != null)
                {
                    domain.Schedule = new TaskSchedule()
                    {
                        Interval = task.Schedule.Interval,
                        IntervalType = task.Schedule.IntervalType
                    };
                }
                return domain;
            }
            catch(Exception e)
            {
                logger.Error(e.ToString());
                return null;
            }
        }
        public List<TaskBase> GetAllTask()
        {
            var now = DateTime.UtcNow.Ticks;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                 return (from t in ctx.Task
                            join s in ctx.RMTaskSchedule on t.ScheduleId equals s.Id
                            where t.NextRunTime <= now
                            select new
                            {
                                Id = t.Id,
                                Type = t.Type,
                                NextTime = t.NextRunTime,
                                Interval = s.Interval,
                                IntervalType = s.IntervalType,
                                ScheduleId = t.ScheduleId,
                                ProfileId = t.ProfileId,
                                Status = t.Status,
                                RowVersion = t.RowVersion1
                            }).AsEnumerable().Select(t => 
                                AssembleTask(new RMTask()
                                {
                                    Id = t.Id,
                                    ProfileId = t.ProfileId,
                                    RowVersion1 = t.RowVersion,
                                    NextRunTime = t.NextTime,
                                    ScheduleId = t.ScheduleId,
                                    Status = t.Status,
                                    Type = t.Type,
                                    Schedule = new RMTaskSchedule() { Interval = t.Interval, IntervalType = t.IntervalType }
                                })).ToList();
            }
        }

        public bool LockTask(TaskBase task)
        {
            bool result = false;
            try
            {
                var isTimeout = task.IsTimeout;
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    RMTask tempTask = ctx.Task.Where(t => t.Id == task.Id && (t.Status == RMTaskStatus.Completed || isTimeout)).FirstOrDefault();

                    if (tempTask != null)
                    {
                        tempTask.NextRunTime = task.CalculateNextRunTime(DateTime.UtcNow.Ticks);
                        if (isTimeout)
                        {
                            logger.Warn($"current task lock timeout:{task.Type}, {task.Timeout} min, reset nextRunTime:{tempTask.NextRunTime}");
                        }
                        
                        if (tempTask.DisallowConcurrentExecution)
                        {
                            tempTask.Status = RMTaskStatus.Processing;
                        }
                        tempTask.LastModified = DateTime.UtcNow;
                        result = ApplyCurrentValues(ctx, tempTask);
                    }
                    

                }
            }
            catch (Exception ex)
            {
                logger.Warn($"lock task failed:{task.Type}, {ex.Message}");
                result = false;
            }
            return result;
        }

        public bool ReleaseTask(TaskBase task)
        {
            bool result = false;
            try
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    RMTask tempTask = ctx.Task.Where(t => t.Id == task.Id).FirstOrDefault();

                    if (tempTask != null)
                    {
                        logger.Info($"release task: {task.Type}");
                        tempTask.Status = RMTaskStatus.Completed;
                        tempTask.LastModified = DateTime.UtcNow;
                        result = ApplyCurrentValues(ctx, tempTask);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"release task failed:{task.Type}, {ex.ToString()}");
                result = false;
            }
            return result;
        }

        public RAReturnMessage ReleaseTimer(TaskBase task)
        {
            RAReturnMessage result = new RAReturnMessage() { FaildType = RAFailedType.UpdateFailed, MessageType = RAMessageType.Failed };
            try
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    var now = DateTime.UtcNow;
                    RMTask dbTask = ctx.Task.Where(t => t.Type == task.Type).FirstOrDefault();

                    if (dbTask != null)
                    {
                        logger.Info($"release task: {task.Type}");
                        dbTask.ProfileId = task.ProfileId;
                        dbTask.Status = RMTaskStatus.Completed;
                        dbTask.LastModified = DateTime.UtcNow;
                        if (ApplyCurrentValues(ctx, dbTask))
                        {
                            result.MessageType = RAMessageType.Successful;
                            result.FaildType = RAFailedType.None;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Warn($"release timer failed:{task.Type}, {ex.ToString()}");
                result.FaildType = RAFailedType.UpdateFailed;
            }
            return result;
        }

        public RAReturnMessage LockTimer(TaskBase task)
        {
            RAReturnMessage result = new RAReturnMessage() { FaildType = RAFailedType.UpdateFailed, MessageType = RAMessageType.Failed };
            try
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    var now = DateTime.UtcNow;
                    RMTask dbTask = ctx.Task.Where(t => t.Id == task.Id && ((t.Status == RMTaskStatus.Completed && string.IsNullOrEmpty(t.ProfileId)) || DbFunctions.AddMinutes(t.LastModified, task.Timeout) < now)).FirstOrDefault();
                   
                    if (dbTask != null)
                    {
                        if (dbTask.LastModified.AddMinutes(task.Timeout) < now)
                        {
                            logger.Warn($"Timer locked time out, {dbTask.ProfileId}, lastModified:{dbTask.LastModified}");
                        }
                        dbTask.ProfileId = task.ProfileId;
                        dbTask.Status = RMTaskStatus.Processing;
                        dbTask.LastModified = DateTime.UtcNow;
                        if (ApplyCurrentValues(ctx, dbTask))
                        {
                            result.MessageType = RAMessageType.Successful;
                            result.FaildType = RAFailedType.None;
                        }
                    }
                    
                    
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"lock timer failed:{task.Id}, {ex.ToString()}");
                result.FaildType = RAFailedType.UpdateFailed;
            }
            return result;
        }

        private bool ApplyCurrentValues(RMSysDBContext context, RMTask entity)
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Modified)
            {
                return context.SaveChanges() > 0;
            }
            else if (entry.State == EntityState.Detached)
            {
                context.DetachLocalObject<RMTask>(entity);
                context.Set<RMTask>().Attach(entity);
                entry.State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public string CreateTask(TaskBase task)
        {
            var newTask = Convert2RMTask(task);
            var schedule = task.Schedule == null ? null : Convert2RMTaskSchedule(task.Schedule);
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    if (!ctx.Task.Any(t => t.Type == task.Type))
                    {
                        if (schedule != null)
                        {
                            ctx.RMTaskSchedule.Add(schedule);
                            ctx.SaveChanges();
                        }
                        newTask.LastModified = DateTime.UtcNow;
                        ctx.Task.Add(newTask);
                        ctx.SaveChanges();
                    }
                    scope.Complete();
                }
                    
            }
            return newTask.Id;
        }

        private RMTask Convert2RMTask(TaskBase task)
        {
            return new RMTask()
            {
                Id = task.Id,
                Type = task.Type,
                ScheduleId = task.Schedule?.Id,
                Status = task.Status,
                DisallowConcurrentExecution = task.DisallowConcurrentExecution,
                NextRunTime = task.NextRunTime,
                RowVersion1 = task.RowVersion
            };
        }

        private RMTaskSchedule Convert2RMTaskSchedule(TaskSchedule schedule)
        {
            return new RMTaskSchedule()
            {
                Id = schedule.Id,
                Interval = schedule.Interval,
                IntervalType = schedule.IntervalType,
            };
        }
    }
}
