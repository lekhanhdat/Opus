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

namespace OpenNLP.Tools
{
    using System.Linq;

    public static class PartsOfSpeech
    {
        // List of all parts of speech 

        // verbs
        public const string VerbBaseForm = "VB";
        public const string VerbNon3rdPersSingPresent = "VBP";
        public const string Verb3rdPersSingPresent = "VBZ";
        public const string VerbPastTense = "VBD";
        public const string VerbGerundOrPresentParticiple = "VBG";
        public const string VerbPastParticiple = "VBN";
        // adjectives
        public const string Adjective = "JJ";
        public const string AdjectiveComparative = "JJR";
        public const string AdjectiveSuperlative = "JJS";
        // nouns
        public const string NounSingularOrMass = "NN";
        public const string NounPlural = "NNS";
        public const string ProperNounSingular = "NNP";
        public const string ProperNounPlural = "NNPS";
        // adverbs
        public const string WhAdverb = "WRB";
        public const string Adverb = "RB";
        public const string AdverbComparative = "RBR";
        public const string AdverbSuperlative = "RBS";
        // conjunctions
        public const string CoordinatingConjunction = "CC";
        public const string PrepositionOrSubordinateConjunction = "IN";
        // pronouns
        public const string WhPronoun = "WP";
        public const string PossessiveWhPronoun = "WP$";
        public const string PersonalPronoun = "PRP";
        public const string PossessivePronoun = "PRP$";
        // misc
        public const string Particle = "RP";
        public const string CardinalNumber = "CD";
        public const string Determiner = "DT";
        public const string To = "TO";
        public const string ExistentialThere = "EX";
        public const string Interjection = "UH";
        public const string ForeignWord = "FW";
        public const string ListItemMarker = "LS";
        public const string Modal = "MD";
        public const string WhDeterminer = "WDT";
        public const string Predeterminer = "PDT";
        // punctuation
        public const string LeftOpenDoubleQuote = "``";
        public const string PossessiveEnding = "POS";
        public const string Comma = ",";
        public const string RightCloseDoubleQuote = "''";
        public const string SentenceFinalPunctuation = ".";
        public const string ColonSemiColon = ":";
        public const string LeftParenthesis = "-LRB";
        public const string RightParenthesis = "-RRB";
        // symbols
        public const string DollarSign = "$";
        public const string PoundSign = "#";
        public const string Symbol = "SYM";


       
        public static bool IsVerb(string pos)
        {
            return !string.IsNullOrEmpty(pos) && pos.StartsWith("VB");
        }
         
        public static bool IsNoun(string pos)
        {
            return !string.IsNullOrEmpty(pos) && pos.StartsWith("NN");
        }
         
        public static bool IsProperNoun(string pos)
        {
            return !string.IsNullOrEmpty(pos) && pos.StartsWith("NNP");
        }
         
        public static bool IsAdjective(string pos)
        {
            return !string.IsNullOrEmpty(pos) && pos.StartsWith("JJ");
        }
         
        public static bool IsPersOrPossPronoun(string tag)
        {
            return !string.IsNullOrEmpty(tag) && tag.StartsWith("PRP");
        }
         
        public static string Write(string pos)
        {
            var fields = typeof(PartsOfSpeech).GetFields();
            foreach (var fieldInfo in fields)
            {
                var value = (string)fieldInfo.GetValue(null);
                if (value == pos)
                {
                    return fieldInfo.Name;
                }
            }
            return string.Format("Unsupported function abbreviation ({0})", pos);
        }
         
        public static bool IsSupportedPartOfSpeech(string function)
        {
            var fields = typeof(PartsOfSpeech).GetFields();
            return fields
                .Select(fieldInfo => (string) fieldInfo.GetValue(null))
                .Any(value => value == function);
        }
    }
}
