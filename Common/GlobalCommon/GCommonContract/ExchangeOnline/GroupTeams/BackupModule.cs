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

namespace AvePoint.Core.License
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class BackupModuleExtension
    {
        public static bool HasSharePointData(this BackupModule module)
        {
            return module == BackupModule.Office365Group ||
                module == BackupModule.OneDrive ||
                module == BackupModule.ProjectOnline ||
                module == BackupModule.Teams ||
                module == BackupModule.SharePointSites ||
                module == BackupModule.YammerGroup;
        }

        public static bool HasExchangePointData(this BackupModule module)
        {
            return module == BackupModule.Office365Group ||
                module == BackupModule.Mailbox ||
                module == BackupModule.PublicFolder ||
                module == BackupModule.Teams ||
                module == BackupModule.YammerGroup ||
                module == BackupModule.PublicFolderMetadata;
        }

        public static bool SupportObjectId(this BackupModule module)
        {
            return module == BackupModule.Mailbox || module == BackupModule.PersonalChat || module == BackupModule.PowerBI || module == BackupModule.PowerAutomate || module == BackupModule.PowerApps;
        }

        public static bool SupportEDiscovery(this BackupModule module)
        {
            return module == BackupModule.Mailbox;
        }

        public static bool SupportRestoreCart(this BackupModule module)
        {
            return module == BackupModule.Mailbox|| module == BackupModule.SharePointSites || module == BackupModule.OneDrive;
        }

        public static bool SupportDailyRetention(this BackupModule module)
        {
            return module == BackupModule.Mailbox || module == BackupModule.SharePointSites || module == BackupModule.OneDrive || module == BackupModule.ProjectOnline || module == BackupModule.PublicFolder || module == BackupModule.Office365Group || module == BackupModule.Teams || module == BackupModule.YammerGroup;
        }

        public static bool SupportRansomwareDetection(this BackupModule module)
        {
            return module == BackupModule.SharePointSites || module == BackupModule.OneDrive || module == BackupModule.Office365Group || module == BackupModule.Teams;
        }

        public static IEnumerable<string> GenerateHighPath(this BackupModule module, string planId, string cycleId)
        {
            var paths = new List<string>();
            if (module.HasExchangePointData())
            {
                paths.Add($"data_exchange/{planId}/{cycleId}");
            }
            if (module.HasSharePointData())
            {
                paths.Add($"data_granular/Remote Farm/{planId}/{cycleId}");
            }
            return paths;
        }

        public static string GetDefaultPlanName(this BackupModule module)
        {
            switch (module)
            {
                case BackupModule.Mailbox:
                    return "Default_Mailbox";
                case BackupModule.SharePointSites:
                    return "Default_SharePoint_Site";
                case BackupModule.OneDrive:
                    return "Default_OneDrive";
                case BackupModule.Office365Group:
                    return "Default_Office365Group";
                case BackupModule.ProjectOnline:
                    return "Default_ProjectOnline";
                case BackupModule.PublicFolder:
                    return "Default_PublicFolder";
                case BackupModule.Teams:
                    return "Default_Office365Teams";
                case BackupModule.PublicFolderMetadata:
                    return "Default_PublicFolder_Metadata";
                case BackupModule.YammerGroup:
                    return "Default_YammerGroup";
                case BackupModule.PersonalChat:
                    return "Default_PersonalChat";
                case BackupModule.PowerBI:
                    return "Default_Office365PowerBI";
                case BackupModule.PowerAutomate:
                    return "Default_PowerAutomate";
                case BackupModule.PowerApps:
                    return "Default_PowerApps";
                default:
                    throw new Exception("Not available module");
            }
        }

        public static bool SupportApplicationPool(this BackupModule module)
        {
            return module == BackupModule.SharePointSites || module == BackupModule.OneDrive || module == BackupModule.Office365Group || module == BackupModule.Teams || module == BackupModule.YammerGroup;
        }

        public static bool IsGroupModule(this BackupModule module)
        {
            return module == BackupModule.Office365Group || module == BackupModule.Teams || module == BackupModule.YammerGroup;
        }

    }

    public enum BackupModule
    {
        [DisplayOrder(1)]
        Mailbox = 0,

        [DisplayOrder(3)]
        SharePointSites = 1,

        [DisplayOrder(2)]
        OneDrive = 2,

        [DisplayOrder(4)]
        Office365Group = 3,

        [DisplayOrder(7)]
        ProjectOnline = 4,

        [DisplayOrder(8)]
        PublicFolder = 5,

        [DisplayOrder(5)]
        Teams = 6,

        [DisplayOrder(100)]
        PublicFolderMetadata = 7,

        [DisplayOrder(9)]
        PrivateChannel = 8,

        [DisplayOrder(10)]
        YammerGroup = 9,

        [DisplayOrder(6)]
        PersonalChat = 10,

        [DisplayOrder(11)]
        PowerBI = 11,

        [DisplayOrder(12)]
        PowerAutomate = 12,

        [DisplayOrder(13)]
        SharedChannel = 13,

        [DisplayOrder(14)]
        PowerApps = 14
    }

    public enum GlobalSearchType
    {
        Site = 0,

        Mailbox = 1
    }
    public class DisplayOrderAttribute : Attribute
    {
        public int Order { get; }

        public DisplayOrderAttribute(int order)
        {
            Order = order;
        }
    }

    public static class BackupModuleExtensions
    {
        private static Dictionary<BackupModule, int> moduleOrderMapping = Enum.GetNames(typeof(BackupModule)).Select(n =>
        {
            var attr = typeof(BackupModule).GetMember(n).First().GetCustomAttributes(typeof(DisplayOrderAttribute), false).First() as DisplayOrderAttribute;
            return new
            {
                Module = (BackupModule)Enum.Parse(typeof(BackupModule), n),
                Order = attr.Order
            };
        }).ToDictionary(p => p.Module, p => p.Order);

        public static int DisplayOrderCompareTo(this BackupModule left, BackupModule right)
        {
            return moduleOrderMapping[left].CompareTo(moduleOrderMapping[right]);
        }
    }
}