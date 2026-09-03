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
using AvePoint.GCommon.Contract.Media.Object.Common;
using System.Reflection;
using System.Xml;

namespace RAGoogle.Archive
{
    class CacheNode : IDisposable
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name = string.Empty;
        public XmlElement FileHeader { get; set; }
        public FileHeaderStatus BackupStatus { get; set; } = FileHeaderStatus.None;
        public bool DoDelete { get; set; }
        public Action CustomizedDisposeAction;


        public void Dispose()
        {
            if (CustomizedDisposeAction != null)
            {
                CustomizedDisposeAction();
            }
        }

        public string GenerateSecondFileHeader()
        {
            if (!DoDelete || BackupStatus != FileHeaderStatus.Success)
            {
                return string.Empty;
            }
            if (FileHeader == null)
            {
                throw new ArgumentNullException("FileHeader");
            }
            FileHeader.SetAttribute(GDriveKeyWord.DoDelete, DoDelete.ToString());
            FileHeader.SetAttribute(GDriveKeyWord.BackupStatus, ((int)BackupStatus).ToString());
            return FileHeader.OuterXml;
        }
    }
    public enum GoogleCacheNodeType
    {
        Drive = 1,
        Folder = 10,
        Item = 10000,
        ItemVersion = 10001
    }
    public enum FileHeaderStatus
    {
        None = 0,
        Success = 1,
        Failed = 2,
        Exception = 3,
        Skip = 4
    }
}
