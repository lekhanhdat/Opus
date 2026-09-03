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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackIdentityInfo
    {
        private string authenticationURL;
        private string authRequestString;
        private string[] endpointPublicURLJosnPath;
        private string storageEndpointType = "object-store";
        private string[] tokenJosnPath;
        private bool enableCDN;
        private string cdnEndpointType;
        private string[] cdnURLJosnPath;
        private string region;
        private string[] regionJosnPath;
        private string storageURL;
        private string cdnURL;
        private string authToken;
        private bool hasAuthentication;
        private string errorMessage;

        public string AuthenticationURL
        {
            set
            {
                this.authenticationURL = value;
            }
            get
            {
                return authenticationURL;
            }
        }

        public string AuthRequestString
        {
            set
            {
                this.authRequestString = value;
            }
            get
            {
                return authRequestString;
            }
        }

        public string StorageEndpointType
        {
            set
            {
                this.storageEndpointType = value;
            }
            get
            {
                return storageEndpointType;
            }
        }

        public string[] EndpointPublicURLJosnPath
        {
            get
            {
                return endpointPublicURLJosnPath;
            }
            set
            {
                endpointPublicURLJosnPath = value;
            }
        }


        public string[] TokenJosnPath
        {
            get { return tokenJosnPath; }
            set { tokenJosnPath = value; }
        }

        public bool EnableCDN
        {
            get { return enableCDN; }
            set { enableCDN = value; }
        }

        public string CDNEndpointType
        {
            set
            {
                this.cdnEndpointType = value;
            }
            get
            {
                return cdnEndpointType;
            }
        }

        public string[] CdnURLJosnPath
        {
            get { return cdnURLJosnPath; }
            set { cdnURLJosnPath = value; }
        }

        public string Region
        {
            get { return region; }
            set { region = value; }
        }

        public string[] RegionJosnPath
        {
            get { return regionJosnPath; }
            set { regionJosnPath = value; }
        }

        public string StorageURL
        {
            get { return storageURL; }
            set { storageURL = value; }
        }

        public string CdnURL
        {
            get { return cdnURL; }
            set { cdnURL = value; }
        }

        public string AuthToken
        {
            get { return authToken; }
            set { authToken = value; }
        }

        public bool HasAuthentication
        {
            get { return hasAuthentication; }
            set { hasAuthentication = value; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
    }
}
