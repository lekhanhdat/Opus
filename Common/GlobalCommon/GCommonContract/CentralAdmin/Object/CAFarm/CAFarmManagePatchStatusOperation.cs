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
    #region
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmManagePatchStatusOperation : CAOperation
    {
        [DataMember]
        public List<SharePointServerProducts> SharePointServerProducts { get; set; }


        //public SharePointServerProducts GetSharePointServerProductsInfoByServerName(String serverName)
        //{
        //    var result = default(SharePointServerProducts);
        //    if (this.SharePointServerProducts != null)
        //        result = this.SharePointServerProducts.Find(item => item.ServerName == serverName);
        //    return result;
        //}


        //public List<String> GetServerList()
        //{
        //    var result = default(List<String>);
        //    if (this.SharePointServerProducts != null)
        //        result = this.SharePointServerProducts.ConvertAll<String>(item => item.ServerName);
        //    return result;
        //}
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointServerProducts
    {
        [DataMember]
        public String ServerName { get; set; }

        [DataMember]
        public String ServerDisplayName { get; set; }

        [DataMember]
        public String ServerId { get; set; }

        [DataMember]
        public List<SharePointProductInfo> SharePointProductInfos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointProductInfo
    {
        [DataMember]
        public String ProductName { get; set; }

        [DataMember]
        public List<SharePointPatchInfo> SharePointProductInfos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointPatchInfo
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String PatchName { get; set; }

        [DataMember]
        public String Version { get; set; }

        [DataMember]
        public PatchStatusType InstallStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PatchStatusType
    {
        [EnumMember]
        InstallRequired,

        [EnumMember]
        UpgradeInProgress,

        [EnumMember]
        UpgradeBlocked,

        [EnumMember]
        UpgradeRequired,

        [EnumMember]
        UpgradeAvailable,

        [EnumMember]
        NoActionRequired,

        [EnumMember]
        Unknown
    }
}
