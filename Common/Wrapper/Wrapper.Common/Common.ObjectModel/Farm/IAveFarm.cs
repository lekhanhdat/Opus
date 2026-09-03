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
using System.Text;
using System.Collections;
using System.Security;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFarm : IAvePersistedUpgradableObject
    {
        IAveAlternateUrlCollectionManager AlternateUrlCollections { get; }
        IAveTimerService TimerService { get; }
        IAveServiceCollection Services { get; }
        IAveServerCollection Servers { get; }
        IAveSolutionCollection Solutions { get; }
        IAveFeatureDefinitionCollection FeatureDefinitions { get; }
        IAveFarm Local { get; }
        Version BuildVersion { get; }
        string PasswordChangeEmailAddress { get; set; }
        int PasswordChangeGuardTime { get; set; }
        int PasswordChangeMaximumTries { get; set; }
        int DaysBeforePasswordExpirationToSendEmail { get; set; }
        IAveServiceProxyCollection ServiceProxies { get; }
        bool CEIPEnabled { get; set; }
        Guid ExternalBinaryStoreClassId { get; set; }

        void ValidateLogonAccount(ref string username, SecureString password);
        void Unjoin();
        IAvePersistedObject GetObject(Guid id);
        IAvePersistedObject GetObject(string name, Guid parentId, Type type);
        IAveWebTemplateCollection GetWebTemplates(uint LCID);
    }
}
