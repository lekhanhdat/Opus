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
using System.Xml;

namespace Microsoft.Office.Project.Server.Library
{
	// Token: 0x02000E58 RID: 3672
	public class GeneralUtility
	{
		// Token: 0x06000ECE RID: 3790 RVA: 0x000563F4 File Offset: 0x000545F4
		public static XmlDocument StringToXMLDoc(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				throw new ArgumentNullException(data);
			}
			StringReader stringReader = null;
			XmlDocument result;
			try
			{
				stringReader = new StringReader(data);
				XmlDocument xmlDocument = new XmlDocument();
				using (XmlReader xmlReader = XmlReader.Create(stringReader))
				{
					stringReader = null;
					xmlDocument.Load(xmlReader);
				}
				result = xmlDocument;
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Dispose();
				}
			}
			return result;
		}

		// Token: 0x06000ECF RID: 3791 RVA: 0x00056468 File Offset: 0x00054668
		public static byte[] Compress(byte[] data)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream(data, false))
			{
				using (MemoryStream memoryStream2 = new MemoryStream(data.Length / 4))
				{
					using (DeflateStream deflateStream = new DeflateStream(memoryStream2, CompressionMode.Compress, true))
					{
						int num = 16384;
						byte[] buffer = new byte[num];
						int num2 = memoryStream.Read(buffer, 0, num);
						while (0 < num2)
						{
							deflateStream.Write(buffer, 0, num2);
							num2 = memoryStream.Read(buffer, 0, num);
						}
						deflateStream.Flush();
					}
					memoryStream2.Flush();
					result = memoryStream2.ToArray();
				}
			}
			return result;
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x0005652C File Offset: 0x0005472C
		public static byte[] Decompress(byte[] data)
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream(data, false))
			{
				using (MemoryStream memoryStream2 = new MemoryStream(data.Length * 10))
				{
					using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress, true))
					{
						int num = 16384;
						byte[] buffer = new byte[num];
						int num2 = deflateStream.Read(buffer, 0, num);
						while (0 < num2)
						{
							memoryStream2.Write(buffer, 0, num2);
							num2 = deflateStream.Read(buffer, 0, num);
						}
						memoryStream2.Flush();
						result = memoryStream2.ToArray();
					}
				}
			}
			return result;
		}

	}
}
