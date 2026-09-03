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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOHold : IAveOHold
    {
        #region IAveOHold Members

        public void SetHold(IAveListItem item, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public void ProvisionWeb(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public void ProvisionList(IAveList list)
        {
            throw new NotImplementedException();
        }

        public IAveList GetHoldsList(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public void SetSiteLockProperty(IAveSite site)
        {
            throw new NotImplementedException();
        }

        public List<IAveListItem> GetHolds(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool IsItemOnHold(IAveListItem item)
        {
            throw new NotImplementedException();
        }

        public bool SetHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public bool RemoveHold(IAveListItemCollection items, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public void RemoveHold(IAveListItem item, IAveListItem hold, string comments)
        {
            throw new NotImplementedException();
        }

        public void RegisterCustomHoldProcessor(string strAssembly, string strClass, IAveWebApplication webApp)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterCustomHoldProcessor(IAveWebApplication webApp)
        {
            throw new NotImplementedException();
        }

        public void RemoveHold(int holdID, IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public bool IsHoldEnabled(IAveList list)
        {
            throw new NotImplementedException();
        }

        public IAveListItem GetHold(IAveWeb web, int holdID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
