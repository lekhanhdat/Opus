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
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;
using AvePoint.GCommon.FileTransfer;
using AvePoint.Wrapper.Common;
using System;
using AvePoint.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System.IO.Hashing;
using Microsoft.SharePoint.Client;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2012/8/7",
      "ruiheng.liu@AvePoint.com",
      "yanlong.gu@AvePoint.com",
      new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
      "ADO-44684",
      true
      )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/11/2",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
            {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
            },
    "ADO-53910",
    false
    )]
    /// <summary>
    /// Use to send message to media while performing item backup
    /// </summary>
    class BackupInfoSender
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Hashtable fileHeaderAttribute = new Hashtable();

        /// <summary>
        /// For item multi-threads backup, return parent info
        /// </summary>
        public Hashtable FileHeaderAttribute
        {
            get
            {
                return fileHeaderAttribute;
            }
            internal set
            {
                fileHeaderAttribute = value;
            }
        }

        /// <summary>
        /// Item Level use this property
        /// </summary>
        public IAveBackupStream BackupStream { get; set; }

        /// <summary>
        /// Site ，Subsite use this property
        /// </summary>
        public IArchiverBackupDataWriter FileSender { get; set; }

        /// <summary>
        /// send header message of a backup item
        /// </summary>
        private XmlElement fileHeaderXml;

        private BackupPermissionForEndUser backupPermission;
        public List<PermissionLevel> permissionLevels { get; set; }

        public BackupPermissionForEndUser BackupPermission
        {
            get
            {
                return backupPermission;
            }
        }

        private string FarmId
        {
            get
            {
                return AveEnv.AgentFarmId;
            }
        }

        public void AddBackupFileHeaderAttribute(string key, string value)
        {
            if (fileHeaderAttribute.ContainsKey(key))
            {
                fileHeaderAttribute[key] = value;
            }
            else
            {
                fileHeaderAttribute.Add(key, value);
            }
        }

        //public BackupInfoSender(IFileSender filesender)
        //{
        //    FileSender = filesender;
        //    BackupStream = new AveCommonBackupStream(FileSender);
        //    XmlDocument doc = new XmlDocument();
        //    fileHeaderXml = doc.CreateElement("FileHeader");
        //    fileHeaderXml.SetAttribute("farmGUID", FarmId);

        //    //XmlElement stubInfoXml = doc.CreateElement("StubInfo");
        //    //mStubInfoXml.SetAttribute("mediaHost", mediaHost);
        //    //mStubInfoXml.SetAttribute("agentHost", agentHost);
        //    //mStubInfoXml.SetAttribute("planId", planId);
        //    //mStubInfoXml.SetAttribute("jobId", jobId);


        //    //XmlElement xe = doc.CreateElement("path");
        //    //stubInfoXml.AppendChild(xe);


        //    //xe = doc.CreateElement("path");
        //    //stubInfoXml.AppendChild(xe);


        //    //xe = doc.CreateElement("path");
        //    //stubInfoXml.AppendChild(xe);

        //    //fileHeaderXml.AppendChild(stubInfoXml);
        //    //FileSender = filesender;
        //    //BackupStream = new AveCommonBackupStream(FileSender);
        //    //this.mFileHeaderXml = new XmlDocument().CreateElement("FileHeader");
        //    //mSecondFileHeaderXml = new XmlDocument().CreateElement("FileHeader"); 
        //}

        public BackupInfoSender(IArchiverBackupDataWriter filesender)
        {
            FileSender = filesender;
            BackupStream = new WrapperBackupStreamV1(new ArchiverFileSender(FileSender));
            XmlDocument doc = new XmlDocument();
            fileHeaderXml = doc.CreateElement("FileHeader");
            fileHeaderXml.SetAttribute("farmGUID", FarmId);

            //XmlElement stubInfoXml = doc.CreateElement("StubInfo");
            //mStubInfoXml.SetAttribute("mediaHost", mediaHost);
            //mStubInfoXml.SetAttribute("agentHost", agentHost);
            //mStubInfoXml.SetAttribute("planId", planId);
            //mStubInfoXml.SetAttribute("jobId", jobId);


            //XmlElement xe = doc.CreateElement("path");
            //stubInfoXml.AppendChild(xe);


            //xe = doc.CreateElement("path");
            //stubInfoXml.AppendChild(xe);


            //xe = doc.CreateElement("path");
            //stubInfoXml.AppendChild(xe);

            //fileHeaderXml.AppendChild(stubInfoXml);
            //FileSender = filesender;
            //BackupStream = new AveCommonBackupStream(FileSender);
            //this.mFileHeaderXml = new XmlDocument().CreateElement("FileHeader");
            //mSecondFileHeaderXml = new XmlDocument().CreateElement("FileHeader"); 
        }

        public BackupInfoSender(IArchiverBackupDataWriter filesender, BackupPermissionForEndUser backupPermission)
        {
            FileSender = filesender;
            BackupStream = new WrapperBackupStreamV1(new ArchiverFileSender(FileSender));
            XmlDocument doc = new XmlDocument();
            fileHeaderXml = doc.CreateElement("FileHeader");
            fileHeaderXml.SetAttribute("farmGUID", FarmId);
            this.backupPermission = backupPermission;
        }

        public XmlElement GenerateHeader(string fullpath)
        {
            return GenerateFileHeaderAndExecuteAction(fileHeaderAttribute, GetProperties(fullpath), () => { });
        }

        private XmlElement GenerateFileHeaderAndExecuteAction(Hashtable attributes, string innerXml, Action action)
        {
            if (!string.IsNullOrEmpty(innerXml))
            {
                this.fileHeaderXml.InnerXml = innerXml;
            }
            foreach (object key in attributes.Keys)
            {
                fileHeaderXml.SetAttribute(key.ToString(), attributes[key].ToString());
            }
            XmlElement ret = (XmlElement)(fileHeaderXml.CloneNode(true));
            action();
            if (this.fileHeaderXml.HasAttribute("webApp"))
            {
                this.fileHeaderXml.RemoveAttribute("webApp");
            }
            if (this.fileHeaderXml.HasAttribute("isMyProfileList"))
            {
                this.fileHeaderXml.RemoveAttribute("isMyProfileList");
            }
            return ret;
        }

        public XmlElement GeneSiteHeader(AveSPSite aveSPSite, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            XmlElement headerExtraAttribute = GeneSiteHeaderXML(aveSPSite, entity, size, ruleName, subJobId, mediaName, fullPath);
            Hashtable siteAttributes = GeneSiteHeaderAttributeTable(aveSPSite, entity, size, ruleName, subJobId, mediaName, fullPath);
            return GenerateFileHeaderAndExecuteAction(siteAttributes, headerExtraAttribute.OuterXml, () => { });
        }

        public void SetHeaderAsArchiveSuccessForEnableDelete(XmlElement headerXml)
        {
            try
            {
                XmlElement headerExtraInfo = (XmlElement)headerXml.GetElementsByTagName("HeaderExtraAttribute")[0];
                if (headerExtraInfo != null)
                {
                    headerExtraInfo.SetAttribute("status", "Complete");
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail execute SetHeaderAsArchiveSuccessForEnableDelete, e:{ex}");
            }
        }

        public XmlElement BackupSiteHeader(AveSPSite aveSPSite, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            backupPermission = new BackupPermissionForEndUser(permissionLevels);
            XmlElement headerExtraAttribute = GeneSiteHeaderXML(aveSPSite, entity, size, ruleName, subJobId, mediaName, fullPath);
            Hashtable siteAttributes = GeneSiteHeaderAttributeTable(aveSPSite, entity, size, ruleName, subJobId, mediaName, fullPath);
            foreach (object key in siteAttributes.Keys)
            {
                fileHeaderAttribute.Add(key, siteAttributes[key]);
            }
            return WriteFileHeader(fileHeaderAttribute, headerExtraAttribute.OuterXml);
        }

        private Hashtable GeneSiteHeaderAttributeTable(AveSPSite aveSPSite, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            string siteUrl = entity.LeafName;
            Hashtable res = new Hashtable();
            res.Add(KeyWord.PATH, siteUrl);
            res.Add(KeyWord.TYPE, AveConstants.TYPE_SITE);
            res.Add(KeyWord.SiteUrl, siteUrl);
            res.Add(KeyWord.SIZE, size.ToString());
            res.Add(KeyWord.URL, entity.FullPath);
            res.Add(KeyWord.SUBJOBID, subJobId);
            res.Add(KeyWord.RULENAME, ruleName);
            res.Add(KeyWord.MEDIANAME, mediaName);
            res.Add(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            res.Add(KeyWord.TIME, entity.ScanTime.ToString());
            res.Add(KeyWord.FULLPATH, fullPath);
            return res;
        }

        private XmlElement GeneSiteHeaderXML(AveSPSite aveSPSite, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            string siteUrl = entity.LeafName;
            string siteId = string.Empty;
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("HeaderExtraAttribute");
            headerExtraAttribute.SetAttribute("APUrl", fullPath);

            //AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user1, AveContextKind.ClientObjectModel);

            //AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, new AveBPOSAccountInfo(), AveContextKind.Auto);
            using (IAveSite site = aveSPSite.SPSite)
            {
                //fileHeaderAttribute.Add(KeyWord.WEBAPP, site.WebApplication.GetResponseUri(AveUrlZone.Default));
                using (IAveWeb web = site.RootWeb)
                {
                    siteId = site.ID.ToString();
                    headerExtraAttribute.SetAttribute("guid", site.ID.ToString());
                    XmlElement headerUsers = doc.CreateElement("HeaderUsers");
                    foreach (IAveUser user in web.SiteAdministrators)
                    {
                        XmlElement headerUserInfo = doc.CreateElement("HeaderUserInfo");
                        headerUserInfo.SetAttribute("name", user.LoginName);
                        headerUsers.AppendChild(headerUserInfo);
                    }
                    headerExtraAttribute.AppendChild(headerUsers);
                }
            }
            return headerExtraAttribute;
        }

        public void BackupWebHeader(AveSPWeb aveWeb, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            backupPermission.GetWebRoles(aveWeb);
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveWeb.Name);
            AddBackupFileHeaderAttribute(KeyWord.WebId, aveWeb.SPWeb.ID.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_WEB.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            //if (!aveWeb.SPWeb.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = aveWeb.SPWeb.RoleAssignments.ID,
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveWeb.SPWeb.RoleAssignments));
            //}
        }

        public void BackupAppDefinitionHeader(AveSPWeb aveWeb, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveWeb.Name + "\\" + entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_APP.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, "false");
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
        }

        public void BackupMyListHeader(AveSPList aveList, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PROFILE, "1");
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveList.ParentWeb.Name + "\\" + entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.ListId, aveList.Id.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_MYPROFILE_LIST.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.PROFILE, "1");//MyList Need to give 1 value
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            //if (!aveList.SPList.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() }; //scopeId = aveList.ScopeId,
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveList.SPList.RoleAssignments));
            //}
        }

        public void BackupListHeader(AveSPList aveList, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string nameForSpecialChar, string listType, string fullPath)
        {

            AddBackupFileHeaderAttribute(KeyWord.PROFILE, "0");
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveList.ParentWeb.Name + "\\" + nameForSpecialChar);
            try
            {
                AddBackupFileHeaderAttribute(KeyWord.ListId, entity.LeafName.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? Guid.Empty.ToString() : aveList.Id.ToString());
            }
            catch (Exception e)
            {
                mLog.Debug("Add Backup File Header Attribute 'listId' Error: {0}", e.ToString());
                AddBackupFileHeaderAttribute(KeyWord.ListId, entity.LeafName.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? Guid.Empty.ToString() : aveList.Id.ToString());
            }
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_LIST.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.PROFILE, listType);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            //if ((aveList.SPList == null || aveList.IsSystemList) || !aveList.SPList.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };  // scopeId = aveList.ScopeId, 
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveList.SPList.RoleAssignments));
            //}
        }

        public void BackupMyListContentHeader(XmlNode node, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, node.Attributes["NameValue"].Value);
            AddBackupFileHeaderAttribute(KeyWord.ListId, node.Attributes["ListId"].Value);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_MYPROFILE_ITEM.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
        }

        public void BackupFolderHeader(AveSPFolder aveFolder, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveFolder.Path);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_FOLDER.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);//add for RevIM folder rule进行folder删除的时候需要获取该属性
            //if (!aveFolder.AveItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = aveFolder.AveItem.IsSystemFileOrFolder ? aveFolder.AveList.ScopeId : aveFolder.SPFolder.Item.RoleAssignments.ID
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveFolder.SPFolder.Item.RoleAssignments));
            //}
        }

        public void BackupFolderVerHeader(AveSPFolder aveFolder, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, aveFolder.Path);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_FOLDER_VERSION.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            //if (!aveFolder.AveItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = aveFolder.AveItem.IsSystemFileOrFolder ? aveFolder.AveList.ScopeId : aveFolder.SPFolder.Item.RoleAssignments.ID,
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveFolder.SPFolder.Item.RoleAssignments));
            //}
        }

        public void BackupItemHeader(AveSPListItem listItem, CacheNode parent, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_LISTITEM.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, false.ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, "");
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.RowId, entity.LibRowId.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "false");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            try
            {
                if (listItem.AveSPItem != null && listItem.AveSPItem.SPListItem != null)
                {
                    AddBackupFileHeaderAttribute(KeyWord.Created, DateTime.Parse(listItem.AveSPItem.SPListItem["Created"]?.ToString()).Ticks.ToString());
                    AddBackupFileHeaderAttribute(KeyWord.Modified, DateTime.Parse(listItem.AveSPItem.SPListItem["Modified"]?.ToString()).Ticks.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Info($"Init time header failed when back up item header {e}");
            }
            //if (!listItem.AveSPItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };   //  scopeId = listItem.AveSPItem.IsSystemFileOrFolder ? listItem.AveSPItem.AveSPList.ScopeId : listItem.AveSPItem.SPListItem.RoleAssignments.ID
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(listItem.AveSPItem.SPListItem.RoleAssignments));
            //}
        }


        public void BackupItemVersionHeader(AveSPListItem listItem, CacheNode parent, long size, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_LISTITEM.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, false.ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, "");
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "true");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            try
            {
                if (listItem.AveSPItem != null && listItem.AveSPItem.SPListItem != null)
                {
                    AddBackupFileHeaderAttribute(KeyWord.Created, DateTime.Parse(listItem.AveSPItem.SPListItem["Created"]?.ToString()).Ticks.ToString());
                    AddBackupFileHeaderAttribute(KeyWord.Modified, DateTime.Parse(listItem.AveSPItem.SPListItem["Modified"]?.ToString()).Ticks.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Info($"Init time header failed when back up item version header {e}");
            }
            //if (!listItem.AveSPItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };  //scopeId = listItem.AveSPItem.IsSystemFileOrFolder ? listItem.AveSPItem.AveSPList.ScopeId : listItem.AveSPItem.SPListItem.RoleAssignments.ID, 
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(listItem.AveSPItem.SPListItem.RoleAssignments));
            //}
        }
        private string SplitAuthorAndEditor(string originalString)
        {
            if (!string.IsNullOrEmpty(originalString))
            {
                string[] target = originalString.Split('|');
                int len = target.Length;
                if (target != null && len <= 1)
                {
                    return target[0];
                }
                else
                {
                    return target[len - 1];
                }
            }
            else
            {
                return string.Empty;
            }
        }
        public void BackupDocumentHeader(AveSPDoc aveDoc, CacheNode parent, long size, ArchiveApproveReport entity, AveSPFolder parentFolder, string ruleName, string subJobId, string mediaName, string fullPath, int backupFileType,bool needRecordStubId = false)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_DOCUMENT.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, (parentFolder.Path.EndsWith("\\{System Folder}", StringComparison.OrdinalIgnoreCase) || (aveDoc.AveSPItem != null && aveDoc.AveSPItem.IsSystemFileOrFolder)).ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, "");
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "false");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            AddBackupFileHeaderAttribute(KeyWord.HasUniqueRoleAssignments, aveDoc.AveSPItem.HasUniqueRoleAssignments.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BackupFileType, backupFileType.ToString());
            if (!string.IsNullOrEmpty(entity.StubInfo))
            {
                if (needRecordStubId)
                {
                    Guid stubId = Guid.NewGuid();
                    string stubIdString = stubId.ToString().Replace("-", "");
                    string realStubIdString = string.Concat(stubIdString, DateTime.UtcNow.Ticks.ToString());
                    entity.StubId = realStubIdString;
                }
                var stubInfo = GetStubInfo(entity.StubInfo, entity.StubId);
                AddBackupFileHeaderAttribute(KeyWord.StubInfo, stubInfo);
            }
            try
            {
                if (aveDoc.AveSPItem != null && aveDoc.AveSPItem.SPListItem != null)
                {
                    AddBackupFileHeaderAttribute(KeyWord.Created, DateTime.Parse(aveDoc.AveSPItem.SPListItem["Created"]?.ToString()).Ticks.ToString());
                    AddBackupFileHeaderAttribute(KeyWord.Modified, DateTime.Parse(aveDoc.AveSPItem.SPListItem["Modified"]?.ToString()).Ticks.ToString());
                    AddBackUpFileHeaderEditor(aveDoc);
                    AddBackUpFileHeaderAuthor(aveDoc);
                }
            }
            catch (Exception e)
            {
                mLog.Info($"Init time header failed {e}");
            }
            //if (!aveDoc.AveSPItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = aveDoc.AveSPItem.IsSystemFileOrFolder ? aveDoc.AveSPItem.AveSPList.ScopeId : aveDoc.AveSPItem.SPListItem.RoleAssignments.ID, 
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveDoc.AveSPItem.SPListItem.RoleAssignments));
            //}
        }

        public void BackupManifestDocumentHeader(ManifestDocumentSnapshot manifestSnapshot, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath, int backupFileType, bool needRecordStubId = false)
        {
            if (manifestSnapshot == null)
            {
                throw new ArgumentNullException(nameof(manifestSnapshot));
            }

            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_DOCUMENT.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, manifestSnapshot.IsSystemFile.ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, string.Empty);
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "false");
            long targetSize = manifestSnapshot.DocumentSize > 0 ? manifestSnapshot.DocumentSize : entity.DocumentSize;
            AddBackupFileHeaderAttribute(KeyWord.SIZE, targetSize.ToString(CultureInfo.InvariantCulture));
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName ?? string.Empty);
            AddBackupFileHeaderAttribute(KeyWord.HasUniqueRoleAssignments, manifestSnapshot.HasUniqueRoleAssignments.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BackupFileType, backupFileType.ToString());
            AddBackupFileHeaderAttribute(KeyWord.SiteUrl, manifestSnapshot.Site?.Url ?? entity.SiteUrl ?? string.Empty);
            AddBackupFileHeaderAttribute(KeyWord.WebId, entity.WebID == Guid.Empty ? Guid.Empty.ToString() : entity.WebID.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ListId, manifestSnapshot.List?.Id.ToString() ?? entity.ListID.ToString());

            if (!string.IsNullOrEmpty(entity.StubInfo))
            {
                if (needRecordStubId && string.IsNullOrEmpty(entity.StubId))
                {
                    Guid stubId = Guid.NewGuid();
                    string stubIdString = stubId.ToString().Replace("-", string.Empty, StringComparison.Ordinal);
                    string realStubIdString = string.Concat(stubIdString, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
                    entity.StubId = realStubIdString;
                }
                var stubInfo = GetStubInfo(entity.StubInfo, entity.StubId ?? manifestSnapshot.StubId ?? string.Empty);
                AddBackupFileHeaderAttribute(KeyWord.StubInfo, stubInfo);
            }

            if (manifestSnapshot.CreatedTime.HasValue)
            {
                AddBackupFileHeaderAttribute(KeyWord.Created, manifestSnapshot.CreatedTime.Value.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            if (manifestSnapshot.ModifiedTime.HasValue)
            {
                AddBackupFileHeaderAttribute(KeyWord.Modified, manifestSnapshot.ModifiedTime.Value.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(manifestSnapshot.AuthorString))
            {
                AddBackupFileHeaderAttribute(KeyWord.Author, manifestSnapshot.AuthorString);
            }
            if (!string.IsNullOrWhiteSpace(manifestSnapshot.EditorString))
            {
                AddBackupFileHeaderAttribute(KeyWord.Editor, manifestSnapshot.EditorString);
            }
        }

        public void AddBackUpFileHeaderEditor(AveSPDoc aveDoc)
        {
            try
            {
                string editor = aveDoc.AveSPItem.SPListItem.ModifiedBy?.Email;
                if (string.IsNullOrWhiteSpace(editor))
                {
                    editor = SplitAuthorAndEditor(aveDoc.AveSPItem.SPListItem["Modified_x0020_By"]?.ToString());
                }
                AddBackupFileHeaderAttribute(KeyWord.Editor, editor);
            } catch (Exception e)
            {
                mLog.Warn($"Get Editor faild,exception:{e}");
            } 
        }

        public void AddBackUpFileHeaderAuthor(AveSPDoc aveDoc)
        {
            try
            {
                string author = aveDoc.AveSPItem.SPListItem.Author?.Email;
                if (string.IsNullOrWhiteSpace(author))
                {
                    author = SplitAuthorAndEditor(aveDoc.AveSPItem.SPListItem["Created_x0020_By"]?.ToString());
                }
                AddBackupFileHeaderAttribute(KeyWord.Author, author);
            }
            catch (Exception e)
            {
                mLog.Warn($"Get Author faild,exception:{e}");
            } 
        }

        public void BackupDocumentVersionHeader(AveSPDoc aveDoc, CacheNode parent, long size, ArchiveApproveReport entity, AveSPFolder parentFolder, string ruleName, string subJobId, string mediaName, string fullPath, int backupFileType)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_DOCUMENT.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, (parentFolder.Path.EndsWith("\\{System Folder}", StringComparison.OrdinalIgnoreCase) || (aveDoc.AveSPItem != null && aveDoc.AveSPItem.IsSystemFileOrFolder)).ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, "");
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "true");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());//we do not add it because it can make error in deletion version
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            AddBackupFileHeaderAttribute(KeyWord.BackupFileType, backupFileType.ToString());
            try
            {
                var userData = aveDoc.AveSPItem.GetUserData();
                try
                {
                    AddBackupFileHeaderAttribute(KeyWord.Created, DateTime.Parse(userData["Modified"].ToString()).Ticks.ToString());
                    AddBackupFileHeaderAttribute(KeyWord.Modified, DateTime.Parse(userData["Modified"].ToString()).Ticks.ToString());
                }
                catch (Exception e)
                {
                    mLog.Warn($"Get Created or Modified faild,exception:{e}");
                }
                try
                {
                    string author = string.Empty;
                    string editor = string.Empty;
                    var allUsers = aveDoc.AveSPWeb.SPWeb.AllUsers;
                    if (userData != null && allUsers.GetByID((int)userData["Author"]).Email != null)
                    {
                        author = allUsers.GetByID((int)userData["Author"]).Email.ToString();
                    }
                    if (userData != null && allUsers.GetByID((int)userData["Editor"]).Email != null)
                    {
                        editor = allUsers.GetByID((int)userData["Editor"]).Email.ToString();
                    }
                    if (string.IsNullOrEmpty(author))
                    {
                        author = SplitAuthorAndEditor(aveDoc.AveSPItem.SPListItem["Created_x0020_By"]?.ToString());
                    }
                    if(string.IsNullOrEmpty(editor))
                    {
                        editor = SplitAuthorAndEditor(aveDoc.AveSPItem.SPListItem["Modified_x0020_By"]?.ToString());
                    }
                    AddBackupFileHeaderAttribute(KeyWord.Author, author);
                    AddBackupFileHeaderAttribute(KeyWord.Editor, editor);
                }
                catch (Exception e)
                {
                    mLog.Warn($"Get Editor or Author faild,exception:{e}");
                }

            }
            catch (Exception e)
            {
                mLog.Info($"Init time header failed {e}");
            }
            //if (!aveDoc.AveSPItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = aveDoc.AveSPItem.IsSystemFileOrFolder ? aveDoc.AveSPItem.AveSPList.ScopeId : aveDoc.AveSPItem.SPListItem.RoleAssignments.ID, 
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //    SetEndUserPermission(backupPermission.GetEndUserPermssion(aveDoc.AveSPItem.SPListItem.RoleAssignments));
            //}
        }

        public void BackupManifestDocumentVersionHeader(ManifestDocumentSnapshot manifestSnapshot, ArchiveApproveReport entity, string ruleName, string subJobId, string mediaName, string fullPath, int backupFileType)
        {
            if (manifestSnapshot == null)
            {
                throw new ArgumentNullException(nameof(manifestSnapshot));
            }

            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_DOCUMENT.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, manifestSnapshot.IsSystemFile.ToString().ToLower(CultureInfo.CurrentCulture));
            AddBackupFileHeaderAttribute(KeyWord.TIME, string.Empty);
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.ISVERSION, "true");
            long targetSize = manifestSnapshot.DocumentSize > 0 ? manifestSnapshot.DocumentSize : entity.DocumentSize;
            AddBackupFileHeaderAttribute(KeyWord.SIZE, targetSize.ToString(CultureInfo.InvariantCulture));
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName ?? string.Empty);
            AddBackupFileHeaderAttribute(KeyWord.BackupFileType, backupFileType.ToString());

            if (manifestSnapshot.CreatedTime.HasValue)
            {
                AddBackupFileHeaderAttribute(KeyWord.Created, manifestSnapshot.CreatedTime.Value.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            if (manifestSnapshot.ModifiedTime.HasValue)
            {
                AddBackupFileHeaderAttribute(KeyWord.Modified, manifestSnapshot.ModifiedTime.Value.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(manifestSnapshot.AuthorString))
            {
                AddBackupFileHeaderAttribute(KeyWord.Author, manifestSnapshot.AuthorString);
            }
            if (!string.IsNullOrWhiteSpace(manifestSnapshot.EditorString))
            {
                AddBackupFileHeaderAttribute(KeyWord.Editor, manifestSnapshot.EditorString);
            }
        }

        public void BackupAttaHeader(AveSPItem parentItem, ArchiveApproveReport entity, long size, string ruleName, string subJobId, string mediaName, string fullPath, int backupFileType)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, entity.LeafName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_ATTACHMENTS.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.SYSTEMFILE, false.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, "");
            AddBackupFileHeaderAttribute(KeyWord.NODEGUID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.LEVEL, entity.Level.ToString());
            AddBackupFileHeaderAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ID, entity.NodeId);
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.URL, entity.FullPath);
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, fullPath);
            AddBackupFileHeaderAttribute(KeyWord.IsAppData, entity.IsAppData.ToString());
            AddBackupFileHeaderAttribute(KeyWord.AppDataName, entity.AppDataName == null ? string.Empty : entity.AppDataName);
            //备份attachment时需要记录下其所属item的唯一标识，用来删除时减少load item的次数
            //AddBackupFileHeaderAttribute(KeyWord.ParentId, parentItem.Id.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ParentId, entity.ParentId.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BackupFileType, backupFileType.ToString());
            //if (!attachment.AveSPItem.HasUniqueRoleAssignments)
            //{
            //    EndUserPermission permission = new EndUserPermission() { scopeId = attachment.AveSPItem.ScopeId, isInheritPermission = true, users = new List<string>() };
            //    SetEndUserPermission(permission);
            //}
            //else
            //{
            //SetEndUserPermission(backupPermission.GetEndUserPermssion(parentItem.SPListItem.RoleAssignments));
            //}
            try
            {
                AddBackupFileHeaderAttribute(KeyWord.Created, DateTime.Parse(parentItem.SPListItem["Created"].ToString()).Ticks.ToString());
                AddBackupFileHeaderAttribute(KeyWord.Modified, DateTime.Parse(parentItem.SPListItem["Modified"].ToString()).Ticks.ToString());
                //AddBackupFileHeaderAttribute(KeyWord.Author, SplitAuthorAndEditor(parentItem.SPListItem["Created_x0020_By"].ToString()));
                //AddBackupFileHeaderAttribute(KeyWord.Editor, SplitAuthorAndEditor(parentItem.SPListItem["Modified_x0020_By"].ToString()));
            }
            catch (Exception e)
            {
                mLog.Info($"Init time header failed {e}");
            }
        }

        public XmlElement BackupHeader(string fullpath)
        {
            return WriteFileHeader(fileHeaderAttribute, GetProperties(fullpath));
        }

        public void BackupSecondFileHeader(XmlElement fileHeader, FileHeaderStatus status)
        {
            fileHeader.SetAttribute("fileHeaderType", ((int)FileHeaderType.Second).ToString());
            XmlElement stubInfo = (XmlElement)fileHeader.ChildNodes[0];
            stubInfo.SetAttribute("status", status.ToString());
            BackupStream.WriteHead(fileHeader.OuterXml);
        }
        public string GetProperties(string apUrl)
        {
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("HeaderExtraAttribute");
            headerExtraAttribute.SetAttribute("APUrl", apUrl);
            return headerExtraAttribute.OuterXml;
        }
        private string GetStubInfo(string stubType,string stubId = "")
        {
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("StubInfo");
            headerExtraAttribute.SetAttribute("StubType", stubType);
            headerExtraAttribute.SetAttribute("StubId", stubId);
            return headerExtraAttribute.OuterXml;
        }

        private XmlElement WriteFileHeader(Hashtable attributes, string innerXml)
        {
            return GenerateFileHeaderAndExecuteAction(attributes, innerXml, () => BackupStream.WriteHead(fileHeaderXml.OuterXml));
        }



        /// <summary>
        /// site ,web ,list ,folder FileTail
        /// </summary>
        /// <param name="successful"></param>
        /// <returns></returns>
        public long BackupTail(bool successful)
        {
            return BackupTail("", successful);
        }

        /// <summary>
        /// web,doc,item,attachment FileTail
        /// </summary>
        /// <param name="tail"></param>
        /// <param name="successful"></param>
        /// <returns></returns>
        public long BackupTail(string tail, bool successful)
        {
            FileSender.HandleTail(GenerateTailWithState(tail, successful));
            return 0;
        }

        private string GenerateTailWithState(string tail, bool successful)
        {
            int index = tail.IndexOf("<BackupDataExtraInfo", StringComparison.OrdinalIgnoreCase);
            string attributes = string.Empty;
            string extraInfo = string.Empty;
            if (index > 0)
            {
                attributes = tail.Substring(0, index);
                extraInfo = tail.Substring(index);
            }
            else
            {
                attributes = tail;
            }
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("FileTail");
            e.SetAttribute("extraInfo", extraInfo);
            e.InnerXml = attributes;
            if (!successful)
            {
                e.SetAttribute("failed", "true");
            }
            return e.OuterXml;
        }
    }

    enum FileHeaderType
    {
        First = 1,
        Second = 2
    }

    enum FileHeaderStatus
    {
        Failed = 1,
        Complete = 2
    }

    public class FileAtrributeInfo
    {
        /// <summary>
        /// property collection of a file，listitem or attachment
        /// </summary>
        private readonly List<string> mAttributeColl = new List<string>();
        private readonly Dictionary<string, string> mFullTextindex = new Dictionary<string, string>();

        public bool IsSystemFile { set; get; }

        public void AddProperty(string prop)
        {
            mAttributeColl.Add(prop);
        }

        public bool ContainFullTextAttribute(string key)
        {
            if (mFullTextindex.ContainsKey(key))
                return true;
            else
                return false;
        }

        public void AddFullTextProperty(string key, string prop)
        {
            if (!mFullTextindex.ContainsKey(key))
            {
                mFullTextindex.Add(key, prop);
            }
        }
        public string ExtraId { get; set; }

        /// <summary>
        /// DisplayName of the item
        /// </summary>
        public string ExtraTitle { set; get; }

        /// <summary>
        /// Add For NewsFeed Post and Reply
        /// </summary>
        public string PostId { get; set; }


        /// <summary>
        /// Add For NewsFeed Post and Reply
        /// </summary>
        public long NewsFeedCreatedTime { get; set; }

        public string Crc64 { get; set; }

        /// <summary>
        /// extra info send to media
        /// </summary>
        public override string ToString()
        {
            var strbuilder = new StringBuilder();
            var xmldoc = new XmlDocument();
            if (IsSystemFile)
            {
                XmlElement systemFileElement = xmldoc.CreateElement("IsSystemFile");
                systemFileElement.InnerText = "true";
                strbuilder.Append(systemFileElement.OuterXml);
            }
            XmlElement titleInfo = xmldoc.CreateElement("Title");
            titleInfo.InnerText = ExtraTitle;
            strbuilder.Append(titleInfo.OuterXml);

            string titleAttr = "Title" + ((Char)0x12).ToString();
            XmlElement itemElement = xmldoc.CreateElement("Attribute");
            itemElement.InnerText = titleAttr + ExtraTitle;
            strbuilder.Append(itemElement.OuterXml);
            foreach (string tmp in this.mAttributeColl)
            {
                if (tmp.StartsWith(titleAttr, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                itemElement.InnerText = tmp;
                strbuilder.Append(itemElement.OuterXml);
                //strbuilder.Append("<Attribute>").Append(tmp).Append("</Attribute>");
            }

            XmlElement backupDataExtraInfo = xmldoc.CreateElement("BackupDataExtraInfo");
            backupDataExtraInfo.SetAttribute("version", "5.2");
            xmldoc.AppendChild(backupDataExtraInfo);

            XmlElement idElement = xmldoc.CreateElement("KeyAndValue");
            idElement.SetAttribute("key", "ID");
            idElement.SetAttribute("value", this.ExtraId);

            XmlElement titleElement = xmldoc.CreateElement("KeyAndValue");
            titleElement.SetAttribute("key", "Title");
            titleElement.SetAttribute("value", ExtraTitle);

            //Media要求如果PostID没有值，就不发送这个属性
            if (!string.IsNullOrEmpty(PostId))
            {
                XmlElement postIdElement = xmldoc.CreateElement("PostId");
                postIdElement.InnerText = PostId;
                strbuilder.Append(postIdElement.OuterXml);
            }

            if (!string.IsNullOrEmpty(Crc64))
            {
                XmlElement postIdElement = xmldoc.CreateElement("CRC64");
                postIdElement.InnerText = Crc64;
                strbuilder.Append(postIdElement.OuterXml);
            }

            XmlElement newsFeedCreatedTimeElement = xmldoc.CreateElement("CreateTime");
            newsFeedCreatedTimeElement.InnerText = NewsFeedCreatedTime.ToString();
            strbuilder.Append(newsFeedCreatedTimeElement.OuterXml);

            backupDataExtraInfo.AppendChild(idElement);
            backupDataExtraInfo.AppendChild(titleElement);

            strbuilder.Append(xmldoc.OuterXml);

            return strbuilder.ToString();
        }
    }

    internal class KeyWord
    {
        internal static string TYPE = "type";
        internal static string PATH = "path";
        internal static string HEADERTYPE = "fileHeaderType";
        internal static string TIME = "archivedTime";
        internal static string ID = "spId";
        internal static string RowId = "rowId";
        internal static string LEVEL = "level";
        internal static string VERSION = "UIVersion";
        internal static string WEBAPP = "webApp";
        internal static string PROFILE = "isMyProfileList";
        internal static string NODEGUID = "nodeGuid";
        internal static string SYSTEMFILE = "isSystemFile";
        internal static string BACKUPTYPE = "backupType";
        internal static string SiteUrl = "siteUrl";
        internal static string WebId = "webId";
        internal static string ListId = "listId";
        internal static string ISVERSION = "isVersion";
        internal static string MYLEVEL = "myLevel";
        internal static string SIZE = "size";
        internal static string URL = "url";
        internal static string RULENAME = "ruleName";
        internal static string SUBJOBID = "subJobId";
        internal static string MEDIANAME = "mediaName";
        internal static string FULLPATH = "fullPath";//for Error page ,give a FullPath
        internal static string scopeId = "scopeId";
        internal static string isInheritPermission = "isInheritPermission";
        internal static string permissions = "permissions";
        internal static string CompatibilityLevel = "compatibilityLevel";  //SAAS-10848 在创建SiteCollection的时候，需要用到这几个属性。
        internal static string LCID = "lcid";
        internal static string Owner = "owner";
        internal static string Template = "template";
        internal static string Title = "title";
        internal static string AppDataName = "appDataName";
        internal static string IsAppData = "isAppData";
        internal static string ParentId = "parentId"; //SAAS-23014 删除attachment时，获取所属listItem时，此属性为判断条件
        internal static string RelativeDataJobId = "relativeDataJobId"; //SAAS-32843 删除Related Document 过程使用
        internal static string DoDelete = "DoDelete";
        internal static string DeleteRelatedRecords = "DeleteRelatedRecords";
        internal static string HasUniqueRoleAssignments = "HasUniqueRoleAssignments";
        internal static string BackupFileType = "BackupFileType";
        internal static string IsRepeatProcess = "IsRepeatProcess";

        internal static string Created = "Created";
        internal static string Modified = "Modified";
        internal static string Author = "Author";
        internal static string Editor = "Editor";
        internal static string StubInfo = "stubInfo";
    }

}