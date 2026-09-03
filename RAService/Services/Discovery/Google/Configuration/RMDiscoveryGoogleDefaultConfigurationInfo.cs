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
using System.Collections.Generic;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Discovery.Model.Rule.Criteria;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Configuration
{
    public static class RMDiscoveryGoogleDefaultConfigurationInfo
    {
        public static RMDiscoveryGoogleScopeInfo DEFAULT_SCOPE_INFO => new();

        public static RMDiscoveryGoogleRotDefinition DEFAULT_ROT_DEFINITION => new()
        {
            Enable = true,
            RedundantRules = new()
            {
                new()
                {
                    Name = I18NEntity.GetString("RM_FA_Discovery_CopiedFileRule"),
                    Description = I18NEntity.GetString("RM_FA_Discovery_CopiedFileRule_Desc"),
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                    Order = 1,
                    CriteriaInfoes = new()
                    {
                        new()
                        {
                            Order = 1,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.Name,
                            LogicType = RMDiscoveryCriteriaLogicType.None,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.Array,
                                Logic = (int)RMDiscoveryArrayConditionType.TextMatchIn,
                                Value = JsonConvert.SerializeObject(
                                        new List<string> {
                                            "*- copy*", "*backup*","*Copy of*"
                                        }),
                            }
                        }
                    }
                }
            },
            ObsoleteRules = new()
            {
                new() {
                    Name = I18NEntity.GetString("RM_FA_Discovery_OldOfficeFileRule"),
                    Description = I18NEntity.GetString("RM_FA_Discovery_OldOfficeFileRule_Desc"),
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                    Order = 1,
                    CriteriaInfoes = new()
                    {
                        new()
                        {
                            Order = 1,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                            LogicType = RMDiscoveryCriteriaLogicType.And,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.DateTime,
                                Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo { Unit = 3, UnitType = RMDiscoveryDateUnitType.Year }),
                            }
                        },
                        new()
                        {
                            Order = 2,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.DocumentType,
                            LogicType = RMDiscoveryCriteriaLogicType.None,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.Array,
                                Logic = (int)RMDiscoveryArrayConditionType.In,
                                Value = JsonConvert.SerializeObject(
                                        new List<string> {
                                            "doc", "docm", "dotm", "xls", "xlsm", "xltm", "xlsb", "xlam", "ppt", "pptm", "ppsx", "ppsm", "potm", "ppam"
                                        }),
                            }
                        }
                    }
                },
                new()
                {
                    Name = I18NEntity.GetString("RM_FA_Discovery_OldNotesRule"),
                    Description = I18NEntity.GetString("RM_FA_Discovery_OldNotesRule_Desc"),
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                    Order = 2,
                    CriteriaInfoes = new()
                    {
                        new()
                        {
                            Order = 1,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                            LogicType = RMDiscoveryCriteriaLogicType.And,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.DateTime,
                                Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo { Unit = 2, UnitType = RMDiscoveryDateUnitType.Year }),
                            }
                        },
                        new()
                        {
                            Order = 2,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.DocumentType,
                            LogicType = RMDiscoveryCriteriaLogicType.None,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.Array,
                                Logic = (int)RMDiscoveryArrayConditionType.In,
                                Value = JsonConvert.SerializeObject(
                                        new List<string> {
                                            "PDF", "txt"
                                        }),
                            }
                        }
                    }
                },
                new()
                {
                    Name = I18NEntity.GetString("RM_FA_Discovery_OldVPZRule"),
                    Description = I18NEntity.GetString("RM_FA_Discovery_OldVPZRule_Desc"),
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                    Order = 3,
                    CriteriaInfoes = new()
                    {
                        new()
                        {
                            Order = 1,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                            LogicType = RMDiscoveryCriteriaLogicType.And,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.DateTime,
                                Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo { Unit = 6, UnitType = RMDiscoveryDateUnitType.Month }),
                            }
                        },
                        new()
                        {
                            Order = 2,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.DocumentType,
                            LogicType = RMDiscoveryCriteriaLogicType.None,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.Array,
                                Logic = (int)RMDiscoveryArrayConditionType.In,
                                Value = JsonConvert.SerializeObject(
                                        new List<string> {
                                            "mp4", "wmv", "avi", "mpg", "mov", "rm", "ram", "swf", "flv", "jpg", "jpeg", "png", "gif", "bmp", "zip", "rar", "jar"
                                        }),
                            }
                        }
                    }
                },
                new()
                {
                    Name = I18NEntity.GetString("RM_FA_Discovery_OldOtherFilesRule"),
                    Description = I18NEntity.GetString("RM_FA_Discovery_OldOtherFilesRule_Desc"),
                    Kind = RMDiscoveryRuleDefinitionKind.ROT,
                    AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                    Order = 4,
                    CriteriaInfoes = new()
                    {
                        new()
                        {
                            Order = 1,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                            LogicType = RMDiscoveryCriteriaLogicType.And,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.DateTime,
                                Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo { Unit = 6, UnitType = RMDiscoveryDateUnitType.Month }),
                            }
                        },
                        new()
                        {
                            Order = 2,
                            CriteriaType = (int)RMDiscoveryDocumentCriteriaType.DocumentType,
                            LogicType = RMDiscoveryCriteriaLogicType.None,
                            ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                            {
                                Category = RMDiscoveryConditionCategory.Array,
                                Logic = (int)RMDiscoveryArrayConditionType.NotIn,
                                Value = JsonConvert.SerializeObject(
                                        new List<string> {
                                            "doc", "docx", "docm", "dotx", "dotm", "xls", "xlsx", "xlsm", "xltx", "xltm", "xlsb", "xlam", "ppt", "pptx", "pptm", "ppsx", "ppsm", "potx", "potm", "ppam",
                                            "PDF", "txt",
                                            "mp4", "wmv", "avi", "mpg", "mov", "rm", "ram", "swf", "flv", "jpg", "jpeg", "png", "gif", "bmp", "zip", "rar", "jar"
                                        }),
                            }
                        }
                    }
                }
            },
            TrivialRules = new()
                {
                    new()
                    {
                        Name = I18NEntity.GetString("RM_FA_Discovery_CustomFolderFileRule"),
                        Description = I18NEntity.GetString("RM_FA_Discovery_CustomFolderFileRule_Desc"),
                        Kind = RMDiscoveryRuleDefinitionKind.ROT,
                        AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                        Order = 1,
                        CriteriaInfoes = new()
                        {
                            new ()
                            {
                                Order = 1,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ParentFolder,
                                LogicType = RMDiscoveryCriteriaLogicType.Or,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic = (int)RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                        new List<string>
                                        {
                                            "appdata", "desktop", "users", "dump"
                                        })
                                }
                            },
                            new ()
                            {
                                Order = 2,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.Name,
                                LogicType = RMDiscoveryCriteriaLogicType.None,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic = (int)RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                        new List<string>
                                        {
                                            "~*", "img_*", "dcm_*", "*copy*", "dump", "*download*"
                                        }),
                                }
                            },
                        }
                    },
                    new()
                    {
                        Name = I18NEntity.GetString("RM_FA_Discovery_CRAFileRule"),
                        Description = I18NEntity.GetString("RM_FA_Discovery_CRAFileRule_Desc"),
                        Kind = RMDiscoveryRuleDefinitionKind.ROT,
                        AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                        Order = 2,
                        CriteriaInfoes = new()
                        {
                            new ()
                            {
                                Order = 1,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ParentFolder,
                                LogicType = RMDiscoveryCriteriaLogicType.Or,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic =(int) RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                    new List<string>
                                    {
                                       "cache", "temp", "tmp", "recycle", "archive", "delete",
                                    }),
                                }
                            },
                            new ()
                            {
                                Order = 2,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.Name,
                                LogicType = RMDiscoveryCriteriaLogicType.None,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic =(int) RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                    new List<string>
                                    {
                                       "cache", "temp", "tmp", "recycle", "archive", "delete",
                                    }),
                                }
                            },
                        }
                    },
                    new()
                    {
                        Name = I18NEntity.GetString("RM_FA_Discovery_PrivateHistoryRule"),
                        Description = I18NEntity.GetString("RM_FA_Discovery_PrivateHistoryRule_Desc"),
                        Kind = RMDiscoveryRuleDefinitionKind.ROT,
                        AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                        Order = 3,
                        CriteriaInfoes = new()
                        {
                            new ()
                            {
                                Order = 1,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ParentFolder,
                                LogicType = RMDiscoveryCriteriaLogicType.Or,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic =(int) RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                    new List<string>
                                    {
                                       "private", "history",
                                    }),
                                }
                            },
                            new ()
                            {
                                Order = 2,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.Name,
                                LogicType = RMDiscoveryCriteriaLogicType.And,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.Array,
                                    Logic =(int) RMDiscoveryArrayConditionType.TextMatchIn,
                                    Value = JsonConvert.SerializeObject(
                                    new List<string>
                                    {
                                       "private", "history",
                                    }),
                                }
                            },
                            new ()
                            {
                                Order = 3,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                                LogicType = RMDiscoveryCriteriaLogicType.None,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.DateTime,
                                    Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                    Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo{ Unit = 1, UnitType = RMDiscoveryDateUnitType.Month }),
                                }
                            }
                        }
                    },
                    new()
                    {
                        Name = I18NEntity.GetString("RM_FA_Discovery_NoFileExtensionRule"),
                        Description =I18NEntity.GetString("RM_FA_Discovery_NoFileExtensionRule_Desc"),
                        Kind = RMDiscoveryRuleDefinitionKind.ROT,
                        AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument,
                        Order = 4,
                        CriteriaInfoes = new()
                        {
                            new ()
                            {
                                Order = 1,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.ModifiedTime,
                                LogicType = RMDiscoveryCriteriaLogicType.And,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.DateTime,
                                    Logic = (int)RMDiscoveryDateTimeConditionType.OlderThan,
                                    Value = JsonConvert.SerializeObject(new RMDiscoveryDateConditionOlderThanInfo{ Unit = 6, UnitType = RMDiscoveryDateUnitType.Month }),
                                }
                            },
                            new ()
                            {
                                Order = 2,
                                CriteriaType = (int)RMDiscoveryDocumentCriteriaType.DocumentType,
                                LogicType = RMDiscoveryCriteriaLogicType.None,
                                ConditionInfo = new RMDiscoveryRuleCriteriaConditionInfo
                                {
                                    Category = RMDiscoveryConditionCategory.BooleanLogic,
                                    Logic = (int)RMDiscoveryBooleanConditionType.IsEmpty,
                                    Value = "true",
                                }
                            }
                        }
                    }
                }
        };

        public static List<RMDiscoveryWithoutInDateDataInfo> DEFAULT_DATE_RANGE_INFOES => new()
        {
            new()
            {
                Unit = 1,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 0,
            },
            new()
            {
                Unit = 3,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 1,
            },
            new()
            {
                Unit = 5,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 2,
            },
            new()
            {
                Unit = 10,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 3,
            },
        };

        public static List<RMDiscoverySizeRangeDataInfo> DEFAULT_SIZE_RANGE_INFOES => new()
        {
            new()
            {
                GenerateEqual = 0,
                LessThan = 1,
                Order = 0,
                Name = "<1 MB",
            },
            new()
            {
                GenerateEqual = 1,
                LessThan = 50,
                Order = 1,
                Name = ">=1 MB",
            },
        };
    }
}
