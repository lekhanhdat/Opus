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
using System;
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class AveOD4BWeb : AveOD4BBase, IAveBackupRestoreWeb
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string mWebUrl = string.Empty;
        private static List<string> mLists = new List<string> { "{0}/Documents" };
        private AveBRWebInfo mInternalInfo = new AveBRWebInfo();
        public AveOD4BWeb(AveOD4BRequestController controller, string url)
            : base(controller)
        {
            this.mWebUrl = url;
            //[pending]
            //controller里面的context url和web会存在不一致的问题，因为有sub web的存在。
        }

        public Guid Id
        {
            get
            {
                VerifyCacheData("WebBasic");
                return this.mInternalInfo.Id;
            }
        }

        public string Title
        {
            get
            {
                VerifyCacheData("WebBasic");
                return this.mInternalInfo.Title;
            }
        }

        public string Url
        {
            get { return this.mWebUrl; }
        }

        public string ServerRelativeUrl
        {
            get
            {
                VerifyCacheData("WebBasic");
                return this.mInternalInfo.ServerRelativeUrl;
            }
        }


        protected override string Level
        {
            get
            {
                return "Web";
            }
        }
        protected override void EnsureExportMethods()
        {
            ExportMethods[BackupOption.User] = ExportUser;
            ExportMethods[BackupOption.Group] = ExportGroup;
            ExportMethods[BackupOption.BasicInfo] = ExportBasicInfo;
            ExportMethods[BackupOption.RoleDefinition] = ExportRoleDefinition;
            ExportMethods[BackupOption.RoleAssignment] = ExportRoleAssignment;
        }

        private ProcessResult ExportUser(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();
            var webUserInfo = this.mController.GetOD4WebUserInfo(this.mWebUrl);
            if (webUserInfo == null)
            {
                throw new Exception("Failed to backup web user info");
            }
            if (webUserInfo.Count > 0)
            {
                List<AveUserInfo> infos = new List<AveUserInfo>();
                foreach (var user in webUserInfo)
                {
                    Controller.GlobalCache.AddUser(user.Id, user.LoginName);
                    infos.Add(InfoConverter<AveUserInfo>.ConvertToCommonInfo(user));
                }
                stream.WriteMetadata(AveMetadataType.Users, infos);
            }
            return result;
        }
        private ProcessResult ExportGroup(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();

            VerifyCacheData("WebGroup");

            List<AveBRGroupInfo> brGroupInfo = null;
            CacheItem cacheItem = null;
            if (this.mInternCache.TryGet("WebGroup", out cacheItem))
            {
                brGroupInfo = (List<AveBRGroupInfo>)cacheItem.Value;
            }
            if (brGroupInfo.Count > 0)
            {
                List<AveGroupInfo> infos = new List<AveGroupInfo>();
                brGroupInfo.ForEach(groupInfo => infos.Add(InfoConverter<AveGroupInfo>.ConvertToCommonInfo(groupInfo)));
                stream.WriteMetadata(AveMetadataType.Groups, infos);
            }
            return result;
        }

        private ProcessResult ExportBasicInfo(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();

            VerifyCacheData("WebBasic");

            if (this.mInternalInfo.SiteNotebookActive)
            {
                mLists.Add("{0}/SiteAssets");
            }
            stream.WriteMetadata(AveMetadataType.WebBasicInfo, InfoConverter<AveWebInfo>.ConvertToCommonInfo(this.mInternalInfo));
            stream.WriteMetadata(AveMetadataType.WebProperty, InfoConverter<AveWebSettingInfo>.ConvertToCommonInfo(this.mInternalInfo));
            return result;
        }

        private ProcessResult ExportRoleDefinition(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();

            VerifyCacheData("WebRoleDefinition");

            List<AveBRRoleDefinitionInfo> brRoleDefinitionInfo = null;
            CacheItem cacheItem = null;
            if (this.mInternCache.TryGet("WebRoleDefinition", out cacheItem))
            {
                brRoleDefinitionInfo = (List<AveBRRoleDefinitionInfo>)cacheItem.Value;
            }
            if (brRoleDefinitionInfo.Count > 0)
            {
                List<AveRoleInfo> infos = new List<AveRoleInfo>();
                foreach (var roleDef in brRoleDefinitionInfo)
                {
                    Controller.GlobalCache.AddRoleDefinition(roleDef.Id, roleDef.Name);
                    AveRoleInfo info = InfoConverter<AveRoleInfo>.ConvertToCommonInfo(roleDef);
                    info.PermMask = (long)roleDef.BasePermissions;
                    infos.Add(info);
                }
                stream.WriteMetadata(AveMetadataType.Roles, infos);
            }
            return result;
        }

        private ProcessResult ExportRoleAssignment(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();
            var webRoleAssignmentInfo = this.mController.GetOD4BWebRoleAssignmentInfo(this.mWebUrl);
            if (webRoleAssignmentInfo == null)
            {
                throw new Exception("Failed to backup web role assignment info");
            }
            if (webRoleAssignmentInfo.Count > 0)
            {
                List<AveRoleAssignmentInfo> infos = new List<AveRoleAssignmentInfo>();
                webRoleAssignmentInfo.ForEach(roleAssignmentInfo =>
                {
                    infos.Add(InfoConverter<AveRoleAssignmentInfo>.ConvertToCommonInfo(roleAssignmentInfo));
                });
                stream.WriteMetadata(AveMetadataType.RoleAssignment, infos);
            }
            return result;
        }

        protected override void FillCacheData(ProcessResult result)
        {
            mLog.Info("Begin to fill {0} cache data", this.Url);
            var info = base.mController.BatchGetOD4BWebInfo(this.Url);

            foreach (var kv in info)
            {
                if (string.Equals(kv.Key, "WebBasic", StringComparison.OrdinalIgnoreCase))
                {
                    this.mInternalInfo = (AveBRWebInfo)kv.Value;
                }
                base.mInternCache.Add(kv.Key, new CacheItem() { Value = kv.Value, Result = result });
            };
        }

        protected override void AddFakeData(ProcessResult result)
        {
            base.mInternCache.Add("WebBasic", new CacheItem() { Value = null, Result = result });
            base.mInternCache.Add("WebGroup", new CacheItem() { Value = null, Result = result });
            base.mInternCache.Add("WebRoleDefinition", new CacheItem() { Value = null, Result = result });
        }

        public List<IAveBackupRestoreList> GetLists()
        {
            List<IAveBackupRestoreList> lists = new List<IAveBackupRestoreList>();
            foreach (string url in mLists)
            {
                lists.Add(new AveOD4BList(Controller, mWebUrl, string.Format(url, mWebUrl.TrimEnd('/'))));
            }
            //if (AveOD4BConfig.IncludeCustomList)
            //{ }
            return lists;
        }

        protected override List<AveBRChangeObject> GetChangedObjects()
        {
            return new List<AveBRChangeObject>();
        }

        public void Dispose()
        {
            base.mInternCache.Clear();
        }
    }
}
