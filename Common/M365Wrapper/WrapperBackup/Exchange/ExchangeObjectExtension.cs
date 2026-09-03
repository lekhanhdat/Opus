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

namespace ExchangeUtility.Graph
{
    using AvePoint.RA.CommonUtil;
    using ExchangeCommonWrapper;

    using Microsoft.Exchange.WebServices.Data;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    static class FolderExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(GraphMethodExtension));
        private static ExtendedPropertyDefinition EPD_FOLDER_PATH = new ExtendedPropertyDefinition(26293, MapiPropertyType.String);

        public static bool IsExcludeByFolderClass(this Folder folder)
        {
            var lowerCaseFolderClass = (folder.FolderClass ?? string.Empty).ToLowerInvariant();
            //已经通过HiddenFolder过滤掉
            //if (lowerCaseFolderClass.Equals(ipf.contacts)) return IsSpecialFolder(folder);
            if (//lowerCaseFolderClass.StartsWith("ipf.configuration", StringComparison.Ordinal) ||
                lowerCaseFolderClass.StartsWith("ipf.note.socialconnector.feeditems", StringComparison.Ordinal) ||
                lowerCaseFolderClass.Equals("ipf.webextension"))
            {
                return true;
            }
            return false;
        }

        public static bool ExcludeHiddenFolder(this Folder folder)
        {
            return (IsHiddenFolder(folder) && !ExchangeGlobalConfig.EnableHideFolder) || IsSearchFolder(folder);
        }

        private static bool IsHiddenFolder(Folder folder)
        {
            if (folder.TryGetProperty(new ExtendedPropertyDefinition(4340, MapiPropertyType.Boolean), out Boolean isHidden))
            {
                return isHidden;
            }
            return false;
        }
        private static bool IsSearchFolder(Folder folder)
        {
            if (folder.TryGetProperty(new ExtendedPropertyDefinition(13825, MapiPropertyType.Integer), out int isSearch))
            {
                return isSearch == 2;
            }
            return false;
        }

        //public static bool IsSpecialFolder(Folder folder)
        //{
        //    return ExchangeGlobalConfig.BackupExceptPersonMetadata && folder.DisplayName.Equals("PersonMetadata", StringComparison.OrdinalIgnoreCase);
        //}
        public static bool IsExcludeByFolderWellknownName(this Folder folder)
        {
            if (folder.TryGetProperty(FolderSchema.WellKnownFolderName, out var value))
            {
                if (!ExchangeGlobalConfig.IncludeDeletedItems && folder.WellKnownFolderName is WellKnownFolderName.DeletedItems or WellKnownFolderName.ArchiveDeletedItems) return true;
                if (!ExchangeGlobalConfig.IncludeJunkEmail && folder.WellKnownFolderName == WellKnownFolderName.JunkEmail) return true;
                if (folder.WellKnownFolderName == WellKnownFolderName.SyncIssues || folder.WellKnownFolderName == WellKnownFolderName.Conflicts
                    || folder.WellKnownFolderName == WellKnownFolderName.LocalFailures || folder.WellKnownFolderName == WellKnownFolderName.ServerFailures) return true;
                if (ExchangeGlobalConfig.IsRecoverableItemsMailbox &&
                    folder.WellKnownFolderName != WellKnownFolderName.RecoverableItemsDeletions &&
                    folder.WellKnownFolderName != WellKnownFolderName.ArchiveRecoverableItemsDeletions) return true;
            }
            return false;
        }

        private static bool IsExcludeByFolderNameAndWellknownName4RecoverbleItems(Folder folder)
        {
            if (folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsDiscoveryHolds) return true;
            if (folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsVersions) return true;
            if (folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsPurges) return true;
            if (folder.DisplayName == "Audits") return true;
            if (folder.DisplayName == "SubstrateHolds") return true;
            if (folder.DisplayName == "Calendar Logging") return true;
            return false;
        }

        public static bool IsExcludeByFilterProfile(this Folder folder)
        {
            if (ExchangeGlobalConfig.FolderFilterProfile?.Any() ?? false)
            {
                try
                {
                    foreach (var filter in ExchangeGlobalConfig.FolderFilterProfile)
                    {
                        if (filter.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                        {
                            if (filter.Value.Contains(folder.DisplayName, StringComparer.OrdinalIgnoreCase)) return true;
                        }
                        else if (filter.Key.Equals("path", StringComparison.OrdinalIgnoreCase))
                        {
                            var path = folder.Path();
                            if (null == path)
                            {
                                logger.Warn("The path of foler[{0}] is not obtained, so skip the path filter.", folder.DisplayName);
                                continue;
                            }
                            if (filter.Value.Contains(path, StringComparer.OrdinalIgnoreCase)) return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while comparing filter profile. Foler name[{0}]. Reason {1} ", folder.DisplayName, ex.ToString());
                    return false;
                }
            }
            return false;
        }
        public static FindItemsResults<Item> FindItems(this Folder folder, SearchFilter searchFilter, ItemView view, ExchangeService service)
        {
            return service.FindItems(folder.Id, searchFilter, view).ExecuteAsyncTask();
        }

        public static string Path(this Folder folder)
        {
            string path = null;
            folder.TryGetProperty(EPD_FOLDER_PATH, out path);
            return path?.Replace("\ufffe", ExchangeConstants.PathCombine);
        }
        public static bool IsPersonMetadataOrSub(this Folder folder)
        {
            var path = folder.Path().Split('\\');
            return path[0].Equals(string.Empty) && path[1].Equals("PersonMetadata", StringComparison.OrdinalIgnoreCase);
        }
    }

    static class ItemExtension
    {
        private const int MSGFLAG_UNSENT = 0x8;
        private const int MSGFLAG_READ = 0x1;

        /// <summary>
        /// MSGFLAG_FROMME, MSGFLAG_UNMODIFIED is not included
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int MessageFlag(this Item item)
        {
            int messageFlag = 0;
            bool isDraft = false;
            if (item.TryGetProperty(ItemSchema.IsDraft, out isDraft) && isDraft)
            {
                messageFlag |= MSGFLAG_UNSENT;
            }
            var message = item as EmailMessage;
            if (message != null && message.IsRead)
            {
                messageFlag |= MSGFLAG_READ;
            }
            return messageFlag;
        }

        public static ImportanceM ToM(this Importance self)
        {
            return (ImportanceM)self;
        }

        public static Importance ToEX(this ImportanceM self)
        {
            return (Importance)self;
        }
    }

    static class ChangeTypeExtension
    {
        public static ChangeStatus ToChangeStatus(this ChangeType changeType)
        {
            switch (changeType)
            {
                case ChangeType.Delete:
                    return ChangeStatus.Delete;
                case ChangeType.Update:
                    return ChangeStatus.Update;
                case ChangeType.Create:
                default:
                    return ChangeStatus.Create;
            }
        }
    }

    static class FolderPermissionExtension
    {
        public static FolderPermissionCollectionM ToM(this FolderPermissionCollection fpc)
        {
            return new FolderPermissionCollectionM()
            {
                Permissions = fpc.Select(fp => fp.ToM()).ToList()
            };
        }

        public static FolderPermissionM ToM(this FolderPermission fp)
        {
            var pd = fp.PermissionLevel == FolderPermissionLevel.Custom ?
                new IndividualFolderPermissionsM
                {
                    CanCreateItems = fp.CanCreateItems,
                    CanCreateSubFolders = fp.CanCreateSubFolders,
                    DeleteItems = (PermissionScopeM)fp.DeleteItems,
                    EditItems = (PermissionScopeM)fp.EditItems,
                    IsFolderContact = fp.IsFolderContact,
                    IsFolderOwner = fp.IsFolderOwner,
                    IsFolderVisible = fp.IsFolderVisible,
                    ReadItems = (FolderPermissionReadAccessM)fp.ReadItems,
                } : null;
            return new FolderPermissionM()
            {
                PermissionDetails = pd,
                PermissionLevel = (FolderPermissionLevelM)fp.PermissionLevel,
                UserId = fp.UserId.ToM(),
            };
        }

        public static UserIdM ToM(this UserId uid)
        {
            return new UserIdM()
            {
                DisplayName = uid.DisplayName,
                PrimarySmtpAddress = uid.PrimarySmtpAddress,
                SID = uid.SID,
                StandardUser = (StandardUserM?)uid.StandardUser,
            };
        }

        public static IEnumerable<FolderPermission> ToEx(this IEnumerable<FolderPermissionM> fpcM)
        {
            return fpcM.Select(fpM => fpM.ToEx());
        }

        public static FolderPermission ToEx(this FolderPermissionM fpM)
        {
            if (fpM.PermissionLevel == FolderPermissionLevelM.Custom)
            {
                return new FolderPermission()
                {
                    CanCreateItems = fpM.PermissionDetails.CanCreateItems,
                    CanCreateSubFolders = fpM.PermissionDetails.CanCreateSubFolders,
                    DeleteItems = (PermissionScope)fpM.PermissionDetails.DeleteItems,
                    EditItems = (PermissionScope)fpM.PermissionDetails.EditItems,
                    IsFolderContact = fpM.PermissionDetails.IsFolderContact,
                    IsFolderOwner = fpM.PermissionDetails.IsFolderOwner,
                    IsFolderVisible = fpM.PermissionDetails.IsFolderVisible,
                    ReadItems = (FolderPermissionReadAccess)fpM.PermissionDetails.ReadItems,

                    UserId = fpM.UserId.ToEx(),
                };
            }
            return new FolderPermission(fpM.UserId.ToEx(), (FolderPermissionLevel)fpM.PermissionLevel);
        }

        public static UserId ToEx(this UserIdM uIdM)
        {
            return new UserId()
            {
                DisplayName = uIdM.DisplayName,
                PrimarySmtpAddress = uIdM.PrimarySmtpAddress,
                SID = uIdM.SID,
                StandardUser = (StandardUser?)uIdM.StandardUser,
            };
        }

        public static void Merge(this FolderPermissionCollection fpc1, IEnumerable<FolderPermission> fpc2)
        {
            foreach (var fp2 in fpc2)
            {
                if (fp2.UserId.StandardUser.HasValue) continue;

                var fp1 = fpc1.FirstOrDefault(fpArg => fpArg.EqualByUserId(fp2));

                if (fp1 != null)// find
                {
                    if (fp1.SamePermission(fp2)) continue;// same not update
                    fpc1.Remove(fp1);
                }
                fpc1.Add(fp2);

            }
        }

        public static void Replace(this FolderPermissionCollection fpc1, IEnumerable<FolderPermission> fpc2)
        {
            RemovePermissions(fpc1, fpc2);
            fpc1.Merge(fpc2);
        }

        internal static void RemovePermissions(FolderPermissionCollection fpc1, IEnumerable<FolderPermission> fpc2)
        {
            for (int t = fpc1.Count - 1; t >= 0; t--)
            {
                var fp1 = fpc1[t];
                if (fp1.UserId.StandardUser.HasValue) continue;

                var fp2 = fpc2.FirstOrDefault(fpArg => fpArg.EqualByUserId(fp1));
                if (fp2 != null && fp2.SamePermission(fp1)) continue;
                fpc1.Remove(fp1);
            }
        }

        public static bool EqualByUserId(this FolderPermission fp1, FolderPermission fp2)
        {
            if (fp2?.UserId?.PrimarySmtpAddress == null) return false;
            return string.Equals(fp1.UserId.PrimarySmtpAddress, fp2.UserId.PrimarySmtpAddress, StringComparison.OrdinalIgnoreCase);
        }

        public static bool SamePermission(this FolderPermission fp1, FolderPermission fp2)
        {
            if (fp1.PermissionLevel != fp2.PermissionLevel) return false;
            //fp1==fp2
            if (fp1.PermissionLevel != FolderPermissionLevel.Custom) return true;
            return
                fp1.CanCreateItems == fp2.CanCreateItems &&
                fp1.CanCreateSubFolders == fp2.CanCreateSubFolders &&
                fp1.IsFolderContact == fp2.IsFolderContact &&
                fp1.IsFolderVisible == fp2.IsFolderVisible &&
                fp1.IsFolderOwner == fp2.IsFolderOwner &&
                fp1.EditItems == fp2.EditItems &&
                fp1.DeleteItems == fp2.DeleteItems &&
                fp1.ReadItems == fp2.ReadItems;
        }

        //public static List<string> ToPermissionList(this FolderPermission permission)
        //{
        //    var list = new List<string>();
        //    if (permission.CanCreateItems)
        //    {
        //        list.Add("CreateItems");
        //    }
        //    if (permission.CanCreateSubFolders)
        //    {
        //        list.Add("CreateSubfolders");
        //    }
        //    if (permission.ReadItems == FolderPermissionReadAccess.FullDetails)
        //    {
        //        list.Add("ReadItems");
        //    }
        //    if (permission.DeleteItems == PermissionScope.All)
        //    {
        //        list.Add("DeleteAllItems");
        //    }
        //    if (permission.DeleteItems == PermissionScope.Owned)
        //    {
        //        list.Add("DeleteOwnItems");
        //    }
        //    if (permission.IsFolderContact)
        //    {
        //        list.Add("FolderContact");
        //    }
        //    if (permission.IsFolderVisible)
        //    {
        //        list.Add("FolderVisible");
        //    }
        //    if (permission.IsFolderOwner)
        //    {
        //        list.Add("FolderOwner");
        //    }
        //    if (permission.EditItems == PermissionScope.All)
        //    {
        //        list.Add("EditAllItems");
        //    }
        //    if (permission.EditItems == PermissionScope.Owned)
        //    {
        //        list.Add("EditOwnItems");
        //    }
        //    return list;
        //}
    }
    public enum ChangeStatus
    {
        Create = 0,
        Update = 1,
        Delete = 2
    }

    static class EmailAddressExtension
    {
        public static string ToFormatString(this EmailAddress address)
        {
            if (string.IsNullOrEmpty(address.Address)) return address.Name;
            if (string.IsNullOrEmpty(address.Name)) return address.Address;
            return string.Format("{0} <{1}>", address.Name, address.Address);
        }

        public static string ToFormatString(string name, string address)
        {
            if (string.IsNullOrEmpty(address)) return name;
            if (string.IsNullOrEmpty(name)) return address;
            return string.Format("{0} <{1}>", name, address);
        }
    }

    class ItemChangeComparer : IEqualityComparer<ItemChange>
    {
        public bool Equals(ItemChange x, ItemChange y)
        {
            var xItemId = x?.ItemId?.UniqueId;
            var yItemId = y?.ItemId?.UniqueId;
            return string.Equals(xItemId, yItemId, StringComparison.Ordinal);
        }

        public int GetHashCode(ItemChange obj)
        {
            return obj?.ItemId?.GetHashCode() ?? 0;
        }
    }
}