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

    #endregion

    public class ArchiverIndexInfo
    {
        public String PathMD5 { get; set; }
        public String Path { get; set; }
        public Int64 EndTime { get; set; }
        public Int32 OffSet { get; set; }
        public Int32 Length { get; set; }

        public ArchiverIndexInfo()
        {
            this.OffSet = 0;
            this.Length = int.MaxValue - 1;
        }

        public ArchiverIndexInfo(ArchiverBrowseInfo browseInfo)
        {
            this.Path = browseInfo.Path;
            this.EndTime = browseInfo.EndTime;
            this.OffSet = browseInfo.OffSet;
            this.Length = browseInfo.Length;
        }

        //public ArchiverIndexInfo(EndUserBrowseInfo browseInfo)
        //{
        //    this.PathMD5 = browseInfo.PathMD5;
        //    this.OffSet = browseInfo.OffSet;
        //    this.Length = browseInfo.Length;
        //}

        public override String ToString()
        {
            return String.Format("Index path: {0}, End time: {1}, Offset: {2}, Length: {3}.",
                this.Path,
                this.EndTime,
                this.OffSet,
                this.Length);
        }
    }
}