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
    using OfficeDevPnP.Core.Pages;
    using System.Collections.Generic;
    using System;
    using Microsoft.SharePoint;
    using AvePoint.Wrapper.Common;
    using Newtonsoft.Json.Linq;

    class ClientSideWebPartHeroWorker : ClientSideWebPartCommonWorker, IClientSideWebPartWorker
    {
        public const string Id = "c4bd7b2f-7b6e-4599-8485-16504575f590";
        protected List<string> Properties = new List<string>() { };

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
                return new List<ClientSideWebpartProperty>(){
                    new ClientSideWebpartProperty("content", ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                            new ClientSideWebpartProperty("previewImage",  ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
                                new ClientSideWebpartProperty("siteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
                                new ClientSideWebpartProperty("webId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site),
                                new ClientSideWebpartProperty("listId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.List),
                                new ClientSideWebpartProperty("id", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Item)
                            })
                        })
                    };
            }
        }

        //不用处理 serverProcessedContent 下面的属性，用PNP 再转成HTML 时候，这个属性不会被写回去，所以不用处理
        //private List<ClientSideWebpartProperty> heroPorperties = new List<ClientSideWebpartProperty> {
        //            new ClientSideWebpartProperty("customMetadata", ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
        //                    new ClientSideWebpartProperty("content[0].previewImage.url", ClientSideWebpartPropertyTypes.Invalid, new List<ClientSideWebpartProperty>(){
        //                        new ClientSideWebpartProperty("siteId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.SiteCollection),
        //                        new ClientSideWebpartProperty("webId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Site),
        //                        new ClientSideWebpartProperty("listId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.List),
        //                        new ClientSideWebpartProperty("uniqueId", ClientSideWebpartPropertyTypes.Guid, ClientSideWebpartPropertyScopes.Item)
        //                    })
        //                })
        //            };

    }
}
