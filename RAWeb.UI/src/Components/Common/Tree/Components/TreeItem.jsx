import TreeList from './TreeList';
import DefaultNodeContent from '../NodeContents/DefaultNodeContent';
import { TreeContextMenuTrigger } from '../Components/TreeActions';
import { bindEvents } from '../../../../Utilities/CommonUtil';
import { pagerModes } from './Constants';

export default class TreeItem extends React.Component {
    constructor(props) {
        super(props);
        let item = props.item;
        this.treeContext = props.treeContext;
        this.exactPaging = props.item.exactPaging !== false;
        this.allChildren = null;    //for web front end pager
        this.isUnmount = false;
        this.childItemComponents = {};
        this.pagerAnchorCache = {}; //exactPaging = false时有用
        this.isCommonNode = item.nodeType == this.treeContext.commonNodeTypes.selectAll || item.nodeType == this.treeContext.commonNodeTypes.includeNew;
        if (!this.exactPaging) {
            this.pagerAnchorCache[item.pagerIndex] = item.pagerAnchor;
        }
        this.initSelectedStatus(item);
        this.state = {
            loading: false,
            selected: item.checked === "mixed" ? "mixed" : !!item.checked,
            expanded: item.expanded,
            items: this.processItems(item.items, item.pagerIndex, item.pagerByServer),
            itemsCount: item.itemsCount,
            pagerIndex: item.pagerIndex
        };

        bindEvents(this, "onChange", "onNodeClick", "onNodeKeyDown", "clearSelectedStatus",
            "setSelectedStatus", "toggleExpand", "onExpandBtnKeyDown", "loadNodes", "reload",
            "appendNodeItem", "loading", "onCheckedChange");
    }

    componentDidMount() {
        if (this.props.parentItemComponent) {
            this.props.parentItemComponent.childItemComponents[this.props.item.nodeKey] = this;
        }
    }

