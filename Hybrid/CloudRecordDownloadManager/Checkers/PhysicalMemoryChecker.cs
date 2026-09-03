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
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using CloudRecordDownloadManager.Modules;
using CloudRecordDownloadManager.Properties;

namespace CloudRecordDownloadManager.Checkers {

    public class PhysicalMemoryChecker : Checker {

        public PhysicalMemoryChecker(ExaminationItem item, TaskScheduler taskScheduler) : base(item, taskScheduler) {
        }

        public override Task<ExaminationStatus> CheckTask() {
            var task = new Task<ExaminationStatus>(() => {
                Log.Info($"[{ClassName}] start");
                Log.Info($"[{ClassName}] expact: {ConstValue.MemoryLimit}GB");
                var memory = GetPhysicalMemory();
                var gb = memory / ConstValue.BytesPerGB;
                Log.Info($"[{ClassName}] total memory: {memory} | {gb:F1}GB");
                var result = ExaminationStatus.Pass;
                if (gb >= ConstValue.MemoryLimit) {
                    Log.Info($"[{ClassName}] passed");
                } else {
                    Log.Error($"[{ClassName}] error");
                    result = ExaminationStatus.Error;
                }

                Task.Factory.StartNew(() => {
                    if (result == ExaminationStatus.Error) {
                        Item.TipMessage = string.Format(I18N.key_8f862b95_5033_4680_9b5d_42a3309d052c, ConstValue.MemoryLimit);
                    }

                    Item.Status = result;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
                return result;
            });
            return task;
        }

        private static long GetPhysicalMemory() {
            var st = string.Empty;
            var mc = new ManagementClass("Win32_ComputerSystem");
            var moc = mc.GetInstances();
            foreach (var o in moc) {
                var mo = (ManagementObject) o;
                st = mo["TotalPhysicalMemory"].ToString();
            }

            var parsed = long.TryParse(st, out var res);

            return parsed ? res : 0;
        }

    }

}