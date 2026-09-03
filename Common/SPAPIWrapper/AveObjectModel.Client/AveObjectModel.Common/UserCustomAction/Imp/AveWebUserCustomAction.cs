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
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    class AveWebUserCustomAction:AveUserCustomAction
    {
        private AveWeb mParentWeb { get; set; }

        public AveWebUserCustomAction(AveWeb web, AveUserCustomActionCollection parent, IAveRequest request, IDictionary<string, object> properties)
           : base(parent, request, properties)
        {
            mParentWeb = web;
        }

        public override void DeleteObject()
        {
            Request.UserCustomAction_Delete(Scope, mParentWeb.ServerRelativeUrl, Guid.Empty, Id);
            base.DeleteObject();
        }

        public override void Update()
        {
            var newProps = Request.UserCustomAction_Update(Scope, mParentWeb.ServerRelativeUrl, Guid.Empty, Id, base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(newProps);
            base.Update();
        }
    }
}
