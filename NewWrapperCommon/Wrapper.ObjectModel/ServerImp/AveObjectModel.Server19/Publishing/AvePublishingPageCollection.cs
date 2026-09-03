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
using Microsoft.SharePoint.Publishing;

namespace AvePoint.ObjectModel.Server19
{
    class AvePublishingPageCollection : AveAbstractCommonCollection<IAvePublishingPage>, IAvePublishingPageCollection
    {
        private PublishingPageCollection mPublishingPageCollections;
        private AveList mList;

        public AvePublishingPageCollection(AveList list, PublishingPageCollection publishingPageCollections)
            : base(publishingPageCollections)
        {
            mList = list;
            mPublishingPageCollections = publishingPageCollections;
        }

        #region IAvePublishingPageCollection Members

        public IAvePublishingPage Add(string pageName, IAvePageLayout pageLayout)
        {
            PublishingPage page = mPublishingPageCollections.Add(pageName, (PageLayout)pageLayout.PageLayout);
            if (page == null)
            {
                return null;
            }
            AveListItem listitem = new AveListItem(mList.Items as AveListItemCollection, page.ListItem);
            return new AvePublishingPage(listitem, page);
        }

        public IAvePublishingPage this[string pageUrl]
        {
            get
            {
                PublishingPage page = mPublishingPageCollections[pageUrl];
                if (page == null)
                {
                    return null;
                }
                AveListItem listitem = new AveListItem(mList.Items as AveListItemCollection, page.ListItem);
                return new AvePublishingPage(listitem, page);
            }
        }

        public override IAvePublishingPage this[int index]
        {
            get
            {
                PublishingPage page = mPublishingPageCollections[index];
                AveListItem listitem = new AveListItem(mList.Items as AveListItemCollection, page.ListItem);
                return new AvePublishingPage(listitem, page);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            AveListItem listitem = new AveListItem(mList.Items as AveListItemCollection, (t as PublishingPage).ListItem);
            return new AvePublishingPage(listitem, t as PublishingPage);
        }

        public override int Count
        {
            get { return mPublishingPageCollections.Count; }
        }

        #endregion
    }
}
