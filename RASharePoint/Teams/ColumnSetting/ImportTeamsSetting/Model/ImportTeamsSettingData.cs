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


using AvePoint.RA.SharePoint.Common.Setting.Model;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Model
{
    public class ImportTeamsSettingData
    {
        #region csv column
        public string ContainerName { get; set; }
        public string TeamsOrGroupName { get; set; }
        public string SiteCollectionUrl { get; set; }
        public string SitePath { get; set; }
        public string ListPath { get; set; }
        public string FolderPath { get; set; }
        public string TermScopePath { get; set; }
        public string DefaultTermPath { get; set; }
        public bool ApplyExisting { get; set; }
        public bool IncludeDeclaredDoc { get; set; }
        public bool IsOverwrite { get; set; }
        public string WorkflowName { get; set; }
        public int ApprovalType { get; set; }
        public bool IsSendEmail { get; set; }
        public bool ApplyTermsOnFolders { get; set; }
        public int DeployTermMethod { get; set; }
        #endregion
        public string TermGroup { get; set; }
        public string TeamsGroupId { get; set; }
        public string TermSet { get; set; }
        public string TermScopeRelativePath { get; set; }
        public SettingLevel SettingLevel { get; set; }
        public string FullUrl { get; set; }
    }
}
