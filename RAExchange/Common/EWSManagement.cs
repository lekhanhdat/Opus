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
using Aspose.Email.Clients.Exchange.WebService;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RAExchange.Authorization;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using ExchangeUtility;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph.Models.CallRecords;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using static Org.BouncyCastle.Math.EC.ECCurve;
using ExchangeVersion = Microsoft.Exchange.WebServices.Data.ExchangeVersion;

namespace AvePoint.RA.RAExchange.Common
{
    public class EWSManagement
    {
        public AppProfileInfo Profile { get; }
        public TokenType tokenType = TokenType.ApplicationToken;
        private ExchangeService service;
        private const ExchangeVersion EXCHANGE_VERSION = ExchangeVersion.Exchange2016;
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string GroupEmail;
        private string UserEmail;
        private bool UseEWS = true;
        public EWSManagement(string o365TenantId,string groupEmail,string userEmail)
        {
            try
            {
                if (UseEWS)
                {
                    UserEmail = userEmail;
                    GroupEmail = groupEmail;
                    var bposInfo = RABrowserClient.GetBPOSInfoByTenantId(o365TenantId);
                    bposInfo.ConnectionType = (GCommon.Contract.CentralAdmin.Object.BposConnectionType)BposConnectionType.AppToken;
                    bposInfo.UserAccountInfo.Username = "";
                    bposInfo.TenantGroupId = TenantLocalValue.LogonGroupId;
                    var authObject = AuthObjectFactory.CreateAuthObject(bposInfo, AuthResourceType.EWS);
                    service = new ExchangeService(EXCHANGE_VERSION);
                    authObject.BindToExchangeService(service);
                    service.ClientRequestId = Guid.NewGuid().ToString();
                    service.ReturnClientRequestId = true;
                    service.AutodiscoverUrl(groupEmail, RedirectionUrlValidationCallback);
                    authObject.AddImpersonationHeader(service, userEmail);
                    authObject.SetImpersonatedUserId(service, userEmail);
                }


            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while initializing EWSManagement for tenant {o365TenantId} and group {groupEmail}.err:{mLog}");
            }
        }
        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            return redirectionUrl.StartsWith("https://");
        }
        public List<Item> GetExchangeItems(string topic)
        {
            try
            {
                if (UseEWS)
                {
                    var groupMailbox = new Mailbox(GroupEmail);
                    SearchFilter filter = new SearchFilter.IsEqualTo(ItemSchema.Subject, topic);
                    ItemView view = new ItemView(int.MaxValue);
                    var inbox = Folder.Bind(service, new FolderId(WellKnownFolderName.Inbox, groupMailbox)).GetAwaiter().GetResult();
                    var items = inbox.FindItems(filter, view).GetAwaiter().GetResult();
                    return items.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while getting Exchange item with topic {topic}. Error: {ex}");
                return null;
            }
        }
        public Appointment GetCalendarEvent(string subject)
        {
            try
            {
                if (UseEWS)
                {
                    var groupMailbox = new Mailbox(GroupEmail);
                    SearchFilter filter = new SearchFilter.IsEqualTo(ItemSchema.Subject, subject);
                    ItemView view = new ItemView(int.MaxValue);
                    var inbox = Folder.Bind(service, new FolderId(WellKnownFolderName.Calendar, groupMailbox)).GetAwaiter().GetResult();
                    var items = inbox.FindItems(filter, view).GetAwaiter().GetResult();
                    return items?.FirstOrDefault() as Appointment;
                }
                return null;
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while getting Exchange event with subject {subject}. Error: {ex}");
                return null;
            }
        }

    }
    public class ConsoleTraceListener : ITraceListener
    {
        public void Trace(string traceType, string traceMessage)
        {
            Console.WriteLine($"[{traceType}] {traceMessage}");
        }
    }
}
