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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudRecordDownloadManager.Cache;
using CloudRecordDownloadManager.Modules;
using CloudRecordDownloadManager.Properties;

namespace CloudRecordDownloadManager.Checkers {

    public class DiskSpaceChecker : Checker {

        public DiskSpaceChecker(ExaminationItem item, TaskScheduler taskScheduler) : base(item, taskScheduler) {
        }

        public override Task<ExaminationStatus> CheckTask() {
            var task = new Task<ExaminationStatus>(() => {
                Log.Info($"[{ClassName}] start");
                Log.Info($"[{ClassName}] expact: {ConstValue.DiskFreeSpaceLimit}GB");
                var root = Path.GetPathRoot(RuntimeCache.InstallPath);
                var result = ExaminationStatus.Error;
                try {
                    var drive = DriveInfo.GetDrives().First(d => d.Name == root);
                    var leftGB = drive.TotalFreeSpace / ConstValue.BytesPerGB;
                    if (leftGB >= ConstValue.DiskFreeSpaceLimit) {
                        result = ExaminationStatus.Pass;
                        Log.Info($"[{ClassName}] passed");
                    } else {
                        Log.Error($"[{ClassName}] error");
                    }
                } catch (Exception e) {
                    Log.Error(e, $"[{ClassName}] error, cannot read drive left space: {root}");
                }

                Task.Factory.StartNew(() => {
                    if (result == ExaminationStatus.Error) {
                        Item.TipMessage = string.Format(I18N.key_6f5a6290_b14d_4104_9061_a0a27b6e0209, ConstValue.DiskFreeSpaceLimit);
                    }
                    Item.Status = result;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);

                return result;
            });
            return task;
        }

    }

}