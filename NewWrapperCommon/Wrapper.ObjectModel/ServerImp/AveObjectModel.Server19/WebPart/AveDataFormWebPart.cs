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
using Microsoft.SharePoint.WebPartPages;
using Microsoft.SharePoint;
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveDataFormWebPart : AveWebPart, IAveDataFormWebPart
    {
        DataFormWebPart mDataFormWebPart;
        public AveDataFormWebPart(AveLimitedWebPartManager manager, DataFormWebPart dateFormWebPart)
            : base(manager, dateFormWebPart, -1)
        {
            mDataFormWebPart = dateFormWebPart;
        }

        #region IAveDataFormWebPart Members

        public string ListName
        {
            get
            {
                return mDataFormWebPart.ListName;
            }
            set
            {
                mDataFormWebPart.ListName = value;
            }
        }

        public string DataSourcesString
        {
            get
            {
                return mDataFormWebPart.DataSourcesString;
            }
            set
            {
                mDataFormWebPart.DataSourcesString = value;
            }
        }

        public string ParameterBindings
        {
            get
            {
                return mDataFormWebPart.ParameterBindings;
            }
            set
            {
                mDataFormWebPart.ParameterBindings = value;
            }
        }

        #endregion

        #region IAveListWebPart Members

        public Guid ListId
        {
            get
            {
                return mDataFormWebPart.ListId;
            }
            set
            {
                mDataFormWebPart.ListId = value;
            }
        }

        public AvePAGETYPE PageType
        {
            get
            {
                return (AvePAGETYPE)mDataFormWebPart.PageType;
            }
            set
            {
                mDataFormWebPart.PageType = (PAGETYPE)value;
            }
        }

        public AveViewFlags ViewFlags
        {
            get
            {
                return (AveViewFlags)mDataFormWebPart.ViewFlags;
            }
            set
            {
                mDataFormWebPart.ViewFlags = (SPViewFlags)value;
            }
        }

        public int ViewId
        {
            get
            {
                return mDataFormWebPart.ViewId;
            }
            set
            {
                mDataFormWebPart.ViewId = value;
            }
        }

        #endregion

    }
}
