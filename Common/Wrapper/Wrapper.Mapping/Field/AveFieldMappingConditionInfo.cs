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

namespace AvePoint.Wrapper.Mapping
{
    public class AveFieldMappingConditionInfo
    {
        #region site condition
        private string siteUrl;
        public string SiteUrl
        {
            get { return siteUrl; }
            set { siteUrl = value; }
        }

        private List<string> siteContentTypeCollection = new List<string>();
        public List<string> SiteContentTypeCollection
        {
            get { return siteContentTypeCollection; }
            set { siteContentTypeCollection = value; }
        }
        #endregion

        #region list condition
        private string listTemplateID;
        public string ListTemplateID
        {
            get { return listTemplateID; }
            set { listTemplateID = value; }
        }

        private string listTitle;
        public string ListTitle
        {
            get { return listTitle; }
            set { listTitle = value; }
        }

        private List<string> listContentTypeCollection = new List<string>();
        public List<string> ListContentTypeCollection
        {
            get { return listContentTypeCollection; }
            set { listContentTypeCollection = value; }
        }
        #endregion

        #region item condition
        private string itemName;
        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        #endregion
    }
}
