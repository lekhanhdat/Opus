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




namespace AvePoint.Media.ClassicStorage
{
    using global::Storage;
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    #endregion

    public abstract class XObject : XLibrary,IEnumerable, IDisposable
    {
        private List<XObject> mFactoryObjects = new List<XObject>();

        protected List<XObject> FactoryObjects { get { return mFactoryObjects; } }
        
        #region IDisposable

        public virtual void Dispose()
        {
            foreach (XObject xo in FactoryObjects)
            {
                xo.Dispose();
            }
            FactoryObjects.RemoveAll(delegate(XObject obj) { return true; });
            //GC.SuppressFinalize(this);
            Close();
        }

        public virtual void Close() { }

        #endregion

        #region IEnumerable

        public IEnumerator GetEnumerator()
        {
            return FactoryObjects.GetEnumerator();
        }

        #endregion
    }
}
