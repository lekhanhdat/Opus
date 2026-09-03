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
namespace Microsoft365.SharePoint.WebService.FormsServices
{
    using System.Xml;
    using System.Xml.Serialization;
    using static Microsoft365.Common.SoapClient.NameSpaceConst;

    /// <summary>
    /// need to see if BrowserEnableUserFormTemplateResult should be a complex type
    /// </summary>
    [XmlRoot(ElementName = "BrowserEnableUserFormTemplateResponse", Namespace = nsSharePointForms)]
    public class BrowserEnableUserFormTemplateResponse
    {
        [XmlElement(ElementName = "BrowserEnableUserFormTemplateResult")]
        public MessagesResponse BrowserEnableUserFormTemplateResult { get; set; }
    }

    //<? xml version="1.0" encoding="utf-8"?>
    //<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    //  <soap:Body>
    //    <BrowserEnableUserFormTemplateResponse xmlns = "http://schemas.microsoft.com/office/infopath/2007/formsServices" >
    //      < BrowserEnableUserFormTemplateResult >
    //        < xsd:schema>schema</xsd:schema> xml</BrowserEnableUserFormTemplateResult>
    //     </BrowserEnableUserFormTemplateResponse>
    //  </soap:Body>
    //</soap:Envelope>

    [XmlType(Namespace = nsSharePointForms)]
    public class MessagesResponse
    {
        [XmlArray(Order = 0)]
        public Message[] Messages { get; set; }
    }

}
