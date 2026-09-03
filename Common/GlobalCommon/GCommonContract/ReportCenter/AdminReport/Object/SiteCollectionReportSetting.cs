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
    public class SiteCollectionGeneralSettingItem : BaseReportSettingItem
    {
        public static readonly string[] KeyValue = new string[]{
            "Size(MB)",
            "Administrators",
            "Description",
            "Created",
            "Last Modified",
            "Last Accessed Time",
            "Portal Connection",
            "Owners",
            "Lock",
            "Quota",
            "Bandwidth",
        "Discussion Storage",
        "Visits"};

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Key { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (Key.Equals(SiteCollectionSettingNameConstants.Size))
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
            var another = anotherItem as SiteCollectionGeneralSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteCollectionGeneralSettingItem> tempItems = new List<SiteCollectionGeneralSettingItem>();
            allItems.ForEach(i =>
            {
                SiteCollectionGeneralSettingItem tempItem = i as SiteCollectionGeneralSettingItem;
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
    public class SiteCollectionRSSSettings : BaseReportSettingItem
    {
        public static readonly string[] Columns = new string[]{
        "Allow RSS feeds in this site collection",
        "Allow RSS feeds in this site",
        "Copyright",
        "Managing Editor",
        "Webmaster",
        "Time To Live (minutes)"
        };

        [DataMember]
        public string Key { set; get; }
        [DataMember]
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
            var another = anotherItem as SiteCollectionRSSSettings;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteCollectionRSSSettings> tempItems = new List<SiteCollectionRSSSettings>();
            allItems.ForEach(i =>
            {
                SiteCollectionRSSSettings tempItem = i as SiteCollectionRSSSettings;
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
    public class SiteCollectionFeatures : BaseReportSettingItem
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
            var another = anotherItem as SiteCollectionFeatures;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteCollectionFeatures> tempItems = new List<SiteCollectionFeatures>();
            allItems.ForEach(i =>
            {
                SiteCollectionFeatures tempItem = i as SiteCollectionFeatures;
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
    public class SiteCollectionSearch : BaseReportSettingItem
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
            var another = anotherItem as SiteCollectionSearch;
            if (Group.Equals(another.Group) && Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteCollectionSearch> tempItems = new List<SiteCollectionSearch>();
            allItems.ForEach(i =>
            {
                SiteCollectionSearch tempItem = i as SiteCollectionSearch;
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
    public class SiteCollectionContentAnalysis : BaseReportSettingItem
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
            var another = anotherItem as SiteCollectionContentAnalysis;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<SiteCollectionContentAnalysis> tempItems = new List<SiteCollectionContentAnalysis>();
            allItems.ForEach(i =>
            {
                SiteCollectionContentAnalysis tempItem = i as SiteCollectionContentAnalysis;
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
    public class SiteCollectionUsageReport : BaseReportSettingItem
    {
        public static readonly string TotalHits = "Total Hits";
        public static readonly string HitSitesAllTime = "Top 10 Hit Sites (All time)";
        public static readonly string HitSitesLastMonth = "Top 10 Hit Sites (Last Month)";
        public static readonly string HitsAllTime = "Top 10 Hit Pages (All Time)";
        public static readonly string HitsLastMonth = "Top 10 Hit Pages (Last Month)";
        public static readonly string UsersAllTime = "Top 10 Users (All Time)";
        public static readonly string UsersLastMonth = "Top 10 Users (Last Month)";
        public static readonly string LeastHitSitsAllTime = "Top 10 Least Hit Sites (All Time)";
        public static readonly string LeastHitSitesLastMonth = "Top 10 Least Hit Sites(Last Month)";
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
            if (Type.Equals(HitSitesAllTime, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(HitSitesLastMonth, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
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
            if (Type.Equals(LeastHitSitsAllTime, StringComparison.CurrentCulture))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = Url, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = Hits, ValueType = AdminReportValueType.BasicValue},
                };
            }
            if (Type.Equals(LeastHitSitesLastMonth, StringComparison.CurrentCulture))
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
    public class SiteCollectionStorageReportItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {"Site Collection","URL","Site Count",
                                                  "List Count","Total Size","Latest Size","Version Size","Site Collection RecycleBin Size",
                                                  "User RecycleBin Size"};
        public static readonly string SiteCollectionStr = "Site Collection";
        public SiteCollectionStorageReportItem()
        {
            Level = SiteCollectionStr;
        }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Level { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ContentDatabase { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string SiteCollection { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string URL { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string SiteCount { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ListCount { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string TotalSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string SQLSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string LatestSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string VersionSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string SiteCollectionRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_12)]
        public string UserRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_13)]
        public string SystemDataSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_14)]
        public string ExtenderDataRealSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ConnectorDataRealSize { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                //new AdminReportValue(){ Value = ContentDatabase, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SiteCollection, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = URL, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SiteCount, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ListCount, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TotalSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SQLSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = LatestSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = VersionSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = SiteCollectionRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = UserRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = SystemDataSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ExtenderDataRealSize, ValueType = AdminReportValueType.UnitValue},
                //new AdminReportValue(){ Value = ConnectorDataRealSize, ValueType = AdminReportValueType.UnitValue},
            };
        }
    }
}
