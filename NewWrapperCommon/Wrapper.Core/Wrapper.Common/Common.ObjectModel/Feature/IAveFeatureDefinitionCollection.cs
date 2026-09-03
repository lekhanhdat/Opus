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
using System.Text;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveFeatureDefinitionCollection : IAvePersistedChildCollection<IAveFeatureDefinition>
    {
        IAveFeatureDefinition this[string name] { get; }
        IAveFeatureDefinition this[Guid id] { get; }

        IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId);
        IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId, bool force);
        IAveFeatureDefinition Add(string relativePathToFeatureManifest, int compatibilityLevel, Guid solutionId, bool force);

        void Remove(Guid id);
        void Remove(string relativePathToFeatureManifest);
        void Remove(Guid id, bool force);
        void Remove(string relativePathToFeatureManifest, bool force);
        void Remove(Guid featureId, int compatibilityLevel);
        void Remove(string relativePathToFeatureManifest, int compatibilityLevel);
        void Remove(Guid featureId, int compatibilityLevel, bool force);
        void Remove(string relativePathToFeatureManifest, int compatibilityLevel, bool force);

        #region add for SP2013
        IAveFeatureDefinition this[Guid id, int compatibilityLevel] { get; }
        IAveFeatureDefinition this[string name, int compatibilityLevel] { get; }
        #endregion
    }
}
