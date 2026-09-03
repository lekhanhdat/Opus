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
using System.Threading;
using System.Threading.Tasks;

using Azure.Core.Cryptography;
using Azure.Storage;

using Util.Security;

namespace AvePoint.Application.Security;
public static class AesClientSideEncryptionExtension
{
    public static ClientSideEncryptionOptions Create(byte[] key)
    {
       return new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V2_0)
       {
           KeyEncryptionKey = new AesKeyEncryptionKey(key),
           KeyResolver = new AesKeyEncryptionKeyResolver(),
           // String value that the client library will use when calling IKeyEncryptionKey.WrapKey()
           KeyWrapAlgorithm = "AES"
       };
    }

    private class AesKeyEncryptionKeyResolver : IKeyEncryptionKeyResolver
    {
        public IKeyEncryptionKey Resolve(string keyId, CancellationToken cancellationToken = default)
        {
            return new AesKeyEncryptionKey(Convert.FromBase64String(keyId));
        }

        public async Task<IKeyEncryptionKey> ResolveAsync(string keyId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(Resolve(keyId, cancellationToken));
        }
    }

    private class AesKeyEncryptionKey : IKeyEncryptionKey
    {
        public string KeyId { get { return Convert.ToBase64String(Key); } }
        protected byte[] Key { get; set; }
        public AesKeyEncryptionKey(byte[] key)
        {
            Key = key;
        }

        public byte[] UnwrapKey(string algorithm, ReadOnlyMemory<byte> encryptedKey, CancellationToken cancellationToken = default)
        {
            return new AesGcm(Key).Decrypt(encryptedKey.ToArray());
        }

        public async Task<byte[]> UnwrapKeyAsync(string algorithm, ReadOnlyMemory<byte> encryptedKey, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(UnwrapKey(algorithm, encryptedKey, cancellationToken));
        }

        public byte[] WrapKey(string algorithm, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
        {
            return new AesGcm(Key).Encrypt(key.ToArray());
        }

        public async Task<byte[]> WrapKeyAsync(string algorithm, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(WrapKey(algorithm, key, cancellationToken));
        }
    }
}


