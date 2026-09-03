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
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Globalization;
//    using System.IO;
//    using System.Linq;
//    using System.Threading;
//    using System.Threading.Tasks;

//    using ExchangeCommonWrapper;
//    using ExchangeUtility.Graph;
//    using Microsoft.Exchange.WebServices.Data;
//    using Microsoft365.Graph.Service;
//    using Newtonsoft.Json;
//    using Polly;

//    #endregion

//    public class ExchangeFolder : ExchangeObjectBase
//    {
//        #region Properties

//        internal ExchangeService Service { get; private set; }

//        internal FolderId ParentFolderId { get; private set; }

//        protected Folder currentFolder = null;
//        protected Folder rootFolder = null;
//        private string displayFolderPath = null;
//        private string internalFolderPath = null;
//        protected static Dictionary<string, string> existFolderPath = new Dictionary<string, string>();

//        public string FolderName { get; private set; }

//        public string FolderId { get; private set; }

//        //public string FolderType { get; private set; }
//        //public bool HasItems { get; private set; }
//        //public int ItemsCount { get; private set; }

//        public string DisplayFolderPath
//        {
//            get { return displayFolderPath; }
//            set { displayFolderPath = value; }
//        }

//        public string InternalFolderPath
//        {
//            get { return internalFolderPath; }
//            set { internalFolderPath = value; }
//        }

//        public ExchangeMailbox Mailbox { get; private set; }

//        //private string userName;
//        //private string password;

//        private const string ObjectNonexistError = "The specified object was not found in the store.";
//        #endregion

//        public ExchangeFolder(ExchangeMailbox mailbox, IEWSAuthObject authObj)
//            : base(authObj)
//        {
//            Service = CreateExchangeService();
//            SetServiceUrl(Service);
//            AddImpersonationHeader(mailbox);
//            SetImpersonateId(mailbox);
//            Mailbox = mailbox;
//        }

//        protected ExchangeFolder(ExchangeMailbox mailbox, Folder folder, IEWSAuthObject authObj)
//             : base(authObj)
//        {
//            Mailbox = mailbox;
//            Service = folder.Service;
//            currentFolder = folder;
//            GenerateFolderInfo(currentFolder);
//            ParentFolderId = FolderId;//TODO:qlluo
//        }

//        //Using for find item
//        public ExchangeFolder(ExchangeFolder folder, ExchangeMailbox mailbox)
//            : base(folder.AuthObject)
//        {
//            Service = folder.CreateExchangeService();
//            SetServiceUrl(folder);
//            AddImpersonationHeader(mailbox);
//            SetImpersonateId(mailbox);
//            Mailbox = mailbox;
//            currentFolder = folder.currentFolder;// do not bind new folder, improve performance
//            GenerateFolderInfo(currentFolder);
//            ParentFolderId = FolderId;//TODO:qlluo
//        }

//        protected ExchangeFolder(ExchangeMailbox mailbox, GraphService graphService) : base(graphService)
//        {
//            Mailbox = mailbox;
//        }

//        private void SetServiceUrl(ExchangeFolder folder)
//        {
//            try
//            {
//                this.Service.Url = folder.currentFolder.Service.Url;
//            }
//            catch (Exception)
//            {
//                logger.Warn("Current folder is null, or current folder service url is null. ");
//                base.SetServiceUrl(this.Service);
//            }
//        }

//        public ExchangeFolder GetRootFolder()
//        {
//            this.currentFolder = Folder.Bind(Service, this.Mailbox.RootFolderId).ExecuteAsyncTask();
//            GenerateFolderInfo(this.currentFolder);
//            this.displayFolderPath = this.Mailbox.OriginalMailboxAddress;
//            this.internalFolderPath = this.Mailbox.OriginalMailboxAddress;
//            return this;
//        }

//        public virtual FolderId GetCurrentFolderId()
//        {
//            return this.currentFolder.Id;
//        }

//        private void CreateNewService()
//        {
//            try
//            {
//                logger.Info("Create new service. User name : {0}", this.UserName);
//                this.Service = CreateExchangeService();
//                base.SetServiceUrl(this.Service);
//            }
//            catch (Exception e)
//            {
//                Service = null;
//                //serviceBinding = null;
//                logger.Error(string.Format("UserName: {0}, error: {1}", this.UserName, e.ToString()));
//                throw;
//            }
//        }

//        public virtual void SetImpersonateId(ExchangeMailbox mailbox)
//        {
//            var impersonateId = GlobalExchangeSetting.GetImpersonateIdByMailbox(mailbox.OriginalMailboxAddress);
//            {
//                AuthObject.SetImpersonatedUserId(Service, ExchangeMailbox.DecodeEmailAddress(impersonateId));
//            }
//        }

//        public virtual void AddImpersonationHeader(ExchangeMailbox mailbox)
//        {
//            AddImpersonationHeader(Service, mailbox.MailboxAddress);
//        }

