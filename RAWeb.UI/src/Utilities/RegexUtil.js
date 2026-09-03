const PatternType = {
    REGEX_DIGIT_ONLY : /^\d+(\.\d+)?$/,
    REGEX_CHAR_ONLY : /^[A-Za-z]+$/,
    REGEX_SPECIAL_CHAR_ONLY : /^[\s!"#$%&'()*+,./:;<=>?@[\\\]^_`{|}~-]+$/,
    REGEX_DIGIT_AND_CHAR : /^[\dA-Za-z]+$/,
    REGEX_DIGIT_AND_SPECIAL_CHAR : /^[\d\s!"#$%&'()*+,./:;<=>?@[\\\]^_`{|}~-]+$/,
    REGEX_CHAR_AND_SPECIAL_CHAR : /^[\s!"#$%&'()*+,./:;<=>?@A-Z[\\\]^_`a-z{|}~-]+$/,
    REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR : /^[\s\w!"#$%&'()*+,./:;<=>?@[\\\]^`{|}~-]+$/
};

class RegexUtil
{
    static IsMath(str, pattern = PatternType.REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR)
    {
        if(!str)
        {
            throw new Error("Please check the input string.");
        }
        if(!pattern)
        {
            throw new Error("Please check the regex pattern.");
        }
        return pattern.test(str);
    }
}

export { RegexUtil, PatternType };