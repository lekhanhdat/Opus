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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Service.Services.Settings;
using Media.Common.ClassicStorageApi;
using Microsoft.SharePoint.Client;
using RAExportCommon;
using Storage;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexDBManager : System.IDisposable
    {
        private static readonly string s_encryptionKey;

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexDBManager));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IndexDatabaseHelper _indexDBHelper;

        private readonly ArchiverVolumeGenerator _volumeGenerator;

        private readonly RMArchivedFullTextIndexSiteManager _siteManager;

        private readonly string _indexDBName;

        private IXSystem _indexStorageDevice;

        private IXSystem _indexLocalDevice;

        static RMArchivedFullTextIndexDBManager()
        {
            s_encryptionKey = new SettingProfileService().GetDBSEEMasterKey().Replace("\"", "#").Replace("\\", "*");
        }

        public RMArchivedFullTextIndexDBManager(RMArchivedFullTextIndexSiteManager siteManager)
        {
            _indexDBHelper = new();
            _volumeGenerator = new();
            _siteManager = siteManager;
            _indexDBName = "index.db";
        }

        public void Open()
        {
            var storageInfo = OpenStorageDevice();
            var localInfo = OpenLocalDevice();

            var buffer = new byte[1024];
            using (var storageStream = _indexStorageDevice.OpenStream(storageInfo, FileMode.Open))
            {
                using var localStream = _indexLocalDevice.OpenStream(localInfo, FileMode.CreateNew);
                var readLen = 0;
                while ((readLen = storageStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    localStream.Write(buffer, 0, readLen);
                }
            }

            _indexDBHelper.Open(SecurityUtils.SafeCombinePath(System.Environment.CurrentDirectory, "index_db_cache", _indexDBName), s_encryptionKey);
            _logger.Info($"The index db of site [{_siteManager.SiteUrl}] has been download and open.");
        }

        public bool TryGetFirst(string jobId, out ArchiverBasicIndex item)
        {
            item = null;

            var sql = $@"SELECT * FROM TB_BODY_INDEX 
WHERE COL_JOBID = '{jobId}' AND COL_TYPE = 'D' AND (COL_ISSYSTEMFILE LIKE 'False' OR COL_ISSYSTEMFILE IS NULL) 
ORDER BY COL_ARCHIVE_TIME 
LIMIT 1 OFFSET 0";
            var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, []);
            if (items == null || items.Count == 0)
            {
                return false;
            }

            item = items.First();
            return true;
        }

        public IEnumerable<(ArchiverBasicIndex Item, TreeNode Node)> Read(string jobId, int pageSize)
        {
            using (new PerformanceScope($"Read batch items", $"[{_siteManager.SiteUrl}]", true))
            {
                for (var i = 0; ; i++)
                {
                    var sql = $@"SELECT * FROM TB_BODY_INDEX 
WHERE COL_JOBID = '{jobId}' AND COL_TYPE = 'D' AND (COL_ISSYSTEMFILE LIKE 'False' OR COL_ISSYSTEMFILE IS NULL) 
ORDER BY COL_ARCHIVE_TIME 
LIMIT {pageSize} OFFSET {i * pageSize}";
                    var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, []);
                    foreach(var item in items)
                    {
                        var treeNode = AssembleTreeNode(item);
                        yield return (item, treeNode);
                    }

                    if (items.Count < pageSize)
                    {
                        break;
                    }
                }
            }
        }

        public List<ArchiverBasicIndex> ReadRelateOldDataList(string id, string pathMd5)
        {
            using (new PerformanceScope($"Read old data from the same path", $"[{_siteManager.SiteUrl}]", true))
            {
                var sql = $@"SELECT * FROM TB_BODY_INDEX
WHERE COL_PATH_MD5 = '{pathMd5}' AND COL_ID != '{id}'";
                var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, []);
                return items;
            }
        }

        public string GetFriendlyFullPath(ArchiverBasicIndex index)
        {
            using (new PerformanceScope($"Read friendly full path", $"[{_siteManager.SiteUrl}]", true))
            {
                if (index.Type == "D")
                {
                    var sql = "select * from " + IndexConstants.TableNameArchiveHead
            + " where COL_PATH_MD5 = @COL_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
            + " order by COL_ARCHIVE_TIME desc";
                    var items = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new()
                {
                    { "@COL_PATH_MD5", index.ParentPathMD5 },
                    { "@COL_ARCHIVE_TIME", index.ArchiveTime }
                });

                    if (items.Count > 0)
                    {
                        return index.SitePath + "\\" + items[0].Name + "\\" + index.Name;
                    }
                }

                return index.Url;
            }
        }

        private TreeNode AssembleTreeNode(ArchiverBasicIndex index)
        {
            using (new PerformanceScope($"Assemble tree node", $"[{_siteManager.SiteUrl}]", true))
            {
                var rootName = index.SitePath;
                return AssembleBranch(index, null, index.SitePath, rootName);
            }
        }

        private TreeNode AssembleBranch(ArchiverBasicIndex index, TreeNode node, string siteUrl, string root)
        {
            var parentIndex = GetParentIndex(index);

            TreeNode resultNode;
            if (IsRootParentCase(parentIndex, index, siteUrl, root))
            {
                resultNode = node ?? CreateTreeNode(index, true);
                resultNode.Parent = CreateTreeNode(parentIndex, true);
            }
            else
            {
                var parentNode = CreateTreeNode(parentIndex, false);
                var childNode = node ?? CreateTreeNode(index, true);
                parentNode.Children.Add(childNode);
                resultNode = parentIndex.Name != root
                    ? AssembleBranch(parentIndex, parentNode, siteUrl, root)
                    : parentNode ?? CreateTreeNode(index, true);
            }

            resultNode.Count = index.FlagExtend;
            return resultNode;
        }

        private ArchiverBasicIndex GetParentIndex(ArchiverBasicIndex index)
        {
            var parentPathMd5 = index.ParentPathMD5 ?? string.Empty;
            var archiveTime = index.ArchiveTime;

            var sql = @"SELECT * FROM TB_HEAD_INDEX 
WHERE COL_PATH_MD5 = @COL_PATH_MD5  
AND COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME 
ORDER BY COL_ARCHIVE_TIME DESC";
            var indexes = _indexDBHelper.ExecuteReader<ArchiverBasicIndex>(sql, new()
            {
                { "@COL_PATH_MD5", parentPathMd5 },
                { "@COL_ARCHIVE_TIME", archiveTime }
            });
            return indexes.Count > 0 ? indexes[0] : new ArchiverBasicIndex();
        }

        private static bool IsRootParentCase(ArchiverBasicIndex parentIndex, ArchiverBasicIndex index, string siteUrl, string root)
        {
            return parentIndex.Name == siteUrl && index.Name != "." && root == ".";
        }

        private TreeNode CreateTreeNode(ArchiverBasicIndex index, bool isLeafNode)
        {
            var treeNodeDto = new TreeNode();
            PopulateNodeLevelAndType(treeNodeDto, index);
            PopulateNodeNames(treeNodeDto, index);
            PopulateNodeFields(treeNodeDto, index);
            PopulateNodeTitleAndDescription(treeNodeDto, index);

            if (!isLeafNode)
            {
                treeNodeDto.SelectorHidden = true;
            }

            return treeNodeDto;
        }

        private static void PopulateNodeLevelAndType(TreeNode treeNodeDto, ArchiverBasicIndex index)
        {
            treeNodeDto.TreeNodeLevel = index.Type.ToNodeLevelByMediaDataTypeString().ToString().ToEnum<TreeNodeLevel>();
            if (treeNodeDto.TreeNodeLevel == TreeNodeLevel.List && index.ListType != 0)
            {
                treeNodeDto.Type = TreeNodeType.DocumentLibrary;
            }
            else
            {
                treeNodeDto.Type = TreeNodeType.GenericList;
            }
        }

        private static void PopulateNodeNames(TreeNode treeNodeDto, ArchiverBasicIndex index)
        {
            int position = index.Name.Contains("\\")
                ? index.Name.LastIndexOf("\\", System.StringComparison.OrdinalIgnoreCase)
                : index.Name.LastIndexOf("/", System.StringComparison.OrdinalIgnoreCase);
            string tempName = treeNodeDto.TreeNodeLevel == TreeNodeLevel.SiteCollection
                ? index.Name
                : index.Name.Substring(position + 1);
            treeNodeDto.Name = tempName;
            treeNodeDto.DisplayName = tempName;
        }

        private static void PopulateNodeFields(TreeNode treeNodeDto, ArchiverBasicIndex index)
        {
            treeNodeDto.FullPath = index.Url;
            treeNodeDto.FullPathForUI = index.Url;
            treeNodeDto.FarmName = "";
            treeNodeDto.FarmId = "";
            treeNodeDto.ID = System.Guid.NewGuid().ToString();
            treeNodeDto.Expanded = true;
            treeNodeDto.SitePath = index.SitePath;
            treeNodeDto.ChildrenLoaded = true;
            treeNodeDto.CanChildrenBeLoaded = true;
            treeNodeDto.ModifiedTime = index.ModifyTime;
            treeNodeDto.CreatedTime = index.CreateTime;
            treeNodeDto.ArchivedTime = index.ArchiveTime;
            treeNodeDto.PathMD5 = index.PathMD5;
            treeNodeDto.ParentPathMD5 = index.ParentPathMD5;
            treeNodeDto.TypeInIndex = index.Type;
            treeNodeDto.ModifiedBy = index.Editor;
        }

        private static void PopulateNodeTitleAndDescription(TreeNode treeNodeDto, ArchiverBasicIndex index)
        {
            if (index.Type.Equals("W", System.StringComparison.OrdinalIgnoreCase) && !index.Attributes.Equals(string.Empty))
            {
                var tempTitle = index.Attributes.Substring(index.Attributes.IndexOfIgnoreCase("Title:") + 6);
                treeNodeDto.Title = tempTitle.Remove(tempTitle.IndexOf(ServiceConstants.ExtraChar));
            }

            if (index.Attributes.Contains(ServiceConstants.Delimiter.ToString()))
            {
                treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), System.Environment.NewLine).Replace(ServiceConstants.Delimiter.ToString(), ":");
            }
            else
            {
                treeNodeDto.Description = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), System.Environment.NewLine);
            }
        }

        private StorageInfo OpenStorageDevice()
        {
            var storageDeviceDto = _storageDeviceService.GetIndexDevice();
            var logicDeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives =
                [
                    new()
                    {
                        Id = storageDeviceDto.Id,
                        ConnectionString = storageDeviceDto.ConnectionString,
                        ModifyTime = storageDeviceDto.ModifyTime,
                        Type = storageDeviceDto.Type,
                    }
                ]
            };
            _indexStorageDevice = XFactoryCommon.InstanceSystem(logicDeviceDto.ToXRIS().First());
            _indexStorageDevice.Open();
            _logger.Info($"The storage device of site [{_siteManager.SiteUrl}] has been open.");

            var indexVolume = _volumeGenerator.GenerateIndexVolume(new()
            {
                FarmName = "",
                SiteCollectionUrl = _siteManager.SiteUrl
            });
            var indexStorageInfo = XConvert.FromNames(indexVolume, _indexDBName, "");
            return indexStorageInfo;
        }

        private StorageInfo OpenLocalDevice()
        {
            var localPath = System.Environment.CurrentDirectory;
            var localdeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives =
                [
                    PhysicalDeviceDto.GenterateFS(localPath, string.Empty, string.Empty)
                ]
            };
            _indexLocalDevice = XFactoryCommon.InstanceSystem(localdeviceDto.ToXRIS().First());
            _indexLocalDevice.Open();
            _logger.Info($"The local device of site [{_siteManager.SiteUrl}] has been open.");

            return new StorageInfo("index_db_cache", _indexDBName);
        }

        public void Dispose()
        {
            _indexStorageDevice?.Dispose();
            _indexDBHelper.Close();
            _indexLocalDevice?.Dispose();
            _indexLocalDevice?.DeleteFile(new StorageInfo("index_db_cache", _indexDBName));
        }
    }
}
