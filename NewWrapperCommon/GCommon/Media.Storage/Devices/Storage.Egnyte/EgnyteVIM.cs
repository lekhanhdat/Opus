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

namespace AvePoint.Media.Storage.Egnyte
{
    #region
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Storage.Inner;
    #endregion

    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    [VIM(VIMName.Egnyte, typeof(AvePoint.Media.Storage.Egnyte.EgnyteVIM))]
    class EgnyteVIM : AbstractVIM
    {
        #region VIM Members
        public override IXSystem CreateSystem(String xri, AbstractXSystem parentSystem)
        {
            return new EgnyteSystem(xri, parentSystem);
        }

        public override List<String> GetFeatureXML(Int32 type)
        {
            return EgnyteFeature.Getstances(type).FeatureXMLs;
        }

        public override List<StorageFeature> GetFeatureObj(Int32 type)
        {
            return EgnyteFeature.Getstances(type).FeatureObjs;
        }

        public override List<StorageFeature> GetFeatureObj(Int32 type, String culture)
        {
            return EgnyteFeature.Getstances(type, culture).FeatureObjs;
        }
        #endregion
    }

}