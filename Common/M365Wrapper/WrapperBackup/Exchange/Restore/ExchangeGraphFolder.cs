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

//namespace ExchangeRestoreUtility
//{
//    #region

//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using AngleSharp.Common;
//    using ExchangeUtility.Graph;
//    using Microsoft.Exchange.WebServices.Data;
//    using Microsoft365.Graph.Extensions;
//    using Microsoft365.Graph.Service;
//    using Microsoft365.Graph.Service.ImportItems;
//    using Microsoft365.Graph.Util;

//    #endregion

//    public class ExchangeGraphFolder : ExchangeFolder
//    {

//        public string CurrentMailboxId { get; private set; }
//        private string tempMailboxId;
//        public string CurrentFolderId { get; private set; }
//        private MailItemUploader mailItemUploader;

//        public ExchangeGraphFolder(ExchangeMailbox mailbox, GraphService graphService) : base(mailbox, graphService)
//        {
//        }

//        public ExchangeGraphFolder(ExchangeGraphFolder graphFolder) : base(graphFolder.Mailbox, graphFolder.GetGraphService())
//        {
//            CurrentMailboxId = graphFolder.CurrentMailboxId;
//            CurrentFolderId = graphFolder.CurrentFolderId;
//        }

//        private (string ParentPath, string FolderName) GetParentPathAndFolderName(string folderPath)
//        {
//            folderPath = folderPath.TrimEnd(ExchangeConstants.PathParser);
//            if (!folderPath.Contains(ExchangeConstants.PathParser))
//            {
//                return (string.Empty, folderPath);
//            }
//            string parentFolderPath = folderPath.Remove(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser));
//            var folderName = folderPath.Substring(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser) + 1);
//            return (parentFolderPath, folderName);
//        }

//        public override bool FindFolder(string folderPath)
//        {
//            if (existFolderPath.Keys.Contains(folderPath.TrimEnd(ExchangeConstants.PathParser)))
//            {
//                return true;
//            }
//            var parentInfo = GetParentPathAndFolderName(folderPath);
//            var parentFolderPath = parentInfo.ParentPath;
//            var folderName = parentInfo.FolderName;
//            if (existFolderPath.TryGetValue(parentFolderPath, out string parentFolderId))
//            {
//                try
//                {
//                    var folder = graphService.Mails.ExportImport.GetFolderByNameAsync(CurrentMailboxId, parentFolderId, folderName).ExecuteAsyncTask();
//                    if (folder?.Id != null)
//                    {
//                        existFolderPath.TryAdd(parentFolderPath + ExchangeConstants.PathParser + folder.DisplayName, folder.Id); // Cache the folder path
//                        tempMailboxId = folder.MailboxId();
//                        CurrentFolderId = folder.Id;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    if (ex is Microsoft.Graph.Beta.Models.ODataErrors.ODataError error && error.Error.Code.EqualsIgnoreCase("ErrorItemNotFound"))
//                    {
//                        logger.Warn(string.Format("Folder {0} not found in parent folder {1}.", folderName, parentFolderPath));
//                        return false;
//                    }
//                    logger.Warn(string.Format("Cannot bind to parent folder {0}.Message {1}.", parentFolderPath, ex.ToString()));
//                    return false;
//                }
//            }
//            else
//            {
//                return FindFolder(folderPath, string.Empty, false);
//            }
//            return CurrentFolderId != null;
//        }

//        public override FolderId GetCurrentFolderId()
//        {
//            return new FolderId(CurrentFolderId);
//        }

//        public override void FindAndCreateParentFolder(string folderPath, string targetFolderType)
//        {
//            if (existFolderPath.TryGetValue(folderPath.Trim(ExchangeConstants.PathParser), out var partnerFolderId))
//            {
//                CurrentFolderId = partnerFolderId;
//            }
//            else
//            {
//                FindFolder(folderPath, targetFolderType, true);
//            }
//        }

//        public bool FindFolder(string folderPath, string targetFolderType, bool createIfNotExist = false)
//        {
//            folderPath = folderPath.TrimEnd(ExchangeConstants.PathParser);
//            if (existFolderPath.TryGetValue(folderPath, out var folderId))
//            {
//                CurrentFolderId = folderId;
//                return true;
//            }

//            if (!folderPath.Contains(ExchangeConstants.PathParser))
//            {
//                CurrentFolderId = base.Mailbox.MsgFolderRoot.ToString();
//                return true;
//            }

//            var parentInfo = GetParentPathAndFolderName(folderPath);
//            var parentFolderPath = parentInfo.ParentPath;
//            var folderName = parentInfo.FolderName;
//            var parentFolderExist = FindFolder(parentFolderPath, targetFolderType, createIfNotExist);
//            if (!parentFolderExist)
//            {
//                return false;
//            }

