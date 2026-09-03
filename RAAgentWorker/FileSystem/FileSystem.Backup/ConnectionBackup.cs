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
using AvePoint.Media.Storage;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using RAFileSystem.FileSystem.Backup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{
    public class ConnectionBackup : IDisposable
    {
        private IXSystem mCurrentDevice;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public ConnectionBackup(IXSystem currentDevice)
        {
            mCurrentDevice = currentDevice;
        }
        public void ExportFSConnectionPermission(IAveBackupStream stream)
        {
            try
            {
                XDirectoryInfo mCurrentDir = this.mCurrentDevice.OpenDirectory(new StorageInfo(string.Empty, string.Empty), System.IO.FileMode.Open);
                FSPermissionInfo filePermissionInfo = new FSPermissionInfo();
                filePermissionInfo.IsInherit = mCurrentDir.AccessControl.AreAccessRulesProtected;
                filePermissionInfo.PermissionInfo = mCurrentDir.AccessControl.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                stream.WriteMetadata(AveMetadataType.Security, filePermissionInfo);
            }
            catch (Exception ex)
            {
                log.Debug("Get connection permission metadata error {0}", ex.ToString());
            }
        }
        public void ExportBaseConnectionInfo(IAveBackupStream stream)
        {
            FSConnectionInfo result = new FSConnectionInfo();
            result.id = mCurrentDevice.SystemID;
            result.LocationName = mCurrentDevice.SystemLocation;
            result.name = mCurrentDevice.SystemName;
            stream.WriteMetadata(AveMetadataType.SiteBasicInfo, result);// 延用之前的Metadata Type 
        }
        public void ExportFullTextIndex(IAveBackupStream stream)
        {
            //generate fulltext ......
            FullTextIndex fulltextIndex = new FullTextIndex();
            Dictionary<string, object> fullText = new Dictionary<string, object>();
            fulltextIndex.SetCustomColumnValues(fullText);
            stream.WriteMetadata(AveMetadataType.FullTextIndex, fulltextIndex);
        }


        public void Dispose()
        {
        }
    }

    public class FSConnectionInfo
    {
        public string name { get; set; }
        public string LocationName { get; set; }
        public string id { get; set; }
        //other property
    }
}
