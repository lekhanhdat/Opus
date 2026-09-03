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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery
{
    public class RMDiscoveryCustomColumn
    {
        public string Name { get; set; }

        public SqlDbType DBType { get; set; }

        public bool NeedIndex { get; set; }

        public string DBTypeName => DBType.ToString();

        public RMDiscoveryCustomColumn()
        {

        }

        public RMDiscoveryCustomColumn(string name, SqlDbType dbType)
        {
            Name = name;
            DBType = dbType;
        }

        public RMDiscoveryCustomColumn(string name, SqlDbType dbType, bool needIndex)
        {
            Name = name;
            DBType = dbType;
            NeedIndex = needIndex;
        }
    }

    public class RMDiscoveryCustomColumnWithValue
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public Type ValueType { get; set; }

        public RMDiscoveryCustomColumnWithValue()
        {

        }

        public RMDiscoveryCustomColumnWithValue(string name, object value, Type valueType)
        {
            Name = name;
            Value = value;
            ValueType = valueType;
        }
    }
}
