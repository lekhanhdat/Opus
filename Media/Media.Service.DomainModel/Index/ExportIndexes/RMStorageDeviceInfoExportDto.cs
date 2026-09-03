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
using System.Text.RegularExpressions;

namespace AvePoint.Media.Service.DomainModel
{
    [Table("RMStorageDeviceInfoes")]
    public class RMStorageDeviceInfoExportDto : IIndexable
    {
        [Column("Id")]
        public string Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Type")]
        public int Type { get; set; }
        [Column("ModifiedTime")]
        public long ModifiedTime { set; get; }
        [Column("ConnectionString")]
        public string ConnectionString { get; set; }
        [Column("IsSystemStorage")]
        public bool IsSystemStorage { get; set; }

        private const string PASSWORD_PATTERN = "([^&]*)secret=([^&]*)";
        private readonly static Regex r = new Regex(PASSWORD_PATTERN);
        private List<string> password;
        public List<string> Password
        {
            get
            {
                lock (r)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ConnectionString))
                        {
                            Match m = r.Match(ConnectionString);
                            if (m.Success)
                            {
                                password = new List<string>();
                            }
                            while (m.Success)
                            {
                                password.Add("&" + m.Groups[0].Value);
                                m = m.NextMatch();

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message, e);
                    }
                }
                return password;
            }
        }
        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Id", this.Id },
                {"Name", this.Name },
                {"Type", this.Type },
                {"ModifiedTime", this.ModifiedTime },
                {"ConnectionString", this.ConnectionString },
                {"IsSystemStorage", this.IsSystemStorage },
            };
            return dic;
        }
    }
}
