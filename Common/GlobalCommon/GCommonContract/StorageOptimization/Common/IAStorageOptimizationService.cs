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





using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Common
{
    /// <summary>
    /// 此接口以后将作为StorageOptimization模块Manager调Agent的WCF Service使用.
    /// 所有Agent的WCF Service只需实现此接口.
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAStorageOptimizationService
    {
        /// <summary>
        /// Agent端WCF Service中只需实现HandleMessage方法,此方法的参数和返回值都是SOMessage.
        /// 所有需要传递给agent的具体参数,都以SOMessage的属性传递.
        /// 在SOMessage中需要赋值SOAction,Agent端根据SOAction判断具体调Agent的哪个方法.
        /// </summary>
        /// <param name="msg">
        /// 1.SOMessge中AgentInfo和Action为必传属性.
        /// 2.其中根据具体方法Manger和Agent协商定义.
        /// </param>
        /// <returns>
        /// 1.如果方法不需要返回值,可以New一个SOMessage返回即可.
        /// 2.如果需要返回成功失败,可以为SOMessage中MessageType枚举赋值.
        /// 3.如果还想返回具体错误信息可以为SOMessage中ErrorMessage中赋值.
        /// </returns>
        [OperationContract]
        SOMessage HandleMessage(SOMessage msg);
    }
}
