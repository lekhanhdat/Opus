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
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    abstract class AveQuerySessionSchema
    {
        protected AveQueryWorker mQueryWorker;

        public AveQuerySessionSchema(AveQueryWorker queryWorker)
        {
            mQueryWorker = queryWorker;
        }

        protected string ReplaceDirNameAndLeafName(string fullUrl, string commandText)
        {
            string dirName;
            string leafName;
            AveUrlUtility.SplitUrl(fullUrl, out dirName, out leafName);
            return commandText.Replace("@DirName", FilterParameterString(dirName)).Replace("@LeafName", FilterParameterString(leafName));
        }

        private string FilterParameterString(string str)
        {
            return "N'" + str.Replace("'", "''") + "'";
        }

        protected string WebTemplateIdName(int id, string configuration, IAveWebTemplateCollection webTemplates)
        {
            string webTemplateStr = null;
            string sConfig = "#" + configuration;
            foreach (IAveWebTemplate sWebTemplate in webTemplates)
            {
                if (sWebTemplate.ID == id && sWebTemplate.Name.EndsWith(sConfig, StringComparison.OrdinalIgnoreCase))
                {
                    webTemplateStr = sWebTemplate.Name;
                    break;
                }
            }
            return webTemplateStr;
        }

        protected bool IsContainContentTypeId(Dictionary<byte[], AveContentTypeObject> contentTypeChanges, byte[] contentTypeId, out AveContentTypeObject contentTypeChange)
        {
            foreach (var kvp in contentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        contentTypeChange = kvp.Value;
                        return true;
                    }
                }
            }
            contentTypeChange = null;
            return false;
        }

        protected void RemoveContentType(Dictionary<byte[], AveContentTypeObject> ContentTypeChanges, byte[] contentTypeId)
        {
            foreach (var kvp in ContentTypeChanges)
            {
                byte[] bs = kvp.Key;
                if (bs.Length != contentTypeId.Length)
                {
                    continue;
                }
                else
                {
                    int i = 0;
                    for (; i < bs.Length; i++)
                    {
                        if (bs[i] != contentTypeId[i])
                        {
                            break;
                        }
                    }
                    if (i == bs.Length)
                    {
                        ContentTypeChanges.Remove(kvp.Key);
                        return;
                    }
                }
            }
        }
    }
}