//        private FolderPermissionCollectionM FixUserInfo(FolderPermissionCollectionM fpcM, out List<FolderPermissionM> failedToFixList)
//        {
//            var result = new FolderPermissionCollectionM() { Permissions = new List<FolderPermissionM>() };
//            failedToFixList = new List<FolderPermissionM>();
//            foreach (var fpM in fpcM)
//            {
//                if (FixUserInfo(fpM))
//                {
//                    result.Permissions.Add(fpM);
//                }
//                else//cannot find user
//                {
//                    failedToFixList.Add(fpM);
//                }
//            }
//            return result;
//        }

//        private bool FixUserInfo(FolderPermissionM fpM)
//        {
//            if (fpM.UserId.StandardUser != null) return true;

//            //https://stackoverflow.com/questions/6400253/find-primary-smtp-address-using-secondary-e-mail-address-with-ews
//            var email = FindFirstMatchedEmailAddress(fpM.UserId);
//            if (email != null)
//            {
//                var newUserId = new UserIdM() { PrimarySmtpAddress = email.Address };//DisplayName = email.Name,
//                logger.Info($"Fix user info: {fpM.UserId} --> {newUserId}");
//                fpM.UserId = newUserId;
//                return true;
//            }
//            //else //cannot find user by PrimarySmtpAddress and DisplayName, try use SID only
//            //{
//            //    fpM.UserId.DisplayName = null;
//            //    fpM.UserId.PrimarySmtpAddress = null;
//            //    return true;
//            //}
//            return false;
//        }

//        private EmailAddress FindFirstMatchedEmailAddress(UserIdM user)
//        {
//            var email = ResolveName(user.PrimarySmtpAddress, true);
//            if (email != null) return email;
//            return ResolveName(user.DisplayName, false);
//        }

//        private EmailAddress ResolveName(string nameToResolve, bool smtpAddress)
//        {
//            if (!string.IsNullOrEmpty(nameToResolve))
//            {
//                var name = smtpAddress ? $"smtp:{nameToResolve}" : nameToResolve;
//                return Service.ResolveName(name).ExecuteAsyncTask().Select(n => n.Mailbox).FirstOrDefault(m => m != null);
//            }
//            return null;
//        }

//        public void SetFolderPermission(FolderPermissionCollectionM fpcM, RestorePermissionOption option)
//        {
//            if (option == RestorePermissionOption.None) return;
//            List<FolderPermissionM> failedToFixList;
//            var permissionsM = FixUserInfo(fpcM, out failedToFixList);
//            InternalSetFolderPermission(option, permissionsM);
//            if (failedToFixList.Count > 0)
//            {
//                throw new UserNotFoundException(failedToFixList.Select(p => $"{p.UserId.DisplayName ?? string.Empty} <{p.UserId.PrimarySmtpAddress ?? string.Empty}>"));
//            }
//        }

//        //public void SetAllPublicFolderMetadata(string path, PublicFolderMetadata publicFolderInfo)
//        //{
//        //    int retryTime = 1;
//        //    do
//        //    {
//        //        try
//        //        {
//        //            //var folderId = ConvertHexEntryId();
//        //            using (ExchangeUser exchangeUser = ExchangeServiceFactory.CreateExchangeUser(this.AuthObject))
//        //            {
//        //                exchangeUser.SetPublicFolderStatus(path, publicFolderInfo);
//        //                exchangeUser.SetPublicFolderMetadata(path, publicFolderInfo);
//        //                if (publicFolderInfo.MailEnabled)
//        //                {
//        //                    exchangeUser.SetMailPublicFolderMetadata(path, publicFolderInfo);
//        //                    exchangeUser.SetPublicFolderPermissionMetadata(publicFolderInfo);
//        //                }
//        //            }
//        //            break;
//        //        }
//        //        catch (PSRemotingTransportException ex)
//        //        {
//        //            logger.Warn("Set all public folder metadata, try block throw a PSRemotingTransportException, retry time: {0}", retryTime);
//        //            if (retryTime > 5) throw ex;
//        //            Thread.Sleep(1000 * 60);
//        //        }
//        //        catch (RemoteException ex)
//        //        {
//        //            logger.Warn("Set all public folder metadata, try block throw a RemoteException, retry time: {0}", retryTime);
//        //            if (retryTime > 5) throw ex;
//        //            Thread.Sleep(1000 * 60);
//        //        }
//        //        retryTime++;
//        //    } while (true);
//        //}

//        protected virtual void InternalSetFolderPermission(RestorePermissionOption option, FolderPermissionCollectionM permissionsM)
//        {
//            //https://msdn.microsoft.com/en-us/library/office/dn641962(v=exchg.150).aspx#bk_folderperms
//            var tempFolder = Folder.Bind(this.Service, this.currentFolder.Id, new PropertySet(FolderSchema.Permissions)).ExecuteAsyncTask();
//            switch (option)
//            {
//                case RestorePermissionOption.Merge:
//                    tempFolder.Permissions.Merge(permissionsM.ToEx());
//                    break;

//                case RestorePermissionOption.Replace:
//                    tempFolder.Permissions.Replace(permissionsM.ToEx());
//                    break;

//                default:
//                    throw new InvalidOperationException("Unreachable code.");
//            }

//            tempFolder.Update().ExecuteAsyncTask();
//        }

