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
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
namespace AvePoint.ObjectModel.Common
{
    class AveFeatureCollection : AveAbstractCommonCollection<IAveFeature>, IAveFeatureCollection
    {
        private IAveRequest mRequest;
        private AveWeb mWeb;
        private string mFeatureSource;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFeatureCollection));

        public AveFeatureCollection(AveWeb web, IAveRequest request, Dictionary<string, object> featureColProperties, string featureSource)
        {
            mWeb = web;
            mRequest = request;
            mFeatureSource = featureSource;
            base.DataCache.AddPropertyies(featureColProperties);
            InitFeatureCollection();
        }

        internal void InitFeatureCollection()
        {
            var featurePropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveFeature>(featurePropertiesList.Count);

            foreach (var featureProperties in featurePropertiesList)
            {
                AveFeature f = new AveFeature(mRequest, featureProperties);
                mListData.Add(f);
            }
        }

        public IAveFeature this[Guid featureId]
        {
            get
            {
                return GetById(featureId);
            }
        }
        public void Add(IAveFeature feature)
        {
            this.mListData.Add(feature);
        }
        public IAveFeature Add(Guid featureid)
        {
            return this.Add(featureid, false);
        }
        public IAveFeature Add(Guid featureid, bool force)
        {
            return this.Add(featureid, force, AveFeatureDefinitionScope.Farm);
        }
        public IAveFeature Add(Guid featureId, bool force, AveFeatureDefinitionScope featdefScope)
        {
            Dictionary<string, object> featureProperties = this.mRequest.AddFeature(mWeb == null ? null : mWeb.ServerRelativeUrl, featureId, force, (int)featdefScope, mFeatureSource);
            AveFeature feature = new AveFeature(mRequest, featureProperties);
            mListData.Add(feature);
            return feature;
        }
        public IAveFeature GetById(Guid featureId)
        {
            return mListData.Find(f => f.DefinitionId.Equals(featureId));
        }
        public void Remove(Guid featureId, bool force)
        {
            try
            {
                IAveFeature feature = this.GetById(featureId);
                if (feature != null)
                {
                    this.mRequest.DeleteFeature(this.mWeb == null ? null : this.mWeb.ServerRelativeUrl, featureId, force, mFeatureSource);
                    mListData.Remove(this.GetById(featureId));
                }
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.RemoveFeatureErrorForce, featureId, this.mWeb?.Url, e.ToString());
                throw;
                //Log
            }
        }
        public void Remove(Guid featureId)
        {
            try
            {
                this.Remove(featureId, false);
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.RemoveFeatureError, featureId,this.mWeb.Url, e.ToString());
                throw;
                //Log
            }
        }

        #region IAveFeatureCollection Members


        //public AveFeatureInfoBox Export(AveFeatureScope scope)
        //{
        //    AveFeatureInfoBox featureBox = new AveFeatureInfoBox();
        //    foreach (AveFeature feature in this)
        //    {
        //        AveFeatureInfo info = new AveFeatureInfo();
        //        info.Id = feature.DefinitionId;
        //        info.Scope = scope;
        //        //if (feature.Definition != null && feature.Definition.ActivationDependencies != null)
        //        //{
        //        //    foreach (AveFeatureDependency depFeature in feature.Definition.ActivationDependencies)
        //        //    {
        //        //        info.Dependencies.Add(depFeature.FeatureId);
        //        //    }
        //        //}
        //        featureBox.FeatureList.Add(info);
        //    }
        //    featureBox.FeatureList.Sort();
        //    return featureBox;
        //}

        public void RestoreFeatures(List<AveFeatureInfo> featureInfoList)
        {
            List<Dictionary<string, object>> featureInfoListProperties = new List<Dictionary<string, object>>();
            foreach (AveFeatureInfo featureInfo in featureInfoList)
            {
                Dictionary<string, object> featureInfoProperties = new Dictionary<string, object>();
                featureInfoProperties.Add("ID", featureInfo.Id);
                featureInfoProperties.Add("Dependences", featureInfo.Dependencies);
                featureInfoListProperties.Add(featureInfoProperties);
                //开启web publish feature会自动勾选Navigation的Show pages,在这里手动赋值，可以减少不必要的获取web信息的通信次数
                if (featureInfo.Id.ToString().Equals("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb") && !mWeb.AllProperties.ContainsKey("__GlobalNavigationIncludeTypes") && !mWeb.AllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
                {
                    mWeb.AllProperties["__GlobalNavigationIncludeTypes"] = 2;
                    mWeb.AllProperties["__CurrentNavigationIncludeTypes"] = 2;
                }
            }
            List<Dictionary<string, object>> featuresProperties = mRequest.RestoreFeatures(mWeb == null ? null : mWeb.ServerRelativeUrl, true, (int)AveFeatureDefinitionScope.Farm, mFeatureSource, featureInfoListProperties);
            foreach (Dictionary<string, object> featureProperties in featuresProperties)
            {
                string id = featureProperties["DefinitionId"].ToString();
                if (!string.IsNullOrEmpty(id) && id.Equals("f6924d36-2fa8-4f0b-b16d-06b7250180fa"))
                {
                    (mWeb?.Site as AveSite)?.DataCache.AddProperty("IsPublish",true);
                }
                else if (!string.IsNullOrEmpty(id) && id.Equals("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb"))
                {
                    (mWeb as AveWeb)?.DataCache.AddProperty("IsPublish",true);
                }
                AveFeature feature = new AveFeature(mRequest, featureProperties);
                mListData.Add(feature);
            }
        }
        #endregion
    }
}
