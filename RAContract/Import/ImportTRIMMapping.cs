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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Import
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RecordTypeMapping
    {
        [DataMember]
        public string SrcRecordType { set; get; }

        [DataMember]
        public string DestTemplateType { set; get; }

        [DataMember]
        public string DestTemplateName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnMapping
    {
        [DataMember]
        public int RecordType { set; get; }

        [DataMember]
        public List<ColumnMappingDetail> Details { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnMappingDetail
    {
        [DataMember]
        public string SrcName { set; get; }
        [DataMember]
        public string DestName { set; get; }
        [DataMember]
        public string ColumnType { set; get; }
        [DataMember]
        public string MustHave { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnValueMapping
    {
        [DataMember]
        public string RecordType { set; get; }
        [DataMember]
        public string SrcColumn { set; get; }
        [DataMember]
        public string DescColumn { set; get; }
        [DataMember]
        public string SrcValue { set; get; }
        [DataMember]
        public string DestValue { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserMapping
    {
        [DataMember]
        public string SrcUserName { set; get; }

        [DataMember]
        public string DestEmailAddress { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ImportGeneralSetting
    {
        [DataMember]
        public string DateTimeFormate { set; get; }
        [DataMember]
        public string DateFormate { set; get; }
        [DataMember]
        public string TimeZone { set; get; }
        [DataMember]
        public double DefaultBoxSize { set; get; }
        [DataMember]
        public double DefaultLocaionSize { set; get; } 
    }

    public enum ImportProfileType
    {
        None = 0,
        RecordTypeMapping = 1,
        BoxColumnMapping = 2,
        FolderColumnMapping = 3,
        RecordColumnMapping = 4,
        ColumnValueMapping = 5,
        UserMapping = 6,
        GeneralSetting = 7
    }
}
