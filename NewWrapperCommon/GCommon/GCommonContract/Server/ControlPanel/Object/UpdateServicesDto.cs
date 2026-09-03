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
using AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateServicesDto
    {
        /// <summary>
        /// key : address
        /// value : services
        /// </summary>
        [DataMember]
        public Dictionary<string, List<ServiceDto>> ManagerService { get; set; }

        /// <summary>
        /// key : farmName
        /// value : services
        /// </summary>
        [DataMember]
        public Dictionary<string, List<ServiceDto>> AgentService { get; set; }

        /// <summary>
        /// key : address
        /// values : services
        /// </summary>
        [DataMember]
        public Dictionary<string, List<ServiceDto>> GAService { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceInfoWithUpdateHistory
    {
        /// <summary>
        /// key.displayname : address
        /// value : services
        /// </summary>
        [DataMember]
        public Dictionary<DisplayNameWithStatus, List<ServiceDto>> ManagerService { get; set; }

        /// <summary>
        /// key.display : farmName
        /// value : services
        /// </summary>
        [DataMember]
        public Dictionary<DisplayNameWithStatus, List<ServiceDto>> AgentService { get; set; }

        [DataMember]
        public Dictionary<DisplayNameWithStatus, List<ServiceDto>> GAService { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DisplayNameWithStatus
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public InstallStatusHistory Status { get; set; }
    }
}
