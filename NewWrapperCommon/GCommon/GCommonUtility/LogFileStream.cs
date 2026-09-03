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
using System.Text;
using AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object;

namespace AvePoint.GCommon.Utility
{
    public class LogFileStream : FileStream
    {
        #region Members
        private List<LogRetrieveDto> _listDto;
        private StreamReader _reader;
        private byte[] _buffer;
        private int _bufferlength;
        private int _readIndex;
        private readonly int[] _offsets;
        #endregion

        public LogFileStream(string path, FileMode mode, FileAccess access, FileShare share, List<LogRetrieveDto> listDto)
            : base(path, mode, access, share)
        {
            _listDto = listDto;
            _reader = new StreamReader(new FileStream(path, mode, access, share), Encoding.UTF8);
            _offsets = new int[listDto.Count];
        }

        public override int Read(byte[] array, int offset, int count)
        {
            if (count > array.Length)
            {
                count = array.Length;
            }
            for (int i = 0; i < count && offset < count; i++)
            {
                if (_readIndex == _bufferlength)
                {
                    if (!FillBuffer())
                    {
                        return i;
                    }
                    _readIndex = 0;
                }
                array[offset] = _buffer[_readIndex];
                offset++;
                _readIndex++;
            }
            return count;
        }

        private bool FillBuffer()
        {
            string line = _reader.ReadLine();
            if (line == null) return false;
            StringBuilder result = ReplaceString(line);
            result.Append(Environment.NewLine);
            int maxByteCount = result.Length*4;
            if (_buffer == null || _buffer.Length < maxByteCount)
            {
                _buffer = new byte[maxByteCount];
            }
            _bufferlength = Encoding.UTF8.GetBytes(result.ToString(), 0, result.Length, _buffer, 0);
            return true;
        }

        private StringBuilder sBuilder = new StringBuilder(1024 * 256);
        private StringBuilder ReplaceString(string line)
        {
            if (sBuilder.Length != 0)
            {
                sBuilder.Remove(0, sBuilder.Length);
            }
            Array.Clear(_offsets, 0 ,_offsets.Length);
            string lowerLine = line.ToLower(CultureInfo.CurrentCulture);
            int mi = -1;
            int wi = -1;
            int ti = 0;
            while (true)
            {
                mi = -1;
                for (int i = 0; i < _listDto.Count; i++)
                {
                    if (_offsets[i] != -1 && _offsets[i] <= ti)
                    {
                        _offsets[i] = lowerLine.IndexOf(_listDto[i].OldString, ti, StringComparison.Ordinal); 
                    }
                    if (_offsets[i] >= 0 && (_offsets[i] < mi || mi == -1))
                    {
                        mi = _offsets[i];
                        wi = i;
                    }
                }
                if (mi >= 0)
                {
                    if (mi > 0)
                    {
                        sBuilder.Append(line, ti, mi - ti);
                    }
                    sBuilder.Append(_listDto[wi].NewString);
                    ti = (mi + _listDto[wi].OldString.Length);
                }
                else
                {
                    sBuilder.Append(line,ti, line.Length - ti);
                    break;
                }
            }
            return sBuilder;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_reader != null)
            {
                _reader.Dispose();
            }
            _listDto = null;
            _buffer = null;
            _reader = null;
            sBuilder = null;
        }
    }
}
