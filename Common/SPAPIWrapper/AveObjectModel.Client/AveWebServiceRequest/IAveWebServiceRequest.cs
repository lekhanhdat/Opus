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
namespace AvePoint.ObjectModel.WebService
{
    using AvePoint.Wrapper.Common;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ApiAttribute:Attribute
    {
        public AveWebServiceType WebServiceType { get; set; }
    }
    public interface IAveWebServiceRequestOnline
    {
        bool IsAvaliable { get; }
        /// <summary>
        /// used for publish 2010 mode workflow association
        /// </summary>
        /// <param name="formTemplateUrl"></param>
        [Api(WebServiceType = AveWebServiceType.WebPartPages)]
        string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion);
        /// <summary>
        /// used for publish 2010 mode workflow form file
        /// </summary>
        /// <param name="formTemplateUrl"></param>
        [Api(WebServiceType = AveWebServiceType.FormsServices)]
        void BrowserEnableUserFormTemplate(string formTemplateUrl);
        /// <summary>
        /// SAAS-7142 used for checkin thumbnail files in image gallery
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="pageUrl"></param>
        /// <param name="comment"></param>
        /// <param name="checkinType"></param>
        [Api(WebServiceType = AveWebServiceType.Lists)]
        void CheckInFile(string webUrl, string pageUrl, string comment, int checkinType);
        /// <summary>
        /// Compatible mode to download file content
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="source"></param>
        /// <param name="isSpecialList"></param>
        /// <returns></returns>
        [Api(WebServiceType = AveWebServiceType.HttpWebRequest)]
        Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source, bool isSpecialList = false);
        /// <summary>
        /// Compatible mode to download file content
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="source"></param>
        /// <param name="isSpecialList"></param>
        /// <returns></returns>
        [Api(WebServiceType = AveWebServiceType.HttpWebRequest)]
        Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId);
        /// <summary>
        /// used to get webpart xml,fixup some incorrect property from CSOM api
        /// InPlaceSearchEnabled
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="personalizationScope"></param>
        /// <returns></returns>
        [Api(WebServiceType = AveWebServiceType.WebPartPages)]
        Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope);

        /// <summary>
        /// not sure when this method will be called,
        /// restore theme have another CSOM method normally
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="siteServerRelativeUrl"></param>
        /// <param name="webSettingInfo"></param>
        /// <param name="themedCssFolderUrl"></param>
        [Api(WebServiceType = AveWebServiceType.HttpWebRequest)]
        void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl);
        /// <summary>
        /// restore form files
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="lcid"></param>
        /// <param name="base64FormTemplate"></param>
        /// <param name="applicationId"></param>
        /// <param name="listGuid"></param>
        /// <param name="contentTypeId"></param>
        [Api(WebServiceType = AveWebServiceType.FormsServices)]
        void SetFormForList(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId);

        /// <summary>
        /// survey list will be updated with this method, need to know why if remove this usage.
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="listName"></param>
        /// <param name="listId"></param>
        /// <param name="listProperties"></param>
        /// <returns></returns>
        [Api(WebServiceType = AveWebServiceType.Lists)]
        Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties);
        /// <summary>
        /// calender list item will be updated by this method,
        /// to keep some item properties
        /// </summary>
        /// <param name="webAppName"></param>
        /// <param name="webRelativeUrl"></param>
        /// <param name="listName"></param>
        /// <param name="itemId"></param>
        /// <param name="fileRef"></param>
        /// <param name="itemProp"></param>
        [Api(WebServiceType = AveWebServiceType.Lists)]
        void UpdateListItems(string webAppName, string webRelativeUrl, string listName, int itemId, string fileRef, Dictionary<string, object> itemProp);
        /// <summary>
        /// use for update some special webparts
        /// Microsoft.Office.InfoPath.Server.Controls.WebUI.BrowserFormWebPart
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="newId"></param>
        /// <param name="definitionXml"></param>
        [Api(WebServiceType = AveWebServiceType.WebPartPages)]
        void UpdateBroswerFormWebPartProperty(string webUrl, string webServerRelativeUrl, string fileServerRelativeUrl, Guid newId, string definitionXml);
        /// <summary>
        /// used for update content type xml docments
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="listName"></param>
        /// <param name="listId"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="updateChildren"></param>
        /// <param name="contentTypeSource"></param>
        /// <param name="needUpdateContentTypeProperties"></param>
        /// <param name="supportedResourceCultureNames"></param>
        /// <returns></returns>
        [Api(WebServiceType = AveWebServiceType.Lists)]
        Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames);
    }
}