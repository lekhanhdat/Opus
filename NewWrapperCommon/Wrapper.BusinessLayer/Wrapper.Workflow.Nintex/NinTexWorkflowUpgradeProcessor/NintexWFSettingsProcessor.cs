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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Linq;

namespace LS.SPWorkflowProcessor
{
    class NintexWFSettingsProcessor
    {
        private IAveWeb web;
        private bool isPostAction;
        public NintexWFSettingsProcessor(IAveWeb aveWeb, bool isPostAction)
        {
            this.web = aveWeb;
            this.isPostAction = isPostAction;
        }

        public WorkflowSettings GetWorkflowSettingContent(NWActionConfig rootAction)
        {
            return new WorkflowSettings
            {
                Title = GetStringValue(rootAction.Parameters, "WorkflowName", string.Empty),
                Description = GetStringValue(rootAction.Parameters, "WorkflowDescription", string.Empty),
                HistoryListId = GetListTitle(rootAction.Parameters, "HistoryListName", "Workflow History", false),
                TaskListId = GetListTitle(rootAction.Parameters, "TaskListId", "Workflow Tasks", true),
                StartManually = GetBoolValue(rootAction.Parameters, "StartManually", true),
                StartOnChange = GetBoolValue(rootAction.Parameters, "StartOnChange", false),
                StartOnCreate = GetBoolValue(rootAction.Parameters, "StartOnCreate", false),
            };
        }

        public static void CheckAutoStartOption(NWActionConfig rootAction)
        {
            if (GetBoolValue(rootAction.Parameters, "StartOnChange", false) || GetBoolValue(rootAction.Parameters, "StartOnCreate", false))
            {
                throw new NWNeedPostActionException("auto start option is true, restore wf in post action");
            }
        }

        private static string GetStringValue(ActivityParameter[] parameters, string parameterName, string defaultValue)
        {
            var parameter = parameters.FirstOrDefault(para => string.Equals(para.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            if (parameter == null)
            {
                return defaultValue;
            }
            return parameter.PrimitiveValue.Value;
        }
        private Guid CreateList(string title, Guid featureId, int baseTemplate)
        {
            return this.web.Lists.Add(title, "", title, featureId.ToString(), baseTemplate, null, AveQuickLaunchOptions.Off);
        }
        private void CreateWorkflowHistoryList(string historyListTitle)
        {
            var historyListId = CreateList(historyListTitle, new Guid("00bfea71-4ea5-48d4-a4ad-305cf7030140"), 140);
            var historyList = this.web.Lists.GetById(historyListId);
            historyList.Hidden = true;
            historyList.Update();
        }

        private void CreateWorkflowTaskList(string taskListTitle)
        {
            CreateList(taskListTitle, new Guid("f9ce21f8-f437-4f7e-8bc6-946378c850f0"), 171);
        }

        private string GetListTitle(ActivityParameter[] parameters, string parameterName, string defaultValue, bool isTask)
        {
            var result = GetStringValue(parameters, parameterName, defaultValue);
            if (IsListExist(result))
            {
                return result;
            }
            else if (isPostAction)
            {
                if (isTask)
                {
                    CreateWorkflowTaskList(result);
                }
                else
                {
                    CreateWorkflowHistoryList(result);
                }
                return result;
            }
            throw new NWListNotFoundException(result);
        }

        private bool IsListExist(string listTitle)
        {
            return this.web.GetListByName(listTitle, false) != null;
        }

        private static bool GetBoolValue(ActivityParameter[] parameters, string parameterName, bool defaultValue)
        {
            return string.Equals(GetStringValue(parameters, parameterName, defaultValue.ToString()), bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
