import { DefaultSecurityGroup, TermObjType, SetTermPermissionMethod } from "../../../Constants/Constants";
const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export default class TermPermissionSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.securityGroupId = this.props.groupId || -1;
        this.defaultPageIndex = 0;
        this.defaultPageSize = 10;
        this.cacheNodeInfo = this.getTreeRootNode();
        this.termSettings = this.getDefaultTermSettings();
        this.state = {
            isSelectedAll: false,
            treeDataObj: {},
            disabled: this.getSelectAllDisablesStatus()
        };
        this.bind('handleCheckBoxChanged', 'handleSelectedAll', 'getSelectedItems', 'onTermGroupPageChanged', 'onTermSetPageChanged', 'updateCacheTreeNodeInfo',
            'initTreeNodeCache', 'onTermSetRowClick', 'treeNodeChecked');
    }

    componentInit() {
        if(this.securityGroupId == -1)
        {
            this.loadTermTree(false);
        }
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "edit":
                this.setTermTree(args[0]);
                break;
            case "save":
                this.saveTermSettings(args[0]);
                break;
            case "reload":
                this.loadTermTree(false);
                break;
        }
    }

    getSelectAllDisablesStatus()
    {
        return DefaultSecurityGroup.BuiltInAdmin == this.securityGroupId;
    }

    loadTermTree(rootChecked)
    {
        let [pageIndex, pageSize] = [this.defaultPageIndex, this.defaultPageSize];
        let callback = (data) => {
            this.initTreeNodeCache(data.TermObjItems, rootChecked);
            this.initTreeDataObj(pageIndex, pageSize);
        };
        let reqOption = this.getRequestOption(GuidEmpty, TermObjType.Root, pageIndex, pageSize);
        this.loadTermObjData(callback, reqOption);
    }

    getRequestOption(pNodeId, pNodeType, pageIndex, pageSize)
    {
        return {
            ParentType: pNodeType,
            ParentId: pNodeId,
            PageInfo: {
                PagerIndex: pageIndex,
                PagerSize: pageSize
            },
            GroupId: this.securityGroupId
        };
    }

    loadTermObjData(callback, reqOption) {
        $$.loading(true);
        let option = {
            url: `/api/CPApi/LoadTermObjData`,
            method: "POST",
            data: reqOption
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if(result)
            {
                var data = JSON.parse(result);
                callback(data);
            }

        }).catch((e) => {
            $$.loading(false);
        });
    }

    saveTermSettings(callback)
    {
        callback(RM.deepcopy(this.termSettings));
    }

    initTreeNodeCache(subNodes, isChecked)
    {
        let node = this.cacheNodeInfo;
        node.IsChecked = isChecked;
        node.SubTerms = subNodes;
        node.SubTermCount = subNodes.length;
        node.SubPerIndex = this.defaultPageIndex;
        node.SubPerSize = this.defaultPageSize;
        this.initTermSetsSelectedCount(node);
        node.SubTerms.map(o => {
            //初始化TermSet选中状态
            this.updateSubNodeCheckedStatus(o);
        });
    }

    initTreeDataObj(pageIndex, pageSize)
    {
        let rootCacheNode = RM.deepcopy(this.cacheNodeInfo);
        rootCacheNode.SubTerms = this.getOnePageSubItems(this.cacheNodeInfo, pageIndex, pageSize); //Term Group Items
        rootCacheNode.SubTerms.map(tg => {
            tg.SubTerms = this.getOnePageSubItems(tg, pageIndex, pageSize); //Term Set Items
        });
        this.setState({
            treeDataObj: rootCacheNode
        });
    }

    getOnePageSubItems(parentNode, pageIndex, pageSize)
    {
        if(parentNode.SubTerms && parentNode.SubTerms.length > 0)
        {
            return JSON.parse(JSON.stringify(parentNode.SubTerms.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        }
        return [];
    }

    updateSubNodeCheckedStatus(treeNode)
    {
        if(treeNode.SubTerms && treeNode.SubTerms.length > 0)
        {
            treeNode.SubTerms.map(o => {
                o.IsChecked = treeNode.IsChecked;
            });
        }
    }

    loadTermSets(tGroupId)
    {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        let termGroupNode = treeRootNode.SubTerms.find(o => o.UniqueId == tGroupId);
        if(termGroupNode)
        {
            let [pageIndex, pageSize] = [this.defaultPageIndex, this.defaultPageSize];
            //保存节点展开状态
            termGroupNode.IsExpand = true;
            if(!termGroupNode.IsLoaded)
            {
                console.log("need load term sets.");
                let callback = (data) => {
                    termGroupNode.SubTerms = data.TermObjItems || [];
                    //当IsLoaded属性为true，将不再向后台发送请求
                    termGroupNode.IsLoaded = termGroupNode.SubTerms.length > 0;
                    this.updateCacheTreeNodeInfo(termGroupNode, true, true);
                    //取出一页数据用于显示
                    termGroupNode.SubTerms = this.getOnePageSubItems(termGroupNode, pageIndex, pageSize);
                    this.setState({
                        treeDataObj: treeRootNode
                    });
                };
                let reqOption = this.getRequestOption(termGroupNode.UniqueId, TermObjType.TermGroup, pageIndex, pageSize);
                this.loadTermObjData(callback, reqOption);
            }
            else 
            {
                this.updateCacheTreeNodeInfo(termGroupNode, false);
                this.setState({
                    treeDataObj: treeRootNode
                });
            }
        }
    }

    hideTermSets(termGroup)
    {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        let termGroupNode = treeRootNode.SubTerms.find(o => o.UniqueId == termGroup.UniqueId && o.Type == TermObjType.TermGroup);
        if(termGroupNode)
        {
            termGroupNode.IsExpand = false;
            this.updateCacheTreeNodeInfo(termGroupNode);
            this.setState({
                treeDataObj: treeRootNode
            });
        }
    }

    setTermTree(treeNodeInfo)
    {
        this.termSettings.treeNodeInfo = treeNodeInfo;
        let noSetTermPermission = !treeNodeInfo;
        let rootNodeChecked = !noSetTermPermission && treeNodeInfo.IsChecked;
        if(noSetTermPermission || rootNodeChecked)
        {
            //没设置权限/保存All权限
            this.setState({
                isSelectedAll: rootNodeChecked
            }, () => {
                this.loadTermTree(rootNodeChecked);
            });
        } else {
            this.cacheNodeInfo = treeNodeInfo;
            this.cacheNodeInfo.SubPerIndex = 0;
            let rootCacheNode = this.cacheNodeInfo;
            this.setSubNodesSelectedInfo(rootCacheNode);
            let treeRootNode = RM.deepcopy(rootCacheNode);
            treeRootNode.SubTerms = this.getOnePageSubItems(rootCacheNode, this.defaultPageIndex, this.defaultPageSize);
            this.initTermGroupNodes(treeRootNode);
            this.setState({
                treeDataObj: treeRootNode
            });
        }
    }

    initTermGroupNodes(rootNode)
    {
        let termGroups = rootNode.SubTerms;
        termGroups?.forEach((tg) => {
            let termSets = tg.SubTerms;
            tg.SubPerIndex = 0;
            if(!termSets)
            {
                tg.SubTerms = [];
            }
            else if(termSets.length > this.defaultPageSize)
            {
                tg.SubTerms = this.getOnePageSubItems(tg, this.defaultPageIndex, this.defaultPageSize);
            }
            tg.IsExpand = false;
        });
    }

    setSubNodesSelectedInfo(node)
    {
        let termGroups = node.SubTerms;
        termGroups?.forEach((tg) => {
            if(tg.SubTerms)
            {
                tg.SubSelectedCount = this.getSubNodesCheckedCount(tg);
                if(tg.SubSelectedCount > 0 && tg.SubSelectedCount < tg.SubTerms.length)
                {
                    tg.IsChecked = false;
                }
            }
        });
    }

    initTermSetsSelectedCount(rootNode)
    {
        if(rootNode.IsChecked)
        {
            rootNode.SubTerms.map(termGroup => {
                termGroup.SubSelectedCount = termGroup.SubTermCount;
                termGroup.IsChecked = rootNode.IsChecked;
            });
        } else {
            rootNode.SubTerms.map(termGroup => {
                termGroup.SubSelectedCount = 0;
                termGroup.IsChecked = rootNode.IsChecked;
            });
        }
    }

    updateCacheTreeNodeInfo(termObj, isResetSubNodes, isResetSubNodeCheckedStatus)
    {
        let [rootCacheNode, needUpdateNode] = [this.cacheNodeInfo];
        switch(termObj.Type)
        {
            case TermObjType.Root:
                needUpdateNode = rootCacheNode;
                break;
            case TermObjType.TermGroup:
                needUpdateNode = rootCacheNode.SubTerms.find(o => o.UniqueId == termObj.UniqueId);
                break;
            case TermObjType.TermSet:
                var termGroupCacheNode = rootCacheNode.SubTerms.find(o => o.UniqueId == termObj.ParentId);
                needUpdateNode = termGroupCacheNode.SubTerms.find(o => o.UniqueId == termObj.UniqueId);
                break;
        }
        if(needUpdateNode)
        {
            needUpdateNode.SubPerIndex = termObj.SubPerIndex;
            needUpdateNode.SubPerSize = termObj.SubPerSize;
            needUpdateNode.IsExpand = termObj.IsExpand;
            needUpdateNode.IsChecked = termObj.IsChecked;
            needUpdateNode.SubSelectedCount = termObj.SubSelectedCount;
            if(isResetSubNodes)
            {
                needUpdateNode.SubTerms = termObj.SubTerms;
            }
            if(isResetSubNodeCheckedStatus)
            {
                this.updateSubNodeCheckedStatus(needUpdateNode);
            }
        }
    }

    updateParentCheckedStatus(curNode)
    {
        let rootNode = this.cacheNodeInfo;
        switch(curNode.Type)
        {
            case TermObjType.Root:
                break;
            case TermObjType.TermGroup:
                let noCheckedTermGroup = rootNode.SubTerms.find(o => o.IsChecked == false);
                if (noCheckedTermGroup) {
                    rootNode.IsChecked = false;
                }
                break;
            case TermObjType.TermSet:
                var pTermGroupNode = rootNode.SubTerms.find(o => o.UniqueId == curNode.ParentId);
                if(pTermGroupNode)
                {
                    var allSubSelected = pTermGroupNode.SubTerms.every(o => o.IsChecked);
                    if (pTermGroupNode.SubTerms.find(o => o.IsChecked) && !allSubSelected) {
                        pTermGroupNode.IsChecked = false;//Ui中checkbox显示半选状态
                    } else {
                        if (!allSubSelected) {
                            pTermGroupNode.IsChecked = false;
                        }
                    }

                    let noCheckedTermSet = pTermGroupNode.SubTerms.find(o => o.IsChecked == false);
                    if (noCheckedTermSet) {
                        rootNode.IsChecked = false;
                    }
                }
                break;
        }
    }

    getTermTreeInfo() {
        return this.cacheNodeInfo;
    }

    getTreeRootNode()
    {
        return {
            Id: -1,
            UniqueId: GuidEmpty,
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

    getSubNodesCheckedCount(treeNode)
    {
        if(!treeNode.SubTerms)
        {
            return 0;
        }
        return treeNode.SubTerms.filter(o => o.IsChecked).length;
    }

    handleCheckBoxChanged(item, checked, value) {
        if (value) {
            let isChecked = checked;
            item.IsChecked = isChecked;
            let treeRootNode = RM.deepcopy(this.state.treeDataObj);
            if(item.Type == TermObjType.TermGroup)
            {
                let curTermGroup = treeRootNode.SubTerms.find(o => o.UniqueId == item.UniqueId);
                let termGroupCacheNode = this.cacheNodeInfo.SubTerms.find(o => o.UniqueId == item.UniqueId);
                if(termGroupCacheNode)
                {
                    //更新cache中term group/ root 节点属性
                    termGroupCacheNode.IsChecked = isChecked;
                    this.updateParentCheckedStatus(termGroupCacheNode);
                    this.updateSubNodeCheckedStatus(termGroupCacheNode);
                    termGroupCacheNode.SubSelectedCount = this.getSubNodesCheckedCount(termGroupCacheNode);
                    //更新state中term group节点属性
                    this.copyPropertyFromCache(termGroupCacheNode, curTermGroup);
                }
            }
            else if(item.Type == TermObjType.TermSet)
            {
                //更新cache中term set/term group/ root节点属性
                let pTermGroupCacheNode = this.cacheNodeInfo.SubTerms.find(o => o.UniqueId == item.ParentId);
                let termSetCacheNode = pTermGroupCacheNode.SubTerms.find(o => o.UniqueId == item.UniqueId);
                termSetCacheNode.IsChecked = isChecked;
                this.updateParentCheckedStatus(termSetCacheNode);
                pTermGroupCacheNode.SubSelectedCount = this.getSubNodesCheckedCount(pTermGroupCacheNode);
                //更新state中term group节点属性
                let pTermGroupNode = treeRootNode.SubTerms.find(o => o.UniqueId == item.ParentId);
                this.copyPropertyFromCache(pTermGroupCacheNode, pTermGroupNode);
            }

            treeRootNode.IsChecked = this.cacheNodeInfo.IsChecked;
            this.setState({
                treeDataObj: treeRootNode,
                isSelectedAll: treeRootNode.IsChecked
            }, () => {
                this.termSettings.treeNodeInfo = this.getTermTreeInfo();
            });
        }
    }

    copyPropertyFromCache(cacheNode, newNode)
    {
        switch(cacheNode.Type)
        {
            case TermObjType.Root:
                var termGroupCacheNodes = cacheNode.SubTerms;
                if(termGroupCacheNodes)
                {
                    termGroupCacheNodes.map(termGroupCacheNode => {
                        let termGroupNode = newNode.SubTerms.find(o => o.UniqueId == termGroupCacheNode.UniqueId);
                        if(termGroupNode)
                        {
                            termGroupNode.IsChecked = termGroupCacheNode.IsChecked;
                            termGroupNode.SubTerms = this.getOnePageSubItems(termGroupCacheNode, termGroupCacheNode.SubPerIndex, termGroupCacheNode.SubPerSize);
                            termGroupNode.SubSelectedCount = termGroupCacheNode.SubSelectedCount;
                        }
                    });
                }
                break;
            case TermObjType.TermGroup:
                newNode.IsChecked = cacheNode.IsChecked;
                newNode.SubSelectedCount = cacheNode.SubSelectedCount;
                newNode.SubTerms = this.getOnePageSubItems(cacheNode, newNode.SubPerIndex, newNode.SubPerSize);
                break;
            default:
                break;
        }
    }

    onTermGroupPageChanged(pageIndex, pageSize)
    {
        var treeRootNode = RM.deepcopy(this.state.treeDataObj);
        treeRootNode.SubTerms = this.getOnePageSubItems(this.cacheNodeInfo, pageIndex, pageSize);
        treeRootNode.SubPerIndex = pageIndex;
        treeRootNode.SubPerSize = pageSize;
        this.initTermGroupNodes(treeRootNode);
        this.updateCacheTreeNodeInfo(treeRootNode, false);
        this.setState({
            treeDataObj: treeRootNode
        });
    }

    onTermSetPageChanged(parentTermGroup, pageIndex, pageSize)
    {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        var termGroup = treeRootNode.SubTerms.find(o => o.UniqueId == parentTermGroup.UniqueId);
        let termGroupCacheNode = this.cacheNodeInfo.SubTerms.find(o => o.UniqueId == parentTermGroup.UniqueId);
        termGroup.SubTerms = this.getOnePageSubItems(termGroupCacheNode, pageIndex, pageSize);
        termGroup.SubPerIndex = pageIndex;
        termGroup.SubPerSize = pageSize;
        this.updateCacheTreeNodeInfo(termGroup, false);
        this.setState({
            treeDataObj: treeRootNode
        });
    }

    handleSelectedAll(checked) {
        let isChecked = checked;
        let rootCacheNode = this.cacheNodeInfo;
        this.updateTreeNodeCheckedStatus(rootCacheNode, isChecked);
        this.initTermSetsSelectedCount(rootCacheNode);
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        this.copyPropertyFromCache(rootCacheNode, treeRootNode);
        this.setState({
            isSelectedAll: isChecked,
            treeDataObj: treeRootNode
        }, () => {
            this.termSettings.treeNodeInfo = isChecked ? this.getTermTreeInfo() : null;
        });
    }

    updateTreeNodeCheckedStatus(rootNode, isChecked)
    {
        rootNode.IsChecked = isChecked;
        rootNode.SubTerms.map((termGroup) => {
            termGroup.IsChecked = isChecked;
            if(termGroup.SubTerms)
            {
                termGroup.SubTerms.map((termSet) => {
                    termSet.IsChecked = isChecked;
                });
            }
        });
    }

    getDefaultTermSettings()
    {
        return {
            treeNodeInfo: null,
            permissionMethod: SetTermPermissionMethod.None
        };
    }

    renderTermGroups()
    {
        let rootNode = this.state.treeDataObj;
        let termGroupItems = rootNode.SubTerms || [];
        let expanders = [];
        let hasPager = rootNode.SubTermCount > rootNode.SubPerSize;
        termGroupItems.map((item, index) => {
            let keyIndex = `g${index}`;
            let showEmptyRow = item.SubTerms !== null && item.SubTerms.length == 0;
            let hasSubTermSets = item.SubTerms && item.SubTerms.length > 0;
            let selectedCount = `(${RMResx.RM_CP_AM_SelectedCount.format(item.SubSelectedCount)})`;
            expanders.push(<R.Expander bgColor={"#E6E7E8"} lazyLoad={true} status={{ show: item.IsExpand }} key={keyIndex} onShow={this.loadTermSets.bind(this, item.UniqueId)} onHide={this.hideTermSets.bind(this, item)} >
                <div className="termGroup-row">
                    <R.Scope><R.Checkbox
                        text={item.Name}
                        title={item.Name}
                        value={item.UniqueId}
                        checked={item.IsChecked}
                        onChange={this.handleCheckBoxChanged.bind(this, item)}
                    />
                    </R.Scope>
                    {item.SubSelectedCount > 0 && <span className="node-selected-count">{selectedCount}</span>}
                </div>
                <div className="termSets-container">
                    {hasSubTermSets && this.renderTermSets(item)}
                    {showEmptyRow && <div className="empty-row" key={1} tabIndex="0">{RMResx.RM_CP_AM_TermPermission_NoTermSets}</div>}
                </div>
            </R.Expander>);
        });
        return <div>
            <div>{expanders}</div>
            {hasPager && <div className="tg-pager-position">
                <$g.Pager
                    itemsCount={rootNode.SubTermCount}
                    pagerIndex={rootNode.SubPerIndex}
                    pagerSize={rootNode.SubPerSize}
                    showPagerSize={false}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onTermGroupPageChanged} />
            </div>}
        </div>;
    }

    renderTermSets(termGroupNode)
    {
        let termSetItems = termGroupNode.SubTerms;
        if(termSetItems !== null)
        {
            let termSetsRow = [];
            let hasPager = termGroupNode.SubTermCount > termGroupNode.SubPerSize;
            termSetItems.map((item, index) => {
                let keyIndex = `s${index}`;
                termSetsRow.push(<div className="termSet-row text-overflow" key={keyIndex} data-tooltip="ifneed">
                    <R.Checkbox
                        text={item.Name}
                        title={item.Name}
                        value={item.UniqueId}
                        checked={item.IsChecked}
                        onChange={this.handleCheckBoxChanged.bind(this, item)}
                    />
                </div>);
            });
            return <div>
                <div className="termSets-list">{termSetsRow}</div>
                {hasPager && <div className="pager-position">
                    <$g.Pager
                        itemsCount={termGroupNode.SubTermCount}
                        pagerIndex={termGroupNode.SubPerIndex}
                        pagerSize={termGroupNode.SubPerSize}
                        showPagerSize={false}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.onTermSetPageChanged.bind(this, termGroupNode)} />
                </div>}
            </div>;
        }
    }

    render() {
        return <div id={this.props.id}>
            <div id="selAllContainer">
                <R.Checkbox
                    name="selAllTermSets"
                    text={RMResx.RM_CP_AM_TermPermission_AllTermTitle}
                    value={''}
                    disabled={this.state.disabled}
                    checked={this.state.isSelectedAll}
                    onChange={this.handleSelectedAll}
                />
            </div>
            {this.renderTermGroups()}
        </div>;
    }
}