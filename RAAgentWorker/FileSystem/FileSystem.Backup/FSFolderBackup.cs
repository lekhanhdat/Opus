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
using System.Xml;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{
    public class FSFolderBackupIMP : IDisposable
    {
        private XDirectoryInfo mCurrentDir;
        private IXSystem mCurrentDevice;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public FSFolderBackupIMP(XDirectoryInfo dirInfo, IXSystem device)
        {
            mCurrentDevice = device;
            mCurrentDir = dirInfo;
        }
        public void ExportFSFolderPermission(IAveBackupStream stream)
        {
            try
            {
                FSPermissionInfo filePermissionInfo = new FSPermissionInfo();
                filePermissionInfo.IsInherit = mCurrentDir.AccessControl.AreAccessRulesProtected;
                filePermissionInfo.PermissionInfo = mCurrentDir.AccessControl.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                stream.WriteMetadata(AveMetadataType.Security, filePermissionInfo);
            }
            catch (Exception ex)
            {
                log.Debug("Get folder permission metadata error {0}", ex.ToString());
            }
        }
        public void ExportBaseFolderInfo(IAveBackupStream stream)
        {
            FSFolderInfo result = new FSFolderInfo();
            result.highname = mCurrentDir.HighName;
            result.lowname = mCurrentDir.LowName;
            result.name = mCurrentDir.Name;
            stream.WriteMetadata(AveMetadataType.DocProperty, result);// 延用之前的Metadata Type 
        }
        public void ExportFullTextIndex(IAveBackupStream stream, Dictionary<string, object> fullText)
        {
            FullTextIndex fulltextIndex = new FullTextIndex();
            fulltextIndex.CreatedByLoginName = this.mCurrentDir.Owner;
            fulltextIndex.CreatedByDisplayName = this.mCurrentDir.Owner;
            fulltextIndex.Created = this.mCurrentDir.CreationTime;
            fulltextIndex.Modified = this.mCurrentDir.LastWriteTime;
            fulltextIndex.ModifiedByLoginName = this.mCurrentDir.ModifiedBy;
            fulltextIndex.Title = this.mCurrentDir.Name;
            fullText.Add("CreatedByLoginName", this.mCurrentDir.Owner);
            fullText.Add("Created", this.mCurrentDir.CreationTime);
            fullText.Add("Modified", this.mCurrentDir.LastWriteTime);
            fullText.Add("ModifiedByLoginName", this.mCurrentDir.ModifiedBy);
            fullText.Add("Title", this.mCurrentDir.Name);
            fulltextIndex.SetCustomColumnValues(fullText);
            stream.WriteMetadata(AveMetadataType.FullTextIndex, fulltextIndex);
        }
        public string GetTailInfo()
        {
            StringBuilder tail = new StringBuilder();
            string Delimiter = ((Char)0x12).ToString();
            XmlElement xe = new XmlDocument().CreateElement("Attribute");
            xe.InnerText = "Title" + Delimiter + this.mCurrentDir.Name;
            tail.Append(xe.OuterXml);
            xe.InnerText = "Created" + Delimiter + this.mCurrentDir.CreationTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            tail.Append(xe.OuterXml);
            xe.InnerText = "Modified" + Delimiter + this.mCurrentDir.LastWriteTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            tail.Append(xe.OuterXml);
            xe.InnerText = "Owner" + Delimiter + this.mCurrentDir.Owner;
            tail.Append(xe.OuterXml);
            return tail.ToString();
        }
        public void Dispose()
        {
        }
    }

    public class FSFolderInfo
    {
        public string name { get; set; }
        public string path { get; set; }
        public string id { get; set; }
        public string highname { get; set; }
        public string lowname { get; set; }
        //other property
    }
}
