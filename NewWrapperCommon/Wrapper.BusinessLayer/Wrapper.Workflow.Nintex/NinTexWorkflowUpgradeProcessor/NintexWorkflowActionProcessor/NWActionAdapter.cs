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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    /// <summary>
    /// 根据Action的类型调用不同的Processor
    /// </summary>
    class NWActionAdapter
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(NWActionAdapter));

        private readonly NintexWFActionProcessor workflowActionProcessor;

        private static readonly Dictionary<string, string> supportedActions;

        /// <summary>
        /// 记录是否存在PlaceHolderAction 如果存在PlaceHolderAction的话 无法Publish workflow，只能Save
        /// </summary>
        public bool HasPlaceHolderAction
        {
            get;
            private set;
        }

        private static readonly List<string> skipActions;
        static NWActionAdapter()
        {
            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Wrapper.Workflow.Nintex.NWActionProcessor.config");
            Debug.Assert(stream != null, "stream != null");

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            var supportedActionsNodes = doc.SelectNodes("/Actions/SupportedActions/Action");
            if (supportedActionsNodes == null)
            {
                logger.Warn("Can not find supported actions in config file.");
                throw new ArgumentNullException("nodes");
            }
            supportedActions = supportedActionsNodes.Cast<XmlElement>().ToDictionary(node => node.GetAttribute("ActionType"), node => node.GetAttribute("ProcessorType"));

            var skipActionsNodes = doc.SelectNodes("/Actions/SkipActions/Action");
            if (skipActionsNodes != null)
            {
                skipActions = skipActionsNodes.Cast<XmlElement>().Select(node => node.GetAttribute("ActionType")).ToList();
            }
            else
            {
                skipActions = new List<string>();
            }
        }


        private static void InitFilterActions()
        { }

        public NWActionAdapter(NintexWFActionProcessor actionProcessor)
        {
            this.workflowActionProcessor = actionProcessor;
        }

        /// <summary>
        /// 把NWActionConfig数据类型转换成WorkflowAction类型
        /// </summary>
        /// <param name="sourceConfig"></param>
        /// <returns></returns>
        internal WorkflowAction UpgradeWorkflowAction(NWActionConfig sourceConfig)
        {
            return UpdateActionByType(sourceConfig.Type, sourceConfig);
        }

        internal WorkflowAction CreateSequenceActivityWorkflowAction()
        {
            return UpdateActionByType("Nintex.Workflow.Activities.Adapters.WFSequenceAdapter", null);
        }

        /// <summary>
        /// 把NWActionConfig数据类型转换成WorkflowAction类型
        /// </summary>
        /// <param name="sourceConfig"></param>
        /// <returns></returns>
        internal WorkflowAction UpdateActionByType(string type, NWActionConfig sourceConfig)
        {
            try
            {
                if (supportedActions.ContainsKey(type))
                {
                    var processorType = supportedActions[type];
                    var actionInstance = Activator.CreateInstance(Assembly.GetCallingAssembly().GetType(processorType), new object[] { this.workflowActionProcessor }) as INWActionProcessor;
                    return actionInstance.UpgradeWorkflowAction(sourceConfig);
                }
                else if (skipActions.Contains(type))
                {
                    logger.Debug("Skip the action {0}.", type);
                    return null;
                }
                else
                {
                    HasPlaceHolderAction = true;
                    logger.Debug("Replace unsupported action type:{0} with placeholder action.", type);
                    return new NWPlaceHolderActionProcesor(this.workflowActionProcessor).UpgradeWorkflowAction(sourceConfig);
                }
            }
            catch (NotSupportedException e)
            {
                HasPlaceHolderAction = true;
                logger.Debug("Replace action type:{0} with placeholder action. Reason:{1}", type, e.Message);
                return new NWPlaceHolderActionProcesor(this.workflowActionProcessor).UpgradeWorkflowAction(sourceConfig);
            }
            //throw new NotSupportedException(string.Format("{0} action is not supported now", type));
        }

    }
}
