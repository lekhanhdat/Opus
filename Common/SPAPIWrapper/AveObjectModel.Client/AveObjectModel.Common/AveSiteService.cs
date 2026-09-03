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



//using System;
//using System.Diagnostics;
//using System.ComponentModel;
//using System.Xml.Serialization;
//using AveClientRequest.Common;

//namespace AvePoint.ObjectModel.Common
//{
//    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
//    [System.Diagnostics.DebuggerStepThroughAttribute()]
//    [System.ComponentModel.DesignerCategoryAttribute("code")]
//    [System.Web.Services.WebServiceBindingAttribute(Name = "SitesSoap", Namespace = "http://schemas.microsoft.com/sharepoint/soap/")]
//    public partial class AveSiteService : AveSoapHttpClientProtocol
//    {
//        /// <remarks/>
//        public AveSiteService(string url)
//        {
//            this.Url = url;
//        }

//        public new string Url
//        {
//            get
//            {
//                return base.Url;
//            }
//            set
//            {                
//                base.Url = value;
//            }
//        }

//        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/GetSite", RequestNamespace = "http://schemas.microsoft.com/sharepoint/soap/", ResponseNamespace = "http://schemas.microsoft.com/sharepoint/soap/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
//        public string GetSite(string SiteUrl)
//        {
//            object[] results = this.Invoke("GetSite", new object[] {
//                        SiteUrl});
//            return ((string)(results[0]));
//        }

//        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/GetUpdatedFormDigest", RequestNamespace = "http://schemas.microsoft.com/sharepoint/soap/", ResponseNamespace = "http://schemas.microsoft.com/sharepoint/soap/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
//        public string GetUpdatedFormDigest()
//        {
//            object[] results = this.Invoke("GetUpdatedFormDigest", new object[0]);
//            return ((string)(results[0]));
//        }
//    }
//}