//        public void SetFolderACL(List<ACLEntry> folderAcl)
//        {
//            try
//            {
//                PropertySet propertySet = new PropertySet(BasePropertySet.FirstClassProperties);
//                propertySet.Add(FolderSchema.Permissions);
//                Folder folder = Folder.Bind(Service, FolderId, propertySet).ExecuteAsyncTask();

//                foreach (ACLEntry entry in folderAcl)
//                {
//                    FolderPermission permission = new FolderPermission();
//                    permission.UserId = new UserId(entry.UserId);
//                    if (entry.Permissions.Contains("CreateItems"))
//                    {
//                        permission.CanCreateItems = true;
//                    }
//                    if (entry.Permissions.Contains("CreateSubfolders"))
//                    {
//                        permission.CanCreateSubFolders = true;
//                    }
//                    if (entry.Permissions.Contains("ReadItems"))
//                    {
//                        permission.ReadItems = FolderPermissionReadAccess.FullDetails;
//                    }
//                    if (entry.Permissions.Contains("DeleteAllItems"))
//                    {
//                        permission.DeleteItems = PermissionScope.All;
//                    }
//                    if (entry.Permissions.Contains("DeleteOwnItems"))
//                    {
//                        permission.DeleteItems = PermissionScope.Owned;
//                    }
//                    if (entry.Permissions.Contains("FolderContact"))
//                    {
//                        permission.IsFolderContact = true;
//                    }
//                    if (entry.Permissions.Contains("FolderVisible"))
//                    {
//                        permission.IsFolderVisible = true;
//                    }
//                    if (entry.Permissions.Contains("FolderOwner"))
//                    {
//                        permission.IsFolderOwner = true;
//                    }
//                    if (entry.Permissions.Contains("EditAllItems"))
//                    {
//                        permission.EditItems = PermissionScope.All;
//                    }
//                    if (entry.Permissions.Contains("EditOwnItems"))
//                    {
//                        permission.EditItems = PermissionScope.Owned;
//                    }
//                    folder.Permissions.Add(permission);
//                }
//                folder.Update();
//            }
//            catch (Exception e)
//            {
//                logger.Warn(string.Format("Add folder permission, reason: {0}", e.ToString()));
//            }
//        }

//        public UserConfigurationCollectionM GetUserConfigurations()
//        {
//            var config = UserConfiguration.Bind(Service, "OWA.ViewStateConfiguration", WellKnownFolderName.Root, UserConfigurationProperties.Dictionary).ExecuteAsyncTask();
//            IEnumerable folderView = null;
//            UserConfigurationM result;
//            UserConfigurationCollectionM resultDictionary = new UserConfigurationCollectionM() { UserConfigurations = new Dictionary<string, UserConfigurationM> { } };
//            if (config.Dictionary.ContainsKey("FolderViewState"))
//            {
//                folderView = config.Dictionary["FolderViewState"] as IEnumerable;
//            }
//            if (folderView != null)
//            {
//                foreach (var item in folderView)
//                {
//                    result = JsonConvert.DeserializeObject<UserConfigurationM>(item.ToString());
//                    resultDictionary.UserConfigurations[result.FolderId.Id] = result;
//                }
//            }
//            return resultDictionary;
//        }

//        /// <summary>
//        /// Bind folder for user configuration
//        /// </summary>
//        public UserConfiguration BindFolder()
//        {
//            return UserConfiguration.Bind(Service, "OWA.ViewStateConfiguration", WellKnownFolderName.Root, UserConfigurationProperties.Dictionary).ExecuteAsyncTask();
//        }

//        /// <summary>
//        /// Create new folder
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="targetFolderType"></param>
//        /// <param name="parentFolderId"></param>
//        public virtual void CreateFolder(string path, string name, string targetFolderType, string sourceId)
//        {
//            Folder folder = ConfirmFolderType(targetFolderType);
//            folder.DisplayName = name;
//            folder.Save(ParentFolderId).ExecuteAsyncTask();
//            currentFolder = folder;
//            folder = Folder.Bind(Service, folder.Id).ExecuteAsyncTask();

//            if (sourceId.IsNotNullOrEmpty())
//            {
//                var defId = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF556, MapiPropertyType.String);
//                folder.SetExtendedProperty(defId, sourceId);
//            }

//            if (targetFolderType.Equals("IPF.StickyNote", StringComparison.OrdinalIgnoreCase)
//                || targetFolderType.Equals("IPF.Journal", StringComparison.OrdinalIgnoreCase)
//                || targetFolderType.Equals("IPF.Contact", StringComparison.OrdinalIgnoreCase))
//            {
//                var defType = new ExtendedPropertyDefinition(0x3613, MapiPropertyType.String);
//                folder.SetExtendedProperty(defType, targetFolderType);
//            }
//            folder.Update();

//            if (!existFolderPath.Keys.Contains(path.TrimEnd(ExchangeConstants.PathParser)))
//            {
//                existFolderPath.Add(path.TrimEnd(ExchangeConstants.PathParser), currentFolder.Id.ToString());
//            }
//        }

//        /// <summary>
//        /// deal with the particular folder type:calendar and task
//        /// </summary>
//        /// <param name="targetFolderType"></param>
//        /// <returns>folder instance</returns>
//        protected Folder ConfirmFolderType(string targetFolderType)
//        {
//            switch (targetFolderType.ToLower())
//            {
//                case "ipf.task":
//                    return new TasksFolder(Service);

