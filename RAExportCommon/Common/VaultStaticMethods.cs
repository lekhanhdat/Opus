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



using Aspose.Email;
using Aspose.Email.Clients.Exchange.WebService;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Configurations.Bootstrap;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace RAExportCommon
{
    internal static class FullURL
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Dlg")]
        public static string GetItemFullUrl(AveSPListItem item, bool IsDlg = true)
        {
            //add [ && (int)item.AveSPItem.AveSPList.SPList.BaseTemplate!= 550] for ADO-110855
            if (string.IsNullOrEmpty(item.AveSPItem.AveSPList.SPList.DefaultDisplayFormUrl) && (int)item.AveSPItem.AveSPList.SPList.BaseTemplate != 550)
                return string.Empty;

            if (item.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.Posts)
            {
                string itemFullPatch = System.Web.HttpUtility.UrlPathEncode(item.AveSPItem.AveSPList.ParentWeb.SPWeb.Url.TrimEnd('/'))
                    + "/"
                    + item.AveSPItem.AveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(item.AveSPItem.AveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/')
                    + "?";
                itemFullPatch += "List=" + item.AveSPItem.AveSPList.SPList.ID.ToString();
                itemFullPatch += "&ID=" + item.AveSPItem.RowId.ToString();
                itemFullPatch += "&Web=" + item.AveSPItem.AveSPList.ParentWeb.SPWeb.ID.ToString();
                return itemFullPatch;
            }
            else if ((int)item.AveSPItem.AveSPList.SPList.BaseTemplate == 550)
            {
                string fileUrl = item.AveSPItem.AveSPList.SPList.DefaultViewUrl.TrimStart('/').Substring(item.AveSPItem.AveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                return string.Format("{0}/{1}?ID={2}", item.AveSPItem.AveSPList.ParentWeb.SPWeb.Url.TrimEnd('/'), fileUrl, item.AveSPItem.RowId);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                string fileUrl = item.AveSPItem.AveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(item.AveSPItem.AveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');

                sb.Append(System.Web.HttpUtility.UrlPathEncode(item.AveSPItem.AveSPList.ParentWeb.SPWeb.Url.TrimEnd('/')));
                sb.Append("/");
                sb.Append(fileUrl);
                sb.Append("?ID=");
                sb.Append(item.AveSPItem.RowId);
                if (item.AveSPItem.SPListItem.ContentType != null)
                {
                    sb.Append("&ContentTypeId=");
                    sb.Append(item.AveSPItem.SPListItem.ContentType.ID);//to do debug...
                }
                sb.Append("&VersionNo=");
                sb.Append(item.AveSPItem.Version.ToString());
                if (IsDlg)
                {
                    sb.Append("&IsDlg=yes");
                }
                return sb.ToString();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Dlg")]
        public static string GetItemFullUrl(AveSPFolder folder, bool IsDlg = true)
        {
            if (string.IsNullOrEmpty(folder.AveItem.AveSPList.SPList.DefaultDisplayFormUrl))
                return string.Empty;

            string fileUrl = folder.AveItem.AveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(folder.AveItem.AveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
            StringBuilder sb = new StringBuilder();
            sb.Append(System.Web.HttpUtility.UrlPathEncode(folder.AveItem.AveSPList.ParentWeb.SPWeb.Url.TrimEnd('/')));
            sb.Append("/");
            sb.Append(fileUrl);
            sb.Append("?ID=");
            sb.Append(folder.AveItem.RowId);
            if (folder.AveItem.SPListItem != null && folder.AveItem.SPListItem.ContentType != null)
            {
                sb.Append("&ContentTypeId=");
                sb.Append(folder.AveItem.SPListItem.ContentType.ID);//to do debug
            }
            sb.Append("&VersionNo=");
            sb.Append(folder.AveItem.Version.ToString());
            if (IsDlg)
            {
                sb.Append("&IsDlg=yes");
            }
            return sb.ToString();
        }

        public static string GetItemFullUrl(AveSPDoc doc, bool IsDlg = true)
        {
            string docFullPatch = System.Web.HttpUtility.UrlPathEncode(doc.AveSPItem.AveSPList.SPList.ParentWeb.Url) + '/' + doc.AveSPItem.SPListItem.Url;
            if (doc.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.WebPageLibrary
                && doc.AveSPItem.IsVersion)
            {
                docFullPatch += "?PageVersion=" + doc.AveSPItem.Version.ToString();
            }
            return docFullPatch;
        }

        public static string GetItemFullUrl(AveSPAttachment att, bool IsDlg = true)
        {
            int parentId = att.HostListItem.RowId;
            string attachmentName = NameFactory.GetAttachmentname(att.Name);
            //return System.Web.HttpUtility.UrlPathEncode(string.Format("{0}/lists/{1}/attachments/{2}/{3}", att.AveSPItem.AveSPList.ParentWeb.SPWeb.Url, att.AveSPItem.AveSPList.ServerRelativeUrl.Substring(att.AveSPItem.AveSPList.ServerRelativeUrl.LastIndexOf('/')).TrimStart('/'), parentId.ToString(), attachmentName));
            return System.Web.HttpUtility.UrlPathEncode(string.Format("{0}/{1}/attachments/{2}/{3}", att.AveSPItem.AveSPList.ParentWeb.SPWeb.Url, att.AveSPItem.AveSPList.SPList.RootFolder.Url, parentId.ToString(), attachmentName));
        }

        public static string ParseSPUrl(string spUrl, string replaceString)
        {
            string[] specialList = { "://", "//", ":", "/" };
            foreach (string special in specialList)
            {
                if (spUrl.Contains(special))
                {
                    spUrl = spUrl.Replace(special, replaceString);
                }
            }
            return spUrl;
        }

        public static string GetNewsFeedURL(AveSPListItem item, bool IsDlg = true)
        {
            StringBuilder strb = new StringBuilder();
            strb.Append(Path.Combine(item.AveSPWeb.SPWeb.Url, "newsfeed.aspx"));
            strb.Append("?ThreadID=");
            strb.Append(item.AveSPItem.AveSPList.SPList.DefaultDisplayFormUrl);
            strb.Append("?ID=");
            strb.Append(item.AveSPItem.RowId);
            return strb.ToString();
        }
    }

    internal static class VaultCover
    {
        public static string ConverSizeFormat(long ContentSize, ConverSizeType type)
        {
            StringBuilder result = new StringBuilder();
            if (type == ConverSizeType.SPType)
            {
                result = result.AppendFormat("{0}KB", Math.Ceiling(ContentSize / 1024.0));
            }
            else
            {
                if (ContentSize < 1024)
                    result = result.AppendFormat("{0}Bytes", ContentSize);
                else if (ContentSize >= 1024 && ContentSize < 1024 * 1024)
                    result = result.AppendFormat("{0:F}KB", ContentSize / 1024.0);
                else if (ContentSize >= 1024 * 1024 && ContentSize < 1024 * 1024 * 1024)
                    result = result.AppendFormat("{0:F}MB", ContentSize / (1024 * 1024.0));
                else if (ContentSize >= 1024 * 1024 * 1024 && ContentSize < 1024L * 1024 * 1024 * 1024)
                    result = result.AppendFormat("{0:F}GB", ContentSize / (1024 * 1024 * 1024.0));
                else
                    result = result.AppendFormat("{0:F}TB", ContentSize / (1024L * 1024 * 1024 * 1024.0));
            }
            return result.ToString();
        }

        public static DateTime ConverTimeToLocal(DateTime time)
        {
            if (time.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(time, DateTimeKind.Utc).ToLocalTime();
            else if (time.Kind == DateTimeKind.Utc)
                return time.ToLocalTime();
            else
                return time;
        }

        public enum ConverSizeType
        {
            Normal,
            SPType,
        }

        public static string SafeConverObjectToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            else
            {
                return obj.ToString();
            }
        }

        public static Dictionary<string, object> ConverDicFilterOutNullValue(Dictionary<string, object> dic)
        {
            Dictionary<string, object> nDic = new Dictionary<string, object>();
            if (dic == null)
            {
                return nDic;
            }
            foreach (KeyValuePair<string, object> pair in dic)
            {
                if (pair.Value != null)
                {
                    nDic.AddOrReplace(pair.Key, pair.Value);
                }
            }
            return nDic;
        }

        public static string ConverStreamToString(Stream fileStream)
        {
            StringBuilder builder = new StringBuilder();
            var buffer = new byte[64 * 1024];
            var len = 0;
            while ((len = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                builder.Append(Convert.ToBase64String(buffer));
            }
            // 设置当前流的位置为流的开始
            fileStream.Seek(0, SeekOrigin.Begin);
            return builder.ToString();
        }
    }

    internal static class PathValidation
    {
        //private static string pathValidatorExpression = "^[^" + string.Join("", Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))) + "]+$";
        //private static Regex pathValidator = new Regex(pathValidatorExpression, RegexOptions.Compiled);

        //private static string fileNameValidatorExpression = "^[^" + string.Join("", Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]+$";
        //private static Regex fileNameValidator = new Regex(fileNameValidatorExpression, RegexOptions.Compiled);

        //private static string pathCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))) + "]";
        //private static Regex pathCleaner = new Regex(pathCleanerExpression, RegexOptions.Compiled);

        //private static string fileNameCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]";
        //private static Regex fileNameCleaner = new Regex(fileNameCleanerExpression, RegexOptions.Compiled);
        private static Char[] CustomerChars = new Char[] { '!', '#' };
        private static string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + new string(CustomerChars);
        private static Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

        //public static bool ValidatePath(string path)
        //{
        //    return pathValidator.IsMatch(path);
        //}

        //public static bool ValidateFileName(string fileName)
        //{
        //    return fileNameValidator.IsMatch(fileName);
        //}

        //public static string CleanPath(string path)
        //{
        //    return pathCleaner.Replace(path, "");
        //}

        //public static string CleanFileName(string fileName)
        //{
        //    return fileNameCleaner.Replace(fileName, "");
        //}

        public static string ConverSpecialChar(string originalString, string ConvertIllegalCharacterTo)
        {
            //return r.Replace(originalString, new MatchEvaluator(PathValidation.XMLConverText));
            return r.Replace(originalString, ConvertIllegalCharacterTo);
        }

        public static string ConverSpecialChar(string originalString)
        {
            return r.Replace(originalString, new MatchEvaluator(PathValidation.XMLConverText));
        }

        public static string XMLConverText(Match m)
        {
            string str = m.ToString();
            if (str.Equals(":"))
            {
                return "_x003a_";
            }
            else
            {
                return XmlConvert.EncodeName(str);
            }
        }

        public static bool ValidateFileName(string originalString)
        {
            return r.IsMatch(originalString);
        }
    }

    public class ExchangeUtils
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //public static string GetEXOItemLocalFTSFilePath(string jobID, string itemId, ExchangeService service)
        //{
        //    string filePath = Path.Combine(AveEnv.AgentJobFolder, jobID);
        //    try
        //    {
        //        Directory.CreateDirectory(filePath);
        //        var itemResponse = service.ExportItems(GetItemId(itemId), filePath).GetAwaiter().GetResult();
        //        if (itemResponse[0].Result == ServiceResult.Success)
        //        {
        //            //var fileResponse = itemResponse[0] as FileExportItemsResponse;
        //            //if (fileResponse != null) return fileResponse.DataFilePath;
        //        }
        //        filePath = Path.Combine(Path.Combine(AveEnv.AgentJobFolder, jobID), Guid.NewGuid().ToString() + ".fts");
        //        using (var dest = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
        //        {
        //            try
        //            {
        //                using (var source = itemResponse[0].OpenBinaryStream())
        //                {
        //                    source.CopyTo(dest);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Error("Copy data to temp file error, Error: {0}", e.ToString());
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("Export Exchange Item Error, itemId: {0}, Error: {1}", itemId, e.ToString());
        //    }
        //    return filePath;
        //}

        public static string GetEXOItemLocalEMLFilePath(string jobID, string itemId, ExchangeService service)
        {
            string filePath = Path.Combine(AveEnv.AgentJobFolder, jobID);
            try
            {
                Directory.CreateDirectory(filePath);
                filePath = Path.Combine(Path.Combine(AveEnv.AgentJobFolder, jobID), Guid.NewGuid().ToString() + ".eml");
                Item exoItem = Item.Bind(service, new ItemId(itemId)).GetAwaiter().GetResult();
                using (var dest = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    try
                    {
                        dest.Write(exoItem.MimeContent.Content, 0, exoItem.MimeContent.Content.Length);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Write data to temp file error, Error: {0}", e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("GetEXOItemLocalEMLFilePath Error, itemId: {0}, Error: {1}", itemId, e.ToString());
            }
            return filePath;
        }

        public static string GetEXOItemLocalMSGFilePath(string jobID, string itemId, string mailboxUri, ICredentials Credentials)
        {
            string folderPath = GetMSGTempPath(jobID);
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, Guid.NewGuid().ToString() + ".msg");
            try
            {
                IEWSClient client = EWSClient.GetEWSClient(mailboxUri, Credentials);
                MailMessage message = client.FetchMessage(itemId);
                message.Save(filePath, SaveOptions.DefaultMsgUnicode);
            }
            catch (Exception e)
            {
                logger.Error("GetEXOItemLocalMSGFilePath Error, itemId: {0}, Error: {1}", itemId, e.ToString());
                filePath = string.Empty;
            }
            return filePath;
        }

        private static string GetMSGTempPath(string jobId)
        {
            var separator = Path.DirectorySeparatorChar;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { separator }).LastIndexOf(separator.ToString()));
            return Path.Combine(basePath, "Temp", jobId);
        }

        public static string GetEXOItemLocalMSGFilePath(string jobID, string itemId, ExchangeService service)
        {
            string filePath = GetMSGTempPath(jobID);
            string emlFilePath = string.Empty;
            try
            {
                Directory.CreateDirectory(filePath);
                emlFilePath = Path.Combine(filePath, Guid.NewGuid().ToString() + ".eml");
                PropertySet propertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.MimeContent);
                Item exoItem = Item.Bind(service, new ItemId(itemId), propertySet).GetAwaiter().GetResult();
                using (var dest = new FileStream(emlFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    try
                    {
                        dest.Write(exoItem.MimeContent.Content, 0, exoItem.MimeContent.Content.Length);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Write data to temp file error, Error: {0}", e.ToString());
                    }
                }
                filePath = ConvertEmlFileToMsgFile(emlFilePath);
            }
            catch (Exception e)
            {
                logger.Error("GetEXOItemLocalEMLFilePath Error, itemId: {0}, Error: {1}", itemId, e.ToString());
                filePath = string.Empty;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(emlFilePath))
                {
                    try
                    {
                        File.Delete(emlFilePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while deleting eml file. Path:{emlFilePath} Error:{ex.ToString()}");
                    }
                }
            }
            return filePath;
        }
        public static async Task<string> GetEXOItemLocalMSGFilePath(string jobID, IExchangeItem item)
        {
            string filePath = GetMSGTempPath(jobID);
            string emlFilePath = string.Empty;
            try
            {
                Directory.CreateDirectory(filePath);
                emlFilePath = Path.Combine(filePath, Guid.NewGuid().ToString() + ".eml");
                using (var dest = new FileStream(emlFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    try
                    {
                        var result = await item.GetMimeContentAsync();
                        await result.CopyToAsync(dest);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Write data to temp file error, Error: {0}", e.ToString());
                    }
                }
                filePath = ConvertEmlFileToMsgFile(emlFilePath);
            }
            catch (Exception e)
            {
                logger.Error("GetEXOItemLocalEMLFilePath Error, itemId: {0}, Error: {1}", item.ItemId, e.ToString());
                filePath = string.Empty;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(emlFilePath))
                {
                    try
                    {
                        File.Delete(emlFilePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while deleting eml file. Path:{emlFilePath} Error:{ex.ToString()}");
                    }
                }
            }
            return filePath;
        }

        public static string ConvertEmlFileToMsgFile(string emlFilePath)
        {
            string msgFilePath = emlFilePath.Substring(0, emlFilePath.LastIndexOf('.')) + ".msg";
            MailMessage eml = MailMessage.Load(emlFilePath);
            try
            {
                RegisterLicense();

                eml.Save(msgFilePath, SaveOptions.DefaultMsgUnicode);
            }
            catch (Exception e)
            {
                logger.Error("ConvertEmlFileToMsgFile Error, emlFilePath: {0}, Error: {1}", emlFilePath, e.ToString());
                msgFilePath = string.Empty;
            }
            finally
            {
                eml.Dispose();
            }
            return msgFilePath;
        }

        public static string CreateEncryptedFile2Local(Stream inputStream, string jobId, byte[] encryptionKey, byte[] encryptionIV)
        {
            var folderPath = Path.Combine(AveEnv.AgentJobFolder, jobId);
            string filePath = Path.Combine(folderPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (FileStream fout = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fout.SetLength(0);
                //Create variables to help with read and write.
                byte[] bin = new byte[1024]; //This is intermediate storage for the encryption.
                long rdlen = 0;              //This is the total number of bytes written.
                long totlen = inputStream.Length;    //This is the total length of the input file.
                int len;                     //This is the number of bytes to be written at a time.
                inputStream.Position = 0;
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (CryptoStream encStream = new CryptoStream(fout, aes.CreateEncryptor(encryptionKey, encryptionIV), CryptoStreamMode.Write))
                    {

                        //Read from the input file, then encrypt and write to the output file.
                        while (rdlen < totlen)
                        {
                            len = inputStream.Read(bin, 0, 1024);
                            encStream.Write(bin, 0, len);
                            rdlen = rdlen + len;
                        }
                    }
                }
            }
            return filePath;
        }

        public static IEnumerable<ItemId> GetItemId(string id)
        {
            yield return new ItemId(id);
        }

        public static string ConvertMailTypeToDisplayType(string itemType)
        {
            switch (itemType)
            {
                case "IPM.Note":
                    return "Message";
                case "IPM.Task":
                    return "Task";
                case "IPM.Post":
                    return "Post";
                case "IPM.Appointment":
                    return "Event";
                case "IPM.Activity":
                    return "Journal";
                case "IPM.StickyNote":
                    return "Note";
                case "IPM.Contact":
                    return "Contact";
                case "IPM.Document":
                    return "Document";
                default:
                    return "Message";
            }
        }

        public static string EmailAddressToFormatString(EmailAddress address)
        {
            if (string.IsNullOrEmpty(address.Address)) return address.Name;
            if (string.IsNullOrEmpty(address.Name)) return address.Address;
            return string.Format("{0} <{1}>", address.Name, address.Address);
        }

        public static string ReplaceSpecicalCharactersToUnderline(string folderName)
        {
            string returnFolderName = string.Empty;
            try
            {
                string reg = @"\:" + @"|\/" + @"|\\" + @"|\|" + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";//特殊字符
                Regex r = new Regex(reg);
                returnFolderName = r.Replace(folderName, "_");
                //logger.Info("Replace Special Characters To Underline while Merge VEO job, SourceFolderName:{0}, ConvertFolderName:{1}.", folderName, returnFolderName);
            }
            catch (Exception ex)
            {
                returnFolderName = folderName;
                logger.Warn("Can not Replace Special Characters To Underline while Merge VEO job, Message:{0}.", ex.ToString());
            }
            return returnFolderName;
        }

        public static string ConvertByteToKB(string strByte)
        {
            string kb = "0";
            if (string.IsNullOrEmpty(strByte))
            {
                return kb;
            }
            double i;
            bool b = double.TryParse(strByte, out i);
            if (b)
            {
                double dkb = i / 1024;
                if (dkb > (int)dkb)
                {
                    dkb++;
                }
                int value = (int)dkb;
                kb = value.ToString();
            }
            return kb;
        }

        public static Stream GetEXOItemLocalMSGFileStream(string filePath)
        {
            try
            {
                var ms = new MemoryStream();
                using (Stream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    docStream.CopyTo(ms);
                }
                ms.Position = 0; 
                return ms;
            }
            catch (Exception ex)
            {
                logger.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while get EXOItem local (.msg) file stream content.", filePath, ex.Message);
                return new MemoryStream();
            }
        }

        private static void RegisterLicense()
        {
            AsposeLicenseBootstrap.Setup();
        }
    }

    public class HashCodeMd5Helper
    {
        public static string HashCodeMD5(byte[] value)
        {
            if (CryptographyManagement.CryptoMode == CryptoMode.FIPS)
            {
                IHashAlgorithm md5 = HashAlgorithmFactory.CreateHashAlgorithm(AvePoint.GCommon.Utility.Cryptography.HashAlgorithm.MD5);
                byte[] md5ByteArray = md5.ComputeHash(value);
                return BitConverter.ToString(md5ByteArray).Replace("-", "").ToLowerInvariant();
            }
            else
            {
                using (var hashAlgorithm = System.Security.Cryptography.HashAlgorithm.Create("MD5"))
                {
                    hashAlgorithm.Initialize();
                    var hashByteArray = hashAlgorithm.ComputeHash(value);
                    return BitConverter.ToString(hashByteArray).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
