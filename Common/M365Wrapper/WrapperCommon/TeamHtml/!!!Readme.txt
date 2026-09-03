1.  How to change team conversation format?
1.1 Only change display style.
    a. Change html in Template/ConversationReplyTemplate.html or ConversationTopicTemplate.html.
    b. Add css in Template/ConversationHeaderTemplate.html.
1.2 Add property(variable)
    a. Change html in Template/ConversationReplyTemplate.html or ConversationTopicTemplate.html. Variable should be replaced with {0},{1}...
	b. Add property in class ConversationItem or its subclass
	c. Change TeamHtmlResources.AssemblyTopicHtml or AssemblyReplyHtml
	    public static string AssemblyTopicHtml(ConversationTopic topic)
        {
            //performance issue if body is large.
            return string.Format(ConversationTopicTemplate_html,
                topic.PostedBy,                              //{0}
                topic.PostedTime,                            //{1}
                topic.PostedTime,                            //{2}
                topic.Important ? Important : string.Empty,  //{3}
                topic.Subject ?? string.Empty,               //{4}
                topic.Body);                                 //{5}
        } 

2. How to run test easily?
Run test in class TeamsConversationBuilderTest(in project O365GroupUnitTest), change parameter if needed.

3. How to change topic\reply body, like replace url?
It is not recommended to change topic\reply body, it may cause performance issue(like string replace, HtmlDocument.Load). If you have to, make sure it is efficient.
Uncomment TeamsHtmlBuilder.FormatItemBody and imply the logic there.
