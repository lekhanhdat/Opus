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



namespace AvePoint.ObjectModel.Server19.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.Search.Administration;
    using AvePoint.Wrapper.Common;
    #endregion

    class AveOContentSourceCollection : AveAbstractCommonCollection<IAveOContentSource>, IAveOContentSourceCollection
    {
        private ContentSourceCollection mContentSources;

        public AveOContentSourceCollection(ContentSourceCollection contentSources)
            : base(contentSources)
        {
            mContentSources = contentSources;
        }

        protected override object CreatElementInstance(object t)
        {
            return CreateContentSourceByType(t as ContentSource);
        }

        private Dictionary<string, AveOContentSource> mContentSourceCache = new Dictionary<string, AveOContentSource>();
        private AveOContentSource CreateContentSourceByType(ContentSource cs)
        {
            if (cs != null)
            {
                string sKey = string.Format("{0}:{1}", cs.Name, cs.Id);
                lock (mContentSourceCache)
                {
                    if (!mContentSourceCache.ContainsKey(sKey))
                    {
                        mContentSourceCache[sKey] = AveServerAssemblyInit.CreateElement(typeof(IAveOContentSource), new object[] { cs }) as AveOContentSource;
                    }
                }

                return mContentSourceCache[sKey];
            }

            return null;
        }

        public override int Count
        {
            get
            {
                return mContentSources.Count;
            }
        }

        #region IAveOContentSourceCollection Members

        public IAveOContentSource this[string name]
        {
            get
            {
                return new AveOContentSource(mContentSources[name]);
            }
        }

        public bool Exists(string name)
        {
            return mContentSources.Exists(name);
        }

        public IAveOContentSource Create(Type type, string name)
        {
            Type realType = AveAssemblyUtility.GetGenerticType(type, null);
            return AveServerAssemblyInit.CreateElement(type, new object[] { mContentSources.Create(realType, name) }) as AveOContentSource;
        }

        public bool Exists(int id)
        {
            return mContentSources.Exists(id);
        }

        public new IAveOContentSource this[int id]
        {
            get
            {
                return new AveOContentSource(mContentSources[id]);
            }
        }

        #endregion
    }
}