//                case "ipf.appointment":
//                    return new CalendarFolder(Service);

//                case "ipf.contact":
//                    return new ContactsFolder(Service);
//                //AOSBR-6386兼容PF老数据，还原成默认类型
//                case "mailbox":
//                    return new Folder(Service) { FolderClass = "IPF.Note" };

//                default:
//                    return new Folder(Service) { FolderClass = targetFolderType };
//            }
//        }

//        /// <summary>
//        /// find folder in outlook
//        /// </summary>
//        /// <param name="folderPath"></param>
//        /// <param name="folderId">need to compare folderId after get it in the specific path</param>
//        /// <returns>true means find the folder successfully</returns>
//        public virtual Boolean FindFolder(string folderPath)
//        {
//            if (existFolderPath.Keys.Contains(folderPath.TrimEnd(ExchangeConstants.PathParser)))
//            {
//                return true;
//            }
//            string parentFolderPath = folderPath.Remove(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser));
//            if (existFolderPath.Keys.Contains(parentFolderPath))
//            {
//                try
//                {
//                    if (currentFolder != null)
//                    {
//                        FindFoldersResults allFolders = currentFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderPath.Substring(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser) + 1)), new FolderView(100)/* {  PropertySet = DEFAULT_FOLDER_PROPERTY_SET }*/).ExecuteAsyncTask();//add this in feature to get more properties
//                        if (allFolders.TotalCount > 0)
//                        {
//                            if (!existFolderPath.Keys.Contains(folderPath.TrimEnd(ExchangeConstants.PathParser)))
//                            {
//                                existFolderPath.Add(folderPath.TrimEnd(ExchangeConstants.PathParser), allFolders.Folders[0].Id.ToString());
//                            }
//                            currentFolder = allFolders.Folders[0];
//                            return true;
//                        }
//                    }
//                    Folder folder = Folder.Bind(Service, new FolderId(existFolderPath[parentFolderPath])).ExecuteAsyncTask();
//                    FindFoldersResults allFolder = folder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderPath.Substring(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser) + 1)), new FolderView(100)/* { PropertySet = DEFAULT_FOLDER_PROPERTY_SET }*/).ExecuteAsyncTask();//add this in feature to get more properties
//                    if (allFolder.TotalCount > 0)
//                    {
//                        if (!existFolderPath.Keys.Contains(folderPath.TrimEnd(ExchangeConstants.PathParser)))
//                        {
//                            existFolderPath.Add(folderPath.TrimEnd(ExchangeConstants.PathParser), allFolder.Folders[0].Id.ToString());
//                        }
//                        currentFolder = allFolder.Folders[0];
//                        return true;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn(string.Format("Cannot bind to parent folder {0}.Message {1}.", parentFolderPath, ex.ToString()));
//                }
//            }
//            else
//            {
//                var folder = FindFolder(folderPath, null, false);
//                currentFolder = folder;
//                return folder != null;
//            }
//            return false;
//        }

//        ///// <summary>
//        ///// find item in outlook
//        ///// </summary>
//        ///// <param name="itemPath"></param>
//        ///// <param name="itemId"></param>
//        ///// <param name="needDelete">true means delete the item after founded in outlook because 'Overwrite' was selected at content level conflict resolution</param>
//        ///// <returns>true means find the folder successfully</returns>
//        //public Boolean FindItem(string itemPath, string itemId, Boolean needDelete)
//        //{
//        //    Folder rootFolder = null;
//        //    if (existFolderPath.Keys.Contains(itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser))))
//        //    {
//        //        if (currentFolder != null)
//        //        {
//        //            string tempString = itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser));
//        //            string folderName = tempString.Substring(tempString.LastIndexOf(ExchangeConstants.PathParser) + 1);
//        //            if (String.Equals(folderName, currentFolder.DisplayName, StringComparison.OrdinalIgnoreCase))
//        //            {
//        //                rootFolder = currentFolder;
//        //            }
//        //        }
//        //        else
//        //        {
//        //            rootFolder = Folder.Bind(service, new FolderId(existFolderPath[itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser))]));
//        //        }
//        //        FindItemsResults<Item> resultSecond = rootFolder.FindItems(new SearchFilter.IsEqualTo(ItemSchema.Id, itemId), new ItemView(100));
//        //        if (resultSecond.TotalCount > 0)
//        //        {
//        //            if (needDelete)
//        //            {
//        //                logger.Warn("The Item {0} was deleted,because content level conflict resolution is overwrite.", resultSecond.Items[0].Subject);
//        //                resultSecond.Items[0].Delete(DeleteMode.HardDelete);
//        //            }
//        //            return true;
//        //        }
//        //        //The Guid and id parameter is specified while backup item,so if you change ,please change these parameters in backup project too.
//        //        ExtendedPropertyDefinition def = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);
//        //        PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, def);
//        //        FindItemsResults<Item> searchResult = rootFolder.FindItems(new SearchFilter.IsEqualTo(def, itemId), new ItemView(100));
//        //        if (searchResult.TotalCount > 0)
//        //        {
//        //            foreach (Item item in searchResult)
//        //            {
//        //                if (needDelete)
//        //                {
//        //                    logger.Warn("The Item {0} was deleted,because content level conflict resolution is overwrite.",item.Subject);
//        //                    item.Delete(DeleteMode.HardDelete);
//        //                }
//        //            }
//        //            return true;
//        //        }
//        //    }
//        //    return false;
//        //}

