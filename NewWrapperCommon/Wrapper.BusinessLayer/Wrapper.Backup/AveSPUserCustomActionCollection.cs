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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.Wrapper.Backup
{
    public abstract class AveSPUserCustomActionCollection
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public abstract IAveUserCustomActionCollection UserCustomActions { get; }

        public List<AveUserCustomActionInfo> GetUserCustomActionInfos()
        {
            List<AveUserCustomActionInfo> infos = new List<AveUserCustomActionInfo>();
            if (UserCustomActions != null)
            {
                foreach (IAveUserCustomAction ca in UserCustomActions)
                {
                    infos.Add(AssemblyBackupInfo(ca));
                }
            }
            return infos;
        }

        private AveUserCustomActionInfo AssemblyBackupInfo(IAveUserCustomAction ca)
        {
            AveUserCustomActionInfo info = new AveUserCustomActionInfo();
            info.Id = ca.Id;
            info.CommandUIExtension = ca.CommandUIExtension;
            info.Description = ca.Description;
            info.Group = ca.Group;
            info.ImageUrl = ca.ImageUrl;
            info.Location = ca.Location;
            info.Name = ca.Name;
            info.RegistrationId = ca.RegistrationId;
            info.RegistrationType = ca.RegistrationType;
            info.Rights = ca.Rights;
            info.ScriptBlock = ca.ScriptBlock;
            info.ScriptSrc = ca.ScriptSrc;
            info.Sequence = ca.Sequence;
            info.Title = ca.Title;
            info.Url = ca.Url;
            return info;
        }
    }

    public class AveSPSiteUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveSite site;
        public AveSPSiteUserCustomActionCollection(IAveSPSite backupSite)
        {
            site = backupSite.SPSite;
        }
        public override IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class AveSPWebUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveWeb web;
        public AveSPWebUserCustomActionCollection(IAveSPWeb backupWeb)
        {
            web = backupWeb.SPWeb;
        }
        public override IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class AveSPListUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveList list;
        public AveSPListUserCustomActionCollection(IAveSPList backupList)
        {
            list = backupList.SPList;
        }
        public override IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return list.UserCustomActions;
            }
        }
    }
}
