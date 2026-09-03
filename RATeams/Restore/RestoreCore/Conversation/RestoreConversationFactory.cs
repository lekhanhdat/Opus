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

namespace Office365GroupRestore
{
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
    using AvePoint.Wrapper.Common;
    using ExchangeUtility.Graph;

    internal static class RestoreConversationFactory
    {
        internal static RestoreConversation Create(BaseRestoreHelperBatch baseHelper, DataSource dataSource, RestoreConversationType mode) =>
             mode == RestoreConversationType.Html
                ? dataSource == DataSource.Graph ? new RestoreConversationFromGraphAsHtml(baseHelper) : new RestoreConversationFromEwsAsHtml(baseHelper)
                : dataSource == DataSource.Graph ? new RestoreConversationFromGraphAsPost(baseHelper) : new RestoreConversationFromEwsAsPost(baseHelper);

        internal static void Restore(this RestoreConversation instance, RestoreConfig config, AuthorizationManager authorizationManager, IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            //MicrosoftTeamsAPIBase exchangeMicrosoftTeams = null;
            //instance.Init(null, config);
            //if (instance is RestoreConversationAsPost)
            //{
            //    var authObj = authorizationManager.GetDelegateAppAuthObject(RestoreConfig.CurrentRestoreMailbox, DelegateAppCloudBackupModuleType.Channel) ?? throw new NoDelegateAppException();
            //    exchangeMicrosoftTeams = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj);
            //    instance.TeamsMembershipService = exchangeMicrosoftTeams.AuthObject.AuthType switch
            //    {
            //        AuthObjectType.PasswordAccessToken => exchangeMicrosoftTeams,
            //        AuthObjectType.AccessToken or _ => ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authorizationManager.GetAuthObjectForGraph(RestoreConfig.CurrentRestoreMailbox))
            //    };
            //}
            instance.Restore(dataCollection);
        }
    }
}