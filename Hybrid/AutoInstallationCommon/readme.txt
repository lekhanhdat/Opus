1. Please add these codes in your app.config or web.config in your project.
<configuration>
	<configSections>
		<section name="system.directoryservices" type="System.DirectoryServices.SearchWaitHandler, System.DirectoryServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
	</configSections>
	 <system.directoryservices>
    		<DirectorySearcher waitForPagedSearchData="true"/>
  	</system.directoryservices>
</configuration>

2. Examples

	    //Create a checker for a domain with username and password
            ActiveDirectoryChecker checker = new ActiveDirectoryChecker("avepoint.com","zbsun","password");
            //Get domain's nETBiosName
            string domainNetBiosName = checker.NetBIOSName;
            //Create a searcher to search in a domain
            ActiveDirectorySearcher searcher = checker.CreateDefaultSearcher();
            //Search a User with common name or samaccountname as "zbsun"
            ActiveDirectoryObject userObj = searcher.SingleSearchUser("zbsun");
            //Get user ObjectSID (the uniqued id in domain)
            string userObjectSID = userObj.ObjectSID;
            //Get User's UserPrincipalName , such as "zbsun@avepoint.com"
            string userPrincipalName = userObj.UPN;
            //Get User's msDS_principalName, such as AVEPOINT\zbsun
            string user_msDS_PrincipalName = userObj.MSDS_PrincipalName;
            //Get Group with common name as CC_DEV_5
            ActiveDirectoryObject groupObj = searcher.SingleSearchGroup("CC_DEV_5");
            //Check if userObj is a member of groupObj
            //include nested group chain
            bool isIn = userObj.IsMemeberOf(groupObj);

            //Wildcard search, you can get the results with any attribute value like 'abc*'
            var wildcard_result = searcher.WildcardYieldSearch("abc");
            //Set custom search string to search in domain. and the result is yield return.
            var yield_result=searcher
                .SetFilter("(&(objectClass=user)(objectCategory=person)(cn=something))")
                .YieldSearch();

            //Connect to CC00538 computer and focus to Administrators group and Set username and password to access
            WorkGroupObject workGroupObj = new WorkGroupObject("CC00538", "Administrators")
                                                        .Logon("zbsun@avepoint.com","password");
            //get all direct groups(no nested) in Administrators group
            List<WorkGroupObject> groups = workGroupObj.Groups;
            //get all direct memebrs(no nested) in Administrators group
            List<WorkGroupObject> members = workGroupObj.Members;

            //Get user in workgroup
            WorkGroupObject workGroupUserobj = new WorkGroupObject("CC00538","zbsun")
                                                        .Logon("zbsun@avepoint.com","password");
            //Check if an user is in a group
            bool isInGroup = workGroupUserobj.IsMemberOf(workGroupObj);