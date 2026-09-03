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

namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using System.Collections.Generic;
    using AvePoint.Media.Storage.Inner;
    using AvePoint.Media.Storage.Util;
    using AvePoint.Media.Storage.Cloud.HCP; 
    #endregion

    [VIM(VIMName.HCP, typeof(AvePoint.Media.Storage.HCP.HCPVIM))]
    class HCPVIM : AbstractVIM
    {
        public override IXSystem CreateSystem(string xri, AbstractXSystem parentSystem)
        {
            return new HCPSystem(xri, XSystemConst.MODE_NOW_INITSYSTEM, parentSystem);
        }

        public override List<string> GetFeatureXML(int type)
        {
            return HCPFeature.Getstances(type).FeatureXMLs;
        }

        public override List<StorageFeature> GetFeatureObj(int type)
        {
            return HCPFeature.Getstances(type).FeatureObjs;
        }

        public override List<StorageFeature> GetFeatureObj(int type,string culture)
        {
            return HCPFeature.Getstances(type,culture).FeatureObjs;
        }
    }
}
