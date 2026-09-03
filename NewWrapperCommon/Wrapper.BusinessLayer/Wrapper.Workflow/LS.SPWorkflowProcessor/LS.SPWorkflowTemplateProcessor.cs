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

using System.Linq;

namespace LS.SPWorkflowProcessor
{

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;

    public class SPWorkflowTemplateHelper
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
       

        /// <summary>
        /// 解析出template对应的config文件信息，然后取出template file的所有version对应的internal name，然后逐个备份
        /// </summary>
        /// <param name="web"></param>
        /// <param name="template"></param>
        /// <param name="tempName"></param>
        /// <returns></returns>
        internal static List<WFTemplateVersionInfo> GetInternalNameForAllTemplateVersions(IAveWeb web, IAveWorkflowTemplate template, string tempName)
        {
            List<WFTemplateVersionInfo> templateVersions = new List<WFTemplateVersionInfo>();
            Guid noCodeWorkflowLibId = Guid.Empty;
            int cfgFileItemId = -1;
             int cfgFileItemVersion = -1;
            string cfgName = tempName;
 
            if (!GetTemplateInfoFromDeclarativeConfiguration(tempName, out noCodeWorkflowLibId, out cfgFileItemId, out cfgFileItemVersion))
            {
                return templateVersions;
            }
           
            if (Guid.Empty.Equals(noCodeWorkflowLibId) || cfgFileItemId == -1)
            {
                return templateVersions;
            }

            try
            {
                IAveWeb libWeb = template.IsRootPublic ? web.Site.RootWeb : web;
                IAveList list = libWeb.Lists[noCodeWorkflowLibId];
                IAveListItem item = list.GetItemById(cfgFileItemId);
                templateVersions.AddRange(item.Versions.Select(version => new WFTemplateVersionInfo(template, noCodeWorkflowLibId, cfgFileItemId, version.VersionId, version.IsCurrentVersion)));
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting name of all versions. NoCodeWorkflowLibId: {0}, CfgFileItemId: {1}, Error: {2}",noCodeWorkflowLibId,cfgFileItemId,e);
            }
            //根据version 从小到大
            templateVersions.Reverse();
            return templateVersions;
        }

        public static void DeleteTemplateFiles(IAveWorkflowTemplate template, IAveWeb parentWeb)
        {
            string internalName = (string)template["DeclarativeConfiguration"];
            if (string.IsNullOrEmpty(internalName))
            {
                return;
            }
            var tempWeb = parentWeb;
            if (template.IsRootPublic)
            {
                tempWeb = parentWeb.Site.RootWeb;
            }
            Guid noCodeWorkflowLibId = Guid.Empty;
            int cfgFileItemId = -1;
            int cfgFileItemVersion = -1;
            if (SPWorkflowTemplateHelper.GetTemplateInfoFromDeclarativeConfiguration(internalName, out noCodeWorkflowLibId, out cfgFileItemId, out cfgFileItemVersion))
            {
                try
                {
                    IAveList list = tempWeb.Lists.GetById(noCodeWorkflowLibId);
                    IAveListItem item = list.GetItemById(cfgFileItemId);
                    if(item.File!=null)
                    {
                        IAveFolder parentFolder=item.File.ParentFolder;
                        parentFolder.Delete();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while deleting template folder. DeclarativeConfiguration: {0}, Error:{1}", internalName, e);
                }

            }

        }

        internal static bool GetTemplateInfoFromDeclarativeConfiguration(string cfgName, out Guid noCodeWorkflowLibId, out int cfgFileItemId, out int cfgFileItemVersion)
        {
            bool result = false;
            noCodeWorkflowLibId = Guid.Empty;
            cfgFileItemId = -1;
            cfgFileItemVersion = -1;

            if (cfgName == null
               || !cfgName.StartsWith("<cfg.", StringComparison.OrdinalIgnoreCase)
               || !cfgName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            try
            {
                cfgName = cfgName.Substring(1, cfgName.Length - 2);
                string[] splitedCfgName = cfgName.Split('.');
                noCodeWorkflowLibId = new Guid(splitedCfgName[1].Replace('_', '-'));
                cfgFileItemId = int.Parse(splitedCfgName[2]);
                cfgFileItemVersion = int.Parse(splitedCfgName[3]);
                result = true;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting template file info from internal name. Name: {0}, Error: {1}", cfgName, e);
                result = false;
            }
            return result;
        }
    }

    internal class WFTemplateVersionInfo
    {
        private const string internalNameFormat = "{0}\n<Xoml.{1}.{2}.{3}.-1.0.dll>\n<Cfg.{1}.{2}.{3}.>";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template">throw argument null exception if template is null</param>
        /// <param name="noCodeWorkflowLibId"></param>
        /// <param name="cfgFileItemId"></param>
        /// <param name="cfgFileItemVersion"></param>
        /// <param name="isCurrent"></param>
        public WFTemplateVersionInfo(IAveWorkflowTemplate template, Guid noCodeWorkflowLibId, int cfgFileItemId, int cfgFileItemVersion, bool isCurrent)
        {
            if (template == null)
            {
                throw new ArgumentNullException("Template is null");
            }
            BaseId = template.BaseId;
            IsCurrent = isCurrent;
            TemplateName = template.Name;
            CfgFileItemVersion = cfgFileItemVersion;
            CfgFileItemId = cfgFileItemId;
            NoCodeWorkflowLibId = noCodeWorkflowLibId;
        }

        internal string TemplateName { get; private set; }

        internal Guid BaseId { get; private set; }

        internal Guid NoCodeWorkflowLibId { get; private set; }

        internal int CfgFileItemId { get; private set; }

        internal int CfgFileItemVersion { get; private set; }

        internal bool IsCurrent { get; private set; }

        public override string ToString()
        {
            return string.Format(internalNameFormat, TemplateName, NoCodeWorkflowLibId, CfgFileItemId, CfgFileItemVersion);
        }
    }
}
