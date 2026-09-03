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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using AvePoint.GCommon;

namespace StorageTable
{
    public class TableService
    {

        private static AveLogger logger = AveLogger.GetInstance(typeof(TableService));

        private string account;
        private string accessKey;

        public TableService(string account, string accessKey)
        {
            this.account = account;
            this.accessKey = accessKey;
        }

        public void CreateTable(string tableName)
        {
            string canonicalizedResource = String.Format("/{0}/{1}", account, AzureStorageConstants.CResource_Table);
            HttpWebRequest request = GetAzureRequest(string.Format(AzureStorageConstants.TableURI, account), canonicalizedResource);
            string body = CreateTableBody(tableName);
            DoSendRequest(request, body);
        }

        public int AddEntity(string table, LogEntry item)
        {
            int result = 0;
            //string canonicalizedResource = String.Format("/{0}/{1}(PartitionKey=\'{2}\',RowKey=\'{3}\')", account, table, item.PartitionKey, item.RowKey);
            //string uri = string.Format(AzureStorageConstants.EntryURI2, account, table,item.PartitionKey,item.RowKey);

            string canonicalizedResource = String.Format("/{0}/{1}", account, table);
            string uri = string.Format(AzureStorageConstants.EntryURI1, account, table);
            HttpWebRequest request = GetAzureRequest(uri, canonicalizedResource);
            string body = this.CreateEntryBody(item);
            DoSendRequest(request, body);
            return result;
        }

        public int BatchCommitEnties(string table, IEnumerable<LogEntry> items)
        {
            int index = 0;
            IEnumerable<string> bodies = this.CreateEntriesBody(items);
            string canonicalizedResource = String.Format("/{0}/$batch", account);
            string uri = string.Format(AzureStorageConstants.EntryURIBATCH, account);
            while (true)
            {
                IEnumerable<string> oneBatchBodies = bodies.Skip(index * 100).Take(100);
                if (oneBatchBodies.Count() == 0)
                {
                    break;
                }
                HttpWebRequest request = GetAzureRequest(uri, canonicalizedResource);
                request.ContentType = "multipart/mixed; boundary=" + AzureStorageConstants.BatchBoundary;
                DoSendRequest(request, oneBatchBodies, table);
                index++;
            }
            return 0;
        }

        public bool ValidateAzureStorageCredential()
        {
            string canonicalizedResource = String.Format("/{0}/Tables()", account);
            string uri = string.Format(AzureStorageConstants.EntryURIValidate, account);
            HttpWebRequest request = GetAzureRequest(uri, canonicalizedResource);
            request.Method = AzureStorageConstants.GET;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        protected HttpWebRequest GetAzureRequest(string uri, string canonicalizedResource)
        {
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(uri);

            Request.Method = AzureStorageConstants.POST;

            string time = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);


            string stringToSign = String.Format("{0}\n{1}", time, canonicalizedResource);
            Request.Headers.Add("Authorization", CreateAuthorizationHeader(stringToSign));

            Request.Headers.Add("x-ms-date", time);
            Request.Headers.Add("x-ms-version", "2009-09-19");
            Request.ContentType = "application/atom+xml";
            Request.Accept = ("application/atom+xml,application/xml");
            Request.Headers.Add("Accept-Charset", "UTF-8");
            Request.Headers.Add("DataServiceVersion", "1.0;NetFx");
            Request.Headers.Add("MaxDataServiceVersion", "1.0;NetFx");

            return Request;
        }

        private void DoSendRequest(HttpWebRequest request, string body)
        {

            try
            {
                request.ContentLength = body.Length;
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                Byte[] bBody = enc.GetBytes(body);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bBody, 0, bBody.Length);
                    requestStream.Close();
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // Check that (response.StatusCode == HttpStatusCode.Created)
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(stream);

                                stream.Close();
                                response.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Falied to send web Request.Error message : {e.Message}");
            }
        }

