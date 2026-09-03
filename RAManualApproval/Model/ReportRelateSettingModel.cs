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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Model
{
    public class ReportRelateSettingModel
    {
        public string Id { get; set; }

        public bool HasParent { get; private set; }
        
        public string ParentId { get; private set; }

        public bool HasSetting { get; private set; }

        public int SettingId { get; private set; }

        public bool IsRoot { get; private set; }

        private ReportRelateSettingModel() { }

        public static ReportRelateSettingModel GenerateModel(Guid groupId)
        {
            return GenerateModel(groupId, Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
        }

        public static ReportRelateSettingModel GenerateModel(Guid groupId, Guid siteId)
        {
            return GenerateModel(groupId, siteId, Guid.Empty, Guid.Empty, string.Empty);
        }

        public static ReportRelateSettingModel GenerateModel(Guid groupId, Guid siteId, Guid webId)
        {
            return GenerateModel(groupId, siteId, webId, Guid.Empty, string.Empty);
        }

        public static ReportRelateSettingModel GenerateModel(Guid groupId, Guid siteId, Guid webId, Guid listId)
        {
            return GenerateModel(groupId, siteId, webId, listId, string.Empty);
        }

        public static ReportRelateSettingModel GenerateModel(Guid groupId, Guid siteId, Guid webId, Guid listId, string folderRelativeUrl)
        {
            var keyList = new List<string>
            {
                groupId.ToString(),
                siteId.ToString(),
                webId.ToString(),
                listId.ToString(),
                folderRelativeUrl
            };
            return new ReportRelateSettingModel
            {
                Id = string.Join("=Ave=", keyList),
                HasParent = false,
                ParentId = "",
                HasSetting = false,
                SettingId = -1
            };
        }

        public static string GenerateKey(ManualExportReportInfo reportInfo)
        {
            if (!string.IsNullOrEmpty(reportInfo.ServerRelativeUrl) && !reportInfo.ServerRelativeUrl.StartsWith("/"))
            {
                reportInfo.ServerRelativeUrl = "/" + reportInfo.ServerRelativeUrl;
            }
            var folderRelativeUrl = reportInfo.ServerRelativeUrl.Contains("\\") ? reportInfo.ServerRelativeUrl.Substring(0, reportInfo.ServerRelativeUrl.IndexOf("\\")) : reportInfo.ServerRelativeUrl;
            var keyList = new List<string>
            {
                reportInfo.SiteGroupID.ToString(),
                reportInfo.RegistedSiteId.ToString(),
                reportInfo.WebID.ToString(),
                reportInfo.ListID.ToString(),
                folderRelativeUrl
            };
            return string.Join("=Ave=", keyList);
        }

        public static string GenerateKeyForOnpremise(ManualExportReportInfo reportInfo)
        {
            var folderId = reportInfo.ObjectLevel == RMReportObjectLevel.Folder ? reportInfo.NodeID : reportInfo.ParentID;
            var keyList = new List<string>
            {
                reportInfo.SiteGroupID.ToString(),
                reportInfo.RegistedSiteId.ToString(),
                reportInfo.WebID.ToString(),
                reportInfo.ListID.ToString(),
                folderId.ToString()
            };
            return string.Join("=Ave=", keyList);
        }

        public ReportRelateSettingModel SetSettingId(int settingId)
        {
            if(settingId > 0)
            {
                SettingId = settingId;
                HasSetting = true;
            }
            return this;
        }

        public ReportRelateSettingModel SetParentId(string parentId)
        {
            if(!string.IsNullOrEmpty(parentId))
            {
                ParentId = parentId;
                HasParent = true;
            }
            return this;
        }

        public ReportRelateSettingModel SetRoot()
        {
            IsRoot = true;
            return this;
        }
    }
}
