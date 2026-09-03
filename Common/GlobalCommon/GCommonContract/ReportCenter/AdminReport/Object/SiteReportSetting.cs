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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteSecuritySettingItem : BaseReportSettingItem
    {
        public static readonly string Group = "Group Permission";
        public static readonly string[] GroupColumns = { "Group Name", "User Name", "Permission" };
        public static readonly string User = "User Permission";
        public static readonly string[] UserColumns = { "User Name", "Permission" };
        public static readonly string GroupOrUserNameMapColumn = ContractConstants.STRING_1;
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string GroupOrUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Username { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string GroupName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Permission { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string UUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string UPermission { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (GroupOrUserName.Equals(Group, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = GroupName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Username, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Permission, ValueType = AdminReportValueType.BasicValue},
                };
            }
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = UUserName, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = UPermission, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteSecuritySettingItem;
            if (GroupOrUserName.Equals(Group, StringComparison.CurrentCulture))
            {
                if (!GroupName.Equals(another.GroupName) || !Username.Equals(another.Username) || !Permission.Equals(another.Permission))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (!UUserName.Equals(another.UUserName) || !UPermission.Equals(another.UPermission))
                {
                    return true;
                }
                return false;
            }

        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteGeneralSettingItem : BaseReportSettingItem
    {
        public static readonly string[] KeyValue = new string[]{
            "isOrphanSite",
            "Author",
            "Owner",
            "Last Modifier",
            "Last Accessed Time",
            "Created",
            "Last Modified",
            "Size(MB)",
            "Description",
            "Database Name",
            "Parent Site",
            "site collection url",
            "Template ID"};

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (Key.Equals(SiteSettingNameConstants.Size))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.UnitValue},
                };
            }
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteGeneralSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteRSSSettingItem : BaseReportSettingItem
    {
        

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteRSSSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteAlertsSettingItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {
            "Alert Title",
            "User",
            "Frequency",
            "Location",
            "File Name",
            "File Size"
        };

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string AlertTitle { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string User { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Frequency { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Location { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string FileName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string FileSize { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = AlertTitle, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = User, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Frequency, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Location, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = FileName, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = FileSize, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteAlertsSettingItem;
            if (AlertTitle.Equals(another.AlertTitle) && User.Equals(another.User) && Frequency.Equals(another.Frequency) && Location.Equals(another.Location)
                && FileName.Equals(another.FileName) && FileSize.Equals(another.FileSize))
            {
                return false;
            }
            return true;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteRegionalSettingItem : BaseReportSettingItem
    {


        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteRegionalSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SitePropertiesItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_12)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SitePropertiesItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SitePropertiesItem> tempItems = new List<SitePropertiesItem>();
            allItems.ForEach(i =>
            {
                SitePropertiesItem tempItem = i as SitePropertiesItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteFeaturesItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteFeaturesItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteFeaturesItem> tempItems = new List<SiteFeaturesItem>();
            allItems.ForEach(i =>
            {
                SiteFeaturesItem tempItem = i as SiteFeaturesItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentAnalysisItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as ContentAnalysisItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<ContentAnalysisItem> tempItems = new List<ContentAnalysisItem>();
            allItems.ForEach(i =>
            {
                ContentAnalysisItem tempItem = i as ContentAnalysisItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListAndDocumentLibraryInformation : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Type { set; get; }
        //Trun On Email Sub Setting
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string TrunOnEmailListCount { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string EmailAddress { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ListName { set; get; }
        //Allow Request
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string AllowRequestListCount { set; get; }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ListSettingType
        {
            [EnumMember]
            Normal = 0,//key value
            [EnumMember]
            TrunOnEmailListCount = 1,
            [EnumMember]
            EmailAddress = 2,
            [EnumMember]
            ListName = 3,
            [EnumMember]
            AllowListToReceiveEmail = 4,
            [EnumMember]
            AllowListToReceiveEmailCount = 5,
            [EnumMember]
            AllowRequestAccess = 6,
            [EnumMember]
            AllowRequestAccessCount = 7,
        }

        public override List<AdminReportValue> Row()
        {
            if (Type.Equals(ListSettingType.Normal.ToString()) || Type.Equals(string.Empty))
            {
                if (Key.Equals(SiteSettingNameConstants.DocumentsTotalSize) || Key.Equals(SiteSettingNameConstants.DocumentVersionTotalSize) || Key.Equals(SiteSettingNameConstants.ListTotalSize) || Key.Equals(SiteSettingNameConstants.SurveyTotalSize) || Key.Equals(SiteSettingNameConstants.DiscussionBoardTotalSize))
                {
                    return new List<AdminReportValue>()
                    {
                        new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.Key},
                        new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.UnitValue},
                    };
                }
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.TrunOnEmailListCount.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = SiteSettingNameConstants.Number, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = TrunOnEmailListCount, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.EmailAddress.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = SiteSettingNameConstants.Recipients, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = EmailAddress, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.ListName.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = SiteSettingNameConstants.ListLibrary, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = ListName, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.AllowListToReceiveEmailCount.ToString()))
            { 
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = SiteSettingNameConstants.NumberOfListTurnedOnReceiveEmail, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = TrunOnEmailListCount, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.AllowListToReceiveEmail.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = ListName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = EmailAddress, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.AllowRequestAccessCount.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = SiteSettingNameConstants.NumberOfListAllowAccessRequestEmail, ValueType = AdminReportValueType.Key},
                    new AdminReportValue(){ Value = AllowRequestListCount, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else if (Type.Equals(ListSettingType.AllowRequestAccess.ToString()))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = ListName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = EmailAddress, ValueType = AdminReportValueType.BasicValue},
                };
            }
            return null;
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            if (Type.Equals(ListSettingType.Normal.ToString()) || Type.Equals(string.Empty))
            {
                var another = anotherItem as ListAndDocumentLibraryInformation;
                if (Key.Equals(another.Key) && !Value.Equals(another.Value))
                {
                    return true;
                }
                return false;
            }
            else if (Type.Equals(ListSettingType.AllowListToReceiveEmailCount.ToString()))
            {
                var another = anotherItem as ListAndDocumentLibraryInformation;
                if (!TrunOnEmailListCount.Equals(another.TrunOnEmailListCount))
                {
                    return true;
                }
                return false;
            }
            else if (Type.Equals(ListSettingType.AllowRequestAccessCount.ToString()))
            {
                var another = anotherItem as ListAndDocumentLibraryInformation;
                if (!AllowRequestListCount.Equals(another.AllowRequestListCount))
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubSitesAndPageInformationItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SubSitesAndPageInformationItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<AuditInfoSettingItem> tempItems = new List<AuditInfoSettingItem>();
            allItems.ForEach(i =>
            {
                AuditInfoSettingItem tempItem = i as AuditInfoSettingItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteUsageReport : BaseReportSettingItem
    {
        public static readonly string TotalHits = "Total Hits";
        public static readonly string HitsAllTime = "Top 10 Hit Pages (All Time)";
        public static readonly string HitsLastMonth = "Top 10 Hit Pages (Last Month)";
        public static readonly string UsersAllTime = "Top 10 Users (All Time)";
        public static readonly string UsersLastMonth = "Top 10 Users (Last Month)";
        public static readonly string LeastHitsAllTime = "Top 10 Least Hit Pages(All Time)";
        public static readonly string LeastHitsLastMonth = "Top 10 least Hit Pages(Last Month)";
        public static readonly string[] Columns = { "Name(URL)", "Hits" };
        public static readonly string[] UserHitsColumns = { "User", "Hits" };

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Value { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string UserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Url { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string Hits { set; get; }

        public override List<AdminReportValue> Row() 
        {
            if (Type.Equals(TotalHits, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(HitsAllTime, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(HitsLastMonth, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(LeastHitsAllTime, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(LeastHitsLastMonth, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(UsersAllTime, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = UserName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(UsersLastMonth, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = UserName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            return null;
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteSearch : BaseReportSettingItem
    {
        public static readonly string[] Column = new string[]{
        "Search Center and Custom Scopes",
        "Site Search Dropdown Mode",
        "Site Search Box Target Results Page",
        "Allow this web to appear in search results?",
        "The site's ASPX page indexing behavior"
        };
        public static readonly string SearchSettings = "Search Settings";
        public static readonly string SearchVisibility = "Search Visibility";

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Group { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as SiteSearch;
            if (Group.Equals(another.Group) && Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<AuditInfoSettingItem> tempItems = new List<AuditInfoSettingItem>();
            allItems.ForEach(i =>
            {
                AuditInfoSettingItem tempItem = i as AuditInfoSettingItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteStorageReportItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {"Site Collection","Site","URL","List",
                                                  "Total Size","Latest Size","Version Size",
                                                  "User RecycleBin Size"};
        public static readonly string SiteStr = "Site";
        public SiteStorageReportItem()
        {
            Level = SiteStr;
        }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Level { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string SiteCollection { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Site { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string URL { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string List { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string TotalSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string SQLSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string LatestSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string VersionSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string UserRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string SystemDataSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_12)]
        public string ExtenderDataRealSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_13)]
        public string ConnectorDataRealSize { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = SiteCollection, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Site, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = URL, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = List, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TotalSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SQLSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = LatestSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = VersionSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = UserRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SystemDataSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ExtenderDataRealSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ConnectorDataRealSize, ValueType = AdminReportValueType.UnitValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListGeneralSettingItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (Key.Equals(ListSettingNameConstants.ListTotalSize))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.UnitValue},
                };
            }
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as ListGeneralSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<AuditInfoSettingItem> tempItems = new List<AuditInfoSettingItem>();
            allItems.ForEach(i =>
            {
                AuditInfoSettingItem tempItem = i as AuditInfoSettingItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListSecuritySettingItem : BaseReportSettingItem
    {
        public static readonly string Group = "Group";
        public static readonly string[] GroupColumns = { "Group Name", "User Name", "Permission" };
        public static readonly string User = "User";
        public static readonly string[] UserColumns = { "User Name", "Permission" };
        public static readonly string GroupOrUserNameMapColumn = ContractConstants.STRING_1;

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string GroupOrUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Username { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string GroupName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Permission { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string UUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string UPermission { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (GroupOrUserName.Equals(Group, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = GroupName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Username, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Permission, ValueType = AdminReportValueType.BasicValue},
                };
            }
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = UUserName, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = UPermission, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListStorageReportItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {"Site","List", "Total Size","Latest Size","Version Size"};
        public static readonly string ListStr = "List";
        public ListStorageReportItem()
        {
            Level = ListStr;
        }

        public static readonly string MapColumn = ContractConstants.STRING_1;
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Level { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Site { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string List { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string TotalSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string SQLSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string LatestSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string VersionSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string UserRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string SystemDataSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string ExtenderDataRealSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string ConnectorDataRealSize { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Site, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = List, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TotalSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SQLSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = LatestSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = VersionSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = UserRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SystemDataSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ExtenderDataRealSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ConnectorDataRealSize, ValueType = AdminReportValueType.UnitValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditInfoSettingItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            AdminReportValue adminReportValue = null;
            if (string.Equals(Key, SiteSettingNameConstants.ApproxSizeOfSiteAuditRecords))
            {
                adminReportValue = new AdminReportValue { Value = Value, ValueType = AdminReportValueType.UnitValue };
            }
            else
            {
                adminReportValue = new AdminReportValue { Value = Value, ValueType = AdminReportValueType.BasicValue };
            }
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Key, ValueType = AdminReportValueType.BasicValue},

                adminReportValue,
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as AuditInfoSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<AuditInfoSettingItem> tempItems = new List<AuditInfoSettingItem>();
            allItems.ForEach(i => 
            {
                AuditInfoSettingItem tempItem = i as AuditInfoSettingItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            var count = tempItems.Count(d => string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase));
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }
}
