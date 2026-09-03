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

    #endregion using directives

    public class IndexDatabaseDownLoadResult
    {
        public IndexDatabaseStatus Status { get; set; }

        public String IndexFullPath { get; set; }

        public IndexDatabaseDownLoadResult(IndexDatabaseStatus indexDownLoadResult, string indexFullPath)
        {
            Status = indexDownLoadResult;
            IndexFullPath = indexFullPath;
        }

        public override string ToString()
        {
            return string.Format("IndexDatabaseDownLoadResult Status : {0}, IndexFullPath : {1}.", Status, IndexFullPath);
        }
    }
    public enum IndexDatabaseStatus
    {
        [OpenMode(OpenMode = IndexDatabaseOpenMode.Create)]
        Nonexistent,
        [OpenMode(OpenMode = IndexDatabaseOpenMode.Open)]
        Cached,
        [OpenMode(OpenMode = IndexDatabaseOpenMode.Open)]
        Downloaded,
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OpenModeAttribute : Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        // This is a named argument
        public IndexDatabaseOpenMode OpenMode { get; set; }
    }
    public enum IndexDatabaseOpenMode
    {
        Open,
        Create,
    }
    public class IndexDatabaseUpLoadResult
    {
        public String StorageInfo { get; set; }
        public Boolean IsCommit { get; set; }
        public override string ToString()
        {
            return string.Format("IndexDatabaseUpLoadResult StorageInfo : {0}.", StorageInfo);
        }
    }
    public enum IndexDatabaseProperties
    {
        None,
        LastModifyTime,
        LastAccessTime,
    }
}