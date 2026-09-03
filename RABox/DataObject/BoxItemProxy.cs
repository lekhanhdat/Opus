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
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.Records.Core.Utilities.Extensions;
using Box.V2.Models;
using Newtonsoft.Json;
using RABox.Util;

namespace RABox
{
    public class BoxItemProxy
    {
        public string Id { get; internal set; }

        private Guid uniqueId = Guid.Empty;
        public Guid UniqueId
        {
            get
            {
                if (uniqueId == Guid.Empty)
                {
                    uniqueId = $"{_clientContext.ConnectionInfo.EnterpriseId}/{Id}".ToMd5();
                }
                return uniqueId;
            }
        }

        protected BoxFolderProxy _parent;

        public string ParentId { get; internal set; }

        public string ETag { get; internal set; }

        public string Type { get; internal set; }

        public string Name { get; internal set; }

        public long? Size { get; internal set; }

        public long Created { get; internal set; }

        public long Modified { get; internal set; }

        public BoxFolderProxy Parent
        {
            get
            {
                return _parent;
            }
        }

        public string FullPath
        {
            get
            {
                return $"{DirName}\\{Name}";
            }
        }

        public string DirName { get; internal set; }

        public string ScopePath
        {
            get
            {
                return $"{IdPath}\\{UniqueId}";
            }
        }

        public string IdPath { get; internal set; }

        public BoxUser CreateBy { get; internal set; }

        public BoxUser ModifiedBy { get; internal set; }

        public BoxCollection<BoxFolder> PathCollection { get; internal set; }

        public long? TrashedAt { get; internal set; }

        protected BoxClientContext _clientContext;


        public BoxItemProxy(BoxClientContext clientContext, BoxItem boxItem)
        {
            _clientContext = clientContext;
            if (boxItem?.Parent != null)
            {
                ParentId = boxItem.Parent?.Id;
                _parent = new BoxFolderProxy(_clientContext, boxItem.Parent);
            }
            InitProperties(boxItem);
        }

        public BoxItemProxy(BoxClientContext clientContext, BoxItem boxItem, BoxFolderProxy parentFolder)
        {
            _clientContext = clientContext;
            if (parentFolder != null)
            {
                ParentId = parentFolder?.Id;
                _parent = parentFolder;
            }
            InitProperties(boxItem);
        }

        private BoxItemProxy InitProperties(BoxItem _boxItem)
        {
            if (_boxItem == null)
            {
                if (_clientContext == null)
                {
                    throw new Exception("Cannot init properies for box item proxy");
                }

            }else if (_boxItem.Id == BoxUtility.BoxRootFolderId)
            {
                Id = BoxUtility.BoxRootFolderId;
                DirName = "";
                IdPath = "";
                return this;
            }

            Id = _boxItem.Id;
            Name = _boxItem.Name;
            ETag = _boxItem.ETag;
            Type = _boxItem.Type;

            if(_boxItem.CreatedAt != null)
            {
                Created = _boxItem.CreatedAt.Value.UtcTicks;
            }

            if (_boxItem.ModifiedAt != null)
            {
                Modified = _boxItem.ModifiedAt.Value.UtcTicks;
            }

            if (_boxItem.Size != null)
            {
                Size = _boxItem.Size;
            }

            if (_boxItem.PathCollection != null)
            {
                PathCollection = _boxItem.PathCollection;
            }

            if (_boxItem.CreatedBy != null)
            {
                CreateBy = _boxItem.CreatedBy;
            }

            if (_boxItem.ModifiedBy != null)
            {
                ModifiedBy = _boxItem.ModifiedBy;
            }

            if (Parent != null && !Parent.IsRootFolder)
            {
                DirName = $"{Parent.FullPath}";
                IdPath = $"{Parent.ScopePath}";
            }

            TrashedAt = _boxItem.TrashedAt?.UtcTicks;

            return this;
        }

