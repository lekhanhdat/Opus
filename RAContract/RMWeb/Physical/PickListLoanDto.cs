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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    [DataContract]
    public class PickListLoanDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public int NodeType { get; set; }
        [DataMember]
        public string RecordName { get; set; }
        [DataMember]
        public string UniqueId { get; set; }
        [DataMember]
        public string Requestor { get; set; }

        //public string RequestedDate { get; set; }
        [DataMember]
        public string HomeLocation { get; set; }

        //public string CurrentHeldBy { get; set; }
        [DataMember]
        public int Status { get; set; }

    }
    [DataContract]
    public class PickListLoanResultDto
    {
        [DataMember]
        public List<PickListLoanDto> List { get; set; }
        [DataMember]
        public int TotalCount { set; get; }
        [DataMember]
        public string PageIndex { get; set; }
    }
    [DataContract]
    public class PickListLoanParam
    {
        [DataMember]
        public string SearchText { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public PickFilterOption FilterOptions { get; set; }
    }

    #region Common Dto
    [DataContract]
    public enum PickStatusType
    {
        [EnumMember]
        Pendding = 0,
        [EnumMember]
        Complete = 1
    }

    public class PickFilterOption
    {
        public List<PickStatusType> Status { get; set; }
    }
    [DataContract]
    public class CompleteActionParam
    {
        [DataMember]
        public bool IsSelectAll { get; set; }
        [DataMember]
        public bool IsContainerLevel { get; set; }
        [DataMember]
        public List<Guid> SelectedItemIds { get; set; }
        [DataMember]
        public string SearchText { get; set; }
        [DataMember]
        public PickFilterOption FilterOptions { get; set; }
    }

    public class PickListJobMessage
    {
        public CompleteActionParam ActionParam { get; set; }
        public string LogonUserId { get; set; }
    }

    public enum PickObjectType
    {
        Loan = 0,
        Destruction = 1,
        ReturnHistory = 2,
        Move = 3
    }

    public enum PickActionType
    {
        Complete = 0,
        Export = 1
    }

    public class PickListStartJobDto
    {
        public PickObjectType ObjectType { get; set; }
        public PickActionType PickActionType { get; set; }
    }
    #endregion
}
