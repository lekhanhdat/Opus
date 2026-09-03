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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDOffLineSearchResultPagingRequest : EDiscoveryRequest
    {

        [DataMember]
        public int EveryPageCount { get; set; }

        [DataMember]
        public int CurrentPage { get; set; }

        //        [DataMember]
        //        public string OrderStr { get; set; }

        [DataMember]
        public OrderColumn OrderColumn { get; set; }

        [DataMember]
        public OrderType OrderType { get; set; }

        [DataMember]
        public string SearchKeyword { get; set; }

        [DataMember]
        public SharePointType TypeFilter { get; set; }

        [DataMember]
        public string SearchJobId { get; set; }

        [DataMember]
        public ActionEnum Action { get; set; }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ActionEnum
        {
            [EnumMember]
            IntoOffLineSearchResultPage = 0,
            [EnumMember]
            GetPaging = 1
        }



    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OrderColumn
    {
        [EnumMember]
        Id = 0,
        [EnumMember]
        FileItemName = 1,
        [EnumMember]
        Version = 2,
        [EnumMember]
        Size = 3,
        [EnumMember]
        Location = 4,
        [EnumMember]
        CreateBy = 5,
        [EnumMember]
        LastModifyTime = 6
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OrderType
    {
        [EnumMember]
        ASC = 0,
        [EnumMember]
        DESC = 1
    }

}
