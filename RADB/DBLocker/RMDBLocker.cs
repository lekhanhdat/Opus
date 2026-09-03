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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.DBLocker
{
    public class RMDBlLocker
    {
        private static readonly RALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static int sleepTime = 50;
        private static int timeoutlength = 1000 * 60 * 5;//Timeout的意思是说，如果超过这个时间仍然不能get到locker的话，则强行Get。（也就是执行SQL Update）目前默认两分钟。

        private static string processName = string.Empty;
        private static int processId = 0;
        private static string computername = string.Empty;
        
        public static void Initialize()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                processName = process.ProcessName;
                processId = process.Id;
                computername = RMGlobalConfiguration.EnvSetting.RoleId;
            }
            catch (Exception ex)
            {
                mLog.Error("An error occurred while getting the process information. " + ex.ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lockKey"></param>
        /// <returns></returns>
        private static async Task<long> GetLockerAsync(string lockKey)
        {
            try
            {
                IRMLockDao RecordLock = PlatformWindsorManager.GetService(typeof(IRMLockDao)) as IRMLockDao;
                long lastID = 0;
                long currentID = 0;
                byte[] rowVersion;
                while (true)
                {
                    DateTime timeStamp = DateTime.MinValue;
                    int status = -1;
                    var lockObj = RecordLock.GetLockerRecord(lockKey, out timeStamp, out status, out currentID, out rowVersion);
                    if (lockObj == null)
                    {
                        try
                        {
                            lockObj = new RMLock { TenantGroupId = lockKey, RecordId = 1, ProcessName = processName, Status = 1, ProcessId = processId, ThreadId = Thread.CurrentThread.ManagedThreadId, ComputerName = computername, UpdateTime = DateTime.Now };
                            RecordLock.InserLockerRecord(lockObj);
                            return 1;
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Error occurred while creating the locker. " + ex.ToString());
                            var lockObj1 = RecordLock.GetLockerRecord(lockKey, out timeStamp, out status, out currentID, out rowVersion);
                            if (lockObj1 != null)
                            {
                                //this means failed to get locker, 
                                //maybe there is another program is getting locker at the same time, 
                                //need wait sometime and then try again.

                                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));
                                continue;
                                //not finish the coding, need more code for try again.
                            }
                            else
                            {
                                //Something wrong with the Locker logic.
                                return 0;
                            }
                        }
                    }
                    else//find the locker in the database.
                    {
                        if (status == 0 || (lastID == currentID && lastID != 0 && DateTime.Now.AddMilliseconds(-timeoutlength) > timeStamp))
                        {
                            if (status != 0)
                            {
                                mLog.Warn("Update and get locker forcely.");
                            }
                            lastID = currentID;

                            long tempId = currentID + 1;
                            //var updatelockObj = new RMLock { TenantGroupId = TenantLocalValue.LogonGroupId, Status = 1, ProcessId = processId, ThreadId = Thread.CurrentThread.ManagedThreadId, ComputerName = computername, UpdateTime = DateTime.Now };
                            lockObj.Status = 1; lockObj.RecordId = tempId; lockObj.ProcessId = processId; lockObj.ThreadId = Thread.CurrentThread.ManagedThreadId; lockObj.ComputerName = computername; lockObj.UpdateTime = DateTime.Now;
                            if (await RecordLock.UpdateLockerRecordAsync(lockObj))
                            {
                                return tempId;
                            }
                            else
                            {
                                //mLog.Debug("Fail to update, need try again. ");
                                //need wait and try again.
                                //this means there is another program is getting locker at the same time,  
                                //Thread.Sleep(sleepTime);
                                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));
                                continue;
                            }
                        }
                        else
                        {
                            //mLog.Debug("Locker is being used, need wait and try again. ");
                            lastID = currentID;
                            //need wait and try again.
                            //this means the locker is owned by another program.
                            //Thread.Sleep(sleepTime);
                            await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Error occurred while getting the locker. " + ex.ToString());
                return 0;
            }
        }
        private static Task ReleaseLockerAsync(string lockKey, long id)
        {
            IRMLockDao RecordLock = PlatformWindsorManager.GetService(typeof(IRMLockDao)) as IRMLockDao;
            //RMLock releaseLock = new RMLock { TenantGroupId = TenantLocalValue.LogonGroupId, RecordId = id, ProcessName = "", Status = 0, ProcessId = 0, ThreadId = 0, ComputerName = "", UpdateTime = DateTime.Now };
            var lockObj = RecordLock.GetLockerRecord(lockKey);
            lockObj.Status = 0; lockObj.ProcessId = 0; lockObj.ThreadId = 0; lockObj.ComputerName = ""; lockObj.UpdateTime = DateTime.Now;
            return RecordLock.UpdateLockerRecordAsync(lockObj);
        }

        public static async Task<long> GetIdAsync(string lockKey)
        {

            long id = await GetLockerAsync(lockKey);
            await ReleaseLockerAsync(lockKey, id);
            return id;
        }

        public static async Task ReleaseRecordsLockerAsync(string objectName)
        {
            IRMDeclaredSettingLockDao RecordSettingLock = PlatformWindsorManager.GetService(typeof(IRMDeclaredSettingLockDao)) as IRMDeclaredSettingLockDao;
            var lockObj = RecordSettingLock.GetLockerRecord(objectName);
            lockObj.Status = 0; lockObj.ProcessId = 0; lockObj.ThreadId = 0; lockObj.ComputerName = ""; lockObj.UpdateTime = DateTime.Now;
            await RecordSettingLock.UpdateLockerRecordAsync(lockObj);
        }
        public static async Task<bool> GetRecordsLockerAsync(string objectName)
        {
            try
            {
                IRMDeclaredSettingLockDao RecordSettingLock = PlatformWindsorManager.GetService(typeof(IRMDeclaredSettingLockDao)) as IRMDeclaredSettingLockDao;
                byte[] rowVersion;
                while (true)
                {
                    DateTime timeStamp = DateTime.MinValue;
                    int status = -1;
                    var lockObj = RecordSettingLock.GetLockerRecord(objectName, out timeStamp, out status, out rowVersion);
                    if (lockObj == null)
                    {
                        try
                        {
                            lockObj = new RMDeclaredSettingLock { ObjectName = objectName, ID = Guid.NewGuid(), ProcessName = processName, Status = 1, ProcessId = processId, ThreadId = Thread.CurrentThread.ManagedThreadId, ComputerName = computername, UpdateTime = DateTime.Now };
                            RecordSettingLock.InserLockerRecord(lockObj);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Error occurred while creating the locker. " + ex.ToString());
                            var lockObj1 = RecordSettingLock.GetLockerRecord(objectName, out timeStamp, out status, out rowVersion);
                            if (lockObj1 != null)
                            {
                                //this means failed to get locker, 
                                //maybe there is another program is getting locker at the same time, 
                                //need wait sometime and then try again.

                                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));
                                continue;
                                //not finish the coding, need more code for try again.
                            }
                            else
                            {
                                //Something wrong with the Locker logic.
                                return false;
                            }
                        }
                    }
                    else//find the locker in the database.
                    {
                        if (status == 0 || DateTime.Now.AddMilliseconds(-timeoutlength) > timeStamp)
                        {
                            if (status != 0)
                            {
                                mLog.Warn("Update and get locker forcely.");
                            }
                            //var updatelockObj = new RMLock { TenantGroupId = TenantLocalValue.LogonGroupId, Status = 1, ProcessId = processId, ThreadId = Thread.CurrentThread.ManagedThreadId, ComputerName = computername, UpdateTime = DateTime.Now };
                            lockObj.Status = 1;
                            lockObj.ProcessId = processId;
                            lockObj.ThreadId = Thread.CurrentThread.ManagedThreadId;
                            lockObj.ComputerName = computername;
                            lockObj.UpdateTime = DateTime.Now;
                            if (await RecordSettingLock.UpdateLockerRecordAsync(lockObj))
                            {
                                return true;
                            }
                            else
                            {
                                //mLog.Debug("Fail to update, need try again. ");
                                //need wait and try again.
                                //this means there is another program is getting locker at the same time,  
                                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));

                                continue;
                            }
                        }
                        else
                        {
                            //mLog.Debug("Locker is being used, need wait and try again. ");
                            //need wait and try again.
                            //this means the locker is owned by another program.
                            await Task.Delay(TimeSpan.FromMilliseconds(sleepTime));

                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Error occurred while getting the locker. " + ex.ToString());
                return false;
            }
        }
    }
}
