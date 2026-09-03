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
namespace AvePoint.Wrapper.Restore.Core
{
    using System;
    using System.Collections.Generic;
    using GCommon;

    [Obsolete("SharePoint Online removed it, use News worker instead.")]
    class ClientSideWebPartNewsListWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "0ef418ba-5d19-4ade-9db0-b339873291d0";

        protected override AveLogger logger { get { return AveLogger.GetInstance(typeof(ClientSideWebPartNewsListWorker)); } }

        public override bool Process(AveClientSideWebPart webPart, AveSPDoc document, bool lastPost)
        {
            var requirePostAction = false;

            return !requirePostAction;
        }
    }
}
