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
using System.Security.Principal;
using System.Collections.Specialized;
using System.Security;

namespace AvePoint.Wrapper.Common
{
    public interface IAveManagedAccount : IAvePersistedObject
    {
         bool AutomaticChange { get; set; }
         bool CanChangePassword { get; }
         IAveSchedule ChangeSchedule { get; set; }
         StringCollection ComponentsUsingThisAccount { get; }
         int DaysBeforeChangeToEmail { get; set; }
         int DaysBeforeExpiryToChange { get; set; }
         string DisplayName { get; }
         string Domain { get; }
         bool EnableEmailBeforePasswordChange { get; set; }
         int MinPasswordLen { get; }
         DateTime NextChangeTime { get; }
         IAveGeneratePasswordJobDefinition PasswordChangeJob { get; }
         string PasswordChangeJobName { get; }
         DateTime PasswordExpiration { get; }
         DateTime PasswordLastChanged { get; }
         SecurityIdentifier Sid { get; set; }
         string SplitName { get; }
         string SplitServer { get; }
         bool TimeToNotifyAboutChange { get; }
         bool TimeToNotifyAboutExpiry { get; }
         string TypeName { get; }
         string UPNName { get; }
         int UserAccountControl { get; }
         string Username { get; set; }

         void ChangePassword(SecureString newPassword, AveEventProcessingOptions eventFlags);
         void Update();
         bool SetPassword(SecureString value);
         void GeneratePassword(AveEventProcessingOptions eventFlags);
         void PropagatePassword(SecureString newPassword, AveEventProcessingOptions eventFlags);
         void Delete();
    }
}
