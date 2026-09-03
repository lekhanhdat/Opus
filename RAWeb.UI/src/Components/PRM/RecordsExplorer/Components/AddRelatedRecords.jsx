import { bindEvents } from "../../../../Utilities/CommonUtil";
import RelatedTable from '../Components/Table/RelatedTable';
import PhyObjectDetail from '../../Common/PhyObjectDetail';
import SPObjectDetail from '../../Common/SPObjectDetail';
import {NodeType} from "../../../../Constants/DAEnums";
import "../../../../Less/PRM/RelatedRecords.less";

export default class AddRelatedRecords extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            delBtnStatus: true,
            phyObjDetailParam: {},
            showViewDetailPanel: {show: false},
            showTip: false,
            tipType: "success",
            tipMsg: "",
            addedItems: this.props.newAddedItems,
            currentRecordId: this.props.recordId,
            pager: {
                pageIndex: 0,
                pageSize: 10,
                shownCount: 10,
                hasNext: false,
            },
        };
        this.cachePageBrowserState = [];
        this.currentPage = {
            pageIndex: 0,
            pageSize: 10,
        };
        this.operateLimitCount = 15;
        this.tableColumns = this.initColumns();
        this.cacheItems = [];
        this.searchKey = "";
        this.addRelatedId = "addRelated";
        this.addRelatedTableId = "addRelatedTable";
        bindEvents(this, "onCheckChanged", "onClickCell", "setBtnStatus", "onSearch", "onShowAddRelatedPanel", "pagerChange");
    }
    
    componentReceive(type, callback) {
        switch (type) {
            case "onAdd":
                callback(this.getSelectedItems());
                break;
            case "initAddRelatedPanelData":
                this.loadData();
                break;
        }
    }

    initColumns() {
        let colInfo = [{
            header: RMResx.RM_PRM_PRE_MRR_Column_Form,
            width: 180,
        },{
            header: RMResx.RM_PRM_PRE_MRR_Column_NameOrTitle,
            width: 180,
        }, {
            header: RMResx.RM_PRM_PRE_Column_ID,
            width: 180,
        },{
            header: RMResx.RM_PRM_PRE_MRR_Column_Type,
            width: 180,
        }, {
            header: RMResx.RM_PRM_PRE_Column_Modifier,
            width: 180,
        },{
            header: RMResx.RM_PRM_PRE_Column_DisposalClass,
            width: 180,
        },{
            header: RMResx.RM_PRM_PRE_Column_RuleAction,
            width: 220,
        },{
            header: RMResx.RM_PRM_PRE_Column_DisposalStatus,
            width: 100,
        }];
        return colInfo;
    }

    loadData(){
        $$.loading(true);
        let pagePager = {};
        let hasNext = false;
        pagePager.PageIndex = this.currentPage.pageIndex;
        pagePager.PageSize = this.currentPage.pageSize;
        if (pagePager.PageIndex != 0) {
            if (this.currentPage.pageIndex < this.state.pager.pageIndex) {
                pagePager.currentBrowserState = this.cachePageBrowserState[this.currentPage.pageIndex - 1];
            } else {
                pagePager.currentBrowserState = this.state.pager.currentBrowserState;
            }
        }
        let requestParam = {
            PageIndex: pagePager.currentBrowserState || "",
            PageSize: pagePager.PageSize,
            Value: this.searchKey,
            CurrentId: this.state.currentRecordId,
            RelatedsCache: this.getRelatedCache()
        };
        let url = `/api/RecordsExplorerApi/SearchRecords`;
        let option = {
            url: url,
            method: "POST",
            data: requestParam
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let res = JSON.parse(result);
            // console.log(res.Datas);
            let currentBrowserState = res.PagingInfo.PageIndex || "";
            hasNext = res.PagingInfo.HasNextPage;
            if (pagePager.PageIndex >= this.state.pager.pageIndex) {
                if (pagePager.PageIndex == 0) {
                    this.cachePageBrowserState = [];
                }
                if (this.cachePageBrowserState.indexOf(currentBrowserState) == -1) {
                    if (currentBrowserState) {
                        this.cachePageBrowserState.push(currentBrowserState);
                    }
                }
            }

            let pager = {
                pageIndex: pagePager.PageIndex,
                pageSize: pagePager.PageSize,
                shownCount: res.Datas.length,
                hasNext: hasNext,
                currentBrowserState: currentBrowserState
            };
            this.setState({
                pager: pager
            }, () => {
                this.setCacheData(res.Datas);
                this.dispatch(this.addRelatedId, "initPageData", res.Datas);
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    pagerChange(pageIndex, pageSize) {
        this.currentPage.pageIndex = pageIndex;
        this.currentPage.pageSize = pageSize;
        this.loadData();
    }

    setCacheData(data){
        if(data && data.length > 0){
            data.map(item => {
                let isExits = this.cacheItems.find( r => r.Id == item.Id);
                if(!isExits){
                    item.isChecked = false;
                    item.isRemoved = false;
                    this.cacheItems.push(item);
                }else{
                    //存在则keep原来的选中状态
                    this.resetCheckStatus(item);
                }
            });
        }
    }

    resetCheckStatus(item){
        let matchItem = this.cacheItems.find( r=> r.Id == item.Id);
        if(matchItem){
            item.isChecked = matchItem.isChecked;
        }
    }

    setCheckStatus(items){
        if(items && items.length > 0){
            this.cacheItems.map(c => {
                let item = items.find(t => t.Id == c.Id);
                if(item !== undefined){
                    c.isChecked = item.isChecked;   
                }
            });
        }
    }

    clearCacheData(){
        this.cacheItems = [];
    }

    getSelectedItems(){
        return this.cacheItems.filter(t => t.isChecked && t.isRemoved == false);
    }

    getRelatedCache(){
        let items = RM.deepcopy(this.state.addedItems);
        return items.map(d => {return d.Id;});
    }
    
    onSearch = (args) => {
        this.searchKey = args;
        if($.trim(this.searchKey)){
            this.clearCacheData();
        }
        this.currentPage.pageIndex = 0;
        this.loadData();
    }

    onCheckChanged(items) {
        let currentPageItems = items.slice();
        this.setCheckStatus(currentPageItems);
        this.setBtnStatus(this.cacheItems.filter(t => t.isChecked));
    }

    onClickCell(data) {
        let isPhyObject = data.NodeType == NodeType.PhyFile || data.NodeType == NodeType.PhyRecord;
        if (isPhyObject) {
            this.setState({
                phyObjDetailParam: {
                    isRequest: false,
                    id: data.Id,
                    nodeType: data.NodeType,
                    BoxId: data.BoxId,
                    FileId: data.FileId,
                },
                showViewDetailPanel: { show: true }
            });
        }
        else {
            this.setState({
                phyObjDetailParam: {
                    isRequest: false,
                    id: data.Id,
                    nodeType: data.NodeType
                },
                showViewDetailPanel: { show: true }
            });
        }
    }

    onCloseViewDetail() {
        this.setState({
            showViewDetailPanel: {show: false}
        });
    }

    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = ()=> {
        this.setState({showTip: false});
    }

    setBtnStatus(items) {
        this.setState({
            delBtnStatus: items.length == 0,
        });
    }

    renderViewDetailPanel() {
        let oNodeType = this.state.phyObjDetailParam.nodeType;
        let isPhyObject = oNodeType == NodeType.PhyFile || oNodeType == NodeType.PhyRecord;
        return <R.Panel
            id="addRelatedViewDetailPanel"
            header={RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}
            size={600}
            actionType='back'
            status={this.state.showViewDetailPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                { isPhyObject && <PhyObjectDetail
                    data={this.state.phyObjDetailParam}
                ></PhyObjectDetail>
                }
                { !isPhyObject && <SPObjectDetail
                    data={this.state.phyObjDetailParam}
                ></SPObjectDetail>
                }
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onCloseViewDetail.bind(this)} />
        </R.Panel>;
    }

    renderActionBar() {
        return <div className='navbar'>
            <div className='navbar-left'>
                <div className='navbar-search'>
                    <R.Searchbox
                        id="raPhyRelatedRecordsSearchBox"
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                        width={922}
                        labelby="ariaSearchDesc"
                    />
                </div>
            </div>
        </div>;
    }

    render(){
        let pager = this.state.pager;
        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                status={{ show: this.state.showTip }}
            />
            <div className="text-desc" id="ariaSearchDesc">{RMResx.RM_PRM_PRE_MRR_Add_SearchDesc}</div>
            {this.renderActionBar()}
            <div>
                <RelatedTable
                    id={this.addRelatedId}
                    tableId={this.addRelatedTableId}
                    onCheckChanged={this.onCheckChanged}
                    cellClick={this.onClickCell}
                    columns={this.tableColumns}
                    showCheckBox={true}
                    frozenCount={3}
                />
                <div className={"table-foot ra-clearafter"}>
                    <div className={"table-foot-left" + (this.state.pagerTotal > this.state.pagerSize ? "" : " none")}>
                        {RMResx.RM_JS_CRM_ShowItemsCount.format(this.state.shownCount, this.state.pagerTotal)}
                    </div>
                    <div className={"table-foot-right"}>
                        {/* <$g.Pager
                            itemsCount={this.state.pagerTotal}
                            pagerIndex={this.state.pagerIndex}
                            pagerSize={this.state.pagerSize}
                            showPagerSize={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.loadData} /> */}
                        <$g.SimplePager
                            pagerIndex={pager.pageIndex}
                            pagerSize={pager.pageSize}
                            shownCount={pager.shownCount}
                            hasNext={pager.hasNext}
                            onChange={this.pagerChange}
                        ></$g.SimplePager>
                    </div>
                </div>
            </div>
            {this.renderViewDetailPanel()}
        </div>;
    }
}