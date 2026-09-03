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
using System.Text;
using System.Text.RegularExpressions;

namespace LS.SPWorkflowProcessor
{
    class NWRequestActionUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(NWRequestActionUtility));
        public T GetApprovalType<T>(NWActionConfig nwActionConfig, string parameterName)
        {
            var approval = nwActionConfig.Parameters.First(para => string.Equals(para.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            return (T)Enum.Parse(typeof(T), approval.PrimitiveValue.Value);
        }

        public Message GetApprovalMsg(NWActionConfig sourceConfig, bool required)
        {
            if (required)
            {
                return sourceConfig.Approvers.First(approver => approver.PrincipalType == AvePoint.Wrapper.Common.AvePrincipalType.None).ApprovalRequiredMsg;
            }
            else
            {
                return sourceConfig.Approvers.First(approver => approver.PrincipalType == AvePoint.Wrapper.Common.AvePrincipalType.None).ApprovalNotRequiredMsg;
            }
        }

        public string HandleEmailBodyContent(string oldContent, List<KeyValuePair<string, bool>> references, ref List<FormatValues> formatValuesList)
        {
            if (formatValuesList == null)
            {
                return oldContent;
            }

            List<int> needKeepedFormatValuesList = new List<int>();
            for (int i = 0; i < formatValuesList.Count; i++)
            {
                if (formatValuesList[i].SelectedValue.PrimitiveValue != null && !references[i].Value)
                {
                    oldContent = oldContent.Replace(string.Format("{{{0}}}", i), formatValuesList[i].SelectedValue.PrimitiveValue.Value.StringValue);
                }
                else
                {
                    needKeepedFormatValuesList.Add(i);
                }
            }

            if (needKeepedFormatValuesList.Count != formatValuesList.Count)
            {
                List<FormatValues> newFormatValuesList = new List<FormatValues>();
                int counter = 0;
                foreach (int index in needKeepedFormatValuesList)
                {
                    oldContent = oldContent.Replace(string.Format("{{{0}}}", index), string.Format("{{{0}}}", counter));
                    newFormatValuesList.Add(formatValuesList[index]);
                    counter++;
                }
                formatValuesList = newFormatValuesList;
            }

            return oldContent;
        }

        /// <summary>
        /// useDefault is true:
        /// On-Premise上没有找到对应的属性，但是这个Parameter是必填项，
        /// 因此把Online上的默认值作为值设置
        /// ContentType对应Online上的built-in ContentType:Workflow Task (SharePoint 2013)
        /// useDefault is false:
        /// 需要自定义outcomes 对应online上的 Define outcomes for task
        /// </summary>
        /// <returns></returns>
        public Parameters CreateRelatedContentTypeIdParameter(bool useDefault)
        {
            string id = useDefault ? "0x0108003365C4474CAE8C42BCE396314E88E51F" : "A8F63F64-5077-49A4-B57A-6F870E75460F";

            return new Parameters
            {
                Name = "ContentTypeId",
                Description = "Defines the content type the task will use when it is created.",
                Required = true,
                DataType = "String",
                Direction = "Input",
                Value = new ParametersValue { ContentType = new ContentType { Id = id } },
            };
        }

        /// <summary>
        /// useDefault is true:
        /// On-Premise上没有找到对应的属性，但是这个Parameter是必填项，
        /// 因此把Online上的默认值作为值设置
        /// ID对应Online上的built-in ContentType Workflow Task (SharePoint 2013)中的Field:Task Outcome
        /// useDefault is false:
        /// 需要自定义outcomes 对应online上的 Define outcomes for task
        /// </summary>
        /// <returns></returns>
        public Parameters CreateOutcomeFieldNameParameter(bool useDefault)
        {
            string valueStr = useDefault ? "TaskOutcome" : "FEE5CD0D-AF7C-431F-A41F-86079E81AA07";

            return new Parameters
            {
                Name = "OutcomeFieldName",
                Description = "Defines which field within the content type is the Outcome field.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value(valueStr) }
                },
            };
        }

        /// <summary>
        /// 获取Start a task process action中不需要convert的parameter
        /// </summary>
        /// <returns></returns>
        public List<Parameters> GetCommonNoNeedConvertParameters(bool isWebLevelWorkflow)
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.Add(CreateRelatedContentLinkListIdParameter(isWebLevelWorkflow));
            parameters.Add(CreateRelatedRelatedContentLinkListItemIdParameter(isWebLevelWorkflow));
            parameters.Add(CreateParallelAssignmentParameter());
            parameters.Add(CreateCompletedStatusParameter());
            parameters.Add(CreateWaitForTaskCompletionParameter());
            parameters.Add(CreateAllowLazyApprovalParameter());
            parameters.Add(CreateTaskFormEdit());
            parameters.Add(CreatePreserveIncompleteTasksParameter());
            parameters.Add(CreateWaiveAssignmentEmailParameter());
            parameters.Add(CreateWaiveCancelationEmailParameter());

            return parameters;
        }

        public Parameters CreateSendReminderEmailParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            string valueStr = useDefault ? "False" : (NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "RemindersRequired", false), "0") != "0").ToString();

            return new Parameters
            {
                Name = "SendReminderEmail",
                Description = "Set to true if you want to have a reminder email sent out when the task becomes overdue.",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value(valueStr) }
                },
            };
        }

        #region On-Premise 没有的Parameters

        private Parameters CreateRelatedContentLinkListIdParameter(bool isWebLevelWorkflow)
        {
            Parameters parameters = new Parameters
            {
                Name = "RelatedContentLinkListId",
                Required = false,
                DataType = "Guid",
                Direction = "Input",
                Value = new ParametersValue()
            };

            if (!isWebLevelWorkflow) //List Level
            {
                parameters.DesignerType = "None";
                parameters.Value = new ParametersValue
                {
                    ListLookup = new ListLookup
                    {
                        SelectList = "[Current Item]",
                        SelectField = string.Empty,
                        SelectFieldType = string.Empty,
                        WhereField = string.Empty,
                        WhereFieldType = string.Empty,
                        DisplayName = string.Empty,
                        DisplayValue = string.Empty,
                    }
                };
            }

            return parameters;
        }

        private Parameters CreateRelatedRelatedContentLinkListItemIdParameter(bool isWebLevelWorkflow)
        {
            Parameters parameters = new Parameters
            {
                Name = "RelatedContentLinkListItemId",
                Description = "References an item that the task is associated to.",
                Required = false,
                DataType = "Guid",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!isWebLevelWorkflow) //ListLevel
            {
                parameters.Required = true;
                parameters.DesignerType = "None";
                parameters.Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Guid",
                        Value = new Value("[Unique Id]"),
                    }
                };
            }

            return parameters;
        }

        private Parameters CreateCompletedStatusParameter()
        {
            return new Parameters
            {
                Name = "CompletedStatus",
                Required = true,
                DataType = "String",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value("Completed") }
                },
            };
        }

        private Parameters CreateWaitForTaskCompletionParameter()
        {
            return new Parameters
            {
                Name = "WaitForTaskCompletion",
                Description = "When set to true, this will cause the workflow to pause until the task completes.",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value("True") }
                },
            };
        }

        public Parameters CreateDefaultTaskOutcomeParameter(string defaultValue)
        {
            return new Parameters
            {
                Name = "DefaultTaskOutcome",
                Description = "Used to indicate what the outcome should be if the task does not complete successfully.",
                Required = true,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Int32", Value = new Value(defaultValue) }
                },
            };
        }

        public Parameters CreateOverdueReminderRepeatParameter()
        {
            return new Parameters
            {
                Name = "OverdueReminderRepeat",
                Description = "Indicates how often the action should send the reminder. 0:None, 1:Daily, 2:Weekly and 3:Monthly.",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Int32", Value = new Value("1") }
                },
            };
        }

        public Parameters CreateOverdueRepeatTimesParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            string valueStr = useDefault ? "1" : NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "RemindersRequired", false), "1");

            return new Parameters
            {
                Name = "OverdueRepeatTimes",
                Description = "Indicates how many times the overdue reminder action should be executed.",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Int32", Value = new Value(valueStr) }
                },
            };
        }

        public Parameters CreateOverdueEmailSubjectParameter(NintexWFActionProcessor workflowActionProcessor, NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "OverdueEmailSubject",
                Description = "Text for the subject of the email that gets sent out when a task is overdue.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("Task Overdue - {0}"),
                        FormatValues = new List<FormatValues>
                            {
                                NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("Title","String"),
                            }
                    }
                },
            };

            if (!useDefault)
            {
                string subject = string.Empty;
                List<FormatValues> formatValuesList = null;
                if (sourceConfig.Message != null)
                {
                    subject = sourceConfig.Message.Subject;
                    List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
                    subject = NWCommonUtility.ReplaceNintexWorkflowContent(subject, ref references);
                    formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false);
                }

                parameters.Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(string.IsNullOrEmpty(subject) ? "Reminder Subject" : subject),
                        FormatValues = formatValuesList
                    }
                };
            }

            return parameters;
        }

        public Parameters CreateOverdueEmailBodyParameter(NintexWFActionProcessor workflowActionProcessor, NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "OverdueEmailBody",
                Description = "Text for the body of the email that gets sent out when a task is overdue.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("<div><span style=\"font-size:13.5pt;\">You have an </span><span style=\"font-size:13.5pt;color:#ff3b3b;\">overdue</span><span style=\"font-size:13.5pt;\"> task.</span></div><div><span>&nbsp;</span></div><a #\"\"=\"\" href=\"{0}\">{2}</a><div><span>&nbsp;</span></div><table><tbody><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top;padding-top:2px;\">Assigned To</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\">{4}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#ff3b3b;white-space:nowrap;padding-right:15px;vertical-align:top;padding-top:2px;\">Due Date</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#ff3b3b;vertical-align:top;\">{5}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top;padding-top:2px;\">Description</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\">{6}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top;padding-top:2px;\">Related Item</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\"><a #\"\"=\"\" href=\"{1}\">{3}</a></td></tr></tbody></table><div></div>"),
                        FormatValues = new List<FormatValues>
                            {
                                NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("TaskUrl","String"),
                                NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("RelatedItemUrl","String"),
                                NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("Title","String"),
                                NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("RelatedItemTitle","String"),
                                NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("AssignedTo","String"),
                                NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("DueDate","String"),
                                NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("Body","String"),
                            }
                    }
                },
            };

            if (!useDefault)
            {
                string message = string.Empty;
                List<FormatValues> formatValuesList = null;
                if (sourceConfig.Message != null)
                {
                    message = sourceConfig.Message.Body;
                    List<KeyValuePair<string, bool>> references = new List<KeyValuePair<string, bool>>();
                    message = NWCommonUtility.ReplaceNintexWorkflowContent(message, ref references);
                    formatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(references, workflowActionProcessor, false, true);
                    message = HandleEmailBodyContent(message, references, ref formatValuesList);
                }

                parameters.Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value(string.IsNullOrEmpty(message) ? "Reminder body" : message),
                        FormatValues = formatValuesList
                    }
                };
            }

            return parameters;
        }

        public Parameters CreateEscalationTypeParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            string escalationType = "0";

            if (!useDefault)
            {
                if (NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false) != null)
                {
                    if (NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false).PrimitiveValue.Value.Equals("DelegateTask", StringComparison.OrdinalIgnoreCase))
                    {
                        escalationType = "1";
                    }
                    else if (NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false).PrimitiveValue.Value.Equals("CompleteTask", StringComparison.OrdinalIgnoreCase))
                    {
                        escalationType = "2";
                    }
                }
            }

            return new Parameters
            {
                Name = "EscalationType",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Int32",
                        Value = new Value(escalationType),
                    }
                },
            };
        }

        public Parameters CreateEscalationDateParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationDate",
                Required = false,
                DataType = "DateTime",
                DesignerType = "DateTime",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                parameters.Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "DateTime",
                        Value = new Value(new DateTimeInfo()
                        {
                            UseCurrentDate = true
                        })
                    }
                };
            }

            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0 is None, 1 is Business days,2 is Business hours,3 is Business minutes, 4 is Calendar days,5 is Calendar hours,6 is Calendar minutes</returns>
        private int GetEscalationDateCalculationUnit(NWActionConfig sourceConfig)
        {
            int escalationDateCalculationUnit = 0;
            string escalationWaitMode = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationWaitMode", false), "None");
            string daysEscalation = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DaysEscalation", false), "0");
            string hoursEscalation = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "HoursEscalation", false), "0");
            string minutesEscalation = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "MinutesEscalation", false), "0");
            if (escalationWaitMode.Equals("BusinessDaysOnly", StringComparison.OrdinalIgnoreCase)
                || escalationWaitMode.Equals("BusinessHoursOnly", StringComparison.OrdinalIgnoreCase))
            {
                if (daysEscalation != "0")
                {
                    escalationDateCalculationUnit = 1; //Business days
                }
                if (hoursEscalation != "0")
                {
                    escalationDateCalculationUnit = 2; //Business hours
                }
                if (minutesEscalation != "0")
                {
                    escalationDateCalculationUnit = 3; //Business minutes
                }
            }
            else
            {
                if (daysEscalation != "0")
                {
                    escalationDateCalculationUnit = 4; //Calendar days
                }
                if (hoursEscalation != "0")
                {
                    escalationDateCalculationUnit = 5; //Calendar hours
                }
                if (minutesEscalation != "0")
                {
                    escalationDateCalculationUnit = 6; //Calendar minutes
                }
            }

            return escalationDateCalculationUnit;
        }

        public Parameters CreateEscalationDateCalculationUnitParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationDateCalculationUnit",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                int escalationDateCalculationUnit = GetEscalationDateCalculationUnit(sourceConfig);

                parameters.Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "Int32",
                        Value = new Value(escalationDateCalculationUnit.ToString())
                    }
                };
            }

            return parameters;
        }

        public Parameters CreateEscalationDateCalculationValueParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationDateCalculationValue",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                int escalationDateCalculationUnit = GetEscalationDateCalculationUnit(sourceConfig);
                int escalationDateCalculationValue = 0;
                if (escalationDateCalculationUnit == 1 || escalationDateCalculationUnit == 4)
                {
                    escalationDateCalculationValue = Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DaysEscalation", false), "0"));
                }
                else if (escalationDateCalculationUnit == 2 || escalationDateCalculationUnit == 5)
                {
                    escalationDateCalculationValue = Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "HoursEscalation", false), "0")) + Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DaysEscalation", false), "0")) * 24;
                }
                else if (escalationDateCalculationUnit == 3 || escalationDateCalculationUnit == 6)
                {
                    escalationDateCalculationValue = Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "MinutesEscalation", false), "0")) + Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "HoursEscalation", false), "0")) * 60 + Convert.ToInt32(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DaysEscalation", false), "0")) * 24 * 60;
                }

                parameters.Required = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false), "None").Equals("None", StringComparison.OrdinalIgnoreCase) ? false : true;
                parameters.Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "Int32",
                        Value = new Value(escalationDateCalculationValue.ToString())
                    }
                };
            }

            return parameters;
        }

        public Parameters CreateEscalationOutcomeParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationOutcome",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                ParametersValue pv = new ParametersValue { PrimitiveValue = new PrimitiveValue("", "") };
                if (!string.IsNullOrEmpty(NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationAutoOutcome", false), "")))
                {
                    ActivityParameter escalationAutoOutcome = NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationAutoOutcome", false);
                    if (sourceConfig.Outcomes.FirstOrDefault(outcome => outcome.Name.Equals(escalationAutoOutcome.PrimitiveValue.Value, StringComparison.OrdinalIgnoreCase)) != null)
                    {
                        pv.PrimitiveValue.Type = "Int32";
                        pv.PrimitiveValue.Value = new Value(sourceConfig.Outcomes.FirstOrDefault(outcome => outcome.Name.Equals(escalationAutoOutcome.PrimitiveValue.Value, StringComparison.OrdinalIgnoreCase)).BranchIndex.ToString());
                    }
                    else
                    {
                        logger.Warn("Can not support EscalationOutcome value, cannot support value is {0}.", escalationAutoOutcome.PrimitiveValue.Value);
                        pv.PrimitiveValue.Type = "Int32";
                        pv.PrimitiveValue.Value = new Value("0");
                    }
                }

                parameters.Value = pv;
            }

            return parameters;
        }

        public Parameters CreateEscalationToParameter(NintexWFActionProcessor workflowActionProcessor, NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationTo",
                Required = false,
                DataType = "String",
                DesignerType = "User",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                ParametersValue parametersValue = new ParametersValue();
                if (NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DelegateTo", false) != null)
                {
                    parametersValue = NWUserConverter.ConvertPrimitiveValueToParametersValue(workflowActionProcessor, NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "DelegateTo", false).PrimitiveValue);
                }

                parameters.Required = NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false), "None").Equals("None", StringComparison.OrdinalIgnoreCase) ? false : true;
                parameters.Value = parametersValue;
            }

            return parameters;
        }

        public Parameters CreateEscalationCCParameter(NWActionConfig sourceConfig, bool useDefault)
        {
            Parameters parameters = new Parameters
            {
                Name = "EscalationCC",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue(),
            };

            if (!useDefault)
            {
                ParametersValue pv = new ParametersValue();

                if (NWCommonUtility.TryGetTheValueOfPrimitiveValue(NWCommonUtility.GetActivityParameterByName(sourceConfig.Parameters, "EscalationType", false), "None").Equals("DelegateTask", StringComparison.OrdinalIgnoreCase))
                {
                    pv.PrimitiveValue = new PrimitiveValue("Boolean", "False");
                }

                parameters.Value = pv;
            }

            return parameters;
        }

        public Parameters CreateEscalationEmailSubjectParameter()
        {
            return new Parameters
            {
                Name = "EscalationEmailSubject",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("Task Escalated - {0}"),
                        FormatValues = new List<FormatValues>
                        {
                            NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("Title","String"),
                        }
                    }
                },
            };
        }

        public Parameters CreateEscalationEmailBodyParameter()
        {
            return new Parameters
            {
                Name = "EscalationEmailBody",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("<html><body style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;\"><div><span style=\"font-size:13.5pt\">Task has been escalated:</span></div><div><span>&nbsp;</span></div><a style=\"font-size:21pt;color:#0066cc;\" href=\"{0}\">{2}</a><div><span>&nbsp;</span></div><table><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top; padding-top:2px;\">Originally Assigned To</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\">{4}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top; padding-top:2px;\">Due Date</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\">{5}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top; padding-top:2px;\">Description</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\">{6}</td></tr><tr><td style=\"font-size:10pt;text-transform:uppercase;font-family:Segoe UI Light,sans-serif;color:#444444;white-space:nowrap;padding-right:15px;vertical-align:top; padding-top:2px;\">Related Item</td><td style=\"font-size:11pt;font-family:Segoe UI Light,sans-serif;color:#444444;vertical-align:top;\"><a href=\"{1}\" class=\"link\">{3}</a></td></tr></table></body></html><div></div>"),
                        FormatValues = new List<FormatValues>
                        {
                            NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("TaskUrl","String"),
                            NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("RelatedItemUrl","String"),
                            NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("Title","String"),
                            NWRequestActionUtility.CreateFormatValuesWithPrimitiveValue("RelatedItemTitle","String"),
                            NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("AssignedTo","String"),
                            NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("DueDate","String"),
                            NWRequestActionUtility.CreateFormatValuesWithoutPrimitiveValue("Body","String"),
                        }
                    }
                },
            };
        }

        public static FormatValues CreateFormatValuesWithoutPrimitiveValue(string taskPropertyValue, string taskPropertyType)
        {
            return new FormatValues
            {
                SelectedValue = new SelectedValue
                {
                    Coercion = "AsDNString",
                    TaskProperty = new TaskProperty
                    {
                        Value = taskPropertyValue,
                        Type = taskPropertyType,
                    },
                }
            };
        }

        public static FormatValues CreateFormatValuesWithPrimitiveValue(string taskPropertyValue, string taskPropertyType)
        {
            return new FormatValues
            {
                SelectedValue = new SelectedValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("{0}"),
                        FormatValues = new List<FormatValues>
                    {
                        new FormatValues
                        {
                            SelectedValue = new SelectedValue
                            {
                                Coercion="AsDNString",
                                TaskProperty=new TaskProperty
                                {
                                    Value =taskPropertyValue,
                                    Type =taskPropertyType
                                }
                            },
                        }
                    }
                    }
                }
            };
        }

        private Parameters CreateParallelAssignmentParameter()
        {
            return new Parameters
            {
                Name = "ParallelAssignment",
                Description = "When set to true, the tasks will all be assigned out at once. If set to false, the tasks will be assigned one at a time.",
                Required = false,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("True"),
                    }
                },
            };
        }

        private Parameters CreateTaskFormEdit()
        {
            return new Parameters
            {
                Name = "TaskFormEdit",
                Required = false,
                DataType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("True"),
                    }
                }
            };
        }

        private Parameters CreateAllowLazyApprovalParameter()
        {
            return new Parameters
            {
                Name = "AllowLazyApproval",
                Required = false,
                DataType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    }
                },
            };
        }

        private Parameters CreatePreserveIncompleteTasksParameter()
        {
            return new Parameters
            {
                Name = "PreserveIncompleteTasks",
                Required = false,
                Description = "Set to true if you want non-completed tasks to be deleted when the task process is complete.",
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    }
                },
            };
        }

        private Parameters CreateWaiveAssignmentEmailParameter()
        {
            return new Parameters
            {
                Name = "WaiveAssignmentEmail",
                Required = false,
                Description = "Set to false if you want to have an email sent out to the assignee when a task is created.",
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    }
                },
            };
        }

        private Parameters CreateWaiveCancelationEmailParameter()
        {
            return new Parameters
            {
                Name = "WaiveCancelationEmail",
                Required = false,
                Description = "Set to false if you want to have an email sent out to the assignee when a task is canceled.",
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    }
                },
            };
        }
        #endregion
    }
}
