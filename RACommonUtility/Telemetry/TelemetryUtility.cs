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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry
{
    public class TelemetryUtility
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TelemetryUtility));

        public static string ConvertSourceFlag(int flag)
        {
            return ConvertSourceFlag((SourceFlag)flag);
        }

        public static string ConvertSourceFlag(SourceFlag flag)
        {
            switch (flag)
            {
                case SourceFlag.SharePoint:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_SP");
                case SourceFlag.FileSystem:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_FS");
                case SourceFlag.Exchange:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_EXO");
                case SourceFlag.Physical:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_Physical");
                case SourceFlag.SharePointOnPrem:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_SPLocal");
                case SourceFlag.OneDrive:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive");
                case SourceFlag.Box:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_Box");
            }
            Logger.Warn($"Can't find [{flag}] I18N.");
            return string.Empty;
        }
    }
}
