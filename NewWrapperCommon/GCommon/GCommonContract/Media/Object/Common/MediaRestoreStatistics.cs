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
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MediaRestoreStatistics
    {
        [DataMember]
        public Double TotalCount { get; set; }

        [DataMember]
        public Double TotalSize { get; set; }

        public override String ToString()
        {
            return String.Format("Media Restore Statistics: Total Count: {0}, Total Size: {1}",
                this.TotalCount,
                this.TotalSize);
        }
    }

    /// <summary> Restore statistics function需要前台传递给后台参数封装类. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreStatisticsContract
    {
        [DataMember]
        public String BackupJobId { get; set; }

        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        [DataMember]
        public Boolean OnlyOneJob { get; set; }

        public override String ToString()
        {
            return String.Format("Restore Statistics Contract: Backup Job Id: {0}, Tree Node: {1}",
                this.BackupJobId,
                this.TreeNode);
        }
    }
}
