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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeleteJobDto : BaseJobParamDto
    {
        [DataMember]
        public SearchJobDto searchDto { get; set; }
        /// <summary>
        /// 准备要删除的Job
        /// </summary>
        [DataMember]
        public Dictionary<DeleteType, List<BaseJobDto>> JobsForDelete { get; set; }
    }
    
    [DataContract]
    public enum DeleteType
    {
        /// <summary>
        /// 直接删Job记录
        /// </summary>
        [EnumMember]
        Normal = 0,
        /// <summary>
        ///需要起Retention job删backup数据以及job记录
        /// </summary>
        [EnumMember]
        BackupData = 1,
        /// <summary>
        /// 起retention job 删 job相关的tempDB以及job记录
        /// </summary>
        [EnumMember]
        TempDB = 2,
    }
}
