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

namespace ExchangeUtility
{
    using AvePoint.RA.CommonUtil;
    using ExchangeCommonWrapper;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static class FolderExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FolderExtension));
        private static ExtendedPropertyDefinition EPD_FOLDER_PATH = new ExtendedPropertyDefinition(26293, MapiPropertyType.String);

        public static bool IsExcludeByFolderClass(this Folder folder)
        {
            var lowerCaseFolderClass = (folder.FolderClass ?? string.Empty).ToLowerInvariant();
            switch (lowerCaseFolderClass)
            {
                case "ipf.contact.galcontacts":
                case "ipf.contact.recipientcache":
                case "ipf.contact.moc.imcontactlist":
                case "ipf.contact.moc.quickcontacts":
                case "ipf.files"://Files folder
                case "ipf.webextension"://WebExtAddins
                    return true;
                default:
                    break;
            }
            if (lowerCaseFolderClass.StartsWith("ipf.configuration", StringComparison.Ordinal) ||
                lowerCaseFolderClass.StartsWith("ipf.note.socialconnector.feeditems", StringComparison.Ordinal))
            {
                return true;
            }
            //skip process contact type folder and contact folder file.
            if (folder is ContactsFolder)
            {
                //Add log here to output contact folder name if need.
                logger.Info($"Current folder:{folder.DisplayName} is ContactsFolder and skip process.");
                return true;
            }
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
        public static List<string> ToPermissionList(this FolderPermission permission)
        {
            var list = new List<string>();
            if (permission.CanCreateItems)
            {
                list.Add("CreateItems");
            }
            if (permission.CanCreateSubFolders)
            {
                list.Add("CreateSubfolders");
            }
            if (permission.ReadItems == FolderPermissionReadAccess.FullDetails)
            {
                list.Add("ReadItems");
            }
            if (permission.DeleteItems == PermissionScope.All)
            {
                list.Add("DeleteAllItems");
            }
            if (permission.DeleteItems == PermissionScope.Owned)
            {
                list.Add("DeleteOwnItems");
            }
            if (permission.IsFolderContact)
            {
                list.Add("FolderContact");
            }
            if (permission.IsFolderVisible)
            {
                list.Add("FolderVisible");
            }
            if (permission.IsFolderOwner)
            {
                list.Add("FolderOwner");
            }
            if (permission.EditItems == PermissionScope.All)
            {
                list.Add("EditAllItems");
            }
            if (permission.EditItems == PermissionScope.Owned)
            {
                list.Add("EditOwnItems");
            }
            return list;
        }

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

        public static bool ExcludeHiddenFolder(this Folder folder)
        {
            return (IsHiddenFolder(folder) && !ExchangeGlobalConfig.EnableHideFolder) || IsSearchFolder(folder);
        }

        private static bool IsSearchFolder(Folder folder)
        {
            if (folder.TryGetProperty(new ExtendedPropertyDefinition(13825, MapiPropertyType.Integer), out int isSearch))
            {
                return isSearch == 2;
            }
            return false;
        }

        private static bool IsHiddenFolder(Folder folder)
        {
            if (folder.TryGetProperty(new ExtendedPropertyDefinition(4340, MapiPropertyType.Boolean), out Boolean isHidden))
            {
                return isHidden;
            }
            return false;
        }

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
