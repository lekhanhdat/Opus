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
    class AveListViewWebPart : AveWebPart, IAveListViewWebPart
    {
        private ListViewWebPart mListViewWebPart;

        public AveListViewWebPart(AveLimitedWebPartManager manager, ListViewWebPart listViewWebPart)
            : base(manager, listViewWebPart, -1)
        {
            mListViewWebPart = listViewWebPart;
        }

        //TODO 查看谁最终调用这个构造函数
        public AveListViewWebPart()
            : this(null, new ListViewWebPart())
        { }

        #region IAveListWebPart Members

        public Guid ListId
        {
            get
            {
                return mListViewWebPart.ListId;
            }
            set
            {
                mListViewWebPart.ListId = value;
            }
        }

        public AvePAGETYPE PageType
        {
            get
            {
                return (AvePAGETYPE)mListViewWebPart.PageType;
            }
            set
            {
                mListViewWebPart.PageType = (PAGETYPE)value;
            }
        }

        public AveViewFlags ViewFlags
        {
            get
            {
                return (AveViewFlags)mListViewWebPart.ViewFlags;
            }
            set
            {
                mListViewWebPart.ViewFlags = (SPViewFlags)value;
            }
        }

        public int ViewId
        {
            get
            {
                return mListViewWebPart.ViewId;
            }
            set
            {
                mListViewWebPart.ViewId = value;
            }
        }

        #endregion

        #region IAveListViewWebPart Members

        public Guid WebId
        {
            get
            {
                return mListViewWebPart.WebId;
            }
            set
            {
                mListViewWebPart.WebId = value;
            }
        }

        public string ListName
        {
            get
            {
                return mListViewWebPart.ListName;
            }
            set
            {
                mListViewWebPart.ListName = value;
            }
        }

        public string ViewGuid
        {
            get
            {
                return mListViewWebPart.ViewGuid;
            }
            set
            {
                mListViewWebPart.ViewGuid = value;
            }
        }

        #endregion
    }
}
