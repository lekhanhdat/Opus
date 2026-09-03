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
    using AvePoint.GCommon.Utility.Cryptography;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ExchangeConstants
    {
        public const char PathParser = (char)0x12;
        public static readonly string PathStarts = ((char)0x12).ToString() + ((char)0x12).ToString();
        public const string PathCombine = "\\";
        public const char PathCombineChar = '\\';
        public const long FolderSize = 51200;
        public const string DeleteStatus = "Delete";
        public const string ShellUrl = "https://schemas.microsoft.com/powershell/Microsoft.Exchange";
        public const string InPlaceArchiveMailbox = "In-Place Archive Mailbox";
        public const string ResourceMailbox = "Resource Mailbox";
        public const string RecoverableItemsMailbox = "Recoverable Items Mailbox";
        public const string Archive_RecoverableItemsMailbox = "In-Place Archive_Recoverable Items Mailbox";
        public const string Resource_RecoverableItemsMailbox = "Resource_Recoverable Items Mailbox";
        public const string IMPERSONATION_HEADER_NAME = "X-AnchorMailbox";

        public const string SYSTEM_FOLDER_RECOVERABLE_ITEMS = "Recoverable Items folder (System)";
        public const string SYSTEM_FOLDER_RECOVERABLE_ITEMS_RESTORE = "Recoverable Items (System)_Restored";

        public const string ERRORMESSAGE_GROUP_NONEUSER = "{0} does not have any owners or members,please add it and try again";
        public const string MicrosoftTeamsPath = @"WindowsPowerShell\Modules\MicrosoftTeams";
        public const string MicrosoftTeamsFileName = "MicrosoftTeams.psm1";
        public const string ERRORMESSAGE_AUDITS_FOLDER = "Access is denied. Check credentials and try again., Non-system logon cannot access Audits folder.";
        public const string CALENDAR_LOGGING = "\ufffeRecoverable Items\ufffeCalendar Logging";
        public const string ExtendedPropertyGuid = "F6E4BA45-C83C-45DA-8F38-43BD3FE76D5C";
        public const string ConversationThreadType = "IOpenTypedFacet.SkypeSpaces_ConversationPost_Extension#ThreadType";
        public const string ConversationThreadId = "IOpenTypedFacet.SkypeSpaces_ConversationPost_Extension#ThreadId";
        public const string ConversationTopicId = "IOpenTypedFacet.SkypeSpaces_ConversationPost_Extension#ParentMessageId";
        public const string ConversationChannel = "IOpenTypedFacet.SkypeSpaces_ConversationPost_Extension#Topic";
        public const string ConversationLinkId = "IOpenTypedFacet.Com_Compliance_Callback#LinkId";
        public const string ConversationEmptyBody = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n</head>\r\n<body>\r\n<div></div>\r\n</body>\r\n</html>\r\n";
        public const string ConversationDeleteBody = "<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n</head>\r\n<body>\r\n<div>This message has been deleted.</div>\r\n</body>\r\n</html>\r\n";
        public const string SharedDocuments = "Shared Documents";
        public const string SharedDocuments_Escape = "Shared%20Documents";
        /// <summary>
        /// service plan Id 与 license 对应关系可以参考官方文档：
        /// https://docs.microsoft.com/en-us/azure/active-directory/users-groups-roles/licensing-service-plan-reference
        /// </summary>
        public static Dictionary<String, Boolean> LicenseIdDic = new Dictionary<String, Boolean>(StringComparer.OrdinalIgnoreCase)
        {
            {"9aaf7827-d63c-4b61-89c3-182f06f82e5c",true },//Exchange online license (plan 1)    
            {"efb87545-963c-4e0d-99df-69c6916d9eb0",true },//Exchange online license (plan 2)     //EXCHANGE_S_ENTERPRISE
            {"4a82b400-a79f-41a4-b4e2-e94f5787b113",true },//Exchange online Kiosk //Office365 F1 //EXCHANGE_S_DESKLESS
            {"90927877-dcff-4af6-b346-2332c0b15bb7",true },//EXCHANGE_B_STANDARD //Unverified
            {"d42bdbd6-c335-4231-ab3d-c8f348d5aff5",true },//EXCHANGE_L_STANDARD //Unverified
            {"da040e0a-b393-4bea-bb76-928b3fa1cf5a",false },//EXCHANGE_S_ARCHIVE //Unverified
            {"1126bef5-da20-4f07-b45e-ad25d2581aa8",true },//EXCHANGE_S_ESSENTIALS //Unverified
            {"fc52cc4b-ed7d-472d-bbe7-b081c23ecc56",true },//EXCHANGE_S_STANDARD_MIDMARKET //Unverified
            {"8c3069c0-ccdb-44be-ab77-986203a67df2",true },//Exchange Online (Plan 2) for Government CLOUDIBM-197
            {"e9b4930a-925f-45e2-ac2a-3f7788ca6fdd",true },//Exchange Online (Plan 1) for Government CLOUDIBM-197

            {"33c4f319-9bdd-48d6-9c4d-410b750a4a5a",false },//Insights by MyAnalytics
            {"5136a095-5cf0-4aff-bec3-e84448b38ea5",false },//Information Protection for Office 365 - Premium
            {"efb0351d-3b08-4503-993d-383af8de41e3",false },//Information Protection for Office 365 - Standard
            {"617b097b-4b93-4ede-83de-5f075bb5fb2f",false },//Premium Encryption in Office 365
            {"b1188c4c-1b36-4018-b48b-ee07604f6feb",false },//Office 365 Privileged Access Management
            {"34c0d7a0-a70f-4668-9238-47f9fc208882",false },//Microsoft MyAnalytics (Full)
            {"9f431833-0334-42de-a7dc-70aa40db46db",false },//Customer Lockbox
            {"4de31727-a228-4ec3-a5bf-8e45b5ca48cc",false },//Office 365 Advanced eDiscovery
            {"8e0c0a52-6a6c-4d40-8370-dd62790dcd70",false },//Office 365 Advanced Threat Protection (Plan2)
            {"c4801e8a-cb58-4c35-aca6-f2dcc106f287",false },//Information Barriers
            {"2f442157-a11c-46b9-ae5b-6e39ff4e5849",false },//Microsoft 365 Advanced Auditing
            {"bf6f5520-59e3-4f82-974b-7dbbc4fd27c7",false },//Office 365 SafeDocs
            {"6db1f1db-2b46-403f-be40-e39395f08dbb",false },//Microsoft Customer Key
            {"6dc145d6-95dd-4191-b9c3-185575ee6f6b",false },//Microsoft Communications DLP
            {"41fcdd7d-4733-4863-9cf4-c65b83ce2df4",false },//Microsoft Communications Compliance
            {"e26c2fcc-ab91-4a61-b35c-03cdc8dddf66",false },//Microsoft Information Governance
            {"65cc641f-cccd-4643-97e0-a17e3045e541",false },//Microsoft Records Management
            {"199a5c09-e0ca-4e37-8f7c-b05d533e1ea2",false },//Microsoft Bookings 
            {"46129a58-a698-46f0-aa5b-17f6586297d9",false },//Data Investigations
            {"9d0c4ee5-e4a1-4625-ab39-d82b619b1a34",false },//AOSBR-16459
            {"d2d51368-76c9-4317-ada2-a12c004c432f",false },//AOSBR-16459
            {"897d51f1-2cfa-4848-9b30-469149f5e68e",false },//AOSBR-36962

            //{"5bfe124c-bbdc-4494-8835-f1297d457d79",false },//OUTLOOK CUSTOMER MANAGER //Unverified
            //{"176a09a6-7ec5-4039-ac02-b2791c6ba793",false },//EXCHANGE ONLINE ARCHIVING FOR EXCHANGE ONLINE //Unverified
            //{"9bec7e34-c9fa-40b7-a9d1-bd6d1165c7ed",false },//Unverified //AOSBR-15999 false
        };

        public static string ConvertItemId(string itemId)
        {
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
            byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(itemId));
            string idValue = string.Empty;
            char[] HEXChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            for (int i = 0; i < 4; i++)
            {
                byte t = result[i];
                idValue += HEXChar[(int)((t >> 4) & 0x0f)];
                idValue += HEXChar[(int)(t & 0x0f)];
            }
            return idValue;
        }

        internal const string MSTeamsPowershellCmdletsAppId = "12128f48-ec9e-42f0-b203-ea49fb6af367";
        internal const string MSExchangeRESTAPIBasedPowershellAppId = "fb78d390-0c51-40cd-8e17-fdbfab77341b";
        internal const string MicrosoftOfficeAppId = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
        internal const string AADPowerShellAppId = "1b730954-1685-4b74-9bfd-dac224a7b894";

        internal const string UnkonwUserDisplayName = "Unknown User";
        internal const string UnkonwUserEmail = "unknown_email_address@invalid.teams.ms";

        public const string ChinaNotSupportedHostedContentKey = "Agent.Teams.ChinaNotSupportedHostedContent_FD8720C9-9409-4DF8-BCF7-7AF5D2F5721F";
        public const string ImpersonationAccountNotConfiguredKey = "Agent.PublicFolder.NoImpersonationAccountConfigured_F0DA1B53-AF9F-4934-A7F4-45C1561A7398";

        public class ItemType
        {
            public const string Activity = "IPM.Activity"; // Journal entries
            public const string Appointment = "IPM.Appointment"; // Appointments
            public const string Contact = "IPM.Contact"; // Contacts
            public const string DistList = "IPM.DistList"; // Distribution lists
            public const string Document = "IPM.Document"; // Documents
            public const string Unknown = "IPM"; // Items with unknown form
            public const string Message = "IPM.Note"; // Email messages
            public const string MeetingRequest = "IPM.Schedule.Meeting.Request";
            public const string Post = "IPM.Post"; // Posting notes
            public const string StickyNote = "IPM.StickyNote"; // Notes
            public const string Task = "IPM.Task";
        }
    }
}