        public List<Guid> BuildAncestors(BoxTreeNode topNode)
        {
            var ancestors = new List<Guid>();

            if (Parent != null && !Parent.IsRootFolder && Parent.ScopePath != null)
            {
                ancestors.AddRange(Parent.ScopePath.Trim('\\').Split('\\').Select(Guid.Parse).Reverse().ToList());
            }

            BoxTreeNode currentNode = topNode;

            while (currentNode.Level != RMNodeLevel.BoxConnection)
            {
                if (currentNode.Level == RMNodeLevel.BoxFolder && currentNode.RealId != topNode.RealId &&
                    topNode.Parent.Level == RMNodeLevel.BoxFolder && currentNode.RealId != topNode.Parent.RealId)
                {
                    ancestors.Add(new(currentNode.Id));
                }

                if (currentNode.Level == RMNodeLevel.BoxUser)
                {
                    ancestors.AddRange([new(currentNode.Id), new(currentNode.ConnectionId), new(currentNode.ContainerId)]);
                    break;
                }

                currentNode = currentNode.Parent;
            }
            
            return ancestors;
        }

        public Record ConvertToRecord(Record? existItem, BoxTreeNode selectedNode)
        {
            if (existItem == null)
            {
                existItem = new Record
                {
                    RecordStatus = 1,
                    CreateDate = Convert.ToInt32(new DateTime(Created, DateTimeKind.Utc).ToString("yyyyMMdd")),
                    TimeCreated = Created,
                    RecordsId = null,
                };
            }

            var extension = Path.GetExtension(Name);
            var type = extension?.IndexOf(".") == 0 ? extension.Substring(1) : "";

            existItem.CollectTime = DateTime.UtcNow.Ticks;
            existItem.SourceFlag = (int)SourceFlag.Box;
            existItem.ETag = ETag;
            existItem.Id = UniqueId;
            existItem.ParentId = Parent != null ? Parent.UniqueId : Guid.Empty;
            existItem.LeafName = Name;
            existItem.ExtensionForFile = Type == "file" ? type : I18NResource.DataTypeBoxFolder;
            existItem.ScopeId = new Guid(selectedNode.Id);
            existItem.NodeType = Type == "file" ? (int)RMNodeLevel.BoxFile : (int)RMNodeLevel.BoxFolder;
            existItem.NodeId = UniqueId; // using for reclassify
            existItem.TimeModified = Modified;
            existItem.ContainerId = selectedNode.ConnectionId;
            existItem.ExternalId = Id;
            existItem.AveSiteId = selectedNode.OwnerId;
            existItem.DirPath = Id == selectedNode.RealId || Id == BoxUtility.BoxRootFolderId ? selectedNode.FullPath : CombinePath(selectedNode.FullPath, FullPath);
            existItem.CreatedBy = CreateBy?.Id == BoxUtility.BoxAnonymousUserId ? I18NEntity.GetString(I18NResource.BoxAnonymousUser) : CreateBy?.Name;
            existItem.ModifiedBy = ModifiedBy?.Id == BoxUtility.BoxAnonymousUserId ? I18NEntity.GetString(I18NResource.BoxAnonymousUser) : ModifiedBy?.Name;
            existItem.Ancestors = BuildAncestors(selectedNode);

            if (Type == "file" && this is BoxFileProxy fileProxy)
            {
                existItem.CreatedBy = CreateBy?.Id == BoxUtility.BoxAnonymousUserId ? 
                    string.IsNullOrEmpty(fileProxy.UploaderDisplayName) ? 
                    I18NEntity.GetString(I18NResource.BoxAnonymousUser) : fileProxy.UploaderDisplayName : CreateBy?.Name;
                existItem.ModifiedBy = ModifiedBy?.Id == BoxUtility.BoxAnonymousUserId ? 
                    string.IsNullOrEmpty(fileProxy.UploaderDisplayName) ? 
                    I18NEntity.GetString(I18NResource.BoxAnonymousUser) : fileProxy.UploaderDisplayName : ModifiedBy?.Name;

                var metaInfo = new RecordMetaInfo
                {
                    FileSize = Size ?? 0,
                };
                existItem.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            return existItem;
        }

        public string CombinePath(string path1, string path2)
        {
            var pathArr1 = path1.Split(@"\");
            var pathArr2 = path2.Trim('\\').Split(@"\");
            
            int index = Array.IndexOf(pathArr2, pathArr1.Last());

            return string.Join(@"\", pathArr1.Concat(pathArr2.Skip(index + 1)));
        }
    }
}
