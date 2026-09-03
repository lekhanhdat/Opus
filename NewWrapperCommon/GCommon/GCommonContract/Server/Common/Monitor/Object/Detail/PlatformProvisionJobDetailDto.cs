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

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    #region using directives
    using AvePoint.GCommon.Contract.Common;
    using System.Runtime.Serialization;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlatformProvisionJobDetailDto : JobDetailDto
    {
        [DataMember]
        public string SourceComponentName { get; set; }

        [DataMember]
        public string SourceContainer { get; set; }

        [DataMember]
        public string SourceServer { get; set; }

        [DataMember]
        public string WFAProfileName { get; set; }

        [DataMember]
        public string WFAJobNumber { get; set; }

        [DataMember]
        public string DestinationComponentName { get; set; }

        [DataMember]
        public string DestinationContainer { get; set; }

        [DataMember]
        public string DestinationServer { get; set; }

        [DataMember]
        public string Protocol { get; set; }

        [DataMember]
        public long StartTime { get; set; }
        
        [DataMember]
        public long FinishTime { get; set; }
        
        [DataMember]
        public string TotalTime { get; set; }
        
        #region SnapMirror Provision and SnapMirror Discover
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DataType { get; set; }
        
        [DataMember]
        public string StorageSystem { get; set; }

        [DataMember]
        public string Customized { get; set; }

        [DataMember]
        public string DestinationStorageSystem { get; set; }

        [DataMember]
        public string DestinationAggregate { get; set; }
        #endregion
    }
}
