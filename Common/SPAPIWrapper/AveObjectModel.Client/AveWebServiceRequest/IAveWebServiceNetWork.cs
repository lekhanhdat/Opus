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
    using System.Xml;
    using System;
    public interface IAveWebServiceNetWork:IDisposable
    {
        string AssociateWorkflowMarkup(string configUrl, string configVersion);
        void BrowserEnableUserFormTemplate(string formTemplateUrl);
        void CheckInFile(string pageUrl, string comment, int checkinType);
        bool InitialNetWorker(AveWebServiceType type, string netWorkUrl);
        XmlNode ListGetList(string listTile);
        void ListUpdateList(string listGuid, XmlNode listProperties, XmlNode newFields, XmlNode updateFields, XmlNode deleteFields, string listVersion);
        void SetFormsForListItem(int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId);
        XmlNode UpdateContentType(string listName, string ctId, XmlNode node);
        XmlNode UpdateContentTypeXmlDocuments(string listName, string ctId, XmlNode node);
        string GetWebPartPage(string documentName);
        XmlNode GetWebPartProperties2(string documentName);
        XmlNode GetWebPartProperties(string documentName);

        XmlNode UpdateListItems(string listName, XmlNode updates);
    }
}