//        public virtual Boolean NeedSkipSystemFolder(string folderPath)
//        {
//            string[] names = folderPath.Trim(ExchangeConstants.PathParser).Split(ExchangeConstants.PathParser);
//            try
//            {
//                if (rootFolder == null)
//                    rootFolder = RetryBind(this.Mailbox.RootFolderId);
//                for (int i = 1; i < names.Length; i++)
//                {
//                    string name = DecodeFolderName(names[i]);
//                    FindFoldersResults allFolders = rootFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, name), new FolderView(100, 0) { Traversal = FolderTraversal.Shallow, PropertySet = DEFAULT_FOLDER_PROPERTY_SET }).ExecuteAsyncTask();
//                    if (allFolders.TotalCount > 0 && (allFolders.Folders[0].WellKnownFolderName == WellKnownFolderName.Inbox || allFolders.Folders[0].WellKnownFolderName == WellKnownFolderName.Calendar))
//                    {
//                        return false;
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

//        protected Boolean FindItemById(Folder folder, string itemId, Boolean needDelete)
//        {
//            FindItemsResults<Item> result = null;
//            string exchangeId = ExchangeConstants.ConvertItemId(itemId);
//            int retryTimes = 0;
//            while (retryTimes < 5)
//            {
//                try
//                {
//                    result = folder.FindItems(new SearchFilter.IsEqualTo(ItemSchema.Id, itemId), new ItemView(10), this.Service);
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("Find item {1} with exception : {0}. Try times: {2}.", ex.ToString(), exchangeId, retryTimes);
//                    if (ex.Message.Contains(ObjectNonexistError))
//                    {
//                        break;
//                    }
//                    retryTimes++;
//                    Thread.Sleep(1000);
//                }
//            }
//            if (result != null && result.TotalCount > 0)
//            {
//                if (needDelete)
//                {
//                    DeleteItem(result.Items[0], exchangeId);
//                }
//                return true;
//            }
//            return false;
//        }

//        private static void DeleteItem(Item item, string exchangeId)
//        {
//            logger.Warn("The Item {0} was deleted,because content level conflict resolution is overwrite.", item.Subject);
//            try
//            {
//                if (item is Appointment)
//                {
//                    var currentAppointment = item as Appointment;
//                    currentAppointment.Delete(DeleteMode.HardDelete, SendCancellationsMode.SendToNone);
//                }
//                else
//                {
//                    item.Delete(DeleteMode.HardDelete);
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Delete item {1} with exception : {0}", ex.ToString(), exchangeId);
//                item.Delete(DeleteMode.MoveToDeletedItems);
//            }
//        }

//        protected Boolean FindItemByProperty(Folder folder, string itemId, Boolean needDelete)
//        {
//            //The Guid and id parameter is specified while backup item,so if you change ,please change these parameters in backup project too.
//            ExtendedPropertyDefinition def = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);
//            PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, def);
//            FindItemsResults<Item> result = null;
//            Boolean findResult = false;
//            string exchangeId = ExchangeConstants.ConvertItemId(itemId);
//            int retryTimes = 0;
//            while (retryTimes < 5)
//            {
//                try
//                {
//                    result = folder.FindItems(new SearchFilter.ContainsSubstring(def, itemId, ContainmentMode.FullString, ComparisonMode.Exact), new ItemView(10), this.Service);
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("Find item {1} with property with exception : {0}. Try times: {2}.", ex.ToString(), exchangeId, retryTimes);
//                    if (ex.Message.Contains(ObjectNonexistError))
//                    {
//                        break;
//                    }
//                    retryTimes++;
//                    Thread.Sleep(1000);
//                }
//            }
//            if (result != null && result.TotalCount > 0)
//            {
//                foreach (Item item in result)
//                {
//                    string tempPropertyValue = GetPropertyValue(item, def);
//                    if (itemId.Equals(tempPropertyValue))
//                    {
//                        if (needDelete)
//                        {
//                            DeleteItem(item, exchangeId);
//                        }
//                        findResult = true;
//                    }
//                }
//            }
//            return findResult;
//        }

//        /// <summary>
//        /// find item in outlook
//        /// </summary>
//        /// <param name="itemPath"></param>
//        /// <param name="itemId"></param>
//        /// <param name="created"></param>
//        /// <param name="needDelete">true means delete the item after founded in outlook because 'Overwrite' was selected at content level conflict resolution</param>
//        /// <returns>true means find the folder successfully</returns>
//        public Boolean FindItem(string itemPath, string itemId, Boolean needDelete)
//        {
//            Folder itemParentFolder = null;
//            Boolean findResult = false;
//            if (existFolderPath.Keys.Contains(itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser))))
//            {
//                if (currentFolder != null)
//                {
//                    string tempString = itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser));
//                    string folderName = tempString.Substring(tempString.LastIndexOf(ExchangeConstants.PathParser) + 1);
//                    if (String.Equals(folderName, currentFolder.DisplayName, StringComparison.OrdinalIgnoreCase))
//                    {
//                        itemParentFolder = currentFolder;
//                    }
//                }
//                else
//                {
//                    itemParentFolder = RetryBind(new FolderId(existFolderPath[itemPath.Remove(itemPath.LastIndexOf(ExchangeConstants.PathParser))]));
//                }
//                findResult = FindItemById(itemParentFolder, itemId, needDelete);
//                if (findResult)
//                {
//                    return true;
//                }

