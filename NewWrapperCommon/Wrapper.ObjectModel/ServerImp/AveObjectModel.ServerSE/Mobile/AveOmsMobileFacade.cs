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



using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveOmsMobileFacade : IAveOmsMobileFacade
    {
        private const string mOmsMobileFacade_Type = "Microsoft.SharePoint.SPOmsMobileFacade";
        private const string mOmsMobileFacade_GetOutboundSmsAccount_Method = "GetOutboundSmsAccount";
        private object mOmsMobileFacade;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint type name")]
        public AveOmsMobileFacade()
        {
            mOmsMobileFacade = AveAssemblyUtility.CreateInstance(mOmsMobileFacade_Type);
        }

        #region IAveOmsMobileFacade Members

        public IAveMobileMessagingAccount GetOutboundSmsAccount(IAveWebApplication webApp)
        {
            IAveMobileMessagingAccount outboundSmsServiceAccount = webApp.OutboundSmsServiceAccount;
            if (outboundSmsServiceAccount == null)
            {
                AveAdministrationWebApplication administrationWebApp = new AveAdministrationWebApplication();
                outboundSmsServiceAccount = administrationWebApp.Local.OutboundSmsServiceAccount;
            }
            return outboundSmsServiceAccount;
        }

        #endregion
    }
}
