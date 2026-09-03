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
using AvePoint.Wrapper.Common;
using System.IO;


namespace AvePoint.Wrapper.Backup
{
    public class AveSPAppManager : IAveSPAppManager
    {
        //Host Web
        AveSPWeb aveSPWeb = null;
        Guid productId = Guid.Empty;

        public bool IsPRItem = false;
       
        IAveAppSerializer serializer;       

        public AveSPAppManager(AveSPWeb web, Guid productId)
        {
            aveSPWeb = web;
            this.productId = productId;
            
        }

        public void ExportAppBaseInfo(IAveBackupStream output)
        {
            serializer = this.aveSPWeb.SPWeb.AppSerializer;
            serializer.ProductId = this.productId;
            AveAppPackageInfo packageInfo = serializer.GetObjectData();

            if (packageInfo != null)
                output.WriteMetadata(AveMetadataType.AppPackageInfo, packageInfo);
        }

        public void ExportAppPackage(IAveBackupStream output,bool IsPRItem=false)
        {
            Stream mStream = null;
            if (IsPRItem)
            {
                mStream = serializer.GetAppPackageForPRItem13();
            }
            else
            {
                mStream = serializer.GetAppPackage();
            }
            
            if (mStream != null)
            {
#if DEBUG
                try
                {
                    byte[] content = new byte[mStream.Length];
                    mStream.Read(content, 0, (int)mStream.Length);
                    File.WriteAllBytes(@"c:\AveApp.app", content);
                }
                catch(Exception ex)
                {
                    File.WriteAllText(@"c:\AveApp.app", ex.ToString());
                }
                finally
                {
                    mStream.Seek(0, SeekOrigin.Begin);
                }
#endif
                try
                {
                    byte[] buffer = output.DataBuffer;
                    int length;
                    output.FlushMetadata(mStream.Length);
                    long readSize = 0;
                    while (readSize < mStream.Length)
                    {
                        length = mStream.Read(buffer, 0, buffer.Length);
                        if (length == 0)
                        {
                            break;
                        }
                        readSize += length;
                        output.WriteContent(buffer, 0, length);
                    }
                }
                finally
                {
                    mStream.Dispose();
                }
            }
            else
            {
                output.FlushMetadata(0);
            }
        }

        private AveAppPackageInfo GetAppPackageInfo(IAveAppInstance appInstance)
        {
            AveAppPackageInfo packageInfo = new AveAppPackageInfo();
            packageInfo.Title = appInstance.Title;
            packageInfo.ProductId = appInstance.App.ProductId;
            packageInfo.Version = appInstance.App.VersionString;
            packageInfo.AppSource = appInstance.App.Source;
            packageInfo.InstanceId = appInstance.Id;

            return packageInfo;
        }
    }
}
