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
namespace AvePoint.Office365.Api.AIR
{
    using System;
    using System.IO;
    using AvePoint.Office365.Api.AIR.IPC;
    using GCommon;
    using System.Reflection;
    using Wrapper.Common;
    public class FileProtector : IDisposable
    {
        private readonly SafeIpcPromptContext context;
        private readonly SymmetricKeyCredential credential;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public FileProtector(string tenantId, string appPrincipalId, string symmetricKey)
        {
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(appPrincipalId) || string.IsNullOrEmpty(symmetricKey))
            {
                throw new AveWrapperOffice365ApiException(AveWrapperErrorCode.AIRSuperUserNotConfigured, AveInternalResourceKey.Wrapper_Exception_Common_AIRSuperUserNotConfigured);
            }

            log.Info("Tenant Id:{0}, App principal Id:{1}, Key Hash Code:{2}", tenantId, appPrincipalId, symmetricKey.GetHashCode());

            MSIPCRuntime.EnsureRuntime();

            credential = new SymmetricKeyCredential()
            {
                Base64Key = symmetricKey,
                AppPrincipalId = appPrincipalId,
                BposTenantId = tenantId
            };

            this.context = SafeNativeMethods.CreateIpcPromptContext(true, false, true, IntPtr.Zero, credential, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream">
        /// Pointer to the byte stream that represents the file to be queried.
        /// </param>
        /// <param name="inputFilePath">
        /// The path to the file to decrypt. 
        /// The path must include the file name and, if one exists, the file name extension.
        /// This parameter is only used determine the file format, 
        /// based on the file name extension of the file in the input file stream.
        /// Based on this, the suggested output filename is returned via pwszOutputFilePath parameter.
        /// </param>
        /// <returns></returns>
        public static bool IsFileEncrypted(Stream inputStream, string inputFilePath)
        {
            MSIPCRuntime.EnsureRuntime();

            return SafeFileApiNativeMethods.IpcfIsFileStreamEncrypted(inputStream, inputFilePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream">
        /// Pointer to the byte stream that represents the file to be decrypted.
        /// </param>
        /// <param name="inputFilePath">
        /// The path to the file to decrypt. 
        /// The path must include the file name and, if one exists, the file name extension.
        /// This parameter is only used determine the file format, 
        /// based on the file name extension of the file in the input file stream.
        /// Based on this, the suggested output filename is returned via pwszOutputFilePath parameter.
        /// </param>
        /// <param name="outputStream"></param>
        /// <returns>return the output file path</returns>
        public string DecryptFile(Stream inputStream, string inputFilePath, Stream outputStream)
        {
            return SafeFileApiNativeMethods.IpcfDecryptFileStream(inputStream, inputFilePath, SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_DEFAULT, ref outputStream, context);
        }
            
        public void Dispose()
        {
            SafeNativeMethods.ReleaseIpcPromptContext(context);
        }
    }
}
