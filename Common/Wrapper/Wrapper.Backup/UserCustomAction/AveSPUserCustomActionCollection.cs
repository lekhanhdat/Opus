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
namespace AvePoint.Wrapper.Backup
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;
    using System.Reflection;
    public abstract class AveSPUserCustomActionCollection
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public abstract IAveUserCustomActionCollection UserCustomActions { get; }

        public List<AveUserCustomActionInfo> GetUserCustomActionInfos()
        {
            List<AveUserCustomActionInfo> infos = new List<AveUserCustomActionInfo>();
            if (UserCustomActions != null)
            {
                log.Info("Start to Get UserCustomActionInfos...");
                foreach (IAveUserCustomAction ca in UserCustomActions)
                {
                    infos.Add(AssemblyUserCustomActionInfo(ca));
                    OutputUserCustomAction(ca);
                }
            }
            return infos;
        }

        private void OutputUserCustomAction(IAveUserCustomAction customAction)
        {
            try
            {
                log.Info($"[OutputUserCustomAction]_Id:{customAction.Id}_Name:{customAction.Name}_Location:{customAction.Location}_Description:{customAction.Description}_Group:{customAction.Group}_ImageUrl:{customAction.ImageUrl}_ScriptBlock:{customAction.ScriptBlock}_ScriptSrc:{customAction.ScriptSrc}_Sequence:{customAction.Sequence}_Title:{customAction.Title}_Url:{customAction.Url}");
            }
            catch (System.Exception e)
            {
                log.Error("[OutputUserCustomAction]:{0} failed due to {1}", customAction.Id,e);
            }
        }

        private AveUserCustomActionInfo AssemblyUserCustomActionInfo(IAveUserCustomAction ca)
        {
            AveUserCustomActionInfo info = new AveUserCustomActionInfo
            {
                Id=ca.Id,
                Name = ca.Name,
                Location = ca.Location,
                RegistrationId = ca.RegistrationId,
                RegistrationType = ca.RegistrationType,
                Scope=ca.Scope,
                ClientSideComponentId=ca.ClientSideComponentId,
                ClientSideComponentProperties=ca.ClientSideComponentProperties,
                CommandUIExtension = ca.CommandUIExtension,
                Description = ca.Description,
                Group = ca.Group,
                ImageUrl = ca.ImageUrl,
                Rights = ca.Rights,
                ScriptBlock = ca.ScriptBlock,
                ScriptSrc = ca.ScriptSrc,
                Sequence = ca.Sequence,
                Title = ca.Title,
                Url = ca.Url,
                VersionOfUserCustomAction=ca.VersionOfUserCustomAction
            };
            return info;
        }
    }

    public class AveSPSiteUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveSite site;
        public AveSPSiteUserCustomActionCollection(AveSPSite backupSite)
        {
            site = backupSite.SPSite;
        }
        public override IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return site.UserCustomActions;
            }
        }
    }

    public class AveSPWebUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveWeb web;
        public AveSPWebUserCustomActionCollection(AveSPWeb backupWeb)
        {
            web = backupWeb.SPWeb;
        }
        public override IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
               return web.UserCustomActions;
            }
        }
    }

    public class AveSPListUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveList list;
        public AveSPListUserCustomActionCollection(AveSPList backupList)
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
