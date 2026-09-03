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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.MonitorTool
{
    public partial class MonitorLogDecryption : Form
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(MonitorLogDecryption));
        private string filePath;
        private string tmpString;
        private bool isDecryptedFinish;
        private object locker = new object();
        public delegate void ShowData();

        public MonitorLogDecryption()
        {
            InitializeComponent();
        }

        private void Btn_SelectFile_Click(object sender, EventArgs e)
        {
            Ofd_OpenFileDialog.ShowDialog();
        }

        private void Ofd_OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            Txb_FilePath.Text = Ofd_OpenFileDialog.FileName;
            filePath = Txb_FilePath.Text;
        }

        private void Tsm_WordWrap_Click(object sender, EventArgs e)
        {
            if (Rtb_DecryptedFileContent.WordWrap)
            {
                Tsm_WordWrap.Image = Wrapper.MonitorTool.Properties.Resources.btm_WordWrap_Off;
                Rtb_DecryptedFileContent.WordWrap = false;
            }
            else
            {
                Tsm_WordWrap.Image = Wrapper.MonitorTool.Properties.Resources.btm_WordWrap_On;
                Rtb_DecryptedFileContent.WordWrap = true;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DecryptedContentTyper is a key")]
        private void Btn_DecryptFile_Click(object sender, EventArgs e)
        {
            Rtb_DecryptedFileContent.Text = string.Empty;
            Thread typer = new Thread(ContentTyper);
            typer.IsBackground = true;
            typer.Name = "DecryptedContentTyper";
            typer.Start();
        }

        private void ContentTyper()
        {
            try
            {
                isDecryptedFinish = false;
                Btn_DecryptFile.Invoke(new ShowData(ChangeBtn_DecryptFileStatus));
                tmpString = string.Empty;
                lock (locker)
                {
                    FileInfo logFile = new FileInfo(filePath);
                    if (logFile.Exists)
                    {
                        StreamReader sw = new StreamReader(filePath, Encoding.Default);
                        string conecnt = sw.ReadLine();
                        while (conecnt != null)
                        {
                            tmpString = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(conecnt));

                            Rtb_DecryptedFileContent.Invoke(new ShowData(ShowContent));
                            conecnt = sw.ReadLine();
                        }
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, ex.ToString());
            }
            finally
            {
                isDecryptedFinish = true;
                Btn_DecryptFile.Invoke(new ShowData(ChangeBtn_DecryptFileStatus));
            }
        }

        private void ShowContent()
        {
            Rtb_DecryptedFileContent.AppendText(tmpString + "\n");
        }

        private void ChangeBtn_DecryptFileStatus()
        {
            Btn_DecryptFile.Enabled = isDecryptedFinish;
            Btn_Clean.Enabled = isDecryptedFinish;
            Btn_ExportToFile.Enabled = isDecryptedFinish;
        }

        private void Btn_Clean_Click(object sender, EventArgs e)
        {
            Rtb_DecryptedFileContent.Text = string.Empty;
        }

        private void Btn_ExportToFile_Click(object sender, EventArgs e)
        {
            Thread exportThread = new Thread(ExportContentToFile);
            exportThread.IsBackground = true;
            exportThread.Name = "ExportContentToFile";
            exportThread.Start();
        }

        private void ExportContentToFile()
        {
            try
            {
                isDecryptedFinish = false;
                Btn_ExportToFile.Invoke(new ShowData(ChangeBtn_DecryptFileStatus));
                lock (locker)
                {
                    FileInfo logFile = new FileInfo(filePath);
                    string exportFile = filePath + ".txt";
                    if (logFile.Exists)
                    {
                        if (!File.Exists(exportFile))
                        {
                            using (FileStream fs = new FileStream(exportFile, FileMode.Create))
                            {
                                fs.Close();
                            }
                        }
                        StreamReader sr = new StreamReader(filePath, Encoding.Default);
                        string conecnt = sr.ReadLine();
                        StreamWriter sw = new StreamWriter(exportFile, false, Encoding.Default);
                        while (conecnt != null)
                        {
                            tmpString = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(conecnt));
                            sw.WriteLine(tmpString);
                            conecnt = sr.ReadLine();
                        }
                        sr.Close();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, ex.ToString());
            }
            finally
            {
                isDecryptedFinish = true;
                Btn_ExportToFile.Invoke(new ShowData(ChangeBtn_DecryptFileStatus));
            }
        }
    }
}
