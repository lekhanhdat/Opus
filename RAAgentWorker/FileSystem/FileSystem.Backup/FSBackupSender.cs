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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.Media.Storage.Util;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RAFileSystem.FileSystem.FileSystem.Backup
{
    public class FSBackupSender
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private string farmId = string.Empty;
        private Hashtable fileHeaderAttribute = new Hashtable();
        public string jobId;
        public Hashtable FileHeaderAttribute
        {
            get
            {
                return fileHeaderAttribute;
            }
            set
            {
                fileHeaderAttribute = value;
            }
        }
        /// <summary>
        /// Item Level use this property
        /// </summary>
        public IAveBackupStream BackupStream { get; set; }

        /// <summary>
        /// Site ，Subsite use this property
        /// </summary>
        public IFSBackupDataWriter FileSender { get; set; }

        /// <summary>
        /// send header message of a backup item
        /// </summary>
        private XmlElement fileHeaderXml;

        private XmlElement mSecondFileHeaderXml;

        private BackupPermissionForFileSystem backupFSPermission;

        private bool isFSAJob = false;
        public List<PermissionLevel> permissionLevels { get; set; }
        private string secondHeaderFolderPath = string.Empty;
        private string secondHeaderFilePath = string.Empty;
        private const string TEMPFOLDERNAME = "FSArchiver";
        private string secondHeaderGuid = string.Empty;
        private StreamWriter streamWriter = null;
        public string ConnectionId;
        public string ConnectionName;
        private string FarmId
        {
            get
            {
                return AveEnv.AgentFarmId;
            }
        }

        public void AddBackupFileHeaderAttribute(string key, string value)
        {
            if (fileHeaderAttribute.ContainsKey(key))
            {
                fileHeaderAttribute[key] = value;
            }
            else
            {
                fileHeaderAttribute.Add(key, value);
            }
        }

        public FSBackupSender(IFSBackupDataWriter filesender)
        {
            FileSender = filesender;
            BackupStream = new WrapperBackupStreamV1(new FSArchiveFileSender(FileSender));
            XmlDocument doc = new XmlDocument();
            fileHeaderXml = doc.CreateElement("FileHeader");
            fileHeaderXml.SetAttribute("farmGUID", FarmId);
            secondHeaderGuid = Guid.NewGuid().ToString();
            jobId = filesender.GetJobId();
            ConnectionName = filesender.GetConnectionName();
            ConnectionId = filesender.GetConnectionId();
            secondHeaderFolderPath = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, BackgroundSettings.GetInstance().ArchiveCache, secondHeaderGuid, TEMPFOLDERNAME, TenantAgentInfo.JobId);
            secondHeaderFilePath = SecurityUtils.SafeCombinePath(secondHeaderFolderPath, TenantAgentInfo.JobId + ".tmpheader");
            InitStreamWriter();
        }
        private void InitStreamWriter()
        {
            if (!Directory.Exists(secondHeaderFolderPath))
            {
                mLog.Info("Begin Create second header temp folder for Deletion");
                Directory.CreateDirectory(secondHeaderFolderPath);
            }
            streamWriter = new StreamWriter(secondHeaderFilePath);
        }
        public void CacheSecondHeader(string tempHeader)
        {
            if (string.IsNullOrEmpty(tempHeader))
            {
                mLog.Info("Current second Header IsNullOrEmpty.");
                return;
            }

            try
            {
                streamWriter.WriteLine(tempHeader);
                if (tempHeader.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Current second Header write failed,header:{tempHeader.LogBase64()},it may caused the file delete failed,error:{e}.");
            }
        }
        public void SendSecondHeaders(Action<FSAzureTableEntityDto> removeFile)
        {
            if (System.IO.File.Exists(secondHeaderFilePath))
            {
                mLog.Info($"Second header file exist.path:{secondHeaderFilePath.LogBase64()}");
                using (StreamReader streamReader = new StreamReader(secondHeaderFilePath))
                {
                    while (streamReader.Peek() > 0)
                    {
                        string tempHeader = streamReader.ReadLine();
                        if (tempHeader.Equals("End", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        var dto = SerializerHelper.DeserializeByDataContractSerializer<FSAzureTableEntityDto>(tempHeader);
                        removeFile(dto);
                    }
                }
                System.IO.File.Delete(secondHeaderFilePath);
            }
            else
            {
                mLog.Info("Second header file not exist.");
            }
        }
        #region FileSystem Header method
        public void BackupConnectionHeader(IXSystem physical, string path, FSAzureTableEntityDto entity, long size, string ruleName, string subJobId, string mediaName)
        {
            AddBackupFileHeaderAttribute(KeyWord.PATH, path);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_SITE.ToString());
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            //AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.LibRowId, "0");
            //AddBackupFileHeaderAttribute(KeyWord.PathMD5, entity.PathMD5.ToString());
        }

        public void BackupFSFolderHeader(XDirectoryInfo dir, FSAzureTableEntityDto entity, string UNCPath, string ruleName, string subJobId, string mediaName, long size)
        {
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, Path.Combine(UNCPath, dir.HighName));
            AddBackupFileHeaderAttribute(KeyWord.PATH, dir.HighName);
            AddBackupFileHeaderAttribute(KeyWord.HIGH_NAME, dir.HighName);
            AddBackupFileHeaderAttribute(KeyWord.LOW_NAME, dir.LowName);
            AddBackupFileHeaderAttribute(KeyWord.LEAF_NAME, dir.Name);
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_FOLDER.ToString());
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            //AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.LibRowId, "0");
            //AddBackupFileHeaderAttribute(KeyWord.PathMD5, entity.PathMD5.ToString());
            ////scope id to do
            //try
            //{
            //    FSEndUserPermisson permission = backupFSPermission.GetFolderPermission(dir, entity.PermissionScopeId, false);
            //    SetFSPermission(permission);
            //    entity.PermissionScopeId = permission.scopeId;
            //}
            //catch (Exception ex)
            //{
            //    mLog.Debug("Get Folder Permission Header Error {0}", ex.ToString());
            //}
        }
        public void BackupFSDocumentHeader(XFileInfo fileInfo, FSAzureTableEntityDto entity, string UNCPath, long size, string ruleName)
        {
            AddBackupFileHeaderAttribute(KeyWord.FULLPATH, Path.Combine(UNCPath, fileInfo.HighName??"", fileInfo.LowName));
            AddBackupFileHeaderAttribute(KeyWord.PATH, fileInfo.Name);
            AddBackupFileHeaderAttribute(KeyWord.HIGH_NAME, fileInfo.HighName ?? "");
            AddBackupFileHeaderAttribute(KeyWord.LOW_NAME, fileInfo.LowName);
            AddBackupFileHeaderAttribute(KeyWord.LEAF_NAME, fileInfo.Name);
            AddBackupFileHeaderAttribute(KeyWord.TYPE, AveConstants.TYPE_DOCUMENT.ToString());
            AddBackupFileHeaderAttribute(KeyWord.RULENAME, ruleName);
            //AddBackupFileHeaderAttribute(KeyWord.MEDIANAME, mediaName);
            //AddBackupFileHeaderAttribute(KeyWord.SUBJOBID, subJobId);
            AddBackupFileHeaderAttribute(KeyWord.MYLEVEL, ((int)CacheNodeType.Item).ToString());
            AddBackupFileHeaderAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            AddBackupFileHeaderAttribute(KeyWord.BACKUPTYPE, "0");
            AddBackupFileHeaderAttribute(KeyWord.LibRowId, "0");
            AddBackupFileHeaderAttribute(KeyWord.SIZE, size.ToString());
            AddBackupFileHeaderAttribute(KeyWord.Extra, fileInfo.HighName);
            AddBackupFileHeaderAttribute(KeyWord.CreatedTime, entity.CreateTime.Ticks.ToString());
            AddBackupFileHeaderAttribute(KeyWord.ModifiedTime, entity.LastModifiedTme.Ticks.ToString());
            //AddBackupFileHeaderAttribute(KeyWord.PathMD5, entity.PathMD5.ToString());
            //try
            //{
            //    FSEndUserPermisson permission = backupFSPermission.GetFilePermission(fileInfo, entity.PermissionScopeId);
            //    SetFSPermission(permission);
            //    entity.PermissionScopeId = permission.scopeId;
            //}
            //catch (Exception ex)
            //{
            //    mLog.Debug("Get File Permission Header Error {0}", ex.ToString());
            //}
        }
        #endregion

        public XmlElement BackupHeader()
        {
            return WriteFileHeader(fileHeaderAttribute);
        }

        public string GetProperties(string apUrl)
        {
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("HeaderExtraAttribute");
            headerExtraAttribute.SetAttribute("APUrl", apUrl);
            return headerExtraAttribute.OuterXml;
        }


        private void SetFSPermission(FSEndUserPermisson permission)
        {
            try
            {
                AddBackupFileHeaderAttribute(KeyWord.scopeId, permission.scopeId.ToString());
                AddBackupFileHeaderAttribute(KeyWord.isInheritPermission, permission.isInheritPermission.ToString());
                AddBackupFileHeaderAttribute(KeyWord.permissions, permission.GetUserString());
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while setting file system end user permission:{0}", e.ToString());
            }
        }
        private XmlElement WriteFileHeader(Hashtable attributes, string innerXml)
        {
            if (!string.IsNullOrEmpty(innerXml))
            {
                this.fileHeaderXml.InnerXml = innerXml;
            }
            foreach (object key in attributes.Keys)
            {
                fileHeaderXml.SetAttribute(key.ToString(), attributes[key] == null ? string.Empty : attributes[key].ToString());
            }

            BackupStream.WriteHead(fileHeaderXml.OuterXml);

            XmlElement ret = (XmlElement)(fileHeaderXml.CloneNode(true));
            if (this.fileHeaderXml.HasAttribute("webApp"))
            {
                this.fileHeaderXml.RemoveAttribute("webApp");
            }
            if (this.fileHeaderXml.HasAttribute("isMyProfileList"))
            {
                this.fileHeaderXml.RemoveAttribute("isMyProfileList");
            }
            return ret;
        }

        private XmlElement WriteFileHeader(Hashtable attributes)
        {
            return WriteFileHeader(attributes, "");
        }
        public long BackupTail(string tail, bool successful)
        {
            FileSender.HandleTail(GenerateTailWithState(tail, successful));
            return 0;
        }
        private string GenerateTailWithState(string tail, bool successful)
        {
            int index = tail.IndexOf("<BackupDataExtraInfo", StringComparison.OrdinalIgnoreCase);
            string attributes = string.Empty;
            string extraInfo = string.Empty;
            if (index > 0)
            {
                attributes = tail.Substring(0, index);
                extraInfo = tail.Substring(index);
            }
            else
            {
                attributes = tail;
            }
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("FileTail");
            e.SetAttribute("extraInfo", extraInfo);
            e.InnerXml = attributes;
            if (!successful)
            {
                e.SetAttribute("failed", "true");
            }
            return e.OuterXml;
        }


        ///// <summary>
        ///// web,doc,item,attachment FileTail
        ///// </summary>
        ///// <param name="tail"></param>
        ///// <param name="successful"></param>
        ///// <returns></returns>
        //public long BackupTail(string tail, bool successful)
        //{
        //    return FileSender.WriteTail(tail, successful);
        //}
    }
    enum FileHeaderType
    {
        First = 1,
        Second = 2
    }

    enum FileHeaderStatus
    {
        Failed = 1,
        Complete = 2
    }
    internal class BackupPermissionForFileSystem
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public string HostName = string.Empty;
        public string UNCPath = string.Empty;
        public void GetADUserinLocalGroups(string groupName, List<string> usernames)
        {
            try
            {
                using (DirectoryEntry groupEntry = new DirectoryEntry(string.Format("WinNT://{0}/{1}", this.HostName, groupName)))
                {
                    if (!string.IsNullOrEmpty(groupEntry.SchemaClassName) && groupEntry.SchemaClassName.Equals("group", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (object member in (IEnumerable)groupEntry.Invoke("Members"))
                        {
                            using (DirectoryEntry memberEntry = new DirectoryEntry(member))
                            {
                                SecurityIdentifier sid = new SecurityIdentifier((Byte[])memberEntry.Properties["objectSid"].Value, 0);
                                mLog.Debug("local group user name {0} :{1}", memberEntry.Path.LogBase64(), sid.Value.LogBase64());
                                usernames.Add(sid.Value);
                                #region
                                //if (!string.IsNullOrEmpty(memberEntry.SchemaClassName))
                                //{
                                //    var userFullName = memberEntry.Path.Substring(8);//8 length of WinNT://
                                //    if (!usernames.Contains(userFullName))
                                //    {
                                //        usernames.Add(userFullName.Replace('/', '\\'));
                                //    }
                                //}
                                #endregion
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(groupEntry.SchemaClassName) && groupEntry.SchemaClassName.Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                        SecurityIdentifier sid = new SecurityIdentifier((Byte[])groupEntry.Properties["objectSid"].Value, 0);
                        mLog.Debug("local user name {0} : {1}", groupEntry.Path.LogBase64(), sid.Value.LogBase64());
                        usernames.Add(sid.Value);
                    }
                    else
                    {
                        mLog.Info("Invalid group name {0}", groupName.LogBase64());
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Local User {0} : {1}", groupName, ex.ToString());
                try
                {
                    using (DirectoryEntry userEntry = new DirectoryEntry(string.Format("WinNT://{0}/{1},user", this.HostName, groupName)))
                    {
                        SecurityIdentifier sid = new SecurityIdentifier((Byte[])userEntry.Properties["objectSid"].Value, 0);
                        mLog.Debug("local user name {0} : {1}", userEntry.Path.LogBase64(), sid.Value.LogBase64());
                        usernames.Add(sid.Value);
                    }
                }
                catch (Exception ex1)
                {
                    mLog.Info("Invalid local user name {0}:{1}", groupName.LogBase64(), ex1.ToString());
                }
            }
        }
        //public string GetHostNameWithHandleException(string host)
        //{
        //    try
        //    {
        //        return Dns.GetHostEntry(host).HostName;
        //    }
        //    catch (Exception e)
        //    {
        //        mLog.Warn("Get Host Entry Error, Host : {0}, Error Message : {1}", host, e.Message);
        //        try
        //        {
        //            IPHostEntry dnshost = Dns.Resolve(host);
        //            return dnshost.HostName;
        //        }
        //        catch (Exception ex)
        //        {
        //            mLog.Warn("Use Dns.Resolve to Get Host Failed, Error Message : {0}", ex.Message);
        //        }
        //        return string.Empty;
        //    }
        //}



        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)")]
        public string GetLocalGroupName(string IdentityValue)
        {
            string localGroupName = string.Empty;
            string[] names = IdentityValue.Split('\\');
            if (IdentityValue.StartsWith("BUILTIN\\"))
            {
                localGroupName = names[1];

            }
            else if (names.Length > 0)
            {
                if (!names[0].Equals(Environment.UserDomainName) && this.HostName.Contains(names[0]))
                {
                    localGroupName = names[1];
                }
            }

            return localGroupName;
        }
        //public string ToHashCode(string value, string hashAlgorithmName)
        //{
        //    using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create(hashAlgorithmName))
        //    {
        //        hashAlgorithm.Initialize();
        //        var hashByteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
        //        return BitConverter.ToString(hashByteArray).Replace("-", "").ToLowerInvariant();
        //    }
        //}

    }
    internal class FSEndUserPermisson
    {
        public string scopeId { get; set; } //path of md5

        public bool isInheritPermission { get; set; }

        public List<string> users = new List<string>();

        public void AddUser(string user)
        {
            if (!users.Contains(user))
                users.Add(user);
        }

        public string GetUserString()
        {
            if (users.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(";");
            foreach (string user in users)
            {
                sb.Append(user);
                sb.Append(";");
            }
            return sb.ToString();
        }
    }
    public class FileAtrributeInfo
    {
        /// <summary>
        /// property collection of a file，listitem or attachment
        /// </summary>
        private readonly List<string> mAttributeColl = new List<string>();
        private readonly Dictionary<string, string> mFullTextindex = new Dictionary<string, string>();

        public bool IsSystemFile { set; get; }

        public void AddProperty(string prop)
        {
            mAttributeColl.Add(prop);
        }

        public bool ContainFullTextAttribute(string key)
        {
            if (mFullTextindex.ContainsKey(key))
                return true;
            else
                return false;
        }

        public void AddFullTextProperty(string key, string prop)
        {
            if (!mFullTextindex.ContainsKey(key))
            {
                mFullTextindex.Add(key, prop);
            }
        }
        public string ExtraId { get; set; }

        /// <summary>
        /// DisplayName of the item
        /// </summary>
        public string ExtraTitle { set; get; }

        /// <summary>
        /// Add For NewsFeed Post and Reply
        /// </summary>
        public string PostId { get; set; }


        /// <summary>
        /// Add For NewsFeed Post and Reply
        /// </summary>
        public long NewsFeedCreatedTime { get; set; }

        /// <summary>
        /// extra info send to media
        /// </summary>
        public override string ToString()
        {
            var strbuilder = new StringBuilder();
            var xmldoc = new XmlDocument();
            if (IsSystemFile)
            {
                XmlElement systemFileElement = xmldoc.CreateElement("IsSystemFile");
                systemFileElement.InnerText = "true";
                strbuilder.Append(systemFileElement.OuterXml);
            }
            XmlElement titleInfo = xmldoc.CreateElement("Title");
            titleInfo.InnerText = ExtraTitle;
            strbuilder.Append(titleInfo.OuterXml);

            string titleAttr = "Title" + ((Char)0x12).ToString();
            XmlElement itemElement = xmldoc.CreateElement("Attribute");
            itemElement.InnerText = titleAttr + ExtraTitle;
            strbuilder.Append(itemElement.OuterXml);
            foreach (string tmp in this.mAttributeColl)
            {
                if (tmp.StartsWith(titleAttr, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                itemElement.InnerText = tmp;
                strbuilder.Append(itemElement.OuterXml);
                //strbuilder.Append("<Attribute>").Append(tmp).Append("</Attribute>");
            }

            XmlElement backupDataExtraInfo = xmldoc.CreateElement("BackupDataExtraInfo");
            backupDataExtraInfo.SetAttribute("version", "5.2");
            xmldoc.AppendChild(backupDataExtraInfo);

            XmlElement idElement = xmldoc.CreateElement("KeyAndValue");
            idElement.SetAttribute("key", "ID");
            idElement.SetAttribute("value", this.ExtraId);

            XmlElement titleElement = xmldoc.CreateElement("KeyAndValue");
            titleElement.SetAttribute("key", "Title");
            titleElement.SetAttribute("value", ExtraTitle);

            //Media要求如果PostID没有值，就不发送这个属性
            if (!string.IsNullOrEmpty(PostId))
            {
                XmlElement postIdElement = xmldoc.CreateElement("PostId");
                postIdElement.InnerText = PostId;
                strbuilder.Append(postIdElement.OuterXml);
            }

            XmlElement newsFeedCreatedTimeElement = xmldoc.CreateElement("CreateTime");
            newsFeedCreatedTimeElement.InnerText = NewsFeedCreatedTime.ToString();
            strbuilder.Append(newsFeedCreatedTimeElement.OuterXml);

            backupDataExtraInfo.AppendChild(idElement);
            backupDataExtraInfo.AppendChild(titleElement);

            strbuilder.Append(xmldoc.OuterXml);

            return strbuilder.ToString();
        }
    }

    internal class KeyWord
    {
        internal static string TYPE = "type";
        internal static string HIGH_NAME = "highname";
        internal static string LOW_NAME = "lowname";
        internal static string LEAF_NAME = "leafname";
        internal static string PATH = "path";
        internal static string HEADERTYPE = "fileHeaderType";
        internal static string TIME = "archivedTime";
        internal static string ID = "spId";
        internal static string RowId = "rowId";
        internal static string LEVEL = "level";
        internal static string VERSION = "UIVersion";
        internal static string WEBAPP = "webApp";
        internal static string PROFILE = "isMyProfileList";
        internal static string NODEGUID = "nodeGuid";
        internal static string SYSTEMFILE = "isSystemFile";
        internal static string BACKUPTYPE = "backupType";
        internal static string SiteUrl = "siteUrl";
        internal static string WebId = "webId";
        internal static string ListId = "listId";
        internal static string ISVERSION = "isVersion";
        internal static string MYLEVEL = "myLevel";
        internal static string SIZE = "size";
        internal static string URL = "url";
        internal static string RULENAME = "ruleName";
        internal static string SUBJOBID = "subJobId";
        internal static string MEDIANAME = "mediaName";
        internal static string FULLPATH = "fullPath";//for Error page ,give a FullPath
        internal static string scopeId = "scopeId";
        internal static string isInheritPermission = "isInheritPermission";
        internal static string permissions = "permissions";
        internal static string LibRowId = "LibRowId";
        internal static string EndUserJobId = "endUserJobId";
        internal static string PathMD5 = "PathMD5";
        internal static string DoDelete = "DoDelete";
        internal static string DeleteRelatedRecords = "DeleteRelatedRecords";
        internal static string Extra = "extra";
        internal static string CreatedTime = "Created";
        internal static string ModifiedTime = "Modified";
    }
}
