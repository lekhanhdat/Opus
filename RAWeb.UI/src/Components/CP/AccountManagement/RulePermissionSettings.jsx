import { DefaultSecurityGroup, RuleObjType, RulePermissionMethod } from "../../../Constants/Constants";
const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export default class RulePermissionSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.securityGroupId = this.props.groupId || -1;
        this.defaultPageIndex = 0;
        this.defaultPageSize = 10;
        this.cacheNodeInfo = this.getTreeRootNode();
        this.ruleSettings = this.getDefaultRuleSettings();
        this.state = {
            isSelectedAll: false,
            treeDataObj: {},
            disabled: this.getSelectAllDisablesStatus()
        };
        this.bind('handleCheckBoxChanged', 'handleSelectedAll', 'getSelectedItems', 'onRuleContainerPageChanged', 'onRuleItemPageChanged', 'updateCacheTreeNodeInfo',
            'initTreeNodeCache', 'onTermSetRowClick', 'treeNodeChecked');
    }

    componentInit() {
        if (this.securityGroupId == -1) {
            this.loadRuleContainerTree(false);
        }
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "edit":
                this.setRuleContainerTree(args[0]);
                break;
            case "save":
                this.saveRuleSettings(args[0]);
                break;
            case "reload":
                this.loadRuleContainerTree(false);
                break;
        }
    }

    getSelectAllDisablesStatus() {
        return DefaultSecurityGroup.BuiltInAdmin == this.securityGroupId;
    }

    loadRuleContainerTree(rootChecked) {
        let [pageIndex, pageSize] = [this.defaultPageIndex, this.defaultPageSize];
        let callback = (data) => {
            this.initTreeNodeCache(data.TermObjItems, rootChecked);
            this.initTreeDataObj(pageIndex, pageSize);
        };
        let reqOption = this.getRequestOption(GuidEmpty, RuleObjType.Root, pageIndex, pageSize);
        this.loadRuleObjData(callback, reqOption);
    }

    getRequestOption(pNodeId, pNodeType, pageIndex, pageSize) {
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

    loadRuleObjData(callback, reqOption) {
        $$.loading(true);
        let option = {
            url: `/api/CPApi/LoadRuleObjData`,
            method: "POST",
            data: reqOption
        };
        fetchUtility(option).then((result) => {
            if (result) {
                var data = JSON.parse(result);
                callback(data);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    saveRuleSettings(callback) {
        callback(RM.deepcopy(this.ruleSettings));
    }

    initTreeNodeCache(subNodes, isChecked) {
        let node = this.cacheNodeInfo;
        node.IsChecked = isChecked;
        node.SubItems = subNodes;
        node.SubItemCount = subNodes.length;
        node.SubPerIndex = this.defaultPageIndex;
        node.SubPerSize = this.defaultPageSize;
        this.initRuleItemsSelectedCount(node);
        node.SubItems.map(o => {
            //初始化RuleItem选中状态
            this.updateSubNodeCheckedStatus(o);
        });
    }

    initTreeDataObj(pageIndex, pageSize) {
        let rootCacheNode = RM.deepcopy(this.cacheNodeInfo);
        rootCacheNode.SubItems = this.getOnePageSubItems(this.cacheNodeInfo, pageIndex, pageSize); //Rule Container Items
        rootCacheNode.SubItems.map(tg => {
            tg.SubItems = this.getOnePageSubItems(tg, pageIndex, pageSize); //Rule Items
        });
        this.setState({
            treeDataObj: rootCacheNode
        });
    }

    getOnePageSubItems(parentNode, pageIndex, pageSize) {
        if (parentNode.SubItems && parentNode.SubItems.length > 0) {
            return JSON.parse(JSON.stringify(parentNode.SubItems.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)));
        }
        return [];
    }

    updateSubNodeCheckedStatus(treeNode) {
        if (treeNode.SubItems && treeNode.SubItems.length > 0) {
            treeNode.SubItems.map(o => {
                o.IsChecked = treeNode.IsChecked;
            });
        }
    }

    loadRuleItems(tGroupId) {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        let ruleContainerNode = treeRootNode.SubItems.find(o => o.UniqueId == tGroupId);
        if (ruleContainerNode) {
            let [pageIndex, pageSize] = [this.defaultPageIndex, this.defaultPageSize];
            //保存节点展开状态
            ruleContainerNode.IsExpand = true;
            if (!ruleContainerNode.IsLoaded) {
                console.log("need load term sets.");
                let callback = (data) => {
                    ruleContainerNode.SubItems = data.TermObjItems || [];
                    //当IsLoaded属性为true，将不再向后台发送请求
                    ruleContainerNode.IsLoaded = ruleContainerNode.SubItems.length > 0;
                    this.updateCacheTreeNodeInfo(ruleContainerNode, true, true);
                    //取出一页数据用于显示
                    ruleContainerNode.SubItems = this.getOnePageSubItems(ruleContainerNode, pageIndex, pageSize);
                    this.setState({
                        treeDataObj: treeRootNode
                    });
                };
                let reqOption = this.getRequestOption(ruleContainerNode.UniqueId, RuleObjType.RuleContainer, pageIndex, pageSize);
                this.loadRuleObjData(callback, reqOption);
            }
            else {
                this.updateCacheTreeNodeInfo(ruleContainerNode, false);
                this.setState({
                    treeDataObj: treeRootNode
                });
            }
        }
    }

    hideRuleItems(ruleContainer) {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        let ruleContainerNode = treeRootNode.SubItems.find(o => o.UniqueId == ruleContainer.UniqueId && o.Type == RuleObjType.RuleContainer);
        if (ruleContainerNode) {
            ruleContainerNode.IsExpand = false;
            this.updateCacheTreeNodeInfo(ruleContainerNode);
            this.setState({
                treeDataObj: treeRootNode
            });
        }
    }

    setRuleContainerTree(treeNodeInfo) {
        this.ruleSettings.treeNodeInfo = treeNodeInfo;
        let noSetRulePermission = !treeNodeInfo;
        let rootNodeChecked = !noSetRulePermission && treeNodeInfo.IsChecked;
        if (noSetRulePermission || rootNodeChecked) {
            //没设置权限/保存All权限
            this.setState({
                isSelectedAll: rootNodeChecked
            }, () => {
                this.loadRuleContainerTree(rootNodeChecked);
            });
        } else {
            this.cacheNodeInfo = treeNodeInfo;
            this.cacheNodeInfo.SubPerIndex = 0;
            let rootCacheNode = this.cacheNodeInfo;
            this.setSubNodesSelectedInfo(rootCacheNode);
            let treeRootNode = RM.deepcopy(rootCacheNode);
            treeRootNode.SubItems = this.getOnePageSubItems(rootCacheNode, this.defaultPageIndex, this.defaultPageSize);
            this.initRuleContainerNodes(treeRootNode);
            this.setState({
                treeDataObj: treeRootNode
            });
        }
    }

    initRuleContainerNodes(rootNode) {
        let ruleContainers = rootNode.SubItems;
        ruleContainers.forEach((tg) => {
            let ruleItems = tg.SubItems;
            tg.SubPerIndex = 0;
            if (!ruleItems) {
                tg.SubItems = [];
            }
            else if (ruleItems.length > this.defaultPageSize) {
                tg.SubItems = this.getOnePageSubItems(tg, this.defaultPageIndex, this.defaultPageSize);
            }
            tg.IsExpand = false;
        });
    }

    setSubNodesSelectedInfo(node) {
        let ruleContainers = node.SubItems;
        ruleContainers.forEach((tg) => {
            if (tg.SubItems) {
                tg.SubSelectedCount = this.getSubNodesCheckedCount(tg);
                if (tg.SubSelectedCount > 0 && tg.SubSelectedCount < tg.SubItems.length) {
                    tg.IsChecked = null;
                }
            }
        });
    }

    initRuleItemsSelectedCount(rootNode) {
        if (rootNode.IsChecked) {
            rootNode.SubItems.map(ruleContainer => {
                ruleContainer.SubSelectedCount = ruleContainer.SubItemCount;
                ruleContainer.IsChecked = rootNode.IsChecked;
            });
        } else {
            rootNode.SubItems.map(ruleContainer => {
                ruleContainer.SubSelectedCount = 0;
                ruleContainer.IsChecked = rootNode.IsChecked;
            });
        }
    }

    updateCacheTreeNodeInfo(ruleObj, isResetSubNodes, isResetSubNodeCheckedStatus) {
        let [rootCacheNode, needUpdateNode] = [this.cacheNodeInfo];
        switch (ruleObj.Type) {
            case RuleObjType.Root:
                needUpdateNode = rootCacheNode;
                break;
            case RuleObjType.RuleContainer:
                needUpdateNode = rootCacheNode.SubItems.find(o => o.UniqueId == ruleObj.UniqueId);
                break;
            case RuleObjType.Rule:
                var ruleContainerCacheNode = rootCacheNode.SubItems.find(o => o.UniqueId == ruleObj.ParentId);
                needUpdateNode = ruleContainerCacheNode.SubItems.find(o => o.UniqueId == ruleObj.UniqueId);
                break;
        }
        if (needUpdateNode) {
            needUpdateNode.SubPerIndex = ruleObj.SubPerIndex;
            needUpdateNode.SubPerSize = ruleObj.SubPerSize;
            needUpdateNode.IsExpand = ruleObj.IsExpand;
            needUpdateNode.IsChecked = ruleObj.IsChecked;
            needUpdateNode.SubSelectedCount = ruleObj.SubSelectedCount;
            if (isResetSubNodes) {
                needUpdateNode.SubItems = ruleObj.SubItems;
            }
            if (isResetSubNodeCheckedStatus) {
                this.updateSubNodeCheckedStatus(needUpdateNode);
            }
        }
    }

    updateParentCheckedStatus(curNode) {
        let rootNode = this.cacheNodeInfo;
        switch (curNode.Type) {
            case RuleObjType.Root:
                break;
            case RuleObjType.RuleContainer:
                let noCheckedRuleContainer = rootNode.SubItems.find(o => o.IsChecked == false);
                if (noCheckedRuleContainer) {
                    rootNode.IsChecked = false;
                }
                break;
            case RuleObjType.Rule:
                var pRuleContainerNode = rootNode.SubItems.find(o => o.UniqueId == curNode.ParentId);
                if (pRuleContainerNode) {
                    var allSubSelected = pRuleContainerNode.SubItems.every(o => o.IsChecked);
                    if (pRuleContainerNode.SubItems.find(o => o.IsChecked) && !allSubSelected) {
                        pRuleContainerNode.IsChecked = null;//Ui中checkbox显示半选状态
                    }
                    else {
                        pRuleContainerNode.IsChecked = allSubSelected;
                    }
                    rootNode.IsChecked = rootNode.SubItems.every(o => o.IsChecked);
                }
                break;
        }
    }

    getRuleContainerTreeInfo() {
        return this.cacheNodeInfo;
    }

    getTreeRootNode() {
        return {
            Id: -1,
            UniqueId: GuidEmpty,
            Name: "Groups",
            Type: RuleObjType.Root,
            IsExpand: true,
            IsChecked: false,
            SubPerIndex: 0,
            SubPerSize: 10,
            SubItemCount: 0,
            SubItems: []
        };
    }

    getSubNodesCheckedCount(treeNode) {
        if (!treeNode.SubItems) {
            return 0;
        }
        return treeNode.SubItems.filter(o => o.IsChecked).length;
    }

    handleCheckBoxChanged(item, checked, value) {
        if (value) {
            let isChecked = checked;
            item.IsChecked = isChecked;
            let treeRootNode = RM.deepcopy(this.state.treeDataObj);
            if (item.Type == RuleObjType.RuleContainer) {
                let curRuleContainer = treeRootNode.SubItems.find(o => o.UniqueId == item.UniqueId);
                let ruleContainerCacheNode = this.cacheNodeInfo.SubItems.find(o => o.UniqueId == item.UniqueId);
                if (ruleContainerCacheNode) {
                    //更新cache中rule container/ root 节点属性
                    ruleContainerCacheNode.IsChecked = isChecked;
                    this.updateParentCheckedStatus(ruleContainerCacheNode);
                    this.updateSubNodeCheckedStatus(ruleContainerCacheNode);
                    ruleContainerCacheNode.SubSelectedCount = this.getSubNodesCheckedCount(ruleContainerCacheNode);
                    //更新state中rule container节点属性
                    this.copyPropertyFromCache(ruleContainerCacheNode, curRuleContainer);
                }
            }
            else if (item.Type == RuleObjType.Rule) {
                //更新cache中rule item/rule container/ root节点属性
                let pRuleContainerCacheNode = this.cacheNodeInfo.SubItems.find(o => o.UniqueId == item.ParentId);
                let ruleItemCacheNode = pRuleContainerCacheNode.SubItems.find(o => o.UniqueId == item.UniqueId);
                ruleItemCacheNode.IsChecked = isChecked;
                this.updateParentCheckedStatus(ruleItemCacheNode);
                pRuleContainerCacheNode.SubSelectedCount = this.getSubNodesCheckedCount(pRuleContainerCacheNode);
                //更新state中rule container节点属性
                let pRuleContainerNode = treeRootNode.SubItems.find(o => o.UniqueId == item.ParentId);
                this.copyPropertyFromCache(pRuleContainerCacheNode, pRuleContainerNode);
            }

            treeRootNode.IsChecked = this.cacheNodeInfo.IsChecked;
            this.setState({
                treeDataObj: treeRootNode,
                isSelectedAll: treeRootNode.IsChecked
            }, () => {
                this.ruleSettings.treeNodeInfo = this.getRuleContainerTreeInfo();
            });
        }
    }

    copyPropertyFromCache(cacheNode, newNode) {
        switch (cacheNode.Type) {
            case RuleObjType.Root:
                var ruleContainerCacheNodes = cacheNode.SubItems;
                if (ruleContainerCacheNodes) {
                    ruleContainerCacheNodes.map(ruleContainerCacheNode => {
                        let ruleContainerNode = newNode.SubItems.find(o => o.UniqueId == ruleContainerCacheNode.UniqueId);
                        if (ruleContainerNode) {
                            ruleContainerNode.IsChecked = ruleContainerCacheNode.IsChecked;
                            ruleContainerNode.SubItems = this.getOnePageSubItems(ruleContainerCacheNode, ruleContainerCacheNode.SubPerIndex, ruleContainerCacheNode.SubPerSize);
                            ruleContainerNode.SubSelectedCount = ruleContainerCacheNode.SubSelectedCount;
                        }
                    });
                }
                break;
            case RuleObjType.RuleContainer:
                newNode.IsChecked = cacheNode.IsChecked;
                newNode.SubSelectedCount = cacheNode.SubSelectedCount;
                newNode.SubItems = this.getOnePageSubItems(cacheNode, newNode.SubPerIndex, newNode.SubPerSize);
                break;
            default:
                break;
        }
    }

    onRuleContainerPageChanged(pageIndex, pageSize) {
        var treeRootNode = RM.deepcopy(this.state.treeDataObj);
        treeRootNode.SubItems = this.getOnePageSubItems(this.cacheNodeInfo, pageIndex, pageSize);
        treeRootNode.SubPerIndex = pageIndex;
        treeRootNode.SubPerSize = pageSize;
        this.initRuleContainerNodes(treeRootNode);
        this.updateCacheTreeNodeInfo(treeRootNode, false);
        this.setState({
            treeDataObj: treeRootNode
        });
    }

    onRuleItemPageChanged(parentTermGroup, pageIndex, pageSize) {
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        var ruleContainer = treeRootNode.SubItems.find(o => o.UniqueId == parentTermGroup.UniqueId);
        let ruleContainerCacheNode = this.cacheNodeInfo.SubItems.find(o => o.UniqueId == parentTermGroup.UniqueId);
        ruleContainer.SubItems = this.getOnePageSubItems(ruleContainerCacheNode, pageIndex, pageSize);
        ruleContainer.SubPerIndex = pageIndex;
        ruleContainer.SubPerSize = pageSize;
        this.updateCacheTreeNodeInfo(ruleContainer, false);
        this.setState({
            treeDataObj: treeRootNode
        });
    }

    handleSelectedAll(checked) {
        let isChecked = checked;
        let rootCacheNode = this.cacheNodeInfo;
        this.updateTreeNodeCheckedStatus(rootCacheNode, isChecked);
        this.initRuleItemsSelectedCount(rootCacheNode);
        let treeRootNode = RM.deepcopy(this.state.treeDataObj);
        this.copyPropertyFromCache(rootCacheNode, treeRootNode);
        this.setState({
            isSelectedAll: isChecked,
            treeDataObj: treeRootNode
        }, () => {
            this.ruleSettings.treeNodeInfo = isChecked ? this.getRuleContainerTreeInfo() : null;
        });
    }

    updateTreeNodeCheckedStatus(rootNode, isChecked) {
        rootNode.IsChecked = isChecked;
        rootNode.SubItems.map((ruleContainer) => {
            ruleContainer.IsChecked = isChecked;
            if (ruleContainer.SubItems) {
                ruleContainer.SubItems.map((ruleItem) => {
                    ruleItem.IsChecked = isChecked;
                });
            }
        });
    }

    getDefaultRuleSettings() {
        return {
            treeNodeInfo: null,
            permissionMethod: RulePermissionMethod.None
        };
    }

    renderRuleContainers() {
        let rootNode = this.state.treeDataObj;
        let ruleContainers = rootNode.SubItems || [];
        let expanders = [];
        let hasPager = rootNode.SubItemCount > rootNode.SubPerSize;
        ruleContainers.map((item, index) => {
            let keyIndex = `g${index}`;
            let showEmptyRow = item.SubItems !== null && item.SubItems.length == 0;
            let hasRuleItems = item.SubItems && item.SubItems.length > 0;
            // let selectedCount = `(${RMResx.RM_CP_AM_SelectedCount.format(item.SubSelectedCount)})`;
            expanders.push(<R.Expander bgColor={"#E6E7E8"} lazyLoad={true} status={{ show: item.IsExpand }} key={keyIndex} onShow={this.loadRuleItems.bind(this, item.UniqueId)} onHide={this.hideRuleItems.bind(this, item)} >
                <div className="termGroup-row">
                    <R.Scope><R.Checkbox
                        text={item.Name}
                        title={item.Name}
                        value={item.UniqueId}
                        checked={item.IsChecked}
                        onChange={this.handleCheckBoxChanged.bind(this, item)}
                    />
                    </R.Scope>
                    {/* {item.SubSelectedCount > 0 && <span className="node-selected-count">{selectedCount}</span>} */}
                </div>
                <div className="termSets-container">
                    {hasRuleItems && this.renderRuleItems(item)}
                    {showEmptyRow && <div className="empty-row" key={1} tabIndex="0">{RMResx.RM_CP_AM_RulePermission_NoRuleItems}</div>}
                </div>
            </R.Expander>);
        });
        return <div>
            <div>{expanders}</div>
            {hasPager && <div className="pager-position">
                <$g.Pager
                    itemsCount={rootNode.SubItemCount}
                    pagerIndex={rootNode.SubPerIndex}
                    pagerSize={rootNode.SubPerSize}
                    showPagerSize={false}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onRuleContainerPageChanged} />
            </div>}
        </div>;
    }

    renderRuleItems(ruleContainerNode) {
        let ruleItems = ruleContainerNode.SubItems;
        if (ruleItems !== null) {
            let ruleItemsRow = [];
            let hasPager = ruleContainerNode.SubItemCount > ruleContainerNode.SubPerSize;
            ruleItems.map((item, index) => {
                let keyIndex = `s${index}`;
                ruleItemsRow.push(<div className="termSet-row text-overflow" key={keyIndex} data-tooltip="ifneed" tabIndex="0">
                    {item.Name}
                    {/* <R.Checkbox
                        text={item.Name}
                        title={item.Name}
                        value={item.UniqueId}
                        checked={item.IsChecked}
                        onChange={this.handleCheckBoxChanged.bind(this, item)}
                    /> */}
                </div>);
            });
            return <div>
                <div className="termSets-list">{ruleItemsRow}</div>
                {hasPager && <div className="pager-position">
                    <$g.Pager
                        itemsCount={ruleContainerNode.SubItemCount}
                        pagerIndex={ruleContainerNode.SubPerIndex}
                        pagerSize={ruleContainerNode.SubPerSize}
                        showPagerSize={false}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.onRuleItemPageChanged.bind(this, ruleContainerNode)} />
                </div>}
            </div>;
        }
    }

    render() {
        return <div id={this.props.id}>
            <div id="selAllContainer">
                <R.Checkbox
                    name="selAllRules"
                    text={RMResx.RM_CP_AM_RulePermission_AllRuleTitle} 
                    value={''}
                    disabled={this.state.disabled}
                    checked={this.state.isSelectedAll}
                    onChange={this.handleSelectedAll}
                />
            </div>
            {this.renderRuleContainers()}
        </div>;
    }
}