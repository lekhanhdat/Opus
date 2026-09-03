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

using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.Metadata;
using ExchangeCommonWrapper;
using ExchangeUtility.Graph;
using Job.ModernManagement.Report;
using Office365GroupBackup;
using RAArchiverCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Office365GroupRestore
{
    public class TaskRestoreHelperBatch : BaseRestoreHelperBatch
    {
        public TaskRestoreHelperBatch(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {

        }
        private Dictionary<string, string> allTaskIdDic = new Dictionary<string, string>();
        private Office365PlannerTaskEntity taskProperties;
        private string targetPlanId = string.Empty;
        private string targetTaskId = string.Empty;
        private string targetBucketId = null;
        protected override void InitReport(MetadataEntity baseEntity, String sourceUrlDic)
        {
            base.InitReport(baseEntity, sourceUrlDic);
            ReportDto.Type = ReportNodeHeader.Task;
        }

        protected override bool NeedRestore() => !string.IsNullOrEmpty(RestoreConfig.CurrentRestoreMailbox);

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            var restoreData = dataCollection.First().RestoreData;
            var entity = restoreData.Metadata;
            var sourceUrlPath = restoreData.SourceUrlPath;
            try
            {
                this.InitReport(entity, sourceUrlPath);

                logger.Info($"Start to restore {ReportDto.Type}, name:{ReportDto.Name}, path: {ReportDto.Path}, id:{entity.Id}");

                if (null == PlannerService) throw new Exception("Unsupport AuthType");

                GetTaskProperties(restoreData);

                GetTargetPlanId();

                CreateBucket(targetPlanId);

                var needCreateTask = NeedNewCreateTask(out targetTaskId);


                if (!needCreateTask && Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    AddOptionReport(needCreateTask, entity.Title);
                    return;
                }

                var needAddTaskComment = false;
                String newConversationThreadId = null;
                try
                {
                    needAddTaskComment = RestorePlannerTaskComments(needCreateTask, out newConversationThreadId);
                }
                catch (Exception e)
                {
                    logger.Warn($"Failed to restore task comment. error:{e}");
                }

                var isNewTask = CreateTask(needCreateTask, targetTaskId);

                var taskInfoTuple = BatchGetNecessaryInformation();

                var needUpdatePlannerTask = UpdatePlannerTask(isNewTask, needAddTaskComment, taskInfoTuple.Item1, newConversationThreadId);

                var needUpdatePlannerTaskDetails = UpdatePlannerTaskDetails(isNewTask, taskInfoTuple.Item2, taskInfoTuple.Item3);

                AddOptionReport(isNewTask, entity.Title);

                if (RestoreConfig.NeedRecordTaskAttachmentsLink) RecordAttachmentLink(taskProperties?.DetailProperties?.References);
            }
            catch (GraphAPIException ex)
            {
                logger.Info($"Failed to restore {ReportDto.Type}, path: {ReportDto.Path}, error:{ex}");
                ReportDto.Status = ReportStatus.Failed;
                ReportDto.ErrorMessage = ErrorCodeConverter.GraphAPIErrorCodeConverter(ex, I18NDataCollector);
            }
            catch (Exception ex)
            {
                logger.Info($"Failed to restore {ReportDto.Type}, path: {ReportDto.Path}, error:{ex}");

                if (ex.Message.StartsWith("Unsupport AuthType"))
                {
                    logger.Error("Can not use AppToken to restore Planner data.{0}", ex);
                    ReportDto.Status = ReportStatus.Skipped;
                    ReportDto.ErrorMessage = "Agent.Office365Group.RestorePlannerFailedWithAppToken_009DDBA9-4786-4B36-87DF-C6D52E937E45";
                }
                else
                {
                    ReportDto.ErrorMessage = ex.Message;
                    ReportDto.Status = ReportStatus.Failed;
                    logger.Error("An error occurred while restore planner task {0}. Message {1}. ", entity.Title, ex.ToString());
                }
            }
            finally
            {
                Report.AddRestoreReport(ReportDto);
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
            }
        }

        private void GetTaskProperties(ExchangeRestoreDataForBatch restoreData)
        {
            taskProperties = restoreData.TryGetMetadata<Office365PlannerTaskEntity>(AveMetadataType.ExchangePlannerTask);
            if (taskProperties.BasicProperties.LabelDictionary == null)
            {
                taskProperties.BasicProperties.LabelDictionary = new Dictionary<string, bool>()
                {
                    {"Category1",taskProperties.BasicProperties.Labels.Label1 },
                    {"Category2",taskProperties.BasicProperties.Labels.Label2 },
                    {"Category3",taskProperties.BasicProperties.Labels.Label3 },
                    {"Category4",taskProperties.BasicProperties.Labels.Label4 },
                    {"Category5",taskProperties.BasicProperties.Labels.Label5 },
                    {"Category6",taskProperties.BasicProperties.Labels.Label6 },
                    {"Category7",taskProperties.BasicProperties.Labels.Label7 },
                    {"Category8",taskProperties.BasicProperties.Labels.Label8 },
                    {"Category9",taskProperties.BasicProperties.Labels.Label9 },
                    {"Category10",taskProperties.BasicProperties.Labels.Label10 },
                    {"Category11",taskProperties.BasicProperties.Labels.Label11 },
                    {"Category12",taskProperties.BasicProperties.Labels.Label12 },
                    {"Category13",taskProperties.BasicProperties.Labels.Label13 },
                    {"Category14",taskProperties.BasicProperties.Labels.Label14 },
                    {"Category15",taskProperties.BasicProperties.Labels.Label15 },
                    {"Category16",taskProperties.BasicProperties.Labels.Label16 },
                    {"Category17",taskProperties.BasicProperties.Labels.Label17 },
                    {"Category18",taskProperties.BasicProperties.Labels.Label18 },
                    {"Category19",taskProperties.BasicProperties.Labels.Label19 },
                    {"Category20",taskProperties.BasicProperties.Labels.Label20 },
                    {"Category21",taskProperties.BasicProperties.Labels.Label21 },
                    {"Category22",taskProperties.BasicProperties.Labels.Label22 },
                    {"Category23",taskProperties.BasicProperties.Labels.Label23 },
                    {"Category24",taskProperties.BasicProperties.Labels.Label24 },
                    {"Category25",taskProperties.BasicProperties.Labels.Label25 },
                };
            }
        }

        private void AddOptionReport(bool isNewTask, string title)
        {
            if (!isNewTask)
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    ReportDto.Option = RestoreOption.Skipped.GetEnumDescription();
                    ReportDto.ErrorMessage = "Agent.Office365Group.ExistTaskSkipped_9044B8D0-3E21-47AD-8C81-4433D7AE585E";
                    ReportDto.Status = ReportStatus.Skipped;
                    logger.Info("The Task {0} is skipped because it already exist in destination.", title);
                }
                else
                {
                    ReportDto.Option = RestoreOption.Overwritten.GetEnumDescription();
                    ReportDto.Status = ReportStatus.Success;
                    logger.Info("The Task {0} is Overwrited.", title);
                }
            }
        }

        private void RecordAttachmentLink(List<TaskReference> references)
        {
            if (null == references) return;
            var filterResult = references.Select(AttachmentLinkselector).Where(AttachmentLinkFilter);
            TaskAttachmentLinkCollector.AddRang(filterResult);
        }
        private string AttachmentLinkselector(ExchangeCommonWrapper.TaskReference reference)
        {
            var temp = Uri.UnescapeDataString(Uri.UnescapeDataString(reference.ReferencesId));
            var r = System.Text.RegularExpressions.Regex.Match(temp, @".+/sites/.+/Shared Documents/.+(?=\?)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return r.Success ? r.Value : string.Empty;
        }
        private bool AttachmentLinkFilter(string referencesId)
        {
            //return referencesId.Contains(ExchangeConstants.SharedDocuments);
            return !string.IsNullOrEmpty(referencesId);
        }


        private bool GetTargetPlanId()
        {
            var oldPlanId = taskProperties.BasicProperties.PlanId;
            var canGetPlanId = EntityIdDic.TryGetValue(oldPlanId, out targetPlanId);
            logger.Info("[TASK]:Can read targetPlanId : {0}, TargetPlanId: {1}", canGetPlanId, targetPlanId);
            if (!canGetPlanId) throw new Exception("Agent.Office365Group.UnableReadTargetPlanId_62C6BF1C-E589-473D-B72F-A2648B5BE8F1");
            return !oldPlanId.Equals(targetPlanId);
        }

        #region bucket
        private bool CreateBucket(string targetPlanId)
        {
            //处理空值的情况
            if (string.IsNullOrEmpty(taskProperties?.BucketProperties?.Id))
            {
                targetBucketId = null;
                logger.Info("The original bucket id is null, so no need to create a bucket.");
                return false;
            }

            var bucketProperties = taskProperties.BucketProperties;
            targetBucketId = bucketProperties.Id;
            //处理已存在mapping 关系的情况
            var isCreated = BucketIdDic.ContainsKey(bucketProperties.Id);
            if (isCreated)
            {
                targetBucketId = BucketIdDic[bucketProperties.Id];
                return false;
            }

            var needCreate = false;
            var bucketInfo = _UnmatchBuckets.FindLast(bucket => bucket.OId == bucketProperties.Id);
            if (bucketInfo != null)
            {
                if (bucketInfo.CanGetByName)
                {
                    targetBucketId = bucketInfo.NId;
                }
                needCreate = !bucketInfo.CanGetByName;
                logger.Info("Find bucket [{0},{1}] :{2}", bucketInfo.Name, bucketInfo.OId, !needCreate);
            }
            else { UpdateBucktet(targetBucketId); }

            if (needCreate)
            {
                targetBucketId = PlannerService.CreatePlannerBucket(bucketProperties, targetPlanId, bucketInfo.OrderHint);
                logger.Info("Create bucket [{0},{1}]", bucketProperties.Name, targetBucketId);
            }
            BucketIdDic.TryAdd(bucketProperties.Id, targetBucketId);
            return needCreate;
        }
        public void UpdateBucktet(String bucketId)
        {
            if (String.IsNullOrEmpty(bucketId)) return;
            if (Config.ContentConflictResolution == EOConflictResolutionType.Skip) return;
            if (_NeedUpdatePlanBuckets.TryGetValue(bucketId, out Office365PlannerBucketProperties existBucket))
            {
                var buckteDto = new NewIdDto()
                {
                    NewId = existBucket.Id,
                    OdataEtag = existBucket.OdataEtag,
                };
                PlannerService.UpdatePlannerBucket(buckteDto, existBucket.Name);//此时 existBucket.Name 为源端name
                _NeedUpdatePlanBuckets.Remove(bucketId);
            }
        }
        #endregion

        #region CreateTask
        private bool NeedNewCreateTask(out string taskId)
        {
            if (Config.RestoreType == EORestoreType.OutOfPlace)
            {
                var oldTaskName = taskProperties.BasicProperties.Title;
                taskId = _AllTasks.FirstOrDefault(q => q.Value == oldTaskName).Key;
                logger.Info("Restore Type: OutOfPlace, TaskName: {0}, TaskId: {1}", oldTaskName, taskId);
                return string.IsNullOrEmpty(taskId);
            }
            else
            {
                var taskName = string.Empty;
                var oldTaskId = taskProperties.BasicProperties.Id;
                taskId = oldTaskId;
                logger.Info("Restore Type: InPlace, TaskId: {0}", taskId);
                return !_AllTasks.TryGetValue(oldTaskId, out taskName);
            }
        }

        private bool CreateTask(Boolean needCreateTask, String outTaskId)
        {
            targetTaskId = ToCreateTask(needCreateTask, outTaskId, targetPlanId);
            logger.Info("[Task]:TargetTaskId : {0}", targetTaskId);
            return needCreateTask;
        }

        private String ToCreateTask(bool needCreateTask, string targetTaskId, string targetPlanId)
        {
            logger.Info("Need create task:{0} ", needCreateTask);
            var basicProperties = taskProperties.BasicProperties;
            if (needCreateTask)
            {
                var createTask = PlannerService.CreatePlannerTask(basicProperties, targetPlanId, targetBucketId);
                return createTask.NewId;
            }
            logger.Warn("This task {0} [{1}] was not created because it exists. ", basicProperties.Title, targetTaskId);
            return targetTaskId;
        }
        #endregion

        #region UpdateTaskAndTaskDetails
        private bool UpdatePlannerTask(bool needCreatePlannerTask, bool needAddTaskComment, GraphPlannerTask newTaskObj, string newConversationThreadId)
        {
            var basicProperties = taskProperties.BasicProperties;
            var needUpdatePlannerTask = NeedUpdateTask(needCreatePlannerTask, needAddTaskComment, newTaskObj, basicProperties.OdataEtag);
            ToUpdatePlannerTask(basicProperties, newTaskObj, needUpdatePlannerTask, newConversationThreadId);
            logger.Info("[TASK]:NeedUpdateTask: {0}", needUpdatePlannerTask);
            return needUpdatePlannerTask;
        }

        private bool UpdatePlannerTaskDetails(bool needCreatePlannerTask, GraphPlannerTaskDetails newTaskDetailsObj, string siteUrl)
        {
            var detailsProperties = taskProperties.DetailProperties;
            var needUpdatePlannerTaskDetails = NeedUpdateTaskDetails(needCreatePlannerTask, newTaskDetailsObj, detailsProperties.OdataEtag);
            ToUpdatePlannerTaskDetails(detailsProperties, newTaskDetailsObj, needUpdatePlannerTaskDetails, siteUrl);
            logger.Info("[TASK]:NeedUpdateTaskDetails: {0}", needUpdatePlannerTaskDetails);
            return needUpdatePlannerTaskDetails;
        }

        private bool NeedUpdateTask(bool needCreateTask, bool needAddTaskComment, GraphPlannerTask newTaskObj, string taskEtag)
        {
            if (needCreateTask)
            {
                return needCreateTask;
            }
            else
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    return needCreateTask;
                }
                else
                {
                    return (!taskEtag.Equals(newTaskObj.OdataEtag)) || needAddTaskComment;
                }
            }

        }

        private bool NeedUpdateTaskDetails(bool needCreateTask, GraphPlannerTaskDetails newTaskDetailsObj, string taskDetailsEtag)
        {
            if (needCreateTask)
            {
                return needCreateTask;
            }
            else
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    return needCreateTask;
                }
                else
                {
                    return !taskDetailsEtag.Equals(newTaskDetailsObj.OdataEtag);
                }
            }
        }

        private void ToUpdatePlannerTask(Office365PlannerTaskBasicProperties basicProperties, GraphPlannerTask newTaskObj, bool needUpdateTask, string newConversationThreadId = "")
        {
            if (needUpdateTask)
            {
                var updateTaskObj = basicProperties.ToUpdateObj(targetBucketId, newConversationThreadId);
                newTaskObj.Assignments.Keys.ForEach(key => MergeDic(updateTaskObj.Assignments, key));
                //for oop mapping
                if (Config.RestoreType == EORestoreType.OutOfPlace || _SpecialTeamAdapter.IsSpecialTeam)
                {
                    var mappedUsers = basicProperties.AssignmentNames != null ? DoMapping(basicProperties.AssignmentNames) : new Dictionary<string, string>();
                    updateTaskObj.Assignments = mappedUsers.Values.ToDictionary(key => key, value =>
                    new UTAssignmentValue()
                    {
                        OdataType = "#microsoft.graph.plannerAssignment",
                        OrderHint = " !"
                    });
                }
                else
                {
                    var skipUser = updateTaskObj.Assignments.Where(u => !Guid.TryParse(u.Key, out Guid guid)).ToList();
                    foreach (var u in skipUser)
                    {
                        logger.Info("Remove invalid assignee [{0}].", u.Key);
                        updateTaskObj.Assignments.Remove(u.Key);
                    }
                }
                try
                {
                    PlannerService.UpdateTask(updateTaskObj, new NewIdDto { NewId = newTaskObj.Id, OdataEtag = newTaskObj.OdataEtag });
                }
                catch (GraphAPIException ex) when (ex.Error.Message.Contains("The assignee id is invalid"))
                {
                    logger.Warn("Update task assignment has error. Assginee ids: {0}. Error: {1}.", string.Join(',', updateTaskObj.Assignments.Keys), ex);
                }
                catch (GraphAPIException ex) when (ex.Error.Message.Contains("Referenced User") && ex.Error.Message.Contains("is not found"))
                {
                    ReportDto.Status = ReportStatus.Skipped;
                    ReportDto.ErrorMessage = "Agent.Office365Group.AssignUserNotFound_2317A0B1-822D-01DE-9522-07B7A84429A2";
                    logger.Error("Task assignment not found, {0}.", ex);
                    updateTaskObj.Assignments = new Dictionary<string, UTAssignmentValue>();
                    PlannerService.UpdateTask(updateTaskObj, new NewIdDto { NewId = newTaskObj.Id, OdataEtag = newTaskObj.OdataEtag });
                }
            }
        }

        private void ToUpdatePlannerTaskDetails(Office365PlannerTaskDetailsProperties detailProperties, GraphPlannerTaskDetails newTaskDetailsObj, bool needUpdatePlannerTaskDetails, String siteWebUrl)
        {
            if (needUpdatePlannerTaskDetails)
            {
                var updateTaskDetailsObj = detailProperties.ToUpdateObj(siteWebUrl);
                newTaskDetailsObj.Checklist.Keys.ForEach(key => MergeDic(updateTaskDetailsObj.Checklist, key));
                newTaskDetailsObj.References.Keys.ForEach(key => MergeDic(updateTaskDetailsObj.References, key));
                PlannerService.UpdateTaskDetails(updateTaskDetailsObj, new NewIdDto { NewId = newTaskDetailsObj.Id, OdataEtag = newTaskDetailsObj.OdataEtag });
            }
        }

        #endregion

        #region TaskComment
        private Boolean RestorePlannerTaskComments(Boolean isNewTask, out String newConversationThreadId)
        {
            var oldConversationThreadId = taskProperties.BasicProperties.ConversationThreadId;
            newConversationThreadId = oldConversationThreadId;
            if (string.IsNullOrWhiteSpace(oldConversationThreadId)) return false;

            var oldConversationExsit = PlannerServiceForDelegate.CheckConversationThreadExist(_GroupId, oldConversationThreadId);
            var needAddTaskComment = NeedAddTaskComments(isNewTask, oldConversationExsit);
            (newConversationThreadId, var messageId) = GetNewConversationThreadId(needAddTaskComment);
            AddTaskComments(needAddTaskComment, newConversationThreadId, messageId);
            return needAddTaskComment;
        }

        private bool NeedAddTaskComments(bool isNewTask, bool oldConversationExsit)
        {
            if (PlannerServiceForDelegate is ExchangePlannerAppService exchangePlannerAppService
                && !exchangePlannerAppService.IsCustomApp
                && !exchangePlannerAppService.ContainsRoles(new List<string> { "Mail.Send", "Mail.Read" }))
            {
                logger.Info("Don't need to add comments beaceuse of restore by app profile.");
                return false;
            }
            if (isNewTask)
            {
                if (oldConversationExsit) return false;
            }
            else
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip) return false;
            }
            var commentProperties = taskProperties.CommentProperties;
            return commentProperties.Comments?.Count > 0;
        }

        private (string, string) GetNewConversationThreadId(bool needAddTaskComment)
        {
            logger.Info("[TASK]:NeedAddTaskComment: {0}", needAddTaskComment);
            if (needAddTaskComment)
            {
                var createInfo = new CreateConversationThreadInfo() { GroupId = _GroupId, GroupMail = RestoreConfig.CurrentRestoreMailbox, TaskProperties = taskProperties };
                if (!PlannerServiceForDelegate.CreateConversationThread(createInfo, out var result))
                {
                    throw new Exception("Agent.Office365Group.SomeTaskCommentsNotRestored_098A52BA-D574-40DD-9A70-BE6B2EB2303C");
                }
                logger.Info("TargetConversationThreadId: {0}.", result.ConversationThreadId);
                return result;
            }
            return (string.Empty, string.Empty);
        }

        private void AddTaskComments(bool needAddTaskComment, string newConversationThreadId, string messageId)
        {
            if (needAddTaskComment)
            {
                logger.Info("[Task]: Add task Comment.");
                var addCommentsInfo = new AddPlannerTaskCommentsInfo()
                {
                    TaskComments = taskProperties.CommentProperties.Comments,
                    GroupId = _GroupId,
                    GroupMail = RestoreConfig.CurrentRestoreMailbox,
                    ConversationThreadId = newConversationThreadId,
                    MessageId = messageId
                };
                var isSuccess = PlannerServiceForDelegate.BatchAddPlannerTaskComments(addCommentsInfo);
                if (!isSuccess) throw new Exception("Agent.Office365Group.SomeTaskCommentsNotRestored_098A52BA-D574-40DD-9A70-BE6B2EB2303C");
            }
        }
        #endregion
        /// <summary>
        /// 解析返回的批量responseItem, 返回 taskObj, taskDetailsObj, siteWebUrl, hasOldConversation 
        /// </summary>
        /// <returns>
        /// Item1 : GraphPlannerTask
        /// Item2 : GraphPlannerTaskDetails
        /// Item3 : SiteWebUrl
        /// Item4 : hasOldConversation
        /// </returns>
        private Tuple<GraphPlannerTask, GraphPlannerTaskDetails, String> BatchGetNecessaryInformation()
        {
            var responseItemDic = PlannerService.BatchSelectInfo(targetTaskId, _GroupId);
            Exception exception = null;
            foreach (var responseItem in responseItemDic.Values)
            {
                if (!responseItem.IsSuccessStatusCode)
                {
                    switch (responseItem.Id)
                    {
                        case SimpleItemId.GetGroupSite: { logger.Warn("Can't update group site webUrl because group site is not ready yet."); break; }
                        case SimpleItemId.GetConversationThread: { logger.Warn("Can't find old available conversation"); break; }
                        default:
                            {
                                exception = new GraphAPIException(responseItem.Status, responseItem.ToObject<GraphApiErrorRoot>());
                                logger.Error($"An error occurred while executing the batch request [{responseItem.Id}], Result: {exception.Message}");
                                break;
                            }
                    }
                }
            }
            if (exception != null) throw exception;
            var task = responseItemDic[SimpleItemId.GetTask];
            var taskDetails = responseItemDic[SimpleItemId.GetTaskDetails];
            var groupSite = responseItemDic[SimpleItemId.GetGroupSite];

            var taskObj = task.ToObject<GraphPlannerTask>();
            var taskDetailsObj = taskDetails.ToObject<GraphPlannerTaskDetails>();
            var siteWebUrl = groupSite.ToObject<GetGroupSiteObj>()?.WebUrl;

            var result = Tuple.Create(taskObj, taskDetailsObj, siteWebUrl);
            logger.Info("NecessaryInformation: task etag: {0}, task detail etag: {1}, site web url: {2}.", taskObj.OdataEtag, taskDetailsObj.OdataEtag, siteWebUrl);
            return result;
        }

        public Dictionary<string, string> DoMapping(Dictionary<string, string> assignToUsers)
        {
            try
            {
                var defaultUser = Config.DefaultUser4Mapping;
                var sourceBackupAssignUsers = assignToUsers.Keys.ToList();
                var userMappinpMatchUsers = new List<string>();
                var tempUserList = new List<string>();
                sourceBackupAssignUsers.ForEach(user =>
                {
                    logger.Info("User mapping--task assign user: {0}.", user);
                    var mUser = string.Empty;
                    if (Config.UserMapping.TryGetValue(user, out mUser))
                    {
                        logger.Info("User Mapping:{0} --> {1}", user, mUser);
                        userMappinpMatchUsers.Add(mUser);
                        tempUserList.Add(user);
                    }
                });
                tempUserList.ForEach(removeUser => sourceBackupAssignUsers.Remove(removeUser));

                var domainMappinpMatchUsers = new List<string>();
                var tempDomainUserList = new List<string>();
                sourceBackupAssignUsers.ForEach(user =>
                {
                    logger.Info("Domain mapping--task assign user: {0}.", user);
                    var mDomain = string.Empty;
                    var ud = user.Split('@');
                    var userName = ud[0];
                    var domain = ud[1];
                    if (Config.DomainMapping.TryGetValue(domain, out mDomain))
                    {
                        var mUser = string.Format("{0}@{1}", userName, mDomain);
                        logger.Info("Domain Mapping:{0} --> {1}", user, mUser);
                        domainMappinpMatchUsers.Add(mUser);
                        tempDomainUserList.Add(user);
                    }
                });
                tempDomainUserList.ForEach(removeUser => sourceBackupAssignUsers.Remove(removeUser));


                var allMappingUsers = new List<string>(userMappinpMatchUsers);
                allMappingUsers.AddRange(domainMappinpMatchUsers);
                allMappingUsers.AddRange(sourceBackupAssignUsers);
                if (!string.IsNullOrEmpty(defaultUser)) allMappingUsers.Add(defaultUser);
                //var dAllMappingUsers = allMappingUsers.Distinct().ToList();

                var gUsers = BatchGetUsers(allMappingUsers);

                logger.Info("Does not match users count: {0}, DefaultUser:{1}, All users Count:{2}, Exist user Count:{3}", sourceBackupAssignUsers.Count, defaultUser, allMappingUsers.Count, gUsers.Count);

                if (gUsers.Count == allMappingUsers.Count)
                {
                    gUsers.Remove(defaultUser ?? string.Empty);
                }
                return gUsers;
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when mapping users,ex :{0}", ex);
                return new Dictionary<string, string>();
            }
        }

        private Dictionary<string, string> BatchGetUsers(List<string> allMappingUsers)
        {
            if (!allMappingUsers.Any()) return new Dictionary<string, string>();
            var responseItems = PlannerService.BatchGetUsers(allMappingUsers);
            return responseItems
                .Where(item => item.IsSuccessStatusCode)
                .ToDictionary(key => key.Id, value => value.ToObject<GraphUser>().Id);
        }
        /// <summary>
        /// 删除Checklist ，References ，Assignments 中多余item 时，需要将其 value 置为 null.
        /// </summary>
        /// <param name="sourceDic"></param>
        /// <param name="key"></param>
        static void MergeDic(IDictionary sourceDic, string key)
        {
            // 使用sourceDic[key] = null会删除所有数据！
            if (!sourceDic.Contains(key))
            {
                sourceDic.Add(key, null);
            }
        }
    }
}