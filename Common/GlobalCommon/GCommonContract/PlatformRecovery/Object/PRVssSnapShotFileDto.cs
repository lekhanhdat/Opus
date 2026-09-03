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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRVssSnapShotFileDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Id")]
        public Guid Id { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "DataNodeFullPath")]
        public string FullPath { get; set; }
        [DataMember(IsRequired = true)]
        public string mRelativePath;
        [DataMember]
        [ColumnMapAttribute(DBColumn = "RelativePath")]
        public string RelativePath
        {
            get
            {
                return mRelativePath.TrimStart('\\');
            }
            set
            {
                mRelativePath = value.TrimStart('\\');
            }
        }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "State")]
        public PRSnapShotState State { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "Type")]
        public SnapShotFileType FileType { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "Path")]
        public string FilePath { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Specification")]
        public string FileSpecification { get; set; }
        [DataMember]
        public List<VssRangeInfo> mDiffInfos { get; set; }
        public PRVssSnapShotDto parent { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = "SnapShotId")]
        public string SnapShotId { get; set; }
        //properties
        public string subFileType
        {
            get
            {
                switch (FileType)
                {
                    case SnapShotFileType.Directory:
                        return "D";
                    case SnapShotFileType.FullFile:
                        return "F";
                    case SnapShotFileType.PartialFile:
                        return "R";
                    default:
                        throw new Exception("Unknown File Type " + FileType.ToString());
                }
            }
        }

    }
    [DataContract]
    public enum SnapShotFileType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        FullFile = 1,
        [EnumMember]
        PartialFile = 2,
        [EnumMember]
        Directory = 3,
    }
    [DataContract]
    public struct VssRangeInfo
    {
        [DataMember]
        public long offset { get; set; }
        [DataMember]
        public long length { get; set; }
    }
}
