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




namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlatformRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public String Type { get; set; }

        [DataMember]
        public String Guid { get; set; }
        [DataMember]
        public DRInfo DRInfo { get; set; }
        [DataMember]
        public PRTreeNodeDto WfeNode { get; set; }
        [DataMember]
        public SPTreeNodeDto TreeRoot { get; set; }
        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }
        [DataMember]
        public Boolean OnlyGenerateMapping { get; set; }
        [DataMember]
        public String StorageInfoExtention { get; set; }
        [DataMember]
        public PlatformType PlatformType { get; set; }
        [DataMember]
        public ProductVersion ProductVersion { get; set; }
        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Platform Restore Request: ");
            stringBuilder.AppendFormat("Type: {0}, ", this.Type);
            stringBuilder.AppendFormat("Guid: {0}, ", this.Guid);
            stringBuilder.AppendFormat("DR Info: {0}, ", this.DRInfo);
            stringBuilder.AppendFormat("WFE Node: {0}, ", this.WfeNode);
            stringBuilder.AppendFormat("Tree Root: {0}, ", this.TreeRoot);
            stringBuilder.AppendFormat("Cache Location: {0}, ", this.CacheLocation);
            stringBuilder.AppendFormat("Logical Device: {0}", this.LogicalDevice);
            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DRInfo
    {
        [DataMember]
        public String FarmName { get; set; }
        [DataMember]
        public String LogicalDeviceId { get; set; }
        [DataMember]
        public String JobId { get; set; }
        [DataMember]
        public String PlanId { get; set; }
        [DataMember]
        public String AgentHost { get; set; }
        [DataMember]
        public String MemberAgentHost { get; set; }
        [DataMember]
        public String MediaHost { get; set; }
        [DataMember]
        public String Path { get; set; }
        [DataMember]
        public String StorageInfo { get; set; }
        [DataMember]
        public Dictionary<String, String> StorageInfoMap { get; set; }

        public override String ToString()
        {
            return String.Format("Farm Name: {0}, Job Id: {1}, Agent Host: {2}, Member Agent Host: {3}, " +
                "Media Host: {4}, Path: {5}",
                this.FarmName,
                this.JobId,
                this.AgentHost,
                this.MemberAgentHost,
                this.MediaHost,
                this.Path);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WFEList
    {
        [DataMember]
        public List<WFEFile> WFEFileList { get; set; }
        [DataMember]
        public List<WFEFoder> WFEFoderList { get; set; }

        public WFEList()
        {
            this.WFEFileList = new List<WFEFile>();
            this.WFEFoderList = new List<WFEFoder>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WFEFile
    {
        [DataMember]
        public Int64 Id { get; set; }
        [DataMember]
        public String Guid { get; set; }
        [DataMember]
        public String FileHeader { get; set; }

        public override String ToString()
        {
            return String.Format("Id: {0}, Guid: {1}, File Header: {2}",
                this.Id,
                this.Guid,
                this.FileHeader);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WFEFoder
    {
        [DataMember]
        public String Guid { get; set; }
        [DataMember]
        public String RestoreOption { get; set; }
        [DataMember]
        public String Location { get; set; }
        [DataMember]
        public String FileHeader { get; set; }
        [DataMember]
        public List<WFEFile> WFEFileList { get; set; }

        public override String ToString()
        {
            return String.Format("Guid: {0}, Restore Option: {1}, Location: {2}, File Header: {3}",
                this.Guid,
                this.RestoreOption,
                this.Location,
                this.FileHeader);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreStatistics
    {
        [DataMember]
        public Double TotalCount { get; set; }

        [DataMember]
        public Double TotalSize { get; set; }

        public override String ToString()
        {
            return String.Format("Total Count: {0}, Total Size: {1}",
                this.TotalCount,
                this.TotalSize);
        }
    }
}
