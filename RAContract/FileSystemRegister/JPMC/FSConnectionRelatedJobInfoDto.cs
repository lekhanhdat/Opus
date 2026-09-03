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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.FileSystemRegister.JPMC
{
    [DataContract]
    public class FSConnectionRelatedJobInfoDto
    {
        [DataMember(EmitDefaultValue = false)]
        public string JobId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int JobType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string JobRunBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ConnectionGroupName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Path { get; set; } //Connection UNCPath/DFSPath

        [DataMember(EmitDefaultValue = false)]
        public int Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string StartTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EndTime { get; set; }
    }

    [DataContract]
    public class FSConnectionMonitorResultData
    {
        [DataMember(EmitDefaultValue = false)]
        public List<FSConnectionRelatedJobInfoDto> ConnectionMonitorList { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? TotalCount { get; set; }
    }
  
    [DataContract]
    public class FSConnectionMonitorQueryPager
    {
        [DataMember]
        public Guid ConnectionId { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public string SearchKey { get; set; }

        [DataMember]
        public List<FSConnectionMonitorFilter> Filters { get; set; }

        [DataMember]
        public FSConnectionMonitorOrder Order { get; set; }
    }

    [DataContract]
    public class FSConnectionMonitorFilter
    {
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }

    [DataContract]
    public class FSConnectionMonitorOrder
    {
        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public bool IsDesc { get; set; }
    }
}
