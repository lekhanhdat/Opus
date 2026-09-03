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
    public class FarmConfigurationDatabaseItem : BaseReportSettingItem
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
            var another = anotherItem as FarmConfigurationDatabaseItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmDefaultDatabaseServerItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmDefaultDatabaseServerItem;
            if (!Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmAntivirusItem : BaseReportSettingItem
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
            var another = anotherItem as FarmAntivirusItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmOutgoingEmailSettingsItem : BaseReportSettingItem
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
            var another = anotherItem as FarmOutgoingEmailSettingsItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<FarmOutgoingEmailSettingsItem> tempItems = new List<FarmOutgoingEmailSettingsItem>();
            allItems.ForEach(i =>
            {
                FarmOutgoingEmailSettingsItem tempItem = i as FarmOutgoingEmailSettingsItem;
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
    public class FarmIncomingEmailSettingsItem : BaseReportSettingItem
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
            var another = anotherItem as FarmIncomingEmailSettingsItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<FarmIncomingEmailSettingsItem> tempItems = new List<FarmIncomingEmailSettingsItem>();
            allItems.ForEach(i =>
            {
                FarmIncomingEmailSettingsItem tempItem = i as FarmIncomingEmailSettingsItem;
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
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmCurrentLicenseItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmCurrentLicenseItem;
            if (!Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmTypeItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmTypeItem;
            if (!Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSecuritySettingsItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Value { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Value, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmSecuritySettingsItem;
            if (!Value.Equals(another.Value))
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmPropertiesItem : BaseReportSettingItem
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
            var another = anotherItem as FarmPropertiesItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<FarmPropertiesItem> tempItems = new List<FarmPropertiesItem>();
            allItems.ForEach(i =>
            {
                FarmPropertiesItem tempItem = i as FarmPropertiesItem;
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
    public class FarmServersAndServicesItem : BaseReportSettingItem
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ServerName { set; get; }

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
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSolutionsItem : BaseReportSettingItem
    {
        public static readonly string[] SolutionColumn = { "Name", "Status", "Deployment to" };

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Name { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Status { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string DeploymentTo { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Name, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Status, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = DeploymentTo, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmSolutionsItem;
            if (Name.Equals(another.Name) && Status.Equals(another.Status) && DeploymentTo.Equals(another.DeploymentTo))
            {
                return false;
            }
            return true;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmFeaturesItem : BaseReportSettingItem
    {
        public static readonly string[] FeatureColumn = { "Name", "Version", "Status", "Scope", "HiddenToUI", "Parent solution name" };

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Name { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Version { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Status { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Scope { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string HiddenToUI { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ParentSolutionName { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Name, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Version, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Status, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Scope, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = HiddenToUI, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ParentSolutionName, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmFeaturesItem;
            if (Name.Equals(another.Name) && Version.Equals(another.Version) && Status.Equals(another.Status)
                && Scope.Equals(another.Scope) && HiddenToUI.Equals(another.HiddenToUI) && ParentSolutionName.Equals(another.ParentSolutionName))
            {
                return false;
            }
            return true;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmFarmFeaturesItem : BaseReportSettingItem
    {
        public static readonly string[] FeatureColumn = { "Name", "ID", "Status" };

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Name { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ID { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Status { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Name, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ID, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Status, ValueType = AdminReportValueType.BasicValue},
            };
        }

        public override bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            var another = anotherItem as FarmFarmFeaturesItem;
            if (Name.Equals(another.Name) && ID.Equals(another.ID) && Status.Equals(another.Status))
            {
                return false;
            }
            return true;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmEnvironmentOverviewItem : BaseReportSettingItem
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
            var another = anotherItem as FarmEnvironmentOverviewItem;
            if (Key.Equals(another.Key) && !Value.Equals(another.Value))
            {
                return true;
            }
            List<FarmEnvironmentOverviewItem> tempItems = new List<FarmEnvironmentOverviewItem>();
            allItems.ForEach(i =>
            {
                FarmEnvironmentOverviewItem tempItem = i as FarmEnvironmentOverviewItem;
                if (tempItem != null)
                {
                    tempItems.Add(tempItem);
                }
            });
            int count = 0;
            tempItems.ForEach(d =>
            {
                if (string.Equals(d.Key, Key, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            });
            if (count == 1)
            {
                return true;
            }
            return false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmStorageReportItem : BaseReportSettingItem
    {
        public static readonly string[] Columns = {"Web Application","Content Database","Site Collection","Site",
                                                  "Total Size","SQL Size","Latest Size","Version Size","Site Collection RecycleBin Size",
                                                  "User RecycleBin Size","System Data Size","Extender Data Real Size", "Connector Data Real Size"};
        public static readonly string Farm = "Farm";
        public FarmStorageReportItem()
        {
            Level = Farm;
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
        public string SiteCollection { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Site { set; get; }
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
        public string SiteCollectionRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string UserRecycleBinSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_12)]
        public string SystemDataSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_13)]
        public string ExtenderDataRealSize { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_14)]
        public string ConnectorDataRealSize { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = WebApplication, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ContentDatabase, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SiteCollection, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Site, ValueType = AdminReportValueType.BasicValue},
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceItem : BaseReportSettingItem
    {
        public static readonly string MapColumn = ContractConstants.STRING_1;
        public static readonly string ApplicationNameMapColumn = ContractConstants.STRING_15;
        public static readonly string ProfileServices = "Profile Services";
        public static readonly string[] ProfileServicesColumns = { "Application Name", "My Site Host URL", "Personal Site Location" };
        public static readonly string SharedServices = "Shared Services";
        public static readonly string[] SharedServicesColumns = { "Application Name", "Administration Site URL" };

        public FarmSharedServiceItem()
        {
            SettingType = ReportSettingType.FarmSharedServiceItem;
        }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string MySiteHostURL { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string PersonalSiteLocation { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string AdministrationSiteURL { set; get; }

        public override List<AdminReportValue> Row()
        {
            if (ServiceType.Equals("Shared Services"))
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = ApplicationName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = AdministrationSiteURL, ValueType = AdminReportValueType.BasicValue},
                };
            }
            else
            {
                return new List<AdminReportValue>()
                {
                    new AdminReportValue(){ Value = ApplicationName, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = MySiteHostURL, ValueType = AdminReportValueType.BasicValue},
                    new AdminReportValue(){ Value = PersonalSiteLocation, ValueType = AdminReportValueType.BasicValue},
                };
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceSearchBasedAlertsItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string SearchBasedAlerts = "Search Based Alerts";
        public static readonly string[] SearchBasedAlertsColumns = { "Status" };
        public FarmSharedServiceSearchBasedAlertsItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = SearchBasedAlerts;
            SettingType = ReportSettingType.FarmSharedServiceSearchBasedAlertsItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Status { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Status, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceScopesItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string Scopes = "Scopes";
        public static readonly string[] ScopesColumns = { "Title", "Description", "Last Modified by", "Target Results Page" };
        public FarmSharedServiceScopesItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = Scopes;
            SettingType = ReportSettingType.FarmSharedServiceScopesItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Title { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Description { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string LastModifiedBy { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string TargetResultsPage { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Title, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Description, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = LastModifiedBy, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TargetResultsPage, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceMetadataPropertiesItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string MetadataProperties = "Metadata Properties";
        public static readonly string[] MetadataPropertiesColumns = { "Name", "Description", "Type", "Content using this property", "Mapping to crawled properties", "Use in scopes" };
        public FarmSharedServiceMetadataPropertiesItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = MetadataProperties;
            SettingType = ReportSettingType.FarmSharedServiceMetadataPropertiesItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ServiceType2 { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Name { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Description { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ContentUsingThisProperty { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string MappingToCrawledProperties { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string UseInScopes { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Name, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Description, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Type, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ContentUsingThisProperty, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = MappingToCrawledProperties, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = UseInScopes, ValueType = AdminReportValueType.BasicValue},
            };
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceFederatedLocationsItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string FederatedLocations = "Federated Locations";
        public static readonly string[] FederatedLocationsColumns = { "Location Name", "Display Name", "Description", "Author", "Version", "Location Information", "Query Template", "\"More Results\" Link Template", "Federated Search Results Display Metadata", "Top Federated Results Display Metadata", "Restrict Usage", "Specify Credentials" };
        public FarmSharedServiceFederatedLocationsItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = FederatedLocations;
            SettingType = ReportSettingType.FarmSharedServiceFederatedLocationsItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string LocationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string DisplayName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Description { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string Author { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string Version { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string LocationInformation { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string QueryTemplate { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string MoreResultsLinkTemplate { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string FederatedSearchResultsDisplayMetadata { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_12)]
        public string TopFederatedResultsDisplayMetadata { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_13)]
        public string RestrictUsage { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_14)]
        public string SpecifyCredentials { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = LocationName, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = DisplayName, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Description, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Author, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Version, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = LocationInformation, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = QueryTemplate, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = MoreResultsLinkTemplate, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = FederatedSearchResultsDisplayMetadata, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = TopFederatedResultsDisplayMetadata, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = RestrictUsage, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SpecifyCredentials, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceAuthoritativePagesItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string AuthoritativePages = "Authoritative Pages";
        public static readonly string[] AuthoritativePagesColumns = { "Most authoritative pages", "Second-level authoritative pages", "Third-level authoritative pages", "Non-authoritative Sites", "Refresh Now" };

        public FarmSharedServiceAuthoritativePagesItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = AuthoritativePages;
            SettingType = ReportSettingType.FarmSharedServiceAuthoritativePagesItem;
        }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string MostAuthoritativePages { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string SecondLevelAuthoritativePages { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string ThirdLevelAuthoritativePages { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string NonAuthoritativeSites { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string RefreshNow { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = MostAuthoritativePages, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = SecondLevelAuthoritativePages, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = ThirdLevelAuthoritativePages, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = NonAuthoritativeSites, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = RefreshNow, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceFileTypesItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string FileTypesStr = "File Types";
        public static readonly string[] FileTypesColumns = { "File Types" };
        public FarmSharedServiceFileTypesItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = FileTypesStr;
            SettingType = ReportSettingType.FarmSharedServiceFileTypesItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string FileTypes { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = FileTypes, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceCrawlRulesItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string CrawlRules = "Crawl Rules";
        public static readonly string[] CrawlRulesColumns = { "URL", "Include or exclude", "Authentication Type" };
        public FarmSharedServiceCrawlRulesItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = CrawlRules;
            SettingType = ReportSettingType.FarmSharedServiceCrawlRulesItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string URL { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string IncludeOrExclude { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string AuthenticationType { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = URL, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = IncludeOrExclude, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = AuthenticationType, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSharedServiceContentSourceItem : BaseReportSettingItem
    {
        public static readonly string SearchServiceApplicationSettings = "Search Service Application Settings";
        public static readonly string ContentSource = "Content Source";
        public static readonly string[] ContentSourceColumns = { "Name", "Details", "Start Addresses", "Crawl Settings", "Crawl Schedules", "Start Full Crawl" };
        public FarmSharedServiceContentSourceItem()
        {
            ServiceType = SearchServiceApplicationSettings;
            Type = ContentSource;
            SettingType = ReportSettingType.FarmSharedServiceContentSourceItem;
        }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_15)]
        public string ApplicationName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ServiceType { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string Type { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string Name { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Details { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_11)]
        public string StartAddresses { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string CrawlSettings { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string CrawlSchedules { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string StartFullCrawl { set; get; }

        public override List<AdminReportValue> Row()
        {
            return new List<AdminReportValue>()
            {
                new AdminReportValue(){ Value = Name, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = Details, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = StartAddresses, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = CrawlSettings, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = CrawlSchedules, ValueType = AdminReportValueType.BasicValue},
                new AdminReportValue(){ Value = StartFullCrawl, ValueType = AdminReportValueType.BasicValue},
            };
        }
    }

}
