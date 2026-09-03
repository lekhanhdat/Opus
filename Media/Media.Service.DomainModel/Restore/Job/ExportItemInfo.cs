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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.Media.Object;

    #endregion using directives

    public class ExportItemInfo
    {
        public String Version { get; set; }

        public String FullPath { get; set; }

        public String PathMD5 { get; set; }

        public Int64 CreateTime { get; set; }

        public String CreateBy { get; set; }

        public String ModifiedBy { get; set; }

        public Int64 LastModifiedTime { get; set; }

        public Int64 ArchiverTime { get; set; }

        public AveSharePointType SpType { get; set; }

        public String SubJobId { get; set; }

        public ExportItemInfo()
        { }

        public ExportItemInfo(SearchRequestResult searchResult)
        {
            this.Version = searchResult.Version;
            this.FullPath = searchResult.FullPath;
            this.PathMD5 = searchResult.PathMD5;
            this.CreateTime = searchResult.CreateTime;
            this.CreateBy = searchResult.CreateBy;
            this.ModifiedBy = searchResult.ModifiedBy;
            this.LastModifiedTime = searchResult.LastModifiedTime;
            this.ArchiverTime = searchResult.ArchiverTime;
            this.SpType = searchResult.SpType;
            this.SubJobId = searchResult.SubJobId;
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ExportItemInfo: ")
            .Append("FullPath:" + FullPath)
            .Append(" ")
            .Append("PathMD5:" + PathMD5)
            .Append(" ")
            .Append("CreateTime:" + CreateTime)
            .Append(" ")
            .Append("ArchiverTime:" + ArchiverTime)
            .Append(" ")
            .Append("SpType:" + SpType)
            .Append(" ")
            .Append("Version:" + Version);
            return sb.ToString();
        }
    }
}