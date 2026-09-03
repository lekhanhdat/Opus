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
using System.Xml;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow.Nintex;

namespace LS.SPWorkflowProcessor
{
    internal abstract class NWActionProcessorBase : INWActionProcessor
    {
        protected string actionId = Guid.NewGuid().ToString();
        protected NWActionConfig sourceConfig;
        protected string CLASSNAME { set; get; }

        protected NWActionProcessorBase(NintexWFActionProcessor workflowActionProcessor)
        {
            this.workflowActionProcessor = workflowActionProcessor;
        }

        /// <summary>
        /// </summary>
        protected NintexWFActionProcessor workflowActionProcessor = null;

        /// <summary>
        /// </summary>
        /// <param name="nwActionConfig"></param>
        /// <returns></returns>
        public virtual WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;

            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };

            return action;
        }

        protected virtual Configuration CreateConfiguration()
        {
            return new Configuration
            {
                Id = actionId,
                Name = GetWorkflowActionName(),
                Image = CreateImage(),
                ServerInfo = CreateServerInfo(),
                Properties = CreateProperties(),
                HelpKey = CLASSNAME,
                Disabled = !sourceConfig.Enabled,
            };
        }

        protected virtual string GetWorkflowActionName()
        {
            return sourceConfig.TLabel;
        }

        protected abstract Image CreateImage();

        protected virtual ServerInfo CreateServerInfo()
        {
            return new ServerInfo
            {
                ClassName = CLASSNAME,
                Assembly = string.Empty
            };
        }

        protected virtual List<Property> CreateProperties()
        {
            return new List<Property>();
        }

        protected virtual ParametersValue ConvertParameterValue(ActivityParameter activityParameter)
        {
            var temp = NWValueConverter.ConvertValueToParametersValue(workflowActionProcessor, activityParameter);
            if (temp == null)
            {
                return new ParametersValue { };
            }
            return temp;
        }

        protected void CheckUnsupportedActionType(ActivityParameter para)
        {
            if (para.ProfileLookup != null)
            {
                throw new UnSupportedActionTypeException("Unsupported value type ProfileLookup");
            }
            if (para.WorkflowConstant != null)
            {
                throw new UnSupportedActionTypeException("Unsupported value type WorkflowConstant");
            }
            if (para.PrimitiveValue != null && para.PrimitiveValue.Value.LastIndexOf("{WFConstant:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new UnSupportedActionTypeException("Unsupported value type WorkflowConstant");
            }
        }

        protected void CheckUnsupportedOperator(string actionType, ActivityParameter para)
        {
            if (actionType.Equals("Microsoft.SharePoint.WorkflowServices.Activities.WaitForFieldChange", StringComparison.OrdinalIgnoreCase) && !para.PrimitiveValue.Value.Equals("Equal", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnSupportedOperatorException(WrapperNintexWorkflowResource.UnSupportedOperator, CLASSNAME, para.PrimitiveValue.Value);
            }
        }

        protected string TryGetTheValueOfPrimitiveValue(ActivityParameter activityPara, string defaultValue)
        {
            if (activityPara == null || activityPara.PrimitiveValue == null)
            {
                return defaultValue;
            }
            else
            {
                return activityPara.PrimitiveValue.Value;
            }
        }
    }
}