//            var folderExist = false;
//            try
//            {
//                var folder = graphService.Mails.ExportImport.GetFolderByNameAsync(CurrentMailboxId, CurrentFolderId, folderName).ExecuteAsyncTask();
//                if (folder?.Id != null)
//                {
//                    existFolderPath.TryAdd(parentFolderPath + ExchangeConstants.PathParser + folder.DisplayName, folder.Id); // Cache the folder path
//                    tempMailboxId = folder.MailboxId();
//                    CurrentFolderId = folder.Id;
//                    return true;
//                }
//            }
//            catch (Exception ex)
//            {
//                if (ex is Microsoft.Graph.Beta.Models.ODataErrors.ODataError error && error.Error.Code.EqualsIgnoreCase("ErrorItemNotFound"))
//                {
//                    logger.Warn("Folder {0} not found in parent folder {1}.", folderName, parentFolderPath);
//                }
//                else
//                {
//                    logger.Warn("Cannot bind to parent folder {0}.Message {1}.", parentFolderPath, ex);
//                }
//                folderExist = false;
//            }

//            if (!folderExist && createIfNotExist)
//            {
//                CreateFolder(folderPath, folderName, "IPF.Note", null);
//                return true;
//            }

//            return folderExist;
//        }

//        public override void CreateFolder(string path, string name, string targetFolderType, string sourceId)
//        {
//            var folder = graphService.Mails.ExportImport.CreateFolderAsync(tempMailboxId ?? CurrentMailboxId, CurrentFolderId, name, targetFolderType).GetAwaiter().GetResult();
//            CurrentFolderId = folder.Id;
//            if (!existFolderPath.Keys.Contains(path.TrimEnd(ExchangeConstants.PathParser)))
//            {
//                existFolderPath.Add(path.TrimEnd(ExchangeConstants.PathParser), CurrentFolderId);
//            }
//        }

//        public GraphService GetGraphService()
//        {
//            return graphService;
//        }

//        public (Dictionary<string, string> ItemIdChangeKeyDic, Dictionary<string, string> OldNewIdDic) GetItemIdsInfo()
//        {
//            var items = graphService.Mails.ExportImport.ListItemsAsync(tempMailboxId ?? CurrentMailboxId, CurrentFolderId).ToListAsync().GetAwaiter().GetResult();
//            var idChangeKeyDic = items.ToDictionary(i => i.Id.Replace('-', '/').Replace('_', '+'), i => i.ChangeKey);
//            var oldNewIdDic = items
//                .Where(i => i.RestoreItemId().IsNotNullOrEmpty())
//                .ToDictionary(i => i.RestoreItemId(), i => i.Id.Replace('-', '/').Replace('_', '+'));
//            return (idChangeKeyDic, oldNewIdDic);
//        }

//        public bool InitMailboxId(string emailAddress)
//        {
//            var isArchive = ExchangeMailbox.IsArchiveMailboxAddress(emailAddress, out var mail);
//            var user = graphService.Users.GetUserByMailOrUpnAsync(mail).ExecuteAsyncTask();
//            var exchangeSetting = graphService.Mails.GetExchangeSettingsAsync(user.Id).ExecuteAsyncTask();
//            var mailboxId = isArchive ? exchangeSetting?.InPlaceArchiveMailboxId : exchangeSetting?.PrimaryMailboxId;
//            if (mailboxId.IsNullOrEmpty())
//            {
//                return false;
//            }
//            CurrentMailboxId = mailboxId;
//            mailItemUploader = new MailItemUploader(graphService.Mails, CurrentMailboxId);
//            return true;
//        }

//        public ImportItemResponse ImportItem(Stream steam)
//        {
//            var uploader = tempMailboxId.Equals(CurrentMailboxId) ? mailItemUploader : new MailItemUploader(graphService.Mails, tempMailboxId);
//            return uploader.ImportItemAsync(CurrentFolderId, steam).ExecuteAsyncTask();
//        }

//        public ImportItemResponse UpdateItem(string itemId, string changeKey, Stream steam)
//        {
//            var uploader = tempMailboxId.Equals(CurrentMailboxId) ? mailItemUploader : new MailItemUploader(graphService.Mails, tempMailboxId);
//            return uploader.UpdateItemAsync(CurrentFolderId, itemId, changeKey, steam).ExecuteAsyncTask();
//        }

//        public override bool NeedSkipSystemFolder(string folderPath)
//        {
//            string[] names = folderPath.Trim(ExchangeConstants.PathParser).Split(ExchangeConstants.PathParser);
//            try
//            {
//                for (int i = 1; i < names.Length; i++)
//                {
//                    string name = DecodeFolderName(names[i]);
//                    try
//                    {
//                        var folder = graphService.Mails.ExportImport.GetFolderByNameAsync(CurrentMailboxId, base.Mailbox.MsgFolderRoot.ToString(), name).ExecuteAsyncTask();
//                        if (folder != null && (folder.WellKnownFolderName().EqualsIgnoreCase("inbox") || folder.WellKnownFolderName().EqualsIgnoreCase("calendar")))
//                        {
//                            return false;
//                        }
//                    }
//                    catch (Microsoft.Graph.Beta.Models.ODataErrors.ODataError ex) when (ex.Error.Code.EqualsIgnoreCase("ErrorItemNotFound"))
//                    {
//                        continue;
//                    }
//                }
//                return true;
//            }
//            catch (Exception ex)
//            {
//                logger.Warn(string.Format("Find target folder [{0}] failed, exception: {1}", folderPath, ex.ToString()));
//                return false;
//            }
//        }
//    }
//}