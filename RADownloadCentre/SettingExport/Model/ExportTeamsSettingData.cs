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
using AvePoint.RA.DB.Model;

namespace RADownloadCentre.SettingExport.Model
{
    public class ExportTeamsSettingData
    {
        public int Id { set; get; } 
        public string NodeInfo { get; set; }
        public Guid TermId { get; set; }
        public Guid TermSetId { get; set; }
        public string TermScopeNamePath { get; set; }
        public string TermDefaultNamePath { get; set; }
        public Guid DefaultTermId { get; set; }
        public bool NeedCheckDefaultValue { get; set; }
        public bool IncludeDeclaredRecords { get; set; }
        public bool? ApplyTermIncludeFolder { get; set; }
        public int ApplyExistType { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public bool EMailToRecordOwner { get; set; }
        public string WorkflowReferenceId { get; set; }
        public int DeployTermMethod { get; set; }
        public string ContainerName { get; set; }
        public string TeamsOrGroupName { get; set; }
        public string FullPath { get; set; }
        public WorkflowInfomation WorkflowInfomation { get; set; }
        public List<string> UserName { get; set; }
        public bool IsInheritSetting { get; set; }
        public bool IsEmptySetting { get; set; }
        public string TeamsId { get; set; }
        public bool IsSkipSetting { get; set; }
    }
    public class WorkflowInfomation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
