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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LS.SPWorkflowProcessor
{
    class NWDataMappingManager : INintexDataMappingManager
    {
        private NWUserMappingManager userMappingManager;
        private AveMappingManager mappingManager;
        private IAveWeb parentWeb;
        private IAveList parentList;
        private ListReferenceCollection listReferences;
        private Dictionary<Guid, string> listTitleCollection;
        private short srcTimeZone;

        public NWDataMappingManager(AveMappingManager mappingManager, IAveSPMembers spMembers, IAveWeb parentWeb, IAveList parentList, ListReferenceCollection listReferences, bool forceEnsureUsersInWorkflow, short srcTimeZone)
        {
            this.userMappingManager = new NWUserMappingManager(spMembers, forceEnsureUsersInWorkflow);
            this.mappingManager = mappingManager;
            this.parentWeb = parentWeb;
            this.listReferences = listReferences;
            this.parentList = parentList;
            this.srcTimeZone = srcTimeZone;
        }


        public Guid GetListIdFromMapping(string srcListId)
        {
            var guidsrcList = new Guid(srcListId);
            return GetListIdFromMapping(guidsrcList);
        }

        public Guid GetListIdFromMapping(Guid srcListId)
        {
            Guid value;
            if (mappingManager.SiteMappingManager.GetValueFromListIdMapping(srcListId, out value))
            {
                return value;
            }
            else if (listReferences != null)
            {
                var list = GetListByListReference(srcListId);
                if (list != null)
                {
                    return list.ID;
                }
            }
            throw new NWListNotFoundException(srcListId.ToString());
        }

        public string GetListTitleFromMapping(string srcListTitle)
        {
            string destListTitle;
            if (!mappingManager.SiteMappingManager.GetValueFromListTitleMappnig(parentWeb.ID, srcListTitle, out destListTitle))
            {
                destListTitle = srcListTitle;
            }
            return destListTitle;
        }

        private IAveList GetListByListReference(Guid sourceId)
        {
            if (listReferences != null)
            {
                var result = listReferences.FirstOrDefault(listReference => sourceId.Equals(listReference.ListId));
                if (result != null)
                {
                    return parentWeb.Lists.GetListByName(result.ListName, false);
                }
            }
            return null;
        }

        public string GetListTitleFromMapping(Guid srcListId)
        {
            Guid destListId;
            if (mappingManager.SiteMappingManager.GetValueFromListIdMapping(srcListId, out destListId))
            {
                var list = parentWeb.Lists.GetListById(destListId, false);
                if (list != null)
                {
                    return list.Title;
                }
            }
            else if (listReferences != null)
            {
                var list = GetListByListReference(srcListId);
                if (list != null)
                {
                    return list.Title;
                }
            }
            throw new NWListNotFoundException(srcListId.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLoginName"></param>
        /// <returns>when result is empty, return source user login name.</returns>
        public string GetMappingLoginName(string userLoginName)
        {
            var mappedName = userMappingManager.GetMappingLoginName(userLoginName);
            if (!string.Equals(mappedName, userLoginName, StringComparison.OrdinalIgnoreCase))
            {
                return mappedName;
            }
            var member = userMappingManager.GetUserByLoginName(userLoginName);
            if (member != null)
            {
                return member.LoginName;
            }
            return userLoginName;
        }

        public bool TryGetValueFromListLevelContentTypeIdMapping(Guid listId, string sourceCTId, out IAveContentTypeId desCTId)
        {
            return mappingManager.SiteMappingManager.TryGetValueFromListLevelContentTypeIdMapping(listId, sourceCTId, out desCTId);
        }

        public string GetListTitle(Guid listId)
        {
            if (listTitleCollection == null)
            {
                listTitleCollection = new Dictionary<Guid, string>();
            }
            if (listTitleCollection.ContainsKey(listId))
            {
                return listTitleCollection[listId];
            }
            else
            {
                string tmpListTitle = parentWeb.GetList(listId).Title;
                listTitleCollection.Add(listId, tmpListTitle);
                return tmpListTitle;
            }
        }


        public string GetContentTypeIdFromDestinationList(IAveList list, string sourceCTId)
        {
            IAveContentTypeId destinationContentTypeId;
            if (mappingManager.SiteMappingManager.TryGetValueFromListLevelContentTypeIdMapping(list.ID, sourceCTId, out destinationContentTypeId))
            {
                return destinationContentTypeId.ToString();
            }
            else if (listReferences != null)
            {
                var listReference = listReferences.FirstOrDefault(lr => lr.ListName.Equals(list.Title, StringComparison.OrdinalIgnoreCase));
                if (listReference != null)
                {
                    var contentTypeName = (from ct in listReference.ContentTypes
                                           where ct.Id.Equals(sourceCTId, StringComparison.OrdinalIgnoreCase)
                                           select ct.Name
                                          ).First();
                    return list.ContentTypes[contentTypeName].ID.ToString();
                }
            }
            throw new NWNeedPostActionException(string.Format("Can not found content type by content type id, source id is {0}, list id: {1}, list title: {2}", sourceCTId, list.ID, list.Title));
        }

        public string MappingUrl(string sourceUrl)
        {
            var siteMappingManager = mappingManager.SiteMappingManager;
            return AveReplaceProcessor.UrlReplace(sourceUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }

        public IAveWeb GetParentWeb()
        {
            return parentWeb;
        }

        public IAveList GetParentList()
        {
            return parentList;
        }

        public string MappingXmlLinks(string sourceXml)
        {
            bool needReplaceLast = true;
            var siteMappingManager = mappingManager.SiteMappingManager;
            return AveReplaceProcessor.ReplaceXmlLinks(sourceXml, mappingManager, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl, parentList, ref needReplaceLast);

        }

        public FieldReference GetFieldFromListReferences(string srcListIdOrListName, string fieldInternalName)
        {
            FieldReference fr = null;
            if (listReferences != null)
            {
                ListReference listReference = null;
                Guid tempGuid = Guid.Empty;
                bool isGuid = true;
                try
                {
                    tempGuid = new Guid(srcListIdOrListName);
                    isGuid = true;
                }
                catch
                {
                    isGuid = false;
                }

                if (isGuid)
                {
                    listReference = listReferences.FirstOrDefault(reference => tempGuid.Equals(reference.ListId));
                }
                else
                {
                    listReference = listReferences.FirstOrDefault(reference => srcListIdOrListName.Equals(reference.ListName, StringComparison.OrdinalIgnoreCase));
                }

                if (listReference != null)
                {
                    fr = listReference.Fields.FirstOrDefault(field => field.InternalName.Equals(fieldInternalName, StringComparison.OrdinalIgnoreCase));
                }
            }
            return fr;
        }

        public string GetListTitleFromListReferences(string srcListId)
        {
            if (listReferences != null)
            {
                var result = listReferences.FirstOrDefault(listReference => listReference != null && listReference.ListId != null && string.Equals(srcListId, listReference.ListId.ToString(), StringComparison.OrdinalIgnoreCase));
                return result != null ? result.ListName : string.Empty;
            }
            return string.Empty;
        }

        public short GetSourceWebTimeZone()
        {
            return srcTimeZone;
        }
    }
}
