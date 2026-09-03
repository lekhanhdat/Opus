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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SolutionManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ResponseMessage
    {
        [DataMember]
        public SolutionAction Action { get; set; }

        [DataMember]
        public int WfeCount { get; set; }

        [DataMember]
        public string AgentId { get; set; }

        [DataMember]
        public string SolutionId { get; set; }

        [DataMember]
        public string SolutionName { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string FileVersion { get; set; }

        [DataMember]
        public SolutionInfo SolutionInfo { get; set; }

        [DataMember]
        public bool IsCa { get; set; }

        [DataMember]
        public bool IsWFE { get; set; }

        [DataMember]
        public List<WebAppInfoDto> WebAppInfos { get; set; }
    }

}