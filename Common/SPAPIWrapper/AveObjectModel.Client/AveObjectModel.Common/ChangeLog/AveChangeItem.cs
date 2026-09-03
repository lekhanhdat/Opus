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

namespace AvePoint.ObjectModel.Common
{
    class AveChangeItem : AveChange, IAveChangeItem
    {
        internal AveChangeItem(IDictionary<string, object> changeProperties) : base(changeProperties) { }

        public int AfterId
        {
            get { return base.DataCache.GetProperty<int>("AfterId"); }
        }

        public Guid AfterListId
        {
            get { return base.DataCache.GetProperty<Guid>("AfterListId"); }
        }

        public int BeforeId
        {
            get { return base.DataCache.GetProperty<int>("BeforeId"); }
        }

        public Guid BeforeListId
        {
            get { return base.DataCache.GetProperty<Guid>("BeforeListId"); }
        }

        public int Id
        {
            get { return base.DataCache.GetProperty<int>("ItemId"); ; }
        }

        public Guid ListId
        {
            get { return base.DataCache.GetProperty<Guid>("ListId"); }
        }

        public Guid UniqueId
        {
            get { return base.DataCache.GetProperty<Guid>("UniqueId"); }
        }

        public Guid WebId
        {
            get { return base.DataCache.GetProperty<Guid>("WebId"); }
        }
    }
}
