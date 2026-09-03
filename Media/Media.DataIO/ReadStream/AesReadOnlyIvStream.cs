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

namespace MediaDataIO;
public class AesReadOnlyIvStream : ReadOnlyStreamBase
{
    protected Aes Aes = Aes.Create();
    protected Stream EncryptedStream { get; set; }
    protected Stream InternalStream { get; set; }
    protected bool FirstRead { get; set; } = true;
    public AesReadOnlyIvStream(Stream encryptedStream, byte[] key)
    {
        Aes.Key = key;
        //use default padding PKCS7
        // Aes.Padding = PaddingMode.None;
        EncryptedStream = encryptedStream;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (FirstRead)
        {
            var tempBuffer = new byte[Aes.BlockSize / 8];
            var size = await EncryptedStream.ReadAtLeastAsync(tempBuffer, tempBuffer.Length, true);
            if (size < tempBuffer.Length)
            {
                throw new InvalidDataException($"read size {size} less than expected {tempBuffer.Length}");
            }
            Aes.IV = tempBuffer;
            InternalStream = new CryptoStream(EncryptedStream, Aes.CreateDecryptor(), CryptoStreamMode.Read);
            FirstRead = false;
        }
        return await InternalStream.ReadAsync(buffer, cancellationToken);
    }

    public async override ValueTask DisposeAsync()
    {
        if (InternalStream != null)
        {
            await InternalStream.DisposeAsync();
            InternalStream = null;
        }
        await base.DisposeAsync();
    }
    protected override void Dispose(bool disposing)
    {
        if (InternalStream != null)
        {
            InternalStream.Dispose();
            InternalStream = null;
        }
        base.Dispose(disposing);
    }

    public override int Read(Span<byte> buffer)
    {
        if (FirstRead)
        {
            var tempBuffer = new byte[Aes.BlockSize / 8];
            var size = EncryptedStream.ReadAtLeast(tempBuffer, tempBuffer.Length, true);
            if (size < tempBuffer.Length)
            {
                throw new InvalidDataException($"read size {size} less than expected {tempBuffer.Length}");
            }
            Aes.IV = tempBuffer;
            InternalStream = new CryptoStream(EncryptedStream, Aes.CreateDecryptor(), CryptoStreamMode.Read);
            FirstRead = false;
        }
        return InternalStream.Read(buffer);
    }
}