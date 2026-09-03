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
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.PlatformRecovery.PRMaintenance
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRMaintenanceMessage : PRMultipleControlMessage
    {
        private List<string> mJobList = new List<string>();
        [DataMember]
        public List<string> JobList
        {
            get
            {
                return mJobList;
            }
            set
            {
                mJobList = value;
            }
        }

        private Dictionary<string, PRBackupJobDto> mJobCol = new Dictionary<string, PRBackupJobDto>();
        [DataMember]
        public Dictionary<string, PRBackupJobDto> JobCol
        {
            get
            {
                return mJobCol;
            }
            set
            {
                mJobCol = value;
            }
        }

        [DataMember]
        public PRMaintenanceJobDto MaintenanceJob { get; set; }

        private List<string> mMappingJobList = new List<string>();
        [DataMember]
        public List<string> MappingJobList
        {
            get
            {
                return mMappingJobList;
            }
            set
            {
                mMappingJobList = value;
            }
        }
        private List<string> mCopyDataJobList = new List<string>();
        [DataMember]
        public List<string> CopyDataJobList
        {
            get
            {
                return mCopyDataJobList;
            }
            set
            {
                mCopyDataJobList = value;
            }
        }
        private List<string> mIndexJobList = new List<string>();  
        [DataMember]
        public List<string> IndexJobList
        {
            get
            {
                return mIndexJobList;
            }
            set
            {
                mIndexJobList = value;
            }
        }
        private Dictionary<string, PRBackupCatalogDto> mCatalogList = new Dictionary<string, PRBackupCatalogDto>();   
        [DataMember]
        public Dictionary<string, PRBackupCatalogDto> CatalogList
        {
            get
            {
                return mCatalogList;
            }
            set
            {
                mCatalogList = value;
            }
        }
        
        //private Dictionary<string, Dictionary<string, PRTreeNodeDto>> mTreeNodeList = new Dictionary<string, Dictionary<string, PRTreeNodeDto>>();
        //[DataMember]
        //public Dictionary<string, Dictionary<string, PRTreeNodeDto>> TreeNodeList
        //{
        //    get
        //    {
        //        return mTreeNodeList;
        //    }
        //    set
        //    {
        //        mTreeNodeList = value;
        //    }
        //}
        private Dictionary<string, ServiceDto> mMediaList = new Dictionary<string, ServiceDto>();
        [DataMember]
        public Dictionary<string,ServiceDto>MediaList
        {
            get
            {
                return mMediaList;
            }
            set
            {
                mMediaList = value;
            }
        }
        private Dictionary<string, PlatformBackupRequest> mConfigForMediaList = new Dictionary<string, PlatformBackupRequest>();
        [DataMember]
        public Dictionary<string, PlatformBackupRequest> ConfigForMediaList
        {
            get
            {
                return mConfigForMediaList;
            }
            set
            {
                mConfigForMediaList = value;
            }
        }
        
        private Dictionary<string, PRStagingPolicyDto> mStagingPolicyList = new Dictionary<string, PRStagingPolicyDto>();
        [DataMember]
        public Dictionary<string, PRStagingPolicyDto> StagingPolicyList
        {
            get
            {
                return mStagingPolicyList;
            }
            set
            {
                mStagingPolicyList = value;
            }

        }

    }
}