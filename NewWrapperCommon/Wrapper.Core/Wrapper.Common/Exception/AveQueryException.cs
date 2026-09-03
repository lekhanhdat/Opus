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
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class AveQueryException : AveWrapperBaseException
    {
        public AveQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AveQueryException(SqlException ex)
            : base(ex, AveInternalResourceKey.Wrapper_Exception_QueryService_ErrorCode, ex.Number)
        {

        }

        public int ErrorCode
        {
            get
            {
                var innerEx = base.InnerException as SqlException;
                return innerEx != null ? innerEx.Number : 0;
            }
        }

        public new Exception InnerException
        {
            get { return null; }
        }

        public override string ToString()
        {
            return string.Format("AvePoint.Wrapper.QueryService.AveQueryException:{0}\r\n{1}\r\n{2}", this.Message, AnalyzeString(this.StackTrace), WrapperException(base.InnerException));
            //return this.GetType().FullName + ":" + this.Message + "\r\n" + this.StackTrace;
        }

        public static string AnalyzeException(Exception exception, bool throwIt)
        {
            string exceptionDetails = string.Empty;

            if (exception != null)
            {
                if (exception is SqlException)
                {
                    SqlException sqlException = (SqlException)exception;
                    exceptionDetails = string.Format("Exception error code:{0}\r\n{1}", sqlException.Number, WrapperException(sqlException));
                }
                else
                {
                    exceptionDetails = string.Format("Exception message:{0}\r\n{1}", exception.Message, WrapperException(exception));
                }

                if (throwIt)
                {
                    throw new Exception(exceptionDetails);
                }
            }

            return exceptionDetails;
        }

        public static string WrapperException(Exception exception)
        {
            string encryptedInfo = string.Empty;

            if (exception != null)
            {
                try
                {
                    encryptedInfo = exception.ToString();
                    encryptedInfo = string.Format("[Dump binary]:{0}\r\n",
                        InternalCrypto.EncryptMessage(exception.ToString()));
                    //CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(exception.ToString())));
                }
                catch (Exception ex)
                {
                    encryptedInfo += ex.ToString();
                }
            }

            return encryptedInfo;
        }

        public static string AnalyzeString(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    const string exceptionHeader = "exception:";
                    const string executeString = "Execute";

                    StringBuilder builder = new StringBuilder();
                    string[] messageArray = message.Split('\n');
                    for (int i = 0; i < messageArray.Length; i++)
                    {
                        string str = messageArray[i];
                        if (i == 0)
                        {
                            int index = str.IndexOf(exceptionHeader, StringComparison.OrdinalIgnoreCase);
                            if (index >= 0)
                            {
                                builder.Append(str.Substring(i + exceptionHeader.Length));
                                continue;
                            }
                        }

                        if (str.IndexOf(executeString, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            continue;
                        }

                        builder.Append(str);
                    }

                    message = builder.ToString();
                }
                catch (Exception ex)
                {
                    message += ex.ToString();
                }
            }

            return message;
        }

        /// <summary>
        /// Wrapper里面需要一个Hard Code来加密内容
        /// </summary>
        public class InternalCrypto
        {
            private static byte[] key = { 15, 218, 43, 167, 98, 156, 234, 134 };
            private static byte[] iv = { 145, 138, 67, 7, 198, 56, 224, 113 };

            public static string EncryptMessage(string message)
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(stream, desProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                                    cryptoStream.Write(buffer, 0, buffer.Length);
                                    cryptoStream.Close();
                                    result = Convert.ToBase64String(stream.ToArray());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
                    }
                }

                return result;
            }

            public static string DecryptMessage(string message)
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(stream, desProvider.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                                {
                                    byte[] buffer = Convert.FromBase64String(message);//Encoding.UTF8.GetBytes(message);
                                    cryptoStream.Write(buffer, 0, buffer.Length);
                                    cryptoStream.Close();
                                    result = Encoding.UTF8.GetString(stream.ToArray());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
                    }
                }

                return result;
            }
        }
    }
}