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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobMonitorCommandType
    {
        /// <summary>
        /// 根据条件获得job列表
        /// </summary>
        [EnumMember]
        GetJobValues = 0,
        /// <summary>
        /// 根据JobId获取JobId的更新信息（保留暂时不做）
        /// </summary>
        [EnumMember]
        GetJobStates = 1,
        /// <summary>
        /// 删除Job的操作状态
        /// </summary>
        [EnumMember]
        DeleteJobValues = 2,
        /// <summary>
        /// 操作Ribbon的操作状态
        /// </summary>
        [EnumMember]
        OptionRibbon = 3,
        /// <summary>
        /// 获得Job详细信息
        /// </summary>
        [EnumMember]
        GetJobDetail = 4,
        /// <summary>
        /// 执行job的Pause操作
        /// </summary>
        [EnumMember]
        PauseJobAction = 6,
        /// <summary>
        /// 执行job的Resume操作
        /// </summary>
        [EnumMember]
        ResumeJobAction = 7,
        /// <summary>
        /// 执行job的Stop操作
        /// </summary>
        [EnumMember]
        StopJobAction = 8,
        /// <summary>
        /// 执行job的Start操作
        /// </summary>
        [EnumMember]
        StartJobAction = 9,
        /// <summary>
        /// 执行获取视图操作
        /// </summary>
        [EnumMember]
        GetView = 10,
        /// <summary>
        /// 执行删除job的一些相关内容，此操作不会删除Job和Job Detail等相关信息。
        /// </summary>
        [EnumMember]
        DeleteJobContent = 11,
        /// <summary>
        /// 执行rollback的相关操作
        /// </summary>
        [EnumMember]
        Rollback = 12,
        /// <summary>
        /// 执行Index的相关操作
        /// </summary>
        [EnumMember]
        Index = 13,
        /// <summary>
        /// 执行Restart的相关操作
        /// </summary>
        [EnumMember]
        Restart = 14,
        /// <summary>
        /// 执行Mapping的相关操作
        /// </summary>
        [EnumMember]
        Mapping = 15,
        /// <summary>
        /// 执行CopySnapShot的相关操作
        /// </summary>
        [EnumMember]
        CopySnapShot = 16,
        /// <summary>
        /// 执行Dead Account Deletion相关操作
        /// </summary>
        [EnumMember]
        DeadAccountDeletion = 17,
        /// <summary>
        /// 执行Search Result相关操作
        /// </summary>
        [EnumMember]
        SearchResult = 18,
        /// <summary>
        /// 执行Rollback Changes相关操作
        /// </summary>
        [EnumMember]
        RollbackChanges = 19,
        [EnumMember]
        ChangeStatus = 20,

        /// <summary>
        /// 执行Report Collect相关操作
        /// </summary>
        [EnumMember]
        CollectReport = 21,

        /// <summary>
        /// 执行Resync相关操作
        /// </summary>
        [EnumMember]
        Resync = 22,

        [EnumMember]
        RerunJobWithDebugMode = 23,

        /// <summary>
        /// 执行Download Search Result相关操作
        /// </summary>
        [EnumMember]
        DownloadSearchResults = 24,

        [EnumMember]
        RetryJobAction = 25,
    }
}
