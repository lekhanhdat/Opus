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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Diagnostics;
using System.ComponentModel;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOULS : IAveOULS
    {
        private static string ULS_FullName = "Microsoft.Office.Server.Diagnostics.ULS";
        private static string ULSCatBase_FullName = "Microsoft.Office.Server.Diagnostics.ULSCatBase";
        private static string ULSTraceLevel_FullName = "Microsoft.Office.Server.Diagnostics.ULSTraceLevel";
        public AveOULS()
        { }

        public Guid CorrelationGet()
        {
            return (Guid)AveAssemblyUtility.InvokeStaticMethod((typeof(PortalLog)).Assembly.GetType(ULS_FullName), "CorrelationGet", null);
        }

        #region IAveOULS Members

        public void SendTraceTag(uint tagID, IAveOULSCatBase categoryID, AveULSTraceLevel level, string output, params object[] data)
        {
            EnumConverter EC = new EnumConverter((typeof(PortalLog)).Assembly.GetType(ULSTraceLevel_FullName));
            AveAssemblyUtility.InvokeStaticMethod(ULS_FullName, "SendTraceTag", new Type[] { typeof(uint), (typeof(PortalLog)).Assembly.GetType(ULSCatBase_FullName), (typeof(PortalLog)).Assembly.GetType(ULSTraceLevel_FullName),typeof(string),typeof(object[]) }, new object[] { tagID, (categoryID as AveOULSCatBase).ULSCatBase, EC.ConvertFrom(level.ToString()), output, data});
        }

        #endregion
    }
}
