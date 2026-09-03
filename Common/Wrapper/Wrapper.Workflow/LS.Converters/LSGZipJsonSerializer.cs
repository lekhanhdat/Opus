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
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using StjJsonSerializer = System.Text.Json.JsonSerializer;
using StjJsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace LS.Converters
{
    /// <summary>
    /// Serializes objects into GZip-compressed byte arrays using
    /// <see cref="System.Text.Json"/>. The uncompressed payload is prefixed with a
    /// version marker so that <see cref="Deserialize{TData}(byte[])"/> can distinguish
    /// new JSON payloads from legacy <c>BinaryFormatter</c> data. Legacy (unmarked)
    /// payloads are read through a read-only <c>BinaryFormatter</c> fallback so that data
    /// persisted before this migration can still be loaded.
    /// </summary>
    public static class LSGZipJsonSerializer
    {
        /// <summary>
        /// Marker prepended to every uncompressed JSON payload. The bytes are chosen so
        /// that they never match the start of a legacy <c>BinaryFormatter</c> stream
        /// (which always begins with the serialization header byte 0x00).
        /// </summary>
        private static readonly byte[] JsonFormatMarker = Encoding.UTF8.GetBytes("JSONv1|");

        /// <summary>
        /// Options used to replace <c>BinaryFormatter</c>-based serialization with
        /// <see cref="System.Text.Json"/>. Fields are included since the types previously
        /// serialized via <c>BinaryFormatter</c> expose data through fields rather than
        /// properties. A <see cref="HashtableJsonConverter"/> preserves the runtime types
        /// and case-insensitive key semantics of non-generic <see cref="System.Collections.Hashtable"/> values.
        /// </summary>
        private static readonly StjJsonSerializerOptions JsonOptions = CreateOptions();

        private static StjJsonSerializerOptions CreateOptions()
        {
            StjJsonSerializerOptions options = new StjJsonSerializerOptions
            {
                IncludeFields = true
            };
            options.Converters.Add(new HashtableJsonConverter());
            return options;
        }

        /// <summary>
        /// Serializes <paramref name="data"/> to UTF-8 JSON bytes, prepends the version
        /// marker and GZip-compresses the result.
        /// </summary>
        /// <typeparam name="TData">The type of data to serialize.</typeparam>
        /// <param name="data">The data to serialize.</param>
        /// <returns>A GZip-compressed byte array, or <c>null</c> when <paramref name="data"/> is <c>null</c>.</returns>
        public static byte[] Serialize<TData>(TData data)
        {
            if (data == null)
                return null;

            byte[] jsonBytes = StjJsonSerializer.SerializeToUtf8Bytes(data, typeof(TData), JsonOptions);

            byte[] markedBytes = new byte[JsonFormatMarker.Length + jsonBytes.Length];
            Buffer.BlockCopy(JsonFormatMarker, 0, markedBytes, 0, JsonFormatMarker.Length);
            Buffer.BlockCopy(jsonBytes, 0, markedBytes, JsonFormatMarker.Length, jsonBytes.Length);

            return Compress(markedBytes);
        }

        /// <summary>
        /// GZip-decompresses <paramref name="serializedData"/> and deserializes it. Marked
        /// payloads are read with <see cref="System.Text.Json"/>; unmarked payloads are
        /// treated as legacy <c>BinaryFormatter</c> data for backward compatibility.
        /// </summary>
        /// <typeparam name="TData">The type of data to deserialize.</typeparam>
        /// <param name="serializedData">The GZip-compressed byte array.</param>
        /// <returns>The deserialized object.</returns>
        public static TData Deserialize<TData>(byte[] serializedData)
        {
            if (serializedData == null)
                return default;

            byte[] decompressedData = new byte[0];

            #region Decompress serialized Metadata
            MemoryStream tempStream = new MemoryStream(serializedData);
            tempStream.Position = 0L;
            using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
            {

                byte[] temp = new byte[4096];
                int readLen;
                while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                {
                    LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                }
            }
            #endregion

            if (HasJsonMarker(decompressedData))
            {
                int offset = JsonFormatMarker.Length;
                ReadOnlySpan<byte> jsonSpan = new ReadOnlySpan<byte>(decompressedData, offset, decompressedData.Length - offset);
                return (TData)StjJsonSerializer.Deserialize(jsonSpan, typeof(TData), JsonOptions);
            }

            // Legacy data written by the old BinaryFormatter based build.
            using (MemoryStream stream = new MemoryStream(decompressedData))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (TData)formatter.Deserialize(stream);
            }
        }

        private static bool HasJsonMarker(byte[] data)
        {
            if (data == null || data.Length < JsonFormatMarker.Length)
                return false;

            for (int i = 0; i < JsonFormatMarker.Length; i++)
            {
                if (data[i] != JsonFormatMarker[i])
                    return false;
            }
            return true;
        }

        private static byte[] Compress(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data.Length))
            {
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                byte[] compressed = stream.GetBuffer();
                Array.Resize(ref compressed, Convert.ToInt32(stream.Length));
                return compressed;
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            using (MemoryStream source = new MemoryStream(data))
            using (GZipStream gzipStream = new GZipStream(source, CompressionMode.Decompress, true))
            using (MemoryStream destination = new MemoryStream())
            {
                gzipStream.CopyTo(destination);
                return destination.ToArray();
            }
        }
    }
}
