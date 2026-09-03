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


using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [XmlRoot(ElementName = "UserInformation")]
    public class UserInformation
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string UserName { get; set; }
        [XmlAttribute]
        public string Password { get; set; }
        [XmlAttribute]
        public string ConfirmPassword { get; set; }
        [XmlAttribute]
        public string FirstName { get; set; }
        [XmlAttribute]
        public string LastName { get; set; }
        [XmlAttribute]
        public string ServiceName { get; set; }
        [XmlAttribute]
        public string Telephone { get; set; }
        [XmlAttribute]
        public string Organization { get; set; }
        [XmlAttribute]
        public string Address { get; set; }
        [XmlAttribute]
        public string City { get; set; }
        [XmlAttribute]
        public string PostalCode { get; set; }
        [XmlAttribute]
        public string VerificationCode { get; set; }
        [XmlAttribute]
        public string Host { get; set; }
        [XmlAttribute]
        public string Port { get; set; }
        [XmlAttribute]
        public string Schema { get; set; }
        [XmlAttribute]
        public string Email { get; set; }
        [XmlAttribute]
        public string SiteCollectionUrl { get; set; }
        [XmlAttribute]
        public string IsRegisterByApp { get; set; }
        [XmlAttribute]
        public string Country { get; set; }
        [XmlAttribute]
        public string State { get; set; }
        [XmlAttribute]
        public string ContextToken { get; set; }
        [XmlAttribute]
        public string Authority { get; set; }
        [XmlAttribute]
        public string AccountNumber { get; set; }
        [XmlAttribute]
        public string InvoiceNumber { get; set; }
        [XmlAttribute]
        public string AppModule { get; set; }
        [XmlAttribute]
        public string TenantId { get; set; }
        [XmlAttribute]
        public string IsMySite { get; set; }

        public static UserInformation GetDefaultUser()
        {
            return new UserInformation()
            {
                Id = "",
                Password = "",
                Email = "",
                ServiceName = "",
                Schema = "",
                SiteCollectionUrl = "",
                State = "",
                Address = "",
                Authority = "",
                City = "",
                ContextToken = "",
                Country = "",
                FirstName = "",
                Host = "",
                IsRegisterByApp = "",
                LastName = "",
                Organization = "",
                Port = "",
                PostalCode = "",
                Telephone = "",
                UserName = "",
                VerificationCode = "",
                ConfirmPassword = "",
                AppModule = "",
                TenantId = "",
                IsMySite = "",
            };
        }
    }
}
