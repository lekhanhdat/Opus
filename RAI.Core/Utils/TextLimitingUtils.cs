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
using System.Linq;

namespace AvePoint.RAI.Core.Utils
{
    /// <summary>
    /// Utility class for text length limiting and token estimation for AI services
    /// </summary>
    public static class TextLimitingUtils
    {
        // Constants for text length limits
        private const int MAX_TEXT_LENGTH_CHARS = 3500; // More generous limit for multilingual text, especially CJK
        private const int MAX_TEXT_LENGTH_TOKENS = 2048; // Conservative token limit for text-multilingual-embedding-002

        /// <summary>
        /// Limits text length to prevent API errors with text-multilingual-embedding-002
        /// </summary>
        /// <param name="text">Input text to limit</param>
        /// <returns>Text limited to safe length</returns>
        public static string LimitTextLength(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Check if text contains significant CJK content
            var cjkCharCount = text.Count(c =>
                (c >= 0x4E00 && c <= 0x9FFF) ||  // CJK Unified Ideographs
                (c >= 0x3400 && c <= 0x4DBF) ||  // CJK Extension A
                (c >= 0x3040 && c <= 0x309F) ||  // Hiragana
                (c >= 0x30A0 && c <= 0x30FF) ||  // Katakana
                (c >= 0xAC00 && c <= 0xD7AF));   // Hangul Syllables

            var isCJKHeavy = cjkCharCount > text.Length * 0.3; // More than 30% CJK characters

            // Use more generous limits for CJK-heavy text to allow up to ~2500 characters
            // Define the character limit for CJK-heavy text
            const int CJK_CHAR_LIMIT = 2500;
            var effectiveCharLimit = isCJKHeavy ? CJK_CHAR_LIMIT : MAX_TEXT_LENGTH_CHARS;

            // First check character limit
            if (text.Length <= effectiveCharLimit)
            {
                // Also check estimated token count
                var estimatedTokens = EstimateTokenCount(text);
                if (estimatedTokens <= MAX_TEXT_LENGTH_TOKENS)
                {
                    return text;
                }
            }

            // If text is too long, truncate it
            // For CJK text, we need to be much more conservative
            var targetLength = Math.Min(effectiveCharLimit, text.Length);
            var targetTokens = MAX_TEXT_LENGTH_TOKENS;

            // For CJK-heavy text, use 95% of limit for maximum text retention
            if (isCJKHeavy)
            {
                targetTokens = (int)(MAX_TEXT_LENGTH_TOKENS * 0.95); // Use 95% of limit (1945 tokens)
            }

            // Iteratively reduce length until we're under token limit
            while (targetLength > 0)
            {
                var truncatedText = text.Substring(0, targetLength);
                var estimatedTokens = EstimateTokenCount(truncatedText);

                if (estimatedTokens <= targetTokens)
                {
                    // For CJK text, try to break at punctuation or logical boundaries
                    if (isCJKHeavy)
                    {
                        // Try to find a good break point (punctuation, sentence end)
                        var goodBreakPoints = new char[] { '。', '！', '？', '；', '，', '.', '!', '?', ';', ',' };
                        var bestBreakIndex = -1;

                        for (int i = truncatedText.Length - 1; i >= targetLength * 0.7; i--)
                        {
                            if (goodBreakPoints.Contains(truncatedText[i]))
                            {
                                bestBreakIndex = i + 1;
                                break;
                            }
                        }

                        if (bestBreakIndex > 0)
                        {
                            return truncatedText.Substring(0, bestBreakIndex);
                        }
                    }
                    else
                    {
                        // For non-CJK text, try to end at a word boundary
                        var lastSpaceIndex = truncatedText.LastIndexOf(' ');
                        if (lastSpaceIndex > targetLength * 0.8)
                        {
                            return truncatedText.Substring(0, lastSpaceIndex);
                        }
                    }

                    return truncatedText;
                }

                // Reduce target length less aggressively for CJK text to retain more content
                var reductionFactor = isCJKHeavy ? 0.85 : 0.9;
                targetLength = (int)(targetLength * reductionFactor);
            }

            // Fallback: use much more generous limits for Chinese text to reach ~2000+ characters
            var fallbackLength = isCJKHeavy ? 2000 : 1200;
            return text.Length > fallbackLength ? text.Substring(0, fallbackLength) : text;
        }

        /// <summary>
        /// Estimate token count for a given text (approximation for multilingual text)
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>Estimated token count</returns>
        public static int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // More accurate estimation for multilingual text
            var tokenCount = 0;
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Different tokenization rules based on character types
                if (ContainsCJK(word))
                {
                    // Chinese, Japanese, Korean: More conservative estimation
                    // For Chinese text, approximately 1 character ≈ 1 token (sometimes more)
                    tokenCount += Math.Max(1, (int)(word.Length * 1.2)); // Conservative: 1.2 tokens per character
                }
                else if (ContainsArabic(word))
                {
                    // Arabic: typically 2-3 characters per token
                    tokenCount += Math.Max(1, (int)(word.Length / 2.5));
                }
                else if (ContainsCyrillic(word))
                {
                    // Russian, Ukrainian, etc.: typically 3-4 characters per token
                    tokenCount += Math.Max(1, (int)(word.Length / 3.5));
                }
                else
                {
                    // Latin-based languages (English, Spanish, French, etc.): 4-5 characters per token
                    tokenCount += Math.Max(1, (int)(word.Length / 4.5));
                }
            }

            // Add tokens for punctuation and special characters
            var punctuationCount = text.Count(c => char.IsPunctuation(c) || char.IsSymbol(c));
            tokenCount += punctuationCount / 2; // Approximate: 2 punctuation marks ≈ 1 token

            return Math.Max(1, (int)tokenCount);
        }

        /// <summary>
        /// Check if text contains Chinese, Japanese, or Korean characters
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if contains CJK characters</returns>
        private static bool ContainsCJK(string text)
        {
            return text.Any(c =>
                (c >= 0x4E00 && c <= 0x9FFF) ||  // CJK Unified Ideographs
                (c >= 0x3400 && c <= 0x4DBF) ||  // CJK Extension A
                (c >= 0x20000 && c <= 0x2A6DF) || // CJK Extension B
                (c >= 0x3040 && c <= 0x309F) ||  // Hiragana
                (c >= 0x30A0 && c <= 0x30FF) ||  // Katakana
                (c >= 0xAC00 && c <= 0xD7AF));   // Hangul Syllables
        }

        /// <summary>
        /// Check if text contains Arabic characters
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if contains Arabic characters</returns>
        private static bool ContainsArabic(string text)
        {
            return text.Any(c =>
                (c >= 0x0600 && c <= 0x06FF) ||  // Arabic
                (c >= 0x0750 && c <= 0x077F) ||  // Arabic Supplement
                (c >= 0x08A0 && c <= 0x08FF) ||  // Arabic Extended-A
                (c >= 0xFB50 && c <= 0xFDFF) ||  // Arabic Presentation Forms-A
                (c >= 0xFE70 && c <= 0xFEFF));   // Arabic Presentation Forms-B
        }

        /// <summary>
        /// Check if text contains Cyrillic characters
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if contains Cyrillic characters</returns>
        private static bool ContainsCyrillic(string text)
        {
            return text.Any(c =>
                (c >= 0x0400 && c <= 0x04FF) ||  // Cyrillic
                (c >= 0x0500 && c <= 0x052F) ||  // Cyrillic Supplement
                (c >= 0x2DE0 && c <= 0x2DFF) ||  // Cyrillic Extended-A
                (c >= 0xA640 && c <= 0xA69F));   // Cyrillic Extended-B
        }
    }
}
