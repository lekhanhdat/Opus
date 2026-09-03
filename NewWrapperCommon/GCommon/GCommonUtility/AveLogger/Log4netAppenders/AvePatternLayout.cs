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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using log4net.Layout;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon
{
    public class AvePatternLayout : PatternLayout
    {
        private string mHeader;
        public override string Header
        {
            get
            {
                return this.mHeader;
            }
            set 
            {
                var deployId = "";
                try
                {
                    var insatllPath = InstallationUtility.GetControlInstallPath();
                    var path = Path.Combine(insatllPath, @"Control\bin\ServiceVersion.config");
                    if (File.Exists(path))
                    {
                        var doc = new XmlDocument();
                        doc.Load(path);
                        var node = doc.SelectSingleNode("/configuration/properties/DeployId");
                        if (node != null && !string.IsNullOrEmpty(node.InnerText)) 
                        {
                            deployId = node.InnerText;
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
                string header = value;
                if (!string.IsNullOrEmpty(deployId))
                {
                    header = string.Format(string.Format("{0}Deploy Id: {1}\r\n", header, deployId));
                }

                CustomerInfo customerInfo = new CustomerInfo();
                try
                {
                    customerInfo = AveCustomerInfoHelper.GetCustomerInfoForLog();
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
                if (customerInfo != null)
                {
                    header = string.Format(string.Format("{0}Account Number: {1}\r\n", header, customerInfo.AccountNumber));
                }
                this.mHeader = header;
            }
        }
    }
}
