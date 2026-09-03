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
using AvePoint.RA.Contract.BoxBrowser;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract(IsReference = true)]
    public class BoxTreeNodeDto
    {

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string RealId { get; set; }

        [DataMember]
        public string ContainerId { get; set; }

        [DataMember]
        public RMNodeLevel Level { get; set; }

        [DataMember]
        public string LeafName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string OwnerId { get; set; }

        [DataMember]
        public BoxTreeNodeDto Parent { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public RMNodeLevel StartJobNodeLevel { get; set; }

        [DataMember]
        public int PageIndex { get; set; }

        [DataMember]
        public List<string> ChildrenIds { get; set; }

        [DataMember]
        public List<BoxTreeNodeDto> Children { get; set; }

        [DataMember]
        public int ChildrenCount { get; set; }

        [DataMember]
        public int CheckNumber { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool Expanded { get; set; }
    }
}
