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
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AvePoint.RA.Web.Models.PRM
{
    [DataContract]
    public class PhysicalHoldValidateBoxParam
    {
        [DataMember]
        public List<Guid> NodeId { set; get; }
        [DataMember]
        public NodeType NodeType { set; get; }
    }
    [DataContract]
    public class PhysicalHoldValidateBoxResult
    {
        [DataMember]
        public bool HasChildrenHold { set; get; }
        [DataMember]
        public List<string> FolderNames { set; get; }
    }
    [DataContract]
    public class ColumnParam
    {
        [DataMember]
        public int typeId { set; get; }
        [DataMember]
        public string columnName { set; get; }
    }
}