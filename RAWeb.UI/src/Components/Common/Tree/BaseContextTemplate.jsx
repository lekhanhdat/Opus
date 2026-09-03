﻿import { NodeLevel } from '../../../Constants/DAEnums';

export default class BaseContextTemplate {
    
    commonNodeTypes = {
        selectAll: NodeLevel.RMSelectAll,
        includeNew: NodeLevel.RMIncludeNew
    };
    //Is Multiple selection
    multiSelection = false;
    //when multiSelection=true
    allowSelectedWithoutChildren = false;
    //react component for render node content
    nodeContentComponent = null;

    transToTreeNodeObject = (oitem) => {
        return {
            origin: oitem, 
            nodeKey: oitem.Id,
            nodeType: "",
            nodeClass: "",
            iconClass: "",
            text: "",
            enableIncludeNew: false,
            includeNew: false,
            selectAll: false,   //只有支持includeNew时才有意义
            checked: false,
            loaded: false,
            expanded: false,
            //disableSelect=1时，表示在支持多选时，不可选的Node也会显示Disabled状态的Checkbox
            //disableSelect=true时，则不会显示Checkbox
            //disableSelect 跟selectAll的逻辑可能有冲突，慎用，
            //最好只在allowSelectedWithoutChildren=true时，设置disableSelect=1
            disableSelect: false,   
            hasChildren: false,
            isLeafNode: false,
            enableContextMenu: false,
            items: [],
            itemsCount: null,
            pagerIndex: 0,
            pagerSize: 10,
            exactPaging: false, //是否为精确分页，类似从CosmosDB查询只能做非精确分页(不能确定总数)，exactPaging应置为false
            pagerByServer: true,
            hasNextPage: false,
            pagerAnchor: null   //exactPaging=false时，用来存获取数据的PagePostion (或者叫ResponseContinuation)
        };
    }
    updateOriginObject = (item) => {
        //let oitem = item.origin;
        //oitem.Loaded = item.loaded;
        //oitem.Expanded = item.expanded;
        //oitem.ChildrenCount = item.itemsCount;
        //oitem.PagerIndex = item.pagerIndex;
        //oitem.PagerSize = item.pagerSize;
    }
    //sortChild = (a, b) => {
    //    if (a.Name == b.Name) {
    //        return 0;
    //    } else if (a.Name > b.Name) {
    //        return -1;
    //    } else {
    //        return 1;
    //    }
    //}
    getAllChildren = (oitem) => { }
    onExpandClick = (parentItem, isExpanded) => {

    }
    //funcSuccess(children node items)
    //funcFail(error message)
    onLoadNodes = (parentItem, funcSuccess, funcFail) => {
        
    }
    //item: selected items
    onNodeSelectedChange = (items) => {

    }
    //item: selected item
    onNodeSelected = (item) => {

    }
    //item: selected item, funcAllow: (allow) => { confirm if ignore changed }
    //confirmOnNodeSelected = (item, funcAllow) => {

    //}
    confirmOnNodeSelected = null;



    // ------------------------------------------------------
    //internal proerties:
    selectedNode = null;
    selectedNodes = {};//{"nodeKey": NodeComponent}
    expandBottomLocationAndBoxNodes = [];
    selectedItem = null;
    selectedItems = [];

    //internal functions, no need implement
    getSelection() {
        if (this.multiSelection) {
            return this.selectedItems;
        } else {
            return this.selectedItem;
        }
    }
}