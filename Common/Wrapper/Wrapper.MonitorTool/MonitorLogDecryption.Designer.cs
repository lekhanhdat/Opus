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



using System.Diagnostics.CodeAnalysis;
namespace AvePoint.Wrapper.MonitorTool
{
    partial class MonitorLogDecryption
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of keys")]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonitorLogDecryption));
            this.Ofd_OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.Lbl_FilePath = new System.Windows.Forms.Label();
            this.Txb_FilePath = new System.Windows.Forms.TextBox();
            this.Btn_SelectFile = new System.Windows.Forms.Button();
            this.Btn_DecryptFile = new System.Windows.Forms.Button();
            this.Btn_Clean = new System.Windows.Forms.Button();
            this.Tsp_TooBar = new System.Windows.Forms.ToolStrip();
            this.Tss_ViewButton = new System.Windows.Forms.ToolStripSplitButton();
            this.Tsm_WordWrap = new System.Windows.Forms.ToolStripMenuItem();
            this.Tss_HelpButton = new System.Windows.Forms.ToolStripSplitButton();
            this.Tsm_HowToUse = new System.Windows.Forms.ToolStripMenuItem();
            this.Tsm_AboutDocAve = new System.Windows.Forms.ToolStripMenuItem();
            this.Rtb_DecryptedFileContent = new System.Windows.Forms.RichTextBox();
            this.Btn_ExportToFile = new System.Windows.Forms.Button();
            this.Tsp_TooBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ofd_OpenFileDialog
            // 
            this.Ofd_OpenFileDialog.Filter = "DocAve Encrypted Files|*.dat|All Files|*.*";
            this.Ofd_OpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.Ofd_OpenFileDialog_FileOk);
            // 
            // Lbl_FilePath
            // 
            this.Lbl_FilePath.AutoSize = true;
            this.Lbl_FilePath.Location = new System.Drawing.Point(24, 47);
            this.Lbl_FilePath.Name = "Lbl_FilePath";
            this.Lbl_FilePath.Size = new System.Drawing.Size(143, 17);
            this.Lbl_FilePath.TabIndex = 0;
            this.Lbl_FilePath.Text = "Encrypted File Path : ";
            // 
            // Txb_FilePath
            // 
            this.Txb_FilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Txb_FilePath.Location = new System.Drawing.Point(173, 48);
            this.Txb_FilePath.Name = "Txb_FilePath";
            this.Txb_FilePath.Size = new System.Drawing.Size(615, 22);
            this.Txb_FilePath.TabIndex = 1;
            this.Txb_FilePath.Tag = "";
            // 
            // Btn_SelectFile
            // 
            this.Btn_SelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_SelectFile.Location = new System.Drawing.Point(815, 47);
            this.Btn_SelectFile.Name = "Btn_SelectFile";
            this.Btn_SelectFile.Size = new System.Drawing.Size(102, 23);
            this.Btn_SelectFile.TabIndex = 2;
            this.Btn_SelectFile.Tag = "";
            this.Btn_SelectFile.Text = "Select File";
            this.Btn_SelectFile.UseVisualStyleBackColor = true;
            this.Btn_SelectFile.Click += new System.EventHandler(this.Btn_SelectFile_Click);
            // 
            // Btn_DecryptFile
            // 
            this.Btn_DecryptFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_DecryptFile.Location = new System.Drawing.Point(815, 85);
            this.Btn_DecryptFile.Name = "Btn_DecryptFile";
            this.Btn_DecryptFile.Size = new System.Drawing.Size(102, 23);
            this.Btn_DecryptFile.TabIndex = 3;
            this.Btn_DecryptFile.Tag = "";
            this.Btn_DecryptFile.Text = "Decrypt File";
            this.Btn_DecryptFile.UseVisualStyleBackColor = true;
            this.Btn_DecryptFile.Click += new System.EventHandler(this.Btn_DecryptFile_Click);
            // 
            // Btn_Clean
            // 
            this.Btn_Clean.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Clean.Location = new System.Drawing.Point(704, 85);
            this.Btn_Clean.Name = "Btn_Clean";
            this.Btn_Clean.Size = new System.Drawing.Size(84, 23);
            this.Btn_Clean.TabIndex = 4;
            this.Btn_Clean.Tag = "";
            this.Btn_Clean.Text = "Clean";
            this.Btn_Clean.UseVisualStyleBackColor = true;
            this.Btn_Clean.Click += new System.EventHandler(this.Btn_Clean_Click);
            // 
            // Tsp_TooBar
            // 
            this.Tsp_TooBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Tss_ViewButton,
            this.Tss_HelpButton});
            this.Tsp_TooBar.Location = new System.Drawing.Point(0, 0);
            this.Tsp_TooBar.Name = "Tsp_TooBar";
            this.Tsp_TooBar.Size = new System.Drawing.Size(942, 27);
            this.Tsp_TooBar.TabIndex = 6;
            this.Tsp_TooBar.Text = "Tool Bar";
            // 
            // Tss_ViewButton
            // 
            this.Tss_ViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Tss_ViewButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Tsm_WordWrap});
            this.Tss_ViewButton.Image = ((System.Drawing.Image)(resources.GetObject("Tss_ViewButton.Image")));
            this.Tss_ViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Tss_ViewButton.Name = "Tss_ViewButton";
            this.Tss_ViewButton.Size = new System.Drawing.Size(57, 24);
            this.Tss_ViewButton.Tag = "";
            this.Tss_ViewButton.Text = "View";
            // 
            // Tsm_WordWrap
            // 
            this.Tsm_WordWrap.Image = global::AvePoint.Wrapper.MonitorTool.Properties.Resources.btm_WordWrap_Off;
            this.Tsm_WordWrap.Name = "Tsm_WordWrap";
            this.Tsm_WordWrap.Size = new System.Drawing.Size(151, 24);
            this.Tsm_WordWrap.Text = "WordWrap";
            this.Tsm_WordWrap.Click += new System.EventHandler(this.Tsm_WordWrap_Click);
            // 
            // Tss_HelpButton
            // 
            this.Tss_HelpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Tss_HelpButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Tsm_HowToUse,
            this.Tsm_AboutDocAve});
            this.Tss_HelpButton.Image = ((System.Drawing.Image)(resources.GetObject("Tss_HelpButton.Image")));
            this.Tss_HelpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Tss_HelpButton.Name = "Tss_HelpButton";
            this.Tss_HelpButton.Size = new System.Drawing.Size(57, 24);
            this.Tss_HelpButton.Tag = "";
            this.Tss_HelpButton.Text = "Help";
            // 
            // Tsm_HowToUse
            // 
            this.Tsm_HowToUse.Name = "Tsm_HowToUse";
            this.Tsm_HowToUse.Size = new System.Drawing.Size(175, 24);
            this.Tsm_HowToUse.Text = "How To Use?";
            // 
            // Tsm_AboutDocAve
            // 
            this.Tsm_AboutDocAve.Name = "Tsm_AboutDocAve";
            this.Tsm_AboutDocAve.Size = new System.Drawing.Size(175, 24);
            this.Tsm_AboutDocAve.Text = "About DocAve";
            // 
            // Rtb_DecryptedFileContent
            // 
            this.Rtb_DecryptedFileContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Rtb_DecryptedFileContent.Location = new System.Drawing.Point(27, 125);
            this.Rtb_DecryptedFileContent.Name = "Rtb_DecryptedFileContent";
            this.Rtb_DecryptedFileContent.Size = new System.Drawing.Size(890, 498);
            this.Rtb_DecryptedFileContent.TabIndex = 7;
            this.Rtb_DecryptedFileContent.Text = "";
            this.Rtb_DecryptedFileContent.WordWrap = false;
            // 
            // Btn_ExportToFile
            // 
            this.Btn_ExportToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_ExportToFile.Location = new System.Drawing.Point(573, 85);
            this.Btn_ExportToFile.Name = "Btn_ExportToFile";
            this.Btn_ExportToFile.Size = new System.Drawing.Size(100, 23);
            this.Btn_ExportToFile.TabIndex = 8;
            this.Btn_ExportToFile.Text = "Export to File";
            this.Btn_ExportToFile.UseVisualStyleBackColor = true;
            this.Btn_ExportToFile.Click += new System.EventHandler(this.Btn_ExportToFile_Click);
            // 
            // MonitorLogDecryption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 655);
            this.Controls.Add(this.Btn_ExportToFile);
            this.Controls.Add(this.Rtb_DecryptedFileContent);
            this.Controls.Add(this.Tsp_TooBar);
            this.Controls.Add(this.Btn_Clean);
            this.Controls.Add(this.Btn_DecryptFile);
            this.Controls.Add(this.Btn_SelectFile);
            this.Controls.Add(this.Txb_FilePath);
            this.Controls.Add(this.Lbl_FilePath);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "MonitorLogDecryption";
            this.Text = "Monitor Log Decryption";
            this.Tsp_TooBar.ResumeLayout(false);
            this.Tsp_TooBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog Ofd_OpenFileDialog;
        private System.Windows.Forms.Label Lbl_FilePath;
        private System.Windows.Forms.TextBox Txb_FilePath;
        private System.Windows.Forms.Button Btn_SelectFile;
        private System.Windows.Forms.Button Btn_DecryptFile;
        private System.Windows.Forms.Button Btn_Clean;
        private System.Windows.Forms.ToolStrip Tsp_TooBar;
        private System.Windows.Forms.ToolStripSplitButton Tss_ViewButton;
        private System.Windows.Forms.ToolStripMenuItem Tsm_WordWrap;
        private System.Windows.Forms.ToolStripSplitButton Tss_HelpButton;
        private System.Windows.Forms.ToolStripMenuItem Tsm_HowToUse;
        private System.Windows.Forms.ToolStripMenuItem Tsm_AboutDocAve;
        private System.Windows.Forms.RichTextBox Rtb_DecryptedFileContent;
        private System.Windows.Forms.Button Btn_ExportToFile;

    }
}

