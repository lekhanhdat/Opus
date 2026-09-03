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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudRecordDownloadManager.Modules;
using CloudRecordDownloadManager.Properties;
using CloudRecordDownloadManager.Utils.FTP;

namespace CloudRecordDownloadManager.Checkers {

    public class NetworkChecker : Checker {

        public NetworkChecker(ExaminationItem item, TaskScheduler taskScheduler) : base(item, taskScheduler) {
        }

        public override Task<ExaminationStatus> CheckTask() {
            var task = new Task<ExaminationStatus>(() => {
                Log.Info($"[{ClassName}] start");
                Log.Info($"[{ClassName}] host: {ConstValue.NetworkHost}");
                var result = ExaminationStatus.Error;
                // try {
                //     var existed = FtpUtility.FtpExisted(new Uri(ConstValue.Host), out _);
                //     if (!existed) {
                //         result = ExaminationStatus.Error;
                //         Log.Error($"[{ClassName}] error");
                //     }
                // } catch (Exception e) {
                //     Log.Error(e, $"[{ClassName}] error");
                // }
                //
                // Log.Info($"[{ClassName}] passed");
                // Task.Factory.StartNew(() => Item.Status = result, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);

                for (var i = 3; i > 0; i--) {
                    try {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                        var myRequest = WebRequest.Create(ConstValue.NetworkHost);
                        myRequest.Timeout = 1000 * 3;
                        var myResponse = myRequest.GetResponse();
                        myResponse.Close();
                        result = ExaminationStatus.Pass;
                        break;
                    } catch (Exception e) {
                        Log.Error(e, $"[{ClassName}] error and retry left: {i - 1}");
                    }
                }

                if (result == ExaminationStatus.Error) {
                    Log.Error($"[{ClassName}] error");
                } else {
                    Log.Info($"[{ClassName}] passed");
                }
                Task.Factory.StartNew(() => {
                    if (result == ExaminationStatus.Error) {
                        Item.TipMessage = string.Format(I18N.key_8e2ebd87_931c_4d40_9571_52cdc408e63a);
                    }
                    Item.Status = result;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);

                return result;
            });
            return task;
        }

    }

}