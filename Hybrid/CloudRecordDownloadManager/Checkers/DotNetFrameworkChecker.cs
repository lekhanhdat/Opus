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
using System.Threading;
using System.Threading.Tasks;
using CloudRecordDownloadManager.Modules;
using CloudRecordDownloadManager.Properties;
using Microsoft.Win32;

namespace CloudRecordDownloadManager.Checkers {

    public class DotNetFrameworkChecker : Checker {

        private const string VersionKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";

        public DotNetFrameworkChecker(ExaminationItem item, TaskScheduler taskScheduler) : base(item, taskScheduler) {
        }

        public override Task<ExaminationStatus> CheckTask() {
            var task = new Task<ExaminationStatus>(() => {
                Log.Info($"[{ClassName}] start");
                Log.Info($"[{ClassName}] expact: {ConstValue.DotNetFrameworkVersion} | {(int) ConstValue.DotNetFrameworkVersion}");
                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(VersionKey)) {
                    var result = ExaminationStatus.Error;
                    if (ndpKey != null) {
                        var releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                        Log.Info($"[{ClassName}] current dotnet framewrok version: {releaseKey}");
                        if (releaseKey > (int) ConstValue.DotNetFrameworkVersion) {
                            result = ExaminationStatus.Pass;
                            Log.Info($"[{ClassName}] passed");
                        } else {
                            Log.Error($"[{ClassName}] error");
                        }
                    } else {
                        Log.Error($"[{ClassName}] error, no dotnet framewrok found");
                    }

                    Task.Factory.StartNew(() => {
                        if (result == ExaminationStatus.Error) {
                            Item.TipMessage = string.Format(I18N.key_38a3784d_9749_4635_8459_a5f5fb11ed87);
                        }
                        Item.Status = result;
                    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
                    return result;
                }
            });
            return task;
        }

    }

}