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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateProgressDto
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string UpdateName { get; set; }

        [DataMember]
        public string ServiceVersion { get; set; }

        [DataMember]
        public string PatchVersion { get; set; }

        [DataMember]
        public string PatchDisplayVersion { get; set; }

        [DataMember]
        public string HostIP { get; set; }

        [DataMember]
        public ServiceType Type { get; set; }

        [DataMember]
        public InstallStatus Status { get; set; }

        [DataMember]
        public long Index { get; set; }

        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public string Detail { get; set; }

        [DataMember]
        public string PatchFileName { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckInstallStatusDto
    {
        [DataMember]
        public bool IsFinish { get; set; }

        [DataMember]
        public bool IsSuccess { get; set; }
    }
}
