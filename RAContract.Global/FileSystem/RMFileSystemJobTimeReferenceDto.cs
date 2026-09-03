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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.FileSystem
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RMFileSystemJobTimeReferenceDto
    {
        [DataMember]
        public Guid ScopeId { set; get; }       

        [DataMember]
        public DateTime LastJobTime { get; set; }

        [DataMember]
        public string Path { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AgentSyncDataResultDto
    {
        [DataMember]
        public List<Guid> FailedGuids { set; get; }

        [DataMember]
        public List<Guid> SkippedGuids { get; set; }

      
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSDueRecordsDto
    {
        [DataMember]
        public List<FileSystemRecordDto> Records { get; set; }
        [DataMember]
        public SearchPageInfo PageInfo { get; set; }
    }
}
