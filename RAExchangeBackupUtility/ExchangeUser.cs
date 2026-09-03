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
    using ExchangeUtility.MicrosoftGraph;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    //using Microsoft.Azure.ActiveDirectory.GraphClient;
    public static class ExchangeUserFacotry
    {
        public static ExchangeUser CreateExchangeUser(AuthObject authObj)
        {
            return new ExchangeUserWithGraph(authObj);
        }
    }

    public abstract class ExchangeUser : ExchangeObjectBase, IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public ExchangeUser(AuthObject authObj)
            : base(authObj)
        {
        }

        /// <summary>
        /// Get all user mailboxs in current tenant
        /// </summary>
        /// <param name="loadResourceMailbox">whether include resource mailbox</param>
        /// <param name="loadArchiveMailbox">whether include in-place archive mailbox</param>
        /// <returns></returns>
        public abstract List<string> GetUsers(bool loadResourceMailbox, bool loadArchiveMailbox);

        public abstract string GetO365GroupOwner(string o365GroupMailBox);
        public abstract string GetO365GroupMember(string o365GroupMailBox);

        public abstract bool IsO365GroupPrivate(string o365GroupMailBox);

        public virtual string GetO365GroupOwnerOrMember(string o365GroupMailBox)
        {
            var owner = GetO365GroupOwner(o365GroupMailBox);
            if (!string.IsNullOrEmpty(owner)) return owner;
            var member = GetO365GroupMember(o365GroupMailBox);
            if (!string.IsNullOrEmpty(member)) return member;
            if (IsO365GroupPrivate(o365GroupMailBox))
                throw new AccessdeniedException(ExchangeConstants.ERRORMESSAGE_GROUP_NONEUSER);
            return this.UserName;//treat service account as group owner
        }

        public abstract Dictionary<string, object> GetO365GroupMailboxAndSiteUrl();

        //public abstract bool AddPermission(string targetUserName);

        //public abstract bool CheckPermission(string targetUserMailBox, ref string url);

        public abstract bool SetApplicationImpersonationRole();

        public abstract string ServiceUrl { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }

//    public class ExchangeUserWithPS : ExchangeUser
//    {
//        //private ExchangeService service = null;
//        private Runspace runspace = null;

//        public ExchangeUserWithPS(CredentialAuthObject authObj)
//            : base(authObj)
//        {
//        }

//        public override string ServiceUrl
//        {
//            get;
//            set;
//        }

//        public override List<string> GetUsers(bool loadResourceMailbox, bool loadArchiveMailbox)
//        {
//            try
//            {
//                logger.Info("Service Account : {0}", this.UserName);
//                logger.Info("IncludeResourceMailbox: {0}. IncludeArchiveMailxbox: {1}", loadResourceMailbox.ToString(), loadArchiveMailbox.ToString());
//                if (Initialize())
//                {
//                    try
//                    {
//                        if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                        {
//                            runspace.Open();
//                        }
//                        return GetUserMailbox(loadResourceMailbox, loadArchiveMailbox);
//                    }
//                    catch (Exception ex)
//                    {
//                        var errorMessage = CheckPermission();
//                        if (!string.IsNullOrEmpty(errorMessage))
//                        {
//                            logger.Warn("Cannot connect to exchange server throughout cmdlet. Reason:{0}", ex);
//                            throw new Exception(errorMessage);
//                        }
//                        throw;
//                    }
//                }
//            }
//            finally
//            {
//                this.Dispose(true);
//                //if (runspace != null)
//                //{
//                //    runspace.Close();
//                //}
//            }
//            //Unreachable code
//            return null;
//        }

//        public override string GetO365GroupOwner(string o365GroupMailBox)
//        {
//            return GetO365GroupLinks(o365GroupMailBox, "Owners");
//        }

//        public override string GetO365GroupMember(string o365GroupMailBox)
//        {
//            return GetO365GroupLinks(o365GroupMailBox, "Members");
//        }