//                findResult = FindItemByProperty(itemParentFolder, itemId, needDelete);
//            }
//            return findResult;
//        }

//        private Folder RetryBind(FolderId folderId)
//        {
//            Folder parentfolder = null;
//            int retryTimes = 0;
//            while (retryTimes < 5)
//            {
//                try
//                {
//                    parentfolder = Folder.Bind(Service, folderId).ExecuteAsyncTask();
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("An error occurred while bind folder. Try times: {0}, Message : {1}.", retryTimes, ex.ToString());
//                    Thread.Sleep(1000);
//                }
//                retryTimes++;
//            }
//            return parentfolder;
//        }

//        /// <summary>
//        /// find or create folder
//        /// </summary>
//        /// <param name="folderPath"></param>
//        /// <param name="targetFolderType">always empty because we can't get the type of parent folder type in sub node</param>
//        public virtual void FindAndCreateParentFolder(string folderPath, string targetFolderType)
//        {
//            Folder rootFolder = null;
//            if (existFolderPath.Keys.Contains(folderPath.Trim(ExchangeConstants.PathParser)))
//            {
//                if (string.Equals(currentFolder.DisplayName, folderPath.TrimEnd(ExchangeConstants.PathParser).Substring(folderPath.TrimEnd(ExchangeConstants.PathParser).LastIndexOf(ExchangeConstants.PathParser) + 1), StringComparison.OrdinalIgnoreCase))
//                {
//                    ParentFolderId = currentFolder.Id.ToString();
//                    return;
//                }
//                else
//                {
//                    rootFolder = RetryBind(new FolderId(existFolderPath[folderPath.Trim(ExchangeConstants.PathParser)]));// Folder.Bind(service, new FolderId(existFolderPath[folderPath.Trim(ExchangeConstants.PathParser)]));
//                    currentFolder = rootFolder;
//                    ParentFolderId = currentFolder.Id.ToString();
//                    return;
//                }
//            }
//            rootFolder = FindFolder(folderPath, targetFolderType, true);
//            currentFolder = rootFolder;
//            ParentFolderId = currentFolder.Id.ToString();
//        }

//        private Folder FindFolder(string folderPath, string targetFolderType, bool createIfNotFound)
//        {
//            //need to create the folder path while parentfolderpath doesn't exist in existfolderpath
//            string[] names = folderPath.Trim(ExchangeConstants.PathParser).Split(ExchangeConstants.PathParser);
//            try
//            {
//                var rootFolder = RetryBind(this.Mailbox.RootFolderId);// Folder.Bind(service, id);
//                for (int i = 1; i < names.Length; i++)
//                {
//                    string name = DecodeFolderName(names[i]);
//                    bool found = false;
//                    FolderView folderView = new FolderView(100);
//                    folderView.Traversal = FolderTraversal.Shallow;
//                    FindFoldersResults allFolders = rootFolder.FindFolders(new SearchFilter.IsEqualTo(FolderSchema.DisplayName, name), folderView).ExecuteAsyncTask();
//                    if (allFolders.TotalCount > 0)
//                    {
//                        rootFolder = allFolders.Folders[0];
//                        found = true;
//                    }
//                    if (!found)
//                    {
//                        if (createIfNotFound)
//                        {
//                            Folder newFolder = new Folder(Service);
//                            newFolder.DisplayName = name;
//                            if (string.IsNullOrEmpty(targetFolderType))
//                            {
//                                newFolder.FolderClass = targetFolderType;
//                            }
//                            newFolder.Save(rootFolder.Id).ExecuteAsyncTask();
//                            rootFolder = newFolder;
//                        }
//                        else
//                        {
//                            return null;
//                        }
//                    }
//                }
//                if (!existFolderPath.Keys.Contains(folderPath.TrimEnd(ExchangeConstants.PathParser)))
//                {
//                    existFolderPath.Add(folderPath.TrimEnd(ExchangeConstants.PathParser), rootFolder.Id.ToString());
//                }
//                return rootFolder;
//            }
//            catch (Exception e)
//            {
//                logger.Warn(string.Format("Find target folder, reason: {0}", e.ToString()));
//                return null;
//            }
//        }

//        protected string DecodeFolderName(string name)
//        {
//            string result = string.Empty;
//            result = name.Replace("%5C", "\\");
//            return result;
//        }

//        #region SetHomePage

