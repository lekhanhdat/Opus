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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    #endregion

    /// <summary>
    /// This class is used to implemented a text listener which can be rolling back
    /// </summary>
    //public class AveRollingTraceListener : TraceListener
    //{
    //    String fileName;
    //    Int32 maxFileSize { get; set; }
    //    StreamWriter streamWriter;

    //    /// <summary>
    //    /// To construct a AveRollingTraceListener obj
    //    /// </summary>
    //    /// <param name="initializeData"></param>
    //    public AveRollingTraceListener(String initializeData)
    //    {
    //        var data = initializeData.Split(new Char[] { ';' });
    //        this.fileName = data[0];
    //        this.maxFileSize = Int32.Parse(data[1]);
    //        string dir = Path.GetDirectoryName(this.fileName);
    //        if (!Directory.Exists(dir))
    //        {
    //            Directory.CreateDirectory(dir);
    //        }
    //    }

    //    /// <summary>
    //    /// Write a string line message to this listener
    //    /// </summary>
    //    /// <param name="message">message will be written to the listener</param>
    //    public override void WriteLine(String message)
    //    {
    //        this.EnsureStreamWriter();
    //        if (this.NeedIndent)
    //            this.WriteIndent();
    //        this.streamWriter.WriteLine(message);
    //        this.NeedIndent = true;
    //        this.CheckRolling();
    //    }

    //    /// <summary>
    //    /// Write a string message to this listener
    //    /// </summary>
    //    /// <param name="message">message will be written to the listener</param>
    //    public override void Write(string message)
    //    {
    //        this.EnsureStreamWriter();
    //        if (this.NeedIndent)
    //            this.WriteIndent();
    //        this.streamWriter.Write(message);
    //    }

    //    /// <summary>
    //    /// Override the base dispose method to hook the dispose model
    //    /// </summary>
    //    /// <param name="disposing">to identify if dispose from user code, true yes, otherwise not</param>
    //    protected override void Dispose(bool disposing)
    //    {
    //        this.DisposeStreamWriter();
    //        base.Dispose(disposing);
    //    }

    //    void EnsureStreamWriter()
    //    {
    //        if (this.streamWriter == null)
    //            this.streamWriter = new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate));
    //    }

    //    void DisposeStreamWriter()
    //    {
    //        if (this.streamWriter != null)
    //        {
    //            this.streamWriter.Dispose();
    //            this.streamWriter = null;
    //        }
    //    }

    //    void CheckRolling()
    //    {
    //        this.EnsureStreamWriter();
    //        if (this.streamWriter.BaseStream.Length > this.maxFileSize)
    //        {
    //            DisposeStreamWriter();
    //            var rollingFile = fileName + ".bak";
    //            if (File.Exists(rollingFile))
    //                File.Delete(rollingFile);
    //            File.Move(fileName, rollingFile);
    //        }
    //    }
    //}
}
