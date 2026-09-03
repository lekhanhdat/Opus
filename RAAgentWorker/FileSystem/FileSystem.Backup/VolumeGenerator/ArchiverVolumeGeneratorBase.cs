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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Text;

    #endregion

    public abstract class ArchiverVolumeGeneratorBase : VolumeGeneratorBase
    {
        protected void ParseSitePath(String siteURL, out String webAppName, out String siteName)
        {
            int index = -1;
            StringBuilder tmp = new StringBuilder();
            index = siteURL.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            tmp.Append(siteURL.Substring(0, index)).Append("#");
            string temp = siteURL.Substring(index + 3);
            index = -1;
            index = temp.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                tmp.Append(80).Append("#");
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
            }
            else
            {
                String machineName = temp.Substring(0, index);
                temp = temp.Substring(index + 1);
                index = -1;
                index = temp.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    tmp.Append(temp.Substring(0, index));
                    temp = temp.Substring(index + 1);
                }
                else
                {
                    tmp.Append(temp);
                    temp = "";
                }
                tmp.Append("#").Append(machineName);
            }
            webAppName = tmp.ToString();
            tmp.Remove(0, tmp.Length);
            tmp.Append("#");
            if (temp.Length > 0)
            {
                temp = temp.Replace(';', '#');
                tmp.Append(temp.Replace('/', '#'));
            }
            siteName = tmp.ToString();
        }
    }
}