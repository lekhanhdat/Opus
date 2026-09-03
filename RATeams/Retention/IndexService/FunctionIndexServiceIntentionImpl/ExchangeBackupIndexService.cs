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


namespace Office365GroupRetention
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;
    #endregion using directives

    public class ExchangeBackupIndexService
        : ExchangeFunctionIndexServiceBase
        , IExchangeBackupIndexService
    {
        public List<GroupAgentIndex> GetAgentIndex(String backupType)
        {
            return this.AgentIndexService.GetAgentIndex(backupType);
        }

        public void DeleteAgentIndex(String backupType)
        {
            this.AgentIndexService.DeleteAgentIndex(backupType);
        }

        public void InsertSiteMasterIndex(GroupMasterIndex siteMaster)
        {
            this.SiteMasterIndexService.InsertSiteMasterIndex(siteMaster);
        }

        public void UpdateSiteMasterIndex(GroupMasterIndex siteMaster)
        {
            this.SiteMasterIndexService.UpdateSiteMasterIndex(siteMaster);
        }

        public void UpdateAgentIndex(List<GroupAgentIndex> agentList)
        {
            this.AgentIndexService.UpdateAgentIndex(agentList);
        }

        public void CreateTableDataMd5Index()
        {
            this.DataMd5IndexService.CreateTableDataMd5();
        }

        public void InsertDataMd5Index(GroupDataMd5Index dataMd5)
        {
            this.DataMd5IndexService.InsertDataMd5Index(dataMd5);
        }

        public void DeleteDataMd5Index(String jobId)
        {
            this.DataMd5IndexService.DeleteDataMd5IndexByJobId(jobId);
        }

        public List<GroupDataMd5Index> GetDataMd5(string jobId)
        {
            return this.DataMd5IndexService.GetDataMd5(jobId);
        }

        public GroupDataMd5Index GetCurrentDataMd5(string jobId, string dataName, string dataObjectId)
        {
            return this.DataMd5IndexService.GetCurrentDataMd5(jobId, dataName, dataObjectId);
        }

        public void AddColumn(string columnName, string declaredType)
        {
            this.DataMd5IndexService.AddColumn(columnName, declaredType);
        }

        public void UpdateDataMd5Index(GroupDataMd5Index dataMd5)
        {
            this.DataMd5IndexService.UpdateDataMd5Index(dataMd5);
        }

        public void Insert(List<GroupBasicIndex> indexes)
        {
            this.ContainerItemIndexService.Insert(indexes);
        }

        public void UpdateFormerJobIdToCurrentJobId(String jobId)
        {
            this.ContainerItemIndexService.UpdateFormerJobIdToCurrentJobId(jobId);
        }

        public void DeleteDeleteTypeData()
        {
            this.ContainerItemIndexService.DeleteDeleteTypeData();
        }

        //public void UpdateDuplicatedData()
        //{
        //    this.ContainerItemIndexService.UpdateDuplicatedData();
        //}

        public void UpdateHasAttachColumn()
        {
            this.ContainerItemIndexService.UpdateHasAttachColumn();
        }

        public void CreateIndexContainerAndItemIndex()
        {
            this.ContainerItemIndexService.CreateIndexContainerAndItemIndex();
        }

        public Int32 GetRepeatContainerCount(String jobId)
        {
            return Convert.ToInt32(this.ContainerItemIndexService.GetRepeatContainerCount(jobId));
        }

        public Int32 GetIndexTotalCount(String jobId)
        {
            return Convert.ToInt32(this.ContainerItemIndexService.GetIndexTotalCount(jobId));
        }

        public List<GroupBasicIndex> GetIndexs(String jobId, Int32 offset, Int32 length)
        {
            return this.ContainerItemIndexService.GetNeedFiles(jobId, offset, length);
        }

        public List<GroupBasicIndex> GetMetaDataIndexs()
        {
            return this.ContainerItemIndexService.GetMetaDataIndexs();
        }

        public List<GroupBasicIndex> GetContentDataIndexs()
        {
            return this.ContainerItemIndexService.GetContentDataIndexs();
        }

        public Int32 GetContainerIndexTotalCount(String jobId)
        {
            return Convert.ToInt32(this.ContainerItemIndexService.GetContainerIndexTotalCount(jobId));
        }

        public Int32 GetItemIndexTotalCount(String jobId)
        {
            return Convert.ToInt32(this.ContainerItemIndexService.GetItemIndexTotalCount(jobId));
        }

        public Int64 GetItemTotalSize(String jobId)
        {
            return Convert.ToInt64(this.ContainerItemIndexService.GetItemTotalSize(jobId));
        }

        public Boolean NeedResendBackupData(String JobId)
        {
            return this.SiteMasterIndexService.HasSpecifyJobInfo(JobId);
        }

        public void DeleteIndexInHeadAndBodyByJobId(String jobId)
        {
            this.ContainerItemIndexService.DeleteContainerAndItemIndexByJobId(jobId);
        }

        public void DeleteSiteMisterIndexByJobId(String jobId)
        {
            this.SiteMasterIndexService.DeleteSiteMasterIndexByJobId(jobId);
        }

        public void ProcessColumnUpgrate()
        {
            this.ContainerItemIndexService.ProcessColumnUpgrate();
        }

        public int GetContainerCount() => ContainerItemIndexService.GetContainerCount();

        public bool HasContainter(string pathMd5) => ContainerItemIndexService.HasContainter(pathMd5);
    }
}