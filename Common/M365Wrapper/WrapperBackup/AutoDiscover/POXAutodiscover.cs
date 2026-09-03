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

using Microsoft.Exchange.WebServices.Autodiscover;
using Microsoft.Exchange.WebServices.Data;

using AvePoint.RA.CommonUtil;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ExchangeUtility.Graph
{
    public class POXAutodiscoverService
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(DecoratableRetryController));
        public POXCredential Credentials { get; set; }

        public int Timeout { get; set; }
        public string UserAgent { get; set; }
        public string ServiceUrl { get; set; }
        public const string PreferredServiceUrl = "https://autodiscover-s.outlook.com/autodiscover/autodiscover.svc";
        public ExchangeVersion EXCHANGE_VERSION { get; set; }
        public bool EnableScpLookup { get; set; }

        public List<AlternativeMailbox> GetAlternativeMailboxs(string emailAddress)
        {
            return Credentials.IsAppProfile ? GetAlternateMailboxByAutoDiscoverService(emailAddress) : GetAlternativeMailboxsByPoxService(emailAddress);
        }

        public List<AlternativeMailbox> GetAlternativeMailboxsByPoxService(string emailAddress)
        {
            var resultXML = SendRequest(emailAddress);
            var result = ConvertReultXML2Result(resultXML);
            return result;
        }

        private List<AlternativeMailbox> GetAlternateMailboxByAutoDiscoverService(string mailboxAddress)
        {
            List<AlternativeMailbox> alternativeMailboxes = new List<AlternativeMailbox>();
            try
            {
                var autuDiscoverService = new AutodiscoverService(EXCHANGE_VERSION);
                autuDiscoverService.Credentials = GetAuthorization4AutoDiscoverService();

                autuDiscoverService.EnableScpLookup = this.EnableScpLookup;
                autuDiscoverService.Timeout = Timeout;
                autuDiscoverService.Url = new Uri(PreferredServiceUrl);
                autuDiscoverService.RedirectionUrlValidationCallback = x => true;
                autuDiscoverService.PreAuthenticate = true;
                //autuDiscoverService.TraceEnabled = true;
                autuDiscoverService.KeepAlive = false;
                GetUserSettingsResponse setting;
                try
                {
                    setting = autuDiscoverService.GetUserSettings(mailboxAddress, new UserSettingName[1] { UserSettingName.AlternateMailboxes }).ExecuteAsyncTask();
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while getting alternative mailbxoxs, will try again with default URL. Error : {0}", ex.ToString());
                    autuDiscoverService.Url = null;
                    setting = autuDiscoverService.GetUserSettings(mailboxAddress, new UserSettingName[1] { UserSettingName.AlternateMailboxes }).ExecuteAsyncTask();
                }
                logger.Info("Get user setting successfully, service URL: {0}", autuDiscoverService.Url?.ToString());
                if (setting.Settings.Count > 0)
                {
                    AlternateMailboxCollection result;
                    setting.TryGetSettingValue<AlternateMailboxCollection>(UserSettingName.AlternateMailboxes, out result);
                    if (result != null)
                    {
                        result.Entries.ForEach(alternateMailbox => alternativeMailboxes.Add(
                            new AlternativeMailbox()
                            {
                                DisplayName = alternateMailbox.DisplayName,
                                OwnerSmtpAddress = alternateMailbox.OwnerSmtpAddress,
                                Type = alternateMailbox.Type,
                                SmtpAddress = string.Format("ExchangeGUID+{0}", alternateMailbox.Server)
                            }
                            ));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when get alternate mailbox info. {0}", ex);
            }
            return alternativeMailboxes;
        }

        private List<AlternativeMailbox> ConvertReultXML2Result(string resultXML)
        {
            List<AlternativeMailbox> list = new List<AlternativeMailbox>();
            var xdoc = new XmlDocument();
            xdoc.LoadXml(resultXML);
            foreach (XmlNode node in xdoc.GetElementsByTagName("AlternativeMailbox"))
            {
                var obj = new AlternativeMailbox();
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name.Equals("Type")) obj.Type = child.InnerText;
                    if (child.Name.Equals("DisplayName")) obj.DisplayName = child.InnerText;
                    if (child.Name.Equals("SmtpAddress")) obj.SmtpAddress = child.InnerText;
                    if (child.Name.Equals("OwnerSmtpAddress")) obj.OwnerSmtpAddress = child.InnerText;
                }
                list.Add(obj);
            }
            return list;
        }
        private string SendRequest(string emailAddress)
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://outlook.office365.com/autodiscover/autodiscover.xml"))
                {
                    var postObj = new Autodiscover() { Request = new Request { EMailAddress = emailAddress } };
                    var xmls = SerializeToXml<Autodiscover>(postObj);
                    request.Content = GetXMLContent(xmls);
                    request.Headers.Authorization = GetAuthorization();
                    client.DefaultRequestHeaders.Add("X-AutoDiscoverArchiveAsSmtp", "true");
                    using (var response = client.SendAsync(request).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            logger.Info("POX Auto Discover Service, Success Result:{0}", result);
                            logger.Info("POX Auto Discover Service, Success Respose: {0}", response.ToString());
                        }
                        else
                        {
                            var errorString = response.Content?.ReadAsStringAsync().Result;
                            logger.Info("POX Auto Discover Service, Failed Result:{0}", errorString);
                            logger.Info("POX Auto Discover Service, Failed Respose: {0}", response.ToString());
                            throw new Exception(errorString);
                        }
                    }
                }
            }
            return result;
        }

        private AuthenticationHeaderValue GetAuthorization()
        {
            AuthenticationHeaderValue authentication;
            if (this.Credentials.IsAppProfile)
            {
                authentication = new AuthenticationHeaderValue("Bearer", this.Credentials.AccessToken);
            }
            else
            {
                var userToken = ToBase64String(this.Credentials.UserName + ":" + this.Credentials.Password);
                authentication = new AuthenticationHeaderValue("Basic", userToken);
            }
            return authentication;
        }

        private ExchangeCredentials GetAuthorization4AutoDiscoverService()
        {
            ExchangeCredentials exchangeCredentials;
            if (this.Credentials.IsAppProfile)
            {
                exchangeCredentials = new OAuthCredentials(this.Credentials.AccessToken);
            }
            else
            {
                exchangeCredentials = new NetworkCredential(this.Credentials.UserName, this.Credentials.Password);
            }
            return exchangeCredentials;
        }

        private HttpContent GetXMLContent(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return null;
            var content = new StringContent(xml);
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            return content;
        }

        private static string ToBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }
        private static string UnBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        private static string SerializeToXml<T>(T myObject) where T : class
        {
            if (myObject != default(T))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));

                MemoryStream stream = new MemoryStream();
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.None;
                xs.Serialize(writer, myObject);

                stream.Position = 0;
                StringBuilder sb = new StringBuilder();
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                    reader.Close();
                }
                writer.Close();
                return sb.ToString();
            }
            return string.Empty;
        }
    }

    public class POXCredential
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public bool IsAppProfile { get; set; }

        public POXCredential(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }

        public POXCredential(string accessToken)
        {
            this.AccessToken = accessToken;
            this.IsAppProfile = true;
        }

    }
    public class AlternativeMailbox
    {
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public string SmtpAddress { get; set; }
        public string OwnerSmtpAddress { get; set; }
    }

    [XmlRoot(ElementName = "Autodiscover", Namespace = "http://schemas.microsoft.com/exchange/autodiscover/outlook/requestschema/2006")]
    public class Autodiscover
    {
        [XmlElement(ElementName = "Request")]
        public Request Request { get; set; }
    }

    public class Request
    {
        [XmlElement(ElementName = "EMailAddress")]
        public string EMailAddress { get; set; }

        [XmlElement(ElementName = "AcceptableResponseSchema")]
        public string AcceptableResponseSchema { get; set; } = "http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a";
    }


}