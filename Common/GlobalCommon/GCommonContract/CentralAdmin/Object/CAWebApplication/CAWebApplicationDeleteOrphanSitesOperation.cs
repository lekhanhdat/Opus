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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationDeleteOrphanSitesOperation : CAOperation
    {
        [DataMember]
        public String GetJobId { get; set; }

        [DataMember]
        public Boolean IsVerify { get; set; }

        [DataMember]
        public List<OrphanSiteInfo> OrphanSiteInfos { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OrphanSiteInfo
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String Url { get; set; }

        [DataMember]
        public String DatabaseName { get; set; }

        [DataMember]
        public String DatabaseServerName { get; set; }

        [DataMember]
        public Guid DatabaseId { get; set; }

        [DataMember]
        public ResultStatus Status { get; set; }

        [DataMember]
        public String Comment { get; set; }
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OrphanSitesResult : ResultBase
    {
        [DataMember]
        public String JobID { get; set; }

        [DataMember]
        public Int32 TotalCount { get; set; }

        [DataMember]
        public List<OrphanSiteInfo> OrphanSiteInfos { get; set; }
    }

    [DataContract(Namespace = (ContractConstants.Namespace))]
    public enum ResultStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        Failed,
        [EnumMember]
        Succeed

    }
}
