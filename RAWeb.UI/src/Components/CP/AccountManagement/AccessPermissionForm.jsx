import { SetTermPermissionMethod, SourceFlags, TermObjType, PhyUserRoleType, RulePermissionMethod, RuleObjType, RestoreCenterTypeTitle } from "../../../Constants/Constants";
import { LicenseHelper } from '../../../Utilities/CommonUtil';
import { getPermissionReportList } from "./Constants";

export default class AccountManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.defaultPageIndex = 0;
        this.defaultPageSize = 10;
        this.allTermGroupItems = [];
        this.cacheNodeInfo = this.getTreeRootNode();
        this.cacheRuleNodeInfo = this.getRuleContainerTreeRootNode();
        this.state = {
            termPermissionType: SetTermPermissionMethod.None,
            rulePermissionType: RulePermissionMethod.None,
            treeDataObj: {},
            ruleTreeDataObj: {},
            scopePermissionItems: []
        };
        this.bind(['shownScopes', 'hideScopes', 'onTGroupPageChanged', 'onTSetPageChanged', 'onRContainerPageChanged', 'onRItemPageChanged']);
    }

    componentInit() {
        this.initPermissionInfo(this.defaultPageIndex, this.defaultPageSize);
    }

    shownScopes(item)
    {
        let items = RM.deepcopy(this.state.scopePermissionItems);
        let scopeItem = items.find(o => o.dataSourceType == item.dataSourceType);
        if(scopeItem)
        {
            scopeItem.showExpander = true;
        }
        this.setState({
            scopePermissionItems: items
        });
    }

    hideScopes(item)
    {
        let items = RM.deepcopy(this.state.scopePermissionItems);
        let scopeItem = items.find(o => o.dataSourceType == item.dataSourceType);
        if(scopeItem)
        {
            scopeItem.showExpander = false;
        }
        this.setState({
            scopePermissionItems: items
        });
    }

    initPermissionInfo(pageIndex, pageSize)
    {
        let userPermissionInfo = this.props.userPermissionInfo;
        this.cacheNodeInfo.SubTerms = userPermissionInfo.TermPermissionInfo.TermGroups || [];
        this.cacheNodeInfo.SubTermCount = this.cacheNodeInfo.SubTerms.length;
        let tempRootNode = RM.deepcopy(this.cacheNodeInfo);
        tempRootNode.SubTerms = JSON.parse(JSON.stringify(this.cacheNodeInfo.SubTerms.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        this.setTermSetsInfo(tempRootNode);

        this.cacheRuleNodeInfo.SubItems = userPermissionInfo.RulePermissionInfo.RuleContainers || [];
        this.cacheRuleNodeInfo.SubItemCount = this.cacheRuleNodeInfo.SubItems.length;
        let tempRuleRootNode = RM.deepcopy(this.cacheRuleNodeInfo);
        tempRuleRootNode.SubItems = JSON.parse(JSON.stringify(this.cacheRuleNodeInfo.SubItems.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        this.setRuleItemsInfo(tempRuleRootNode);

        this.setState({
            termPermissionType: userPermissionInfo.TermPermissionInfo.TermPermissionType,
            rulePermissionType: userPermissionInfo.RulePermissionInfo.RulePermissionType,
            treeDataObj: tempRootNode,
            ruleTreeDataObj: tempRuleRootNode,
            scopePermissionItems: userPermissionInfo.ScopePermissionInfo
        });
    }

    getTreeRootNode()
    {
        return {
            Id: -1,
            UniqueId: "00000000-0000-0000-0000-000000000000",
            Name: "Groups",
            Type: TermObjType.Root,
            IsExpand: true,
            IsChecked: false,
            SubPerIndex: 0,
            SubPerSize: 10, //初始值TermGroup 10个分页
            SubTermCount: 0,
            SubTerms: []
        };
    }

    getRuleContainerTreeRootNode()
    {
        return {
            Id: -1,
            UniqueId: "00000000-0000-0000-0000-000000000000",
            Name: "RuleContainer",
            Type: RuleObjType.Root,
            IsExpand: true,
            IsChecked: false,
            SubPerIndex: 0,
            SubPerSize: 10, //初始值TermGroup 10个分页
            SubItems: [],
            SubItemCount: 0,
        };
    }

    onTGroupPageChanged(pageIndex, pageSize)
    {
        let allTermGroups = this.cacheNodeInfo.SubTerms;
        let rootNode = RM.deepcopy(this.state.treeDataObj);
        rootNode.SubTerms = JSON.parse(JSON.stringify(allTermGroups.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        rootNode.SubPerIndex = pageIndex;
        rootNode.SubPerSize = pageSize;
        this.setTermSetsInfo(rootNode);
        this.setState({
            treeDataObj: rootNode
        });
    }

    onRContainerPageChanged(pageIndex, pageSize)
    {
        let allRuleContainer = this.cacheRuleNodeInfo.SubItems;
        let rootNode = RM.deepcopy(this.state.ruleTreeDataObj);
        rootNode.SubItems = JSON.parse(JSON.stringify(allRuleContainer.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        rootNode.SubPerIndex = pageIndex;
        rootNode.SubPerSize = pageSize;
        this.setRuleItemsInfo(rootNode);
        this.setState({
            ruleTreeDataObj: rootNode
        });
    }

    onTSetPageChanged(parentTermGroup, pageIndex, pageSize)
    {
        let cTermGroup = this.cacheNodeInfo.SubTerms.find(o => o.UniqueId == parentTermGroup.UniqueId);
        let allSubTermSets = cTermGroup.SubTerms;
        let rootNode = RM.deepcopy(this.state.treeDataObj);
        let rTermGroup = rootNode.SubTerms.find(o => o.UniqueId == parentTermGroup.UniqueId);
        rTermGroup.SubTerms = JSON.parse(JSON.stringify(allSubTermSets.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        rTermGroup.SubPerIndex = pageIndex;
        rTermGroup.SubPerSize = pageSize;
        this.setState({
            treeDataObj: rootNode
        });
    }

    onRItemPageChanged(parentRuleContainer, pageIndex, pageSize)
    {
        let cRuleContainer = this.cacheRuleNodeInfo.SubItems.find(o => o.UniqueId == parentRuleContainer.UniqueId);
        let allRuleItems = cRuleContainer.SubItems;
        let rootNode = RM.deepcopy(this.state.ruleTreeDataObj);
        let rRuleContainer = rootNode.SubItems.find(o => o.UniqueId == parentRuleContainer.UniqueId);
        rRuleContainer.SubItems = JSON.parse(JSON.stringify(allRuleItems.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        rRuleContainer.SubPerIndex = pageIndex;
        rRuleContainer.SubPerSize = pageSize;
        this.setState({
            ruleTreeDataObj: rootNode
        });
    }

    setTermSetsInfo(rootNode)
    {
        let termGroups = rootNode.SubTerms;
        termGroups.forEach((tg)=> {
            let termSets = tg.SubTerms;
            if(!termSets)
            {
                tg.SubTerms = [];
            }
            if(termSets && termSets.length > 10)
            {
                tg.SubTerms = JSON.parse(JSON.stringify(termSets.slice(tg.SubPerIndex * 10, (tg.SubPerIndex + 1) * 10)));
            }
        });
    }

    setRuleItemsInfo(rootNode)
    {
        let ruleContainers = rootNode.SubItems;
        ruleContainers.forEach((tg)=> {
            let ruleItems = tg.SubItems;
            if(!ruleItems)
            {
                tg.SubItems = [];
            }
            if(ruleItems && ruleItems.length > 10)
            {
                tg.SubItems = JSON.parse(JSON.stringify(ruleItems.slice(tg.SubPerIndex * 10, (tg.SubPerIndex + 1) * 10)));
            }
        });
    }

    showTermSets(tGroupId)
    {
        let rootNode = RM.deepcopy(this.state.treeDataObj);
        let rTermGroup = rootNode.SubTerms.find(o => o.UniqueId == tGroupId);
        if(rTermGroup)
        {
            rTermGroup.IsExpand = true;
            this.setState({
                treeDataObj: rootNode
            });
        }
    }

    showRuleItems(rContainerId)
    {
        let rootNode = RM.deepcopy(this.state.ruleTreeDataObj);
        let rRuleContainer = rootNode.SubItems.find(o => o.UniqueId == rContainerId);
        if(rRuleContainer)
        {
            rRuleContainer.IsExpand = true;
            this.setState({
                ruleTreeDataObj: rootNode
            });
        }
    }

    hideTermSets(termGroup)
    {
        let rootNode = RM.deepcopy(this.state.treeDataObj);
        let curTermGroup = rootNode.SubTerms.find(o => o.UniqueId == termGroup.UniqueId);
        if(curTermGroup)
        {
            curTermGroup.IsExpand = false;
            this.setState({
                treeDataObj: rootNode
            });
        }
    }

    hideRuleItems(ruleContainer)
    {
        let rootNode = RM.deepcopy(this.state.ruleTreeDataObj);
        let curRuleContainer = rootNode.SubItems.find(o => o.UniqueId == ruleContainer.UniqueId);
        if(curRuleContainer)
        {
            curRuleContainer.IsExpand = false;
            this.setState({
                ruleTreeDataObj: rootNode
            });
        }
    }

    renderScopePermissionInfo()
    {
        let scopePermissionItems = RM.deepcopy(this.state.scopePermissionItems);
        let scopeExpanderList = [];
        let hasScopePermission = scopePermissionItems.find(o => o.scopesNameOrPath.length > 0);
        let result =  <div className="no-permission">
            <img src="/Images/Base/no_permission_small.svg" className="no-permission-icon"/>
            <div className="category-desc" tabIndex = {0}>{RMResx.RM_CP_AM_NoPermission}</div>
        </div>;

        if(hasScopePermission)
        {
            scopePermissionItems.map((item)=>{
                if(item.scopesNameOrPath.length > 0)
                {
                    let expander = <R.Expander  bgColor={"#E6E7E8"} status={{show: item.showExpander}} key={item.dataSourceType} onShow={this.shownScopes.bind(this, item)} onHide={this.hideScopes.bind(this, item)}>
                        <div>
                            <R.Scope>{item.title}</R.Scope>
                        </div>
                        <div className="info-content">
                            {item.scopesNameOrPath.map((nameOrPath, index) => {
                                return <div key={index} className="info-row-wrapper">
                                    <div className='info-row' tabIndex='0' data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={nameOrPath}>{nameOrPath}</div>
                                </div>;
                            })}
                        </div>
                    </R.Expander>;
                    scopeExpanderList.push(expander);
                }
            });
            result = scopeExpanderList;
        } 
        return result;
    }

    renderTermPermissionInfo()
    {
        let result = <div></div>;
        let termPermissionType = this.state.termPermissionType;
        let rootNode = this.state.treeDataObj;
        let termGroupItems = rootNode.SubTerms || [];
        let hasPager = rootNode.SubTermCount > rootNode.SubPerSize;
        let isAllPermission = termPermissionType == SetTermPermissionMethod.All;
        if (this.props.scopePermissionInfo.length == 1) {
            const hasGoogle = this.props.scopePermissionInfo.some(item => item.DataSourceType == SourceFlags.Google) && !this.props.isAdmin;
            if (hasGoogle) {
                return (
                    <div className="no-permission">
                        <img src="/Images/Base/no_permission_small.svg" className="no-permission-icon"/>
                        <div className="category-desc" tabIndex = {0}>{RMResx.RM_CP_AM_Term_NoPermission}</div>
                    </div>
                )
            }
        }
        switch(termPermissionType)
        {
            case SetTermPermissionMethod.None:
                result = <div className="no-permission">
                    <img src="/Images/Base/no_permission_small.svg" className="no-permission-icon"/>
                    <div className="category-desc" tabIndex = {0}>{RMResx.RM_CP_AM_Term_NoPermission}</div>
                </div>;
                break;
            case SetTermPermissionMethod.All:
            case SetTermPermissionMethod.SpecifyScope:
                result = <div>
                    {isAllPermission && <div className="category-desc" tabIndex = {0}>{RMResx.RM_CP_AM_Term_AllPermission}</div>}
                    <div>{this.getTermExpanderList(termGroupItems)}</div>
                    {hasPager &&  <div className="pager-position">
                        <$g.Pager
                            itemsCount={rootNode.SubTermCount}
                            pagerIndex={rootNode.SubPerIndex}
                            pagerSize={rootNode.SubPerSize}
                            showPagerSize={false}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.onTGroupPageChanged}/>
                    </div>}
                </div>;
                break;
        }
        return result;
    }

    renderRulePermissionInfo()
    {
        let result = <div></div>;
        let rulePermissionType = this.state.rulePermissionType;
        let rootNode = this.state.ruleTreeDataObj;
        let ruleContainers = rootNode.SubItems || [];
        let hasPager = rootNode.SubItemCount > rootNode.SubPerSize;
        let isAllPermission = rulePermissionType == RulePermissionMethod.All;
        switch(rulePermissionType)
        {
            case RulePermissionMethod.None:
                result = <div className="no-permission">
                    <img src="/Images/Base/no_permission_small.svg" className="no-permission-icon"/>
                    <div className="category-desc" tabIndex = {0}>{RMResx.RM_CP_AM_Rule_NoPermission}</div>
                </div>;
                break;
            case RulePermissionMethod.All:
            case RulePermissionMethod.SpecifyScope:
                result = <div>
                    {isAllPermission && <div className="category-desc" tabIndex = {0} >{RMResx.RM_CP_AM_Rule_AllPermission}</div>}
                    <div>{this.getRuleExpanderList(ruleContainers)}</div>
                    {hasPager &&  <div className="pager-position">
                        <$g.Pager
                            itemsCount={rootNode.SubItemCount}
                            pagerIndex={rootNode.SubPerIndex}
                            pagerSize={rootNode.SubPerSize}
                            showPagerSize={false}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.onRContainerPageChanged}/>
                    </div>}
                </div>;
                break;
        }
        return result;
    }

    renderUserRoleAndSubPermissions()
    {
        let items = RM.deepcopy(this.state.scopePermissionItems);
        let scopeInfo = items.find(o => o.dataSourceType == SourceFlags.Phy); 
        if(scopeInfo && scopeInfo.userRoleType != PhyUserRoleType.None)
        {
            let isPhyEndUser = scopeInfo.userRoleType == PhyUserRoleType.EndUser;
            return <div>
                <div className="category-main">
                    <div className="category-title">{RMResx.RM_CP_AM_Table_Column_PermissionName}</div>
                    <div className="category-content">{isPhyEndUser? RMResx.RM_CP_AM_PhysicalPermission_EndUser: RMResx.RM_CP_AM_PhysicalPermission_Admin}</div>
                </div>
                {isPhyEndUser && <div className="category-main">
                    <div className="category-title">{RMResx.RM_CP_AM_Module_PhyExplorer_Permission_Title}</div>
                    <div className="category-content">{this.renderSubPermissionItems(scopeInfo.phySubPermissions)}</div>
                </div>}
            </div>;
        } 
    }

    renderSubPermissionItems(names)
    {
        if (names && names.length > 0) {
            let temp = [];
            names.map((name, index) => {
                let displayName = `- ${name}`;
                temp.push(<div key={index} className="info-normal-row"><span>{displayName}</span></div>);
            });
            return temp;
        } else {
            return RMResx.RM_CP_AM_Module_PhyExplorer_NoPermission;
        }
    }

    getTermExpanderList(termGroupNodes)
    {
        if(termGroupNodes && termGroupNodes.length > 0)
        {
            let termExpanderList = [];
            termGroupNodes.map((termGroup)=> {
                let hasSubTermSets = termGroup.SubTerms && termGroup.SubTerms.length > 0;
                let hasPager = termGroup.SubTermCount > termGroup.SubPerSize;
                let expander = <R.Expander  bgColor={"#E6E7E8"} status={{show: termGroup.IsExpand}} key={termGroup.UniqueId} onShow={this.showTermSets.bind(this, termGroup.UniqueId)} onHide={this.hideTermSets.bind(this, termGroup)}>
                    <div data-tooltip="ifneed" data-tooltip-wrap="force">
                        <R.Scope>{termGroup.Name}</R.Scope>
                    </div>
                    <div className="info-content">
                        {hasSubTermSets && termGroup.SubTerms.map((termSet, index) => {
                            return <div key={index} className="info-row-wrapper">
                                <div className='info-row' tabIndex='0' data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={termSet.Name}>{termSet.Name}</div>
                            </div>;
                        })}
                        {!hasSubTermSets && <div className="info-row" tabIndex="0">{RMResx.RM_CP_AM_TermPermission_NoTermSets}</div>}
                        {hasPager && <div className="pager-position">
                            <$g.Pager
                                itemsCount={termGroup.SubTermCount}
                                pagerIndex={termGroup.SubPerIndex}
                                pagerSize={termGroup.SubPerSize}
                                showPagerSize={false}
                                pagerSizeOptions={[5, 10, 15]}
                                onChange={this.onTSetPageChanged.bind(this, termGroup)}/>
                        </div>}
                    </div>
                </R.Expander>;
                termExpanderList.push(expander);
            });

            return termExpanderList;
        }
    }

    getRuleExpanderList(ruleContainerNodes)
    {
        if(ruleContainerNodes && ruleContainerNodes.length > 0)
        {
            let ruleExpanderList = [];
            ruleContainerNodes.map((ruleContainer)=> {
                let hasRuleItems = ruleContainer.SubItems && ruleContainer.SubItems.length > 0;
                let hasPager = ruleContainer.SubItemCount > ruleContainer.SubPerSize;
                let expander = <R.Expander  bgColor={"#E6E7E8"} status={{show: ruleContainer.IsExpand}} key={ruleContainer.UniqueId} onShow={this.showRuleItems.bind(this, ruleContainer.UniqueId)} onHide={this.hideRuleItems.bind(this, ruleContainer)}>
                    <div data-tooltip="ifneed" data-tooltip-wrap="force">
                        <R.Scope>{ruleContainer.Name}</R.Scope>
                    </div>
                    <div className="info-content">
                        {hasRuleItems && ruleContainer.SubItems.map((ruleItem, index) => {
                            return <div key={index} className="info-row-wrapper">
                                <div className='info-row' tabIndex='0' data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={ruleItem.Name}>{ruleItem.Name}</div>
                            </div>;
                        })}
                        {!hasRuleItems && <div className="info-row" tabIndex='0'>{RMResx.RM_CP_AM_RulePermission_NoRuleItems}</div>}
                        {hasPager && <div className="pager-position">
                            <$g.Pager
                                itemsCount={ruleContainer.SubItemCount}
                                pagerIndex={ruleContainer.SubPerIndex}
                                pagerSize={ruleContainer.SubPerSize}
                                showPagerSize={false}
                                pagerSizeOptions={[5, 10, 15]}
                                onChange={this.onRItemPageChanged.bind(this, ruleContainer)}/>
                        </div>}
                    </div>
                </R.Expander>;
                ruleExpanderList.push(expander);
            });

            return ruleExpanderList;
        }
    }

    renderReportInfo = () => {
        if (this.props.isUseReportingPermissionControl) {
            const permissionReportValue = this.props.reportingPermission;
            const checkedPermissionReportList = getPermissionReportList()
                .map((item) => ({
                    ...item,
                    checked: (permissionReportValue & item.value) !== 0,
                }))
                .filter(item => item.checked)
                .map((item) => item.text);
            return (
                <div>
                    <div className="flex flex-column gap-xs margin-top-s">
                        {checkedPermissionReportList.map((text, index) => (
                            <div key={index} tabIndex={0} className="category-content">
                                - {text}
                            </div>
                        ))}
                    </div>
                </div>
            )
        }

        return (
            <div className="no-permission">
                <img
                    src="/Images/Base/no_permission_small.svg"
                    className="no-permission-icon"
                />
                <div className="category-desc" tabIndex={0}>
                    {RMResx.RM_CP_AM_Report_NoPermission}
                </div>
            </div>
        );
    }

    renderManageHoldsInfo = () => {
        if (this.props.isEnableManageHolds) {
            return <div tabIndex={0} className="category-content">{RMResx.RM_CP_AM_ManageHolds_Option01}</div>
        }

        return (
            <div className="no-permission">
                <img
                    src="/Images/Base/no_permission_small.svg"
                    className="no-permission-icon"
                />
                <div className="category-desc" tabIndex={0}>
                    {RMResx.RM_CP_AM_ManageHolds_NoPermission}
                </div>
            </div>
        );
    }

    renderManageApprovalSettingsInfo = () => {
        if (this.props.isEnableManageApprovalSettings) {
            return <div tabIndex={0} className="category-content">{RMResx.RM_CP_AM_ManageApprovalSettings_Option01}</div>
        }

        return (
            <div className="no-permission">
                <img
                    src="/Images/Base/no_permission_small.svg"
                    className="no-permission-icon"
                />
                <div className="category-desc" tabIndex={0}>
                    {RMResx.RM_CP_AM_ManageApprovalSettings_NoPermission}
                </div>
            </div>
        );
    }

    render()
    {
        return <div id='raAccessPermissionForm'>
            <div className="category-main">
                <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_User_Permission_Scopes}</div>
                <div className="category-content">{this.renderScopePermissionInfo()}</div>
            </div>
            {
                (LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusGoogleLicense()) && 
                <div className="category-main">
                    <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_User_Permission_Terms}</div>
                    <div className="category-content">{this.renderTermPermissionInfo()}</div>
                </div>
            }
            
            <div className="category-main">
                <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_User_Permission_Rules}</div>
                <div className="category-content">{this.renderRulePermissionInfo()}</div>
            </div>
            {LicenseHelper.HasOpusILLicense() && this.renderUserRoleAndSubPermissions()}
            <div className="category-main">
                <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_Report}</div>
                <div className="category-content">{this.renderReportInfo()}</div>
            </div>
            {!LicenseHelper.HasOpusSOLicenseOnly() && (
                <>
                    <div className="category-main">
                        <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_ManageHolds}</div>
                        <div className="category-content">{this.renderManageHoldsInfo()}</div>
                    </div>
                    <div className="category-main">
                        <div className="category-title" tabIndex={0}>{RMResx.RM_CP_AM_ManageApprovalSettings}</div>
                        <div className="category-content">{this.renderManageApprovalSettingsInfo()}</div>
                    </div>
                </>
            )}
            <div className="category-main">
                <div className="category-title" tabIndex = {0}>{RMResx.RM_CP_AM_Permission_FunctionMoudle}</div>
                <div className="category-content">{`${RMResx.RM_CP_AM_Table_Column_RestoreCenterName}: ${RestoreCenterTypeTitle[this.props.functionMoudleRestoreCenter]}`}</div>
            </div>
        </div>;
    }
}