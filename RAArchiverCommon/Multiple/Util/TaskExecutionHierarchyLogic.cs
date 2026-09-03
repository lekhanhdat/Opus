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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    internal class TaskExecutionHierarchyLogic
    {
        private AveMultiTaskTree taskTree;
        private AveMultiTask task;

        public TaskExecutionHierarchyLogic(AveMultiTaskTree taskTree, AveMultiTask task)
        {
            this.taskTree = taskTree;
            this.task = task;
        }

        public void ExecuteTask()
        {
            try
            {
                ProcessTask(task);
            }
            finally
            {
                this.taskTree.TaskProcessEnd(task);
            }
        }

        private void ProcessTask(AveMultiTask task)
        {
            var executing = task;
            do
            {
                InternalProcess(executing);

                InternalProcessPostAction(executing);

                executing = this.taskTree.EnsureNext(executing);
            }
            while (executing != null && !executing.IsMultiple);
        }

        private void InternalProcess(AveMultiTask task)
        {
            try
            {
                task.Status = TaskStatus.Processing;

                task.PreAction();
                task.Process();
                task.Complete();
            }
            catch (Exception ex)
            {
                task.Exception(ex);
            }
            finally
            {
                task.Status = TaskStatus.ProcessEnd;
            }
        }
        private bool InternalExecutePostAction(AveMultiTask task)
        {
            bool executable = false;
            if (task.Status == TaskStatus.ProcessEnd)
            {
                lock (task)
                {
                    if (task.Status == TaskStatus.ProcessEnd)
                    {
                        try
                        {
                            executable = true;
                            task.PostAction();

                            taskTree.Remove(task);
                        }
                        finally
                        {
                            task.Status = TaskStatus.Finished;
                        }
                    }
                }
            }
            return executable;
        }
        private void InternalProcessPostAction(AveMultiTask task)
        {
            if (!this.taskTree.EnsureHasChildren(task))
            {
                InternalExecutePostAction(task);
            }

            var parent = task.Parent;
            if (parent == null) return;

            if (parent.FabricateComplete)
            {
                bool allSubFinished = false;
                lock (parent)
                {
                    allSubFinished = parent.Children.All(n => n.Status == TaskStatus.Finished);
                }
                if (allSubFinished)
                {
                    var executable = InternalExecutePostAction(parent);
                    if (executable)
                    {
                        InternalProcessPostAction(parent);
                    }
                }
            }
        }
    }
}
