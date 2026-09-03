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




namespace AvePoint.GCommon.Contract.GranularBackup.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupRestoreWorkflow
    {
        [DataMember]
        public Boolean IncludeWorkflowDefinition { get; set; }

        [DataMember]
        public Boolean IncludeWorkflowInstance { get; set; }

        [DataMember]
        public WorkflowConflictResolutionType DefinitionConflictResolution { get; set; }

        [DataMember]
        public WorkflowConflictResolutionType InstanceConflictResolution { get; set; }

        public override String ToString()
        {
            return String.Format("Definition Conflict Resolution: {0}, Instance Conflict Resolution: {1}",
                this.DefinitionConflictResolution.ToString(),
                this.InstanceConflictResolution.ToString());
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WorkflowConflictResolutionType
    {
        [EnumMember]
        None,
        /// <summary> Include workflow definition和Include workflow instance共有设置 </summary>
        [EnumMember]
        NotOverwrite,
        /// <summary> 仅Include workflow instance有 </summary>
        [EnumMember]
        Overwrite,
        /// <summary> 仅Include workflow definition有 </summary>
        [EnumMember]
        Append,
        /// <summary> only 'Include workflow definition' hava this setting, 
        /// represent skip the definition if there is any running instance. </summary>
        [EnumMember]
        OverwriteOrSkipDefinition,
        /// <summary> 仅Include workflow definition有 </summary>
        [EnumMember]
        OverwriteDefinitionByForce
    }
}
