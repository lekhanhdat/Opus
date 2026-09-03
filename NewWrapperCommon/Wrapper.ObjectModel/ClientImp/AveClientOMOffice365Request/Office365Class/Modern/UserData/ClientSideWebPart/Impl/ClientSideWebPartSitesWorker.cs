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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    class ClientSideWebPartSitesWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "7cba020c-5ccb-42e8-b6fc-75b3149aba7b";

        protected override List<ClientSideWebpartProperty> PotentialPropertiesAndTypes
        {
            get
            {
                return new List<ClientSideWebpartProperty>()
                {
                    new ClientSideWebpartProperty("siteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
                    new ClientSideWebpartProperty("webId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site),
                    new ClientSideWebpartProperty("sites", ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                            new ClientSideWebpartProperty("ItemReference",  ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                                new ClientSideWebpartProperty("SiteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
                                new ClientSideWebpartProperty("WebId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site)
                            })
                        })
                };
            }
        }

        protected override List<ClientSideWebpartProperty> PotentialHtmlPropertiesAndTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
