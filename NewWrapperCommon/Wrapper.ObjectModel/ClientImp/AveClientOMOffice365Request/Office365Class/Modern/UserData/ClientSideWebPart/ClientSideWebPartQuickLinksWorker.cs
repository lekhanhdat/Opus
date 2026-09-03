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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Collections.Generic;
    using OfficeDevPnP.Core.Pages;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;

    class ClientSideWebPartQuickLinksWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "c70391ea-0b10-4ee9-b2b4-006d3fcad0cd";

        protected override List<ClientSideWebpartProperty> PotentialHtmlPropertiesAndTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override List<ClientSideWebpartProperty> PotentialPropertiesAndTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Process(ClientSideWebPart webPart, IAveFile document, AveSiteMappingManager mapping, bool lastPost)
        {
            var requirePostAction = false;

            return !requirePostAction;
        }
    }
}
