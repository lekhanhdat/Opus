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
using AvePoint.GCommon.Media.StorageService;

namespace RAExportCommon
{
    //public interface IAveVaultExportPathGenerator
    //{
    //    VaultExportInfo GeneratDocFilePath(string jobId, IAveSPItem item);

    //    VaultExportInfo GenerateItemFilePath(string jobId, IAveSPItem item);

    //    VaultExportInfo GenerateFolderFilePath(string jobId, IAveSPItem item);

    //    VaultExportInfo GenerateAttachmentFilePath(string jobId, IAveSPAttachment attachment);
    //}

    public class SiteCollectionLevelPathGeneratorInfo
    {
        private string mPhysicalDeviceDtoId = string.Empty;

        public string JobId { get; set; }

        public AveSPSite Site { get; set; }

        /// <summary>
        /// 此属性用于多个export同时导出时使用，实例化IVaultExport时，需对应选择多export location 的实例化方式。
        /// </summary>
        public string PhysicalDeviceDtoId
        {
            get { return mPhysicalDeviceDtoId; }
            set { mPhysicalDeviceDtoId = value; }
        }
    }

    public class WebLevelPathGeneratorInfo
    {
        private string mPhysicalDeviceDtoId = string.Empty;

        public string JobId { get; set; }

        public AveSPWeb Web { get; set; }

        /// <summary>
        /// 此属性用于多个export同时导出时使用，实例化IVaultExport时，需对应选择多export location 的实例化方式。
        /// </summary>
        public string PhysicalDeviceDtoId
        {
            get { return mPhysicalDeviceDtoId; }
            set { mPhysicalDeviceDtoId = value; }
        }
    }

    public class ListLevelPathGeneratorInfo
    {
        private string mPhysicalDeviceDtoId = string.Empty;

        public string JobId { get; set; }

        public AveSPList List { get; set; }

        /// <summary>
        /// 此属性用于多个export同时导出时使用，实例化IVaultExport时，需对应选择多export location 的实例化方式。
        /// </summary>
        public string PhysicalDeviceDtoId
        {
            get { return mPhysicalDeviceDtoId; }
            set { mPhysicalDeviceDtoId = value; }
        }
    }

    public class FolderLevelPathGeneratorInfo : ItemLevelPathGeneratorInfo
    {
    }

    public class ItemLevelPathGeneratorInfo
    {
        private string mPhysicalDeviceDtoId = string.Empty;

        public string JobId { get; set; }

        public AveSPItem Item { get; set; }

        /// <summary>
        /// 此属性用于多个export同时导出时使用，实例化IVaultExport时，需对应选择多export location 的实例化方式。
        /// </summary>
        public string PhysicalDeviceDtoId
        {
            get { return mPhysicalDeviceDtoId; }
            set { mPhysicalDeviceDtoId = value; }
        }
    }

    public class AttachmentLevelPathGeneratorInfo
    {
        private string mPhysicalDeviceDtoId = string.Empty;

        public string JobId { get; set; }

        public AveSPAttachment Attachment { get; set; }

        /// <summary>
        /// 此属性用于多个export同时导出时使用，实例化IVaultExport时，需对应选择多export location 的实例化方式。
        /// </summary>
        public string PhysicalDeviceDtoId
        {
            get { return mPhysicalDeviceDtoId; }
            set { mPhysicalDeviceDtoId = value; }
        }
    }

    public class VaultExportInfo
    {
        public string JobID { get; set; }

        /// <summary>
        /// content folder name
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// content name
        /// </summary>
        public string ContentFilePath { get; set; }

        //public string FolderPathForMetaData { get; set; }

        public string ItemID { get; set; }

        /// <summary>
        /// For Deloitte
        /// </summary>
        public string PhysicalDevicePath { get; set; }

        /// <summary>
        /// metaData name
        /// </summary>
        public string MetaDataFileName { get; set; }

        /// <summary>
        /// metaData file path
        /// </summary>
        public string MetaDataFilePath { get; set; }

        /// <summary>
        /// mht name
        /// </summary>
        public string MhtFilePath { get; set; }

        /// <summary>
        /// only for Vault Rule, this can Multiple Export.
        /// </summary>
        internal string DeviceDtoId { get; set; }

        public string FullURL { get; set; }
        public string Extension { get; set; }
    }
}
