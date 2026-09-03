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

namespace LS.SPWorkflowProcessor
{
    class NintexAppProcessor
    {
        private readonly Guid NintexWorkflowAppProductId = new Guid("5d3d5c89-3c4c-4b46-ac2c-86095ea300c7");
        private readonly Guid NintexFormsAppProductId = new Guid("353e0dc9-57f5-40da-ae3f-380cd5385ab9");

        private IAveWeb parentWeb;
        private IAveAppInstance nintexWorkflowAppInstance;
        private IAveAppInstance nintexFormsAppInstance;

        public NintexAppProcessor(IAveWeb web)
        {
            this.parentWeb = web;
        }

        private IAveAppInstance GetOrCreateApp(Guid productId)
        {
            var nintexWorkflowAppInstance = parentWeb.GetAppInstancesByProductId(productId);

            if (nintexWorkflowAppInstance.Count == 0)
            {
                return parentWeb.AppSerializer.SetObjectData(new AveAppPackageInfo { ProductId = productId });
            }

            return nintexWorkflowAppInstance[0];
        }

        public IAveAppInstance GetOrCreateNintexWorkflowApp()
        {
            if (nintexWorkflowAppInstance == null)
            {
                nintexWorkflowAppInstance = GetOrCreateApp(NintexWorkflowAppProductId);
            }
            return nintexWorkflowAppInstance;
        }

        public IAveAppInstance GetOrCreateNintexFormsApp()
        {
            if (nintexFormsAppInstance == null)
            {
                nintexFormsAppInstance = GetOrCreateApp(NintexFormsAppProductId);
            }
            return nintexFormsAppInstance;
        }
    }
}
