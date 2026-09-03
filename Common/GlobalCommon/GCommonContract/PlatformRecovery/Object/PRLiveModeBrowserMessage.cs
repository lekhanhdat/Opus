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




using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [KnownType(typeof(PRLiveModeBrowserDocAveMessage))]
    [KnownType(typeof(PRLiveModeBrowserNetAppMessage))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRLiveModeBrowserMessage : PRMessage
    {
        [DataMember]
        public PRLMBrowserType LMBrowserType { get; set; }//0:Browse 1:Search

        private bool mRelay = true;
        [DataMember]
        public bool Relay
        {
            get { return mRelay; }
            set { mRelay = value; }
        }
        [DataMember]
        public SPTreeNodeDto CurrentTreeNode { get; set; }
        [DataMember]
        public ServiceDto MediaInfo { get; set; }
        [DataMember]
        public PRTreeNodeDto DBNode { get; set; }

        private List<SPTreeNodeDto> mSubNodes = new List<SPTreeNodeDto>();
        [DataMember]
        public List<SPTreeNodeDto> SubNodes
        {
            get { return mSubNodes; }
            set { mSubNodes = value; }
        }
        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }
        [DataMember]
        public int StartIndex { get; set; }
        [DataMember]
        public int Length { get; set; }
        [DataMember]
        public int ChildrenCount { get; set; }
        [DataMember]
        public BackupDataSearchContract AdvancedSearchOption { get; set; }

        [DataMember]
        public string SessionKey { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRLiveModeBrowserDocAveMessage : PRLiveModeBrowserMessage
    {
        [DataMember]
        public Dictionary<string, PlatformRestoreRequest> RestoreRequests { get; set; }
        [DataMember]
        public string BackupJobId { get; set; }
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }
        [DataMember]
        public List<string> VSSBackupMetadataXMLFullPaths { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRLiveModeBrowserNetAppMessage : PRLiveModeBrowserMessage
    {
        [DataMember]
        public PRStagingPolicyDto SQLInstanceInfo { get; set; }
        [DataMember]
        public string SnapInfo { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRLMBrowserType
    {
        [EnumMember]
        Browse = 0,
        [EnumMember]
        Search = 1,
    }
}
