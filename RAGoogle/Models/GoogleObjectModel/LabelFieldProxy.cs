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
using System.Threading.Tasks;

namespace RAGoogle.Models.GoogleObjectModel
{
    public class LabelFieldProxy : GDriveObjectProxy
    {
        public LabelFieldProxy(Dictionary<string, object> properties) : base(properties)
        {
        }
        public IList<string> DateString
        {
            get
            {
                return GetProperty<IList<string>>("DateString");
            }
        }
        public string Id
        {
            get
            {
                return GetProperty<string>("Id");
            }
        }
        public IList<long?> Integer
        {
            get
            {
                return GetProperty<IList<long?>>("Integer");
            }
        }
        public string Kind
        {
            get
            {
                return GetProperty<string>("Kind");
            }
        }
        public IList<string> Selection
        {
            get
            {
                return GetProperty<IList<string>>("Selection");
            }
        }
        public IList<string> Text
        {
            get
            {
                return GetProperty<IList<string>>("Text");
            }
        }
        public IList<UserProxy> User
        {
            get
            {
                return GetProperty<IList<UserProxy>>("User");
            }
        }
        public string ValueType
        {
            get
            {
                return GetProperty<string>("ValueType");
            }
        }
    }
}
