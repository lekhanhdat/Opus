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

namespace ExchangeBackupUtility.Graph
{
    #region directory

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Microsoft.Exchange.WebServices.Data;

    using Folder = Microsoft.Exchange.WebServices.Data.Folder;

    #endregion

    public class ExchangeRootFolder : ExchangeFolder, IExchangeRootFolder
    {
        public ExchangeRootFolder(ExchangeMailbox mailbox, IEWSAuthObject authObj)
            : base(mailbox, null, authObj)
        {
            this.isRootFolder = true;
        }

        protected override void SetParentFolderId()
        {
            // Method intentionally left empty.
        }

        protected override void SetFolderId(ExchangeMailbox mailbox, string folderId)
        {
            this.inputFolderId = mailbox.RootFolderId;
        }

        protected override void SetImpersonateId(ExchangeService service, string impersonatedUserAddress)
        {
            logger.Info("Impersonate Id:{0}", impersonatedUserAddress);
            base.SetImpersonateId(service, impersonatedUserAddress);
        }

        public Dictionary<string, FolderChangeStateL> SyncFolderHierarchy()
        {
            var result = AssemblyCollection();
            string syncState = null;
            ChangeCollection<FolderChange> changes;
            var propertySet = new PropertySet(BasePropertySet.IdOnly) { new ExtendedPropertyDefinition(26293, MapiPropertyType.String),//Path
                new ExtendedPropertyDefinition(4340,MapiPropertyType.Boolean),//HiddenFolder
                 new ExtendedPropertyDefinition(13825, MapiPropertyType.Integer),//PR_FOLDER_TYPE
                FolderSchema.ParentFolderId, FolderSchema.FolderClass, FolderSchema.WellKnownFolderName, FolderSchema.DisplayName };

            do
            {
                //todo:qlluo:performance:sync with IdOnly, load other properties later
                //paging is not support for this request type
                try
                {
                    if (ExchangeGlobalConfig.IsRecoverableItemsMailbox)
                    {
                        changes = service.SyncFolderHierarchy(FolderId, new PropertySet(BasePropertySet.IdOnly), syncState).ExecuteAsyncTask();
                        result = LoadSingleFolderProperty(changes, propertySet, false);
                    }
                    else
                    {
                        changes = service.SyncFolderHierarchy(FolderId, propertySet, syncState).ExecuteAsyncTask();
                        var items = changes.Where(c => !c.Folder.ExcludeHiddenFolder() && !c.Folder.IsPersonMetadataOrSub() && !c.Folder.IsExcludeByFolderClass() && !c.Folder.IsExcludeByFolderWellknownName() && !c.Folder.IsExcludeByFilterProfile());
                        IEnumerableExtension.ForEach(items, c => result[c.FolderId.UniqueId] = GenerateFolderChangeStateL(c));
                    }

                }
                catch (ServiceResponseException sEx)
                {
                    logger.Error("An error occurred to sync folder hierarchy. Reason: {0}. ", sEx.ToString());
                    if (IsAuditFolder(sEx))
                    {
                        logger.Info("Try to sync folder hierarchy only Id.");
                        changes = service.SyncFolderHierarchy(FolderId, new PropertySet(BasePropertySet.IdOnly), syncState).ExecuteAsyncTask();
                        logger.Info("Try to load change folder property one by one.");
                        result = LoadSingleFolderProperty(changes, propertySet, true);
                        logger.Info("Finish to sync folder hierarchy.");
                    }
                    else if (sEx.Message.Contains("The mailbox operation failed"))
                    {
                        service.NeedTraceLogMethodNames.Add("SyncFolderHierarchy");
                        changes = service.SyncFolderHierarchy(FolderId, propertySet, syncState).ExecuteAsyncTask();
                    }
                    else
                    {
                        logger.Info("Failed to sync folder hierarchy, not for audits folder, no need to sync.");
                        throw;
                    }
                }
                syncState = changes.SyncState;
            }
            while (changes.MoreChangesAvailable);
            return result;
        }

        private bool IsAuditFolder(ServiceResponseException sEx)
        {
            if (sEx.ErrorCode == ServiceError.ErrorAccessDenied && sEx.Message.Equals(ExchangeConstants.ERRORMESSAGE_AUDITS_FOLDER, StringComparison.OrdinalIgnoreCase))
                return true;
            else
                return false;
        }

        private bool IsAuditFolder(Folder folder)
        {
            if (folder.WellKnownFolderName != null && (folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsDeletions
                || folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsPurges
                || folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsRoot
                || folder.WellKnownFolderName == WellKnownFolderName.RecoverableItemsVersions))
                return true;
            if (folder.ExtendedProperties.Count > 0 && folder.ExtendedProperties[0].Value.ToString().Equals(ExchangeConstants.CALENDAR_LOGGING, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private Dictionary<string, FolderChangeStateL> LoadSingleFolderProperty(ChangeCollection<FolderChange> changes, PropertySet propertySet, bool needFilterAuditFolder)
        {
            var result = AssemblyCollection();
            foreach (var tempChange in changes)
            {
                try
                {
                    tempChange.Folder.Load(propertySet).ExecuteAsyncTask();
                    if (!tempChange.Folder.ExcludeHiddenFolder() && !tempChange.Folder.IsPersonMetadataOrSub()
                        && !tempChange.Folder.IsExcludeByFolderClass()
                        && !tempChange.Folder.IsExcludeByFolderWellknownName()
                        && !tempChange.Folder.IsExcludeByFilterProfile()
                        && (!needFilterAuditFolder || !IsAuditFolder(tempChange.Folder))
                        )
                        result[tempChange.FolderId.UniqueId] = GenerateFolderChangeStateL(tempChange);
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred to load folder. FolderId: {0}. Reason: {1}.", tempChange.FolderId.UniqueId, ex.ToString());
                }
            }
            return result;
        }

        private FolderChangeStateL GenerateFolderChangeStateL(FolderChange change)
        {
            return new FolderChangeStateL()
            {
                Id = change.FolderId.UniqueId,
                ParentFolderId = change.Folder.ParentFolderId.UniqueId,
                Path = $"{this.Mailbox.OriginalMailboxAddress}{change.Folder.Path()}",
                Name = change.Folder.DisplayName,
            };
        }

        private Dictionary<string, FolderChangeStateL> AssemblyCollection()
        {
            return new Dictionary<string, FolderChangeStateL>()
            {
                {
                    this.FolderId,
                    new FolderChangeStateL
                    {
                        Id = this.FolderId,
                        ParentFolderId =null,
                        ItemChange =false,
                        Name =this.Mailbox.OriginalMailboxAddress,
                        Path = this.DisplayFolderPath
                    }
                }
            };
        }
    }
}
