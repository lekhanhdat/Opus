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
    public class AveFeatureInfo:IComparable
    {

        public string FeatureDefinitionName;
        public string SolutionName;
        public Guid FeatureDefinitionId;
        public Guid SolutionId;
        public bool IsFromSandBoxSolution;
        public int CompatibilityLevel;
        private string mFeatureSource;

        public Guid Id;

        //the total number of activation dependencies
        //in other words, how many features depend on this feature.
        //public int Dependencies;

        public AveFeatureScope Scope;

        public List<Guid> Dependencies = new List<Guid>();

        public AveFeatureInfo()
        {
        }

        public AveFeatureInfo(Guid _id, AveFeatureScope _scope)
        {
            Id = _id;
            //Dependencies = _count;
            Scope = _scope;
        }

        public string FeatureSource
        {
            set
            {
                mFeatureSource = value;
            }
            get
            {
                return mFeatureSource;
            }
        }

        public int CompareTo(object obj)
        {
            AveFeatureInfo other = obj as AveFeatureInfo;
            if (other == null)
            {
                return 0;
            }
            return (other.Dependencies.Count - Dependencies.Count);
        }
    }

    // Save featureGuid  and  featureSource
    public class AveFeatureLevel
    {
        private string m_FeatureSource;
        private Guid m_FeatureGuid;

        public AveFeatureLevel(string featureSouce, Guid featureGuid)
        {
            this.m_FeatureSource = featureSouce;
            this.m_FeatureGuid = featureGuid;
        }

        public string FeatureSource
        {
            get
            {
                return m_FeatureSource;
            }
        }
        public Guid FeatureGuid
        {
            get
            {
                return m_FeatureGuid;
            }
        }
    }

    public class AveFeatureInfoBox
    {
        public List<AveFeatureInfo> FeatureList = new List<AveFeatureInfo>();
        public AveFeatureScope Scope;

        public AveFeatureInfoBox()
        { 
        }
    }

}
