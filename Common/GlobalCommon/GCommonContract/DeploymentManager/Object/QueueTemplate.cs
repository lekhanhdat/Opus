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
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(DesignManagerQueueTemplate))]
    public class QueueTemplate
    {
        #region == Plan Setting部分 ==
        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string PlanDescription { get; set; }

        [DataMember]
        public bool IsBackup { get; set; }

        [DataMember]
        public string StorageName { get; set; }

        [DataMember]
        public string EmailNotificationName { get; set; }

        [DataMember]
        public string PlanGroupNames { get; set; }

        //[DataMember]
        //public List<string> PlanGroupNames { get; set; }
        #endregion


        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public DMPlanType DMPlanType { get; set; }

        [DataMember]
        public string SourceFarm { get; set; }

        [DataMember]
        public DMPlanCategory DMPlanCategory { get; set; }

        [DataMember]
        public string UrlOrNodeName { get; set; }

        [DataMember]
        public NodeLevel NodeLevel { get; set; }

        [DataMember]
        public bool Checked { get; set; }

        [DataMember]
        public bool SelectAll { get; set; }

        [DataMember]
        public bool IncludeNew { get; set; }

        [DataMember]
        public string DestFarm { get; set; }

        [DataMember]
        public string DestPath { get; set; }

        [DataMember]
        public NodeLevel DestNodeLevel { get; set; }

        [DataMember]
        public bool DestNodeChecked { get; set; }

        /// <summary>
        /// TreeType 0为Source  1为Destination
        /// </summary>
        [DataMember]
        public int TreeType { get; set; }

        [DataMember]
        public string SrcSPObjectId { get; set; }
        [DataMember]
        public string DestSPObjectId { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DownloadOrUploadCallBackDto
    {
        /// <summary>
        /// 需要显示的Plan
        /// </summary>
        [DataMember]
        public DeploymentManagerPlanGroupDto PlanGroup { get; set; }
        /// <summary>
        /// Server返回给GUI的状态
        /// </summary>
        [DataMember]
        public DownloadOrUploadStatus DownloadOrUploadStatus { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string FilePath { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DownloadOrUploadStatus
    {
        [EnumMember]
        OK,

        [EnumMember]
        Exist,

        [EnumMember]
        FileError
    }
}
