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
    public class CAFarmUpgradeStatusOperation : CAOperation
    {
        [DataMember]
        public List<SharePointUpdateStatusInfo> SharePointUpdateStatusInfos { get; set; }

        [DataMember]
        public List<SharePointUpdateDetailStatusInfo> SharePointUpdateDetailStatusInfos { get; set; }

        public SharePointUpdateDetailStatusInfo GetSharePointUpdateDetailStatusInfoBySessionId(String sessionId)
        {
            var result = default(SharePointUpdateDetailStatusInfo);
            if (this.SharePointUpdateDetailStatusInfos != null)
                result = this.SharePointUpdateDetailStatusInfos.Find(item => item.SessionId == sessionId);
            return result;
        }
        public SharePointUpdateDetailStatusInfo GetSharePointUpdateDetailStatusInfoBySessionId(SharePointUpdateStatusInfo statusInfo)
        {
            return GetSharePointUpdateDetailStatusInfoBySessionId(statusInfo.SessionId);
        }
        public List<SharePointUpdateDetailStatusInfo> GetSharePointUpdateDetailStatusInfoBySessionId(List<String> sessionIds)
        {
            var result = default(List<SharePointUpdateDetailStatusInfo>);
            if (this.SharePointUpdateDetailStatusInfos != null)
                result = this.SharePointUpdateDetailStatusInfos.FindAll(item => sessionIds.Contains(item.SessionId));
            return result;
        }

        public List<SharePointUpdateDetailStatusInfo> GetSharePointUpdateDetailStatusInfoBySessionId(List<SharePointUpdateStatusInfo> statusInfos)
        {
            var result = default(List<SharePointUpdateDetailStatusInfo>);
            if (this.SharePointUpdateDetailStatusInfos != null)
                result = this.SharePointUpdateDetailStatusInfos.FindAll(item => statusInfos.Exists(info => info.SessionId == item.SessionId));
            return result;
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointUpdateStatusInfo
    {
        [DataMember]
        public String SessionId { get; set; }

        [DataMember]
        public String Status { get; set; }

        [DataMember]
        public String Server { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public Int32 Errors { get; set; }

        [DataMember]
        public Int32 Warnings { get; set; }

        [DataMember]
        public String LogPath { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointUpdateDetailStatusInfo : SharePointUpdateStatusInfo
    {
        [DataMember]
        public String StartingObject { get; set; }

        [DataMember]
        public String CurrentObject { get; set; }

        [DataMember]
        public String CurrentDelegate { get; set; }

        [DataMember]
        public UInt32 CurrentStep { get; set; }

        [DataMember]
        public UInt32 TotalSteps { get; set; }

        [DataMember]
        public String ElapsedTime { get; set; }

        [DataMember]
        public String Percentage { get; set; }

        [DataMember]
        public String Process { get; set; }

        [DataMember]
        public Int32 ProcessId { get; set; }

        [DataMember]
        public Int32 ThreadId { get; set; }

        [DataMember]
        public String CommandLine { get; set; }

        [DataMember]
        public String Remedy { get; set; }
    }
}
