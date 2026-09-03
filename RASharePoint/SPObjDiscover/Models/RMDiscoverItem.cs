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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SPObjDiscover.Models
{
    public class RMDiscoverItem
    {
        private IAveListItem _item;
        private AveDiscoverItem _discoverItem;
        public RMDiscoverItem(IAveListItem aveItem, AveDiscoverItem discoverItem)
        {
            _item = aveItem;
            _discoverItem = discoverItem;
        }
        
        public int? ID
        {
            get
            {
                int? Id = null;
                if (_discoverItem != null)
                {
                    Id = _discoverItem.ID;
                }
                else if (_item != null)
                {
                    Id = _item.ID;
                }
                return Id;
            }
        }

        public Guid DocID
        {
            get
            {
                Guid Id = Guid.Empty;
                if (_discoverItem != null)
                {
                    Id = _discoverItem.DocID;
                }
                else if (_item != null)
                {
                    Id = _item.UniqueId;
                }
                return Id;
            }
        }

        public Guid tp_GUID
        {
            get
            {
                Guid Id = Guid.Empty;
                if (_discoverItem != null)
                {
                    Id = _discoverItem.tp_GUID;
                }
                else if (_item != null)
                {
                    Id = _item.UniqueId;
                }
                return Id;
            }
        }

        public bool? Hidden
        {
            get
            {
                bool? hidden = null;
                if (_discoverItem != null)
                {
                    hidden = _discoverItem.Hidden;
                }
                else if (_item != null)
                {
                    hidden = false;
                }
                return hidden;
            }
        }

        public string Url
        {
            get
            {
                string url = null;
                if (_discoverItem != null)
                {
                    url = _discoverItem.FullUrl;
                }
                else if (_item != null)
                {
                    url = _item.Url;
                }
                return url;
            }
        }

        public ChangeType ChangeType
        {
            get
            {
                ChangeType type = ChangeType.None;
                if (_discoverItem != null)
                {
                    type = _discoverItem.ChangeType;
                }
                return type;
            }
        }

        public IAveListItem CurrentItem
        {
            get
            {
                return _item;
            }
        }

    }
    public class CleanUpItemEntry
    {
        public string ItemId { get; set; }
        public string Action { get; set; }
    }
}