//        public override bool IsO365GroupPrivate(string o365GroupMailBox)
//        {
//            try
//            {
//                logger.Info("Get group[{0}] {1}, admin: {2}", o365GroupMailBox, this.UserName);
//                var o365GroupsInfo = new List<string>();
//                if (Initialize())
//                {
//                    if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                    {
//                        runspace.Open();
//                    }
//                    using (Pipeline pipeLine = runspace.CreatePipeline())
//                    {
//                        Command getMemberName = new Command("Get-UnifiedGroup");
//                        getMemberName.Parameters.Add("Identity", o365GroupMailBox);
//                        pipeLine.Commands.Add(getMemberName);
//                        Command commandSelect = new Command("Select-Object");
//                        commandSelect.Parameters.Add("Property", new string[] { "AccessType" });
//                        pipeLine.Commands.Add(commandSelect);
//                        Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                        foreach (PSObject psObject in findResults)
//                        {
//                            //var sKUAssigned = psObject.Members["SKUAssigned"].Value;
//                            var accessType = psObject.Members["AccessType"].Value.ToString();
//                            if (string.IsNullOrEmpty(accessType))
//                            {
//                                logger.Info("Group[{0}] does not have exchange online lincense.", o365GroupMailBox);
//                                break;
//                            }
//                            logger.Info("Group[{0}] IsPrivate: {1}", o365GroupMailBox, accessType);
//                            return accessType.Equals("Private", StringComparison.OrdinalIgnoreCase);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Cannot connect to exchange server throughout cmdlet, UserName : {1}. Reason : {0}", ex.ToString(), this.UserName);
//                return false;
//            }
//            finally
//            {
//                this.Dispose(true);
//                //if (runspace != null)
//                //{
//                //    runspace.Close();
//                //}
//            }
//            return false;
//        }

//        private string GetO365GroupLinks(string o365GroupMailbox, string type)
//        {
//            ValidateArguments(type);
//            try
//            {
//                logger.Info("Get group[{0}] {1}, admin: {2}", o365GroupMailbox, type, this.UserName);

//                var o365GroupsInfo = new List<string>();
//                if (Initialize())
//                {
//                    if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                    {
//                        runspace.Open();
//                    }
//                    using (Pipeline pipeLine = runspace.CreatePipeline())
//                    {
//                        Command getMemberName = new Command("Get-UnifiedGroupLinks");
//                        getMemberName.Parameters.Add("Identity", o365GroupMailbox);
//                        getMemberName.Parameters.Add("LinkType", type);
//                        pipeLine.Commands.Add(getMemberName);
//                        Command commandSelect = new Command("Select-Object");
//                        commandSelect.Parameters.Add("Property", new string[] { "PrimarySmtpAddress", "WindowsLiveID", "SKUAssigned" });
//                        pipeLine.Commands.Add(commandSelect);
//                        Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                        foreach (PSObject psObject in findResults)
//                        {
//                            //var sKUAssigned = psObject.Members["SKUAssigned"].Value;
//                            var ownerEmail = psObject.Members["PrimarySmtpAddress"].Value.ToString();
//                            var ownerName = psObject.Members["WindowsLiveID"].Value.ToString();
//                            if (string.IsNullOrEmpty(ownerEmail))
//                            {
//                                logger.Info("Group[{0}] {1}[{2}] does not have exchange online lincense.", o365GroupMailbox, type, ownerName);
//                                continue;
//                            }
//                            //if (!IsLicenseEnable(sKUAssigned))
//                            //{
//                            //    logger.Info("Group[{0}] {1}[{2}({3})] does not have exchange online lincense.", o365GroupMailbox, type, ownerEmail, ownerName);
//                            //    continue;
//                            //}
//                            logger.Info("Group[{0}] {1}: {2}", o365GroupMailbox, type, ownerEmail);
//                            return ownerEmail;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Cannot connect to exchange server throughout cmdlet, UserName : {1}. Reason : {0}", ex.ToString(), this.UserName);
//                throw;
//            }
//            finally
//            {
//                this.Dispose(true);
//                //if (runspace != null)
//                //{
//                //    runspace.Close();
//                //}
//            }
//            logger.Warn("Failed to get group {0}, group: {1}, current user: {2}", type, o365GroupMailbox, this.UserName);
//            return string.Empty;
//        }

//        private static void ValidateArguments(string type)
//        {
//            if (!string.Equals("Members", type, StringComparison.OrdinalIgnoreCase) &&
//                !string.Equals("Owners", type, StringComparison.OrdinalIgnoreCase))
//            {
//                throw new ArgumentException("type");
//            }
//        }


//        public override Dictionary<string, object> GetO365GroupMailboxAndSiteUrl()
//        {
//            //var o365GroupsMailbox = new List<string>();
//            //var o365GroupsSharePointSiteUrl = new List<string>();
//            //var o365GroupsInfo1 = Tuple.Create<List<string>, List<string>>(o365GroupsMailbox, o365GroupsSharePointSiteUrl);
//            var o365GroupsInfo = new Dictionary<string, object>();

