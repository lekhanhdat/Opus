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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    [DataContract]
    public class PhysicalExplorerQueryDto
    {
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public RMNodeLevel CurrentNodeType { get; set; }
        [DataMember]
        public PhysicalExplorerFilterOption FilterOption { get; set; }
        [DataMember]
        public PhysicalExplorerPagingInfo PagingInfo { get; set; }
        [DataMember]
        public List<int> PermissionIds { get; set; }
        [DataMember]
        public bool HaveCurrentNodePermission { get; set; }
    }
    [DataContract]
    public class PhysicalExplorerFilterOption
    {
        [DataMember]
        public string SearchKey { get; set; }
        [DataMember]
        public List<string> RecordsOwner { get; set; }
        [DataMember]
        public List<string> CreatedBy { get; set; }
        [DataMember]
        public List<string> ModifiedBy { get; set; }
        [DataMember]
        public RMNodeLevel NodeType { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public Guid TermTreeFilter { get; set; }
    }
}
