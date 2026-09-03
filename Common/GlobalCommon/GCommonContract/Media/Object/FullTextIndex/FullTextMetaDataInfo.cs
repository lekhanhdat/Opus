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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextMetaDataInfo
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String SharePointVersion { get; set; }

        [DataMember]
        public String Attributes { get; set; }

        [DataMember]
        public Int64 ArchiveTime { get; set; }

        [DataMember]
        public String ArchiveBy { get; set; }

        [DataMember]
        public String ArchiveTimeText { get; set; }

        [DataMember]
        public Int64 ArchiveTimeForRestore { get; set; }

        [DataMember]
        public String PathMD5 { get; set; }

        [DataMember]
        public String ParentPathMD5 { get; set; }

        [DataMember]
        public String Permission { get; set; }

        [DataMember]
        public FullTextParentFolderInfo ParentFolder { get; set; }

        [DataMember]
        public Boolean HasContent { get; set; }

        [DataMember]
        public String Author { get; set; }

        [DataMember]
        public Int64 CreateTime { get; set; }

        [DataMember]
        public Int64 ModifyTime { get; set; }

        [DataMember]
        public String CreateTimeText { get; set; }

        [DataMember]
        public String ModifyTimeText { get; set; }

        [DataMember]
        public Boolean IsHit { get; set; }

        [DataMember]
        public String Summary { get; set; }

        [DataMember]
        public String ShowSummary { get; set; }

        [DataMember]
        public String AttachmentName { get; set; }

        [DataMember]
        public Int64 Size { get; set; }

        [DataMember]
        public Int64 FileSize { get; set; }

        [DataMember]
        public String Version { get; set; }

        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String StubInfo { get; set; }

        [DataMember]
        public Int64 DataSize { get; set; }

        [DataMember]
        public String Retention { get; set; }

        [DataMember]
        public String SubRetention { get; set; }

        [DataMember]
        public String FullPath { get; set; }

        [DataMember]
        public String Location { get; set; }

        [DataMember]
        public AveSharePointType SharepointType { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public Int64 TotalCount { get; set; }

        [DataMember]
        public Int64 StartTime { get; set; }

        [DataMember]
        public Int64 EndTime { get; set; }

        [DataMember]
        public String StartTimeText { get; set; }

        [DataMember]
        public String EndTimeText { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String CycleId { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public Boolean IsFailed { get; set; }

        [DataMember]
        public FullTextTailInfo FullTextTailInfo { get; set; }

        public override String ToString()
        {
            return String.Format("Full Text Meta Data Info: Name: {0}, Full Path: {1}, Job Id: {2}",
                this.Name,
                this.FullPath,
                this.JobId);
        }
    }
}