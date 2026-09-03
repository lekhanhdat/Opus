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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AvePoint.Hybrid.AgentContract.Rule
{
    public class ArchiverRuleFilter
    {
        /// <summary>
        /// Initializes a new instance of the ArchiverRuleFilter class.
        /// </summary>
        public ArchiverRuleFilter()
        {
            this.Dto = new SOFilterPolicy()
            {
                Value = new PolicyValue(string.Empty, PolicyValueUnit.None, string.Empty, PolicyValueUnit.None)
            };
            //this.RuleType = ArchiverFilterRuleType.TextColumn;
            this.Condition = ArchiverFilterCondition.Equals;
            this.CombineMode = ArchiverFilterCombineMode.Or;
        }
        public ArchiverRuleFilter(SOFilterPolicy dto)
        {
            this.Dto = dto;
        }

        #region Mapping
        private static Dictionary<ArchiverFilterRuleType, List<ArchiverFilterCondition>> FilterRuleAndConditionMapping = new Dictionary<ArchiverFilterRuleType, List<ArchiverFilterCondition>>()
        {
            {
                ArchiverFilterRuleType.Name,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.Size,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo
                }
            },
            {
                ArchiverFilterRuleType.ModifiedTime,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.ModifiedBy,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.CreatedTime,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.CreatedBy,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.ContentType,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.TextColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.MetadataTextColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.NumberColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo,
                    ArchiverFilterCondition.Equals
                }
            },
                        {
                ArchiverFilterRuleType.MetadataNumberColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.BooleanColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.DateTimeColumn,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.ParentListTypeID,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.LastAccessedTime,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.Title,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.DocumentSize,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo
                }
            },
            {
                ArchiverFilterRuleType.KeepTheLatestVersion,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.MajorVersions,
                    ArchiverFilterCondition.MajorAndMinorVersions,
                    ArchiverFilterCondition.MajorVersionsNoMinor,
                    ArchiverFilterCondition.MinorVersionsOfEachMajor,
                    ArchiverFilterCondition.MinorVersionsOfTheLatestMajor
                }
            },
            {
                ArchiverFilterRuleType.TextCustomProperty,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.NumberCustomProperty,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.BooleanCustomProperty,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.DateTimeCustomProperty,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.URL,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.PrimaryAdministrator,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.SiteCollectionSizeTrigger,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo
                }
            },
            {
                ArchiverFilterRuleType.ConversationContent,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.Participant,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.PostedBy,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.RepliedBy,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.LikedBy,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.MentionedName,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.Hashtag,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.Subject,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            },
            {
                ArchiverFilterRuleType.AttachmentCount,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.GreaterThanOrEqualTo,
                    ArchiverFilterCondition.LessThanOrEqualTo
                }
            },
            {
                ArchiverFilterRuleType.SendDateUTC,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.FromTo,
                    ArchiverFilterCondition.Before,
                    ArchiverFilterCondition.OlderThan
                }
            },
            {
                ArchiverFilterRuleType.SendFrom,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.Equals
                }
            },
            {
                ArchiverFilterRuleType.SendTo,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Contains
                }
            },
            {
                ArchiverFilterRuleType.ParentFolderName,
                new List<ArchiverFilterCondition>()
                {
                    ArchiverFilterCondition.Matches,
                    ArchiverFilterCondition.DoesNotMatch,
                    ArchiverFilterCondition.Contains,
                    ArchiverFilterCondition.DoesNotContain,
                    ArchiverFilterCondition.Equals,
                    ArchiverFilterCondition.DoesNotEqual
                }
            }

        };
        #endregion

        public void ResetSOFilter(AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule)
        {
            string AndOrExpression = "(";
            for (int i = 0; i < rule.SOFilters.Count; i++)
            {
                SOFilterPolicy filterDto = rule.SOFilters[i];
                filterDto.Level = rule.PolicyLevel;
                rule.SOFilters[i].SequenceNo = i + 1;
                if (i == rule.SOFilters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                }
            }
            AndOrExpression += ")";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
        }

        public SOFilterPolicy Dto
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the rule type of a filter.
        /// </summary>
        public ArchiverFilterRuleType RuleType
        {
            get
            {
                return GetFilterRuleType(this.Dto.Rule, this.Dto.Level);
                //return this.GetFilterRuleType(this.Dto.RuleGUIType, this.Dto.Rule, this.Dto.Level);
            }
            set
            {
                //this.Dto.RuleType = GetPolicyRuleType(value);
                //this.Dto.RuleGUIType = GetGuiRuleType(value);
                this.Dto.Rule = GetFilterRule(value);
            }
        }

        public int SequenceNo
        {
            get
            {
                return this.Dto.SequenceNo;
            }
            set
            {
                this.Dto.SequenceNo = value;
            }
        }



        public PolicyLevel Level
        {
            get
            {
                return this.Dto.Level;
            }
            set
            {
                this.Dto.Level = value;
            }
        }

        /// <summary>
        /// Gets or sets the condition of a filter.
        /// </summary>
        public ArchiverFilterCondition Condition
        {
            get
            {
                return (ArchiverFilterCondition)this.Dto.Condition;
            }
            set
            {
                this.Dto.Condition = (PolicyCondition)value;
            }
        }

        /// <summary>
        /// Gets or sets the logical relationship between filters.
        /// </summary>
        public ArchiverFilterCombineMode CombineMode
        {
            get
            {
                return this.Dto.IsAnd ? ArchiverFilterCombineMode.And : ArchiverFilterCombineMode.Or;
            }
            set
            {
                this.Dto.IsAnd = (value == ArchiverFilterCombineMode.And);
            }
        }

        public string RuleName
        {
            get
            {
                return this.Dto.Rule.Value1;
            }
            set
            {
                this.Dto.Rule.Value1 = value;
            }
        }

        public string Value1
        {
            get
            {
                return this.Dto.Value.Value1;
            }
            set
            {
                this.Dto.Value.Value1 = value;
            }
        }

        public string Value2
        {
            get
            {
                return this.Dto.Value.Value2;
            }
            set
            {
                this.Dto.Value.Value2 = value;
            }
        }

        public PolicyValueUnit Value1Unit
        {
            get
            {
                return this.Dto.Value.Value1Unit;
            }
            set
            {
                this.Dto.Value.Value1Unit = value;
            }
        }

        public PolicyValueUnit Value2Unit
        {
            get
            {
                return this.Dto.Value.Value2Unit;
            }
            set
            {
                this.Dto.Value.Value2Unit = value;
            }
        }

        public PolicyRuleBase RuleBase
        {
            get
            {
                return this.Dto.Rule;
            }
            //set
            //{ 
            //    this.Dto.Rule = GetFilterRule(this.RuleType);
            //}
        }

        //private ArchiverFilterRuleType GetFilterRuleType(RuleGUIType guiType, PolicyRuleBase ruleBase, PolicyLevel level) {
        //    switch (this.Dto.RuleGUIType)
        //    {
        //        case RuleGUIType.ColumnText:
        //            return ArchiverFilterRuleType.TextColumn;
        //        case RuleGUIType.CustomPropertyText:
        //            return ArchiverFilterRuleType.TextCustomProperty;
        //        case RuleGUIType.ColumnNumber:
        //            return ArchiverFilterRuleType.NumberColumn;
        //        case RuleGUIType.CustomPropertyNumber:
        //            return ArchiverFilterRuleType.NumberCustomProperty;
        //        case RuleGUIType.ColumnBoolean:
        //            return ArchiverFilterRuleType.BooleanColumn;
        //        case RuleGUIType.CustomPropertyBoolean:
        //            return ArchiverFilterRuleType.BooleanCustomProperty;
        //        case RuleGUIType.ColumnDateTime:
        //            return ArchiverFilterRuleType.DateTimeColumn;
        //        case RuleGUIType.CustomPropertyDateTime:
        //            return ArchiverFilterRuleType.DateTimeCustomProperty;
        //        case RuleGUIType.Workflow:
        //            break;
        //        case RuleGUIType.AnonymousAccess:
        //            break;
        //        case RuleGUIType.Attribute:
        //            break;
        //        case RuleGUIType.Attachment:
        //            break;
        //        case RuleGUIType.Auditing:
        //            break;
        //        case RuleGUIType.Category:
        //            break;
        //        case RuleGUIType.ContentType:
        //            return ArchiverFilterRuleType.ContentType;
        //        case RuleGUIType.CreatedBy:
        //            return ArchiverFilterRuleType.CreatedBy;
        //        case RuleGUIType.Created:
        //            return ArchiverFilterRuleType.CreatedTime;
        //        case RuleGUIType.KeepHistoryVersion:
        //            break;
        //        case RuleGUIType.ListType:
        //            break;
        //        case RuleGUIType.ModifiedBy:
        //            return ArchiverFilterRuleType.ModifiedBy;
        //        case RuleGUIType.Modified:
        //            return ArchiverFilterRuleType.ModifiedTime;
        //        case RuleGUIType.NameAndExtention:
        //            break;
        //        case RuleGUIType.Name:
        //            return ArchiverFilterRuleType.Name;
        //        case RuleGUIType.Owner:
        //            break;
        //        case RuleGUIType.SendDate:
        //            break;
        //        case RuleGUIType.Size:
        //            if (ruleBase.Value1.Equals("Document Size", StringComparison.OrdinalIgnoreCase))
        //            {
        //                return ArchiverFilterRuleType.DocumentSize;
        //            }
        //            else if (ruleBase.Value1.Equals("Size"))
        //            {
        //                return ArchiverFilterRuleType.Size;
        //            }
        //            else // "Site Collection Size Trigger"
        //            {
        //                return ArchiverFilterRuleType.SiteCollectionSizeTrigger;
        //            }
        //        case RuleGUIType.Template:
        //            break;
        //        case RuleGUIType.Title:
        //            break;
        //        case RuleGUIType.Url:
        //            break;
        //        case RuleGUIType.Versions:
        //            break;
        //        case RuleGUIType.Versioning:
        //            break;
        //        case RuleGUIType.UserAndGroup:
        //            break;
        //        case RuleGUIType.Inheritance:
        //            break;
        //        case RuleGUIType.StubCreationTime:
        //            break;
        //        case RuleGUIType.StubLastAccessTime:
        //            break;
        //        case RuleGUIType.TemplateId:
        //            break;
        //        case RuleGUIType.LockStatus:
        //            break;
        //        default:
        //            break;
        //    }
        //    throw new NotSupportedException();
        //}

        private ArchiverFilterRuleType GetFilterRuleType(PolicyRuleBase ruleBase, PolicyLevel level)
        {
            //#region remove 

            //switch (RuleBase.Value1)
            //{
            //    case "Name":
            //        return ArchiverFilterRuleType.Name;
            //    case "Size":
            //        return ArchiverFilterRuleType.Size;
            //    case "Document Size":
            //        return ArchiverFilterRuleType.DocumentSize;
            //    case "Site Collection Size Trigger":
            //        return ArchiverFilterRuleType.SiteCollectionSizeTrigger;
            //    case "Modified Time":
            //        return ArchiverFilterRuleType.ModifiedTime;
            //    case "Created Time":
            //        return ArchiverFilterRuleType.CreatedTime;
            //    case "Modified by":
            //        return ArchiverFilterRuleType.ModifiedBy;
            //    case "Created by":
            //        return ArchiverFilterRuleType.CreatedBy;
            //    case "Content Type":
            //        return ArchiverFilterRuleType.ContentType;
            //    case "Column(Text)":
            //        return ArchiverFilterRuleType.TextColumn;
            //    case "Column(Number)":
            //        return ArchiverFilterRuleType.NumberColumn;
            //    case "Column(Yes/No)":
            //        return ArchiverFilterRuleType.BooleanColumn;
            //    case "Column(Date and Time)":
            //        return ArchiverFilterRuleType.DateTimeColumn;
            //    case "Parent List Type ID":
            //        return ArchiverFilterRuleType.ParentListTypeID;
            //    case "Title":
            //        return ArchiverFilterRuleType.Title;
            //    case "Keep the Latest Version":
            //        return ArchiverFilterRuleType.KeepTheLatestVersion;
            //    case "URL":
            //        return ArchiverFilterRuleType.URL;

            //    case "Custom Property(Text)":
            //        return ArchiverFilterRuleType.TextCustomProperty;
            //    case "Custom Property(Number)":
            //        return ArchiverFilterRuleType.NumberCustomProperty;
            //    case "Custom Property(Yse/No)":
            //        return ArchiverFilterRuleType.BooleanCustomProperty;
            //    case "Custom Property(Date and Time)":
            //        return ArchiverFilterRuleType.DateTimeCustomProperty;
            //    default:
            //        throw new NotSupportedException();
            //}

            //#endregion


            if (ruleBase is NameRule)
            {
                return ArchiverFilterRuleType.Name;
            }
            else if (ruleBase is SizeRule)
            {
                if (ruleBase.Value1.Equals("Document Size", StringComparison.OrdinalIgnoreCase))
                {
                    return ArchiverFilterRuleType.DocumentSize;
                }
                else if (ruleBase.Value1.Equals("Size"))
                {
                    return ArchiverFilterRuleType.Size;
                }
                else // "Site Collection Size Trigger"
                {
                    return ArchiverFilterRuleType.SiteCollectionSizeTrigger;
                }
            }
            else if (ruleBase is TermRule)
            {
                return ArchiverFilterRuleType.Term;
            }
            else if (ruleBase is ModifiedRule)
            {
                return ArchiverFilterRuleType.ModifiedTime;
            }
            else if (ruleBase is CreatedRule)
            {
                return ArchiverFilterRuleType.CreatedTime;
            }
            else if (ruleBase is ModifiedByRule)
            {
                return ArchiverFilterRuleType.ModifiedBy;
            }
            else if (ruleBase is CreatedByRule)
            {
                if (level == PolicyLevel.SiteCollection)
                {
                    return ArchiverFilterRuleType.PrimaryAdministrator;
                }
                return ArchiverFilterRuleType.CreatedBy;
            }
            else if (ruleBase is ContentTypeRule)
            {
                return ArchiverFilterRuleType.ContentType;
            }
            else if (ruleBase is ColumnTextRule)
            {
                return ArchiverFilterRuleType.TextColumn;
            }
            else if (ruleBase is MetadataTextColumnRule)
            {
                return ArchiverFilterRuleType.MetadataTextColumn;
            }
            else if (ruleBase is ColumnNumberRule)
            {
                return ArchiverFilterRuleType.NumberColumn;
            }
            else if (ruleBase is MetadataNumberColumnRule)
            {
                return ArchiverFilterRuleType.MetadataNumberColumn;
            }
            else if (ruleBase is ColumnBooleanRule)
            {
                return ArchiverFilterRuleType.BooleanColumn;
            }
            else if (ruleBase is ColumnDateTimeRule)
            {
                return ArchiverFilterRuleType.DateTimeColumn;
            }
            else if (ruleBase is ListTypeRule)
            {
                return ArchiverFilterRuleType.ParentListTypeID;
            }
            else if (ruleBase is StubLastAccessTimeRule /*|| ruleBase is AccessTimeRule*/)
            {
                return ArchiverFilterRuleType.LastAccessedTime;
            }
            else if (ruleBase is TitleRule)
            {
                return ArchiverFilterRuleType.Title;
            }
            else if (ruleBase is KeepHistoryVersionRule)
            {
                return ArchiverFilterRuleType.KeepTheLatestVersion;
            }
            else if (ruleBase is UrlRule)
            {
                return ArchiverFilterRuleType.URL;
            }
            else if (ruleBase is CustomPropertyTextRule)
            {
                return ArchiverFilterRuleType.TextCustomProperty;
            }
            else if (ruleBase is CustomPropertyNumberRule)
            {
                return ArchiverFilterRuleType.NumberCustomProperty;
            }
            else if (ruleBase is CustomPropertyBooleanRule)
            {
                return ArchiverFilterRuleType.BooleanCustomProperty;
            }
            else if (ruleBase is CustomPropertyDateTimeRule)
            {
                return ArchiverFilterRuleType.DateTimeCustomProperty;
            }
            //else if (ruleBase is PostContentRule)
            //{
            //    return ArchiverFilterRuleType.ConversationContent;
            //}
            //else if (ruleBase is ParticipationRule)
            //{
            //    return ArchiverFilterRuleType.Participant;
            //}
            //else if (ruleBase is PostedByRule)
            //{
            //    return ArchiverFilterRuleType.PostedBy;
            //}
            //else if (ruleBase is RepliedByRule)
            //{
            //    return ArchiverFilterRuleType.RepliedBy;
            //}
            //else if (ruleBase is LikedByRule)
            //{
            //    return ArchiverFilterRuleType.LikedBy;
            //}
            //else if (ruleBase is MentionRule)
            //{
            //    return ArchiverFilterRuleType.MentionedName;
            //}
            //else if (ruleBase is TagRule)
            //{
            //    return ArchiverFilterRuleType.Hashtag;
            //}
            else if (ruleBase is SubjectRule)
            {
                return ArchiverFilterRuleType.Subject;
            }
            else if (ruleBase is AttachmentRule)
            {
                return ArchiverFilterRuleType.AttachmentCount;
            }
            else if (ruleBase is SendDateUTCRule)
            {
                return ArchiverFilterRuleType.SendDateUTC;
            }
            else if (ruleBase is SendFromRule)
            {
                return ArchiverFilterRuleType.SendFrom;
            }
            else if (ruleBase is SendToRule)
            {
                return ArchiverFilterRuleType.SendTo;
            }
            else if (ruleBase is ParentFolderNameRule)
            {
                return ArchiverFilterRuleType.ParentFolderName;
            }
            else if (ruleBase is OwnerRule)
            {
                return ArchiverFilterRuleType.Owner;
            }
            else if (ruleBase is FileExtensionsRule)
            {
                return ArchiverFilterRuleType.Type;
            }
            else if (ruleBase is FilePathRule)
            {
                return ArchiverFilterRuleType.FilePath;
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        // server\VCControl\VCManager\StorageOptimization\StorageOptimization.Gui\Xaml\Archiver\ProfileManager\NewRule\ProfileCriteria.xaml.cs
        // Get_*RulesList
        private PolicyRuleBase GetFilterRule(ArchiverFilterRuleType ruleType)
        {
            switch (ruleType)
            {
                case ArchiverFilterRuleType.Name:
                    return new NameRule() { Value1 = "Name" };
                case ArchiverFilterRuleType.Size:
                    return new SizeRule() { Value1 = "Size" };
                case ArchiverFilterRuleType.DocumentSize:
                    return new SizeRule() { Value1 = "Document Size" };
                case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
                    return new SizeRule() { Value1 = "Site Collection Size Trigger" };
                case ArchiverFilterRuleType.ModifiedTime:
                    return new ModifiedRule() { Value1 = "Modified Time" };
                case ArchiverFilterRuleType.CreatedTime:
                    return new CreatedRule() { Value1 = "Created Time" };
                case ArchiverFilterRuleType.ModifiedBy:
                    return new ModifiedByRule() { Value1 = "Modified by" };
                case ArchiverFilterRuleType.CreatedBy:
                case ArchiverFilterRuleType.PrimaryAdministrator:
                    return new CreatedByRule() { Value1 = "Created by" };
                case ArchiverFilterRuleType.ContentType:
                    return new ContentTypeRule() { Value1 = "Content Type" };
                case ArchiverFilterRuleType.TextColumn:
                    return new ColumnTextRule() { Value1 = "Column(Text)" };
                case ArchiverFilterRuleType.MetadataTextColumn:
                    return new MetadataTextColumnRule() { Value1 = "Metadata Column(Text)" };
                case ArchiverFilterRuleType.NumberColumn:
                    return new ColumnNumberRule() { Value1 = "Column(Number)" };
                case ArchiverFilterRuleType.MetadataNumberColumn:
                    return new MetadataNumberColumnRule() { Value1 = "Metadata Column(Number)" };
                case ArchiverFilterRuleType.BooleanColumn:
                    return new ColumnBooleanRule() { Value1 = "Column(Yes/No)" };
                case ArchiverFilterRuleType.DateTimeColumn:
                    return new ColumnDateTimeRule() { Value1 = "Column(Date and Time)" };
                case ArchiverFilterRuleType.ParentListTypeID:
                    return new ListTypeRule() { Value1 = "Parent List Type ID" };
                case ArchiverFilterRuleType.LastAccessedTime:
                    return new StubLastAccessTimeRule() { Value1 = "Last Accessed Time" };
                case ArchiverFilterRuleType.Title:
                    return new TitleRule() { Value1 = "Title" };
                case ArchiverFilterRuleType.KeepTheLatestVersion:
                    return new KeepHistoryVersionRule() { Value1 = "Keep the Latest Version" };
                case ArchiverFilterRuleType.URL:
                    return new UrlRule() { Value1 = "URL" };
                case ArchiverFilterRuleType.Term:
                    return new TermRule() { Value1 = "Column(Text)" };
                case ArchiverFilterRuleType.TextCustomProperty:
                    return new CustomPropertyTextRule() { Value1 = "Custom Property(Text)" };
                case ArchiverFilterRuleType.NumberCustomProperty:
                    return new CustomPropertyNumberRule() { Value1 = "Custom Property(Number)" };
                case ArchiverFilterRuleType.BooleanCustomProperty:
                    return new CustomPropertyBooleanRule() { Value1 = "Custom Property(Yse/No)" };
                case ArchiverFilterRuleType.DateTimeCustomProperty:
                    return new CustomPropertyDateTimeRule() { Value1 = "Custom Property(Date and Time)" };
                //case ArchiverFilterRuleType.ConversationContent:
                //    return new PostContentRule() { Value1 = "Content" };
                //case ArchiverFilterRuleType.Participant:
                //    return new ParticipationRule() { Value1 = "Participation" };
                //case ArchiverFilterRuleType.PostedBy:
                //    return new PostedByRule() { Value1 = "Posted by" };
                //case ArchiverFilterRuleType.RepliedBy:
                //    return new RepliedByRule() { Value1 = "Replied by" };
                //case ArchiverFilterRuleType.LikedBy:
                //    return new LikedByRule() { Value1 = "Liked by" };
                //case ArchiverFilterRuleType.MentionedName:
                //    return new MentionRule() { Value1 = "Mention" };
                //case ArchiverFilterRuleType.Hashtag:
                //    return new TagRule() { Value1 = "Tags" };
                case ArchiverFilterRuleType.Subject:
                    return new SubjectRule() { Value1 = "Subject" };
                case ArchiverFilterRuleType.AttachmentCount:
                    return new AttachmentRule() { Value1 = "Attachment Count" };
                case ArchiverFilterRuleType.SendDateUTC:
                    return new SendDateUTCRule() { Value1 = "Send Time" };
                case ArchiverFilterRuleType.SendFrom:
                    return new SendFromRule() { Value1 = "Send From" };
                case ArchiverFilterRuleType.SendTo:
                    return new SendToRule() { Value1 = "Send To" };
                case ArchiverFilterRuleType.ParentFolderName:
                    return new ParentFolderNameRule() { Value1 = "Parent Folder Name" };
                case ArchiverFilterRuleType.Type:
                    return new FileExtensionsRule() { Value1 = "FileExtensions" };
                case ArchiverFilterRuleType.Owner:
                    return new OwnerRule() { Value1 = "Owner" };
                case ArchiverFilterRuleType.FilePath:
                    return new FilePathRule() { Value1 = "Path" };
                default:
                    throw new NotSupportedException();
            }
        }

        //private PolicyRuleType GetPolicyRuleType(ArchiverFilterRuleType ruleType)
        //{
        //    switch (ruleType)
        //    {
        //        case ArchiverFilterRuleType.Name:
        //            return PolicyRuleType.Name;
        //        case ArchiverFilterRuleType.Size:

        //        case ArchiverFilterRuleType.DocumentSize:

        //        case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
        //            return PolicyRuleType.Size;
        //        case ArchiverFilterRuleType.ModifiedTime:
        //            return PolicyRuleType.ModifiedTime;
        //        case ArchiverFilterRuleType.CreatedTime:
        //            return PolicyRuleType.CreatedTime;
        //        case ArchiverFilterRuleType.ModifiedBy:
        //            return PolicyRuleType.UserAndGroup;
        //        case ArchiverFilterRuleType.CreatedBy:
        //        case ArchiverFilterRuleType.PrimaryAdministrator:
        //            return PolicyRuleType.CreatedBy;
        //        case ArchiverFilterRuleType.ContentType:
        //            return PolicyRuleType.ContentType;
        //        case ArchiverFilterRuleType.TextColumn:
        //        case ArchiverFilterRuleType.Term:
        //        //  return PolicyRuleType.ColumnText;
        //        //case ArchiverFilterRuleType.NumberColumn:
        //        //    return PolicyRuleType.ColumnNumber;
        //        //case ArchiverFilterRuleType.BooleanColumn:
        //        //    return PolicyRuleType.ColumnBoolean;
        //        //case ArchiverFilterRuleType.DateTimeColumn:
        //        //    return PolicyRuleType.ColumnDateTime;
        //        //case ArchiverFilterRuleType.ParentListTypeID:
        //        //    return PolicyRuleType.ColumnNumber;
        //        //case ArchiverFilterRuleType.LastAccessedTime:
        //        //    if (this.Dto.Level == PolicyLevel.Document)
        //        //    {
        //        //        return new AccessTimeRule() { Value1 = "Last Accessed Time" };
        //        //    }
        //        //    else
        //        //    {
        //        //        return new StubLastAccessTimeRule() { Value1 = "Last Accessed Time" };
        //        //    }
        //        case ArchiverFilterRuleType.Title:
        //            return PolicyRuleType.Title;
        //        case ArchiverFilterRuleType.KeepTheLatestVersion:
        //            return PolicyRuleType.Versioning;//to confirm
        //        case ArchiverFilterRuleType.URL:
        //            return PolicyRuleType.Url;
        //        case ArchiverFilterRuleType.TextCustomProperty:

        //        case ArchiverFilterRuleType.NumberCustomProperty:

        //        case ArchiverFilterRuleType.BooleanCustomProperty:

        //        case ArchiverFilterRuleType.DateTimeCustomProperty:

        //            return PolicyRuleType.CustomProperty;
        //        //case ArchiverFilterRuleType.ConversationContent:
        //        //    return new PostContentRule() { Value1 = "Content" };
        //        //case ArchiverFilterRuleType.Participant:
        //        //    return new ParticipationRule() { Value1 = "Participation" };
        //        //case ArchiverFilterRuleType.PostedBy:
        //        //    return new PostedByRule() { Value1 = "Posted by" };
        //        //case ArchiverFilterRuleType.RepliedBy:
        //        //    return new RepliedByRule() { Value1 = "Replied by" };
        //        //case ArchiverFilterRuleType.LikedBy:
        //        //    return new LikedByRule() { Value1 = "Liked by" };
        //        //case ArchiverFilterRuleType.MentionedName:
        //        //    return new MentionRule() { Value1 = "Mention" };
        //        //case ArchiverFilterRuleType.Hashtag:
        //        //    return new TagRule() { Value1 = "Tags" };
        //        default:
        //            throw new NotSupportedException();
        //    }
        //}

        //private RuleGUIType GetGuiRuleType(ArchiverFilterRuleType ruleType)
        //{
        //    switch (ruleType)
        //    {
        //        case ArchiverFilterRuleType.Name:
        //            return RuleGUIType.Name;
        //        case ArchiverFilterRuleType.Size:

        //        case ArchiverFilterRuleType.DocumentSize:

        //        case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
        //            return RuleGUIType.Size;
        //        case ArchiverFilterRuleType.ModifiedTime:
        //            return RuleGUIType.Modified;
        //        case ArchiverFilterRuleType.CreatedTime:
        //            return RuleGUIType.Created;
        //        case ArchiverFilterRuleType.ModifiedBy:
        //            return RuleGUIType.UserAndGroup;
        //        case ArchiverFilterRuleType.CreatedBy:
        //        case ArchiverFilterRuleType.PrimaryAdministrator:
        //            return RuleGUIType.CreatedBy;
        //        case ArchiverFilterRuleType.ContentType:
        //            return RuleGUIType.ContentType;
        //        case ArchiverFilterRuleType.TextColumn:
        //        case ArchiverFilterRuleType.Term:
        //            return RuleGUIType.ColumnText;
        //        case ArchiverFilterRuleType.NumberColumn:
        //            return RuleGUIType.ColumnNumber;
        //        case ArchiverFilterRuleType.BooleanColumn:
        //            return RuleGUIType.ColumnBoolean;
        //        case ArchiverFilterRuleType.DateTimeColumn:
        //            return RuleGUIType.ColumnDateTime;
        //        case ArchiverFilterRuleType.ParentListTypeID:
        //            return RuleGUIType.ColumnNumber;
        //        case ArchiverFilterRuleType.LastAccessedTime:
        //        case ArchiverFilterRuleType.Title:
        //            return RuleGUIType.Title;
        //        case ArchiverFilterRuleType.KeepTheLatestVersion:
        //            return RuleGUIType.Versioning;//to confirm
        //        case ArchiverFilterRuleType.URL:
        //            return RuleGUIType.Url;
        //        case ArchiverFilterRuleType.TextCustomProperty:
        //            return RuleGUIType.CustomPropertyText;
        //        case ArchiverFilterRuleType.NumberCustomProperty:
        //            return RuleGUIType.CustomPropertyNumber;
        //        case ArchiverFilterRuleType.BooleanCustomProperty:
        //            return RuleGUIType.CustomPropertyDateTime;
        //        case ArchiverFilterRuleType.DateTimeCustomProperty:


        //        default:
        //            throw new NotSupportedException();
        //    }
        //}



        /// <summary>
        /// Gets or sets the values of a filter.
        /// </summary>  
        public string[] Values
        {
            get
            {
                if (string.IsNullOrEmpty(this.Dto.Value.Value1))
                {
                    return null;
                }
                else
                {
                    List<string> values = new List<string>();
                    values.Add(string.Format("{0}{1}", this.Dto.Value.Value1, this.Dto.Value.Value1Unit == PolicyValueUnit.None ? string.Empty : ConvertPolicyValueUnitToI18N(this.Dto.Value.Value1Unit)));
                    if (!string.IsNullOrEmpty(this.Dto.Value.Value2))
                    {
                        values.Add(string.Format("{0}{1}", this.Dto.Value.Value2, this.Dto.Value.Value2Unit == PolicyValueUnit.None ? string.Empty : ConvertPolicyValueUnitToI18N(this.Dto.Value.Value2Unit)));
                    }
                    return values.ToArray();
                }
            }
            set
            {
                SetValue(value);
            }
        }

        /// <summary>
        /// value[0] Value1, value[1] timezoneId, value[2] autoAdjustForDSTStr
        /// </summary>
        /// <param name="value"></param>
        private void SetValue(string[] value)
        {
            ValidateUtil.ValidateArrayHasNullOrEmptyString(value);

            ValidateRuleAndCondition();

            if (this.RuleType == ArchiverFilterRuleType.Name || this.RuleType == ArchiverFilterRuleType.ModifiedBy
                || this.RuleType == ArchiverFilterRuleType.CreatedBy || this.RuleType == ArchiverFilterRuleType.ContentType
                || this.RuleType == ArchiverFilterRuleType.Title || this.RuleType == ArchiverFilterRuleType.URL
                || this.RuleType == ArchiverFilterRuleType.PrimaryAdministrator || this.RuleType == ArchiverFilterRuleType.ConversationContent
                || this.RuleType == ArchiverFilterRuleType.Participant || this.RuleType == ArchiverFilterRuleType.PostedBy
                || this.RuleType == ArchiverFilterRuleType.RepliedBy || this.RuleType == ArchiverFilterRuleType.LikedBy
                || this.RuleType == ArchiverFilterRuleType.MentionedName || this.RuleType == ArchiverFilterRuleType.Hashtag
                || this.RuleType == ArchiverFilterRuleType.Subject || this.RuleType == ArchiverFilterRuleType.SendFrom || this.RuleType == ArchiverFilterRuleType.SendTo
                || this.RuleType == ArchiverFilterRuleType.Type || this.RuleType == ArchiverFilterRuleType.Owner || this.RuleType == ArchiverFilterRuleType.FilePath
                || this.RuleType == ArchiverFilterRuleType.ParentFolderName)
            {
                ValidateValueCount(value, 1);
                this.Dto.Value.Value1 = value[0];
            }
            else if (this.RuleType == ArchiverFilterRuleType.Size
                || this.RuleType == ArchiverFilterRuleType.DocumentSize
                || this.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger || this.RuleType == ArchiverFilterRuleType.AttachmentCount)
            {
                ValidateValueCount(value, 1);
                SetValueForSizeRule(value[0]);
            }
            else if (this.RuleType == ArchiverFilterRuleType.ModifiedTime || this.RuleType == ArchiverFilterRuleType.CreatedTime
                || this.RuleType == ArchiverFilterRuleType.LastAccessedTime || this.RuleType == ArchiverFilterRuleType.SendDateUTC)
            {
                if (this.Condition == ArchiverFilterCondition.FromTo)
                {
                    ValidateValueCount(value, 6);
                    DateTime startUtcTime = SetDateTime(value[0], value[1], value[2], false);
                    DateTime endUtcTime = SetDateTime(value[3], value[4], value[5], true);
                    if (DateTime.Parse(value[0]) >= DateTime.Parse(value[3]))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    this.Dto.Value.Value1 = startUtcTime.ToString(ContractConstants.DATETYPEForAPI003);
                    this.Dto.Value.Value2 = endUtcTime.ToString(ContractConstants.DATETYPEForAPI003);
                }
                else if (this.Condition == ArchiverFilterCondition.Before)
                {
                    ValidateValueCount(value, 3);
                    DateTime utcTime = SetDateTime(value[0], value[1], value[2], false);
                    this.Dto.Value.Value1 = utcTime.ToString(ContractConstants.DATETYPEForAPI003);
                }
                else if (this.Condition == ArchiverFilterCondition.OlderThan)
                {
                    ValidateValueCount(value, 1);
                    SetValueForOlderThan(value[0]);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else if (this.RuleType == ArchiverFilterRuleType.ParentListTypeID)
            {
                ValidateValueCount(value, 1);
                int intValue = 0;
                if (int.TryParse(value[0], out intValue))
                {
                    this.Dto.Value.Value1 = intValue.ToString();
                }
                else
                {
                    //throw new InvalidArgumentException(Messages.Get("value_must_be_an_integer"));
                    throw new Exception("");
                }
            }
            else if (this.RuleType == ArchiverFilterRuleType.KeepTheLatestVersion)
            {
                ValidateValueCount(value, 1);
                int intValue = 0;
                if (int.TryParse(value[0], out intValue))
                {
                    if (intValue < 0)
                    {
                        //throw new InvalidArgumentException(Messages.Get("value_must_be_an_integer_greater_than_zero"));
                        throw new Exception("");
                    }
                    else
                    {
                        this.Dto.Value.Value1 = intValue.ToString();
                    }
                }
                else
                {
                    //throw new InvalidArgumentException(Messages.Get("value_must_be_an_integer"));
                    throw new Exception("");
                }
            }
            else if (this.RuleType == ArchiverFilterRuleType.TextColumn
                || this.RuleType == ArchiverFilterRuleType.TextCustomProperty
                || this.RuleType == ArchiverFilterRuleType.MetadataTextColumn)
            {
                ValidateValueCount(value, 2);
                this.Dto.Rule.Value1 = value[0];
                this.Dto.Value.Value1 = value[1];
            }
            else if (this.RuleType == ArchiverFilterRuleType.NumberColumn
                || this.RuleType == ArchiverFilterRuleType.NumberCustomProperty
                || this.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
            {
                ValidateValueCount(value, 2);
                this.Dto.Rule.Value1 = value[0];
                double intValue = 0;
                if (double.TryParse(value[1], out intValue))
                {
                    this.Dto.Value.Value1 = intValue.ToString();
                }
                else
                {
                    //throw new InvalidArgumentException(Messages.Get("value_must_be_an_integer"));
                    throw new Exception("");
                }
            }
            else if (this.RuleType == ArchiverFilterRuleType.BooleanColumn
                || this.RuleType == ArchiverFilterRuleType.BooleanCustomProperty)
            {
                ValidateValueCount(value, 2);
                this.Dto.Rule.Value1 = value[0];
                SetValueForBooleanColumnRule(value[1]);
            }
            else if (this.RuleType == ArchiverFilterRuleType.DateTimeColumn
                || this.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
            {
                this.Dto.Rule.Value1 = value[0];
                if (this.Condition == ArchiverFilterCondition.FromTo)
                {
                    ValidateValueCount(value, 7);
                    DateTime startUtcTime = SetDateTime(value[1], value[2], value[3], false);
                    DateTime endUtctime = SetDateTime(value[4], value[5], value[6], true);
                    if (DateTime.Parse(value[1]) >= DateTime.Parse(value[4]))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    this.Dto.Value.Value1 = startUtcTime.ToString(ContractConstants.DATETYPEForAPI003);
                    this.Dto.Value.Value2 = endUtctime.ToString(ContractConstants.DATETYPEForAPI003);
                }
                else if (this.Condition == ArchiverFilterCondition.Before)
                {
                    ValidateValueCount(value, 4);
                    DateTime utcTime = SetDateTime(value[1], value[2], value[3], false);
                    this.Dto.Value.Value1 = utcTime.ToString(ContractConstants.DATETYPEForAPI003);
                }
                else if (this.Condition == ArchiverFilterCondition.OlderThan)
                {
                    ValidateValueCount(value, 2);
                    SetValueForOlderThan(value[1]);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        private void ValidateValueCount(string[] values, int count)
        {
            if (values.Length != count)
            {
                //throw new InvalidArgumentException(Messages.Get("invalidate_value_count_archive", count, this.RuleType, this.Condition));
                throw new Exception("");
            }
        }
        private void SetValueForSizeRule(string value)
        {
            if (Regex.IsMatch(value, "^(([0-9]+)(KB|MB|GB))$", RegexOptions.IgnoreCase))
            {
                this.Dto.Value.Value1 = value.Substring(0, value.Length - 2);
                if (value.EndsWith("KB", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dto.Value.Value1Unit = PolicyValueUnit.KB;
                }
                else if (value.EndsWith("MB", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dto.Value.Value1Unit = PolicyValueUnit.MB;
                }
                else
                {
                    this.Dto.Value.Value1Unit = PolicyValueUnit.GB;
                }
            }
            else
            {
                //throw new InvalidArgumentException(Messages.Get("invalidate_size_value"));
                throw new Exception("");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="autoAdjustForDSTStr"></param>
        /// <param name="isEndTime"></param>
        /// <returns>返回Utc时间</returns>
        public DateTime SetDateTime(string dateTime, string timeZoneId, string autoAdjustForDSTStr, bool isEndTime)
        {
            DateTime dateTimeObj = DateTime.Parse(dateTime);
            dateTimeObj = DateTime.SpecifyKind(dateTimeObj, DateTimeKind.Unspecified);
            bool autoAdjustForDST = Boolean.Parse(autoAdjustForDSTStr);

            if (isEndTime)
            {
                this.Dto.EndTime = new DisplayDateTime()
                {
                    StartTime = dateTimeObj.ToString(ContractConstants.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture),
                    TimeZoneId = timeZoneId,
                    IsDayLightSaving = autoAdjustForDST ? autoAdjustForDST : false
                };
            }
            else
            {
                this.Dto.BeginTime = new DisplayDateTime()
                {
                    StartTime = dateTimeObj.ToString(ContractConstants.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture),
                    TimeZoneId = timeZoneId,
                    IsDayLightSaving = autoAdjustForDST ? autoAdjustForDST : false
                };
            }
            //time zone info to do next
            // DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeObj, timeZoneInfo);
            TimeZoneInfo sourceTimezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            //DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeObj, tzi);
            if (!autoAdjustForDST && sourceTimezone.SupportsDaylightSavingTime && sourceTimezone.IsDaylightSavingTime(dateTimeObj))
            {
                dateTimeObj = dateTimeObj.AddHours(1);
            }
            return TimeZoneInfo.ConvertTimeToUtc(dateTimeObj, sourceTimezone);
        }

        public DisplayDateTime GetFilterDateTimeInfo(bool isEndTime)
        {
            return isEndTime ? this.Dto.BeginTime : this.Dto.EndTime;

            //DateTime begin = new DateTime(1970, 1, 1, 0, 0, 0);

            //TimeSpan calcTime = new TimeSpan(DateTime.Parse(date.StartTime).ToUniversalTime().Ticks - begin.Ticks);

            //return new FilterDateTime() 
            //{
            //    StartTime = long.Parse(calcTime.TotalSeconds * 1000 + ""),
            //    TimeZoneId = date.TimeZoneId,
            //    IsDayLightSaving = date.IsDayLightSaving
            //};
        }

        private void SetValueForOlderThan(string value)
        {
            if (Regex.IsMatch(value, "^(((0+[1-9]+|[1-9][0-9]*))(day|week|month|year))$", RegexOptions.IgnoreCase))
            {
                if (value.EndsWith("day", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dto.Value.Value1 = value.Substring(0, value.Length - 3).TrimStart('0');
                    this.Dto.Value.Value1Unit = PolicyValueUnit.Days;
                }
                else if (value.EndsWith("week", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dto.Value.Value1 = value.Substring(0, value.Length - 4).TrimStart('0');
                    this.Dto.Value.Value1Unit = PolicyValueUnit.Weeks;
                }
                else if (value.EndsWith("month", StringComparison.OrdinalIgnoreCase))
                {
                    this.Dto.Value.Value1 = value.Substring(0, value.Length - 5).TrimStart('0');
                    this.Dto.Value.Value1Unit = PolicyValueUnit.Months;
                }
                else // years
                {
                    this.Dto.Value.Value1 = value.Substring(0, value.Length - 4).TrimStart('0');
                    this.Dto.Value.Value1Unit = PolicyValueUnit.Years;
                }
            }
            else
            {
                //throw new InvalidArgumentException(Messages.Get("invalidate_before_value"));
                throw new Exception("");
            }
        }
        private void SetValueForTypeRule(string value)
        {
            if (value.Equals("NonOffice", StringComparison.OrdinalIgnoreCase))
            {
                this.Dto.Value.Value1 = "NonOffice";
            }
            else if (value.Equals("Office", StringComparison.OrdinalIgnoreCase))
            {
                this.Dto.Value.Value1 = "Office";
            }
            else
            {
                //throw new GeneralException(Messages.Get("invalidate_type_value"));
                throw new Exception("");
            }
        }
        private void SetValueForBooleanColumnRule(string value)
        {
            if (value.Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                this.Dto.Value.Value1 = "Yes";
            }
            else if (value.Equals("NO", StringComparison.OrdinalIgnoreCase))
            {
                this.Dto.Value.Value1 = "No";
            }
            else
            {
                //throw new GeneralException(Messages.Get("Invalidate_boolean_column_value"));
                throw new Exception("");
            }
        }
        /// <summary>
        /// Creates a filter object from the specified recurrence string.
        /// </summary>
        /// <param name="recurrenceValue">A string that expresses the recurrence.</param>
        /// <returns>An ScheduledStorageManagerRuleFilter object that represents the filter.</returns>
        //private static ArchiverRuleFilter FromString(string recurrenceValue)
        //{
        //    if (string.IsNullOrEmpty(recurrenceValue))
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    ArchiverRuleFilter filter = new ArchiverRuleFilter();
        //    return filter;
        //}
        private void ValidateRuleAndCondition()
        {
            if (!FilterRuleAndConditionMapping[this.RuleType].Contains(this.Condition))
            {
                List<string> conditions = new List<string>();
                FilterRuleAndConditionMapping[this.RuleType].ForEach(delegate (ArchiverFilterCondition condition) { conditions.Add(condition.ToString()); });
                //throw new GeneralException(
                //    string.Format(Messages.Get("rule_and_condition_not_match"),
                //    this.RuleType, this.Condition, string.Join(", ", conditions.ToArray())));
                throw new Exception("");
            }
        }
        internal void Validate()
        {
            ValidateRuleAndCondition();
            ValidateUtil.ValidateArrayHasNullOrEmptyString(this.Values);
            // 在GUI上equals是PolicyCondition.Exactly， = 是PolicyCondition.Equals
            if ((this.RuleType == ArchiverFilterRuleType.NumberColumn && this.Condition == ArchiverFilterCondition.Equals)
                || (this.RuleType == ArchiverFilterRuleType.NumberCustomProperty && this.Condition == ArchiverFilterCondition.Equals)
                || (this.RuleType == ArchiverFilterRuleType.MetadataNumberColumn && this.Condition == ArchiverFilterCondition.Equals))
            {
                this.Dto.Condition = PolicyCondition.Equals;
            }
        }
        /// <summary>
        /// Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns>Returns a string that represents the filter.</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            if (this.RuleType == ArchiverFilterRuleType.TextColumn || this.RuleType == ArchiverFilterRuleType.NumberColumn
                || this.RuleType == ArchiverFilterRuleType.DateTimeColumn || this.RuleType == ArchiverFilterRuleType.BooleanColumn
                || this.RuleType == ArchiverFilterRuleType.TextCustomProperty || this.RuleType == ArchiverFilterRuleType.NumberCustomProperty
                || this.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty || this.RuleType == ArchiverFilterRuleType.BooleanCustomProperty
                || this.RuleType == ArchiverFilterRuleType.MetadataTextColumn || this.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
            {
                str.AppendFormat("{0}: {1}", this.RuleType.ToString(), this.Dto.Rule.Value1).Append(", ");
            }
            else
            {
                str.Append(this.RuleType.ToString()).Append(", ");
            }
            str.Append(this.Condition).Append(", ");
            foreach (string value in this.Values)
            {
                str.Append(value).Append(" ");
            }
            str.Append(", ");
            str.Append(this.CombineMode.ToString());
            return str.ToString();
        }

        public string FilterCretia()
        {
            StringBuilder str = new StringBuilder();
            str.Append(this.Dto.SequenceNo.ToString() + ".").Append(" ");
            str.Append(I18NEntity.GetString(ConvertPolicyLevelToI18NKey(this.Level))).Append(", ");
            switch (this.RuleType)
            {
                case ArchiverFilterRuleType.TextColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnText"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.NumberColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnNumber"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.DateTimeColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnDateTime"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.BooleanColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnBoolean"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.MetadataTextColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.MetadataNumberColumn:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn"), this.Dto.Rule.Value1).Append(", ");
                    break;
                //case ArchiverFilterRuleType.TextCustomProperty:
                //    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnNumber"), this.Dto.Rule.Value1).Append(", ");
                //    break;
                //case ArchiverFilterRuleType.NumberCustomProperty:
                //    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnNumber"), this.Dto.Rule.Value1).Append(", ");
                //    break;
                //case ArchiverFilterRuleType.DateTimeCustomProperty:
                //    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnNumber"), this.Dto.Rule.Value1).Append(", ");
                //    break;
                //case ArchiverFilterRuleType.BooleanCustomProperty:
                //    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ColumnNumber"), this.Dto.Rule.Value1).Append(", ");
                //    break;
                case ArchiverFilterRuleType.Name:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Name")).Append(", ");
                    break;
                case ArchiverFilterRuleType.DocumentSize:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_DocumentSize")).Append(", ");
                    break;
                case ArchiverFilterRuleType.ModifiedTime:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Modified")).Append(", ");
                    break;
                case ArchiverFilterRuleType.CreatedTime:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_CreateTime")).Append(", ");
                    break;
                case ArchiverFilterRuleType.CreatedBy:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_CreatedBy")).Append(", ");
                    break;
                case ArchiverFilterRuleType.PrimaryAdministrator:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_PrimaryAdministrator")).Append(", ");
                    break;
                case ArchiverFilterRuleType.ModifiedBy:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ModifiedBy")).Append(", ");
                    break;
                case ArchiverFilterRuleType.ContentType:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ContentType")).Append(", ");
                    break;
                case ArchiverFilterRuleType.ParentListTypeID:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ParentList")).Append(", ");
                    break;
                case ArchiverFilterRuleType.LastAccessedTime:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_LastAccessedTime")).Append(", ");
                    break;
                case ArchiverFilterRuleType.Title:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Title")).Append(", ");
                    break;
                case ArchiverFilterRuleType.URL:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_URL")).Append(", ");
                    break;
                //youcuowu
                case ArchiverFilterRuleType.TextCustomProperty:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_TextCustomProperty"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.NumberCustomProperty:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_NumberCustomProperty"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.BooleanCustomProperty:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_BooleanCustomProperty"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.DateTimeCustomProperty:
                    str.AppendFormat("{0}: {1}", I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_DateTimeCustomProperty"), this.Dto.Rule.Value1).Append(", ");
                    break;
                case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_SiteCollectionSizeTrigger")).Append(", ");
                    break;
                case ArchiverFilterRuleType.Subject:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Subjecjt")).Append(", ");
                    break;
                case ArchiverFilterRuleType.AttachmentCount:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_AttachmentCount")).Append(", ");
                    break;
                case ArchiverFilterRuleType.Size:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Size")).Append(", ");
                    break;
                case ArchiverFilterRuleType.SendDateUTC:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_SendDateUTC")).Append(", ");
                    break;
                case ArchiverFilterRuleType.SendFrom:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_SendFrom")).Append(", ");
                    break;
                case ArchiverFilterRuleType.SendTo:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_SendTo")).Append(", ");
                    break;
                case ArchiverFilterRuleType.ParentFolderName:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_ParentFolderName")).Append(", ");
                    break;
                case ArchiverFilterRuleType.Type:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_FileType")).Append(", ");
                    break;
                case ArchiverFilterRuleType.Owner:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_FileOwner")).Append(", ");
                    break;
                case ArchiverFilterRuleType.FilePath:
                    str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_FilePath")).Append(", ");
                    break;
            }

            //if (this.RuleType == ArchiverFilterRuleType.TextColumn || this.RuleType == ArchiverFilterRuleType.NumberColumn
            //    || this.RuleType == ArchiverFilterRuleType.DateTimeColumn || this.RuleType == ArchiverFilterRuleType.BooleanColumn
            //    || this.RuleType == ArchiverFilterRuleType.TextCustomProperty || this.RuleType == ArchiverFilterRuleType.NumberCustomProperty
            //    || this.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty || this.RuleType == ArchiverFilterRuleType.BooleanCustomProperty)
            //{
            //    str.AppendFormat("{0}: {1}", this.RuleType.ToString(), this.Dto.Rule.Value1).Append(", ");
            //}
            //else
            //{
            //    str.Append(this.RuleType.ToString()).Append(", ");
            //}
            try
            {
                PolicyCondition contition = (PolicyCondition)this.Condition;
                switch (contition)
                {
                    case PolicyCondition.Contains:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_Contains")).Append(", ");
                        break;
                    case PolicyCondition.DoesNotContains:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains")).Append(", ");
                        break;
                    case PolicyCondition.Match:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_Maths")).Append(", ");
                        break;
                    case PolicyCondition.DoesNotMatch:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath")).Append(", ");
                        break;
                    case PolicyCondition.Exactly:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_Equals")).Append(", ");
                        break;
                    case PolicyCondition.IsExactlyNot:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot")).Append(", ");
                        break;
                    case PolicyCondition.GreaterOrEqualThan:
                        str.Append(">=").Append(", ");
                        break;
                    case PolicyCondition.LessOrEqualThan:
                        str.Append(I18NEntity.GetString("<=")).Append(", ");
                        break;
                    case PolicyCondition.Equals:
                        if (this.RuleType == ArchiverFilterRuleType.NumberColumn
                            || this.RuleType == ArchiverFilterRuleType.NumberCustomProperty
                            || this.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
                        {
                            str.Append("=").Append(", ");
                        }
                        else
                        {
                            str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_Equals")).Append(", ");
                        }
                        break;
                    case PolicyCondition.FromTo:
                        //str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_FromTo")).Append(", ");
                        break;
                    case PolicyCondition.Before:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_Before")).Append(", ");
                        break;
                    case PolicyCondition.OlderThan:
                        str.Append(I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_Older")).Append(", ");
                        break;
                    default:
                        str.Append(contition.ToString()).Append(", ");
                        break;
                }
            }
            catch
            {
                str.Append(this.Condition.ToString()).Append(", ");
            }
            var dtoBeginTime = this.Dto.BeginTime;
            var dtoEndTime = this.Dto.EndTime;
            if ((dtoBeginTime == null && dtoEndTime == null) || (dtoBeginTime.StartTime == null && dtoEndTime.StartTime == null))
            {
                foreach (string value in this.Values)
                {
                    str.Append(value).Append(" ");
                }
            }
            else
            {
                if (dtoBeginTime != null && dtoEndTime != null && dtoBeginTime.StartTime != null && dtoEndTime.StartTime != null)
                {
                    str.AppendFormat("{4} {0} {1} {5} {2} {3} ", dtoBeginTime.StartTime, GetTimeZoneNameById(this.Dto.BeginTime.TimeZoneId), dtoEndTime.StartTime, GetTimeZoneNameById(this.Dto.EndTime.TimeZoneId), I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_From"), I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_To"));
                }
                else if (dtoBeginTime != null && dtoBeginTime.StartTime != null)
                {
                    str.AppendFormat("{0} {1} ", dtoBeginTime.StartTime, GetTimeZoneNameById(this.Dto.BeginTime.TimeZoneId));
                }
                else
                {
                    str.AppendFormat("{0} {1} ", dtoEndTime.StartTime, GetTimeZoneNameById(this.Dto.EndTime.TimeZoneId));
                }

            }

            //str.Append(", ");
            // str.Append(this.CombineMode.ToString());
            return str.ToString();
        }

        private string ConvertPolicyLevelToI18NKey(PolicyLevel level)
        {
            var i18NString = "";
            switch (level)
            {
                case PolicyLevel.WebApplication:
                    i18NString = "RM_JS_Rule_ObjectLevel_WebApplication";
                    break;
                case PolicyLevel.SiteCollection:
                    i18NString = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                case PolicyLevel.Site:
                    i18NString = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case PolicyLevel.List:
                case PolicyLevel.Library:
                    i18NString = "RM_JS_Rule_ObjectLevel_List";
                    break;
                case PolicyLevel.Folder:
                    i18NString = "RM_JS_Rule_ObjectLevel_Folder";
                    break;
                case PolicyLevel.Item:
                    i18NString = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                case PolicyLevel.Document:
                    i18NString = "RM_JS_Rule_ObjectLevel_Document";
                    break;
                case PolicyLevel.ExchangeOnlineItem_Message:
                    i18NString = "RM_JS_EXO_SubTabLabel_Message";
                    break;
                case PolicyLevel.PhysicalFile:
                    i18NString = "RM_JS_RDM_CreateRule_FilterLevel_PhysicalFile";
                    break;
                case PolicyLevel.PhysicalBox:
                    i18NString = "RM_JS_RDM_CreateRule_FilterLevel_PhysicalBox";
                    break;
                case PolicyLevel.FileSysFile:
                    i18NString = "RM_JS_Rule_CreateRule_FilterLevel_Document";
                    break;
                default:
                    i18NString = level.ToString();
                    break;
            }
            return i18NString;
        }

        private string ConvertPolicyValueUnitToI18N(PolicyValueUnit unit)
        {
            if (unit != PolicyValueUnit.None)
            {
                return I18NEntity.GetString($"RM_JS_RDM_CreateRule_Unit_{unit.ToString()}");
            }
            return "";
        }

        public string GetI18NPolicyLevel(PolicyLevel level)
        {
            var strLevel = level.ToString();
            switch (level)
            {
                case PolicyLevel.ExchangeOnlineItem_Message:
                    //strLevel = I18NEntity.GetString("RM_JS_RDM_CreateRule_FilterLevel_EXOMessage");
                    strLevel = I18NEntity.GetString("RM_JS_EXO_SubTabLabel_Message");
                    //case PolicyLevel.ExchangeOnlineItem_Task:
                    //case PolicyLevel.ExchangeOnlineItem_Post:
                    //case PolicyLevel.ExchangeOnlineItem_Event:
                    //case PolicyLevel.ExchangeOnlineItem_Journal:
                    //case PolicyLevel.ExchangeOnlineItem_Note:
                    //case PolicyLevel.ExchangeOnlineItem_Contact:
                    //case PolicyLevel.ExchangeOnlineItem_Document:
                    break;
                case PolicyLevel.PhysicalFile:
                    strLevel = I18NEntity.GetString("RM_JS_RDM_CreateRule_FilterLevel_PhysicalFile");
                    break;
                case PolicyLevel.PhysicalBox:
                    strLevel = I18NEntity.GetString("RM_JS_RDM_CreateRule_FilterLevel_PhysicalBox");
                    break;
            }
            return strLevel;
        }

        internal bool EqualsTo(ArchiverRuleFilter filter)
        {
            if (filter.RuleType != this.RuleType)
            {
                return false;
            }
            if (filter.Condition != this.Condition)
            {
                return false;
            }
            string[] values1 = filter.Values;
            string[] values2 = this.Values;
            if (values1.Length != values2.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < values1.Length; i++)
                {
                    if (!values1[i].Equals(values2[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private string GetTimeZoneNameById(string timeZoneId)
        {
            string timeZoneName = string.Empty;
            if (!string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneName = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).DisplayName;
                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(timeZoneName);
                timeZoneName = matchResult.Value;
            }
            return timeZoneName;
        }
    }
}
