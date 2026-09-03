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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;

namespace DataExportCore;

//ArchiverRestoreTreeHandler
public class SiteDiscover
{
    private readonly RALogger logger = RALogger.GetInstance(typeof(SiteDiscover));
    ExportQueue exportQueue;

    private string _siteURL;
    private IndexDatabaseHelper _dbHelper;
    private IIndexProcessor<ArchiverIndexProcessorParameter> _indexProcessor;

    public SiteDiscover(string siteURL, IndexDatabaseHelper dbHelper, IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor)
    {
        _siteURL = siteURL;
        _dbHelper = dbHelper;
        _indexProcessor = IndexProcessor;
    }

    public void Process(ExportQueue exportQueue)
    {
        try
        {
            this.exportQueue = exportQueue;
            logger.Info($"Starting the process of discovering site {_siteURL}");

            LoadAllDataEncryptionInfo();

            var siteNode = GetSiteCollectionNode();
            exportQueue.Enqueue(siteNode);

            Task.Run(() => ProcessSite(siteNode)).Wait();
            exportQueue.Finish();
            logger.Info("Finished processing the export queue.");
        }
        catch (Exception e)
        {
            logger.Error($"An error occur while discover site {_siteURL}. Ex: {e}");
        }
    }

    public void LoadAllDataEncryptionInfo()
    {
        logger.Info("Loading all data encryption information");
        List<ArchiverSiteMasterIndexContract> siteCollections = GetAllSiteMasterIndexById();
        if (!siteCollections.IsNullOrEmpty())
        {
            foreach (ArchiverSiteMasterIndexContract siteCollection in siteCollections)
            {
                LoadDataEncryptionInfoByJobId(siteCollection.JobId);
            }
        }
        else
        {
            logger.Warn("No site collections found to load data encryption info.");
        }
    }

    public void LoadDataEncryptionInfoByJobId(string jobId)
    {
        logger.Info($"Loading data encryption info by JobId: {jobId}");
        var domains = _dbHelper.ExecuteReader<ArchiverIndexSubInfoExportDto>("SELECT * FROM ArchiverIndexSubInfoes where JobId like @JobId ", new Dictionary<string, object> { { "@JobId", jobId + "%" } });

        if (domains != null && domains.Count > 0)
        {
            foreach (ArchiverIndexSubInfoExportDto domain in domains)
            {
                try
                {
                    var indexSubInfo = new ArchiverIndexSubInfoContract
                    {
                        JobId = domain.JobId,
                        CurrentStorageId = domain.CurrentStorageId,
                        StorageInfo = domain.StorageId,
                    };

                    if (domain.DataEncryptionDynamicKey == null && domain.DataEncryptionType == (int)EncryptionAlgorithm.BLOWFISH_ENCRYPTION)
                    {
                        logger.Info($"data encryption infor is null, using BLOWFISH_ENCRYPTION for domain with JobId: {domain.JobId}");
                    }
                    else
                    {
                        indexSubInfo.DataEncryptionInfo = new DataEncryptionInfo
                        {
                            EncryptionType = domain.DataEncryptionType,
                            EncryptedDynamicKey = ExportUtility.CustomAesEncryptorWrapper.Decrypt(domain.DataEncryptionDynamicKey)
                        };
                    }

                    GlobalDeviceCache.AddIndexSubInfo(domain.JobId, indexSubInfo);
                    logger.Info($"Successfully loaded encryption info for domain with JobId: {domain.JobId}");
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to load data encryption info for domain with JobId: {domain.JobId}. Error: {e}");
                }
            }
        }
        else
        {
            logger.Warn($"No SubInfo found for JobId: {jobId}");
        }
    }

    public List<ArchiverSiteMasterIndexContract> GetAllSiteMasterIndexById()
    {
        List<ArchiverSiteMasterIndexContract> contracts = [];

        try
        {
            var domains = _dbHelper.ExecuteReader<ArchiverSiteMasterIndexExportDto>("select * from ArchiverSiteMasterIndexes where SiteURL = @SiteURL order by ArchiverTime desc", new Dictionary<string, object> { { "@SiteURL", _siteURL } });

            if (domains != null && domains.Count > 0)
            {
                contracts = domains.Select(ConvertUtil.ConvertSiteMasterDtoToContract).ToList();
                logger.Info($"Retrieved {contracts.Count} Site Master Index for SiteUrl: {_siteURL}.");
            }
            else
            {
                logger.Warn($"No Site Master Index found for SiteUrl: {_siteURL}.");
            }
        }
        catch (Exception e)
        {
            logger.Error($"An error occurred while retrieving Site Master Index for SiteUrl: {_siteURL}. Error: {e}");
        }
        return contracts;
    }

