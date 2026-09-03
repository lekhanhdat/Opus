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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserArchiverDownloadInfo
    {
        [DataMember]
        public List<LogicalDeviceDto> DataDeviceList { get; set; }
        [DataMember]
        public LogicalDeviceDto IndexDevice { get; set; }
        [DataMember]
        public String FarmName { get; set; }
        [DataMember]
        public String WebAppUrl { get; set; }
        [DataMember]
        public String SiteUrl { get; set; }
        [DataMember]
        public List<String> PathMD5List { get; set; }
        [DataMember]
        public String SessionId { get; set; }
        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }
        [DataMember]
        public Int32 CodePage { get; set; }

        public override String ToString()
        {
            return String.Format("End User Archiver Download Info: Site Url: {0}, Index Device: {1}",
                this.SiteUrl,
                this.IndexDevice);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserArchiverDownloadResult
    {
        [DataMember]
        public String SessionId { get; set; }
        [DataMember]
        public Int32 MediaServerControlPort { get; set; }
        [DataMember]
        public String MediaServerHostOrIpAddress { get; set; }

        public override String ToString()
        {
            return String.Format("End User Archiver Download Result: Media Server Control Port: {0}, " +
                "Media Server Host Or Ip Address: {1}",
                this.MediaServerControlPort,
                this.MediaServerHostOrIpAddress);
        }
    }
}