//            try
//            {
//                logger.Info("O365 Group Account : {0}", this.UserName);
//                if (Initialize())
//                {
//                    try
//                    {
//                        if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                        {
//                            runspace.Open();
//                        }

//                        using (Pipeline pipeLine = runspace.CreatePipeline())
//                        {
//                            Command getGroup = new Command("Get-UnifiedGroup");
//                            getGroup.Parameters.Add("ResultSize", "Unlimited");
//                            pipeLine.Commands.Add(getGroup);
//                            Command commandSelect = new Command("Select-Object");
//                            commandSelect.Parameters.Add("Property", new string[] { "SharePointSiteUrl", "PrimarySmtpAddress" });
//                            pipeLine.Commands.Add(commandSelect);
//                            Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                            foreach (PSObject psObject in findResults)
//                            {
//                                var o365GroupMailbox = psObject.Members["PrimarySmtpAddress"].Value;
//                                var sharePointSiteUrl = psObject.Members["SharePointSiteUrl"].Value;
//                                if (o365GroupMailbox != null)
//                                {
//                                    o365GroupsInfo.Add(o365GroupMailbox.ToString(), sharePointSiteUrl);
//                                }
//                            }
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        var errorMessage = CheckPermission();
//                        if (!string.IsNullOrEmpty(errorMessage))
//                        {
//                            logger.Warn("Cannot connect to exchange server throughout cmdlet. Reason:{0}", ex);
//                            throw new Exception(errorMessage);
//                        }
//                        throw;
//                    }
//                }
//            }
//            finally
//            {
//                this.Dispose(true);
//                //if (runspace != null)
//                //{
//                //    runspace.Close();
//                //}
//            }
//            return o365GroupsInfo;
//        }

//        private string CheckPermission()
//        {
//            string errorMessage = null;
//            if (runspace != null && runspace.RunspaceStateInfo.State == RunspaceState.Opened)
//            {
//                using (Pipeline pipeLine = runspace.CreatePipeline())
//                {
//                    try
//                    {
//                        Command getRole = new Command("Get-ManagementRoleAssignment");
//                        getRole.Parameters.Add("Role", "Mail Recipients");
//                        getRole.Parameters.Add("-GetEffectiveUsers");
//                        getRole.Parameters.Add("-RoleAssignee", GetRecipient());
//                        pipeLine.Commands.Add(getRole);
//                        Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                        if (findResults.Count == 0)
//                        {
//                            logger.Warn("This account may not be a global administrator.");
//                            errorMessage = "NotGlobalAdmin";
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Error("This user does not have permission 'RoleManagement', Message : {0}", ex.Message);
//                        errorMessage = "NotGlobalAdmin";
//                    }
//                }
//            }
//            else
//            {
//                logger.Info("Runspace state is invalid,Skip check permisssion for failed command.");
//            }

//            return errorMessage;
//        }

//        private string GetRecipient()
//        {
//            using (Pipeline pipeLine = runspace.CreatePipeline())
//            {
//                var getUserName = new Command("Get-Recipient");
//                getUserName.Parameters.Add("Identity", this.UserName);
//                pipeLine.Commands.Add(getUserName);
//                return InternalInvoke(pipeLine).Select(obj => obj.Members["Name"].Value.ToString()).FirstOrDefault();
//            }
//        }

//        private List<string> GetUserMailbox(bool loadResourceMailbox, bool loadArchiveMailbox)
//        {
//            var users = new List<string>();

//            using (Pipeline pipeLine = runspace.CreatePipeline())
//            {
//                Command getUser = new Command("Get-Mailbox");
//                getUser.Parameters.Add("RecipientTypeDetails", new string[] { "usermailbox", "shared", "EquipmentMailbox", "RoomMailbox" });
//                getUser.Parameters.Add("ResultSize", "Unlimited");
//                pipeLine.Commands.Add(getUser);
//                Command commandSelect = new Command("Select-Object");
//                commandSelect.Parameters.Add("Property", new string[] { "WindowsEmailAddress", "RecipientTypeDetails", "ArchiveName", "SKUAssigned" });
//                pipeLine.Commands.Add(commandSelect);
//                foreach (PSObject psObject in InternalInvoke(pipeLine))
//                {
//                    var sKUAssigned = psObject.Members["SKUAssigned"].Value;
//                    string recipientTypeDetails = psObject.Members["RecipientTypeDetails"].Value.ToString();
//                    string emailAddress = psObject.Members["WindowsEmailAddress"].Value.ToString();
//                    if (IsUserMailbox(recipientTypeDetails) && !IsLicenseEnable(sKUAssigned)) continue;// skip user mailbox without license

