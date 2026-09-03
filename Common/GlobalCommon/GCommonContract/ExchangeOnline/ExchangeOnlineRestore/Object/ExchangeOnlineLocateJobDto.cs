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
namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineLocateJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string SearchFilterPolicy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExchangeOnlineLocateJobResultType
    {
        [EnumMember]
        All = 0,

        [EnumMember]
        Partial = 1,

        [EnumMember]
        NoResult = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOCreateLocateJobError
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        ExistRunningJob = 1,

        [EnumMember]
        NoMatchRecords = 2,

        [EnumMember]
        Exception = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOCreateLocateJobResult
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public EOCreateLocateJobError Error { get; set; }
    }
}
