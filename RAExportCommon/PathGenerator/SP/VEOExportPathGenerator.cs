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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Backup;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;

namespace RAExportCommon
{
    #region CodeReview

    #endregion
    public class VEOExportPathGenerator : ExportpathGeneratorBase
    {
        private const string EXPORTCONTENTFORMAT = "{0}_{1}_v{2}.veo"; //e.g. fileId_Version.Extension
        private const string EXPORTHIGHNAME = "{0}";//e.g. jobId


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GeneratListExportInfo(ListLevelPathGeneratorInfo listInfo)
        {
            VaultExportInfo info = new VaultExportInfo();
            info.FolderPath = info.MetaDataFilePath = string.Format(EXPORTHIGHNAME, listInfo.JobId);

            info.ContentFilePath = string.Format(listInfo.List.Id.ToString() + "_" + listInfo.List.Title + ".veo");

            if (listInfo.PhysicalDeviceDtoId != "")
            {
                info.DeviceDtoId = listInfo.PhysicalDeviceDtoId;
            }
            return info;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GeneratDocExportInfo(ItemLevelPathGeneratorInfo docInfo)
        {
            if (docInfo.Item.SPListItem == null)
            {
                throw new Exception("Vault is unsupported system object.");
            }
            VaultExportInfo info = new VaultExportInfo();
            info.FolderPath = info.MetaDataFilePath = string.Format(EXPORTHIGHNAME, docInfo.JobId);
            string fileExtension = NameFactory.GetExtensionName(docInfo.Item.SPListItem.Name);
            string versionNumber = NameFactory.ParseVersionString(docInfo.Item.Version);
            if (!string.IsNullOrEmpty(fileExtension))
            {
                info.ContentFilePath = string.Format(EXPORTCONTENTFORMAT, docInfo.Item.Id, NameFactory.GetName(docInfo.Item.SPListItem.Name), versionNumber, fileExtension);
            }

            if (docInfo.PhysicalDeviceDtoId != "")
            {
                info.DeviceDtoId = docInfo.PhysicalDeviceDtoId;
            }
            return info;
        }

        public override VaultExportInfo GenerateItemExportInfo(ItemLevelPathGeneratorInfo itemInfo)
        {
            return new VaultExportInfo();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public override VaultExportInfo GenerateFolderExportInfo(FolderLevelPathGeneratorInfo folderInfo)
        {
            VaultExportInfo info = new VaultExportInfo();
            info.FolderPath = info.MetaDataFilePath = string.Format(EXPORTHIGHNAME, folderInfo.JobId);

            string folderVersion = string.Empty;
            if (folderInfo.Item != null)
            {
                folderVersion = folderInfo.Item.Version / 512 + "." + folderInfo.Item.Version % 512;
            }

            info.ContentFilePath = folderInfo.Item.Id.ToString() + "_" + folderInfo.Item.SPListItem.Name + "_" + folderVersion + ".veo";

            if (folderInfo.PhysicalDeviceDtoId != "")
            {
                info.DeviceDtoId = folderInfo.PhysicalDeviceDtoId;
            }
            return info;
        }

        public override VaultExportInfo GenerateAttachmentExportInfo(AttachmentLevelPathGeneratorInfo attachmentInfo)
        {
            return new VaultExportInfo();
        }

        public override VaultExportInfo GeneratSiteCollectionExportInfo(SiteCollectionLevelPathGeneratorInfo siteInfo)
        {
            throw new NotImplementedException();
        }

        public override VaultExportInfo GeneratWebExportInfo(WebLevelPathGeneratorInfo webInfo)
        {
            throw new NotImplementedException();
        }
    }

    public static class NameFactory
    {




        private static Int32 folderNum = 1;
        private static Int32 fileNum = 1;
        private static Int32 FILENUM = 1000; //the total count under export folder
        private static String FOLDERNAME = "ExportFolder";

        public static string GetExportFolderName()
        {
            fileNum++;
            if (fileNum > FILENUM)
            {
                fileNum = 1;
                folderNum++;
            }
            return string.Format("{0}{1}", FOLDERNAME, folderNum);
        }

        //
        // Summary:
        //     去除file“.”后的后缀
        public static string GetName(string fileName)
        {
            if (fileName.Contains('.'))
            {
                return fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            else
            {
                return fileName;
            }
        }

        //
        // Summary:
        //     去除attachment name 前的“：”，如“1.00：a.doc” 转换成 “a.doc”
        public static string GetAttachmentname(string attachmentName)
        {
            return attachmentName.Substring(attachmentName.LastIndexOf(':') + 1);
        }

        //
        // Summary:
        //     得到file 的后缀
        public static string GetExtensionName(string fileName)
        {
            if (fileName.Contains('.'))
            {
                return fileName.Substring(fileName.LastIndexOf('.') + 1);
            }
            else
            {
                return string.Empty;
            }
        }

        //
        // Summary:
        //     从1024形式version 获取2.0形式的version
        public static string ParseVersionString(int UIVersion)
        {
            return string.Format("{0}.{1}", UIVersion / 512, UIVersion % 512);
        }

        //
        // Summary:
        //     从1024形式size获取到1MB形式的size
        public static string GetContentSizeForDisplay(long ContentSize)
        {
            StringBuilder result = new StringBuilder();
            if (ContentSize < 1024)
                result = result.AppendFormat("{0}Bytes", ContentSize);
            else if (ContentSize >= 1024 && ContentSize < 1024 * 1024)
                result = result.AppendFormat("{0:F}KB", ContentSize / 1024.0);
            else if (ContentSize >= 1024 * 1024 && ContentSize < 1024 * 1024 * 1024)
                result = result.AppendFormat("{0:F}MB", ContentSize / (1024 * 1024.0));
            else if (ContentSize >= 1024 * 1024 * 1024 && ContentSize < 1024L * 1024 * 1024 * 1024)
                result = result.AppendFormat("{0:F}GB", ContentSize / (1024 * 1024 * 1024.0));
            else
                result = result.AppendFormat("{0:F}TB", ContentSize / (1024L * 1024 * 1024 * 1024.0));
            return result.ToString();
        }
    }

    internal class ChildCounter
    {
        private Dictionary<Guid, List<Guid>> mNodes = new Dictionary<Guid, List<Guid>>();
        public int GetChildCount(Guid parentId, Guid childId)
        {
            if (parentId == null || childId == null
                || parentId == Guid.Empty || childId == Guid.Empty)
            {
                throw new Exception("These ID should not be null or empty");
            }
            if (mNodes.ContainsKey(parentId))
            {
                if (!mNodes[parentId].Contains(childId))
                {
                    mNodes[parentId].Add(childId);
                }
                return mNodes[parentId].IndexOf(childId, 0, mNodes[parentId].Count) + 1;
            }
            else
            {
                mNodes.Clear();
                List<Guid> list = new List<Guid>();
                list.Add(childId);
                mNodes.Add(parentId, list);
                return 1;
            }
        }
    }
}