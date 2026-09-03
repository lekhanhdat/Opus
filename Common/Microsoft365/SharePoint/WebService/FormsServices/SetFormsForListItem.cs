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
    using Microsoft365.Common.SoapClient;
    using System.Xml;
    using System.Xml.Serialization;
    using static Microsoft365.Common.SoapClient.NameSpaceConst;

    [XmlRoot(Namespace = nsSharePointForms)]
    public class SetFormsForListItem : ISoapHttpRequest
    {
        [XmlElement(ElementName = "lcid")]
        public int Lcid { get; set; }
        [XmlElement(ElementName = "base64FormTemplate")]
        public string Base64FormTemplate { get; set; }
        [XmlElement(ElementName = "applicationId")]
        public string ApplicationId { get; set; }

        [XmlElement(ElementName = "listGuid")]
        public string ListGuid { get; set; }
        [XmlElement(ElementName = "contentTypeId")]
        public string ContentTypeId { get; set; }


        [XmlIgnore]
        public string SoapAction => "http://schemas.microsoft.com/office/infopath/2007/formsServices/SetFormsForListItem";

        public SetFormsForListItem()
        {
        }
    }
}
