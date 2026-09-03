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
    class AveChangedItem : AveClientObject,IAveChangedItem
    {
        private IAveRequest m_Request;
        private AveTermStore m_termStore;

        public AveChangedItem(IAveRequest m_Request, AveTermStore m_termStore, Dictionary<string, object> changedProperties)
        {
            this.m_Request = m_Request;
            this.m_termStore = m_termStore;
            base.DataCache.AddPropertyies(changedProperties);
        }

        public string ChangedBy
        {
            get
            {
                return base.DataCache.GetProperty<string>("ChangedBy");
            }
        }

        public DateTime ChangedTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ChangedTime");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public AveChangedItemType ItemType
        {
            get
            {
                return base.DataCache.GetProperty<AveChangedItemType>("ItemType");
            }
        }

        public AveChangedOperationType Operation
        {
            get
            {
                return base.DataCache.GetProperty<AveChangedOperationType>("Operation");
            }
        }
    }
}
