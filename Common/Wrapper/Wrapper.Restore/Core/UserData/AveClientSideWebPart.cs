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
using Newtonsoft.Json.Linq;
using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Restore.Core
{
    public class AveClientSideWebPart
    {
        private IPageWebPart mWebPart;
        private string htmlPropertiesData;
        private JObject properties;

        public Guid InstanceId { get { return mWebPart.InstanceId; } }

        public string WebPartId { get { return mWebPart.WebPartId; } }

        public JObject Properties { get { return properties; } }

        public JObject DynamicDataPaths { get { return JObject.Parse(mWebPart.DynamicDataPaths.ToString()); } }

        public JObject ServerProcessedContent { get { return JObject.Parse(mWebPart.ServerProcessedContent.ToString()); } }

        public string HtmlPropertiesData { get { return htmlPropertiesData; } }

        public string PropertiesJson { get { return mWebPart.PropertiesJson; } set { mWebPart.PropertiesJson = value; } }

        public AveClientSideWebPart(IPageWebPart webPart)
        {
            mWebPart = webPart;
            htmlPropertiesData = mWebPart.HtmlPropertiesData;
            properties = JObject.Parse(mWebPart.Properties.ToString());
        }
    }
}