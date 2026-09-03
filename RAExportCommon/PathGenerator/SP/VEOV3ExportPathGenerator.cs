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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Backup;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RAExportCommon
{
    public class VEOV3ExportPathGenerator : ExportpathGeneratorBase
    {
        private const string EXPORTCONTENTPATH_WITHOUTEXTENSION = "{0}_v{1}";//e.g. fileName_Version
        private const string EXPORTCONTENTPATH = "{0}_v{1}.{2}";//e.g. fileName_Version.Extension
        private const string SITES = "\\Sites\\";
        private const char SLASH = '/';
        private const char BACKSLASH = '\\';
        private const char HASH = '#';
        private const string REPLACECHAR1 = "://";
        private const string REPLACECHAR2 = ":";
        private const string LISTS = "{0}\\Lists\\{1}";
        private const string DEFAULTLISTS = "Lists\\";
        private string _teamsAddress = string.Empty;

        public VEOV3ExportPathGenerator(string teamsAddress)
        {
            _teamsAddress = teamsAddress;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GeneratSiteCollectionExportInfo(SiteCollectionLevelPathGeneratorInfo siteInfo)
        {
            VaultExportInfo info = new()
            {
                JobID = siteInfo.JobId,
                FolderPath = NormalizedPath(SecurityUtils.SafeCombinePath(_teamsAddress, GeneratSitePath(siteInfo.Site))),
                FullURL = siteInfo.Site.SPSite.Url
            };
            return info;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GeneratWebExportInfo(WebLevelPathGeneratorInfo webInfo)
        {
            string sitePath = GeneratSitePath(webInfo.Web.ParentSite);
            var siteRelativeUrl = webInfo.Web.ParentSite.SPSite.ServerRelativeUrl;
            var webRelativeUrl = webInfo.Web.SPWeb.ServerRelativeUrl;
            string webRelativePath = webRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            if (siteRelativeUrl.Length > 0)
            {
                var tempListRelativeUrl = webRelativeUrl.Substring(siteRelativeUrl.Length + 1);
                webRelativePath = tempListRelativeUrl.IndexOf(SLASH) > 0 ? tempListRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH) : tempListRelativeUrl;
            }
            var folderPath = SITES + webRelativePath.Replace(BACKSLASH.ToString(), SITES);
            VaultExportInfo info = new VaultExportInfo
            {
                JobID = webInfo.JobId,
                FolderPath = NormalizedPath(SecurityUtils.SafeCombinePath(_teamsAddress, sitePath, folderPath.Trim(BACKSLASH))),
                FullURL = webInfo.Web.SPWeb.Url
            };
            return info;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GeneratListExportInfo(ListLevelPathGeneratorInfo listInfo)
        {
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
            VaultExportInfo info = new VaultExportInfo
            {
                JobID = listInfo.JobId,
                FolderPath = NormalizedPath(SecurityUtils.SafeCombinePath(_teamsAddress, sitePath, listRelativePath)),
                FullURL = new Uri(GetWebappUrl(listInfo.List.ParentSite) + EncodePath(listInfo.List.ServerRelativeUrl)).AbsoluteUri
            };
            return info;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GenerateFolderExportInfo(FolderLevelPathGeneratorInfo folderInfo)
        {
            if (folderInfo.Item.SPListItem == null)
            {
                throw new Exception("Vault is unsupported system object.");
            }
            string sitePath = GeneratSitePath(folderInfo.Item.ParentSite);
            var siteRelativeUrl = folderInfo.Item.ParentSite.SPSite.ServerRelativeUrl;
            var listRelativeUrl = folderInfo.Item.AveSPList.ServerRelativeUrl;
            string ServerRelativePath = folderInfo.Item.BaseItemInfo.ServerRelativeUrl.Replace(SLASH, BACKSLASH);
            string listRelativePath = listRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH);
            if (siteRelativeUrl.Length > 0)
            {
                var tempListRelativeUrl = listRelativeUrl.Substring(siteRelativeUrl.Length + 1);
                listRelativePath = tempListRelativeUrl.IndexOf(SLASH) > 0 ? tempListRelativeUrl.Replace(SLASH, BACKSLASH).Trim(BACKSLASH) : tempListRelativeUrl;
                ServerRelativePath = ServerRelativePath.Substring(siteRelativeUrl.Length + 1);
            }
            ServerRelativePath = GetFolderPath(listRelativePath, ServerRelativePath);
            VaultExportInfo info = new VaultExportInfo
            {
                JobID = folderInfo.JobId,
                FolderPath = NormalizedPath(SecurityUtils.SafeCombinePath(_teamsAddress, sitePath, ServerRelativePath)),
                FullURL = new Uri(GetWebappUrl(folderInfo.Item.ParentSite) + EncodePath(folderInfo.Item.BaseItemInfo.ServerRelativeUrl)).AbsoluteUri,
                ContentFilePath = string.Empty
            };
            return info;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
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
            string ServerRelativePath = docInfo.Item.BaseItemInfo.ServerRelativeUrl.Replace(SLASH, BACKSLASH);
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
            info.FolderPath = NormalizedPath(SecurityUtils.SafeCombinePath(_teamsAddress, sitePath, folderPath));

            string fileName = NameFactory.GetName(docInfo.Item.SPListItem.Name);
            string fileExtension = NameFactory.GetExtensionName(docInfo.Item.SPListItem.Name);
            string versionNumber = NameFactory.ParseVersionString(docInfo.Item.Version);
            if (string.IsNullOrEmpty(fileExtension))
            {
                info.ContentFilePath = NormalizedPath(string.Format(EXPORTCONTENTPATH_WITHOUTEXTENSION, fileName, versionNumber));
            }
            else
            {
                info.ContentFilePath = NormalizedPath(string.Format(EXPORTCONTENTPATH, fileName, versionNumber, fileExtension));
            }
            info.FullURL = new Uri(GetWebappUrl(docInfo.Item.ParentSite) + EncodePath(docInfo.Item.BaseItemInfo.ServerRelativeUrl)).AbsoluteUri;

            return info;
        }

        public override VaultExportInfo GenerateItemExportInfo(ItemLevelPathGeneratorInfo itemInfo)
        {
            return new VaultExportInfo();
        }

        public override VaultExportInfo GenerateAttachmentExportInfo(AttachmentLevelPathGeneratorInfo attachmentInfo)
        {
            return new VaultExportInfo();
        }

        private string EncodePath(string originalPath)
        {
            var segments = originalPath.Split("/");

            var encodedParts = segments.Select(Uri.EscapeDataString);
            return string.Join("/", encodedParts);
        }

        private string NormalizedPath(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        private string GetFolderPath(string listRelativeUrl, string itemRelativeUrl = "")
        {
            string fPath = string.Empty;
            string result = string.Empty;
            if (!string.IsNullOrEmpty(itemRelativeUrl) && !listRelativeUrl.Equals(itemRelativeUrl))
            {
                fPath = itemRelativeUrl.Remove(0, listRelativeUrl.Length);
            }
            if (listRelativeUrl.StartsWith(DEFAULTLISTS))
            {
                listRelativeUrl = listRelativeUrl.Remove(0, DEFAULTLISTS.Length);
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

        private static string GetWebappUrl(AveSPSite aveSite)
        {
            Uri webAppUri = new Uri(aveSite.SPSite.Url);
            string webAppUrl;
            string siteUrl = aveSite.SPSite.Url;
            int lengh = 0;
            if (siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                lengh = "https://".Length;
            }
            else
            {
                lengh = "http://".Length;
            }
            int indexOfSlash = siteUrl.IndexOf("/", lengh, StringComparison.OrdinalIgnoreCase);
            webAppUrl = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppUrl = siteUrl.Substring(0, indexOfSlash);
            }
            webAppUri = new Uri(webAppUrl);
            return webAppUri.AbsoluteUri.Trim('/');
        }
    }
}