//        public void SetFolderHomePage(string url)
//        {
//            try
//            {
//                logger.Debug(string.Format("Folder homepage: {0}", url));
//                ExtendedPropertyDefinition homePage = new ExtendedPropertyDefinition(14047, MapiPropertyType.Binary);
//                currentFolder.SetExtendedProperty(homePage, EncodeUrl(url));
//                currentFolder.Update();
//            }
//            catch (Exception e)
//            {
//                logger.Warn(string.Format("Update folder homepage, reason: {0}", e.ToString()));
//            }
//        }

//        private byte[] EncodeUrl(string url)
//        {
//            var writer = new StringWriter();
//            var dataSize = ((ConvertToHex(url).Length / 2) + 2).ToString("X2");

//            writer.Write("02"); // Version
//            writer.Write("00000001"); // Type
//            writer.Write("00000001"); // Flags
//            writer.Write("00000000000000000000000000000000000000000000000000000000"); // unused
//            writer.Write("000000");
//            writer.Write(dataSize);
//            writer.Write("000000");
//            writer.Write(ConvertToHex(url));
//            writer.Write("0000");

//            var buffer = HexStringToByteArray(writer.ToString());
//            return buffer;
//        }

//        private string ConvertToHex(string input)
//        {
//            return string.Join(string.Empty, input.Select(c => ((int)c).ToString("x2") + "00").ToArray());
//        }

//        private byte[] HexStringToByteArray(string input)
//        {
//            return Enumerable
//                .Range(0, input.Length / 2)
//                .Select(index => byte.Parse(input.Substring(index * 2, 2), NumberStyles.AllowHexSpecifier)).ToArray();
//        }

//        private string ConvertHexEntryId()
//        {
//            var ewsId = new AlternatePublicFolderId(IdFormat.EwsId, this.FolderId);
//            var hexId = Service.ConvertId(ewsId, IdFormat.HexEntryId).ExecuteAsyncTask() as AlternatePublicFolderId;
//            string idForPowerShell = hexId.FolderId;
//            return idForPowerShell;
//        }

//        #endregion

//        public void SetFolderDescription(string decription)
//        {
//            try
//            {
//                logger.Debug(string.Format("Folder description: {0}", decription));
//                ExtendedPropertyDefinition folderDescription = new ExtendedPropertyDefinition(0x3004, MapiPropertyType.String);
//                currentFolder.SetExtendedProperty(folderDescription, decription);
//                currentFolder.Update();
//            }
//            catch (Exception e)
//            {
//                logger.Warn(string.Format("Update folder description, reason: {0}", e.ToString()));
//            }
//        }

//        private void GenerateFolderInfo(Folder folder)
//        {
//            try
//            {
//                this.FolderName = folder.DisplayName;
//                this.FolderId = folder.Id.ToString();
//                //remove these properties since it is never used.
//                //this.FolderType = folder.FolderClass;
//                //if (string.IsNullOrEmpty(this.FolderType))
//                //    this.FolderType = "IPF.Note";
//                //this.HasItems = folder.TotalCount > 0;
//                //this.ItemsCount = folder.TotalCount;
//            }
//            catch (Exception e)
//            {
//                logger.Warn(string.Format("Generate folder info, reason: {0}", e.ToString()));
//            }
//        }

//        private static bool ValidateRedirectionUrlCallback(String RedirectionUrl)
//        {
//            return true;
//        }

//        private string GetPropertyValue(Item item, ExtendedPropertyDefinition def)
//        {
//            int retryTimes = 0;
//            while (retryTimes < 5)
//            {
//                try
//                {
//                    PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, def);
//                    if (Service == null)
//                        CreateNewService();
//                    Item tempItem = Item.Bind(Service, item.Id, set).ExecuteAsyncTask();
//                    object tempValue = new object();
//                    tempItem.TryGetProperty(def, out tempValue);
//                    return tempValue == null ? string.Empty : tempValue.ToString();
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("Get property value with exception: {0} Try times: {1}", ex.ToString(), retryTimes);
//                    retryTimes++;
//                }
//            }
//            return string.Empty;
//        }
//    }

//    public class ExchangePublicFolder : ExchangeFolder
//    {
//        private readonly FolderId pfFolderId;
//        private bool isOpened = false;

//        public string PublicFolderPath { get; private set; }

//        public ExchangePublicFolder(string folderPath, string folderId, IEWSAuthObject authObj)
//            : base(new ExchangeMailbox(folderId, ExchangeMailboxType.PublicFolder), authObj)
//        {
//            pfFolderId = string.IsNullOrEmpty(folderId) ? WellKnownFolderName.PublicFoldersRoot : folderId;
//            PublicFolderPath = folderPath;
//        }

//        public ExchangePublicFolder(string folderPath, Folder folder, IEWSAuthObject authObj)
//            : base(new ExchangeMailbox(folder.Id.UniqueId, ExchangeMailboxType.PublicFolder), folder, authObj)
//        {
//            pfFolderId = folder.Id.UniqueId;
//            PublicFolderPath = folderPath;
//        }

//        private ExchangePublicFolder(ExchangePublicFolder folder) :
//            base(folder, folder.Mailbox) =>
//            PublicFolderPath = folder.PublicFolderPath;

