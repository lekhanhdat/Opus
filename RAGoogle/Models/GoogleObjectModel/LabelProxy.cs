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
using Google.Apis.Drive.v3.Data;

namespace RAGoogle.Models.GoogleObjectModel
{
    public class LabelProxy : GDriveObjectProxy
    {
        public LabelProxy(Dictionary<string, object> properties) : base(properties)
        {
        }
        public string Id
        {
            get
            {
                return GetProperty<string>("Id");
            }
        }
        public string Kind
        {
            get
            {
                return GetProperty<string>("Kind");
            }
        }
        public string RevisionId
        {
            get
            {
                return GetProperty<string>("RevisionId");
            }
        }
        public Dictionary<string, LabelFieldProxy> Fields
        {
            get
            {
                var rawFields = GetProperty<Dictionary<string, LabelField>>("Fields");

                if (rawFields == null) return null;

                return rawFields.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new LabelFieldProxy(LabelFieldToPropertyDictionary(kvp.Value))
                );
            }
        }
        private Dictionary<string, object> LabelFieldToPropertyDictionary(LabelField field)
        {
            if (field == null)
                return null;

            var dict = new Dictionary<string, object>();

            if (field.DateString != null)
                dict["DateString"] = field.DateString;

            if (!string.IsNullOrEmpty(field.Id))
                dict["Id"] = field.Id;

            if (field.Integer != null)
                dict["Integer"] = field.Integer;

            if (!string.IsNullOrEmpty(field.Kind))
                dict["Kind"] = field.Kind;

            if (field.Selection != null)
                dict["Selection"] = field.Selection;

            if (field.Text != null)
                dict["Text"] = field.Text;

            if (field.User != null)
            {
                var userProxies = field.User
                    .Where(u => u != null)
                    .Select(u => new UserProxy(new Dictionary<string, object>
                    {
                        ["DisplayName"] = u.DisplayName,
                        ["EmailAddress"] = u.EmailAddress,
                        ["PermissionId"] = u.PermissionId
                    }))
                    .ToList();

                dict["User"] = userProxies;
            }

            if (!string.IsNullOrEmpty(field.ValueType))
                dict["ValueType"] = field.ValueType;

            return dict;
        }
    }
}
