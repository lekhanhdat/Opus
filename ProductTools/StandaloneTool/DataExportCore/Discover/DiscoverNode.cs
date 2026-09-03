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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using System.Reflection;
namespace DataExportCore;

public class DiscoverNode
{
    protected readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(DiscoverNode));
    public string Id { get { return Index.Id; } }

    public string Name { get { return Index.Name; } }

    public string Type { get { return Index.Type; } }

    public string Url { get { return Index.Url; } }

    public string PathMD5 { get { return Index.PathMD5; } }

    public string ParentPathMD5 { get { return Index.ParentPathMD5; } }

    public string SitePath { get { return Index.SitePath; } }

    public string BackupJobId { get { return Index.JobId; } }

    public NodeType Level { get; set; }

    public string ExportPath { get; set; }

    public long ExportedFileSize { get; set; }

    public ArchiverBasicIndex Index { get; set; }

    public DiscoverNode(ArchiverBasicIndex index)
    {
        Index = index;
        ExportPath = string.Empty;
    }
}

public class SiteDiscoverNode : DiscoverNode
{
    public SiteDiscoverNode(ArchiverBasicIndex index) : base(index)
    {
        Level = NodeType.Site;
    }
}

public class WebDiscoverNode : DiscoverNode
{
    public WebDiscoverNode(ArchiverBasicIndex index) : base(index)
    {
        Level = NodeType.Web;
    }
}

public class ListDiscoverNode : FolderDiscoverNode
{
    public ListDiscoverNode(ArchiverBasicIndex index, IIndexProcessor<ArchiverIndexProcessorParameter> indexProcessor) : base(index, indexProcessor)
    {
        Level = NodeType.List;
    }
}

public class FolderDiscoverNode : DiscoverNode
{
    public List<FolderDiscoverNode> SubFolders { get; set; }

    private List<ItemDiscoverNode>? _items;

    public List<ItemDiscoverNode> Items
    {
        get
        {
            if (_items == null)
            {
                _items = GetItemNode();
            }
            return _items;
        }
    }

    private IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor;

    public FolderDiscoverNode(ArchiverBasicIndex index, IIndexProcessor<ArchiverIndexProcessorParameter> indexProcessor) : base(index)
    {
        Level = NodeType.Folder;
        IndexProcessor = indexProcessor;
        SubFolders = new List<FolderDiscoverNode>();
    }

    private List<ItemDiscoverNode> GetItemNode()
    {
        var itemNodes = new List<ItemDiscoverNode>();
        var sql = "select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveBody
            + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
        try
        {
            var indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, SiteDiscover.GenerateDefaultParameters(PathMD5)) ?? [];
            logger.Info($"Item node in {Level} [{Url}] retrieved successfully.");
            indexList.ForEach(w => itemNodes.Add(new ItemDiscoverNode(w)));

            return itemNodes;
        }
        catch (Exception e)
        {
            logger.Error($"An error occurred while retrieving item node. Error: {e}");
            throw;
        }
    }
}

public class ItemDiscoverNode : DiscoverNode
{
    private string? _storageId;
    public string StorageId
    {
        get
        {
            if (_storageId == null)
            {
                _storageId = Index.StoragePolicyId;

                if (!GlobalDeviceCache.IsDeviceExist(_storageId))
                {
                    _storageId = GlobalDeviceCache.GetCurrentStoragePolicyIdBySubjobId(BackupJobId);
                }

                logger.Info($"[{Level}][{Url}] has storageId [{_storageId}], jobId [{BackupJobId}], isChanged [{_storageId == Index.StoragePolicyId}].");
            }
            return _storageId;
        }
    }

    public string DataVolume { get { return GetDataVolume(); } }

    private DataEncryptionInfo? _dataEncryptionInfo;

    private bool _isInitDataEncryptionInfo;

    public DataEncryptionInfo? DataEncryptionInfo
    {
        get
        {
            if (_isInitDataEncryptionInfo == true) return _dataEncryptionInfo;

            _dataEncryptionInfo = GetDataEncryptionInfo();
            _isInitDataEncryptionInfo = true;
            return _dataEncryptionInfo;
        }
    }

    public ItemDiscoverNode(ArchiverBasicIndex index) : base(index)
    {
        switch (index.Type)
        {
            case "A":
                Level = NodeType.Attachment;
                break;
            case "D":
            case "V":
                Level = NodeType.Document;
                break;
            case "I":
            case "U":
                Level = NodeType.ListItem;
                break;
            default:
                break;
        }
    }

    private string GetDataVolume()
    {
        var volumeParam = new VolumeParameter()
        {
            FarmName = string.Empty,
            SiteCollectionUrl = SitePath,
        };

        return new ArchiverVolumeGenerator().GenerateDataVolume(volumeParam);
    }

    private DataEncryptionInfo? GetDataEncryptionInfo()
    {
        try
        {
            return GlobalDeviceCache.GetEncryptionInfoBySubJobId(BackupJobId);
        }
        catch (ManagedException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.Error($"An error occurred when getting the Data Encryption Info in {BackupJobId} for item {Name}. Ex: {e}");
            throw new Exception(I18NEntity.GetString("SATool_ExportItemUnexpectedError"));
        }
    }
}
