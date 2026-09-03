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
using Microsoft.SharePoint.Client;
using System;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2016ListItemRestore:Ave2013ListItemRestore, IDisposable
    {
        private AveClientOM2016Request mRequest;
        private ClientContext mContext;
        private object mObj;
        private Site mSite;

        public Ave2016ListItemRestore(AveClientOM2016Request request, Site site, ClientContext context, object obj)
            : base(request, site, context, obj)
        {
            this.mRequest = request;
            this.mSite = site;
            this.mContext = context;
            this.mObj = obj;
        }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}