        private void DoSendRequest(HttpWebRequest request, IEnumerable<string> bodies, string table)
        {
            try
            {
                byte[] tempBuffer;
                using (MemoryStream memStream = AssembleMemoryStream(bodies, table))
                {
                    request.ContentLength = memStream.Length;
                    memStream.Position = 0;
                    tempBuffer = new byte[memStream.Length];
                    memStream.ReadEx(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                }

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                    requestStream.Close();
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // Check that (response.StatusCode == HttpStatusCode.Created)
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(stream);

                                stream.Close();
                                response.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        private MemoryStream AssembleMemoryStream(IEnumerable<string> bodies, string table)
        {
            string begin_batch = string.Format("--{0}\r\nContent-Type: multipart/mixed; boundary={1}\r\n", AzureStorageConstants.BatchBoundary, AzureStorageConstants.ChangesetBoundary);
            string begin_changeset = string.Format("\r\n--{0}\r\nContent-Type: application/http\r\nContent-Transfer-Encoding: binary\r\n\r\n", AzureStorageConstants.ChangesetBoundary);
            string end_batch = string.Format("--{0}--\r\n", AzureStorageConstants.BatchBoundary);
            string end_changeset = string.Format("\r\n--{0}--\r\n", AzureStorageConstants.ChangesetBoundary);
            string body_head = AzureStorageConstants.POST + " " + string.Format(AzureStorageConstants.EntryURI1, account, table) + " HTTP/1.1\r\nContent-ID: {0}\r\nContent-Type: application/atom+xml;type=entry\r\nContent-Length: {1}\r\n\r\n";
            MemoryStream stream = new MemoryStream();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] begin_batch_bytes = enc.GetBytes(begin_batch);
            stream.Write(begin_batch_bytes, 0, begin_batch_bytes.Length);
            byte[] begin_changeset_bytes = enc.GetBytes(begin_changeset);
            int index = 1;
            foreach (var body in bodies)
            {
                stream.Write(begin_changeset_bytes, 0, begin_changeset_bytes.Length);
                byte[] body_head_bytes = enc.GetBytes(string.Format(body_head, index, body.Length));
                stream.Write(body_head_bytes, 0, body_head_bytes.Length);
                byte[] body_bytes = enc.GetBytes(body);
                stream.Write(body_bytes, 0, body_bytes.Length);
                index++;
            }
            byte[] end_changeset_bytes = enc.GetBytes(end_changeset);
            stream.Write(end_changeset_bytes, 0, end_changeset_bytes.Length);
            byte[] end_batch_bytes = enc.GetBytes(end_batch);
            stream.Write(end_batch_bytes, 0, end_batch_bytes.Length);
            return stream;
        }

        private String CreateAuthorizationHeader(String canonicalizedString)
        {
            String signature = string.Empty;

            using (HMACSHA256 hmacSha256 = new HMACSHA256(Convert.FromBase64String(accessKey)))
            {
                Byte[] dataToHmac = System.Text.Encoding.UTF8.GetBytes(canonicalizedString);
                signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }

            String authorizationHeader = String.Format(
                  CultureInfo.InvariantCulture,
                  "{0} {1}:{2}",
                  AzureStorageConstants.SharedKeyLite,
                  account,
                  signature);

            return authorizationHeader;
        }

        private string CreateTableBody(string tableName)
        {
            string body =
            "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
            "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" " +
            "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" " +
            "xmlns=\"http://www.w3.org/2005/Atom\">";

            body += "<title/>" +
                "<updated>" + String.Format("{0:o}", DateTime.UtcNow) + "</updated>";
            body += "<author>" +
                  "<name/>" +
                "</author>" +
                "<id/>" +
                "<content type=\"application/xml\">" +
                  "<m:properties>" +
                    "<d:TableName>" + tableName + "</d:TableName>" +
                  "</m:properties>" +
                "</content>" +
              "</entry>";

            return body;
        }

        private string CreateEntryBody(LogEntry item)
        {
            string body =
            "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
            "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" " +
            "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" " +
            "xmlns=\"http://www.w3.org/2005/Atom\">";
            body += "<title/><updated>" + string.Format("{0:o}", DateTime.UtcNow) + "</updated>";
            body += "<author>" +
            "<name/>" +
            "</author>" +
            "<id/>" +
            "<content type=\"application/xml\">" +
                "<m:properties>" +
                    "<d:PartitionKey>" + item.PartitionKey + "</d:PartitionKey>" +
                    "<d:RowKey>" + item.RowKey + "</d:RowKey>" +
                    "<d:Level>" + item.Level + "</d:Level>" +
                    "<d:Time>" + item.Time + "</d:Time>" +
                    "<d:Thread>" + HttpUtility.HtmlEncode(item.Thread) + "</d:Thread>" +
                    "<d:Logger>" + HttpUtility.HtmlEncode(item.LoggerName) + "</d:Logger>" +
                    "<d:EventID>" + item.EventID + "</d:EventID>" +
                    "<d:Message>" + HttpUtility.HtmlEncode(item.Message) + "</d:Message>" +
                    "<d:Timestamp m:type=\"Edm.DateTime\">0001-01-01T00:00:00</d:Timestamp>" +
                "</m:properties>" +
            "</content>" +
            "</entry>";

            return body;
        }

        private List<string> CreateEntriesBody(IEnumerable<LogEntry> items)
        {
            List<string> bodies = new List<string>();
            foreach (LogEntry item in items)
            {
                bodies.Add(CreateEntryBody(item));
            }
            return bodies;
        }


    }
}
