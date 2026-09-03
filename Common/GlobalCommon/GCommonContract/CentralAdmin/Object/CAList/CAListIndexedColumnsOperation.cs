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






namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListIndexedColumnsOperation : CAOperation
    {
        [DataMember]
        public List<CAListIndexItemOperation> IndexItems { get; set; }

        [DataMember]
        public Dictionary<CAListColumn, List<CAListColumn>> Columns { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListIndexItemOperation
    {
        [DataMember]
        public CAListColumn Primary { get; set; }

        [DataMember]
        public CAListColumn Secondary { get; set; }

        [DataMember]
        public string Id { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListColumn
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public CompositeIndexableStatus IndexableStatus { get; set; }

        [DataMember]
        public string InternalName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CompositeIndexableStatus
    {
        [EnumMember]
        NotCompositeIndexable,
        [EnumMember]
        AsLastFieldInIndex,
        [EnumMember]
        AsAnyFieldInIndex
    }
}
