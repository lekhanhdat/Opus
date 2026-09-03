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


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    internal class NameValuePairList
    {
        #region Fields

        internal readonly string Text;
        private List<KeyValuePair<string, string>> _allPairs;
        private Dictionary<string,List<KeyValuePair<string,string>>> _pairsWithName;

        #endregion

        #region Constructors

        internal NameValuePairList() :
            this(null)
        {
        }

        internal NameValuePairList(string text)
        {
            Text = text;
            _allPairs = new List<KeyValuePair<string, string>>();
            _pairsWithName = new Dictionary<string, List<KeyValuePair<string, string>>>();

            Parse(text);
        }

        #endregion

        #region Internal Methods

        internal static string GetNameValuePairsValue(string text, string name)
        {
            NameValuePairList l = new NameValuePairList(text);
            return l.GetNameValuePairValue(name);
        }

        internal List<KeyValuePair<string,string>> GetNameValuePairs(string name)
        {
            if (name == null)
                return _allPairs;
            return _pairsWithName.ContainsKey(name) ? _pairsWithName[name] : new List<KeyValuePair<string,string>>();
        }

        internal string GetNameValuePairValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            List<KeyValuePair<string,string>> al = GetNameValuePairs(name);
            if (al.Count==0)
                return string.Empty;

            // return first item
             return al[0].Value.Trim();
        }

        #endregion

        #region Private Methods

        private void Parse(string text)
        {
            _allPairs.Clear();
            _pairsWithName.Clear();
            if (text == null)
                return;

            string[] p = text.Split(';');
            foreach (string pv in p)
            {
                if (pv.Length == 0)
                    continue;
                string[] onep = pv.Split(new[] {'='}, 2);
                if (onep.Length==0)
                    continue;
                KeyValuePair<string, string> nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLower(),
                                                                                    onep.Length < 2 ? "" : onep[1]);

                _allPairs.Add(nvp);

                // index by name
                List<KeyValuePair<string, string>> al;
                if (!_pairsWithName.ContainsKey(nvp.Key))
                {
                    al = new List<KeyValuePair<string, string>>();
                    _pairsWithName[nvp.Key] = al;
                }
                else
                    al = _pairsWithName[nvp.Key];

                al.Add(nvp);
            }
        }

        #endregion
    }
}