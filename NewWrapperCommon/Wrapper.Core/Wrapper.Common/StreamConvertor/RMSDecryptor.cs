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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.Office365.Api.AIR;

    public class RMSDecryptor : IStreamConvertor
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(RMSDecryptor), false);

        private readonly string tenantId;
        private readonly string appPrincipalId;
        private readonly string symmetricKey;
        private readonly bool useOriginalStreamOnError;
        private FileProtector fileProtector;
        private Exception exception;

        public RMSDecryptor(string tenantId, string appPrincipalId, string symmetricKey, bool useOriginalStreamOnError)
        {
            this.tenantId = tenantId;
            this.appPrincipalId = appPrincipalId;
            this.symmetricKey = symmetricKey;
            this.useOriginalStreamOnError = useOriginalStreamOnError;
        }

        private FileProtector EnsureProtector()
        {
            if (fileProtector == null && exception == null)
            {
                lock (this)
                {
                    if (fileProtector == null && exception == null)
                    {
                        try
                        {
                            fileProtector = new FileProtector(tenantId, appPrincipalId, symmetricKey);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Initialize File Protector failed:{0} with tenant id:{1}, appPrincipalId:{2}", ex, tenantId, appPrincipalId);
                            exception = ex;
                        }
                    }
                }
            }

            return fileProtector;
        }

        public Stream Process(IAveList list, Stream inputStream, string fileName)
        {
            if (list.IrmEnabled)
            {
                return Process(inputStream, fileName);
            }
            //else
            //{
            //    logger.Info("unprotected file:{0} under list:{1}", fileName, list.Title);
            //}

            return inputStream;
        }

        public void Dispose()
        {
            if (fileProtector != null)
            {
                fileProtector.Dispose();
                fileProtector = null;
            }
        }

        private bool IsFileEncrypted(Stream inputStream, string fileName)
        {
            return FileProtector.IsFileEncrypted(inputStream, fileName);
        }

        public Stream Process(Stream inputStream, string fileName)
        {
            Stream targetStream = inputStream;

            Stopwatch stopwatch = Stopwatch.StartNew();
            if (IsFileEncrypted(inputStream, fileName))
            {
                var protector = EnsureProtector();

                if (protector == null)
                {
                    if (!useOriginalStreamOnError)
                    {
                        throw exception;
                    }
                    inputStream.Position = 0;
                }
                else
                {
                    targetStream = new AveCoordinatedStream();
                    try
                    {
                        protector.DecryptFile(inputStream, fileName, targetStream);
                        targetStream.Position = 0;
                        inputStream.Dispose();
                        stopwatch.Stop();
                        logger.Info("decrypt file:{0} time:{1}", fileName, stopwatch.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        logger.Error("decrypt file:{0} with time:{1} failed:{2}", fileName, stopwatch.Elapsed, ex);
                        targetStream.Dispose();
                        if (!useOriginalStreamOnError)
                        {
                            //var newEx = ex as Office365.Api.AIR.IPC.InformationProtectionException;

                            //if (newEx != null)
                            //{
                            //    throw HandleException(fileName, newEx);
                            //}

                            throw;
                        }
                        else
                        {
                            inputStream.Position = 0;
                            targetStream = inputStream;
                        }
                    }
                }
            }
            else
            {
                stopwatch.Stop();
                inputStream.Position = 0;
                logger.Info("unprotected file:{0} time:{1}", fileName, stopwatch.Elapsed);
            }

            return targetStream;
        }
    }
}
