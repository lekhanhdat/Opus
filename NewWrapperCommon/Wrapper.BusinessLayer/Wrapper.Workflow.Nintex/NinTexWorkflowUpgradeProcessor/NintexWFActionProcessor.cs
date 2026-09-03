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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LS.SPWorkflowProcessor
{
    class NintexWFActionProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private NWVariablesCacheManager variablesCacheManager;

        private Dictionary<string, string> actionIdAndFormMapping = new Dictionary<string, string>();

        public NintexWFActionProcessor(IAveWeb web, Variable[] variables)
            : this(web, null, variables, null)
        { }

        public NintexWFActionProcessor(IAveWeb web, IAveList list, Variable[] variables, INintexDataMappingManager dataMappingManager)
        {
            this.Web = web;
            this.List = list;
            WorkflowActionAdapter = new NWActionAdapter(this);
            variablesCacheManager = new NWVariablesCacheManager(variables);
            DataMappingManager = dataMappingManager;
        }

        /// <summary>
        /// 注意：
        /// 该Mapping Manager只用于Check 数据是否合法，并不用于替换数据
        /// </summary>
        public INintexDataMappingManager DataMappingManager { get; set; }

        public IAveWeb Web { get; private set; }

        public IAveList List { get; private set; }

        public NWVariablesCacheManager VariablesCacheManager
        {
            get
            {
                return variablesCacheManager;
            }
        }


        public bool IsWebLevel
        {
            get
            {
                return List == null;
            }
        }

        /// <summary>
        /// NintexForm和NintexWorkflow是通过Action Id一一对应
        /// </summary>
        public Dictionary<string, string> ActionIdAndFormMapping
        {
            get
            {
                return actionIdAndFormMapping;
            }
        }


        public NWActionAdapter WorkflowActionAdapter { get; private set; }

        public WorkflowAction BuildWorkflowAction(ExportedWorkflow exportedWorkflow)
        {
            WorkflowAction rootAction = WorkflowActionAdapter.UpgradeWorkflowAction(exportedWorkflow.Configurations.ActionConfigs[0]);

            AddChildrenWorkflowAction(rootAction, exportedWorkflow.Configurations.ActionConfigs.Skip(1).ToArray());

            return rootAction;
        }

        private void AddActionIdMapping(NWActionConfig actionConfig, string actionId)
        {
            if (actionConfig.ExtensionProperties != null && actionConfig.ExtensionProperties.Count > 0)
            {
                foreach (ExtensionProperty eq in actionConfig.ExtensionProperties)
                {
                    //源端能够设置nintex form的action如果进入了设置form页面，但是并不保存，会出现value为string.Empty的情况
                    if (!string.IsNullOrEmpty(eq.Value) && eq.Key.IndexOf(".xml", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        if (!ActionIdAndFormMapping.ContainsKey(actionId))
                        {
                            ActionIdAndFormMapping.Add(actionId, eq.Key);
                        }
                    }
                }
            }
        }

        internal void AddChildrenWorkflowAction(WorkflowAction parentAction, NWActionConfig[] childActivities)
        {
            if (childActivities != null && childActivities.Length > 0)
            {
                WorkflowAction childAction = null;
                foreach (var childActionConfig in childActivities)
                {
                    var tempChildAction = WorkflowActionAdapter.UpgradeWorkflowAction(childActionConfig);
                    if (tempChildAction != null)
                    {
                        if (childAction == null)
                        {
                            childAction = tempChildAction;

                            AddActionIdMapping(childActionConfig, childAction.Id);
                            TryAddSequenceActivityWorkflowAction(childAction);
                            if (childAction != null)
                            {
                                parentAction.Children.Add(childAction);
                            }
                            continue;
                        }

                        while (childAction.Next != null) // 两个连续的action set，并且第一个action set中包含两个及以上个action
                        {
                            childAction = childAction.Next;
                        }

                        childAction.Next = tempChildAction;

                        AddActionIdMapping(childActionConfig, childAction.Id);

                        childAction = childAction.Next;
                        TryAddSequenceActivityWorkflowAction(childAction);
                    }
                }
            }
        }

        //当前研究发现如果没有Children则会添加SequenceActivityWorkflowAction
        //当前研究发现Send an email 即使ChildrenCount==0 也没有SequenceActivityWorkflowAction
        private void TryAddSequenceActivityWorkflowAction(WorkflowAction workflowAction)
        {
            if (workflowAction != null && workflowAction.Children.Count == 0
             && (workflowAction.ClassName.Equals("#Filter", StringComparison.OrdinalIgnoreCase)
             || workflowAction.ClassName.Equals("#ForEach", StringComparison.OrdinalIgnoreCase))
             )
            {
                workflowAction.Children.Add(WorkflowActionAdapter.CreateSequenceActivityWorkflowAction());
            }
        }
    }
}