//                    if (recipientTypeDetails.Equals("EquipmentMailbox", StringComparison.OrdinalIgnoreCase)
//                        || recipientTypeDetails.Equals("RoomMailbox", StringComparison.OrdinalIgnoreCase))
//                    {
//                        if (loadResourceMailbox)
//                        {
//                            string resourceEmailAddress = string.Format("{0}({1})", emailAddress, ExchangeConstants.ResourceMailbox);
//                            users.Add(resourceEmailAddress);
//                        }
//                    }
//                    else
//                    {
//                        users.Add(emailAddress);//
//                    }
//                    if (loadArchiveMailbox)
//                    {
//                        string archiveName = psObject.Members["ArchiveName"].Value.ToString();
//                        if (!string.IsNullOrEmpty(archiveName.Trim()))
//                        {
//                            string archiveEmailAddress = string.Format("{0}({1})", emailAddress, ExchangeConstants.InPlaceArchiveMailbox);
//                            users.Add(archiveEmailAddress);
//                        }
//                    }
//                }
//                logger.Info("There are {0} mailboxes in the organization.", users.Count);
//            }

//#if DEBUG
//            var pfEmail = string.Format("public folder mailbox-{0}", this.AuthObject.DomainName);
//            users.Add(pfEmail);
//            logger.Info("Fake PF mailbox({0})", pfEmail);
//#endif

//            return users;
//        }

//        private bool IsLicenseEnable(object sKUAssigned)
//        {
//            var isLicensed = sKUAssigned as bool?;
//            if (isLicensed.HasValue) return isLicensed.Value;
//            return false;
//        }

//        private bool IsUserMailbox(string recipientTypeDetails)
//        {
//            return recipientTypeDetails.Equals("UserMailbox", StringComparison.OrdinalIgnoreCase);
//        }

//        /// <summary>
//        /// Enable organization customization via powershell.
//        /// In the Microsoft datacenters, certain objects are consolidated to save space. 
//        /// When you use Exchange Online PowerShell or the Exchange admin center to modify 
//        /// one of these objects for the first time, you may encounter an error message that 
//        /// tells you to run the Enable-OrganizationCustomization cmdlet.
//        /// https://technet.microsoft.com/en-us/library/jj200665(v=exchg.160).aspx
//        /// </summary>
//        private void EnableOrganizationCustomization()
//        {
//            try
//            {
//                if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                {
//                    runspace.Open();
//                }
//                using (Pipeline pipeLine = runspace.CreatePipeline())
//                {
//                    Command enableCustomization = new Command("Enable-OrganizationCustomization");
//                    pipeLine.Commands.Add(enableCustomization);
//                    Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                    foreach (PSObject psObject in findResults)
//                    {
//                        logger.Info("Enable OrganizationCustomization Successfully.");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Cannot connect to exchange server throughout cmdlet. Reason:{0}", ex.ToString());
//                throw;
//            }
//        }

//        public override bool SetApplicationImpersonationRole()
//        {
//            try
//            {
//                if (Initialize())
//                {
//                    EnableOrganizationCustomization();
//                    try
//                    {
//                        if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
//                        {
//                            runspace.Open();
//                        }
//                        using (Pipeline pipeLine = runspace.CreatePipeline())
//                        {
//                            Command setRole = new Command("New-ManagementRoleAssignment");
//                            setRole.Parameters.Add("Name", GenerateRoleName(this.UserName));
//                            setRole.Parameters.Add("Role", "ApplicationImpersonation");
//                            setRole.Parameters.Add("User", this.UserName);
//                            pipeLine.Commands.Add(setRole);
//                            Collection<PSObject> findResults = InternalInvoke(pipeLine);
//                            foreach (PSObject psObject in findResults)
//                            {
//                                logger.Info("Set impersonate id : {0} Successfully.", this.UserName);
//                            }
//                            logger.Info("Impersonate id : {0}", this.UserName);
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Warn("Cannot connect to exchange server throughout cmdlet, UserName : {1}. Reason : {0}", ex.ToString(), this.UserName);
//                        throw;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Set impersonate id with exception : {0}, UserName : {1}", ex.ToString(), this.UserName);
//            }
//            finally
//            {
//                this.Dispose(true);
//                //try
//                //{
//                //    runspace.Close();
//                //    logger.Info("Sucessful to close runspace.");
//                //}
//                //catch (Exception ex)
//                //{
//                //    logger.Warn("An error occurred while closing runspace. Reason:{0}.", ex.ToString());
//                //}
//            }
//            return true;
//        }
        
