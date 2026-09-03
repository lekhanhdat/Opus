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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server13
{
    class AveFeatureDefinitionCollection : AvePersistedChildCollection<IAveFeatureDefinition>, IAveFeatureDefinitionCollection
    {
        private SPFeatureDefinitionCollection mFeatureDefinitionCollection;

        public AveFeatureDefinitionCollection(SPFeatureDefinitionCollection featureDefinitionCollection)
            : base(featureDefinitionCollection)
        {
            mFeatureDefinitionCollection = featureDefinitionCollection;
        }

        #region IAveFeatureDefinitionCollection Members

        public override IAveFeatureDefinition this[Guid featureId]
        {
            get
            {
                SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection[featureId];
                if (null != featureDefinition)
                {
                    return new AveFeatureDefinition(featureDefinition);
                }
                return null;
            }
        }

        public override IAveFeatureDefinition this[string featureName]
        {
            get
            {
                SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection[featureName];
                if (null != featureDefinition)
                {
                    return new AveFeatureDefinition(featureDefinition);
                }
                return null;
            }
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId)
        {
            SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection.Add(relativePathToFeatureManifest, solutionId);
            if (featureDefinition == null)
            {
                return null;
            }
            return new AveFeatureDefinition(featureDefinition);
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId, bool force)
        {
            SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection.Add(relativePathToFeatureManifest, solutionId, force);
            if (featureDefinition == null)
            {
                return null;
            }
            return new AveFeatureDefinition(featureDefinition);
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, int compatibilityLevel, Guid solutionId, bool force)
        {
            SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection.Add(relativePathToFeatureManifest,compatibilityLevel, solutionId, force);
            if (featureDefinition == null)
            {
                return null;
            }
            return new AveFeatureDefinition(featureDefinition);
        }

        public void Remove(string relativePathToFeatureManifest)
        {
            mFeatureDefinitionCollection.Remove(relativePathToFeatureManifest);
        }

        public void Remove(Guid featureId, bool force)
        {
            mFeatureDefinitionCollection.Remove(featureId, force);
        }

        public void Remove(string relativePathToFeatureManifest, bool force)
        {
            mFeatureDefinitionCollection.Remove(relativePathToFeatureManifest, force);
        }

        public override void Remove(Guid featureId)
        {
            Remove(featureId, false);
        }

        public override int Count
        {
            get
            {
                return mFeatureDefinitionCollection.Count;
            }
        }

        #endregion

        #region add for SP2013
        public IAveFeatureDefinition this[Guid featureId, int compatibilityLevel]
        {
            get
            {
                SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection[featureId, compatibilityLevel];
                if (null != featureDefinition)
                {
                    return new AveFeatureDefinition(featureDefinition);
                }
                return null;
            }
        }

        public IAveFeatureDefinition this[string featureName, int compatibilityLevel]
        {
            get
            {
                SPFeatureDefinition featureDefinition = mFeatureDefinitionCollection[featureName, compatibilityLevel];
                if (null != featureDefinition)
                {
                    return new AveFeatureDefinition(featureDefinition);
                }
                return null;
            }
        }

        public void Remove(Guid featureId, int compatibilityLevel)
        {
            mFeatureDefinitionCollection.Remove(featureId, compatibilityLevel);
        }

        public void Remove(string relativePathToFeatureManifest, int compatibilityLevel)
        {
            mFeatureDefinitionCollection.Remove(relativePathToFeatureManifest, compatibilityLevel);
        }

        public void Remove(Guid featureId, int compatibilityLevel, bool force)
        {
            mFeatureDefinitionCollection.Remove(featureId, compatibilityLevel, force);
        }

        public void Remove(string relativePathToFeatureManifest, int compatibilityLevel, bool force)
        {
            mFeatureDefinitionCollection.Remove(relativePathToFeatureManifest, compatibilityLevel, force);
        }
        #endregion
    }
}
