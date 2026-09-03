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
    #region directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class ExchangeAgentIndexService
        : ExchangeTableIndexServiceBase
        , IExchangeAgentIndexService
    {
        static readonly string selectAgentIndexCount = "select count(*) from " + IndexConstants.TableNameExchangeAgent
            + " where COL_INDEX_TYPE = @COL_INDEX_TYPE ";

        static readonly string updateAgentIndexBackupMessage = "update " + IndexConstants.TableNameExchangeAgent
          + " set COL_TIME_STAMP = @COL_TIME_STAMP,COL_LAST_BACKUP_CONTENT = @COL_LAST_BACKUP_CONTENT,COL_STORAGEINFO = @COL_STORAGEINFO"
           + " where COL_INDEX_TYPE = @COL_INDEX_TYPE ";

        static readonly string selectAgentIndexAll = "select * from " + IndexConstants.TableNameExchangeAgent
            + " where COL_INDEX_TYPE = @COL_INDEX_TYPE ";

        public void UpdateAgentIndex(List<GroupAgentIndex> agentList)
        {
            var parameters = new Dictionary<String, Object>();
            foreach (GroupAgentIndex agentIndex in agentList)
            {
                parameters.Clear();
                parameters["@COL_LAST_BACKUP_CONTENT"] = agentIndex.LastBackUpContent;
                parameters["@COL_INDEX_TYPE"] = agentIndex.IndexType;
                parameters["@COL_TIME_STAMP"] = agentIndex.TimeStamp;
                parameters["@COL_STORAGEINFO"] = agentIndex.StorageInfo;
                Object obj = this.IndexProcessor.ExecuteScalar(selectAgentIndexCount, parameters);
                Int32 count;
                if (!Int32.TryParse(obj.ToString(), out count))
                    count = 0;
                if (count == 0)
                {
                    this.IndexProcessor.Insert<GroupAgentIndex>(agentIndex);
                }
                else
                {
                    this.IndexProcessor.Execute(updateAgentIndexBackupMessage, parameters);
                }
            }
        }

        public List<GroupAgentIndex> GetAgentIndex(String backupType)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_INDEX_TYPE"] = backupType + ".idx";
            var agentIndexList = this.IndexProcessor.ExecuteQuery<GroupAgentIndex>(selectAgentIndexAll, parameters);
            return agentIndexList;
        }

        public void DeleteAgentIndex(String backupType)
        {
            var removeCommand = "delete from " + IndexConstants.TableNameExchangeAgent;
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty (backupType ))
            {
                removeCommand = removeCommand + " where COL_INDEX_TYPE = @COL_INDEX_TYPE ";
                parameters["@COL_INDEX_TYPE"] = backupType + ".idx";
            }
            this.IndexProcessor.Execute(removeCommand, parameters);
        }
    }
}