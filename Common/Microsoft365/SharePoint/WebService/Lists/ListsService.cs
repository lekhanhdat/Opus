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
namespace Microsoft365.SharePoint.WebService
{
    using Microsoft365.SharePoint.WebService.Lists;
    using System;
    using System.Xml;

    public class ListsService : ServiceBase
    {

        public ListsService(string webUrl, Func<string> cookieProvider)
            : base(webUrl, cookieProvider)
        {
        }

        protected override string ServiceEndPoint => "/_vti_bin/Lists.asmx";

        public XmlNode GetList(string listTile)
        {
            var request = new GetList
            {
                ListName=listTile
            };
            var response = SoapClient.SendRequest<GetList, GetListResponse>(request);
            return response.GetListResult;
        }

        public bool CheckInFile(string pageUrl, string comment, string checkinType)
        {
            var request = new CheckInFile
            {
                PageUrl = pageUrl,
                Comment = comment,
                CheckinType=checkinType.ToString()
            };
            var response = SoapClient.SendRequest<CheckInFile, CheckInFileResponse>(request);
            return response.CheckInFileResult;
        }

        public XmlNode UpdateList(string listName, XmlNode listProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string listVersion)
        {
            var request = new UpdateList
            {
                ListName = listName,
                ListProperties = listProperties,
                NewFields = newFields,
                UpdateFields = updateFields,
                DeleteFields = deleteFields,
                ListVersion = listVersion
            };
            var response = SoapClient.SendRequest<UpdateList, UpdateListResponse>(request);
            return response.UpdateListResult;
        }

        public XmlNode UpdateContentTypeXmlDocument(string listName, string contentTypeId, XmlNode newDocument)
        {
            var request = new UpdateContentTypeXmlDocument
            {
              ListName=listName,
              ContentTypeId=contentTypeId,
              NewDocument=newDocument
            };
            var response = SoapClient.SendRequest<UpdateContentTypeXmlDocument, UpdateContentTypeXmlDocumentResponse>(request);
            return response.UpdateContentTypeXmlDocumentResult;
        }

        public XmlNode UpdateContentType(string listName, string contentTypeId, XmlNode contentTypeProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string addToView)
        {
            var request = new UpdateContentType
            {
                ListName= listName,
                ContentTypeId=contentTypeId,
                ContentTypeProperties=contentTypeProperties,
                NewFields=newFields,
                UpdateFields=updateFields,
                DeleteFields=deleteFields,
                AddToView=addToView
            };
            var response = SoapClient.SendRequest<UpdateContentType, UpdateContentTypeResponse>(request);
            return response.UpdateContentTypeResult;
        }

        public XmlNode UpdateListItems(string listName, XmlNode updates)
        {
            var request = new UpdateListItems
            {
                ListName = listName,
                Updates=updates
            };
            var response = SoapClient.SendRequest<UpdateListItems, UpdateListItemsResponse>(request);
            return response.UpdateListItemsResult;
        }

    }
}
