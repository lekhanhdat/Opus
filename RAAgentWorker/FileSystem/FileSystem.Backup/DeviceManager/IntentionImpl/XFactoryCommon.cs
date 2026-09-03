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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Media.Storage;
using AvePoint.Media.Storage.Util;

namespace Media.Common.ClassicStorageApi
{
    public class XFactoryCommon
    { 
        private static List<string> ClassicVIMList = new List<string>() { "box_vim", "rackspace_vim", "netapp_alta_vault_vim" };
        public static IXSystem InstanceSystem(string connectionString)//IXSystem
        {
            XRI xri = XRI.ValueOf(connectionString);
            if (ClassicVIMList.Contains(xri.VIM.ToLower()))
            {
                //return AvePoint.Media.ClassicStorage.XFactory.InstanceSystem(connectionString);
                return XFactory.InstanceSystem(connectionString);
            }
            else
            {
                return XFactory.InstanceSystem(connectionString);
            }
        }
        public static XLibrary InstanceLibrary(List<string> connectionStrings)//XLibrary
        {
            List<string> classicConnectionStrings = new List<string>();
            List<string> modernConnectionStrings = new List<string>();
            XLibrary classicXLibrary = null;
            XLibrary modernXLibrary = null;

            foreach (var c in connectionStrings)
            {
                XRI xri = XRI.ValueOf(c);
                if (ClassicVIMList.Contains(xri.VIM.ToLower()))
                {
                    classicConnectionStrings.Add(c);
                }
                else
                {
                    modernConnectionStrings.Add(c);
                }
            }

            if (classicConnectionStrings.Count > 0)
            {
                //classicXLibrary= AvePoint.Media.ClassicStorage.XFactory.InstanceLibrary(classicConnectionStrings);
            }
            if (modernConnectionStrings.Count > 0)
            {
                modernXLibrary = AvePoint.Media.Storage.XFactory.InstanceLibrary(modernConnectionStrings);
            }

            if(modernXLibrary != null && classicXLibrary != null)
            {
                modernXLibrary.SubSystems.AddRange(classicXLibrary.SubSystems);
                return modernXLibrary;
            }
            else if(modernXLibrary != null)
            {
                return modernXLibrary;
            }
            else if (classicXLibrary != null)
            {
                return classicXLibrary;
            }

            return new XLibrary();
        }
    }

}
