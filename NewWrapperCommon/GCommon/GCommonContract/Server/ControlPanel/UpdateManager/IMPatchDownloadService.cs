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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager
{
    /// <summary>
    /// Patch下载功能接口
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMPatchDownloadService
    {
        /// <summary>
        /// 获取下载任务状态对象,包括全局速度,下载中任务及下载完成任务
        /// 注:如没有下载中任务或下载完成任务则数组为null.
        /// </summary>
        /// <returns>DownloadStatusModel</returns>
        [OperationContract]
        DownloadStatusModel GetDownloadStatusModel();

        /// <summary>
        /// 通过任务key获取任意任务对象.不存在返回null.不建议在Timer中调用此方法.
        /// </summary>
        [OperationContract]
        TaskDto GetTask(string key);

        /// <summary>
        /// 启动下载任务.
        /// </summary>
        [OperationContract]
        void Do(string key);

        /// <summary>
        /// 暂停任务下载.状态在引擎中保持.
        /// </summary>
        [OperationContract]
        void Pause(string key);

        /// <summary>
        /// 抛弃下载任务.无论是何状态.会删除任务文件及签名信息.
        /// </summary>
        /// <param name="key"></param>
        [OperationContract]
        void Drop(string key);

        [OperationContract]
        void DropMore(string[] keys);

        /// <summary>
        /// 创建并启动下载任务. 要保证dto中key的唯一性.
        /// </summary>
        [OperationContract]
        TaskDto Create(TaskSignDto dto);

        /// <summary>
        /// 创建并启动下载任务.
        /// </summary>
        [OperationContract]
        List<TaskDto> CreateMore(List<TaskSignDto> dtos);
    }
}
