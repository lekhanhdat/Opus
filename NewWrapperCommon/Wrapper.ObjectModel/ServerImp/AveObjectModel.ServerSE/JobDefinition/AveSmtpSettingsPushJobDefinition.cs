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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveSmtpSettingsPushJobDefinition : AveAdministrationServiceJobDefinition, IAveSmtpSettingsPushJobDefinition
    {
        private const string mSmtpSettingsPushJobDefinition_Type = "Microsoft.SharePoint.Administration.SPSmtpSettingsPushJobDefinition";
        private object mSmtpSettingsPushJobDefinition;

        public AveSmtpSettingsPushJobDefinition()
            : this(GetSmtpSettingsPushJobDefinition())
        { }

        public AveSmtpSettingsPushJobDefinition(string name, IAveService service)
            : this(GetSmtpSettingsPushJobDefinition())
        { }

        public AveSmtpSettingsPushJobDefinition(object smtpSettingsPushJobDefinition)
            : base(smtpSettingsPushJobDefinition as SPAdministrationServiceJobDefinition)
        {
            mSmtpSettingsPushJobDefinition = smtpSettingsPushJobDefinition;
        }

        private static object GetSmtpSettingsPushJobDefinition()
        {
            return AveAssemblyUtility.CreateInstance(mSmtpSettingsPushJobDefinition_Type, new Type[] { }, new object[] { });
        }
    }
}
