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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Taxonomy.ContentTypeSync;

namespace AvePoint.ObjectModel.Server13
{
    class AveContentTypePublisher : IAveContentTypePublisher
    {
        AveSite aveSite;
        bool newCreateSite;
        ContentTypePublisher ctPublisher;

        public AveContentTypePublisher()
        { }

        public AveContentTypePublisher(IAveSite site)
        {
            this.aveSite = site as AveSite;
            ctPublisher = new ContentTypePublisher(aveSite.Site);
            newCreateSite = false;
        }

        public AveContentTypePublisher(IAveTermStore store)
        {
            this.aveSite = new AveSite(store.ContentTypePublishingHub.ToString());
            newCreateSite = true;
            ctPublisher = new ContentTypePublisher(aveSite.Site);
        }

        public bool IsPublished(IAveContentType contentType)
        {
            AveContentType ct = contentType as AveContentType;
            return this.ctPublisher.IsPublished(ct.ContentType);
        }

        public void Dispose()
        {
            if (newCreateSite && this.aveSite != null)
            {
                this.aveSite.Dispose();
            }
        }

        public bool IsContentTypeSharingEnabled(IAveSite hubSite)
        {
            return ContentTypePublisher.IsContentTypeSharingEnabled((hubSite as AveSite).Site);
        }

        public void Publish(IAveContentType contentType)
        {
            ctPublisher.Publish((contentType as AveContentType).ContentType);
        }

        public void Unpublish(IAveContentType contentType)
        {
            ctPublisher.Unpublish((contentType as AveContentType).ContentType);
        }
    }
}
