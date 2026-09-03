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
using System.IO;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;

namespace LS.BinarySerialization
{
    internal sealed class LSObjectNodeAnalyze
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<int, long> idMap;
        private Dictionary<long, int> idCMap;
        private List<LSObjectNode> binarySerializationTree;
        private List<LSObjectNode> dataOnlyTree;
        private Dictionary<long, List<LSObjectNode>> whoRefMe;
        //Added
        private LSObjectNodeCollection mObjectNodeCollection;


        private List<string> excludeObjectName;

        private LSObjectNode mMemberDataNode;
        internal LSObjectNode MemberDataNode
        {
            get { return mMemberDataNode; }
        }

        internal List<LSObjectNode> AllNodes
        {
            get { return mObjectNodeCollection.ObjectNodes; }
        }

        internal List<LSObjectNode> DataNodes
        {
            get { return dataOnlyTree; }
        }

        internal Dictionary<long,LSObjectNode> FieldNodes
        {
            get { return mObjectNodeCollection.FieldNodes; }
        }

        internal Dictionary<int, long> IndexToObjectId
        {
            get { return idMap; }
        }

        internal Dictionary<long, int> ObjectIdToIndex
        {
            get { return idCMap; }
        }

        internal LSObjectNodeAnalyze(LSObjectNodeCollection nodeCollection)
        {
            mObjectNodeCollection = new LSObjectNodeCollection();
            mObjectNodeCollection.ObjectNodes = new List<LSObjectNode>(nodeCollection.ObjectNodes);
            mObjectNodeCollection.FieldNodes = new Dictionary<long, LSObjectNode>(nodeCollection.FieldNodes);
            nodeCollection.Clear();

            binarySerializationTree = new List<LSObjectNode>(mObjectNodeCollection.ObjectNodes);
            dataOnlyTree = new List<LSObjectNode>();
            whoRefMe = new Dictionary<long, List<LSObjectNode>>();

            idMap = new Dictionary<int, long>(mObjectNodeCollection.ObjectNodes.Count);
            idCMap = new Dictionary<long, int>(mObjectNodeCollection.ObjectNodes.Count);

            excludeObjectName = new List<string>();
            excludeObjectName.Add("System.Reflection.MemberInfoSerializationHolder");
            excludeObjectName.Add("Microsoft.Office.Workflow.Actions.Stage");
            excludeObjectName.Add("Microsoft.Office.Workflow.Actions.StageContainer");
            excludeObjectName.Add("System.Workflow.ComponentModel.WorkflowParameterBindingCollection");
            excludeObjectName.Add("System.Workflow.Runtime.CorrelationTokenCollection");
            excludeObjectName.Add("System.Workflow.Runtime.EventQueueState");
            excludeObjectName.Add("System.Workflow.Runtime.KeyedPriorityQueue`3[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.TimerEventSubscription, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
            excludeObjectName.Add("System.Workflow.Runtime.KeyedPriorityQueue`3+HeapNode`3[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.TimerEventSubscription, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.TimerEventSubscription, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
            excludeObjectName.Add("System.Workflow.Runtime.TimerEventSubscriptionCollection");
            excludeObjectName.Add("System.Workflow.Runtime.TrackingListenerBroker");
            excludeObjectName.Add("System.Collections.Generic.List`1[[System.Workflow.ComponentModel.ActivityExecutorDelegateInfo`1[[System.Workflow.ComponentModel.QueueEventArgs, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]], System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Workflow.Activities.EventQueueName");
            excludeObjectName.Add("System.Workflow.ComponentModel.DependencyProperty+DependencyPropertyReference");
            excludeObjectName.Add("System.Workflow.ComponentModel.Serialization.ActivitySurrogateSelector+ObjectSurrogate+ObjectSerializedRef");
            excludeObjectName.Add("System.Collections.Generic.Dictionary`2[[System.IComparable, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.EventQueueState, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.ComponentModel.WorkflowParameterBinding, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.CorrelationToken, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("Microsoft.SharePoint.Workflow.SPItemKey");
            excludeObjectName.Add("System.Collections.ArrayList");
            excludeObjectName.Add("System.Collections.Generic.KeyValuePair`2[[System.IComparable, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.EventQueueState, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.ComponentModel.WorkflowParameterBinding, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Workflow.Runtime.CorrelationToken, System.Workflow.Runtime, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.List`1[[Microsoft.Office.Workflow.Utility.Contact, Microsoft.Office.Workflow.Tasks, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c]]");
            excludeObjectName.Add("System.Collections.Generic.List`1[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
            excludeObjectName.Add("System.Collections.Generic.List`1[[System.Workflow.ComponentModel.Activity, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.List`1[[System.Workflow.ComponentModel.WorkflowParameterBinding, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Generic.Queue`1[[System.Workflow.ComponentModel.SchedulableItem, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Collections.Hashtable");
            excludeObjectName.Add("System.DelegateSerializationHolder+DelegateEntry");
            excludeObjectName.Add("System.Workflow.ComponentModel.Serialization.DependencyStoreSurrogate+DependencyStoreRef");
            excludeObjectName.Add("System.Workflow.ComponentModel.ActivityExecutionContextInfo");
            excludeObjectName.Add("System.Workflow.ComponentModel.ActivityExecutorDelegateInfo`1[[System.Workflow.ComponentModel.ActivityExecutionStatusChangedEventArgs, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");
            excludeObjectName.Add("System.Workflow.ComponentModel.ActivityExecutorDelegateInfo`1[[System.Workflow.ComponentModel.QueueEventArgs, System.Workflow.ComponentModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]");

            //excludeObjectName.Add("System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef");
            //excludeObjectName.Add("System.UnitySerializationHolder");
        }

        internal void Analyze()
        {
            for (int i = 0; i < AllNodes.Count; i++)
            {
                idMap.Add(i, AllNodes[i].ObjectId);
                idCMap.Add(AllNodes[i].ObjectId, i);
            }
            for (int i = 0; i < AllNodes.Count; i++)
            {
                FixupReference(AllNodes[i]);
                WhoReferenceMe(AllNodes[i]);
                if (IsIncludeDataNode(AllNodes[i]))
                {
                    if (FixupGuidValue(AllNodes[i]))
                        dataOnlyTree.Add(AllNodes[i]);
                }
                if (AllNodes[i].ObjectId == 5)
                    mMemberDataNode = AllNodes[i];

            }
            dataOnlyTree.Sort(new Icp());

            string s = "";
        }

        public void Dispose()
        {
            if(excludeObjectName!=null)
            {
                excludeObjectName.Clear();
                excludeObjectName = null;
            }
            if (whoRefMe != null)
            {
                whoRefMe.Clear();
                whoRefMe = null;
            }
            if (dataOnlyTree != null)
            {
                dataOnlyTree.Clear();
                dataOnlyTree = null;
            }
            if (binarySerializationTree != null)
            {
                binarySerializationTree.Clear();
                binarySerializationTree = null;
            }
            if (idCMap != null)
            {
                idCMap.Clear();
                idCMap = null;
            }
            if (idMap != null)
            {
                idMap.Clear();
                idMap = null;
            }
        }

        private void FixupReference(LSObjectNode node)
        {
            if (node.MemberRefs == null || node.MemberRefs.Count == 0)
                return;

            foreach (KeyValuePair<string, string> pair in node.MemberRefs)
            {
                long objectId = long.Parse(pair.Value);
                int objectIndex = -1;

                try
                {
                    if (idCMap.ContainsKey(objectId))
                        objectIndex = idCMap[objectId];
                    if (objectIndex >= 0)
                    {
                        if (node.Type == InternalObjectTypeE.Array)
                        {
                            string name = AllNodes[objectIndex].Name;
                            if (string.IsNullOrEmpty(name))
                                AllNodes[objectIndex].Name = node.Name;
                            //int arrayItemIndex = int.Parse(pair.Key);
                            //node.ArrayValues[arrayItemIndex] = nodes[objectIndex];
                        }
                        else if (node.Type == InternalObjectTypeE.Object)
                        {
                            string name = AllNodes[objectIndex].Name;
                            if (string.IsNullOrEmpty(name))
                                AllNodes[objectIndex].Name = node.Name;

                            //node.Members[pair.Key] = nodes[objectIndex];
                        }
                    }
                    else if (FieldNodes.ContainsKey(objectId))
                    {
                        LSObjectNode fieldNode = FieldNodes[objectId];
                        if (fieldNode.FieldType == InternalMemberTypeE.Field)
                        {
                            if (node.Type == InternalObjectTypeE.Array)
                            {
                                throw new NotSupportedException();
                            }
                            else if (node.Type == InternalObjectTypeE.Object && fieldNode.Members.ContainsKey(pair.Key))
                            {
                                node.Members[pair.Key] = fieldNode.Members[pair.Key];
                            }
                        }
                        else if (fieldNode.FieldType == InternalMemberTypeE.Item)
                        {
                            if (node.Type == InternalObjectTypeE.Array)
                            {
                                int arrayItemIndex = int.Parse(pair.Key);
                                node.ArrayValues[arrayItemIndex] = fieldNode.ArrayValues[0];
                            }
                            else if (node.Type == InternalObjectTypeE.Object)
                            {
                                node.Members[pair.Key] = fieldNode.ArrayValues[0];
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.FixUpReferenceError, e.ToString());
                }//need not to log
            }
        }

        private bool FixupGuidValue(LSObjectNode node)
        {
            if (!string.IsNullOrEmpty(node.Name) && node.Name == "System.Guid")
            {
                node.GuidInternalValue = new Guid((byte[])node.ArrayValues[0]);
                if (node.GuidInternalValue == Guid.Empty)
                    return false;
            }
            return true;

        }
        
        private bool IsIncludeDataNode(LSObjectNode node)
        {
            if (!string.IsNullOrEmpty(node.Name) && excludeObjectName.Contains(node.Name))
                return false;

            if (node.Type == InternalObjectTypeE.Array)
            {
                foreach (object o in node.ArrayValues)
                {
                    if (o == null)
                        continue;
                    if (!(o is InternalMemberValueE))
                    {
                        return true;
                    }
                    else if (node.Name == "System.Workflow.ComponentModel.Serialization.ActivitySurrogate+ActivitySerializedRef")
                        return true;
                }
            }
            else if (node.Type == InternalObjectTypeE.Object)
            {
                foreach(KeyValuePair<string,object> pair in node.Members)
                {
                    if(pair.Key=="value__")
                        continue;
                    //if (pair.Key == "id" && IsMatchId(pair.Value))
                    //    continue;
                    if (IsMatchSize(pair))
                        continue;
                    if (IsMatchVersion(pair))
                        continue;
                    if (!(pair.Value is InternalMemberValueE))
                        return true;
                }
            }
            return false;
        }

        private bool IsMatchId(object idObj)
        {
            string id = idObj.ToString();
            //id, 2.8.3.0.0.1.0.0.0
            if (string.IsNullOrEmpty(id))
                return true;
            for (int i = 1; i < id.Length; i++)
            {
                int v = (int)id[i];
                if (v!= 46 && (v<48 || v>57))//v!='.' && (v<0 || v>9)
                    return false;
            }
            return true;
        }

        private bool IsMatchSize(KeyValuePair<string, object> pair)
        {
            int value = 0;
            if (pair.Key == "_size" && Int32.TryParse(pair.Value.ToString(), out value) && value == 1)
                return true;
            return false;
        }

        private bool IsMatchVersion(KeyValuePair<string, object> pair)
        {
            int value = 0;
            if (pair.Key == "_version" && Int32.TryParse(pair.Value.ToString(), out value) && value == 1)
                return true;
            return false;
        }

        private void WhoReferenceMe(LSObjectNode node)
        {
            foreach (KeyValuePair<string, string> pair in node.MemberRefs)
            {
                List<LSObjectNode> refs = null;
                long objectId = long.Parse(pair.Value);
                if (whoRefMe.ContainsKey(objectId))
                {
                    refs = whoRefMe[objectId];
                }
                else
                {
                    refs = new List<LSObjectNode>();
                    whoRefMe.Add(objectId, refs);
                }
                refs.Add(node);
            }
        }

        private bool ContainsValue(LSObjectNode objectNode, object value)
        {
            if (objectNode.Type == InternalObjectTypeE.Array)
            {
                foreach (object o in objectNode.ArrayValues)
                {
                    if (o == null)
                        continue;
                    if (o is InternalMemberValueE)
                        continue;

                    if (o.GetType() == value.GetType())
                    {
                        if (o.Equals(value))
                            return true;
                    }
                }
            }
            else if (objectNode.Type == InternalObjectTypeE.Object)
            {
                foreach (KeyValuePair<string, object> pair in objectNode.Members)
                {
                    if (pair.Value == null)
                        continue;
                    if (pair.Value is InternalMemberValueE)
                        continue;

                    if (pair.Value.GetType() == value.GetType())
                    {
                        if (pair.Value.Equals(value))
                            return true;
                    }
                }
            }
            return false;
        }
    }

    internal class Icp : IComparer<LSObjectNode>
    {
        public int Compare(LSObjectNode a, LSObjectNode b)
        {
            string aName = string.Empty;
            string bName = string.Empty;

            if (a.Name == null)
                return 1;
            if (b.Name == null)
                return -1;
            if (a.Name != null)
                aName = a.Name;
            if (b.Name != null)
                bName = b.Name;
            if (string.Compare(aName, bName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (string.Compare(a.GuidInternalValue.ToString(), b.GuidInternalValue.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return (int)(a.ObjectId - b.ObjectId);
                }
                else
                    return string.Compare(a.GuidInternalValue.ToString(), b.GuidInternalValue.ToString(), StringComparison.Ordinal);//a.GuidInternalValue.ToString().CompareTo(b.GuidInternalValue.ToString());
            }
            else
                return string.Compare(aName, bName, StringComparison.Ordinal);//aName.CompareTo(bName) change for add StringComparison
        }
    }
}
