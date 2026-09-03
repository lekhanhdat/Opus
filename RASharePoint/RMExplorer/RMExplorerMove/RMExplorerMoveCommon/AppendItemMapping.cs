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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class AppendItemMapping
    {


        private readonly Dictionary<string, string> mMappingAppendName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);  // It is for append_1 Name conflict solution

        public void AddToMappingAppendName(string key, string value)
        {
            if (mMappingAppendName.ContainsKey(key))
            {
                mMappingAppendName[key] = value;
            }
            else
            {
                mMappingAppendName.Add(key, value);
            }
            //logger.Debug("fileName is {0}, Mapping Name is {1}", key, value);
        }

        public string GetValueAppendName(string key)
        {
            return mMappingAppendName[key];
        }

        public bool ContainsKeyAppendName(string key)
        {
            return mMappingAppendName.ContainsKey(key);
        }

        public void RemoveAll()
        {
            foreach (string fileName in mMappingAppendName.Keys)
            {
                //logger.Debug("fileName is {0}, Mapping Name is {1}", fileName, mMappingAppendName[fileName]);
            }
            mMappingAppendName.Clear();
        }
    }
}
