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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    enum StartTaskProcessApproveType
    {
        WaitForAllResponses = 0,
        WaitForFirstResponse = 1,
        WaitForSpecificResponse = 2,
        WaitForPercentageOfAResponse = 3,
    }

    abstract class NWStartATaskProcessActionBase : NWActionProcessorBase
    {
        protected StateConfiguration stateConfiguration;
        protected int defaultValue;
        protected const string Approved = "Approved";
        protected const string Rejected = "Rejected";
        protected NWRequestActionUtility requestActionUtility;
        protected bool useDefaultOutcome = true;
        protected readonly List<string> parametersOrder = new List<string> {
            "AssignedTo", "DueDate", "Title", "Body", "RelatedContentLinkListId", "RelatedContentLinkListItemId", "ContentTypeId", "OutcomeFieldName",
            "CompletedStatus", "WaitForTaskCompletion", "SendReminderEmail", "DefaultTaskOutcome", "OverdueReminderRepeat", "OverdueRepeatTimes",
            "AssignmentEmailSubject", "AssignmentEmailBody", "OverdueEmailSubject", "OverdueEmailBody", "CancelationEmailSubject", "CancelationEmailBody",
            "ExpandGroup", "ParallelAssignment", "CompletionCriteria", "CompletionCriteriaProperties", "AllowLazyApproval", "TaskFormEdit", "EscalationType",
            "EscalationDate", "EscalationDateCalculationUnit", "EscalationDateCalculationValue", "EscalationOutcome", "EscalationTo", "EscalationCC",
            "EscalationEmailSubject", "EscalationEmailBody", "PreserveIncompleteTasks", "WaiveAssignmentEmail", "WaiveCancelationEmail"
        };

        public NWStartATaskProcessActionBase(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.CompositeTask";
            requestActionUtility = new NWRequestActionUtility();
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374446",
                ClassName = CLASSNAME,
                x49x49 = 294,
                y49x49 = 237,
                x30x30 = 294,
                y30x30 = 286,
                x16x16 = 327,
                y16x16 = 286
            };
        }

        protected abstract void InitializeData(NWActionConfig nwActionConfig);

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            InitializeData(nwActionConfig);
            var workflowAction = base.UpgradeWorkflowAction(nwActionConfig);
            workflowAction.Children = GenerateChildrenWorkflowAction(nwActionConfig.ChildActivities);
            return workflowAction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interestedOutcomeValue"></param>
        /// <param name="percentageValue">如果percentageValue不在0~100以内 即表示没有Percentage</param>
        /// <returns></returns>
        protected DictionaryValue[] GetCompletionCriteriaDictionaryValue(string interestedOutcomeValue, int percentageValue)
        {
            var dictionaryValues = new List<DictionaryValue>();
            dictionaryValues.Add(new DictionaryValue
            {
                Key = "InterestedOutcome",
                Value = new Value
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Int32",
                        Value = new Value(interestedOutcomeValue),
                    }
                }
            });
            if (percentageValue > 0 && percentageValue <= 100)
            {
                dictionaryValues.Add(new DictionaryValue
                {
                    Key = "Percentage",
                    Value = new Value
                    {
                        PrimitiveValue = new PrimitiveValue
                        {
                            Type = "Int32",
                            Value = new Value(percentageValue.ToString())
                        }
                    }
                });

            }
            return dictionaryValues.ToArray();
        }

        protected abstract List<WorkflowAction> GenerateChildrenWorkflowAction(NWActionConfig[] childActivities);

        protected override List<Property> CreateProperties()
        {
            return new List<Property>
            {
                new Property
                {
                    ID="p0",
                    DesignerType="CompositeTaskAction",
                    DisplayName="Process Settings",
                    Parameters = CreateParameters(),
                }
            };
        }

        protected virtual Parameters[] CreateParameters()
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.Add(CreateAssignToParameter());
            parameters.Add(CreateDueDateParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskDueDate", true)));
            parameters.Add(CreateTitleParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskName", false)));
            parameters.Add(CreateDescriptionParameter(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "TaskDescription", true)));
            parameters.Add(CreateCompletionCriteriaParameter());
            parameters.Add(CreateCompletionCriteriaPropertiesParameter());
            parameters.Add(CreateExpandGroupParameter());
            parameters.Add(requestActionUtility.CreateRelatedContentTypeIdParameter(useDefaultOutcome));
            parameters.Add(requestActionUtility.CreateOutcomeFieldNameParameter(useDefaultOutcome));
            parameters.Add(requestActionUtility.CreateDefaultTaskOutcomeParameter(defaultValue.ToString()));

            parameters.AddRange(CreateTaskNotificationRelevantParameters());
            parameters.AddRange(CreateNotRequiredNotificationRelevantParameters());
            parameters.AddRange(CreateRemindersRelevantParameters());
            parameters.AddRange(CreateEscalationRelevantParameters());

            parameters.AddRange(requestActionUtility.GetCommonNoNeedConvertParameters(base.workflowActionProcessor.List == null));

            parameters = SortParametersList(parameters);
            return parameters.ToArray();
        }

        protected List<Parameters> CreateTaskNotificationRelevantParameters()
        {
            List<Parameters> parameters = new List<Parameters>();

            parameters.Add(CreateAssignmentEmailSubjectParameter());
            parameters.Add(CreateAssignmentEmailBodyParameter());
            return parameters;
        }

        private Parameters CreateAssignmentEmailSubjectParameter()
        {
            string subject = requestActionUtility.GetApprovalMsg(sourceConfig, true).Subject;
            List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
            subject = NWCommonUtility.ReplaceNintexWorkflowContent(subject, ref references);
            List<FormatValues> formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false);

            return new Parameters
            {
                Name = "AssignmentEmailSubject",
                Description = "Text for the subject of the email that gets sent out when a task is created.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(subject),
                        FormatValues = formatValuesList
                    }
                },
            };
        }

        private Parameters CreateAssignmentEmailBodyParameter()
        {
            string message = requestActionUtility.GetApprovalMsg(sourceConfig, true).Body;
            List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
            message = NWCommonUtility.ReplaceNintexWorkflowContent(message, ref references);
            List<FormatValues> formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false, true);
            message = requestActionUtility.HandleEmailBodyContent(message, references, ref formatValuesList);

            return new Parameters
            {
                Name = "AssignmentEmailBody",
                Description = "Text for the body of the email that gets sent out when a task is created.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(message),
                        FormatValues = formatValuesList
                    }
                },
            };
        }

        protected List<Parameters> CreateNotRequiredNotificationRelevantParameters()
        {
            List<Parameters> parameters = new List<Parameters>();

            parameters.Add(CreateCancelationEmailSubjectParameter());
            parameters.Add(CreateCancelationEmailBodyParameter());
            return parameters;
        }

        private Parameters CreateCancelationEmailSubjectParameter()
        {
            string subject = requestActionUtility.GetApprovalMsg(sourceConfig, false).Subject;
            List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
            subject = NWCommonUtility.ReplaceNintexWorkflowContent(subject, ref references);
            List<FormatValues> formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false);

            return new Parameters
            {
                Name = "CancelationEmailSubject",
                Description = "Text for the subject of the email that gets sent out when a task is canceled.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(subject),
                        FormatValues = formatValuesList
                    }
                },
            };
        }

        private Parameters CreateCancelationEmailBodyParameter()
        {
            string message = requestActionUtility.GetApprovalMsg(sourceConfig, false).Body;
            List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
            message = NWCommonUtility.ReplaceNintexWorkflowContent(message, ref references);
            List<FormatValues> formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false, true);
            message = requestActionUtility.HandleEmailBodyContent(message, references, ref formatValuesList);

            return new Parameters
            {
                Name = "CancelationEmailBody",
                Description = "Text for the body of the email that gets sent out when a task is canceled.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(message),
                        FormatValues = formatValuesList
                    }
                },
            };
        }

        protected List<Parameters> CreateRemindersRelevantParameters()
        {
            List<Parameters> parameters = new List<Parameters>();

            parameters.Add(requestActionUtility.CreateSendReminderEmailParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateOverdueReminderRepeatParameter());
            parameters.Add(requestActionUtility.CreateOverdueRepeatTimesParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateOverdueEmailSubjectParameter(workflowActionProcessor, sourceConfig, false));
            parameters.Add(requestActionUtility.CreateOverdueEmailBodyParameter(workflowActionProcessor, sourceConfig, false));
            return parameters;
        }

        protected List<Parameters> CreateEscalationRelevantParameters()
        {
            List<Parameters> parameters = new List<Parameters>();

            parameters.Add(requestActionUtility.CreateEscalationTypeParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationDateParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationDateCalculationUnitParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationDateCalculationValueParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationOutcomeParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationToParameter(workflowActionProcessor, sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationCCParameter(sourceConfig, false));
            parameters.Add(requestActionUtility.CreateEscalationEmailSubjectParameter());
            parameters.Add(requestActionUtility.CreateEscalationEmailBodyParameter());
            return parameters;
        }

        protected Parameters CreateTitleParameter(ActivityParameter taskNameParameter)
        {
            return new Parameters
            {
                Name = "Title",
                Description = "Used to define what string will appear in the title of the task.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue { PrimitiveValue = GetTitlePrimitiveValue(taskNameParameter) },
            };
        }

        private PrimitiveValue GetTitlePrimitiveValue(ActivityParameter taskNameParameter)
        {
            if (taskNameParameter == null || string.IsNullOrEmpty(taskNameParameter.PrimitiveValue.Value))
            {
                return new PrimitiveValue { Type = "String", Value = new Value(base.sourceConfig.TLabel) };
            }
            return NWPrimitiveValueConverter.ConvertPrimitiveValue(taskNameParameter.PrimitiveValue, base.workflowActionProcessor, true);
        }

        protected Parameters CreateDescriptionParameter(ActivityParameter descriptionParameter)
        {
            //Source Description is html, destination is text.
            var description = AveHtmlUtility.ConvertHtmlToText(descriptionParameter.PrimitiveValue.Value);

            return new Parameters
            {
                Name = "Body",
                Description = "The description of the task.",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(description, "Text", base.workflowActionProcessor, false) },
            };
        }

        protected abstract int GetCompletionCriteriaValue();

        protected abstract DictionaryValue[] GetCompletionCriteriaDictionaryValue();

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.StateConfiguration = stateConfiguration;
            return configuration;
        }

        protected Parameters CreateCompletionCriteriaPropertiesParameter()
        {
            return new Parameters
            {
                Name = "CompletionCriteriaProperties",
                Required = false,
                DataType = "Dictionary",
                Direction = "Input",
                Value = new ParametersValue
                {
                    Dictionary = GetCompletionCriteriaDictionaryValue(),
                },
            };
        }

        protected Parameters CreateExpandGroupParameter()
        {
            return new Parameters
            {
                Name = "ExpandGroup",
                Description = "When set to true, all groups that are referenced in the assignee field will be expanded, and every user in the group will receive a task.",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value(TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "ExpandGroups", false), "False")),
                    }
                },
            };
        }

        protected Parameters CreateCompletionCriteriaParameter()
        {
            return new Parameters
            {
                Name = "CompletionCriteria",
                Description = "This property cannot be set within the property grid.",
                Required = true,
                DataType = "Int32",
                DesignerType = "Dependent",
                DependentOn = "ParallelAssignment",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Int32",
                        Value = new Value((GetCompletionCriteriaValue()).ToString()),
                    }
                },
            };
        }

        protected Parameters CreateAssignToParameter()
        {
            return new Parameters
            {
                Name = "AssignedTo",
                Description = "SharePoint aliases of the users or groups to whom the tasks will be assigned to.",
                DataType = "Collection",
                Required = true,
                Direction = "Input",
                Value = CreateApproversValue(),
            };
        }

        private ParametersValue CreateApproversValue()
        {
            ParametersValue approvers = new ParametersValue { Collection = new Collection { SelectedValue = new List<SelectedValue>() } };
            foreach (NWApprover approver in base.sourceConfig.Approvers)
            {
                if (!string.IsNullOrEmpty(approver.User))
                {
                    var selectedValue = NWUserConverter.ConvertUserToSelectedValue(base.workflowActionProcessor, approver.User);
                    approvers.Collection.SelectedValue.Add(selectedValue);
                }
            }
            return approvers;
        }

        protected Parameters CreateDueDateParameter(ActivityParameter dueDateParameter)
        {
            return new Parameters
            {
                Name = "DueDate",
                Description = "The date by which the task must be completed by.After which, the task becomes overdue.",
                Required = false,
                DataType = "DateTime",
                DesignerType = "DateTime",
                Direction = "Input",
                Value = base.ConvertParameterValue(dueDateParameter),
            };
        }

        /// <summary>
        /// 只是为了以后分析问题方便，调整parameters
        /// </summary>
        /// <param name="parametersList"></param>
        /// <returns></returns>
        protected List<Parameters> SortParametersList(List<Parameters> parametersList)
        {
            List<Parameters> newParametersList = new List<Parameters>();
            foreach (string parametersName in parametersOrder)
            {
                foreach (Parameters p in parametersList)
                {
                    if (parametersName.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        newParametersList.Add(p);
                        break;
                    }
                }
            }

            return newParametersList;
        }
    }
}
