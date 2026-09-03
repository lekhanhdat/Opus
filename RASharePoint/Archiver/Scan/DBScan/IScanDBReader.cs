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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.StorageOptimization.Schedule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan
{
    internal interface IScanDBReader : IDisposable
    {
        void UpdateStatus(string siteid,Guid itemid, BackupRestoreStatus Status);
        void Init(ArchiverExtendSettingDto archiverExtendSetting, string siteUrl);
        List<Guid> GetWebIds(Guid siteid, int ruleOrder);

        List<Guid> GetListIds(Guid siteid, Guid webid, int ruleOrder);

        List<DBFileInfo> GetFilesInfo(Guid siteid, Guid webid, Guid listid, int ruleOrder);
    }
    public class DBFileInfo
    {
        public string url;
        public Guid itemId;
        public string fullPath;
        public string ruleId;
        public Guid webId;
        public Guid listId;
        public Int32 ID;
        public Int64 Size;
        public Int64 StorageSize;
        public string fileName;
        public Int64 CGDBID;
        public string fileType;
    }

    internal class DBWebInfo
    {
        public Guid webId;
        public List<DBFileInfo> DBFileInfos;

    }
}
