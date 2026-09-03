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
namespace AvePoint.Wrapper.Common
{
    using System;

    internal static class IdcrlMessageConstants
    {
        public const string AdfsAuthMessage = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n    <s:Header>\r\n        <wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n        <wsa:To s:mustUnderstand=\"1\">{0}</wsa:To>\r\n        <wsa:MessageID>{1}</wsa:MessageID>\r\n        <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\">\r\n            <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n            <ps:BinaryVersion>6</ps:BinaryVersion>\r\n            <ps:UIVersion>1</ps:UIVersion>\r\n            <ps:Cookies></ps:Cookies>\r\n            <ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams>\r\n        </ps:AuthInfo>\r\n        <wsse:Security>\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{2}</wsse:Username>\r\n                <wsse:Password>{3}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{4}</wsu:Created>\r\n                <wsu:Expires>{5}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n        </wsse:Security>\r\n    </s:Header>\r\n    <s:Body>\r\n        <wst:RequestSecurityToken Id=\"RST0\">\r\n            <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n            <wsp:AppliesTo>\r\n                <wsa:EndpointReference>\r\n                    <wsa:Address>{6}</wsa:Address>\r\n                </wsa:EndpointReference>\r\n            </wsp:AppliesTo>\r\n            <wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType>\r\n        </wst:RequestSecurityToken>\r\n    </s:Body>\r\n</s:Envelope>";
        public const string AssertionFullName = "{urn:oasis:names:tc:SAML:1.0:assertion}Assertion";
        public const string AuthMessage = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<S:Envelope xmlns:S=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n  <S:Header>\r\n    <wsa:Action S:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n    <wsa:To S:mustUnderstand=\"1\">{0}</wsa:To>\r\n    <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/LiveID/SoapServices/v1\" Id=\"PPAuthInfo\">\r\n      <ps:BinaryVersion>5</ps:BinaryVersion>\r\n      <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n    </ps:AuthInfo>\r\n    <wsse:Security>{1}</wsse:Security>\r\n  </S:Header>\r\n  <S:Body>\r\n    <wst:RequestSecurityToken xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" Id=\"RST0\">\r\n      <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n      <wsp:AppliesTo>\r\n        <wsa:EndpointReference>\r\n          <wsa:Address>{2}</wsa:Address>\r\n        </wsa:EndpointReference>\r\n      </wsp:AppliesTo>\r\n      {3}\r\n    </wst:RequestSecurityToken>\r\n  </S:Body>\r\n</S:Envelope>\r\n";
        public const string AuthURL = "AuthURL";
        public const string BinarySecurityTokenFullName = "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}BinarySecurityToken";
        public const string BodyElementFullName = "{http://www.w3.org/2003/05/soap-envelope}Body";
        public const string CodeFullName = "{http://www.w3.org/2003/05/soap-envelope}Code";
        public const string DetailFullName = "{http://www.w3.org/2003/05/soap-envelope}Detail";
        public const string DomainName = "DomainName";
        public const string ENTITYID = "ENTITYID";
        public const string EnvelopeElementFullName = "{http://www.w3.org/2003/05/soap-envelope}Envelope";
        public const string FaultFullName = "{http://www.w3.org/2003/05/soap-envelope}Fault";
        public const string Federated = "Federated";
        public const string FederationProvider = "FederationProvider";
        public const string FederationTokenIssuer_Int = "urn:federation:MicrosoftOnline-int";
        public const string FederationTokenIssuer_Prod = "urn:federation:MicrosoftOnline";
        public const string FP = "FP";
        public const string FPDOMAINNAME = "FPDOMAINNAME";
        public const string FPList = "FPList";
        public const string FPListFullUrlFormat = "http://msoid.{0}/FPList.xml";
        public const string FPUrlFullUrlFormat = "http://msoid.{0}/FPUrl.xml";
        public const string GETUSERREALM = "GETUSERREALM";
        public const string GetUserRealmContentType = "application/x-www-form-urlencoded";
        public const string GetUserRealmMessage = "login={0}&xml=1";
        public const string Managed = "Managed";
        public const string NameSpaceType = "NameSpaceType";
        public const string PassportCodeFullName = "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}code";
        public const string PassportErrorFullName = "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error";
        public const string PassportInternalErrorFullName = "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}internalerror";
        public const string PassportNamespace = "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault";
        public const string PassportTextFullName = "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}text";
        public const string PassportValueFullName = "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}value";
        public const string PolicyReferenceFragment = "<wsp:PolicyReference URI=\"{0}\"></wsp:PolicyReference>";
        public const string ReasonFullName = "{http://www.w3.org/2003/05/soap-envelope}Reason";
        public const string RequestedSecurityTokenFullName = "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken";
        public const string RequestSecurityTokenResponseFullName = "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse";
        public const string RST2 = "RST2";
        public const string SamlNamespace = "urn:oasis:names:tc:SAML:1.0:assertion";
        public const string SecurityTokenServiceUrl_Int = "https://login.microsoftonline-int.com/rst2.srf";
        public const string SecurityTokenServiceUrl_Prod = "https://login.microsoftonline.com/rst2.srf";
        public const string SoapContentType = "application/soap+xml; charset=utf-8";
        public const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
        public const string STSAuthURL = "STSAuthURL";
        public const string SubcodeFullName = "{http://www.w3.org/2003/05/soap-envelope}Subcode";
        public const string Success = "Success";
        public const string TextFullName = "{http://www.w3.org/2003/05/soap-envelope}Text";
        public const string TrustNamespace = "http://schemas.xmlsoap.org/ws/2005/02/trust";
        public const string URL = "URL";
        public const string UsernameTokenSecurityFragment = "\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{0}</wsse:Username>\r\n                <wsse:Password>{1}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{2}</wsu:Created>\r\n                <wsu:Expires>{3}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n";
        public const string UserRealmServiceUrl_Int = "https://login.microsoftonline-int.com/GetUserRealm.srf";
        public const string UserRealmServiceUrl_Prod = "https://login.microsoftonline.com/GetUserRealm.srf";
        public const string ValueFullName = "{http://www.w3.org/2003/05/soap-envelope}Value";
        public const string WsSecurityNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    }
}

