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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    internal class AveCustomHashtable : Hashtable
    {
        private Action<object, object> _AddAction;
        private Action<object> _RemoveAction;

        public AveCustomHashtable()
            : base()
        {
        }

        public AveCustomHashtable(Action<object, object> customAddAction, Action<object> customRemoveAction = null)
            : base()
        {
            _AddAction = customAddAction;
            _RemoveAction = customRemoveAction;
        }

        public AveCustomHashtable(IDictionary properties, Action<object, object> customAddAction, Action<object> customRemoveAction = null)
            : base(properties)
        {
            _AddAction = customAddAction;
            _RemoveAction = customRemoveAction;
        }

        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
                if (_AddAction != null)
                {
                    _AddAction(key, value);
                }
            }
        }

        public override void Add(object key, object value)
        {
            base.Add(key, value);
            if (_AddAction != null)
            {
                _AddAction(key, value);
            }
        }

        //public override void Remove(object key)
        //{
        //    base.Remove(key);
        //    if (_RemoveAction != null)
        //    {
        //        _RemoveAction(key);
        //    }
        //}
    }
}
