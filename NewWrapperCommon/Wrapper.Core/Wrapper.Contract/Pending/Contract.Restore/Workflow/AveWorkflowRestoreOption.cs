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


namespace AvePoint.Wrapper.Restore
{
    public class AveSPWorkflowRestoreOption
    {
        public bool? RestoreParentAssociationIfNotFound
        {
            set;
            get;
        }
        public bool? ProcessAssociation
        {
            set;
            get;
        }
        public bool? ProcessInstance
        {
            set;
            get;
        }
        public bool? SkipRunningInstance
        {
            set;
            get;
        }
        public bool? RestartRunningInstance
        {
            set;
            get;
        }
        /// <summary>
        /// 是否允许同一个Web下不同List中创建两个同名的Workflow Association, 仅对SPD和Nintex Workflow生效
        /// </summary>
        public bool? AllowDuplicateSPDAndNintexInSameWeb 
        {
            set;
            get;
        }

    }

    public enum WFAssociationConflictType
    {
        None = 0,
        Template,
        #region For future use
        Configuration,
        #endregion
        Same,
    }

    public enum WFTemplateConflictResolutionOption : byte
    {
        /// <summary>
        /// 冲突的话就不还原
        /// </summary>
        NotOverwrite = 0,
        /// <summary>
        /// 冲突的话仍然走还原
        /// </summary>
        Overwrite = 1,
    }

    [System.Flags]
    public enum WFAssociationConflictResolutionOption : byte
    {
        /// <summary>
        /// association不会被restore
        /// </summary>
        NotOverwrite = 0,
        /// <summary>
        /// 重新命名association，直到不冲突。命名规则是: [backed up association name]_[number]
        /// </summary>
        Append = 1,
        /// <summary>
        /// 如果目的端association上没有workflow instance，则将目的端association删除后再restore；如果有instance，则skip
        /// </summary>
        Overwrite = 2,
        /// <summary>
        /// 无论目的端的association是否有instance，都先将其删除，然后重新restore；
        /// </summary>
        ForceOverwrite = 3,
        /// <summary>
        /// 不会删除目的端association，但会更新目的端association的一些配置属性；
        /// </summary>
        UpdateOverwrite = 4,
        /// <summary>
        /// 这个option是为instance所用。当还原instance时，如果其parent association没有被还原，
        /// 将在这个过程中重新还原parent association。为了使instance能够还原回去，
        /// 要保证其parent association不会被其本身的冲突规则再次skip掉，因此加了这个Option。
        /// </summary>
        ForceUse = 5
    }

    [System.Flags]
    public enum WFInstanceConflictResolutionOption : byte
    {
        NotOverwrite = 0,
        Overwrite = 1,
        OverwriteByModifiedTime = 2
    }

    public enum WorkflowTypeFilter
    {
        SPBuiltIn,
        SPD,
        VS
    }

}