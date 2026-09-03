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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RATeams.Restore.Common
{
    public class EXORestoreContentReader
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(EXORestoreContentReader));
        private readonly IAveRestoreStream mRestoreStream;
        private readonly XmlDocument mXDoc;

        //private RestoreTreeNode mCurrentNode;
        //private readonly ItemRestoreConfig mConfig;

        public EXORestoreContentReader(IAveRestoreStream restoreStream)
        {
            mRestoreStream = restoreStream;
            mXDoc = new XmlDocument();
        }

        public EXORestoreContentDto MoveNext()
        {
            string head;
            while ((head = mRestoreStream.ReadHead()) != null)
            {
                mXDoc.LoadXml(head);
                XmlElement rootElement = mXDoc.DocumentElement;
                if (rootElement == null)
                {
                    continue;
                }
                //string path = rootElement.Attributes["path"].Value;
                //char type = rootElement.Attributes["type"].Value[0];
                //bool needRmovePrefixSpace = NeedRmovePrefixSpace(type);
                //int flag = GetAttributeInt("versionFlag", rootElement, 1);
                //int isMyProfile = GetIsMyProfileAttribute(rootElement);
                //bool isChecked = GetAttributeBoolean("isChecked", rootElement, true);
                //bool isSelected = GetAttributeBoolean("isSelected", rootElement, false);
                //bool parentIsSelected = GetAttributeBoolean("parentIsSelected", rootElement, false);
                //bool isFailed = GetAttributeBoolean("isFailed", rootElement, false);
                //string storageId = rootElement.Attributes["StorageId"].Value;
                //string Id = rootElement.Attributes["Id"].Value;
                //string backupJobId = rootElement.Attributes["BackUpJobId"].Value;
                //string itemPathMd5 = rootElement.Attributes["ItemPathMD5"].Value;
                //string stubType = rootElement.HasAttribute("stubType") ? rootElement.GetAttribute("stubType") : string.Empty;
                //long archiveTime = GetAttributeLong("ArchiveTime", rootElement, DateTime.UtcNow.Ticks);
                #region SourceAPUrl
                var att = rootElement.Attributes["HeaderExtraAttribute"].Value;
                string srcUrl = string.Empty;
                if (!string.IsNullOrEmpty(att))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(att);
                    var headExtrAtt = xmlDoc.DocumentElement;
                    srcUrl = headExtrAtt.HasAttribute("APUrl") ? headExtrAtt.Attributes["APUrl"].Value : string.Empty;
                }
                #endregion

                //bool property = GetAttributeBoolean("property", rootElement, true);
                //bool security = GetAttributeBoolean("security", rootElement, true);
                //bool isAppData = GetAttributeBoolean("isAppData", rootElement, false);
                /*string appDataName = rootElement.HasAttribute("appDataName") ? rootElement.Attributes["appDataName"].Value : string.Empty;
                if (isAppData && !string.IsNullOrEmpty(appDataName))
                {
                    path = appDataName;
                }*/
                //var objDto = new EXORestoreContentDto
                //{
                //    Type = type,
                //    SrcName = path,
                //    SrcUrl = srcUrl,
                //    OwnerLogin = mConfig.DestinationInfo.OwerLogin,
                //    IsChecked = isChecked,
                //    IsSelected = isSelected,
                //    ParentIsSelected = parentIsSelected,
                //    IsFailed = isFailed,
                //    IsAppData = isAppData,
                //    StubType = stubType,
                //    Id = Id,
                //    StorageId = storageId,
                //    BackUpJobId = backupJobId,
                //    ItemPathMd5 = itemPathMd5,
                //    ArchiveTime = archiveTime
                //};

                //if (type == AveConstants.TYPE_SITE || type == AveConstants.TYPE_WEB ||
                //    type == AveConstants.TYPE_LIST || type == AveConstants.TYPE_FOLDER)
                //{
                //    if ((!isSelected && !parentIsSelected) && (GetNodeLevel(type) <= GetCheckedNodeLevel(mConfig.ArchiverConfigForMedia.TreeRoot)))
                //    {
                //        objDto.RestoreOption.SetRequestOption(property, security, (int)AveRestoreMode.Default);
                //    }
                //    else
                //    {
                //        objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.ContainerRestoreMode);
                //    }
                //}
                //else if (type == AveConstants.TYPE_PROJECT)
                //{
                //    objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.ContainerRestoreMode);
                //}
                //else if (type == AveConstants.TYPE_APP)
                //{
                //    if (!isSelected && !parentIsSelected)
                //    {
                //        objDto.RestoreOption.SetRequestOption(property, security, (int)AveRestoreMode.Default);
                //    }
                //    else
                //    {
                //        objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.AppRestoreMode);
                //    }
                //}
                //else
                //{
                //    var restoreMode = mConfig.ContentRestoreMode;
                //    if (objDto.Type == AveConstants.TYPE_ATTACHMENTS)
                //    {
                //        switch (restoreMode)
                //        {
                //            case AveRestoreMode.AppendANewVersion:
                //            case AveRestoreMode.Default:
                //            case AveRestoreMode.OverWriteByModifiedTime:
                //                restoreMode = AveRestoreMode.Default;
                //                break;
                //            case AveRestoreMode.Append:
                //            case AveRestoreMode.OverWrite:
                //                restoreMode = AveRestoreMode.OverWrite;
                //                break;
                //        }
                //    }
                //    objDto.RestoreOption.SetRequestOption(property, security, (int)restoreMode);
                //}
                //#endregion

                //if (rootElement.HasAttribute("nodeGuid"))
                //{
                //    string idString = rootElement.Attributes["nodeGuid"].Value;
                //    Guid id;
                //    if (Guid.TryParse(idString, out id))
                //    {
                //        objDto.UniqueId = id;
                //    }
                //}

                ////objDto.RestoreOption.SetRequestOption(property, security, (int)mConfig.RestoreMode);
                //objDto.RestoreOption.mAveEventReceiverOption.DISABLE_EVENT_RECEIVER = mConfig.DisableEventReceiver;
                //objDto.RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME = mConfig.SkipIfSameModified;
                //if (type == AveConstants.TYPE_DOCUMENT || type == AveConstants.TYPE_LISTITEM ||
                //    type == AveConstants.TYPE_VERSION || type == AveConstants.TYPE_LISTITEMVERSION)
                //{
                //    objDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM = (flag & 1) != 0;
                //}
                //else if (type == AveConstants.TYPE_FOLDER)
                //{
                //    objDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM = isChecked;
                //}
                //if (isMyProfile == 1)
                //{
                //    objDto.IsMyProfileList = true;
                //}
                //if (mCurrentNode == null) //In place and not replace restore 
                //{
                //    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                //    return objDto;
                //}
                //if (type == AveConstants.TYPE_LISTITEM || type == AveConstants.TYPE_DOCUMENT ||
                //    type == AveConstants.TYPE_ATTACHMENTS || type == AveConstants.TYPE_VERSION)
                //{
                //    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                //    return objDto;
                //}
                //string name = GetNameFromPath(path);
                //RestoreTreeNode parentNode = GetParentNode(mCurrentNode, path, type);
                //RestoreTreeNode childNode = parentNode.GetChild(name);
                //objDto.ParentName = needRmovePrefixSpace ? parentNode.Path?.RemovePrefixSpace() : parentNode.Path;
                //if (childNode != null)
                //{
                //    mCurrentNode = childNode;
                //    if (childNode.IsOutPlace)
                //    {
                //        childNode.SrcPath = path;
                //        objDto.Name = needRmovePrefixSpace ? childNode.Path?.RemovePrefixSpace() : childNode.Path;
                //    }
                //    else
                //    {
                //        objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                //    }
                //    if (childNode.Checked)
                //    {
                //        objDto.ReplaceType = childNode.Type;
                //    }
                //    if (childNode.IgnoreThisNode)
                //    {
                //        continue;
                //    }
                //    return objDto;
                //}
                //// This node cannot be found in restore tree.
                //// We should create a new node and add to restore tree.
                //// In this way, we keep the structure of this node and its children.
                //childNode = new RestoreTreeNode
                //{
                //    Name = name,
                //    Type = type,
                //    Path = GetPathFromName(parentNode, name, type),
                //    IsOutPlace = mCurrentNode.IsOutPlace
                //};
                //if (parentNode.Children != null && parentNode.Children.Count != 0 &&
                //    parentNode.Type != AveConstants.TYPE_SITE)
                //{
                //    mLog.Error(@"Looks up a localized string similar to {0} should be got in the process of restoring. It will be restored to {1}..", path, childNode.Path);
                //}
                //childNode.SrcPath = path;
                //childNode.HasDestNode = false;
                //parentNode.AddChild(childNode);
                //mCurrentNode = childNode;
                //if (childNode.IsOutPlace)
                //{
                //    objDto.Name = needRmovePrefixSpace ? childNode.Path?.RemovePrefixSpace() : childNode.Path;
                //}
                //else
                //{
                //    //objDto.Name = path;
                //    objDto.Name = needRmovePrefixSpace ? path?.RemovePrefixSpace() : path;
                //}
                //mLog.Debug(@"Looks up a localized string similar to Getting restore content {0} finished..", objDto.Name);
                //return objDto;
            }
            return null;
        }

    }
}