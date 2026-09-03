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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFeatureCollection : ICollection, IEnumerable<IAveFeature>, IEnumerable
    {
        IAveFeature this[Guid featureId] { get; }
        IAveFeature Add(Guid featureid);
        IAveFeature Add(Guid featureid, bool force);
        IAveFeature Add(Guid featureId, bool force, AveFeatureDefinitionScope featdefScope);
        IAveFeature GetById(Guid featureId);
        void Remove(Guid featureId, bool force);
        void Remove(Guid featureId);
        void RestoreFeatures(List<AveFeatureInfo> featureInfoList);

        //AveFeatureInfoBox Export(AveFeatureScope scope);
    }
}
