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



namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobContextDto
    {
        /// <summary>
        /// 对应的jobId或者subjobid
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        /// 保存job resume/restart时需要的信息
        /// </summary>
        [DataMember]
        public string AgentContext { get; set; }


        /// <summary>
        /// 保存job resume/restart时需要的信息
        /// </summary>
        [DataMember]
        public string PlanSettings { get; set; }

        /// <summary>
        /// 根据各个模块的需要, 保存各个模块独特的与job相关的信息.如：Item模块,保存每个subJob里备份的SiteCollection节点;
        /// </summary>
        public string Content { get; set; }

    }
}