//        private static Collection<PSObject> InternalInvoke(Pipeline pipeLine)
//        {
//            var result = pipeLine.Invoke();
//            LogErrorMessage(pipeLine);
//            return result;
//        }

//        private static void LogErrorMessage(Pipeline pipeLine)
//        {
//            if (pipeLine.Error != null && pipeLine.Error.Count > 0)
//            {
//                var error = pipeLine.Error.Read() as ErrorRecord;
//                var reason = error?.CategoryInfo?.Reason;
//                logger.Warn($"Piple line invoke with reason: {reason ?? "Null"}, error: {error?.ToString() ?? "Null"}");
//                if (reason != null)
//                {
//                    switch (reason)
//                    {
//                        case "ManagementObjectNotFoundException":
//                            throw new ObjectNotFoundException(error?.ToString());
//                        default:
//                            break;
//                    }
//                }
//            }
//        }


//        private string GenerateRoleName(string username)
//        {
//            return string.Format("IR_{0}",
//                 this.UserName.Length <= 60 ?
//                 this.UserName :
//                 username.Substring(0, 60));
//        }
//        private static bool ValidateRedirectionUrlCallback(String RedirectionUrl)
//        {
//            return true;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (!disposing) return;
//            if (runspace != null)
//            {
//                try
//                {
//                    if (runspace.RunspaceStateInfo.State != RunspaceState.Closed &&
//                        runspace.RunspaceStateInfo.State != RunspaceState.Closing)
//                    {
//                        runspace.Close();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("Error occurred while closing runspace. Reason:{0}", ex.ToString());
//                }
//                finally
//                {
//                    runspace.Dispose();
//                }
//            }
//        }
//    }

    public class ExchangeUserWithGraph : ExchangeUser
    {
        public ExchangeUserWithGraph(AuthObject authObj)
            : base(authObj)
        {
        }

        protected IAppTokenAuthObject AppTokenAuthObject
        {
            get
            {
                return this.AuthObject as IAppTokenAuthObject;
            }
        }

        public override List<string> GetUsers(bool loadResourceMailbox, bool loadArchiveMailbox)
        {
            throw new NotImplementedException();
            //var list = new List<string>();
            //var serviceUri = BuildServiceUri();
            //var client = new ActiveDirectoryClient(serviceUri, async () => this.AppTokenAuthObject.GetAccessToken());
            //var users = client.Users.Take(20).ExecuteAsync().Result;
            //while (true)
            //{
            //    foreach (var item in users.CurrentPage)
            //    {
            //        if (!string.IsNullOrEmpty(item.Mail))
            //        {
            //            if ("Guest".Equals(item.UserType, StringComparison.OrdinalIgnoreCase))
            //            {
            //                //stringBuilder.AppendFormat("External User:{0}\r\n", item.Mail);
            //            }
            //            else
            //            {
            //                if (item.ProvisionedPlans.Count > 0)
            //                {
            //                    var exchangePlan = item.ProvisionedPlans.FirstOrDefault(a => a.Service.Equals("exchange", StringComparison.OrdinalIgnoreCase));

            //                    if (exchangePlan == null)
            //                    {
            //                        logger.Debug("Non-User Mailbox: {0}", item.Mail);
            //                    }
            //                    else if (exchangePlan.ProvisioningStatus.Equals("success", StringComparison.OrdinalIgnoreCase) &&
            //                        exchangePlan.CapabilityStatus.Equals("enabled", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        list.Add(item.Mail);
            //                    }
            //                }
            //                else
            //                {
            //                    logger.Debug("Non-User Mailbox: {0}", item.Mail);
            //                }
            //            }
            //        }
            //    }
            //    if (users.MorePagesAvailable)
            //    {
            //        users = users.GetNextPageAsync().Result;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //return list;
        }

        public override bool IsO365GroupPrivate(string o365GroupMailBox)
        {
            try
            {
                var accessToken = this.AppTokenAuthObject.GetAccessToken();
                return GetO365GroupVisibility(o365GroupMailBox, accessToken);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when judging group is private,Error:{1}", ex.ToString());
                return false;
            }
        }

        public override string GetO365GroupMember(string o365GroupMailBox)
        {
            try
            {
                var accessToken = this.AppTokenAuthObject.GetAccessToken();

                var groupId = GetGroupIdByName(o365GroupMailBox, accessToken);
                return GetGroupMemberById(accessToken, groupId);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when get group member. Error:{0}", ex);
            }
            return string.Empty;
        }

        private string GetGroupMemberById(string accessToken, string groupId)
        {
            string groupMember = null;
            try
            {
                groupMember = GetGroupMember(accessToken, groupId);
                logger.Info("Group member: {0}", groupMember);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred when get group member.Retry Time: 1 ,Error:{1}", ex.ToString());
                groupMember = GetGroupMember(accessToken, groupId);
                logger.Info("Group member: {0}", groupMember);
            }
            return groupMember;
        }

        public override string GetO365GroupOwner(string o365GroupMailBox)
        {
            try
            {
                var accessToken = this.AppTokenAuthObject.GetAccessToken();

                var groupId = GetGroupIdByName(o365GroupMailBox, accessToken);
                return GetGroupOwnerById(accessToken, groupId);
            }
            catch (Exception ex)
            {
                logger.Error("An Error occurred when get group owner. Error:{0}", ex);
            }
            return string.Empty;
        }

        private string GetGroupOwnerById(string accessToken, string groupId)
        {
            string groupOwner = null;
            try
            {
                groupOwner = GetGroupOwner(accessToken, groupId);
                logger.Info("Group Owner: {0}", groupOwner);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group owner.Retry Time: 1 ,Error:{1}", ex.ToString());
                groupOwner = GetGroupOwner(accessToken, groupId);
                logger.Info("Group Owner: {0}", groupOwner);
            }
            return groupOwner;
        }

        private string GetGroupIdByName(string o365GroupMailBox, string accessToken)
        {
            var groupId = string.Empty;
            try
            {
                groupId = GetGroupId(accessToken, o365GroupMailBox);
                logger.Info("Group Id: {0}", groupId);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group id.Retry Time: 1 ,Error:{1}", ex.ToString());
                groupId = GetGroupId(accessToken, o365GroupMailBox);
                logger.Info("Group Id: {0}", groupId);
            }
            return groupId;
        }

        private bool GetO365GroupVisibility(string o365GroupMailBox, string accessToken)
        {
            var groupVisibility = string.Empty;
            try
            {
                groupVisibility = GetGroupVisibility(accessToken, o365GroupMailBox);
                logger.Info("Group Visibility Value: {0}", groupVisibility);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group visibility.Retry Time: 1 ,Error:{1}", ex.ToString());
                groupVisibility = GetGroupVisibility(accessToken, o365GroupMailBox);
                logger.Info("Group Visibility Value: {0}", groupVisibility);
            }
            return groupVisibility.Equals("Private", StringComparison.OrdinalIgnoreCase);
        }

        public override Dictionary<string, object> GetO365GroupMailboxAndSiteUrl()
        {
            throw new NotImplementedException();
        }

        //private Uri BuildServiceUri()
        //{
        //    return new Uri(new Uri(this.AppTokenAuthObject.ResourceUrl), this.AppTokenAuthObject.DomainName);
        //}

        public override bool SetApplicationImpersonationRole()
        {
            return true;
            //throw new NotImplementedException();
        }

        public override string ServiceUrl
        {
            get
            {
                return null;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        private string GetGroupId(string accessToken, string o365GroupMailBox)
        {
            var group = new GetGroupByMail(this.AppTokenAuthObject.ResourceUrl, accessToken, o365GroupMailBox);
            var listGroupObj = (ListGroupsObj)group.GetApiResult();
            return listGroupObj.Value[0].Id;
        }

        private string GetGroupOwner(string accessToken, string groupId)
        {
            var owner = new ListGroupOwners(this.AppTokenAuthObject.ResourceUrl, accessToken, groupId);
            var listGroupOwnerObj = (ListGroupOwnersObj)owner.GetApiResult();
            return listGroupOwnerObj.Value[0].UserPrincipalName;
        }

        private string GetGroupMember(string accessToken, string groupId)
        {
            var members = new ListGroupMembers(this.AppTokenAuthObject.ResourceUrl, accessToken, groupId);
            var listGroupOwnerObj = (ListGroupMembersObj)members.GetApiResult();
            return listGroupOwnerObj.Value[0].UserPrincipalName;
        }

        private string GetGroupVisibility(string accessToken, string o365GroupMailBox)
        {
            var group = new GetGroupByMail(this.AppTokenAuthObject.ResourceUrl, accessToken, o365GroupMailBox);
            var listGroupObj = (ListGroupsObj)group.GetApiResult();
            return listGroupObj.Value[0].Visibility;
        }
    }
}