//        protected override void InternalSetFolderPermission(RestorePermissionOption option, FolderPermissionCollectionM permissionsM)
//        {
//            try
//            {
//                base.InternalSetFolderPermission(option, permissionsM);
//            }
//            catch (ServiceResponseException srEx)
//            {
//                if (srEx.ErrorCode == ServiceError.ErrorAccessDenied)
//                {
//                    AddOwnerPermission(this.Service?.ImpersonatedUserId?.Id ?? this.UserName);
//                    base.InternalSetFolderPermission(option, permissionsM);
//                    return;
//                }
//                throw;
//            }
//        }

//        public override void SetImpersonateId(ExchangeMailbox mailbox)
//        {
//            var impersonateUser = AuthObject.ImpersonateUser ?? AuthObject.UserName;
//            logger.Info("Impersonate Id: {0}.", impersonateUser);
//            SetImpersonateId(Service, impersonateUser);
//        }

//        public override void AddImpersonationHeader(ExchangeMailbox mailbox)
//        {
//        }

//        public ExchangePublicFolder CreateFolder(string name, string folderType)
//        {
//            try
//            {
//                return CreateFolderInternal(name, folderType);
//            }
//            catch (ServiceResponseException srEx)
//            {
//                if (srEx.ErrorCode == ServiceError.ErrorAccessDenied)
//                {
//                    AddOwnerPermission(this.Service?.ImpersonatedUserId?.Id ?? this.UserName);
//                    return CreateFolderInternal(name, folderType);
//                }
//                throw;
//            }
//        }

//        private ExchangePublicFolder CreateFolderInternal(string name, string folderType)
//        {
//            var folder = ConfirmFolderType(folderType);
//            folder.DisplayName = name;
//            folder.Save(this.pfFolderId).ExecuteAsyncTask();
//            return new ExchangePublicFolder($"{this.DisplayFolderPath}\\{name}", folder, this.AuthObject);
//        }

//        private void AddOwnerPermission(string userName)
//        {
//            logger.Info($"Add owner permission for [{userName}] on folder [{this.PublicFolderPath}].");
//            try
//            {
//                var service = ExchangeServiceFactory.CreateOutlookService(this.AuthObject);
//                Policy.Handle<Exception>(e =>
//                {
//                    if (e.InnerException?.Message?.Contains("MailboxInfoStaleException", StringComparison.OrdinalIgnoreCase) ?? false)
//                    {
//                        service.SetPFPrimarySmtpAddressAsync().ExecuteAsyncTask();
//                        return true;
//                    }
//                    return false;
//                }).RetryAsync(1)
//                .ExecuteAsync(delegate
//                    {
//                        return service.AddPublicFolderClientPermissionAsync(this.PublicFolderPath, userName, FolderPermissionLevel.Owner);
//                    }).ExecuteAsyncTask();
//            }
//            catch (Exception e)
//            {
//                if (e.InnerException?.Message?.Contains("UserAlreadyExistsInPermissionEntryException", StringComparison.OrdinalIgnoreCase) ?? false) return;
//                logger.Error($"Failed to add owner permission. Details: {e}");
//                throw new NotSupportedException($"This operation needs owner permission, please add owner permission for user [{userName}] on folder [{this.PublicFolderPath}] and try again.");
//            }
//        }

//        public ExchangePublicFolder Clone()
//        {
//            return new ExchangePublicFolder(this);
//        }

//        public ExchangePublicFolder FindFolderByName(string displayName)
//        {
//            var children = Service.FindFolders(pfFolderId, new SearchFilter.IsEqualTo(FolderSchema.DisplayName, displayName), new FolderView(1)).ExecuteAsyncTask();
//            if (children.TotalCount <= 0) return null;
//            return new ExchangePublicFolder($"{DisplayFolderPath}\\{displayName}", children.Folders[0], AuthObject);
//        }

//        public bool Exist
//        {
//            get
//            {
//                try
//                {
//                    Open();
//                    return true;
//                }
//                catch
//                {
//                    return false;
//                }
//            }
//        }

//        public void Open()
//        {
//            if (isOpened == true)
//            {
//                return;
//            }
//            currentFolder = Folder.Bind(Service, pfFolderId).ExecuteAsyncTask();
//            isOpened = true;
//        }

//        public bool FindItemInternal(string itemId, bool needDelete)
//        {
//            if (FindItemById(this.currentFolder, itemId, needDelete))
//            {
//                return true;
//            }
//            return FindItemByProperty(this.currentFolder, itemId, needDelete);
//        }

//        public FindItemState FindItem(string itemId, bool needDelete)
//        {
//            var found = FindItemInternal(itemId, needDelete);
//            if (found) return needDelete ? FindItemState.FoundAndDelete : FindItemState.FoundAndSkip;
//            return FindItemState.NotFound;
//        }

//        public FolderId GetPFFolderId()
//        {
//            return this.pfFolderId;
//        }
//    }

//    public enum FindItemState
//    {
//        NotFound = 0,
//        FoundAndSkip = 1,
//        FoundAndDelete = 2,
//    }

//    public struct ACLEntry
//    {
//        public string ObjectSid;
//        public string UserId;		//user mail address
//        public string DisplayName;			// display name
//        public List<string> Permissions;	// string list of permissions
//    }

//    public enum RestorePermissionOption
//    {
//        None = 0,
//        Merge = 1,
//        Replace = 2,
//    }
//}