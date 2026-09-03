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
using Microsoft.SharePoint.WebPartPages;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveXsltListViewWebPart : AveDataFormWebPart, IAveXsltListViewWebPart
    {
        private XsltListViewWebPart mXsltListViewWebPart;

        public AveXsltListViewWebPart(AveLimitedWebPartManager manager, XsltListViewWebPart xsltListViewWebPart)
            : base(manager, xsltListViewWebPart)
        {
            mXsltListViewWebPart = xsltListViewWebPart;
        }

        public string ListName
        {
            get
            {
                return mXsltListViewWebPart.ListName;
            }
            set
            {
                mXsltListViewWebPart.ListName = value;
            }
        }

        public Guid WebId
        {
            get
            {
                return mXsltListViewWebPart.WebId;
            }
            set
            {
                mXsltListViewWebPart.WebId = value;
            }
        }

        public string ViewGuid
        {
            get
            {
                return mXsltListViewWebPart.ViewGuid;
            }
            set
            {
                mXsltListViewWebPart.ViewGuid = value;
            }
        }

        public Guid ListId
        {
            get
            {
                return mXsltListViewWebPart.ListId;
            }
            set
            {
                mXsltListViewWebPart.ListId = value;
            }
        }

        public AvePAGETYPE PageType
        {
            get
            {
                return (AvePAGETYPE)mXsltListViewWebPart.PageType;
            }
            set
            {
                mXsltListViewWebPart.PageType = (PAGETYPE)value;
            }
        }

        public AveViewFlags ViewFlags
        {
            get
            {
                return (AveViewFlags)mXsltListViewWebPart.ViewFlags;
            }
            set
            {
                mXsltListViewWebPart.ViewFlags = (SPViewFlags)value;
            }
        }

        public int ViewId
        {
            get
            {
                return mXsltListViewWebPart.ViewId;
            }
            set
            {
                mXsltListViewWebPart.ViewId = value;
            }
        }
    }
}
