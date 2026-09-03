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
    public class ContentDBGenerateSettingItem : BaseReportSettingItem
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
            var another = anotherItem as ContentDBGenerateSettingItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<ContentDBGenerateSettingItem> tempItems = new List<ContentDBGenerateSettingItem>();
            allItems.ForEach(i =>
            {
                ContentDBGenerateSettingItem tempItem = i as ContentDBGenerateSettingItem;
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
    public class ContentDBStorageReportItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {"Web Application","Content Database","Site Collection Count","Site Count",
                                                  "List Count","Total Size","SQL Size","Latest Size","Version Size","Site Collection RecycleBin Size",
                                                  "User RecycleBin Size","System Data Size","Extender Data Real Size", "Connector Data Real Size"};
        public static readonly string ContentDB = "Content Database";
        public ContentDBStorageReportItem()
        {
            Level = ContentDB;
        }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Level { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string WebApplication { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string ContentDatabase { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string SiteCollectionCount { set; get; }
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
                new AdminReportValue(){ Value = WebApplication, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ContentDatabase, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SiteCollectionCount, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SiteCount, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ListCount, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TotalSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = SQLSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = LatestSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = VersionSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = SiteCollectionRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = UserRecycleBinSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = SystemDataSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = ExtenderDataRealSize, ValueType = AdminReportValueType.UnitValue},
                new AdminReportValue(){ Value = ConnectorDataRealSize, ValueType = AdminReportValueType.UnitValue},
            };
        }
    }

    public class ContentDBPropertiesItem : BaseReportSettingItem
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
            var another = anotherItem as ContentDBPropertiesItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<ContentDBPropertiesItem> tempItems = new List<ContentDBPropertiesItem>();
            allItems.ForEach(i =>
            {
                ContentDBPropertiesItem tempItem = i as ContentDBPropertiesItem;
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
