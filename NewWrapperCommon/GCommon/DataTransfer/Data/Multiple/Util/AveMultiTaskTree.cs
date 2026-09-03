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
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.Multiple.Util
{
    internal class AveMultiTaskTree
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveMultiTaskTree));

        public bool Finished { get; set; }
        public AveMultiTask Root { get; set; }
        public int MaxMultipleTaskCount { get; private set; }

        private int multipleTaskCount;
        private int treeNodeCount;
        private Dictionary<int, AveMultiTask> lastNodeByLevel;
        private AveMultiTask lastAddTask;

        public AveMultiTaskTree(int maxCount)
        {
            MaxMultipleTaskCount = maxCount;
            multipleTaskCount = 0;
            treeNodeCount = 0;
            lastNodeByLevel = new Dictionary<int, AveMultiTask>();
        }

        private void CheckTaskTreeLevel(AveMultiTask task)
        {
            if (task == null) return;
            if (lastAddTask == null) return;
            if (task.TreeLevel <= lastAddTask.TreeLevel + 1) return;

            throw new Exception(string.Format("task level not correct:{0}, expected less than:{1}" , task.TreeLevel, lastAddTask.TreeLevel + 1));
        }
        private AveMultiTask AddNormalTaskNode(AveMultiTask task)
        {
            CheckTaskTreeLevel(task);

            var parentLevel = task.TreeLevel - 1;
            AveMultiTask lastNode;
            if (lastNodeByLevel.TryGetValue(parentLevel, out lastNode))
            {
                lock (lastNode)
                {
                    if (lastNode.Children == null) lastNode.Children = new List<AveMultiTask>(1);
                    lastNode.Children.Add(task);
                    lastNode.ChildrenCount++;
                    task.Parent = lastNode;
                }
            }
            else
            {
                throw new InvalidOperationException("task level not correct:" + task.TreeLevel);
            }
            return task;
        }
        private void ProcessTreeCompleted(AveMultiTask task)
        {
            if (task == null || lastAddTask == null)
            {
                var t = lastAddTask;
                while (t != null)
                {
                    t.FabricateComplete = true;
                    t = t.Parent;
                }
            }
            else
            {
                if (task.TreeLevel == lastAddTask.TreeLevel)
                {
                    lastAddTask.FabricateComplete = true;
                }
                else if (task.TreeLevel < lastAddTask.TreeLevel)
                {
                    var temp = lastAddTask;
                    while (temp != null && !temp.Equals(task.Parent))
                    {
                        temp.FabricateComplete = true;
                        temp = temp.Parent;
                    }
                }
                else { /* do nothing */}
            }
        }
        private void ProcessTreeHierarchy(AveMultiTask task)
        {
            lastNodeByLevel[task.TreeLevel] = task;
            if (lastAddTask != null)
            {
                lastAddTask.Next = task;
            }
            lastAddTask = task;
        }
        private void TaskAdded(AveMultiTask task)
        {
            if (task.IsMultiple || task == Root)
            {
                Interlocked.Increment(ref multipleTaskCount);
            }

            Interlocked.Increment(ref treeNodeCount);
        }
        private void EnsureExistMultiTaskCount(Action<int> treeIsFullCallback)
        {
            var tempCount = treeNodeCount;
            while (multipleTaskCount > MaxMultipleTaskCount)
            {
                if (tempCount > treeNodeCount)
                {
                    break;
                }
                if (treeIsFullCallback != null)
                {
                    treeIsFullCallback(multipleTaskCount);
                }
                Thread.Sleep(10);
            }
        }

        public AveMultiTask AddToTree(AveMultiTask task, Action<int> treeIsFullCallback = null)
        {
            EnsureExistMultiTaskCount(treeIsFullCallback);

            if (task == null)
            {
                ProcessTreeCompleted(task);
                Finished = true;
                return task;
            }
            if (Root == null)
            {
                if (task.TreeLevel != 0) throw new InvalidOperationException("task level not correct." + task.TreeLevel);
                Root = task;
            }
            else
            {
                AddNormalTaskNode(task);
                ProcessTreeCompleted(task);
            }

            ProcessTreeHierarchy(task);
            TaskAdded(task);

            return task;
        }
        public AveMultiTask EnsureNext(AveMultiTask task, bool exitWhenNotFinish = false)
        {
            if (task == null)
            {
                while (this.Root == null)
                {
                    if (Finished) return null;
                    else
                    {
                        if (exitWhenNotFinish) return null;
                        Thread.Sleep(50);
                        continue;
                    }
                }
                return this.Root;
            }
            else
            {
                while (task.Next == null)
                {
                    if (Finished) return null;
                    else
                    {
                        if (exitWhenNotFinish) return null;
                        Thread.Sleep(50);
                        continue;
                    }
                }

                return task.Next;
            }
        }
        public AveMultiTask EnsureNextWithMinStatus(AveMultiTask task, TaskStatus status)
        {
            var next = EnsureNext(task);
            if (next == null) return null;
            while (next.Status < status)
            {
                Thread.Sleep(50);
            }
            return next;
        }
        public bool EnsureHasChildren(AveMultiTask task)
        {
            if (task.ChildrenCount > 0)
            {
                return true;
            }
            if (!task.FabricateComplete)
            {
                EnsureNext(task);
            }

            return task.ChildrenCount > 0;
        }

        public void TaskProcessEnd(AveMultiTask task)
        {
            if (task.IsMultiple || task == Root)
            {
                Interlocked.Decrement(ref multipleTaskCount);
            }
        }

        public void Remove(AveMultiTask task)
        {
            Interlocked.Decrement(ref treeNodeCount);

            var parent = task.Parent;
            if (parent == null) return;
            lock (parent)
            {
                if (parent.Next != null && parent.Next.Equals(task))
                {
                    parent.Next = null;
                }
                if (parent.Children != null)
                {
                    parent.Children.Remove(task);
                }
            }
        }

    }
}
