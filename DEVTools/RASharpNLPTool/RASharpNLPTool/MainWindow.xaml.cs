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
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AvePoint.RA.SharpNLP.WordDeformations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenNLP.Tools.PosTagger;


namespace RASharpNLPTool
{
    using AvePoint.RA.SharpNLP.WordDeformations;
    using AvePoint.RA.SharpNLP;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private Dictionary<string ,List<string>> resultDicVerb=new Dictionary<string, List<string>>();
        private Dictionary<string ,List<string>> resultDicNoun=new Dictionary<string, List<string>>();
        private List<string> tags = new List<string>();


        private void btn_CheckTag_Click(object sender, RoutedEventArgs e)
        {
            tb_OutputTags.Clear();
            tags.Clear();
            string[] inputWords = tb_InputVerb.Text.Split(' ', ',', '.', '?', '(', ')', '!', '"', ':', ';', '+', '-', '*', '/', '=', '{', '}', '[', ']');

            FindVerbAndNoun findVerbAndNoun = new FindVerbAndNoun(inputWords);
            tags=findVerbAndNoun.GetTags();
            StringBuilder result = new StringBuilder();
            for(int i = 0; i < tags.Count; ++i)
            {
                result.Append(inputWords[i] + " - ").Append(tags[i]+" \n\n");

            }

            tb_OutputTags.Text += result;
        }

        private void btn_FindDeformate_Click(object sender, RoutedEventArgs e)
        {
            tb_OutputVerb.Clear();
            resultDicVerb.Clear();

            string[] inputWords = tb_InputVerb.Text.Split('\"', ' ', ',', '.', '?', '(', ')', '!', ':', ';', '+', '-', '*', '/', '=', '{', '}', '[', ']');

            resultDicVerb = RASharpNLPUtility.AnalyzeStringTerms_AllPOS(inputWords);

            foreach (var key in resultDicVerb.Keys)
            {
                tb_OutputVerb.Text += key + ":\n";
                for (int i = 0; i < resultDicVerb[key].Count; i++)
                {
                    tb_OutputVerb.Text += "\t" + resultDicVerb[key][i] + "\n";
                }
                tb_OutputVerb.Text += "\n";
            }
        }
       
        private void btn_NounDeformate_Click(object sender, RoutedEventArgs e)
        {
            tb_OutputNoun.Clear();
            resultDicNoun.Clear();
            string[] inputWords = tb_InputNoun.Text.Split('"', ' ', ',', '.', '?', '(', ')', '!', ':', ';', '+', '-', '*', '/', '=', '{', '}', '[', ']');
            
            resultDicNoun = RASharpNLPUtility.AnalyzeStringTerms(inputWords);
            foreach (var key in resultDicNoun.Keys)
            {
                tb_OutputNoun.Text += key + ":\n";
                for (int i = 0; i < resultDicNoun[key].Count; i++)
                {
                    tb_OutputNoun.Text += "\t" + resultDicNoun[key][i] + "\n";
                }
                tb_OutputNoun.Text += "\n";
            }
        }

        private void btn_ClearInputVerbs_Click(object sender, RoutedEventArgs e)
        {
            tb_InputVerb.Clear();
        }

        private void btn_ClearInputNoun_Click(object sender, RoutedEventArgs e)
        {
            tb_InputNoun.Clear();
        }

        private void btn_ClearOutputNoun_Click(object sender, RoutedEventArgs e)
        {
            tb_OutputNoun.Clear();
        }

        private void btn_ClearOutputVerbs_Click(object sender, RoutedEventArgs e)
        {
            tb_OutputVerb.Clear();
            tb_OutputTags.Clear();
        }

    }
}
