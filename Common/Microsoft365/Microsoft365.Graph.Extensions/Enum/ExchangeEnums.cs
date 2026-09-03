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

namespace Microsoft365.Graph.Extensions;

/// <summary>
/// From the util.exchange (ews sdk), there are more enumerated items than in the official documentation, so the corresponding int values also differ.
/// https://learn.microsoft.com/en-us/dotnet/api/microsoft.exchange.webservices.data.wellknownfoldername?view=exchange-ews-api
/// </summary>
public enum WellKnownFolderName
{
    Calendar,
    Contacts,
    DeletedItems,
    Drafts,
    Inbox,
    Journal,
    Notes,
    Outbox,
    SentItems,
    Tasks,
    MsgFolderRoot,
    PublicFoldersRoot,
    Root,
    JunkEmail,
    SearchFolders,
    VoiceMail,
    RecoverableItemsRoot,
    RecoverableItemsDeletions,
    RecoverableItemsVersions,
    RecoverableItemsPurges,
    RecoverableItemsDiscoveryHolds,
    ArchiveRoot,
    ArchiveInbox,
    ArchiveMsgFolderRoot,
    ArchiveDeletedItems,
    ArchiveRecoverableItemsRoot,
    ArchiveRecoverableItemsDeletions,
    ArchiveRecoverableItemsVersions,
    ArchiveRecoverableItemsPurges,
    ArchiveRecoverableItemsDiscoveryHolds,
    SyncIssues,
    Conflicts,
    LocalFailures,
    ServerFailures,
    RecipientCache,
    QuickContacts,
    ConversationHistory,
    AdminAuditLogs,
    ToDoSearch,
    MyContacts,
    Directory,
    IMContactList,
    PeopleConnect,
    Favorites
}

public enum MsgFlag
{
    MSGFLAG_READ = 0x01,
    MSGFLAG_UNMODIFIED = 0x02,
    MSGFLAG_SUBMIT = 0x04,
    MSGFLAG_UNSENT = 0x08,
    MSGFLAG_HASATTACH = 0x10
}