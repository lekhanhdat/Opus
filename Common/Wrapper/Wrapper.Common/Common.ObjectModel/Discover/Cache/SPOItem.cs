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

namespace AvePoint.Wrapper.Common
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage;
    using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
    using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.ItemStorage;
    using Microsoft.Graph.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SPOItem
    {
        public virtual int Id { get; set; }

        public string Name { get; set; }
    }

    public sealed class SPOFolder : SPOItem, IDisposable
    {
        protected static AveLogger _log = AveLogger.GetInstance(typeof(SPOFolder));

        public string ParentFolderPath
        {
            get
            {
                var pFolderNames = new List<string>();
                var pFolder = ParentFolder;
                while (pFolder != null)
                {
                    pFolderNames.Add(pFolder.Name);
                    if(pFolder.IsRoot)
                    {
                        break;
                    }
                    else if (pFolder.ParentFolder == null)
                    {
                        throw new Exception($"current un root folder not exist parent folder:{CombineFolderPath(pFolderNames)}, id:{pFolder.Id}");
                    }
                    pFolder = pFolder.ParentFolder;
                }

                return CombineFolderPath(pFolderNames);

                string CombineFolderPath(List<string> folderNames)
                {
                    if (folderNames.Count == 0) return "";

                    StringBuilder pathBuilder = new StringBuilder(folderNames.Last().TrimEnd('/'));
                    for (int i = folderNames.Count-2; i >= 0; i--)
                    {
                        pathBuilder.Append('/');
                        pathBuilder.Append(folderNames[i]);
                    }

                    return pathBuilder.ToString();
                }
            }
        }

        public string FullPath
        {
            get
            {
                var parentPath = ParentFolderPath;
                return string.IsNullOrEmpty(parentPath) ? Name : $"{parentPath}/{Name}";
            }
        }

        private int _id;

        public override int Id 
        { 
            get => _id; 
            set 
            {
                AdaptiveSpoStorage?.Folders?.InternalUpdateCurrentFolderId(value);
                _id = value;
            } 
        }

        public AdaptiveSpoStorage AdaptiveSpoStorage { get; }

        public AdaptiveSpoItemStorage Items => AdaptiveSpoStorage?.Items;

        public AdaptiveSpoFolderStorage SubFolders => AdaptiveSpoStorage?.Folders;

        public SPOFolder ParentFolder { get; }

        public bool IsRoot { get; }

        public CacheDBOperator<SPOItem> ItemCacheDBOperator => AdaptiveSpoStorage?.Items.CacheDBOperator;

        public CacheDBOperator<SPOFolder> FolderCacheDBOperator => AdaptiveSpoStorage?.Folders.CacheDBOperator;

        private SPOFolder(CacheDBOperator<SPOItem> cacheDbItemOperator, CacheDBOperator<SPOFolder> cacheDbFolderOperator, string name, bool forceUseDB = false) 
        {
            IsRoot = true;
            Name = name;
            AdaptiveSpoStorage = new AdaptiveSpoStorage(cacheDbItemOperator, cacheDbFolderOperator, this, forceUseDB);
        }

        public static SPOFolder BuildRootFolder(CacheDBOperator<SPOItem> cacheDbItemOperator, CacheDBOperator<SPOFolder> cacheDbFolderOperator, string name, bool forceUseDB = false)
        {
            return new SPOFolder(cacheDbItemOperator, cacheDbFolderOperator, name, forceUseDB);
        }

        private SPOFolder(SPOFolder parentFolder, string name, int id = 0, bool forceUseDB = false)
        {
            IsRoot = false;
            Name = name;
            _id = id;
            ParentFolder = parentFolder;
            AdaptiveSpoStorage = new AdaptiveSpoStorage(parentFolder.ItemCacheDBOperator, parentFolder.FolderCacheDBOperator, this, forceUseDB);
        }

        public static SPOFolder BuildUnRootFolder(SPOFolder parentFolder, string name, int id, bool forceUseDB = false)
        {
            return new SPOFolder(parentFolder, name, id, forceUseDB);
        }

        public SPOFolder()
        {
            
        }

        public void Dispose()
        {
            if (IsRoot)
            {
                ReleaseResource();
            }
        }

        public void ReleaseResource()
        {
            try
            {
                if (SubFolders != null)
                {
                    foreach (var subFolder in SubFolders)
                    {
                        subFolder?.ReleaseResource();
                    }
                }
                AdaptiveSpoStorage?.Dispose();
            }
            catch(Exception ex)
            {
                _log.Error($"ReleaseResource failed for folder {FullPath},ex:{ex}");
            }            
        }
    }
}