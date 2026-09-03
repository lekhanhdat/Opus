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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager
{
    /// <summary>
    /// 各个服务实现此接口,用来接收Patch文件和执行启动Installer的命令
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IADeployPatchService
    {
        /// <summary>
        /// 各个服务实现此方法，用来接收Patch文件，调用PatchDeployUtil.ReceivePatch方法实现
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        [OperationContract]
        ReturnResult DeployPatch(string sessionId, ServiceDto service);

        [OperationContract]
        void StartPatchInstallerWithArguments(string arguments);

        [OperationContract]
        bool CheckDiskSpaceIsFree(double needSpace);

    }
}
