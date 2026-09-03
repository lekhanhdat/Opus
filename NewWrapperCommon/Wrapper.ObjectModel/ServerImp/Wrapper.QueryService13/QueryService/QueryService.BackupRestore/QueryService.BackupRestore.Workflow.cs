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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        public List<Dictionary<String, object>> TryGetWorkflowInfo(Guid siteId, Guid webId, Guid listId, int itemId, Guid workflowAssociationId, out bool hasRunningInstance)
        {
            List<Dictionary<String, object>> workflowInstances = null;
            string cmdText = @"SELECT Id,InternalState,Modified,Created,Status1 FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND ItemId=@ItemId AND TemplateId=@TemplateId";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ItemId", itemId);
            mQueryWorker.AddParameter("@TemplateId", workflowAssociationId);

            workflowInstances = new List<Dictionary<String, object>>();
            hasRunningInstance = false;
            using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    Guid id = dr.GetGuid(0);
                    Int32 internalState = dr.GetInt32(1);
                    DateTime modified = dr.GetDateTime(2);
                    DateTime created = dr.GetDateTime(3);
                    Int32 status1 = dr.GetInt32(4);

                    if ((2 == status1 || status1 >= 15 && internalState == 2))
                    {
                        hasRunningInstance = true;
                    }
                    Dictionary<String, object> workflowInstance = new Dictionary<String, object>();
                    workflowInstance.Add(@"#Id", id);
                    workflowInstance.Add(@"#InternalState", internalState);
                    workflowInstance.Add(@"#Modified", modified);
                    workflowInstance.Add(@"#Created", created);
                    workflowInstance.Add(@"#Status1", status1);

                    workflowInstances.Add(workflowInstance);
                }
            }

            return workflowInstances;
        }

        #region Workflow


        /// <summary>
        /// 从EventReceiver中删除某个具体的EventReceiver
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="hostId"></param>
        /// <param name="contextCollectionId"></param>
        /// <param name="sequenceNumber"></param>   
        [QueryReview("2012/12/18", "Austin Han")]
        public void DeleteSpecificEventFromEventReceiver(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId, object sequenceNumber)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@HostId", hostId);
            mQueryWorker.AddParameter("@ContextCollectionId", contextCollectionId);
            mQueryWorker.AddParameter("@SequenceNumber", sequenceNumber);
            string commandText = @"DELETE EventReceivers WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND Type=32767 AND ContextCollectionId=@ContextCollectionId AND ContextObjectId IS NULL AND ContextId IS NULL AND ContextType IS NUll AND ContextEventType IS NULL AND SequenceNumber=@SequenceNumber AND Assembly='' AND Class=''";
            mQueryWorker.ExecuteNonQuery(commandText);
        }

        public void InsertTableRow(Hashtable data, string tableName)
        {
            string cmdText = BuildInsertCmdText(data, tableName);

            SetParameters(data);

            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <param name="fieldId"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [QueryReview("Workflow-002")]
        public int UpdateTableNameValuePairForWFInstance(Guid siteId, Guid listId, int itemId, Guid workflowInstanceId, Guid fieldId, int level)
        {
            try
            {
                mQueryWorker.AddParameter("@tp_SiteId", siteId);
                mQueryWorker.AddParameter("@tp_ListId", listId);
                mQueryWorker.AddParameter("@tp_Id", itemId);
                mQueryWorker.AddParameter("@tp_WorkflowInstanceId", workflowInstanceId);
                mQueryWorker.AddParameter("@WorkflowInstanceFieldId", fieldId);
                mQueryWorker.AddParameter("@tp_Level", level);
                mQueryWorker.ExecuteNonQuery("UPDATE NameValuePair SET Value = @tp_WorkflowInstanceId WHERE SiteId=@tp_SiteId AND ListId=@tp_ListId AND FieldId=@WorkflowInstanceFieldId AND ItemId=@tp_Id AND Level=@tp_Level");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, WrapperQueryServiceResource.UpdateTableNameValuePairError, e.ToString());
            }
            return 0;
        }
        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Workflow-004")]
        public void UpdateWorkflowConfiguration(Guid siteId, Guid workflowAssociationId, int configuration)
        {
            string cmdText = "UPDATE WorkflowAssociation SET Configuration=@Configuration WHERE SiteId = @SiteId AND Id=@Id";
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            mQueryWorker.AddParameter("@Configuration", configuration);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public void UpdateWorkflowStatusFieldName(Guid siteId, Guid workflowAssociationId, string internalNameStatusField)
        {
            string cmdText = "UPDATE WorkflowAssociation SET StatusFieldName=@StatusField WHERE SiteId = @SiteId AND Id=@Id";
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            if (string.IsNullOrEmpty(internalNameStatusField))
                mQueryWorker.AddParameter("@StatusField", DBNull.Value);
            else
                mQueryWorker.AddParameter("@StatusField", internalNameStatusField);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Workflow-006")]
        public void UpdateAssociationName(Guid siteId, Guid idworkflowAssociationId, string name)
        {
            string cmdText = "SELECT Name FROM WorkflowAssociation  WITH(NOLOCK) WHERE SiteId = @SiteId AND Id=@Id";
            string fixedName = string.Empty;
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", idworkflowAssociationId);
            if (name.IndexOf('\n') > 0)
            {
                fixedName = name;
            }
            else
            {
                fixedName = (string)mQueryWorker.ExecuteScalar(cmdText);
                int index = fixedName.IndexOf('\n');
                if (index > 0)
                {
                    fixedName = name + fixedName.Substring(index);
                }
                else
                {
                    fixedName = name;
                }
            }
            if (string.IsNullOrEmpty(fixedName))
                return;
            cmdText = "UPDATE WorkflowAssociation SET Name=@Name WHERE Id=@Id";
            mQueryWorker.AddParameter("@Name", fixedName);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Workflow-007")]
        public void UpdateWorkflowAssociationCreatedTime(Guid siteId, Guid workflowAssociationId, DateTime created)
        {
            string cmdText = "UPDATE WorkflowAssociation SET Created=@Created WHERE SiteId = @SiteId AND Id=@Id";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            mQueryWorker.AddParameter("@Created", created);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        public void UpdateWorkflowAssociationAuthor(Guid siteId, Guid workflowAssociationId, int userId)
        {
            string cmdText = "UPDATE WorkflowAssociation SET Author=@userId WHERE SiteId = @SiteId AND Id=@Id";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            mQueryWorker.AddParameter("@userId", userId);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Workflow-007")]
        public void UpdateWorkflowAssociationModifiedTime(Guid siteId, Guid workflowAssociationId, DateTime modified)
        {
            string cmdText = "UPDATE WorkflowAssociation SET Modified=@Modified WHERE SiteId = @SiteId AND Id=@Id";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            mQueryWorker.AddParameter("@Modified", modified);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }
        
        public int UpdateTableRow(Hashtable metadata, List<string> excludeField, Hashtable conditionParam, string tableName, string condition)
        {
            try
            {
                bool hasFieldNeedToUpdate = false;
                const string COLUMN_SET = "#tp_ColumnSet";
                StringBuilder cmdBuilder = new StringBuilder();
                cmdBuilder.Append("UPDATE ");
                cmdBuilder.Append(tableName);
                cmdBuilder.Append(" SET ");

                mQueryWorker.ClearParameters();
                foreach (DictionaryEntry de in conditionParam)
                {
                    string key = de.Key.ToString();
                    if (key[0] != '#')
                        continue;
                    string name = key.Substring(1);
                    object value = de.Value;

                    StringBuilder sqlParamName = new StringBuilder();
                    sqlParamName.Append("@");
                    sqlParamName.Append(name);

                    mQueryWorker.AddParameter(sqlParamName.ToString(), value);
                }

                foreach (DictionaryEntry de in metadata)
                {
                    string key = de.Key.ToString();
                    if (key[0] != '#')
                        continue;
                    if (excludeField.Contains(key.ToLower(CultureInfo.InvariantCulture)))
                        continue;
                    if (key.Equals(COLUMN_SET, StringComparison.OrdinalIgnoreCase))
                        continue;


                    string name = key.Substring(1);
                    object value = de.Value;
                    StringBuilder sqlParamName = new StringBuilder();
                    sqlParamName.Append("@");
                    sqlParamName.Append(name);
                    if (!hasFieldNeedToUpdate)
                    {
                        hasFieldNeedToUpdate = true;
                    }
                    else
                    {
                        cmdBuilder.Append(",");
                    }

                    cmdBuilder.Append(name);
                    cmdBuilder.Append("=@");
                    cmdBuilder.Append(name);
                    mQueryWorker.AddParameter(sqlParamName.ToString(), value);
                }

                cmdBuilder.Append(condition);

                string commandText = cmdBuilder.ToString();
                int affectedRowCount = mQueryWorker.ExecuteNonQuery(commandText);
                return 0;
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取Item上的workflow信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="tempIds"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="itemId"></param>
        /// <param name="listId"></param>
        [QueryReview("2012/05/02", "Kexin Guo")]
        public void GetWorkflowId(List<Guid> tempIds, Guid siteId, Guid webId, int itemId, Guid listId)
        {
            string commandText = @"SELECT Id FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebID AND ListId=@ListId AND ItemId=@ItemId ORDER BY Created";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ItemId", itemId);
                mQueryWorker.AddParameter("@ListId", listId);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            tempIds.Add(sdr.GetGuid(0));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Workflow-008")]
        public void RecalculateRunningInstanceCount(Guid siteId, Guid webId, Guid listId, Guid workflowAssociationId)
        {
            string cmdText = @"SELECT COUNT(*) FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND TemplateId=@TemplateId AND ((InternalState & 2)<>0)";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@TemplateId", workflowAssociationId);
            int runningInstanceCount = (int)mQueryWorker.ExecuteScalar(cmdText);

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", workflowAssociationId);
            mQueryWorker.AddParameter("@Count", runningInstanceCount);
            cmdText = "UPDATE WorkflowAssociation SET InstanceCount=@Count WHERE Id=@Id";
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        [QueryReview("Workflow-001")]
        public void UpdateWorkflowTemplateFileDocFlags(Guid siteId, Guid parentId, Guid uniqueId, byte level)
        {
            string cmdText = @"Update Alldocs Set DocFlags=DocFlags|0x00080000
Where SiteId=@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And Level=@Level;";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", uniqueId);
            mQueryWorker.AddParameter("@Level", level);
            mQueryWorker.ExecuteNonQuery(cmdText);
            mQueryWorker.ClearParameters();
        }

        #endregion

        /// <summary>
        /// 通过Workflow id获取此item的workflow的status.
        /// workflowId 一般是AllUserData表的workflowstatus的值，需要特殊处理
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public AveWorkflowStatus GetWorkflowStatus(string workflowId)
        {
            Guid id = Guid.Empty;
            try
            {
                id = new Guid(Encoding.Unicode.GetBytes(workflowId));
            }
            catch (Exception ex)
            {
                logger.Info("workflow Id is not unicode format.WorkflowId:{0}, exception:{1}", workflowId, ex.ToString());
                try
                {
                    id = new Guid(Convert.FromBase64String(workflowId));
                }
                catch (Exception ex1)
                {
                    logger.Info("workflow Id is not string format.WorkflowId:{0}, exception:{1}", workflowId, ex1.ToString());
                    return AveWorkflowStatus.NotStarted;
                }
            }
            return GetWorkflowStatus(id);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public AveWorkflowStatus GetWorkflowStatus(Guid workflowId)
        {
            var cmdText = @"SELECT Status1 FROM Workflow WITH(NOLOCK) WHERE Id=@Id";
            mQueryWorker.AddParameter("@Id", workflowId);
            var status = mQueryWorker.ExecuteScalar(cmdText);
            if (status == null)
            {
                return AveWorkflowStatus.NotStarted;
            }
            return (AveWorkflowStatus)status;
        }

        [QueryReview("2012/05/09", "Fengfu Zhang")]
        private string BuildInsertCmdText(Hashtable ht, string tableName)
        {
            StringBuilder cmdText = new StringBuilder();

            cmdText.Append("INSERT INTO ");
            cmdText.Append(tableName);
            cmdText.Append(" (");

            bool isFirstItem = true;

            string key;
            foreach (string tempKey in ht.Keys)
            {
                if (tempKey.StartsWith("#", StringComparison.Ordinal))
                {
                    key = tempKey.Substring(1);

                    if (isFirstItem)
                    {
                        isFirstItem = false;
                        cmdText.Append(key);
                    }
                    else
                    {
                        cmdText.Append(", ");
                        cmdText.Append(key);
                    }
                }
            }

            cmdText.Append(") VALUES (");

            isFirstItem = true;

            foreach (DictionaryEntry entry in ht)
            {
                string tempKey = (string)entry.Key;

                if (tempKey.StartsWith("#", StringComparison.Ordinal))
                {
                    key = tempKey.Substring(1);

                    if (isFirstItem)
                    {
                        isFirstItem = false;
                        if (entry.Value == null)
                        {
                            cmdText.Append("NULL");
                        }
                        else
                        {
                            cmdText.Append("@");
                            cmdText.Append(key);
                        }
                    }
                    else
                    {
                        if (entry.Value == null)
                        {
                            cmdText.Append(", NULL");
                        }
                        else
                        {
                            cmdText.Append(", @");
                            cmdText.Append(key);
                        }
                    }
                }
            }

            cmdText.Append(")");
            return cmdText.ToString();
        }

        [QueryReview("2012/05/09", "Fengfu Zhang")]
        private void SetParameters(Hashtable ht)
        {
            mQueryWorker.ClearParameters();

            foreach (DictionaryEntry entry in ht)
            {
                string key = (string)entry.Key;
                object value = entry.Value;
                if (value == null)
                    continue;
                if (key.StartsWith("#", StringComparison.Ordinal))
                {
                    key = key.Substring(1);

                    mQueryWorker.AddParameter("@" + key, value);
                }
            }
        }

        /// <summary>
        /// 获取Item上的workflowAssociation信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="tempIds"></param>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="itemId"></param>
        /// <param name="listId"></param>
        [QueryReview("2013/04/05", "Guanqun Zhou")]
        public void GetWorkflowAssociationId(List<Guid> tempIds, Guid siteId, Guid webId, Guid listId)
        {
            string commandText = @"SELECT Distinct([BaseId]) FROM [WorkflowAssociation] WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebID AND ListId=@ListId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sdr.HasRows)
                    {
                        while (sdr.Read())
                        {
                            tempIds.Add(sdr.GetGuid(0));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取特定Workflow的所有信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [QueryReview("2012/05/21", "Guoqin Sun")]
        public IAveQueryDataReader BackupInstance(Guid id)
        {
            string commandText = "SELECT * FROM Workflow WHERE Id=@Id ";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@Id", id);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
                //using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                //{
                //    if (sdr.Read())
                //    {
                //        SetPropsFromDataReader(sdr, 0, null, 0, properties);
                //        return true;
                //    }
                //}
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        
        /// <summary>
        /// 备份workflow instance关联的scheduled work item信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupScheduledWorkItem(Guid siteId, Guid instanceId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WorkflowInstanceId", instanceId);
                string commandText = @"SELECT * FROM ScheduledWorkItems WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@WorkflowInstanceId";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        
        public void BackupInstanceSelf(Guid siteId, Guid webId, Guid id, Hashtable properties, string customFieldProfix)
        {
            string taskListTitle = string.Empty;
            string histListTitle = string.Empty;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@Id", id);
                string commandText = "SELECT TaskListTitle,HistoryListTitle,cast(cast(TaskListId as VARBINARY) as UNIQUEIDENTIFIER),cast(cast(HistoryListId as VARBINARY) as UNIQUEIDENTIFIER),Name,BaseId FROM WorkflowAssociation WITH(NOLOCK) WHERE Id=@Id ";
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sdr.Read())
                    {
                        if (!sdr.IsDBNull(2))
                        {
                            properties.Add(customFieldProfix + "TaskListId", sdr.GetGuid(2));
                            taskListTitle = string.Empty;
                        }
                        else
                        {
                            if (!sdr.IsDBNull(0))
                                taskListTitle = sdr.GetString(0);
                        }
                        if (!sdr.IsDBNull(3))
                        {
                            properties.Add(customFieldProfix + "HistoryListId", sdr.GetGuid(3));
                            histListTitle = string.Empty;
                        }
                        else
                        {
                            if (!sdr.IsDBNull(1))
                                histListTitle = sdr.GetString(1);
                        }
                        if (!sdr.IsDBNull(4))
                        {
                            string name = sdr.GetString(4).Split(new char[] { '\n' })[0];
                            properties.Add(customFieldProfix + "ParentAssociationName", name);
                        }
                        if (!sdr.IsDBNull(5))
                        {
                            properties.Add(customFieldProfix + "ParentAssociationBaseId", sdr.GetGuid(5));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(taskListTitle))
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@Title", taskListTitle);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    commandText = "SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_WebId=@WebID AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
                    using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                    {
                        if (sdr.Read())
                            properties.Add(customFieldProfix + "TaskListId", sdr.GetGuid(0));
                    }
                }

                if (!string.IsNullOrEmpty(histListTitle))
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@Title", histListTitle);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    commandText = "SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId tp_WebId=@WebID AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
                    using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                    {
                        if (sdr.Read())
                            properties.Add(customFieldProfix + "HistoryListId", sdr.GetGuid(0));
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
    }
}
