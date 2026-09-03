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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion using directives

    public abstract class MetaData
    {
        [ConcordanceMetaData("FileName")]
        public String FileName { get; set; }

        [ConcordanceMetaData("Size")]
        public String ContentSizeForDisplay
        {
            get
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

        [ConcordanceMetaData("ExportPath")]
        public String ExportPath { get; set; }

        [ConcordanceMetaData("MetadataInfo")]
        public HashSet<MetaDataItemInfo> MetadataInfo { get; set; }

        [ConcordanceMetaData("Attachments")]
        public List<String> Attachments { get; set; }

        [AutonomyMetaData("#DREFIELD", "")]
        public HashSet<MetaDataItemInfo> MetaDataItemInfoSet { get; set; }

        public String DataFileExtensionName { get; set; }

        public MetaDataFormat Format { get; set; }

        [HoldServiceMetaDataAttribute("ContentSize")]
        public Int64 ContentSize { get; set; }
        [CsvMetaData("MetadataInfo")]
        public List<MetaDataItemInfo> CsvMetadataInfo { get; set; }

        ///// <summary>
        ///// This field only used in MHT format
        ///// </summary>
        //public String BodyHtml { get; set; }

        //public Encoding BodyEncoding { get; set; }
        //public String Subject { get; set; }
        //public Encoding SubjectEncoding { get; set; }

        //ItemAttachmentCollection attachments = new ItemAttachmentCollection();
        //public ItemAttachmentCollection Attachments { get { return this.attachments; } }

        /// <summary>
        /// Default use the zip compression method
        /// </summary>
        //public CompressionType CompressionType { get; set; }
        //public EncryptionAlgorithm EncryptionAlgorithm { get; set; }
        //public Boolean IsCompression { get; set; }
        //public Boolean IsEncryption { get; set; }
    }
}