    componentWillUnmount() {
        if (this.props.parentItemComponent) {
            delete this.props.parentItemComponent.childItemComponents[this.props.item.nodeKey];
        }
        this.isUnmount = true;
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item || nextProps.item.items != this.props.item.items) {
            let item = nextProps.item;
            if (!this.exactPaging) {
                this.pagerAnchorCache[item.pagerIndex] = item.pagerAnchor;
            }
            this.initSelectedStatus(item);
            this.setState({
                selected: !!item.checked,
                expanded: item.expanded,
                items: this.processItems(item.items, item.pagerIndex, item.pagerByServer),
                itemsCount: item.itemsCount,
                pagerIndex: item.pagerIndex
            });
        } else {
            this.initSelectedStatus(nextProps.item);
            this.setState({
                selected: nextProps.item.checked === "mixed" ? "mixed" : !!nextProps.item.checked
            });
        }
    }

    componentDidUpdate(prevProps, prevState) {
    }

    reload(countOffset) {
        let itemsCount = this.state.itemsCount + countOffset;
        let pagerCount = Math.ceil(itemsCount / this.props.item.pagerSize);
        let pagerIndex = this.state.pagerIndex >= pagerCount - 1 ? pagerCount - 1 : this.state.pagerIndex;
        if (pagerIndex < 0) { pagerIndex = 0; }
        this.setState({
            itemsCount: itemsCount,
            pagerIndex: pagerIndex
        });
        this.loadNodes(pagerIndex);
    }

    removeEmptyNode() {
        let children = this.state.items;
        let emptyCount = 0;
        children = children.filter(function (i) {
            if (!i.nodeKey) {
                emptyCount++;
                return false;
            }
            return true;
        });

        this.setState({
            items: children,
            itemsCount: this.state.itemsCount - emptyCount
        });

        this.allChildren = this.allChildren.filter((i) => !!i.nodeKey);
    }

    removeChildrenNodeItem(item){
        this.state.items = this.state.items.filter((i)=>i.nodeKey != item.nodeKey);
        this.allChildren = this.allChildren.filter((i)=>i.nodeKey != item.nodeKey);
        this.setState(prevState => ({
            // checked = false to unselect the checked node: UNSAFE_componentWillReceiveProps
            items: prevState.items.map((item) => ({ ...item, checked: false })),
            itemsCount: prevState.itemsCount + 1,
            expanded: true
        }));
    }

    appendNodeItem(item) {
        let appendChild = () => {
            this.state.items.push(item);
            this.allChildren.push(item);
            this.setState({
                items: this.state.items,
                itemsCount: this.state.itemsCount + 1,
                expanded: true
            });
        };

        if (!this.props.item.loaded ||
            !this.state.expanded && this.state.itemsCount > 0
            && (!this.state.items || this.state.items.length == 0)) {
            this.loadNodes(0, this.props.item.pagerSize, (success) => {
                appendChild();
            });
        } else {
            appendChild();
        }
    }

    getNodeClassName(){
        let nodeClassname = "ra-tree-node";
        if(this.state.selected && this.treeContext.singleSelection){
            nodeClassname += " ra-tree-node-checked";
        }
        return nodeClassname;
    }

    // getNodeClassName() {
    //     let item = this.props.item;
    //     let nodeClassname = "ra-tree-node";
    //     if(this.props.item.nodeClass){
    //         nodeClassname += " " + this.props.item.nodeClass;
    //     }
    //     if (item.disableSelect) {
    //         nodeClassname += " ra-tree-node-disableSelect";
    //     } else if (this.state.selected) {
    //         nodeClassname += " ra-tree-node-selected";
    //     }
    //     if(this.treeContext.multiSelection) {
    //         nodeClassname += " ra-tree-multiSelection";
    //     }
    //     return nodeClassname;
    // }

    // getExpandClassName() {
    //     let expandClassName = "ra-tree-node-expand";
    //     if (this.state.loading) {
    //         expandClassName += " none";
    //     } else {
    //         expandClassName += this.props.item.isLeafNode || !this.props.item.hasChildren ? " invisible" : " visible";
    //         expandClassName += this.state.expanded ? " fia-tree-show" : " fia-tree-hide";
    //     }
    //     return expandClassName;
    // }

    updateOriginObject(item) {
        if (item.origin) {
            this.treeContext.updateOriginObject(item);
        }
    }

    initSelectedStatus(item) {
        if (this.treeContext.isMoveToRefresh) {
            if (this.props.item.nodeType == 9200 || this.props.item.nodeType == 9300) {
                let exist = false;
                for (let index = 0; index < this.treeContext.expandBottomLocationAndBoxNodes.length; index++) {
                    const item = this.treeContext.expandBottomLocationAndBoxNodes[index];
                    if (item.props.item.origin.Id == this.props.item.Id) {
                        exist = true;
                        break;
                    }
                }
                if (!exist) {
                    this.treeContext.expandBottomLocationAndBoxNodes.push(this);
                }
            }
        }

        let selNodes = this.treeContext.selectedNodes;
        if (item.checked) {
            if (!selNodes) {
                selNodes = this.treeContext.selectedNodes = {};
            }
            selNodes[item.nodeKey] = this;
        } else if (selNodes) {
            if (selNodes[item.nodeKey]) {
                delete selNodes[item.nodeKey];
            }
        }
    }

    clearSelectedStatus() {
        this.props.item.checked = false;
        this.updateOriginObject(this.props.item);
        if (!this.isUnmount) {
            if (this.state.selected) {
                this.setState({ selected: false });
            }
        }
    }

    setSelectedStatus() {
        let item = this.props.item;
        item.checked = true;
        if (item.enableIncludeNew) {
            item.includeNew = true;
        }
        item.selectAll = true;
        this.updateOriginObject(this.props.item);

        let treeCxt = this.props.treeContext;
        let allowMultiSelected = treeCxt.multiSelection;
        let selNodes = treeCxt.selectedNodes = treeCxt.selectedNodes || {};
        if (!allowMultiSelected) {
            for (const key in selNodes) {
                if (key != this.props.item.nodeKey) {
                    let selNode = selNodes[key];
                    selNode.clearSelectedStatus();
                }
            }
            treeCxt.selectedNodes = {};
        }
        treeCxt.selectedNodes[this.props.item.nodeKey] = this;
        this.setState({ selected: true });
    }

    onCheckboxOrRadioClick(e) {
        e.stopPropagation();
    }

    onCheckedChange(checked) {
        this.processCheckedChange(checked);
        this.notifyNodeSelectedChange();
    }

    resetSelectedStatus = () => {
        this.setState({
            selected: false
        });
    };

    focusRadioInput = () => {
        setTimeout(() => {
            $(this.nodeElement).find('aui-radio input[type="radio"]').focus();
        }, 0);
    }

    onSingleCheckedChange = (value) => {
        this.resetSelectedStatus();
        let treeCxt = this.props.treeContext;
        if (treeCxt.multiSelection) {
            this.processCheckedChange(!this.state.selected);
            this.notifyNodeSelectedChange();
        } else if (!this.state.selected) {
            if (treeCxt.confirmOnNodeSelected) {
                treeCxt.confirmOnNodeSelected(this.props.item, (allow) => {
                    if (allow) {
                        this.setSelectedStatus();
                        this.notifyNodeSelectedChange();
                        this.focusRadioInput();
                    }
                });
            } else if (treeCxt.onNodeSelected) {
                this.setSelectedStatus();
                treeCxt.onNodeSelected(this.props.item);
                this.notifyNodeSelectedChange();
                this.focusRadioInput();
            }
        }
    }

    onTreeChanged() {
        if (this.treeContext.onTreeChanged) {
            this.treeContext.onTreeChanged();
        }
    }

    processCheckedChange(checked) {
        let pComponent = this.props.parentItemComponent;
        let item = this.props.item;
        item.checked = checked;
        if (item.enableIncludeNew) {
            item.includeNew = checked;
        }
        item.selectAll = checked;
        if (item.nodeType != this.treeContext.commonNodeTypes.selectAll
            && item.nodeType != this.treeContext.commonNodeTypes.includeNew) {
            item.selectAllBefore = checked;
        }
        this.updateOriginObject(item);
        if (item.nodeType == this.treeContext.commonNodeTypes.selectAll) {
            this.processChildrenOnSelected(pComponent, checked, false);
            let highLevelComponent = this.processParentOnSelected(this, checked);
            highLevelComponent.reRender();
        } else if (item.nodeType == this.treeContext.commonNodeTypes.includeNew) {
            let highLevelComponent = this.processParentOnSelected(this, checked);
            highLevelComponent.reRender();
        } else {
            this.processChildrenOnSelected(this, checked, true);
            if (this.treeContext.allowSelectedWithoutChildren) {
                this.reRender();
            } else {
                let highLevelComponent = this.processParentOnSelected(this, checked);
                highLevelComponent.reRender();
            }
        }
    }

    processChildrenOnSelected(pComponent, checked, containsIncludeNew) {
        let pItem = pComponent.props.item;
        let allChildren = this.treeContext.searchKey ?
            this.treeContext.getAllChildren(pItem.origin) :
            pComponent.allChildren;
        $.each(allChildren, (idx, child) => {
            let childItem = this.treeContext.transToTreeNodeObject(child);
            childItem.checked = checked;
            if (childItem.enableIncludeNew) {
                childItem.includeNew = checked;
            }
            childItem.selectAll = checked;
            if (childItem.nodeType != this.treeContext.commonNodeTypes.selectAll) {
                childItem.selectAllBefore = checked;
            }
            this.updateOriginObject(childItem);
        });
        $.each(pComponent.childItemComponents, (idx, childComponent) => {
            let childItem = childComponent.props.item;
            if (childItem.nodeType == this.treeContext.commonNodeTypes.includeNew) {
                if (containsIncludeNew) {
                    childItem.checked = checked;
                }
            } else {
                childItem.checked = checked;
                if (childItem.nodeType != this.treeContext.commonNodeTypes.selectAll) {
                    if (childItem.enableIncludeNew) {
                        childItem.includeNew = checked;
                    }
                    childItem.selectAll = checked;
                    childItem.selectAllBefore = checked;
                    this.updateOriginObject(childItem);
                    this.processChildrenOnSelected(childComponent, checked, true);
                }
            }
        });
    }
    processParentOnSelected(itemComponent, checked) {
        let pComponent = itemComponent.props.parentItemComponent;
        if (!pComponent) {
            return itemComponent;
        }
        let pCheckedChange = false;
        let item = itemComponent.props.item;
        let pItem = pComponent.props.item;
        let poItem = pItem.origin;
        if (item.nodeType == this.treeContext.commonNodeTypes.selectAll) {
            pItem.selectAllBefore = checked;
            pItem.selectAll = checked;
            if (!checked && pItem.checked) {
                pItem.checked = false;
                pCheckedChange = true;
            } else if (checked && !pItem.checked && pItem.includeNew) {
                pItem.checked = true;
                pCheckedChange = true;
            }
        } else if (item.nodeType == this.treeContext.commonNodeTypes.includeNew) {
            pItem.includeNew = checked;
            if (!checked && pItem.checked) {
                pItem.checked = false;
                pCheckedChange = true;
            } else if (checked && !pItem.checked && pItem.selectAll) {
                pItem.checked = true;
                pCheckedChange = true;
            }
        } else {
            if(pItem.isHasMixedStatus){
                if(pItem.checked == true){
                    pItem.checked = "mixed";
                }
                if(pItem.checked == false){
                    pItem.checked = false;
                }
            }else{
                if (checked) {
                    let isAllSelected = true;
                    let allChildren = this.treeContext.searchKey ?
                        this.treeContext.getAllChildren(poItem) :
                        pComponent.allChildren;
                    $.each(allChildren, (idx, child) => {
                        let childItem = this.treeContext.transToTreeNodeObject(child);
                        if (!childItem.checked) {
                            isAllSelected = false;
                            return false;
                        }
                    });
    
                    if (isAllSelected) {
                        if (pItem.enableIncludeNew || pItem.onlySupportSelectAll) {
                            pItem.selectAll = true;
                            if (pItem.includeNew) {
                                pItem.checked = true;
                                pCheckedChange = true;
                            }
                        } else {
                            pItem.checked = true;
                            pCheckedChange = true;
                        }
                    }
                } else {
                    if ((pItem.enableIncludeNew || pItem.onlySupportSelectAll) && pItem.selectAll) {
                        pItem.selectAll = false;
                        if (pItem.checked) {
                            pCheckedChange = true;
                        }
                        pItem.checked = false;
                    } else if (!(pItem.enableIncludeNew || pItem.onlySupportSelectAll) && pItem.checked) {
                        pItem.checked = false;
                        pCheckedChange = true;
                    }
                }
            }
        }

        this.updateOriginObject(pItem);
        if (pCheckedChange) {
            return this.processParentOnSelected(pComponent, checked);
        } else {
            return pComponent;
        }
    }
    
    notifyNodeSelectedChange() {
        this.onTreeChanged();
        let selectedChange = this.props.treeContext.onNodeSelectedChange;
        if (selectedChange) {
            selectedChange(this.props.item);
        }
    }

    onNodeKeyDown(e) {
        if (this.treeContext.readonly) {
            return;
        }
        if (e.keyCode === 13) {
            e.stopPropagation();
            this.toggleExpand(e);
        } else {
            if (e.keyCode == 38) {//up
                e.preventDefault();
                let parentItem = $(e.target).closest(".ra-tree-item");
                let topItem = parentItem;
                let prevItem, prevNode = [];
                while (parentItem.length > 0 && prevNode.length == 0) {
                    prevItem = parentItem;
                    //查找上一个可选的兄弟节点
                    do {
                        prevItem = prevItem.prev(".ra-tree-item");
                        prevNode = prevItem.children(".ra-tree-node:not(.ra-tree-node-disableSelect)");
                    } while (prevItem.length > 0 && prevNode.length == 0);
                    //查找上一个可选父节点
                    if (prevNode.length == 0) {
                        topItem = parentItem;
                        parentItem = parentItem.parent().closest(".ra-tree-item");
                        prevNode = parentItem.children(".ra-tree-node:not(.ra-tree-node-disableSelect)");
                    }
                }
                if (prevNode.length > 0) {
                    prevNode.focus();
                }
            } else if (e.keyCode == 40) {//down
                e.preventDefault();
                let hasNextNode = false;
                let nextList = $(e.target).next(".ra-tree-list");
                if (nextList.length > 0 && nextList.css("display") != "none") {
                    let nextNode = nextList.find(".ra-tree-node:not(.ra-tree-node-disableSelect)").eq(0);
                    if (nextNode.length > 0) {
                        nextNode.focus();
                        hasNextNode = true;
                    }
                }
                if (!hasNextNode) {
                    let parentItem = $(e.target).closest(".ra-tree-item");
                    let topItem = parentItem;
                    let nextItem = parentItem.next(".ra-tree-item");
                    while (parentItem.length > 0 && nextItem.length == 0) {
                        topItem = parentItem;
                        parentItem = parentItem.parent().closest(".ra-tree-item");
                        nextItem = parentItem.next(".ra-tree-item");
                    }
                    if (nextItem.length > 0) {
                        nextItem.children(".ra-tree-node").focus();
                    } else {
                        topItem.find(".ra-tree-node:not(.ra-tree-node-disableSelect)").focus();
                    }
                }
            }
        }
    }


    onChange(e) {
        this.props.onChange(e);
    }

    onExpandBtnKeyDown(e) {
        if (e.keyCode == 13) {
            this.toggleExpand(e);
        }
    }

    toggleExpand(e) {
        if (e) {
            e.stopPropagation();
        }

        let item = this.props.item;
        if (item.isLeafNode || (this.treeContext.readonly && !item.loaded)) {
            return;
        }

        if (!item.loaded ||
            !this.state.expanded && this.state.itemsCount > 0
            && (!this.state.items || this.state.items.length == 0)) {
            this.loadNodes(0, item.pagerSize);
        } else {
            this.onTreeChanged();
            item.expanded = !this.state.expanded;
            this.updateOriginObject(item);
            this.setState({ expanded: item.expanded });
            this.treeContext.onExpandClick(item, item.expanded);
        }
    }

    focusNode(){
        setTimeout(()=>{
            $(`#${this.props.item.nodeType}${this.props.item.nodeKey}`).focus();
        }, 300);
    }

    getItemUniqueKey(item) {
        return `${item.nodeType}_${item.nodeKey}`;
    }

    isLoadMoreMode(item) {
        return item && item.pagerMode === pagerModes.loadMore;
    }

    mergeLoadedItems(oldItems, newItems) {
        const keyMap = {};
        const merged = [];

        (oldItems || []).forEach((item) => {
            const key = this.getItemUniqueKey(item);
            if (!keyMap[key]) {
                keyMap[key] = true;
                merged.push(item);
            }
        });

        (newItems || []).forEach((item) => {
            const key = this.getItemUniqueKey(item);
            if (!keyMap[key]) {
                keyMap[key] = true;
                merged.push(item);
            }
        });

        return merged;
    }

    onLoadMoreClick = (e) => {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }
        this.loadNodes((this.state.pagerIndex || 0) + 1);
    }

    loadNodes(pagerIndex, pagerSize, callback) {
        if (this.state.loading) {
            return;
        }
        this.onTreeChanged();
        let item = this.props.item;
        const isLoadMoreMode = this.isLoadMoreMode(item);
        if (!this.exactPaging) {
            if (pagerIndex < item.pagerIndex) {
                item.hasNextPage = true;
            }
            if (pagerIndex == 0) {
                this.pagerAnchorCache = {};
                item.pagerAnchor = null;
            } else {
                let pagerAnchor = this.pagerAnchorCache[pagerIndex - 1];
                if (pagerAnchor) {
                    item.pagerAnchor = pagerAnchor;
                }
            }
        }
        if (pagerSize) {
            item.pagerSize = pagerSize;
        }
        let loaded = item.loaded;
        item.expanded = true;
        item.loaded = true;
        item.pagerIndex = pagerIndex;
        this.updateOriginObject(item);
        if (item.pagerByServer || !loaded) {
            this.setState({ loading: true, pagerIndex: pagerIndex });
            this.treeContext.onLoadNodes(
                item,
                (children, oNewItem) => {
                    
                    if (oNewItem) {
                        Object.assign(item.origin, oNewItem);
                        Object.assign(item, this.treeContext.transToTreeNodeObject(item.origin));
                        this.pagerAnchorCache[item.pagerIndex] = item.pagerAnchor;
                    }
                    let iCount = !item.pagerByServer ? children.length : item.itemsCount;
                    let renderedItems = this.processItems(children, pagerIndex, item.pagerByServer);
                    if(isLoadMoreMode){
                        iCount = renderedItems.length;
                    }
                    if (isLoadMoreMode && pagerIndex > 0) {
                        renderedItems = this.mergeLoadedItems(this.state.items, renderedItems);
                    }
                    this.setState({
                        items: renderedItems,
                        expanded: true,
                        loading: false,
                        itemsCount: iCount
                    });
                    this.treeContext.onExpandClick(item, true);
                    if (callback) callback(true);
                },
                () => {
                    this.setState({
                        loading: false,
                    });
                }
            );
        } else {
            let renderedItems = this.processItems(this.allChildren, pagerIndex, item.pagerByServer);
            if (isLoadMoreMode && pagerIndex > 0) {
                renderedItems = this.mergeLoadedItems(this.state.items, renderedItems);
            }
            this.setState({
                items: renderedItems,
                itemsCount: isLoadMoreMode ? renderedItems.length : this.state.itemsCount,
                pagerIndex: pagerIndex
            });
            if (callback) callback(true);
        }
        if (!this.treeContext.isLoadFollowScrollbar) {
            this.focusNode();
        }
    }

    processItems(items, pagerIdx, pagerByServer) {
        if (items) {
            if (this.treeContext.sortChild) {
                this.allChildren = items.sort(this.treeContext.sortChild);
            } else {
                this.allChildren = items;
            }

            let start = 0;
            let end = this.props.item.pagerSize;
            if (!pagerByServer) {
                if (this.isLoadMoreMode(this.props.item)) {
                    end = ((pagerIdx || 0) + 1) * this.props.item.pagerSize;
                } else {
                    start = (!this.state && !pagerIdx) ? 0 : (pagerIdx * this.props.item.pagerSize);
                    if (start >= this.allChildren.length) {
                        start = 0;
                    }
                    end = start + this.props.item.pagerSize;
                }
            }

            items = this.allChildren
                .slice(start, end)
                .map((item, index) => {
                    return this.treeContext.transToTreeNodeObject(item);
                });
        } else {
            this.allChildren = [];
            items = [];
        }
        return items;
    }

    loading(show) {
        this.setState({ loading: show });
    }

    isShowPager() {
        if (this.isLoadMoreMode(this.props.item)) {
            return false;
        }
        let item = this.props.item;
        return this.state.expanded && (
            (this.exactPaging && this.state.itemsCount > item.pagerSize)
            || (!this.exactPaging && (item.pagerIndex > 0 || this.props.item.hasNextPage))
        );
    }

    isShowLoadMoreBtn(item) {
        if (!this.isLoadMoreMode(item) || !this.state.expanded || this.state.loading) {
            return false;
        }
        return !!item.hasNextPage;
    }

    reRender() {
        this.setState({ items: this.state.items.slice(), selected: !!this.props.item.checked, expanded: this.props.item.expanded});
    }

    renderNodeContent() {
        let ContentPart = this.treeContext.nodeContentComponent || DefaultNodeContent;
        this.props.item.itemsCount = this.state.itemsCount;
        return <ContentPart
            selected={this.state.selected}
            treeContext={this.props.treeContext}
            item={this.props.item}
            itemComponent={this}
            parentItem={this.props.parentItem}
            parentItemComponent={this.props.parentItemComponent}
            recalculatePosition={this.props.treeContext.recalculatePosition}
        // onClick={this.onNodeClick} 
        />;
    }

    renderNode() {
        if (this.props.item.enableContextMenu) {
            return <TreeContextMenuTrigger
                treeContext={this.props.treeContext}
                nodeSelected={this.state.selected}
                itemComponent={this}>
                {this.renderNodeContent()}
            </TreeContextMenuTrigger>;
        } else {
            return this.renderNodeContent();
        }
    }

    renderPager() {
        let item = this.props.item;
        let idx = this.state.pagerIndex || 0;
        if (this.exactPaging) {
            return <$g.Pager
                itemsCount={this.state.itemsCount}
                pagerIndex={idx}
                pagerSize={item.pagerSize}
                showPagerSize={false}
                pagerSizeOptions={[5, 10, 15, 20]}
                onChange={this.loadNodes} />;
        } else {
            return <$g.SimplePager
                pagerIndex={idx}
                pagerSize={item.pagerSize}
                shownCount={this.state.items.length}
                hasNext={item.hasNextPage}
                onChange={this.loadNodes} />;
        }
    }

    renderLoadMoreBtn(){
        return <a onClick={this.onLoadMoreClick}>{RMResx.RM_JS_Common_LoadMore}</a>;
    }

    render() {
        let item = this.props.item;
        let indent = (this.props.treeLevel - 1) * 14;
        let loadMorePaddingLeft = (this.props.treeLevel - 1) * 14 + 44;
        let loading = this.state.loading;
        let showShadow = this.treeContext.shadowInitialNodelevel && this.props.item.nodeType == this.treeContext.shadowInitialNodelevel && item.expanded && this.state.items.length > 0;
        let showrRightArrow = this.treeContext.showrRightArrow && this.state.selected;
        let treeItemClass = showShadow ? "ra-tree-item ra-tree-item-shadow" : "ra-tree-item";
        let treeNodeClass = this.getNodeClassName();
        let dispalyClass = loading ? "none" : "inline-block";
        let spaceClass = item.disableSelect !== true ? "margin-left-s margin-right-s" : "";
        let spaceClassNoSelector = (!this.treeContext.singleSelection || !this.treeContext.multiSelection) && this.treeContext.spaceNoSelection && item.disableSelect ? "margin-left-s" : "";
        let nodeTabIndex = this.treeContext.readonly ? "-1" : "0";
        return (
            <li className={treeItemClass}>
                <div
                    id={this.props.item.nodeType + this.props.item.nodeKey}
                    ref={r => this.nodeElement = r} tabIndex={nodeTabIndex}
                    className={treeNodeClass} style={{ margin: `1px ${indent}px`}}
                    onClick={this.toggleExpand} onKeyDown={this.onNodeKeyDown}
                    role="treeitem" aria-selected="none"
                >
                    {loading && <img className="ra-tree-node-loading" src="/Images/Base/loading_18x18.gif" />}

                    <div className={`ra-tree-node-selector ${dispalyClass}`} onClick={this.onCheckboxOrRadioClick}>
                        {this.treeContext.singleSelection && item.disableSelect !== true &&
                            <R.Radio
                                key={new Date().getMilliseconds()}
                                name={"TreeRadio-" + Math.random() }
                                disabled={item.disabled || this.treeContext.readonly}
                                checked={this.state.selected}
                                onChange={this.onSingleCheckedChange}
                            />
                        }
                        {this.treeContext.multiSelection && item.disableSelect !== true &&
                            <R.Checkbox
                                checked={!item.disableSelect && this.state.selected}
                                disabled={!!item.disableSelect || this.treeContext.readonly}
                                onChange={this.onCheckedChange}
                            />
                        }
                    </div>
    
                    <div className={`ra-tree-node-content ${spaceClassNoSelector} ${spaceClass}`}>
                        {this.renderNode()}
                    </div>
                    
                    {showrRightArrow && <span className="fia-long-arrow ra-tree-node-right-arrow"></span>}
                </div>

                {((item.hasChildren || this.state.itemsCount > 0) && this.state.items && this.state.items.length > 0
                    || item.enableIncludeNew) &&
                    <React.Fragment>
                        <TreeList
                            show={this.state.expanded}
                            treeContext={this.props.treeContext}
                            items={this.state.items}
                            parentItem={item}
                            parentItemComponent={this} />
                        {this.isShowPager() &&
                            <div className="ra-tree-pager" style={{ paddingLeft: `${indent}px` }}>
                                {this.renderPager()}
                            </div>
                        }
                        {this.isShowLoadMoreBtn(item) &&
                            <div className="ra-tree-pager" style={{ paddingLeft: `${loadMorePaddingLeft}px` }}>
                                {this.renderLoadMoreBtn()}
                            </div>
                        }
                    </React.Fragment>
                }
            </li>
        );
    }
}