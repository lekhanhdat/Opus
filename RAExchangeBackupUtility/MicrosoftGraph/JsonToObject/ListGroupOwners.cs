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
using Newtonsoft.Json;

namespace ExchangeUtility.MicrosoftGraph
{
    public class ListGroupOwnersObj
    {
        [JsonProperty("@odata.context")]
        public string OdataContext;

        [JsonProperty("value")]
        public ListGroupOwnersValue[] Value;
    }

    public class ListGroupOwnersValue
    {
        [JsonProperty("@odata.type")]
        public string OdataType;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("businessPhones")]
        public object[] BusinessPhones;

        [JsonProperty("displayName")]
        public string DisplayName;

        [JsonProperty("givenName")]
        public string GivenName;

        [JsonProperty("jobTitle")]
        public object JobTitle;

        [JsonProperty("mail")]
        public string Mail;

        [JsonProperty("mobilePhone")]
        public object MobilePhone;

        [JsonProperty("officeLocation")]
        public object OfficeLocation;

        [JsonProperty("preferredLanguage")]
        public string PreferredLanguage;

        [JsonProperty("surname")]
        public string Surname;

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName;
    }
}
