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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Threads;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Workflow.Builder;
using AvePoint.RA.Workflow.DisposalReview;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AvePoint.RA.Workflow.Common
{
    public abstract class AbstractWFService<T> : IWFService<T> where T : BaseReviewRequestInfo
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IWFInstanceStore WfInstanceStore { get; set; }
        public IWorkflowInstanceDao WorkflowInstanceDao { get; set; }
        public IWorkflowDataDao WorkflowDataDao { get; set; }

        public abstract RMWorkflowType WorkflowType { get; }

        public void Cancel(T request, string definitionXamlStr)
        {
            Guid workflowInstanceId = request.InstanceId;
            try
            {
                AutoResetEvent autoEvent = InitEvent();
                
                AssembleLogonInfo(request);
                WorkflowApplication wfApp = LoadApplicationInstance(request, definitionXamlStr, autoEvent);
                logger.Debug($"Try to cancel the {WorkflowType} workflow {workflowInstanceId}");
                wfApp.Cancel();
                Wait(autoEvent);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while cancel the {WorkflowType} workflow instance : {workflowInstanceId},error message: {e.ToString()}");
                throw;
            }
        }

        public void Resume(T request, string definitionXamlStr, string bookmark = null)
        {
            try
            {
                AutoResetEvent autoEvent = InitEvent();

                AssembleLogonInfo(request);
                WorkflowApplication wfApp = LoadApplicationInstance(request, definitionXamlStr, autoEvent);
                var bookmarks = wfApp.GetBookmarks();
                if (string.IsNullOrEmpty(bookmark) && bookmarks.Count == 1)
                {
                    Resume(wfApp, bookmarks.ToList().First().BookmarkName, request);
                }
                else if (!string.IsNullOrEmpty(bookmark))
                {
                    if (bookmarks.Any(b => b.BookmarkName.Equals(bookmark, StringComparison.CurrentCulture)))
                    {
                        Resume(wfApp, bookmark, request);
                    }
                    else
                    {
                        //no bookmark for nextState
                        wfApp.Unload();
                        throw new ArgumentException($"can't find the corresponding bookmark for {WorkflowType} workflow state '{bookmark}'", "nextStep");
                    }
                }
                else
                {
                    wfApp.Unload();
                }

                Wait(autoEvent);
            }
            catch (Exception e)
            {
                logger.Error($"Resume {WorkflowType} workflow error: {e.ToString()}");
                throw;
            }
        }

        public Guid StartWorkflow(T request, string definitionXamlStr)
        {
            try
            {
                logger.Info($"$$$$workflow start {request.InstanceId}");//TO Do remove before march.
                var inputs = GetInputParam(request);

                Activity wf = XamlBuilder.LoadActivityFromXaml(definitionXamlStr);

                WorkflowApplication wfApp = new WorkflowApplication(wf, inputs);

                request.InstanceId = wfApp.Id;

                //save instance data
                SaveInstance(request);

                AutoResetEvent autoEvent = InitEvent();

                // Configure the instance store, extensions, and 
                // workflow lifecycle handlers.
                ConfigureWorkflowApplication(wfApp, request.InstanceId, autoEvent);
                logger.Info($"Starting the new {WorkflowType} workflow {request.InstanceId}");
                // Start the workflow.
                wfApp.Run();

                

                Wait(autoEvent);
                logger.Info($"$$$$$$Started the new {WorkflowType} workflow {request.InstanceId}");
                return request.InstanceId;
            }
            catch (Exception e)
            {
                logger.Error($"Start new {WorkflowType} workflow error: {e.ToString()}");
                throw;
            }
        }

        #region private method

        private void AssembleLogonInfo(T request)
        {
            var tmp = ThreadSetting.GetSetting();
            request.ThreadSetting = new ThreadSetting()
            {
                LogonGroupId = tmp.LogonGroupId,
                LogonUserId = tmp.LogonUserId,
                DisplayName = tmp.DisplayName,
                LogonUserEmail = tmp.LogonUserEmail
            };
        }

        private IDictionary<string, object> GetInputParam(T request)
        {
            IDictionary<string, object> inputs = new Dictionary<string, object>();
            AssembleLogonInfo(request);
            inputs.Add(DisposalReviewWorkflowBuilder.ArgRequestInfoName, request);
            return inputs;
        }

        private void Resume(WorkflowApplication wfApp, string bookmark, T request)
        {
            logger.Info($"Try to resume bookmark: {bookmark} for {WorkflowType} workflow with instance id: {request.InstanceId}");
            wfApp.ResumeBookmark(bookmark, request);
        }

        private bool SaveInstance(T request)
        {
            var instance = new RMWorkflowInstance()
            {
                Id = request.InstanceId,
                DefinitionId = request.DefinitionId,
                Status = RMWorkflowStatus.Running,
                CurStepId = string.Empty,
                CurStepName = string.Empty,
                ModifiedTime = DateTime.UtcNow
            };

            return WorkflowInstanceDao.Save(instance);
        }

        private void ConfigureWorkflowApplication(WorkflowApplication wfApp, Guid instanceId, AutoResetEvent autoEvent)
        {
            //since workflow activities are executed in different thread, so cache the thread setting and then get it within activities.
            //WFThreadCache.SetVaue(instanceId, ThreadSetting.GetSetting());

            // Configure the persistence store.
            wfApp.InstanceStore = WfInstanceStore.GeInstanceStore(instanceId);

            var status = RMWorkflowStatus.Running;

            wfApp.Completed = delegate (WorkflowApplicationCompletedEventArgs e)
            {
                status = RMWorkflowStatus.Completed;
                if (e.CompletionState == ActivityInstanceState.Faulted)
                {
                    status = RMWorkflowStatus.Faulted;
                    logger.Warn($"{WorkflowType} workflow {e.InstanceId} is terminated. Exception : {e.TerminationException.ToString()}");
                }
                else if (e.CompletionState == ActivityInstanceState.Canceled)
                {
                    //WFThreadHelper.SetForCurrentThread(e.InstanceId); //set the setting for current thread
                    status = RMWorkflowStatus.Canceled;
                    logger.Warn($"{WorkflowType} workflow {e.InstanceId} is canceled.");
                }
                else
                {
                    logger.Debug($"{WorkflowType} workflow {e.InstanceId} is completed.");
                }

                //Update the workflow instance statue
                WorkflowInstanceDao.UpdateStatus(e.InstanceId, status);

            };

            wfApp.Aborted = delegate (WorkflowApplicationAbortedEventArgs e)
            {
                logger.Warn($"{WorkflowType} workflow {e.InstanceId} is aborted.");
            };

            wfApp.OnUnhandledException = delegate (WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                logger.Warn($"UnhandledException for {WorkflowType} workflow {e.InstanceId}, exception: {e.UnhandledException.ToString()}");
                return UnhandledExceptionAction.Abort;
            };

            wfApp.PersistableIdle = delegate (WorkflowApplicationIdleEventArgs e)
            {
                logger.Debug($"{WorkflowType} workflow {e.InstanceId} is idle.");
                return PersistableIdleAction.Unload;
            };

            wfApp.Unloaded = delegate (WorkflowApplicationEventArgs e)
            {
                if (status == RMWorkflowStatus.Completed || status == RMWorkflowStatus.Canceled) //workflow结束，需要删除对应存储在instance store中的data
                {
                    try
                    {
                        WorkflowDataDao.DeleteById(e.InstanceId);
                        logger.Info($"{WorkflowType} workflow instance data : {e.InstanceId} is deleted.");
                    }
                    catch (Exception e1)
                    {
                        logger.Warn($"Failed to delete {WorkflowType} workflow instance data : {e.InstanceId}. {e1.ToString()}");
                    }
                }

                autoEvent.Set();
            };

        }

        private AutoResetEvent InitEvent()
        {
            return new AutoResetEvent(false);
        }

        private void Wait(AutoResetEvent autoEvent)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(30000);
            autoEvent.WaitOne(timeSpan);
        }

        private WorkflowApplication LoadApplicationInstance(T requestInfo, string definitionXamlStr, AutoResetEvent autoEvent)
        {
            // load the instance
            var definition = XamlBuilder.LoadActivityFromXaml(definitionXamlStr);
            WorkflowApplication wfApp = new WorkflowApplication(definition);

            ConfigureWorkflowApplication(wfApp, requestInfo.InstanceId, autoEvent);
            // add the instance to the list of running instances in the host
            wfApp.Load(requestInfo.InstanceId);
            return wfApp;
        }

        #endregion
    }
}
