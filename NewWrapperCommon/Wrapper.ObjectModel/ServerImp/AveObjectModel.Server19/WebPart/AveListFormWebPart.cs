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

namespace AvePoint.ObjectModel.Server19
{
    class AveListFormWebPart : AveWebPart, IAveListFormWebPart
    {
        private ListFormWebPart mListFormWebPart;

        public AveListFormWebPart(AveLimitedWebPartManager manager, ListFormWebPart listFormWebPart)
            : base(manager, listFormWebPart, -1)
        {
            mListFormWebPart = listFormWebPart;
        }

        #region IAveListFormWebPart Members

        public string ListName
        {
            get
            {
                return mListFormWebPart.ListName;
            }
            set
            {
                mListFormWebPart.ListName = value;
            }
        }

        #endregion

        #region IAveListWebPart Members

        public Guid ListId
        {
            get
            {
                return mListFormWebPart.ListId;
            }
            set
            {
                mListFormWebPart.ListId = value;
            }
        }

        public AvePAGETYPE PageType
        {
            get
            {
                return (AvePAGETYPE)mListFormWebPart.PageType;
            }
            set
            {
                mListFormWebPart.PageType = (PAGETYPE)value;
            }
        }

        public AveViewFlags ViewFlags
        {
            get
            {
                return (AveViewFlags)mListFormWebPart.ViewFlags;
            }
            set
            {
                mListFormWebPart.ViewFlags = (SPViewFlags)value;
            }
        }

        public int ViewId
        {
            get
            {
                return mListFormWebPart.ViewId;
            }
            set
            {
                mListFormWebPart.ViewId = value;
            }
        }

        #endregion
    }
}
