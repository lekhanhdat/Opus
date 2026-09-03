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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
{
    public class WebApplicationLevel: IndividualBase
    {
        public WebApplicationLevel(AveObjectModelFactory objectModel)
            : base(objectModel, string.Empty, string.Empty)
        {

        }

        public IAveWebApplication GetWebApplication(string url)
        {
            return ObjectModel.CreateWebApplication(url);
        }
        public List<SPTreeNodeDto> GetWebApplications(bool includeCAWebApp, int startIndex, uint perPage, ref int childrenCount)
        {
            List<SPTreeNodeDto> webApps = new List<SPTreeNodeDto>();
            List<SPTreeNodeDto> pagingWebApps = new List<SPTreeNodeDto>();
            IAveWebService webService = ObjectModel.CreateWebService();
            foreach (IAveWebApplication webApp in webService.ContentService.WebApplications)
            {
                webApps.Add(ConvertToDto(webApp));
            }
            if (includeCAWebApp)
            {
                IAveAdministrationWebApplication CAWebApp = ObjectModel.CreateAdministrationWebApplication();
                webApps.Add(ConvertToDto(CAWebApp));
            }
            webApps.Sort(new SPTreeNodeDtoComparer(true));
            childrenCount = webApps.Count;
            if (perPage >= childrenCount)//all nodes can return in one page
            {
                pagingWebApps.AddRange(webApps);
            }
            else
            {
                int _index = 0;
                int pagingCount = 0;
                if (startIndex > childrenCount)
                {
                    startIndex = 0;
                }
                if (childrenCount - startIndex < perPage)
                {
                    pagingCount = childrenCount - startIndex;
                }
                else
                {
                    pagingCount = (int)perPage;
                }
                try
                {
                    while (_index < pagingCount)
                    {
                        pagingWebApps.Add(webApps[startIndex + _index]);
                        _index++;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"error occured when GetWebApplications,error:{e}");
                }
            }
            return pagingWebApps;
        }

        public List<SPTreeNodeDto> GetContentDBs(string webAppUrl)
        {
            List<SPTreeNodeDto> contentDBs = new List<SPTreeNodeDto>();
            IAveWebApplication webApplication = ObjectModel.CreateWebApplication();
            webApplication = webApplication.Lookup(new Uri(webAppUrl));
            foreach (IAveContentDatabase contentDatabase in webApplication.ContentDatabases)
            {
                SPTreeNodeDto dto = new SPTreeNodeDto();
                dto.Name = contentDatabase.Name;
                dto.FullPath = contentDatabase.Name;
                dto.SPObjectId = contentDatabase.ID.ToString();
                dto.DisplayName = contentDatabase.Name;
                dto.FarmID = FarmId;
                dto.Level = NodeLevel.ContentDB;
                /*
                dto.NodeExtension.ContentDB = new ContentDB();
                dto.NodeExtension.ContentDB.ID = contentDatabase.ID.ToString();
                dto.NodeExtension.ContentDB.Name = contentDatabase.Name;
                dto.Level = NodeLevel.ContentDB;
                 */
                contentDBs.Add(dto);
            }
            return contentDBs;
        }

        public SPTreeNodeDto GetContentDB(IAveWebApplication webApp, string contentDBName)
        {
            foreach (IAveContentDatabase contentDatabase in webApp.ContentDatabases)
            {
                SPTreeNodeDto dto = new SPTreeNodeDto();
                if (contentDatabase.Name.Equals(contentDBName, StringComparison.OrdinalIgnoreCase))
                {
                    dto.SPObjectId = contentDatabase.ID.ToString();
                    dto.FarmID = FarmId;
                    dto.Level = NodeLevel.ContentDB;
                    dto.Name = contentDatabase.Name;
                    return dto;
                }
            }
            throw new Exception(string.Format("Cannot get the content database."));
        }

        private SPTreeNodeDto ConvertToDto(IAveWebApplication webApp)
        {
            SPTreeNodeDto dto = new SPTreeNodeDto();
            string theUrl = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
            dto.FullPath = theUrl;
            dto.Name = theUrl;
            dto.DisplayName = theUrl;
            dto.SPObjectId = webApp.ID.ToString();
            dto.Level = NodeLevel.WebApplication;
            dto.FarmID = FarmId;
            //dto.IsFba = CheckIsPureFba(webApp);
            dto.NodeExtension = new NodeExtensionDto();
            dto.NodeExtension.Languages = new Languages();
            dto.NodeExtension.Languages.Language = new List<Language>();
            IAveRegionalSettings settings = ObjectModel.CreateRegionalSettings();
            dto.NodeExtension.Languages.Default = settings.GlobalServerLanguage.LCID.ToString();
            foreach (IAveLanguage language in settings.GlobalInstalledLanguages)
            {
                Language temp = new Language();
                temp.DisplayName = language.DisplayName;
                temp.LCID = language.LCID;
                dto.NodeExtension.Languages.Language.Add(temp);
            }
            dto.NodeExtension.ContentDBList = new List<ContentDB>();
            foreach (IAveContentDatabase aveContentDB in webApp.ContentDatabases)
            {
                if (aveContentDB == null)
                {
                    continue;
                }
                ContentDB tempDB = new ContentDB();
                tempDB.ID = aveContentDB.ID.ToString();
                tempDB.Name = aveContentDB.DisplayName;
                dto.NodeExtension.ContentDBList.Add(tempDB);
            }
            dto.NodeExtension.ManagedPathList = new List<ManagedPathDto>();
            foreach (IAvePrefix prefix in webApp.Prefixes)
            {
                ManagedPathDto managedPath = new ManagedPathDto();
                switch (prefix.PrefixType.ToString())
                {
                    case "Explicit":
                        managedPath.Type = ManagedPathType.Explicit;
                        break;
                    case "ExplicitInclusion":
                        managedPath.Type = ManagedPathType.ExplicitInclusion;
                        break;
                    case "Wildcard":
                        managedPath.Type = ManagedPathType.Wildcard;
                        break;
                    case "WildcardInclusion":
                        managedPath.Type = ManagedPathType.WildcardInclusion;
                        break;
                    case "Exclusion":
                        managedPath.Type = ManagedPathType.Exclusion;
                        break;
                    default:
                        break;
                }
                managedPath.Name = prefix.Name;
                dto.NodeExtension.ManagedPathList.Add(managedPath);
            }
            return dto;
        }

       /* public SPTreeNodeDto ConvertToWebApplicationDto(IAveWebApplication webApp)
        {
            return ConvertToDto(webApp);
        }*/

        private class SPTreeNodeDtoComparer : IComparer<SPTreeNodeDto>
        {
            private bool mAsc;

            public SPTreeNodeDtoComparer(bool asc)
            {
                this.mAsc = asc;
            }

            public int Compare(SPTreeNodeDto a, SPTreeNodeDto b)
            {
                string x, y;
                if (mAsc)
                {
                    x = a.Name;
                    y = b.Name;
                }
                else
                {
                    x = b.Name;
                    y = a.Name;
                }
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
