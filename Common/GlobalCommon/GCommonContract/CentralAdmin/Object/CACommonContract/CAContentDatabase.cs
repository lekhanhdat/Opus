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
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAContentDatabase
    {
        [DataMember]
        public string DatabaseName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int SiteCollectionNumber { get; set; }

        [DataMember]
        public int MaxNumberOfSiteCollections { get; set; }

        [DataMember]
        public int SiteCollectionWarning { get; set; }

        [DataMember]
        public String DatabaseServer { get; set; }

        [DataMember]
        public Boolean IsWindowsAuthentication { get; set; }

        [DataMember]
        public String SqlAccount { get; set; }

        [DataMember]
        public String SqlAccountPassword { get; set; }

        [DataMember]
        public Int32 NumberOfSiteCountBeforeWaring { get; set; }

        [DataMember]
        public Int32 MaxNumberOfSiteCanCreated { get; set; }

        [DataMember]
        public String SearchService { get; set; }

        [DataMember]
        public SearchServerState State { get; set; }

        [DataMember]
        public String FailOverServer { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchServerState
    {
        [EnumMember]
        Invisible = 0,

        [EnumMember]
        Enable = 1,

        [EnumMember]
        Disable = 2,
    }

}
