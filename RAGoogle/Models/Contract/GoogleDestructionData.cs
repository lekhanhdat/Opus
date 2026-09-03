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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using RAGoogle.Util;

namespace RAGoogle.Models.Contract
{
    public class GoogleDestructionData
    {
        public string ScopeId { get; set; }
        public string ItemName { get; set; }
        public int Level { get; set; }
        public string RuleId { get; set; }
        public string FullPath { get; set; }
        public string TermId { get; set; }
        public long DestroyedTime { get; set; }
        public string MetaInfo { get; set; }
        public JMCreateAndDestroyedFileReportJobDetail GenerateCreateAndDestroyedReportJobDetail(string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new();
            detail.ObjectLevel = Level == (int)RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.Title = ItemName;
            detail.URL = FullPath;
            detail.Comment = comments;
            return detail;
        }
    }

    public class GoogleDestructionMetaData
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Level { get; set; }
        public string ItemExtension { get; set; }
        public string FullPath { get; set; }
        public string TermId { get; set; }
        public string TermName { get; set; }
        public long CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public long ModifiedTime { get; set; }
        public string ModifiedBy { get; set; }
        public int ManualApprovedBy { get; set; }
        public int ManualApprovedStatus { get; set; }
        public int ManualInternalApprovedStatus { get; set; }
        public int ManualArchiveStatus { get; set; }
        public string MetaInfo { get; set; }
    }
}
