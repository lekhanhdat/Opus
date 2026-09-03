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





namespace AvePoint.Media.Storage.Util
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion

    public class XRI : INotifyPropertyChanged
    {
        #region --constant--

        //docave-xam://<vim name>!<connection string>[?<name>=<value>[&<name>=<value>] ... ]
        //patten - docave-xam://()!()

        /** SNIA XAM XRI prefix */
        public static readonly string SNIA_PREFIX = "SNIA-XAM://".ToLower(CultureInfo.InvariantCulture);

        public static readonly string DocAve_PREFIX = "DOCAVE-XAM://".ToLower(CultureInfo.InvariantCulture);

        public static readonly string PREFIX = "[(" + SNIA_PREFIX + ")|(" + DocAve_PREFIX + ")]";

        protected static readonly string PARAM_PATTERN = "[\\&]{0,1}([^=^&]+)\\=([^=^&]*)";

        // Match any set of characters, EXCEPT for the separator "!"
        protected static readonly string VIM_PATTERN = "(?:([^/\\?]+)\\?)";

        // Match any set of characters, EXCEPT for the separator "!" and "?"
        //protected const string SYSTEM_PATTERN = "([^?^!]*){1}";

        protected static readonly string XRI_PARAM_PATTERN = "([^=^&]+=[^=^&]*(?:\\&[^=^&]+\\=[^=^&]*)*)?";

        //protected const string XRI_PATTERN = PREFIX + VIM_PATTERN + SYSTEM_PATTERN + XRI_PARAM_PATTERN;
        protected static readonly string XRI_PATTERN = PREFIX + VIM_PATTERN + XRI_PARAM_PATTERN;

        protected static readonly Regex s_xri = new Regex(XRI_PATTERN);

        protected static readonly Regex s_params = new Regex(PARAM_PATTERN);

        #endregion

        #region --XRI components--
        private string protrocol = DocAve_PREFIX;
        private string vim;
        private string system;
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        public string Protocal
        {
            get { return this.protrocol; }
            set { this.protrocol = value; }
        }

        public string VIM
        {
            get { return this.vim; }
            set { this.vim = value; }
        }

        public string SystemLocation
        {
            get { return this.system; }
            set
            {
                this.system = value;
                OnPropertyChanged("SystemLocation");
            }
        }

        public Dictionary<string, string> Params
        {
            get { return this.parameters; }
        }

        #endregion

        #region -- Convert methods--

        /**
         * Creates an XRI instance from an XRI string. If the string does not contain
         * a valid XRI an {@link InvalidXRIException} is thrown.
         * 
         * @param xriString The XRI string
         * @return An XRI instance
         * @throws InvalidXRIException If the string does not contain a valid XRI
         */
        public static XRI ValueOf(string xriString)
        {
            Match match = s_xri.Match(xriString, 0, xriString.Length);
            if (!match.Success)
            {
                throw new Exception(xriString);
            }

            XRI xri = new XRI();
            xri.Protocal = xriString.StartsWith(XRI.DocAve_PREFIX, StringComparison.OrdinalIgnoreCase) ? XRI.DocAve_PREFIX : XRI.SNIA_PREFIX;
            xri.VIM = match.Groups[1].Value;
            //xri.SystemLocation = match.Groups[2].Value;
            //if (string.IsNullOrEmpty(xri.VIM) || string.IsNullOrEmpty(xri.SystemLocation))
            //{
            //    throw new Exception("Null system name not allowed in XRI");
            //}
            string parameters = match.Groups[2].Value;
            if (parameters != null)
            {
                if (s_params.IsMatch(parameters))
                {
                    match = s_params.Match(parameters, 0, parameters.Length);
                    while (match.Success)
                    {
                        string key = match.Groups[1].Value.ToLower(CultureInfo.InvariantCulture);
                        string value = match.Groups[2].Value.Trim(new char[] { ' ' });
                        xri.Params.Add(key, ValueDecode(value));
                        match = match.NextMatch();
                    }
                }
            }
            return xri;
        }

        /**
         * Creates an XRI instance from an XRI string. If the string does not contain
         * a valid XRI an {@link InvalidXRIException} is thrown.
         * 
         * @param xriString The XRI string
         * @return An XRI instance
         * @throws InvalidXRIException If the string does not contain a valid XRI
         */
        public static void ValueOfWithBinding(XRI xri, string xriString)
        {
            Match match = s_xri.Match(xriString, 0, xriString.Length);
            if (!match.Success)
            {
                throw new Exception(xriString);
            }

            xri.Protocal = xriString.StartsWith(XRI.DocAve_PREFIX, StringComparison.OrdinalIgnoreCase) ? XRI.DocAve_PREFIX : XRI.SNIA_PREFIX;
            xri.VIM = match.Groups[1].Value;
            //xri.SystemLocation = match.Groups[2].Value;
            //if (string.IsNullOrEmpty(xri.VIM) || string.IsNullOrEmpty(xri.SystemLocation))
            //{
            //    throw new Exception("Null system name not allowed in XRI");
            //}
            string parameters = match.Groups[2].Value;
            if (parameters != null)
            {
                if (s_params.IsMatch(parameters))
                {
                    match = s_params.Match(parameters, 0, parameters.Length);
                    while (match.Success)
                    {
                        string key = match.Groups[1].Value.ToLower(CultureInfo.InvariantCulture);
                        string value = match.Groups[2].Value;
                        xri[key] = ValueDecode(value);
                        match = match.NextMatch();
                    }
                }
            }
        }

        public static string UNC2XRIString(string location, string username, string encrptedPassword)
        {
            return XRI.DocAve_PREFIX + "fs_vim?location=" + ValueEncode(location) + "&name=" + ValueEncode(username) + "&secret=" + encrptedPassword;
        }
        /**
         * Converts the XRI into a properly formatted string.
         * 
         * @return The XRI as a string.
         */
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder(80);
            buf.Append(protrocol);
            buf.Append(vim);
            bool first = true;

            foreach (KeyValuePair<string, string> keyVal in parameters)
            {
                if (string.IsNullOrEmpty(keyVal.Value))
                {
                    continue;
                }
                if (first)
                {
                    buf.Append('?');
                    first = false;
                }
                else
                {
                    buf.Append('&');
                }
                string name = keyVal.Key;

                buf.Append(name);
                buf.Append('=');
                string value = ValueEncode(keyVal.Value);//XRIUtil.StringToHexString(keyVal.Value, Encoding.UTF8);
                buf.Append(value);
            }
            return buf.ToString();
        }

        public static string ValueEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
        }

        public static string ValueDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%3D", "=").Replace("%26", "&").Replace("%5e", "^").Replace("%25", "%");
        }

        public void ResetParams(StorageFeature feature)
        {
            if (feature == null || feature.Features == null)
            {
                return;
            }
            string[] keys = new string[parameters.Keys.Count];
            parameters.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                FeatureUnit fu = feature.Features.Find(delegate(FeatureUnit u)
                {
                    return u.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase);
                });
                if (fu != null)
                {
                    this[key] = fu.DefaultValue;
                }
                else
                {
                    this[key] = "";
                }
            }
        }
        #endregion

        #region for binding data to gui

        public string this[string key]
        {
            get
            {
                //key = key.ToLower(CultureInfo.InvariantCulture);
                if (!parameters.ContainsKey(key))
                {
                    return null;
                }
                return parameters[key];
            }
            set
            {

                parameters[key] = value;
                OnPropertyChanged("Item[]");//string.Format("Item[{0}]", key));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

    }
}
