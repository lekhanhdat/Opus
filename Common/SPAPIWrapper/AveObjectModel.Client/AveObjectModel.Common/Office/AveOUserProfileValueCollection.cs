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




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AvePoint.Wrapper.Common;
//using AvePoint.Wrapper.Common.Office;

//namespace AvePoint.ObjectModel.Common.Office
//{
//    class AveOUserProfileValueCollection :AveClientObject , IAveOUserProfileValueCollection
//    {
//        private IAveRequest mRequest;
//        private AveOUserProfile mProfile;
//        private AveOUserProfileManager mProfileManager;
//        private string mPropName;

//        public AveOUserProfileValueCollection(IAveRequest request,AveOUserProfile profile,AveOUserProfileManager profileManager,string propName, Dictionary<string, object> prop) 
//        {   
//            mRequest = request;
//            mProfile = profile;
//            mProfileManager = profileManager;
//            mPropName = propName;
//            base.DataCache.AddPropertyies(prop);
//        }

//        #region IAveUserProfileValueCollection Members

//        public IAveOProperty Property
//        {
//            get 
//            {
//                if (base.DataCache.IsPropertyNotLoaded("Property"))
//                {
//                    Dictionary<string, object> prop = base.DataCache.GetProperty<Dictionary<string, object>>("Property"+AveObjectModelConstant.ObjectPropertySuffix);
//                    AveOProperty property = new AveOProperty(prop);
//                    base.DataCache.AddProperty("Property",property);
//                }
//                return base.DataCache.GetProperty<IAveOProperty>("Property");
//            }
               
//        }

//        public int Capacity
//        {
//            get
//            {
//                return base.DataCache.GetProperty<int>("Capacity");
//            }
//            set
//            {
//                base.DataCache.AddChangedProperty("Capacity", value);
//            }
//        }

//        public AvePrivacy Privacy
//        {
//            get
//            {
//                return base.DataCache.GetProperty<AvePrivacy>("Privacy");
//            }
//            set
//            {
//                base.DataCache.AddChangedProperty("Privacy", value);
//            }
//        }

//        public object this[int index]
//        {
//            get
//            {
//                if (base.DataCache.IsPropertyAvailable("Value" + "TimeZone" + index.ToString()) && base.DataCache.IsPropertyNotLoaded("Value" + index.ToString()))
//                {
//                    Dictionary<string, object> timeZoneProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Value" + "TimeZone" + index.ToString());
//                    AveTimeZone timeZone = new AveTimeZone(this.mRequest, timeZoneProperties);
//                    base.DataCache.AddProperty("Value" + index.ToString(),timeZone);
//                }
//                return base.DataCache.GetProperty<object>("Value" + index.ToString());
//            }
//        }

//        public void Clear()
//        {
//            throw new NotImplementedException();
//        }

//        public void Add(object o)
//        {
//            throw new NotImplementedException();
//        }

//        public object Value
//        {
//            get
//            {
//                return base.DataCache.GetProperty<object>("Value");
//            }
//            set
//            {
//                base.DataCache.AddChangedProperty("Value",value);
//            }
//        }

//        #endregion


//        #region ICollection Members

//        public void CopyTo(Array array, int index)
//        {
//            throw new NotImplementedException();
//        }    
       
//        public int Count
//        {
//            get
//            {
//                return base.DataCache.GetProperty<int>("Count");
//            }
//        }

//        public bool IsSynchronized
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public object SyncRoot
//        {
//            get { throw new NotImplementedException(); }
//        }

//        #endregion

//        #region IEnumerable Members

//        public System.Collections.IEnumerator GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        #endregion

//        #region IAveOUserProfileValueCollection Members


//        public object ProfileSubtypeProperty
//        {
//            get { throw new NotImplementedException(); }
//        }

//        #endregion
//    }
//}
