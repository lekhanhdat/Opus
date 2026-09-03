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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.Records.Core.Utilities.Extensions;
using CamlBuilder;
using Cloud.Sdk.Data.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.EDiscovery
{
    public class RMArchivedFullTextIndexEDiscoveryDataAppender : RMArchivedFullTextIndexEDiscoveryDataOperator
    {

        public RMArchivedFullTextIndexEDiscoveryDataAppender(
            RMArchivedFullTextIndexSiteManager siteManager, 
            RMArchivedFullTextIndexJobManager jobManager, 
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
            : base(siteManager, jobManager, syncJobManager)
        {
        }

        protected override IndexType OperateType => IndexType.Upsert;

        protected override string OperateName => "ItemAppend";

        private readonly AppendState _appendState = new();

        public async Task<bool> AppendAsync(RMArchivedFullTextIndexDataInfo item)
        {
            try
            {
                var archivedMonth = new DateTime(item.ArchiverTime, DateTimeKind.Utc).ToString("yyyyMM");
                var fieldList = BuildDataFieldList(item);
                var fieldJson = JsonConvert.SerializeObject(fieldList);
                var dataFolderPath = BuildAppendFolderPath(archivedMonth);
                _dataFilePath = SecurityUtils.SafeCombinePath(dataFolderPath, $"{OperateName}_data.txt");
                await AppendLineAndUploadIfNeededAsync(
                    fieldJson,
                    _appendState,
                    "Write append item info to local file",
                    $"The appender [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data is reach limit, need to upload data to e-discovery.",
                    dataFolderPath);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append item [{item.FullPath}]. Error: {e}");
                return false;
            }
        }
        
        private string BuildAppendFolderPath(string appendDate)
        {
            var eDiscoveryFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "e_discovery");
            EnsureDirectory(eDiscoveryFolderPath);

            var jobFolderPath = SecurityUtils.SafeCombinePath(eDiscoveryFolderPath, _syncJobManager.ArchiverJobId);
            EnsureDirectory(jobFolderPath);

            var dateFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "data");
            EnsureDirectory(dateFolderPath);

            var dataFolderPath = SecurityUtils.SafeCombinePath(dateFolderPath, CATEGORY_NAME);
            EnsureDirectory(dataFolderPath);

            dataFolderPath = SecurityUtils.SafeCombinePath(dataFolderPath, OperateName);
            EnsureDirectory(dataFolderPath);

            if (!string.IsNullOrEmpty(appendDate) && _dataFolderPaths.TryGetValue(appendDate, out var existDataFolderPath))
            {
                dataFolderPath = existDataFolderPath;
            }
            else if (!string.IsNullOrEmpty(appendDate))
            {
                dataFolderPath = SecurityUtils.SafeCombinePath(dataFolderPath, appendDate);
                EnsureDirectory(dataFolderPath);
                _dataFolderPaths[appendDate] = dataFolderPath;
            }

            return dataFolderPath;
        }

        private List<Field> BuildDataFieldList(RMArchivedFullTextIndexDataInfo item)
        {
            return
            [
                new()
                {
                    Name = "name",
                    Value = item.Name,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "indexDBUniqueId",
                    Value = item.IndexDBUniqueId,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "siteId",
                    Value = item.SiteId,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "fileType",
                    Value = item.FileType,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "siteUrl",
                    Value = item.SiteUrl,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "siteUrlMd5",
                    Value = item.SiteUrl.ToLower().ToMD5HashCode(),
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "fullPath",
                    Value = item.FullPath,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "friendlyFullPath",
                    Value = item.FriendlyFullPath,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "content",
                    Value = item.Content,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "fileSize",
                    Value = item.FileSize.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "isCurrentVersion",
                    Value = item.IsCurrentVersion.ToString(),
                    FieldType = FieldType.Boolean | FieldType.NeedStore | FieldType.NeedIndex
                },
                new()
                {
                    Name = "uIVersion",
                    Value = item.UIVersion.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedStore
                },
                new()
                {
                    Name = "archiverTime",
                    Value = item.ArchiverTime.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "createdTime",
                    Value = item.CreateTime.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "modifiedTime",
                    Value = item.ModifiedTime.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "nodeLevel",
                    Value = item.NodeLevel.ToString(),
                    FieldType = FieldType.Int | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "pathMd5",
                    Value = item.PathMd5,
                    FieldType = FieldType.String | FieldType.NeedStore | FieldType.NeedIndex
                },
                new()
                {
                    Name = "parentPathMd5",
                    Value = item.ParentPathMd5,
                    FieldType = FieldType.String | FieldType.NeedStore | FieldType.NeedIndex
                },
                new()
                {
                    Name = "author",
                    Value = item.Author,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "editor",
                    Value = item.Editor,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore | FieldType.NeedTokenize
                },
                new()
                {
                    Name = "treeNode",
                    Value = item.TreeNode,
                    FieldType = FieldType.String | FieldType.NeedStore
                },
                new()
                {
                    Name = "archiverSubJobId",
                    Value = item.ArchiverJobId,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "accessTierType",
                    Value = item.AccessTierType.ToString(),
                    FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                },
                new()
                {
                    Name = "typeInIndex",
                    Value = item.TypeInIndex,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedStore
                },
                new() 
                { 
                    Name = "metadataInfo",
                    Value = item.MetadataInfo,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                }
            ];
        }
    }
}
