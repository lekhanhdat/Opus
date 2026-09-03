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
using System.IO;
using System.Security.Cryptography;

namespace AvePoint.Hybrid.Utility.Cryptography.Encryption
{
    public class CryptoWithIVStream : Stream, IDisposable
    {
        private bool iVAction = false;
        public byte[] IV
        {
            get;
            set;
        }
        MemoryStream iVStream;
        private CryptoStream cryptoStream;
        AbstractEncryption crypto;
        Stream finalStream;
        CryptoStreamMode mode;
        private string key;


        public CryptoWithIVStream(Stream stream, AbstractEncryption _crypto,EncryptionMode encryptionMode, CryptoStreamMode _mode, string _key)
        {
            key = _key;
            crypto = _crypto;
            finalStream = stream;
            mode = _mode;
            if(encryptionMode == EncryptionMode.DECRYPTION && mode == CryptoStreamMode.Read) {
                    IV = new byte[_crypto.CurrentBlockSize / 8]; 
                    int result = stream.Read(IV, 0, IV.Length);
                    if(result != IV.Length) {
                        throw new Exception("Can not read enough data");
                    }
                    _crypto.IV = IV;
                    cryptoStream = crypto.CreateDecryptStream(stream, _mode, key);

            }
                
            else if (encryptionMode == EncryptionMode.ENCRYPTION && mode == CryptoStreamMode.Write) {


                stream.Write(_crypto.IV, 0, crypto.IV.Length);
                cryptoStream = crypto.CreateEncryptStream(stream, mode, key);

            }

            else if(encryptionMode == EncryptionMode.DECRYPTION && mode == CryptoStreamMode.Write) {
                iVAction = true;
                IV = new byte[_crypto.CurrentBlockSize / 8];
                iVStream = new MemoryStream(IV);


            }

            else if (encryptionMode == EncryptionMode.ENCRYPTION && mode == CryptoStreamMode.Read) {

                iVAction = true;
                IV = _crypto.IV;
                iVStream = new MemoryStream(IV);
                cryptoStream = crypto.CreateEncryptStream(stream, mode, key);
 
            }
            

        }

        public override bool CanRead
        {
            get { return cryptoStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return cryptoStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return cryptoStream.CanWrite; }
        }

        public override void Flush()
        {
            cryptoStream.Flush();
        }

        public override long Length
        {
            get { return cryptoStream.Length; }
        }

        public void FlushFinalBlock()
        {
            cryptoStream.FlushFinalBlock();
        }

        public override long Position
        {
            get
            {
                return cryptoStream.Position;
            }
            set
            {
                cryptoStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {

            if (iVAction == true)
            {
                int result = iVStream.Read(buffer, offset, (count < iVStream.Length ? count : (int)iVStream.Length));
                if (iVStream.Capacity == iVStream.Position)
                {

                    result += cryptoStream.Read(buffer, offset + result, count - result);
                    iVAction = false;
                }
                return result;
            }

            return cryptoStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return cryptoStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            cryptoStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

            if (iVAction == true)
            {
                
                iVStream.Write(buffer, offset, (count < iVStream.Length ? count : (int)iVStream.Length));
                if (iVStream.Capacity == iVStream.Length)
                {
                    crypto.IV = IV;
                    cryptoStream = crypto.CreateDecryptStream(finalStream, mode, key);
                    cryptoStream.Write(buffer, offset + IV.Length, count - IV.Length);
                    iVAction = false;
                }
                return;
            }

            cryptoStream.Write(buffer, offset, count);
        }

        //public override void Close()
        //{
        //    cryptoStream.Close();

        //}
        protected override void Dispose(bool disposing)
        {
            cryptoStream.Dispose();

        }


 
    }
}
