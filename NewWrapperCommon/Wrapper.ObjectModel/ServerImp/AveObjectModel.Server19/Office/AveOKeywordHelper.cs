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



using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common.Search;
using System;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using Microsoft.Office.Server.Search.Extended.Administration.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOKeywordHelper :AveOAdminOMHelperBase, IAveOKeywordHelper
    {
        private static string mKeywordHelper_Type = "Microsoft.Office.Server.Search.Extended.Administration.Facade.KeywordHelper";
        private object mKeywordHelper;
        private AveKeywordCollection mKeywordsCollection;
        private AveKeyword mKey;

        public AveOKeywordHelper(string siteID)
            : base(siteID)
        {
            mKeywordHelper = AveAssemblyUtility.CreateInstance(mKeywordHelper_Type, new Type[] { typeof(string) }, new object[] { siteID });
        }

        public AveOKeywordHelper(string siteID, IAveServiceContext serviceContext)
            : base(siteID, serviceContext)
        {
            mKeywordHelper = AveAssemblyUtility.CreateInstance(mKeywordHelper_Type, new Type[] { typeof(string), typeof(SPServiceContext) }, new object[] { siteID, (serviceContext as AveServiceContext).ServiceContext });
        }

        public IAveKeywordCollection KeywordsCollection
        {
            get
            {
                if (mKeywordsCollection == null)
                {
                    object obj = AveAssemblyUtility.GetPropertyValue(mKeywordHelper, "KeywordsCollection");
                    if (obj == null)
                    {
                        return null;
                    }
                    mKeywordsCollection = new AveKeywordCollection((KeywordCollection)obj);
                }
                return mKeywordsCollection;
            }
        }

        public string Title
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mKeywordHelper, "Title");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mKeywordHelper, "Title", value);
            }
        }

        public IAveKeyword Key
        {
            get
            {
                if (mKey == null)
                {
                    object key = AveAssemblyUtility.GetPropertyValue(mKeywordHelper, "Key");
                    if (key != null)
                    {
                        mKey = new AveKeyword((Keyword)key);
                    }
                }
                return mKey;
            }
        }

        public string keywordDefinition
        {
            get
            {
                return (string)AveAssemblyUtility.GetFieldValue(mKeywordHelper, "keywordDefinition");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mKeywordHelper, "keywordDefinition", value);
            }
        }

        public string keywordPhrase
        {
            get
            {
                return (string)AveAssemblyUtility.GetFieldValue(mKeywordHelper, "keywordPhrase");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mKeywordHelper, "keywordPhrase", value);
            }
        }

        public string synonymOneWay
        {
            get
            {
                return (string)AveAssemblyUtility.GetFieldValue(mKeywordHelper, "synonymOneWay");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mKeywordHelper, "synonymOneWay", value);
            }
        }

        public string synonymTwoWay
        {
            get
            {
                return (string)AveAssemblyUtility.GetFieldValue(mKeywordHelper, "synonymTwoWay");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mKeywordHelper, "synonymTwoWay", value);
            }
        }

        public IAveKeyword AddKeyword(string keywordText, AveMode mode)
        {
            object obj = AveAssemblyUtility.InvokeMethod(mKeywordHelper, "AddKeyword", new Type[] { typeof(string), typeof(Mode) }, new object[] { keywordText, (Mode)mode });
            if (obj == null)
            {
                return null;
            }
            return new AveKeyword((Keyword)obj);
        }

        public bool Delete()
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "Delete", new Type[] { }, new object[] { });
        }

        public bool DeleteKeywordChild(AveChildType child, string childTitle)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "DeleteKeywordChild", new object[] { (int)child, childTitle });
        }

        public IAveKeyword GetKeyword(string keywordText)
        {
            object obj = AveAssemblyUtility.InvokeMethod(mKeywordHelper, "GetKeyword", new Type[] { typeof(string) }, new object[] { keywordText });
            if (obj == null)
            {
                return null;
            }
            return new AveKeyword((Keyword)obj);
        }

        public bool Save()
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "Save", new Type[] { }, new object[] { });
        }

        public bool SaveKeyword(AveMode mode, string keywordText, string oneWaySynonym, string twoWaySynonym, string definition)
        {
            Type[] types = new Type[] { typeof(Mode), typeof(string), typeof(string), typeof(string), typeof(string) };
            object[] paramObjs = new object[] { (Mode)mode, keywordText, oneWaySynonym, twoWaySynonym, definition };
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "SaveKeyword", types, paramObjs);
        }

        public bool SaveSynonyms(IAveKeyword keyword, string synonyms, AveSynonymExpansionType synonymType)
        {
            object[] paramObjs = new object[] { (keyword as AveKeyword).Keyword, synonyms, (int)synonymType };
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "SaveSynonyms", paramObjs);
        }

        public bool Update()
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "Update", new Type[] { }, new object[] { });
        }

        public bool UpdateKeyword(AveMode mode, string keywordText, string newKeywordText, string oneWaySynonym, string twoWaySynonym, string definition)
        {
            Type[] types = new Type[] { typeof(Mode), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) };
            object[] paramObjs = new object[] { (Mode)mode, keywordText, newKeywordText, oneWaySynonym, twoWaySynonym, definition };
            return (bool)AveAssemblyUtility.InvokeMethod(mKeywordHelper, "UpdateKeyword", types, paramObjs);
        }
    }
}
