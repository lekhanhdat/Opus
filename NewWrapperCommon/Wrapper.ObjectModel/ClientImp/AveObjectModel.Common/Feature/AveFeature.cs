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
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveFeature : AveClientObject, IAveFeature
    {
        private IAveRequest mRequest;

        public AveFeature(IAveRequest request, Dictionary<string, object> featureProperties)
        {
            mRequest = request;
            base.DataCache.AddPropertyies(featureProperties);
        }
        public Guid DefinitionId 
        { 
            get
            {
                return base.DataCache.GetProperty<Guid>("DefinitionId");
            } 
        }
        public IAveFeatureDefinition Definition 
        { 
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Definition") 
                    && base.DataCache.IsPropertyAvailable("Definition" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> featureDefinitonProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Definition" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveFeatureDefinition featureDefinition = new AveFeatureDefinition(this, mRequest, featureDefinitonProperties);
                    base.DataCache.PropertiesCache["Definition"] = featureDefinition;
                    return featureDefinition;
                }
                return base.DataCache.GetProperty<IAveFeatureDefinition>("Definition");
            }
        }

        #region add for SP2013
        public AveFeatureDefinitionScope FeatureDefinitionScope
        {
            get
            {
                return base.DataCache.GetProperty<AveFeatureDefinitionScope>("FeatureDefinitionScope");
            }
        }
        #endregion


        public IAveFeaturePropertyCollection Properties
        {
            get { throw new NotImplementedException(); }
        }

        internal static AveFeatureScope StringToScope(string scope)
        {
            if (scope == "Farm")
            {
                return AveFeatureScope.Farm;
            }
            if (scope == "WebApplication" || scope == "WssWebApplication")
            {
                return AveFeatureScope.WebApplication;
            }
            if (scope == "Site")
            {
                return AveFeatureScope.Site;
            }
            if (scope == "Web")
            {
                return AveFeatureScope.Web;
            }
            throw new ArgumentException("Error scope");
        }
    }
}
