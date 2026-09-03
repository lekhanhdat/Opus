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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveChange : AveClientObject, IAveChange
    {
        internal AveChange(Dictionary<string, object> changeProperties)
        {
            this.DataCache.AddPropertyies(changeProperties);
        }

        public long ChangeNumber
        {
            get { throw new NotImplementedException(); }
        }

        public IAveChangeCollection ChangeCollection
        {
            get { throw new NotImplementedException(); }
        }

        public AveChangeType ChangeType
        {
            get { return (AveChangeType)base.DataCache.GetProperty<int>("ChangeType"); }
        }

        public AveChangeToken ChangeToken
        {
            get { return new AveChangeToken(base.DataCache.GetProperty<string>("ChangeTokenString")); }
        }

        public Guid InternalListId
        {
            get { return base.DataCache.GetProperty<Guid>("ListId"); }
        }

        public Guid InternalUniqueId
        {
            get { return base.DataCache.GetProperty<Guid>("UniqueId"); }
        }

        public string InternalUrl
        {
            get { return base.DataCache.GetProperty<string>("InternalUrl"); }
        }

        public Guid InternalWebId
        {
            get { return base.DataCache.GetProperty<Guid>("WebId"); }
        }

        public Guid SiteId
        {
            get { return base.DataCache.GetProperty<Guid>("SiteId"); }
        }

        public DateTime Time
        {
            get { return base.DataCache.GetProperty<DateTime>("Time"); }
        }

        public object[] Rows
        {
            get { throw new NotImplementedException(); }
        }
    }
}
