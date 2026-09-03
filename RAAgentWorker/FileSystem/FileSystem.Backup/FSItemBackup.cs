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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{

    public class FSItemBackup : IDisposable
    {
        private XFileInfo mCurrentFile;
        private StorageInfo mCurrentStorageInfo;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem mCurrentDevice;
        public FSItemBackup(XFileInfo fileInfo, StorageInfo storageInfo, IXSystem device)
        {
            this.mCurrentFile = fileInfo;
            this.mCurrentStorageInfo = storageInfo;
            this.mCurrentDevice = device;
        }
        public void ExportFSFilePermission(IAveBackupStream stream)
        {
            try
            {
                FSPermissionInfo filePermissionInfo = new FSPermissionInfo();
                filePermissionInfo.IsInherit = mCurrentFile.AccessControl.AreAccessRulesProtected;
                filePermissionInfo.PermissionInfo = mCurrentFile.AccessControl.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                stream.WriteMetadata(AveMetadataType.Security, filePermissionInfo);
            }
            catch (Exception ex)
            {
                log.Debug("Get file permission metadata error {0}", ex.ToString());
            }
        }
        public void ExportBaseFileInfo(IAveBackupStream stream)
        {
            FSFileInfo result = new FSFileInfo();
            result.id = mCurrentFile.ObjectId;
            result.path = mCurrentFile.path;
            result.name = mCurrentFile.Name;
            stream.WriteMetadata(AveMetadataType.DocProperty, result);// 延用之前的Metadata Type 
        }
        public void ExportFullTextIndex(IAveBackupStream stream, Dictionary<string, object> fullText)
        {
            FullTextIndex fulltextIndex = new FullTextIndex();
            fulltextIndex.Accessed = this.mCurrentFile.LastAccessTimeUtc;
            fulltextIndex.CreatedByLoginName = this.mCurrentFile.Owner;
            fulltextIndex.CreatedByDisplayName = this.mCurrentFile.Owner;
            fulltextIndex.Created = this.mCurrentFile.CreationTimeUtc;
            fulltextIndex.Modified = this.mCurrentFile.LastWriteTimeUtc;
            fulltextIndex.ModifiedByLoginName = this.mCurrentFile.ModifiedBy;
            fulltextIndex.Size = (int)this.mCurrentFile.FileSize;// to do next long to int////
            fulltextIndex.Title = this.mCurrentFile.Name;
            fulltextIndex.SetCustomColumnValues(fullText);//
            stream.WriteMetadata(AveMetadataType.FullTextIndex, fulltextIndex);
        }
        public void ExportContent(IAveBackupStream stream)
        {
            XStream xStream = null;
            try
            {
                xStream = mCurrentDevice.OpenStream(this.mCurrentStorageInfo, System.IO.FileMode.Open);
                long size = xStream.Length;
                stream.FlushMetadata(size);
                var buffer = stream.DataBuffer;

                int count;
                while ((count = xStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.WriteContent(buffer, 0, count);
                }
            }
            catch (Exception e)
            {
                log.Warn("FS item backup \"ExportContent\" failed,Error:{0}", e);
                throw;
            }
            finally
            {
                if (xStream != null)
                {
                    xStream.Dispose();
                }
            }
            //get content export...
        }
        public string GetTailInfo(Dictionary<string, string> cgTags)
        {
            //Name、TimeZoneID、Archived、Archived By、Created、Modified、Accessed、CG tag
            StringBuilder tail = new StringBuilder();
            string Delimiter = ((Char)0x12).ToString();
            XmlElement xe = new XmlDocument().CreateElement("Attribute");
            xe.InnerText = ConvertProperyString("Title" + Delimiter + this.mCurrentFile.Name);
            tail.Append(xe.OuterXml);
            xe.InnerText = ConvertProperyString("Created" + Delimiter + this.mCurrentFile.CreationTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
            tail.Append(xe.OuterXml);
            xe.InnerText = ConvertProperyString("Modified" + Delimiter + this.mCurrentFile.LastWriteTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
            tail.Append(xe.OuterXml);
            //xe.InnerText = ConvertProperyString("Archived By" + Delimiter + Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveBy").Value.ToString());
            //tail.Append(xe.OuterXml);
            //xe.InnerText = ConvertProperyString("Archived" + Delimiter + ((DateTime)Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveTime").Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
            //tail.Append(xe.OuterXml);
            xe.InnerText = ConvertProperyString("Owner" + Delimiter + this.mCurrentFile.Owner);
            tail.Append(xe.OuterXml);
            foreach (string  key in cgTags.Keys)
            {
                xe.InnerText = ConvertProperyString(key + Delimiter + cgTags[key]);
                tail.Append(xe.OuterXml);
            }
            return tail.ToString();
        }
        private string ConvertProperyString(string propertyString)
        {
            if (!string.IsNullOrEmpty(propertyString))
            {
                Regex reg = new Regex(@"<\s*(\w+)\s*[^>]*>([^<>]*)</\1>", RegexOptions.IgnoreCase);
                MatchEvaluator evaluator = new MatchEvaluator(GetGroup);
                while (reg.IsMatch(propertyString))
                {
                    propertyString = reg.Replace(propertyString, evaluator);
                }
                propertyString = propertyString.Replace("\r\n", "");
                propertyString = propertyString.Replace("<br />", "");
                propertyString = propertyString.Replace("<", "&lt;").Replace(">", "&gt;");
                if (propertyString.StartsWith(";#", StringComparison.OrdinalIgnoreCase) && propertyString.EndsWith(";#", StringComparison.OrdinalIgnoreCase))
                {
                    propertyString = propertyString.Substring(2, propertyString.Length - 4).Replace(";#", ";");
                }
            }
            return propertyString;
        }
        private static string GetGroup(Match m)
        {
            if (m.Groups[1].Value.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                return m.Groups[2].Value + " ";
            }
            return m.Groups[2].Value;
        }
        public void Dispose()
        {
        }
    }

    public class FSFileInfo // to do next ensure backup file info......???
    {
        public string name { get; set; }
        public string path { get; set; }
        public string id { get; set; }
        public string highname { get; set; }
        public string lowname { get; set; }
        //other property
    }
}
