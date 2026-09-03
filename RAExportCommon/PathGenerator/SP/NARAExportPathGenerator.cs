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
using AvePoint.GCommon;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAExportCommon
{
    public class NARAExportPathGenerator : ExportpathGeneratorBase
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EXPORTCONTENTPATH_WITHOUTEXTENSION = "{0}_v{1}";//e.g. fileName_Version
        private const string EXPORTCONTENTPATH = "{0}_v{1}.{2}";//e.g. fileName_Version.Extension
        private const string EXPORTMETADATAFORMAT = "{0}_NARA.CSV";//e.g. itemId_Version.Extension.idx
        private const string EXPORTMHTFORMAT = "{0}_v{1}.mht";//e.g.itemname_Version.mht
        private const string SITES = "\\Sites\\";
        private const char SLASH = '/';
        private const char BACKSLASH = '\\';
        private const char HASH = '#';
        private const string REPLACECHAR1 = "://";
        private const string REPLACECHAR2 = ":";
        private string mConvertIllegalCharacterTo = "_";
        private const string LISTS = "{0}\\Lists\\{1}";
        private string _PhysicalDeviceDtoPath = string.Empty;
        private string _RevIMGlobalSettingColumnName;
        private string _teamsAddress = string.Empty;

        public NARAExportPathGenerator(String ConvertIllegalCharacterTo, String physicalDeviceDtoPath, string ColumnName, string teamsAddress)
        {
            _PhysicalDeviceDtoPath = physicalDeviceDtoPath;
            mConvertIllegalCharacterTo = ConvertIllegalCharacterTo;
            _RevIMGlobalSettingColumnName = ColumnName;
            _teamsAddress = teamsAddress;
        }

        private string GeneratSitePath(AveSPSite aveSite)
        {
            var siteUrl = aveSite.SPSite.Url;
            Uri siteUri = new Uri(siteUrl);

            var tempPath = siteUri.AbsoluteUri.ReplaceFirst(REPLACECHAR1, HASH.ToString());
            if (tempPath.IndexOf(REPLACECHAR2) > 0)
            {
                tempPath = tempPath.ReplaceFirst(REPLACECHAR2, HASH.ToString());
            }

            return tempPath.Replace(SLASH, HASH);
        }

        public override VaultExportInfo GeneratDocExportInfo(ItemLevelPathGeneratorInfo docInfo)
        {
            if (docInfo.Item.SPListItem == null)
            {
                throw new Exception("Vault is unsupported system object.");
            }

            string sitePath = GeneratSitePath(docInfo.Item.ParentSite);

            VaultExportInfo info = new VaultExportInfo();
            info.JobID = docInfo.JobId;
            var siteRelativeUrl = docInfo.Item.ParentSite.SPSite.ServerRelativeUrl;
            var listRelativeUrl = docInfo.Item.AveSPList.ServerRelativeUrl;
            string ServerRelativePath = docInfo.Item.BaseItemInfo.ServerRelativeUrl.Replace(SLASH, BACKSLASH);//RECO-639mergetest
            string listRelativePath = listRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            if (siteRelativeUrl.Length > 0)
            {
                var startToSub = siteRelativeUrl.Length == 1 && siteRelativeUrl.StartsWith(SLASH) ? siteRelativeUrl.Length : siteRelativeUrl.Length + 1;
                var tempListRelativeUrl = listRelativeUrl.Substring(startToSub);
                listRelativePath = tempListRelativeUrl.IndexOf(SLASH) > 0 ? tempListRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH) : tempListRelativeUrl;
                ServerRelativePath = ServerRelativePath.Substring(startToSub);
            }

            int index = ServerRelativePath.LastIndexOf(BACKSLASH);
            string folderPath = ServerRelativePath.Substring(0, index).Trim(BACKSLASH);
            folderPath = GetFolderPath(listRelativePath, folderPath);
            info.FolderPath = Path.Combine(docInfo.JobId, _teamsAddress, sitePath, folderPath);

            string fileName = PathValidation.ConverSpecialChar(docInfo.Item.RowId + "_" + NameFactory.GetName(docInfo.Item.SPListItem.Name));
            string fileExtension = NameFactory.GetExtensionName(docInfo.Item.SPListItem.Name);
            string versionNumber = NameFactory.ParseVersionString(docInfo.Item.Version);
            if (string.IsNullOrEmpty(fileExtension))
            {
                info.ContentFilePath = PathValidation.ConverSpecialChar(string.Format(EXPORTCONTENTPATH_WITHOUTEXTENSION, fileName, versionNumber));
            }
            else
            {
                info.ContentFilePath = PathValidation.ConverSpecialChar(string.Format(EXPORTCONTENTPATH, fileName, versionNumber, fileExtension));
            }
            info.MetaDataFilePath = Path.Combine(docInfo.JobId, GetMetadataRootNodeFilePath(sitePath));

            int listIndex = listRelativePath.LastIndexOf(BACKSLASH);
            string listUrlName = string.Empty;
            if (listIndex > 0)
            {
                listUrlName = listRelativePath.Substring(listRelativePath.LastIndexOf(BACKSLASH));
            }
            else
            {
                listUrlName = listRelativePath;
            }
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, GetMetadataRootNodeFilePath(sitePath));
            info.MhtFilePath = PathValidation.ConverSpecialChar(string.Format(EXPORTMHTFORMAT, docInfo.Item.SPListItem.Name, versionNumber));
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            return info;
        }

        public override VaultExportInfo GenerateItemExportInfo(ItemLevelPathGeneratorInfo itemInfo)
        {
            if (itemInfo.Item.SPListItem == null)
            {
                throw new Exception("Vault is unsupported system object.");
            }

            string sitePath = GeneratSitePath(itemInfo.Item.ParentSite);
            VaultExportInfo info = new VaultExportInfo();
            info.JobID = itemInfo.JobId;

            string webPath = itemInfo.Item.AveSPList.ParentWeb.SPWeb.ServerRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);

            string serverRelativePath = itemInfo.Item.SPListItem.Url.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            string listRelativePath = itemInfo.Item.AveSPList.ServerRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);

            int index = serverRelativePath.LastIndexOf(BACKSLASH);
            string folderPath = serverRelativePath.Substring(0, index).Trim(BACKSLASH);

            info.FolderPath = Path.Combine(itemInfo.JobId, _teamsAddress, sitePath, webPath, folderPath);
            info.MetaDataFilePath = Path.Combine(itemInfo.JobId, GetMetadataRootNodeFilePath(sitePath));
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, GetMetadataRootNodeFilePath(sitePath));
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Empty;
            return info;
        }

        public override VaultExportInfo GenerateFolderExportInfo(FolderLevelPathGeneratorInfo folderInfo)
        {
            if (folderInfo.Item.SPListItem == null)
            {
                throw new Exception("Vault is unsupported system object.");
            }
            string sitePath = GeneratSitePath(folderInfo.Item.ParentSite);
            VaultExportInfo info = new VaultExportInfo();
            info.JobID = folderInfo.JobId;

            var siteRelativeUrl = folderInfo.Item.ParentSite.SPSite.ServerRelativeUrl;
            var listRelativeUrl = folderInfo.Item.AveSPList.ServerRelativeUrl;
            string ServerRelativePath = folderInfo.Item.BaseItemInfo.ServerRelativeUrl.Replace(SLASH, BACKSLASH);//RECO-639mergetest
            string listRelativePath = listRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            if (siteRelativeUrl.Length > 0)
            {
                var tempListRelativeUrl = listRelativeUrl.Substring(siteRelativeUrl.Length + 1);
                listRelativePath = tempListRelativeUrl.IndexOf(SLASH) > 0 ? tempListRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH) : tempListRelativeUrl;
                ServerRelativePath = ServerRelativePath.Substring(siteRelativeUrl.Length + 1);
            }
            ServerRelativePath = GetFolderPath(listRelativePath, ServerRelativePath);
            info.FolderPath = folderInfo.JobId + (string.IsNullOrEmpty(_teamsAddress) ? "" : "\\" + _teamsAddress) +"\\"+sitePath+"\\"+ServerRelativePath;
            info.MetaDataFilePath = Path.Combine(folderInfo.JobId, GetMetadataRootNodeFilePath(sitePath));
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, GetMetadataRootNodeFilePath(sitePath));
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            info.Extension = _RevIMGlobalSettingColumnName;
            info.ContentFilePath = string.Empty;
            return info;
        }

        public override VaultExportInfo GenerateAttachmentExportInfo(AttachmentLevelPathGeneratorInfo attachmentInfo)
        {
            VaultExportInfo info = new VaultExportInfo();
            info.JobID = attachmentInfo.JobId;

            string sitePath = GeneratSitePath(attachmentInfo.Attachment.AveSPItem.ParentSite);
            string listRelativePath = attachmentInfo.Attachment.AveSPItem.AveSPList.ServerRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);

            #region
            string attachmentName = attachmentInfo.Attachment.Name;
            string realAttName = string.Empty;
            string itemNum = string.Empty;
            string attachmentRelativeUrl = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(attachmentName) && attachmentName.Contains("_.000") && !attachmentName.StartsWith(":", StringComparison.OrdinalIgnoreCase))
                {
                    itemNum = attachmentName.Substring(0, attachmentName.IndexOf("_", StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrEmpty(attachmentName))
                {
                    realAttName = attachmentName.Substring(attachmentName.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);
                }
                attachmentRelativeUrl = attachmentInfo.Attachment.ParentFolder.AveList.ServerRelativeUrl +
                                            "/Attachments/" +
                                            itemNum +
                                            "/" +
                                            realAttName;
            }
            catch (Exception e)
            {
                mLog.Warn("Error in get attachment url, Attachment name is : {0},Exception:{1}.", attachmentName, e.ToString());
            }
            #endregion
            IAveFile aveFile = attachmentInfo.Attachment.AveSPItem.AveSPList.ParentWeb.SPWeb.GetFile(attachmentInfo.Attachment.AveSPItem.Id, attachmentRelativeUrl);
            if (!aveFile.Exists)
            {
                throw new Exception("An error occurred while checking file exists. Maybe it does not exist in current SharePoint environment.");
            }

            string ServerRelativePath = aveFile.ServerRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            int index = ServerRelativePath.LastIndexOf(BACKSLASH);
            string folderPath = ServerRelativePath.Substring(0, index).Trim(BACKSLASH);

            info.FolderPath = Path.Combine(attachmentInfo.JobId, _teamsAddress, sitePath, folderPath);
            info.ContentFilePath = ServerRelativePath.Substring(index + 1).Trim(BACKSLASH);

            info.MetaDataFilePath = Path.Combine(attachmentInfo.JobId, GetMetadataRootNodeFilePath(sitePath));
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, GetMetadataRootNodeFilePath(sitePath));
            info.PhysicalDevicePath = _PhysicalDeviceDtoPath;
            return info;
        }

        public override VaultExportInfo GeneratListExportInfo(ListLevelPathGeneratorInfo listInfo)
        {
            VaultExportInfo info = new VaultExportInfo();
            string sitePath = GeneratSitePath(listInfo.List.ParentSite);
            var siteRelativeUrl = listInfo.List.ParentSite.SPSite.ServerRelativeUrl;
            var listRelativeUrl = listInfo.List.ServerRelativeUrl;
            string listRelativePath = listRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            if (siteRelativeUrl.Length > 0)
            {
                var tempListRelativeUrl = listRelativeUrl.Substring(siteRelativeUrl.Length + 1);
                listRelativePath = tempListRelativeUrl.IndexOf(SLASH) > 0 ? tempListRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH) : tempListRelativeUrl;
            }
            listRelativePath = GetFolderPath(listRelativePath);
            info.FolderPath = Path.Combine(listInfo.JobId, _teamsAddress, sitePath, listRelativePath);
            info.MetaDataFileName = string.Format(EXPORTMETADATAFORMAT, GetMetadataRootNodeFilePath(sitePath));
            info.MetaDataFilePath = Path.Combine(listInfo.JobId, GetMetadataRootNodeFilePath(sitePath));
            return info;
        }

        private string GetFolderPath(string listRelativeUrl, string itemRelativeUrl = "")
        {
            string fPath = string.Empty;
            string result = string.Empty;
            if (!string.IsNullOrEmpty(itemRelativeUrl) && !listRelativeUrl.Equals(itemRelativeUrl))
            {
                fPath = itemRelativeUrl.Remove(0, listRelativeUrl.Length);
            }
            if (listRelativeUrl.IndexOf(BACKSLASH) >= 0)
            {
                string subSitePath = listRelativeUrl.Substring(0, listRelativeUrl.LastIndexOf(BACKSLASH));
                string pth1 = SITES + subSitePath.Replace(BACKSLASH.ToString(), SITES);

                string libaryPath = listRelativeUrl.Substring(listRelativeUrl.LastIndexOf(BACKSLASH) + 1);
                result = string.Format(LISTS, pth1, libaryPath + fPath);
            }
            else
            {
                result = string.Format(LISTS, string.Empty, listRelativeUrl + fPath);
            }

            return result.Trim(BACKSLASH);
        }

        public override VaultExportInfo GeneratSiteCollectionExportInfo(SiteCollectionLevelPathGeneratorInfo siteInfo)
        {
            throw new NotImplementedException();
        }

        public override VaultExportInfo GeneratWebExportInfo(WebLevelPathGeneratorInfo webInfo)
        {
            throw new NotImplementedException();
        }

        private string GetMetadataRootNodeFilePath(string sitePath)
        {
            return string.IsNullOrEmpty(_teamsAddress) ? sitePath : _teamsAddress;
        }
    }
}
