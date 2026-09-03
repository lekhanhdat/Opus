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

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchFilter
    {
        [DataMember]
        public FilterType FilterType { get; set; }
        [DataMember]
        public Condition Condition { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public bool IsAnd { get; set; }
        [DataMember]
        public string UserName { set; get; }
        [DataMember]
        public string PassWord { set; get; }

        public override string ToString()
        {
            return string.Format("FilterType {0}, Condition {1}, Value {2}, IsAnd {3}, UserName {4}"
                , FilterType, Condition, Value, IsAnd, UserName);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterType
    {
        [EnumMember]
        Domain = 0,
        [EnumMember]
        IPV4Range =1,
        [EnumMember]
        HostName = 2,
        [EnumMember]
        ServerName = 3,
        [EnumMember]
        ADOUName = 4,
        [EnumMember]
        Description = 5,
        [EnumMember]
        ManagedBy = 6,
        [EnumMember]
        OS = 7, 
        [EnumMember]
        OSVersion = 8,
        [EnumMember]
        ComputerName = 9,
        [EnumMember]
        Name = 10,
        [EnumMember]
        SiteCollection = 11,
        [EnumMember]
        Operation = 12
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Condition
    {
        [EnumMember]
        MatchesExactly = 0,
        [EnumMember]
        DoesNotMatch = 1,
        [EnumMember]
        Contains = 2,
        [EnumMember]
        DoesNotContain = 3,
        [EnumMember]
        Between = 4
    }
}
