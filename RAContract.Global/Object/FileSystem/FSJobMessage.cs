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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract]
    public class FSJobMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public string TenantId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string JobId { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public FSMessageType MessageType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public JobType JobType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FSTreeNodeDto> FSTreeNodes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BaseJobDto Job { set; get; }
        //[DataMember(EmitDefaultValue = false)]
        //public string SubJobId { set; get; }
        //export location
        //export type
        //wep api host address
        [DataMember(EmitDefaultValue = false)]
        public FSJobType FSJobType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public TermConflictOption TermConflictOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime IBStartTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool NeedChangeProfile { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RecordOwner { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string WebApiAddress { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string AllRecordsRule { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<AvePoint.RA.Contract.FileSystem.FSTermDto> AllTerms { get; set; }
        [DataMember(EmitDefaultValue = false)]
        //term-->rule id mapping
        public Dictionary<Guid, List<Guid>> TermRuleMapping { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<AvePoint.RA.Contract.FileSystem.FSSettingDto> RMScopeSettings { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string UniqueIdPrefix { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string CurrentSettingScopeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string GeneralSettingModel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TimeFormat { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ChangedTermIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<string> RunningJobNodeUrls { get; set; }
        #region disposal job
        [DataMember(EmitDefaultValue = false)]
        public List<string> BreakTreeNodeUrls { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, Guid> ConnectionCache { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        public bool BulkImportEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int BulkSize { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public int ClassificationLevel { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string FolderTermId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public ClassCodeInfoDto ClassCodeDto { set; get; }

        #region FS Report
        [DataMember(EmitDefaultValue = false)]
        public DateTime StartTime { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime EndTime { set; get; }
        #endregion

        #region Disposal by Class Code
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ClassCodeIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<ClassCodeInfoDto> ClassCodeInfoList { get; set; }
        #endregion
    }


}
