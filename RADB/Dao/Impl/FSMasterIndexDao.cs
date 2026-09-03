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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSMasterIndexDao : BaseDao<FSMasterIndex>, IFSMasterIndexDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(FSMasterIndexDao));

        public List<FSMasterIndexContract> GetAllConnectionsInfo()
        {
            List<FSMasterIndexContract> domains = new List<FSMasterIndexContract>();
            List<FSMasterIndex> masterIndexes;
            using (var context = GetNewContext())
            {
                masterIndexes = context.FSMasterIndexs.AsNoTracking().Where(a => a.MergeIndexState == (int)MergeIndexState.Succeed).ToList();
            }
            foreach (var index in masterIndexes)
            {
                domains.Add(ConvertToArchiverSiteMasterIndexContract(index));
            }
            return domains;
        }

        public FSMasterIndexContract GetConnectionInfo(FSMasterIndexContract fsIndex)
        {
            FSMasterIndexContract contract = null;
            List<FSMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.FSMasterIndexs.AsNoTracking().Where(s => s.ConnectionId == fsIndex.ConnectionId).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = ConvertToArchiverSiteMasterIndexContract(domains[0]);
            }
            return contract;
        }
        public List<FSMasterIndexContract> GetConnectionInfos(string connectionId)
        {
            List<FSMasterIndexContract> contract = new List<FSMasterIndexContract>();
            List<FSMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.FSMasterIndexs.AsNoTracking().Where(s => s.ConnectionId == connectionId).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                foreach (var domain in domains)
                {
                    contract.Add(ConvertToArchiverSiteMasterIndexContract(domain));
                }
            }
            return contract;
        }

        public List<FSMasterIndexContract> GetIndexByJobId(string jobId)
        {
            List<FSMasterIndexContract> contract = null;
            List<FSMasterIndex> domains = null;
            using (var context = GetNewContext())
            {
                domains = context.FSMasterIndexs.AsNoTracking().Where(s => s.JobId == jobId).OrderByDescending(s => s.ArchiverTime).ToList();
            }
            if (domains != null && domains.Count > 0)
            {
                contract = new List<FSMasterIndexContract>();
                domains.ForEach(a => contract.Add(ConvertToArchiverSiteMasterIndexContract(a)));
            }
            return contract;
        }

        public string InsertIntoFSMasterIndex(FSMasterIndexContract indexDto)
        {
            string id = null;
            try
            {
                logger.Info("Insert into archiver site master index info from media Connection: {0}, job Id: {1}.", indexDto.ConnectionName, indexDto.JobId);
                using (var context = GetNewContext())
                {
                    var existInfo = context.FSMasterIndexs.AsQueryable().Where(s => s.JobId == indexDto.JobId).ToList();
                    if (existInfo == null || existInfo.Count < 1)
                    {
                        logger.Info("Archiver site master Index with job Id {0} does not exist, create one.", indexDto.JobId);
                        var index = context.FSMasterIndexs.Add(ConvertToArchiverSiteMasterIndex(indexDto));
                        id = index.Id;
                    }
                    else
                    {
                        logger.Info("Archiver site master index with job Id {0} already exists.", indexDto.JobId);
                        id = existInfo[0].Id;
                    }
                    if (indexDto.SubInfo != null)
                    {
                        foreach (ArchiverIndexSubInfoContract subInfo in indexDto.SubInfo)
                        {

                            //if (subInfo.StoragePolicyId != null && subInfo.StoragePolicyId != string.Empty)
                            //{
                            //    StoragePolicyDto dto = GetStoragePolicyInfo(subInfo.StoragePolicyId);
                            //    subInfo.ArchiverSubInfoExtension = new ArchiverSubInfoExtension();
                            //    if (dto != null && dto.RetentionOption != null)
                            //    {
                            //        logger.Info("Save archiver retention settings of storage policy {0} to index.", dto.Name);
                            //        subInfo.ArchiverSubInfoExtension.RetentionOption = dto.RetentionOption;
                            //        subInfo.ArchiverSubInfoExtension.RetentionOption.Schedule = null;
                            //        subInfo.ArchiverSubInfoExtension.PrimaryLogicalId = dto.PrimaryLogicalId;
                            //    }
                            //    subInfo.ArchiverSubInfoExtension.DataEncryptionInfo = subInfo.DataEncryptionInfo;
                            //}
                            //CreateSiteMasterSubIndex(subInfo);
                            FSIndexSubInfo archiverIndexSubInfo = new FSIndexSubInfo();
                            archiverIndexSubInfo.Id = subInfo.Id;
                            archiverIndexSubInfo.SubSubJobId = subInfo.JobId;
                            archiverIndexSubInfo.SubJobId = indexDto.JobId;
                            archiverIndexSubInfo.CurrentStorageId = subInfo.PhysicalDeviceId;
                            archiverIndexSubInfo.StorageId = subInfo.PhysicalDeviceId;
                            archiverIndexSubInfo.StorageInfo = subInfo.StorageInfo;
                            archiverIndexSubInfo.AgentDataSize = subInfo.AgentDataSize;
                            archiverIndexSubInfo.RetentionTime = indexDto.ArchiverTime;
                            archiverIndexSubInfo.RuleId = indexDto.RuleId;
                            archiverIndexSubInfo.RetentionCount = 1;
                            subInfo.ArchiverSubInfoExtension = new ArchiverSubInfoExtension();
                            if (!string.IsNullOrEmpty(subInfo.StoragePolicyId))
                            {
                                var storageInfo = context.RMStorageInfos.Where(s => s.Id.Equals(new Guid(subInfo.StoragePolicyId))).FirstOrDefault();
                                if (storageInfo != null && storageInfo.Retention != null)
                                {
                                    try
                                    {
                                        List<RetentionRule> rules = SerializerHelper.DeserializeByDataContractSerializer<List<RetentionRule>>(storageInfo.Retention);
                                        subInfo.ArchiverSubInfoExtension.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(rules);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"error occured when fs back up insert master index,error:{e}");
                                    }
                                }
                            }
                            subInfo.ArchiverSubInfoExtension.DataEncryptionInfo = subInfo.DataEncryptionInfo;

                            archiverIndexSubInfo.Extension = SerializerHelper.SerializeByDataContractSerializer(subInfo.ArchiverSubInfoExtension);
                            context.FSIndexSubInfos.Add(archiverIndexSubInfo);
                        }
                    }
                    else
                    {
                        logger.Info("Update site master index failed for there is no storage info in contract jobId: {0}", indexDto.JobId);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return id;
        }

        private FSMasterIndex ConvertToArchiverSiteMasterIndex(FSMasterIndexContract contract)
        {
            FSMasterIndex domain = null;
            if (contract != null)
            {
                domain = new FSMasterIndex();
                domain.ArchiverTime = contract.ArchiverTime;
                domain.Id = contract.Id;
                //contract.IndexDeviceId = domain.IndexDeviceId;
                domain.JobId = contract.JobId;
                domain.JobState = contract.JobState;
                domain.ConnectionName = contract.ConnectionName;
                domain.ConnectionId = contract.ConnectionId;
                //contract.StoragePolicyId = domain.StoragePolicyId;
                domain.MergeIndexState = (int)contract.MergeIndexState;
                domain.StorageInfo = contract.StorageInfo;
                domain.BackupFileType = contract.BackupFileType;
                domain.AgentId = contract.AgentId;
                if (contract.Extension != null)
                {
                    domain.Extension = SerializerHelper.SerializeByDataContractSerializer(contract.Extension);
                }
            }
            return domain;
        }
        private FSMasterIndexContract ConvertToArchiverSiteMasterIndexContract(FSMasterIndex domain)
        {
            FSMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new FSMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                //domain.IndexDeviceId = contract.IndexDeviceId;
                contract.JobId = domain.JobId;
                contract.JobState = domain.JobState;
                contract.ConnectionName = domain.ConnectionName;
                contract.ConnectionId = domain.ConnectionId;
                //domain.StoragePolicyId = contract.StoragePolicyId;
                contract.MergeIndexState = (MergeIndexState)domain.MergeIndexState;
                contract.StorageInfo = domain.StorageInfo;
                contract.BackupFileType = domain.BackupFileType;
                contract.AgentId = domain.AgentId;
            }
            return contract;
        }
    }
}
