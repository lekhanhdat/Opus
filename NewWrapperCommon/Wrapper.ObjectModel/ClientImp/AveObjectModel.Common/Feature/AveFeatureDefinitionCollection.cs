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
    internal class AveFeatureDefinitionCollection : AveAbstractCommonCollection<IAveFeatureDefinition>, IAveFeatureDefinitionCollection
    {
        private IAveRequest mRequest;
        private IAveWeb mWeb;
        private string mFeatureSource;

        public AveFeatureDefinitionCollection(IAveWeb web, IAveRequest request, Dictionary<string, object> featureColProperties, string featureSource)
        {
            mWeb = web;
            mRequest = request;
            mFeatureSource = featureSource;
            base.DataCache.AddPropertyies(featureColProperties);
            InitFeatureCollection();
        }

        internal void InitFeatureCollection()
        {
            List<Dictionary<string, object>> featurePropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveFeatureDefinition>(featurePropertiesList.Count);

            foreach (Dictionary<string, object> featureProperties in featurePropertiesList)
            {
                AveFeatureDefinition f = new AveFeatureDefinition(null, mRequest, featureProperties);
                mListData.Add(f);
            }
        }

        public IAveFeatureDefinition this[string name]
        {
            get
            {
                return mListData.Find(f => f.Name.Equals(name));
            }
        }

        public IAveFeatureDefinition this[Guid id]
        {
            get
            {
                return mListData.Find(f => f.ID.Equals(id));
            }
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId)
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, Guid solutionId, bool force)
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinition Add(string relativePathToFeatureManifest, int compatibilityLevel, Guid solutionId, bool force)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Remove(string relativePathToFeatureManifest)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid id, bool force)
        {
            throw new NotImplementedException();
        }

        public void Remove(string relativePathToFeatureManifest, bool force)
        {
            throw new NotImplementedException();
        }

        public IAveFeatureDefinition Ensure(IAveFeatureDefinition newObj)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>() where T : IAvePersistedObject
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(Guid id) where T : IAvePersistedObject
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string name) where T : IAvePersistedObject
        {
            throw new NotImplementedException();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #region add for SP2013
        public IAveFeatureDefinition this[Guid id, int compatibilityLevel]
        {
            get
            {
                return this[id];
            }
        }

        public IAveFeatureDefinition this[string name, int compatibilityLevel]
        {
            get
            {
                return this[name];
            }
        }

        public void Remove(Guid featureId, int compatibilitLevel)
        {
            Remove(featureId);
        }

        public void Remove(string relativePathToFeatureManifest, int compatibilityLevel)
        {
            Remove(relativePathToFeatureManifest);
        }

        public void Remove(Guid featureId, int compatibilityLevel, bool force)
        {
            Remove(featureId, force);
        }

        public void Remove(string relativePathToFeatureManifest, int compatibilityLevel, bool force)
        {
            Remove(relativePathToFeatureManifest, force);
        }
        #endregion

    }
}
