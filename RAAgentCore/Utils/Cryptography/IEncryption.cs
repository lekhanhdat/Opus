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



using System.Security;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    #region using directives
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using AvePoint.Hybrid.Utility.Cryptography.Encryption;
    #endregion

    /// <summary>
    /// Represent a global Encryption function of DocAve platform, All of the DocAve
    /// Modules should be use this interface and EncryptionFactory class to 
    /// </summary>
    /// <example> Usage of the DocAve GCommon Utility IEncryption module
    /// <code>
    ///    public static void Test()
    ///    {
    ///        IEncryption e1 = EncryptionFactory.GetEncryption("blowfish");
    ///        string hello = "Hello,World";
    ///        byte[] bts = e1.EncodeString(hello);
    ///        string hello1 = e1.DecodeString(bts);
    ///        Console.WriteLine(string.Compare(hello, hello1, StringComparison.Ordinal) == 0);
    ///
    ///        IEncryption e2 = EncryptionFactory.GetEncryption("aes");
    ///        hello = "Hello,World";
    ///        bts = e2.EncodeString(hello);
    ///        hello1 = e2.DecodeString(bts);
    ///        Console.WriteLine(string.Compare(hello, hello1, StringComparison.Ordinal) == 0);
    ///    }
    /// </code>
    /// </example>
    public interface IEncryption : ICryptography
    {




        /// <summary>
        /// 加密二进制数据
        /// </summary>
        /// <param name="data">要加密的二进制数据的Byte数组</param>
        /// <returns>加密后的二进制数据的Byte数组</returns>
        Byte[] EncryptBinary(Byte[] data);

        /// <summary>
        /// 解密二进制数据
        /// </summary>
        /// <param name="data">要解密的二进制密文的Byte数组</param>
        /// <returns>解密后的二进制明文的Byte数组</returns>
        Byte[] DecryptBinary(Byte[] data);








        /// <summary>
        /// 加密二进制数据
        /// </summary>
        /// <param name="data">要加密的二进制明文的Byte数组</param>
        /// <param name="start">要加密的数组的偏移量</param>
        /// <param name="length">要加密的数组的长度</param>
        /// <returns>加密后的二进制数据的Byte数组</returns>
        Byte[] EncryptBinary(Byte[] data, Int32 start, Int32 length);


        /// <summary>
        /// 解密二进制数据
        /// </summary>
        /// <param name="data">要解密的二进制密文的Byte数组</param>
        /// <param name="start">要解密的数组的偏移量</param>
        /// <param name="length">要解密的数组的长度</param>
        /// <returns>解密后的二进制明文的Byte数组</returns>
        Byte[] DecryptBinary(Byte[] data, Int32 start, Int32 count);




        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainString">要加密的字符串明文，类型为SecureString</param>
        /// <returns>加密后的二进制密文的Byte数组</returns>
        Byte[] EncryptString(SecureString plainString);


        /// <summary>
        /// 解密二进制密文，并转换成字符串明文
        /// </summary>
        /// <param name="encryptedByte">被加密后的二进制密文Byte数组</param>
        /// <returns>原字符串明文</returns>
        SecureString DecryptString(Byte[] encryptedByte);







        /// <summary>
        /// 加密字符串并进行Base64编码
        /// </summary>
        /// <param name="plainString">要加密的字符串明文，类型为SecureString</param>
        /// <returns>加密并且Base64编码后的字符串密文</returns>
        String EncryptStringWithBase64(SecureString plainString);



        /// <summary>
        /// 解密Base64编码后的字符串密文
        /// </summary>
        /// <param name="encryptedString">被加密并被编码后的字符串密文</param>
        /// <returns>原字符串明文</returns>
        SecureString DecryptString(String encryptedString);







        /// <summary>
        /// 加密二进制明文并进行Base64编码
        /// </summary>
        /// <param name="data">二进制明文的Byte数组</param>
        /// <returns>Base64编码后的字符串密文</returns>
        string EncryptBytesWithBase64(byte[] data);



        /// <summary>
        /// 对Base64字符串密文进行解密
        /// </summary>
        /// <param name="data">被加密后并被Base64编码后的字符串密文</param>
        /// <returns>明文Byte数组</returns>
        Byte[] DecryptBytesWithBase64(string data);




        /// <summary>
        /// 获得加密算法的Key
        /// </summary>
        /// <value>加密算法的Key，为数组</value>
        Byte[] Key { get; }


        /// <summary>
        /// 获得和设置加密算法的IV
        /// </summary>
        /// <value>加密算法的IV，为数组</value>
        Byte[] IV { get; set; }

        /// <summary>
        /// 随机生成加密算法的IV
        /// </summary>
        void GenerateIV();

        /// <summary>
        /// 获得当前Key的长度
        /// </summary>
        /// <value>Key的长度，为Int类型</value>
        int CurrentKeySize { get; }

        /// <summary>
        /// 获得当前加密块的长度
        /// </summary>
        /// <value>加密块的长度，为Int类型</value>
        int CurrentBlockSize { get; }



        KeySizes[] SupportedKeySizes { get; }
        KeySizes[] SupportedBlockSizes { get; }



        CryptoStream CreateEncryptStream(Stream stream, CryptoStreamMode mode, string key = null);
        CryptoStream CreateDecryptStream(Stream stream, CryptoStreamMode mode, string key = null);
        CryptoWithIVStream CreateEncryptWithIVStream(Stream stream, CryptoStreamMode mode, string key = null);
        CryptoWithIVStream CreateDecryptWithIVStream(Stream stream, CryptoStreamMode mode, string key = null);

        byte[] GetTestData();

    }
}