    SiteDiscoverNode GetSiteCollectionNode()
    {
        var sql = "select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveHead
            + " where COL_PATH_MD5 = @COL_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
        var parameterDictionary = new Dictionary<String, Object>
        {
            ["@COL_PATH_MD5"] = _siteURL.ToMD5HashCode(),
            ["@COL_FLAG"] = 0,
            ["@COL_ARCHIVE_END_TIME"] = DateTime.MaxValue.Ticks
        };
        try
        {
            var siteIndex = this._indexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary).FirstOrDefault() ?? new();
            logger.Info($"[{NodeType.Site}][{_siteURL}] Site collection node retrieved successfully.");
            return new SiteDiscoverNode(siteIndex);
        }
        catch (Exception e)
        {
            logger.Error($"[{NodeType.Site}][{_siteURL}] An error occurred while retrieving site node. Error: {e}");
            throw;
        }
    }

    List<WebDiscoverNode> GetWebNode(SiteDiscoverNode siteNode)
    {
        var webNodes = new List<WebDiscoverNode>();
        var sql = "select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveHead
            + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
        try
        {
            var indexList = this._indexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, GenerateDefaultParameters(siteNode.PathMD5)) ?? [];
            indexList.ForEach(w => webNodes.Add(new WebDiscoverNode(w)));
            logger.Info($"[{siteNode.Level}][{siteNode.Url}] retrieved {webNodes.Count} web nodes successfully.");
            return webNodes;
        }
        catch (Exception e)
        {
            logger.Error($"[{siteNode.Level}][{siteNode.Url}] An error occurred while retrieving web nodes. Error: {e}");
            throw;
        }
    }

    List<ListDiscoverNode> GetListNode(WebDiscoverNode webNode)
    {
        var listNodes = new List<ListDiscoverNode>();
        var sql = "select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveHead
            + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
        try
        {
            var indexList = this._indexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, GenerateDefaultParameters(webNode.PathMD5)) ?? [];
            indexList.ForEach(w => listNodes.Add(new ListDiscoverNode(w, this._indexProcessor)));
            logger.Info($"[{webNode.Level}][{webNode.Url}] retrieved {listNodes.Count} list nodes successfully.");

            return listNodes;
        }
        catch (Exception e)
        {
            logger.Error($"[{webNode.Level}][{webNode.Url}] An error occurred while retrieving list nodes. Error: {e}");
            throw;
        }
    }

    List<FolderDiscoverNode> GetFolderNode(DiscoverNode parentNode)
    {
        var folderNodes = new List<FolderDiscoverNode>();
        var sql = "select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveHead
            + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
        try
        {
            var indexList = this._indexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, GenerateDefaultParameters(parentNode.PathMD5)) ?? [];
            indexList.ForEach(w => folderNodes.Add(new FolderDiscoverNode(w, this._indexProcessor)));
            logger.Info($"[{parentNode.Level}][{parentNode.Url}] retrieved {folderNodes.Count} folder nodes successfully.");

            return folderNodes;
        }
        catch (Exception e)
        {
            logger.Error($"[{parentNode.Level}][{parentNode.Url}] An error occurred while retrieving folder nodes. Error: {e}");
            throw;
        }
    }

    void ProcessSite(SiteDiscoverNode siteNode)
    {
        var webs = GetWebNode(siteNode);

        foreach (var web in webs)
        {
            try
            {
                exportQueue.Enqueue(web);

                ProcessWeb(web);
                logger.Info($"[{siteNode.Level}][{siteNode.Url}] Processed web node: {web.Name} successfully.");
            }
            catch (Exception e)
            {
                logger.Error($"[{siteNode.Level}][{siteNode.Url}] An error occurred while processing web node: {web.Name}. Error: {e}");
            }

        }
    }

    void ProcessWeb(WebDiscoverNode webNode)
    {
        var lists = GetListNode(webNode);

        foreach (var list in lists)
        {
            try
            {
                ProcessList(list);
                exportQueue.Enqueue(list);
                logger.Info($"[{webNode.Level}][{webNode.Url}] Processed list node: {list.Name} successfully.");
            }
            catch (Exception e)
            {
                logger.Error($"[{webNode.Level}][{webNode.Url}] An error occurred while processing list node: {list.Name}. Error: {e}");
            }
        }
    }

    void ProcessList(ListDiscoverNode listNode)
    {
        var folders = GetFolderNode(listNode);

        foreach (var folder in folders)
        {
            try
            {
                ProcessFolder(folder);
                listNode.SubFolders.Add(folder);
                logger.Info($"[{listNode.Level}][{listNode.Url}] Processed folder node: {folder.Name} successfully");
            }
            catch (Exception e)
            {
                logger.Error($"[{listNode.Level}][{listNode.Url}] An error occurred while processing folder node: {folder.Name}. Error: {e}");
            }
        }

        // Todo: consider about move up when path is too long
    }

    void ProcessFolder(FolderDiscoverNode folderNode)
    {
        var folders = GetFolderNode(folderNode);

        foreach (var folder in folders)
        {
            try
            {
                ProcessFolder(folder);
                folderNode.SubFolders.Add(folder);
                logger.Info($"[{folderNode.Level}][{folderNode.Url}] Processed subfolder node: {folder.Name} successfully");
            }
            catch (Exception e)
            {
                logger.Error($"[{folderNode.Level}][{folderNode.Url}] An error occurred while processing subfolder node: {folder.Name}. Error: {e}");
            }
        }
    }

    public static Dictionary<String, Object> GenerateDefaultParameters(string pathMD5)
    {
        return new Dictionary<String, Object>
        {
            ["@COL_PARENT_PATH_MD5"] = pathMD5,
            ["@COL_FLAG"] = 0,
            ["@COL_ARCHIVE_END_TIME"] = DateTime.MaxValue.Ticks
        };
    }
}
