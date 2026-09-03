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

using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using WebApiContact = DocAveOnline.WebApi.Contracts;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Common;

namespace AvePoint.RA.RADataBroker.Common
{
    public static class ConvertUtility
    {
        #region
        public class SPTreeNodeComparer : IEqualityComparer<RMSPTreeNode>
        {
            public bool Equals(RMSPTreeNode x, RMSPTreeNode y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                if ((x == null && y != null) || (x != null && y == null))
                {
                    return false;
                }
                return x.Level == y.Level && x.Name == y.Name;
            }

            public int GetHashCode(RMSPTreeNode obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.Name.GetHashCode();
            }
        }

        public class EXOTreeNodeComparer : IEqualityComparer<RMEXOTreeNode>
        {
            public bool Equals(RMEXOTreeNode x, RMEXOTreeNode y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                if ((x == null && y != null) || (x != null && y == null))
                {
                    return false;
                }
                return x.Level == y.Level && x.Name == y.Name;
            }

            public int GetHashCode(RMEXOTreeNode obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.Name.GetHashCode();
            }
        }
        #endregion
    }

}
