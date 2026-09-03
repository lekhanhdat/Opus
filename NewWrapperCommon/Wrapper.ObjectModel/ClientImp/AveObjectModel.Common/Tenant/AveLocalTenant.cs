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
///********************************************************************
// *
// *  PROPRIETARY and CONFIDENTIAL
// *
// *  This file is licensed from, and is a trade secret of:
// *
// *                   AvePoint, Inc.
// *                   Harborside Financial Center
// *                   9th Fl.   Plaza Ten
// *                   Jersey City, NJ 07311
// *                   United States of America
// *                   Telephone: +1-800-661-6588
// *                   WWW: www.avepoint.com
// *
// *  Refer to your License Agreement for restrictions on use,
// *  duplication, or disclosure.
// *
// *  RESTRICTED RIGHTS LEGEND
// *
// *  Use, duplication, or disclosure by the Government is
// *  subject to restrictions as set forth in subdivision
// *  (c)(1)(ii) of the Rights in Technical Data and Computer
// *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
// *  FAR 52.227-19 (C) (June 1987).
// *
// *  Copyright © 2001-2015 AvePoint® Inc. All Rights Reserved. 
// *
// *  Unpublished - All rights reserved under the copyright laws of the United States.
// *  $Revision:  $
// *  $Author:  $        
// *  $Date:  $
// */
//using AvePoint.Wrapper.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace AvePoint.ObjectModel.Common
//{
//    public class AveLocalTenant : IAveTenant
//    {
//        string mCAUrl;
//        IAveRequest mRequest;
//        public AveLocalTenant(IAveRequest request, string CAUrl)
//        {
//            this.mRequest = request;
//            this.mCAUrl = CAUrl;
//        }

//        public AveLocalTenant(string CAUrl, AveBPOSAccountInfo userAccountInfo)
//        {
//            
//            AveRequestInterceptor request = new AveRequestInterceptor(CAUrl, userAccountInfo);
//            mRequest = request.Proxy;
//            mCAUrl = CAUrl;
//        }

//        public string CreateSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
//        {
//            return mRequest.AddSite(mCAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
//        }

//        public void DeleteSite(string siteUrl)
//        {
//            mRequest.DeleteSite(mCAUrl, siteUrl);
//        }

//        public string CompatibilityRange
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public bool ExternalServicesEnabled
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public string NoAccessRedirectUrl
//        {
//            get
//            {
//                throw new NotImplementedException();
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public double ResourceQuota
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public double ResourceQuotaAllocated
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public long StorageQuota
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public long StorageQuotaAllocated
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public string SPVersion
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public IAvePrefixCollection Prefixes
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public IAveLanguageCollection InstalledLanguages
//        {
//            get { throw new NotImplementedException(); }
//        }
//        public bool SetAdmin(string url, string admin)
//        {
//            throw new NotImplementedException();
//        }

//        public List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl)
//        {
//            throw new NotImplementedException();
//        }


//        public SiteStatus GetSiteStatus(string siteUrl)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
