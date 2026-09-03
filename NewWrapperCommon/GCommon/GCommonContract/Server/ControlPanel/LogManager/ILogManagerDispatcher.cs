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
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LogManager
{
    public interface ILogManagerDispatcher
    {
        /// <summary>
        /// Change max log file count, log level and max log file size.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>operation result</returns>
        [OperationContract]
        bool ChangeFileOptions(LogConfig config);
        /// <summary>
        /// 收集log.
        /// </summary>
        /// <param name="jobInfo"></param>
        [OperationContract]
        void CollectLogs(SubJobDto jobInfo);

        /// <summary>
        /// 按照jobid 收集其相应的service log
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="planCategoryAndJobIdsDic">key为panCategory即对应哪些模块，Vaule为该模块下的jobIds</param>
        [OperationContract]
        void CollectLogsByJobIds(SubJobDto jobInfo, Dictionary<int, List<string>> planCategoryAndJobIdsDic);

        [OperationContract]
        LogConfig GetLogSettings();
    }
}
