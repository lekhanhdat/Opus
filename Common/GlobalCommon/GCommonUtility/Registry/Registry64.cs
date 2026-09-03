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




namespace AvePoint.GCommon.Utility
{
    #region using directive
    using System;
    using System.Text;
    #endregion

    /// <summary>
    /// Provide the ability of WOW application on a 64 bit OS to access the 64 bit registry.
    /// </summary>
    public class Registry64
    {
        static readonly Int32 KEY_WOW64_64KEY = 0x100;
        static readonly Int32 KEY_WOW64_32KEY = 0x200;
        static readonly Int32 READ_RIGHTS = 131097;
        readonly IntPtr _hKey;

        static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);

        ///<summary>
        /// Initializes a new instance of the <see cref="Registry64"/> class.
        ///</summary>
        ///<param name="hKey">The base HKey</param>
        private Registry64(IntPtr hKey)
        {
            this._hKey = hKey;
        }

        ///<summary>
        /// Initializes a new instance of the <see cref="Registry64"/> class
        /// with HKEY_LOCAL_MACHINE set as the HKey.
        ///</summary>
        public static Registry64 LocalMachine
        {
            get { return new Registry64(HKEY_LOCAL_MACHINE); }
        }

        ///<summary>
        /// Returns the value data for the specified value name under the
        /// given sub key.
        ///</summary>
        ///<param name="subKey">The sub key to look under.</param>
        ///<param name="valueName">The name for the value to retrieve.</param>
        ///<returns>The value data as a string.</returns>
        public String GetValue(String subKey, String valueName)
        {
            var openKey = this.OpenSubKey(subKey);

            try
            {
                var keyValueInfo = this.GetKeyValueInfo(openKey, valueName);
                return this.GetKeyValueData(openKey, keyValueInfo);
            }
            finally
            {
                Win32Native.RegCloseKey(openKey);
            }
        }

        string GetKeyValueData(IntPtr openKey, KeyValueInfo keyValueName)
        {
            var keyValue = new StringBuilder(((Int32)keyValueName.Length) - 1);
            var resultCode = Win32Native.RegQueryValueEx(
                                            openKey,
                                            keyValueName.Name,
                                            IntPtr.Zero,
                                            out keyValueName.Type,
                                            keyValue,
                                            ref keyValueName.Length);
            if (resultCode != 0) this.ThrowException(resultCode);
            return keyValue.ToString();
        }

        KeyValueInfo GetKeyValueInfo(IntPtr openKey, String valueName)
        {
            UInt32 keyType;
            var keyValueLength = 0u;
            var resultCode = Win32Native.RegQueryValueEx(openKey, valueName, IntPtr.Zero, out keyType, null, ref keyValueLength);
            if (resultCode != 0) this.ThrowException(resultCode);
            return new KeyValueInfo(keyType, keyValueLength, valueName);
        }

        IntPtr OpenSubKey(String subKey)
        {
            IntPtr openKey;
            var resultCode = Win32Native.RegOpenKeyEx(this._hKey, subKey, 0, KEY_WOW64_64KEY | READ_RIGHTS, out openKey);
            if (2 == resultCode)
                resultCode = Win32Native.RegOpenKeyEx(this._hKey, subKey, 0, KEY_WOW64_32KEY | READ_RIGHTS, out openKey);
            if (resultCode != 0) this.ThrowException(resultCode);
            return openKey;
        }

        void ThrowException(Int32 errorCode)
        {
            switch (errorCode)
            {
                case 2:
                    throw new InvalidOperationException("Error 2: Key or value name not found.");
                case 3:
                    throw new InvalidOperationException("Error 3: Path not found.");
                case 5:
                    throw new InvalidOperationException("Error 5: Access is denied.");
                case 6:
                    throw new InvalidOperationException("Error 6: Invalid handle");
                case 9:
                    throw new InvalidOperationException("Error 9: Invalid block");
                case 12:
                    throw new InvalidOperationException("Error 12: Invalid Access");
                default:
                    throw new InvalidOperationException("Error " + errorCode + ". Please refer to MSDN documentation on WinError.h for further information.");
            }
        }

        class KeyValueInfo
        {
            public UInt32 Length;
            public String Name;
            public UInt32 Type;

            public KeyValueInfo(UInt32 type, UInt32 length, string name)
            {
                this.Type = type;
                this.Length = length;
                this.Name = name;
            }
        }
    }
}
