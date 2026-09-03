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
using System.Linq;
using System.Text;
namespace AvePoint.GCommon.Contract.Server.DataManager
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using System.Xml;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.AveLicense;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataManagerContactDto
    {
        public DataManagerContactDto()
        {
            this.StoragePolicyList = new List<StoragePolicyDto>();
            this.DataManagerTreeNodeList = new List<DataManagerTreeNodeDto>();
        }

        [DataMember]
        public int ModuleType { get; set; }

        [DataMember]
        public List<StoragePolicyDto> StoragePolicyList { get; set; }

        [DataMember]
        public List<DataManagerTreeNodeDto> DataManagerTreeNodeList { get; set; }

        [DataMember]
        public int RetentionExtendType { get; set; }

        [DataMember]
        public int RetentionExtendValue { get; set; }

        [DataMember]
        public bool IsUpdateCompression { get; set; }

        [DataMember]
        public int CompressionType { get; set; }

        [DataMember]
        public bool IsUpdateDerferCompression { get; set; }

        [DataMember]
        public int DeferCompressionValue { get; set; }

        [DataMember]
        public bool IsIndex { get; set; }

        [DataMember]
        public bool IsIndexValue { get; set; }

        [DataMember]
        public bool IsShred { get; set; }

        [DataMember]
        public bool IsShredValue { get; set; }

        [DataMember]
        public bool IsHold { get; set; }

        [DataMember]
        public bool IsDeleteData { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public ProductType ProductType { get; set; }
    }
}