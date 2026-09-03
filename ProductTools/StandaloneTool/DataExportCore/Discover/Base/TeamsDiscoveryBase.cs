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
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;

namespace DataExportCore.Discover.Base
{
    public class TeamsDiscoveryBase<T>
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(TeamsDiscoveryBase<>));
        protected ExportQueue<T> ExportQueue;
        protected string GroupAddress;
        protected string SiteUrl;
        protected IndexDatabaseHelper DbHelper;
        protected IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor;

        public TeamsDiscoveryBase(string groupAddress, IndexDatabaseHelper dbHelper, IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor, string siteUrl)
        {
            GroupAddress = groupAddress;
            DbHelper = dbHelper;
            this.IndexProcessor = IndexProcessor;
            SiteUrl = siteUrl;
        }

        public void SetExportQueue(ExportQueue<T> exportQueue) => this.ExportQueue = exportQueue;

        protected void LoadAllDataEncryptionInfo()
        {
            logger.Info("Loading all data encryption information of Mail boxes");
            List<ArchiverSiteMasterIndexContract> mailBoxes = GetCommonSiteMasterIndexByGroupAddress();
            if (!mailBoxes.IsNullOrEmpty())
            {
                foreach (ArchiverSiteMasterIndexContract mailBox in mailBoxes)
                {
                    LoadDataEncryptionInfoByJobId(mailBox.JobId);
                }
            }
            else
            {
                logger.Warn("No site collections found to load data encryption info.");
            }
        }

        protected void LoadDataEncryptionInfoByJobId(string jobId)
        {
            logger.Info($"Loading data encryption info by JobId: {jobId}");
            var domains = DbHelper.ExecuteReader<ArchiverIndexSubInfoExportDto>("SELECT * FROM ArchiverIndexSubInfoes where SubJobId = @JobId ", new Dictionary<string, object> { { "@JobId", jobId } });

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

                        GlobalDeviceCache.AddIndexSubInfo(domain.SubJobId, indexSubInfo);
                        logger.Info($"Successfully loaded encryption info for domain with JobId: {domain.SubJobId}");
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

        protected List<ArchiverSiteMasterIndexContract> GetCommonSiteMasterIndexByGroupAddress()
        {
            List<ArchiverSiteMasterIndexContract> contracts = [];

            try
            {
                var domains = DbHelper.ExecuteReader<CommonSiteMasterIndexExportDto>("select * from CommonSiteMasterIndex where SiteURL = @SiteURL order by ArchiverTime desc", new Dictionary<string, object> { { "@SiteURL", GroupAddress } });

                if (domains != null && domains.Count > 0)
                {
                    contracts = domains.Select(_ => ConvertUtil.ConvertSiteMasterDtoToContract(_)).ToList();
                    logger.Info($"Retrieved {contracts.Count} Site Master Index for group Address: {GroupAddress}.");
                }
                else
                {
                    logger.Warn($"No Common Master Index found for group Address: {GroupAddress}.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while retrieving Common Master Index for group Address: {GroupAddress}. Error: {e}");
            }
            return contracts;
        }
    }
}
