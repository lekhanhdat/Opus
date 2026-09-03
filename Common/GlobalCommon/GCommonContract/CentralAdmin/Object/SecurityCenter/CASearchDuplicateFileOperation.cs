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




namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchDuplicateFileOperation : CAOperation
    {
        /// <summary>
        /// For GUI(标识用户是否选择了所有的File Extensions)
        /// </summary>
        [DataMember]
        public bool IsSelectAllFileExtensions { get; set; }
        [DataMember]
        public List<string> IncludeFileExtensions { get; set; }
        /// <summary>
        /// For GUI(标识用户是否选择了所有的File Names)
        /// </summary>
        [DataMember]
        public bool IsSelectAllFileNames { get; set; }
        [DataMember]
        public List<string> ExcludeFileNames { get; set; }
        [DataMember]
        public string FileNamePattern { get; set; }
        [DataMember]
        public bool SearchFile { get; set; }
        [DataMember]
        public bool SearchAttachemnt { get; set; }
        [DataMember]
        public double fileSizeCompareThreshold { get; set; }
        [DataMember]
        public int MinDuplicateNum { get; set; }

        public override string ToString()
        {
            return string.Format(@"searchFile:{0}, searchAttachment:{1}, fileName:{2}, threshold:{3}, minNum:{4}, 
                includeExtension:{5},  excludeFile:{6}", this.SearchFile, this.SearchAttachemnt, this.FileNamePattern, 
                this.fileSizeCompareThreshold, this.MinDuplicateNum, this.IncludeFileExtensions.Count, this.ExcludeFileNames.Count);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DuplicateFilesHolder : ResultBase
    {
        [DataMember]
        public FileKey FileKey { get; set; }
        [DataMember]
        public List<FileData> DuplicateFiles { get; set; }
        [DataMember]
        public decimal AverageSize { get; set; }
        [DataMember]
        public int FileNumber { get; set; }
        public DuplicateFilesHolder(FileKey key)
        {
            this.FileKey = key;
            this.DuplicateFiles = new List<FileData>();
        }
        public FileData AddFile(string name, string url, string version, string modifiedBy, long size, bool isFile)
        {
            FileData item = new FileData(name, url, version, modifiedBy, size, isFile);
            this.DuplicateFiles.Add(item);
            return item;
        }
        public void Clear()
        {
            this.DuplicateFiles.Clear();
        }
        public int GetFileCount()
        {
            return this.DuplicateFiles.Count;
        }

        public decimal GetAverageFileSize()
        {
            decimal num = 0m;
            foreach (FileData current in this.DuplicateFiles)
            {
                num += current.fileSize;
            }
            if (this.DuplicateFiles.Count > 0)
            {
                num /= this.DuplicateFiles.Count;
            }
            return Math.Round(num / 1048576m, 3);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FullUrl { get; set; }
        [DataMember]
        public string FileVersion { get; set; }
        [DataMember]
        public string ModifiedTime { get; set; }
        [DataMember]
        public bool isFile { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        public DateTime CreatedBy { get; set; }
        [DataMember]
        public string ParentUrl { get; set; }
        public string siteGuid;
        public string webGuid;
        public string listGuid;
        public string docGuid;
        [DataMember]
        public long fileSize { get; set; }
        public FileData(string name, string parentUrl, string version, string modifiedBy, long size, bool isFile)
        {
            this.fileSize = size;
            this.Name = name;
            this.ParentUrl = parentUrl;
            this.FileVersion = version;
            this.ModifiedBy = modifiedBy;
            this.isFile = isFile;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileKey
    {
        public class EqualityComparer : IEqualityComparer<FileKey>
        {
            private double _threshold;
            public bool Equals(FileKey x, FileKey y)
            {
                double xSizeInMB = Math.Round(x.size*1.0 / 1024 / 1024, 3);
                double ySizeInMB = Math.Round(y.size*1.0 / 1024 / 1024, 3);
                double num = (double)Math.Abs(xSizeInMB - ySizeInMB);
                double num2 = num / (double)Math.Min(xSizeInMB, ySizeInMB);
                if (num2 > 1.0)
                {
                    num2 = 1.0;
                }
                return x.fileName == y.fileName && (num2 <= this._threshold);
            }
            public int GetHashCode(FileKey x)
            {
                return x.fileName.GetHashCode();
            }
            public EqualityComparer(double fileSizeCompareThreshold)
            {
                this._threshold = fileSizeCompareThreshold;
            }
        }
        [DataMember]
        public string fileName;
        [DataMember]
        public long size;
        public FileKey(string fileName, long size)
        {
            this.size = size;
            this.fileName = (string.IsNullOrEmpty(fileName) ? string.Empty : fileName.Trim().ToLower());
        }
    }
}
