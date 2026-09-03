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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System.Xml;

namespace RAGoogle.Restore.Content
{
    public class GDriveRestoreContentReader
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(GDriveRestoreContentReader));
        private readonly IAveRestoreStream mRestoreStream;
        private readonly XmlDocument mXDoc;

        private RestoreTreeNode mCurrentNode;
        private readonly GDriveRestoreConfig mConfig;

        public GDriveRestoreContentReader(IAveRestoreStream restoreStream, GDriveRestoreConfig config, RestoreTreeNode root)
        {
            mRestoreStream = restoreStream;
            mConfig = config;
            mXDoc = new XmlDocument();
            mCurrentNode = root;
        }

        public RestoreContentDto MoveNext()
        {
            //if (AveItemRestorePauseResume.NeedStopJob)
            //{
            //    throw new PauseProcessException();
            //}
            string head;
            while ((head = mRestoreStream.ReadHead()) != null)
            {
                mXDoc.LoadXml(head);
                XmlElement rootElement = mXDoc.DocumentElement;
                if (rootElement == null)
                {
                    continue;
                }
                string path = rootElement.Attributes["path"].Value;
                string title = rootElement.Attributes["name"].Value;
                GDriveDataType type = (GDriveDataType)GetAttributeInt("type", rootElement, 0);
                bool needRmovePrefixSpace = NeedRmovePrefixSpace(type);
                int flag = GetAttributeInt("versionFlag", rootElement, 1);
                //int isMyProfile = GetIsMyProfileAttribute(rootElement);
                bool isChecked = GetAttributeBoolean("isChecked", rootElement, true);
                bool isSelected = GetAttributeBoolean("isSelected", rootElement, false);
                bool parentIsSelected = GetAttributeBoolean("parentIsSelected", rootElement, false);
                bool isFailed = GetAttributeBoolean("isFailed", rootElement, false);
                string storageId = rootElement.Attributes["StorageId"].Value;
                string Id = rootElement.Attributes["Id"].Value;
                string backupJobId = rootElement.Attributes["BackUpJobId"].Value;
                string itemPathMd5 = rootElement.Attributes["ItemPathMD5"].Value;
                string stubType = rootElement.HasAttribute("stubType") ? rootElement.GetAttribute("stubType") : string.Empty;
                string driveId = rootElement.HasAttribute("driveId") ? rootElement.GetAttribute("driveId") : string.Empty;
                string driveName = rootElement.HasAttribute("driveName") ? rootElement.GetAttribute("driveName") : string.Empty;
                long archiveTime = GetAttributeLong("ArchiveTime", rootElement, DateTime.UtcNow.Ticks);
                string parentId = rootElement.Attributes["parentId"].Value;
                string version = rootElement.Attributes["version"].Value;
                #region SourceAPUrl
                var att = rootElement.Attributes["HeaderExtraAttribute"].Value;
                //string srcUrl = string.Empty;
                //if (!string.IsNullOrEmpty(att))
                //{
                //    var xmlDoc = new XmlDocument();
                //    xmlDoc.LoadXml(att);
                //    var headExtrAtt = xmlDoc.DocumentElement;
                //    srcUrl = headExtrAtt.HasAttribute("APUrl") ? headExtrAtt.Attributes["APUrl"].Value : string.Empty;
                //}
                #endregion

                bool property = GetAttributeBoolean("property", rootElement, true);
                bool security = GetAttributeBoolean("security", rootElement, true);
                bool isAppData = GetAttributeBoolean("isAppData", rootElement, false);
                /*string appDataName = rootElement.HasAttribute("appDataName") ? rootElement.Attributes["appDataName"].Value : string.Empty;
                if (isAppData && !string.IsNullOrEmpty(appDataName))
                {
                    path = appDataName;
                }*/
                var objDto = new RestoreContentDto
                {
                    Type = type,
                    SrcName = title,
                    SrcUrl = path,
                    OwnerLogin = mConfig.DestinationInfo.OwerLogin,
                    IsChecked = isChecked,
                    IsSelected = isSelected,
                    ParentIsSelected = parentIsSelected,
                    IsFailed = isFailed,
                    IsAppData = isAppData,
                    StubType = stubType,
                    Id = Id,
                    StorageId = storageId,
                    BackUpJobId = backupJobId,
                    ItemPathMd5 = itemPathMd5,
                    ArchiveTime = archiveTime,
                    DriveId = driveId,
                    DriveName = driveName,
                    ParentId = parentId,
                    Version = version,
                };

                #region  conflict resolution
                if (type == GDriveDataType.MyDrive || type == GDriveDataType.SharedDrive || type == GDriveDataType.Folder)
                {
                    //if((!isSelected && !parentIsSelected) && (GetNodeLevel(type) <= GetCheckedNodeLevel(mConfig.ArchiverConfigForMedia.TreeRoot)))
                    //{
                    //    objDto.RestoreOption.SetRequestOption(property, security, (int)AveRestoreMode.Default);
                    //}
                    //else
                    {
                        objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.ContainerRestoreMode);
                    }
                }

                else
                {
                    var restoreMode = mConfig.ContentRestoreMode;
                    objDto.RestoreOption.SetRequestOption(property, security, (int)restoreMode);
                }
                #endregion

                if (rootElement.HasAttribute("nodeGuid"))
                {
                    string idString = rootElement.Attributes["nodeGuid"].Value;
                    Guid id;
                    if (Guid.TryParse(idString, out id))
                    {
                        objDto.UniqueId = id;
                    }
                }

                //objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.RestoreMode);
                objDto.RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER = mConfig.DisableEventReceiver;
                objDto.RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = mConfig.SkipIfSameModified;
                if (type == GDriveDataType.File ||
                    type == GDriveDataType.FileVersion)
                {
                    objDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM = (flag & 1) != 0;
                }
                else if (type == GDriveDataType.Folder)
                {
                    objDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM = isChecked;
                }
                //if (isMyProfile == 1)
                //{
                //    objDto.IsMyProfileList = true;
                //}
                if (mCurrentNode == null) //In place and not replace restore 
                {
                    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                    return objDto;
                }
                if (type == GDriveDataType.File ||
                    type == GDriveDataType.FileVersion)
                {
                    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                    return objDto;
                }
                string name = GetNameFromPath(path);
                RestoreTreeNode parentNode = GetParentNode(mCurrentNode, path, type);
                RestoreTreeNode childNode = parentNode.GetChild(name);
                objDto.ParentName = needRmovePrefixSpace ? parentNode.Path?.RemovePrefixSpace() : parentNode.Path;
                if (childNode != null)
                {
                    mCurrentNode = childNode;
                    if (childNode.IsOutPlace)
                    {
                        childNode.SrcPath = path;
                        objDto.Name = needRmovePrefixSpace ? childNode.Path?.RemovePrefixSpace() : childNode.Path;
                    }
                    else
                    {
                        objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                    }
                    if (childNode.Checked)
                    {
                        //objDto.ReplaceType = childNode.Type;
                    }
                    if (childNode.IgnoreThisNode)
                    {
                        continue;
                    }
                    return objDto;
                }
                // This node cannot be found in restore tree.
                // We should create a new node and add to restore tree.
                // In this way, we keep the structure of this node and its children.
                childNode = new RestoreTreeNode
                {
                    Name = name,
                    Type = type,
                    Path = GetPathFromName(parentNode, name, type),
                    IsOutPlace = mCurrentNode.IsOutPlace
                };
                if (parentNode.Children != null && parentNode.Children.Count != 0 &&
                    parentNode.Type != GDriveDataType.MyDrive)
                {
                    mLog.Error(@"Looks up a localized string similar to {0} should be got in the process of restoring. It will be restored to {1}..", path, childNode.Path);
                }
                childNode.SrcPath = path;
                childNode.HasDestNode = false;
                parentNode.AddChild(childNode);
                mCurrentNode = childNode;
                if (childNode.IsOutPlace)
                {
                    objDto.Name = needRmovePrefixSpace ? childNode.Path?.RemovePrefixSpace() : childNode.Path;
                }
                else
                {
                    //objDto.Name = path;
                    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                }
                mLog.Debug(@"Looks up a localized string similar to Getting restore content {0} finished..", objDto.Name);
                return objDto;
            }
            return null;
        }


        private NodeLevel GetCheckedNodeLevel(SPTreeNodeDto dto)
        {
            if (dto == null)
            {
                return NodeLevel.Undefined;
            }
            if (dto.CheckNumber == 1)
            {
                return dto.Level;
            }
            if (dto.Children != null)
            {
                foreach (SPTreeNodeDto children in dto.Children)
                {
                    NodeLevel res = GetCheckedNodeLevel(children);
                    if (res != NodeLevel.Undefined)
                    {
                        return res;
                    }
                }
            }
            return NodeLevel.Undefined;
        }

        private bool NeedRmovePrefixSpace(GDriveDataType type)
        {
            return type == GDriveDataType.File;
        }

        /// <summary>
        /// Whether the list is myProfile list.1 for true,otherwise false
        /// </summary>
        /// <param name="ele"></param>
        /// <returns>If listType attribute exist,return the value of listType;else if isMyProfileList attribute exist,return the value of isMyProfileList;otherwise return default value(0).</returns>
        private int GetIsMyProfileAttribute(XmlElement ele)
        {
            int isMyProfile = GetAttributeInt("isMyProfileList", ele, 0);//For old data
            return GetAttributeInt("listType", ele, isMyProfile);
        }

        public string GetFileTail()
        {
            return mRestoreStream.ReadTail();
        }

        private static int GetAttributeInt(string name, XmlElement rootElement, int defaultValue)
        {
            return !rootElement.HasAttribute(name) ? defaultValue : int.Parse(rootElement.GetAttribute(name));
        }

        private static long GetAttributeLong(string name, XmlElement rootElement, long defaultValue)
        {
            return !rootElement.HasAttribute(name) ? defaultValue : long.Parse(rootElement.GetAttribute(name));
        }

        //private bool GetAttributeBoolean(string name, XmlElement rootElement)
        //{
        //    return GetAttributeBoolean(name, rootElement, false);
        //}

        private static bool GetAttributeBoolean(string name, XmlElement rootElement, bool defaultValue)
        {
            return !rootElement.HasAttribute(name) ? defaultValue : bool.Parse(rootElement.GetAttribute(name));
        }


        //private static string GetDestPath(RestoreTreeNode node)
        //{
        //    while (node.Children != null && node.Children.Count > 0)
        //    {
        //        Dictionary<string, RestoreTreeNode>.Enumerator childs = node.Children.GetEnumerator();
        //        childs.MoveNext();
        //        node = childs.Current.Value;
        //    }
        //    return node.Path;
        //}

        //private static string GetSiteSubUrl(string siteUrl)
        //{
        //    int pos = siteUrl.IndexOf("://", StringComparison.Ordinal);
        //    if (pos < 0)
        //    {
        //        return null;
        //    }
        //    pos = siteUrl.IndexOf("/", pos + 3, StringComparison.Ordinal);
        //    if (pos > 0)
        //    {
        //        return siteUrl.Substring(pos + 1, siteUrl.Length - pos - 1);
        //    }
        //    return null;
        //}

        private static string GetNameFromPath(string path)
        {
            int pos = path.LastIndexOf('\\');
            return path.Substring(pos + 1);
        }

        private static RestoreTreeNode GetParentNode(RestoreTreeNode currentNode, string path, GDriveDataType type)
        {
            RestoreTreeNode parentNode = currentNode;
            char parentType;
            //switch (type)
            //{
            //    case AveConstants.TYPE_SITE:
            //        parentType = AveConstants.TYPE_WEBAPPLICATION;
            //        break;
            //    case AveConstants.TYPE_WEB:
            //        parentType = AveConstants.TYPE_SITE;
            //        break;
            //    case AveConstants.TYPE_LIST:
            //    case AveConstants.TYPE_PROJECT:
            //    case AveConstants.TYPE_APP:
            //        parentType = AveConstants.TYPE_WEB;
            //        break;
            //    default:
            //        parentType = AveConstants.TYPE_LIST;
            //        break;
            //}
            //if (type != AveConstants.TYPE_FOLDER)
            //{
            //    while (parentNode.Type != parentType)
            //    {
            //        parentNode = currentNode.Parent;
            //        if (parentNode == null)
            //        {
            //            throw new AveException(@"Looks up a localized string similar to Cannot find the parent node. Path: {0}, type: {1}, current node name: {2}, current node type: {3}.", path, type, currentNode.Name, currentNode.Type);
            //        }
            //        if (currentNode.Type != AveConstants.TYPE_WEB)
            //        {
            //            parentNode.RemoveChild(currentNode.Name); // Clear for GC
            //            currentNode.Parent = null;
            //        }
            //        currentNode = parentNode;
            //    }
            //    return parentNode;
            //}
            string parentPath;
            int pos = path.LastIndexOf('\\');
            if (pos >= 0)
            {
                parentPath = path.Substring(0, pos);
            }
            else
            {
                throw new AveException(@"Looks up a localized string similar to There is no &apos;\\&apos; in folder&apos;s path. Path: {0}.", path);
            }
            while (!string.IsNullOrEmpty(parentNode.SrcPath) && !parentNode.SrcPath.Equals(parentPath))
            {
                parentNode = currentNode.Parent;
                if (parentNode == null)
                {
                    throw new AveException(@"Looks up a localized string similar to Cannot find the parent node. Path: {0}, type: {1}, current node name: {2}, current node type: {3}.", path, type, currentNode.Name, currentNode.Type);
                }
                parentNode.RemoveChild(currentNode.Name); // Clear for GC
                currentNode.Parent = null;
                currentNode = parentNode;
            }
            return parentNode;
        }

        private static string GetPathFromName(RestoreTreeNode parentNode, string name, GDriveDataType type)
        {
            //if (type == AveConstants.TYPE_SITE)
            //{
            //    return name;
            //}
            //if (type == AveConstants.TYPE_WEB)
            //{
            //    if (name.Equals(AveConstants.ROOT_WEB))
            //    {
            //        return name;
            //    }
            //    string parentName = AveConstants.ROOT_WEB;
            //    if (name.Contains('/'))
            //    {
            //        int index = name.LastIndexOf('/');
            //        parentName = name.Substring(0, index);
            //        name = name.Substring(index + 1);
            //    }
            //    RestoreTreeNode realParentNode = parentNode.GetChild(parentName);
            //    if (realParentNode == null)
            //    {
            //        mLog.Error(@"Looks up a localized string similar to Cannot find the parent node {0}. Current node: {1}.", parentName, name);
            //        return parentNode.Path + "/" + name;
            //    }
            //    if (realParentNode.Path.Equals(AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return name;
            //    }
            //    return realParentNode.Path + "/" + name;
            //}
            return parentNode.Path + "\\" + name;
        }

    }
}
