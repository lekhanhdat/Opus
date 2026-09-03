import { bindEvents } from "../../../../Utilities/CommonUtil";
import RelatedTable from '../Components/Table/RelatedTable';
import PhyObjectDetail from '../../Common/PhyObjectDetail';
import SPObjectDetail from '../../Common/SPObjectDetail';
import AddRelatedRecords from "../Components/AddRelatedRecords";
import {NodeType} from "../../../../Constants/DAEnums";
import {PhysicalDefaultColumnIDs} from "../../../../Constants/Constants";
import "../../../../Less/PRM/RelatedRecords.less";

export default class PhyRelatedRecords extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            pagerIndex: 0,
            pagerSize: 10,
            pagerTotal: 1,
            shownCount: 0,
            showDelBtn: false,
            phyObjDetailParam: {},
            showViewDetailPanel: {show: false},
            showAddRelatedPanel: {show: false},
            showTip: false,
            tipType: "success",
            tipMsg: "",
            phyNodeItem: this.props.data[0],
        };

        this.operateLimitCount = 15;
        this.tableColumns = this.initColumns();
        this.cacheItems = [];
        this.oldRelatedIds = [];
        this.searchKey = "";
        this.searchIds = [];
        this.pageAddRelatedId = "explorerAddRelatedRecords";
        this.manageRelatedId = "manageRelated";
        this.manageRelatedTableId = "manageRelatedTable";
        bindEvents(this, "onCheckChanged", "onClickCell", "setBtnStatus", "onPageChange", 
            "onDeleteRecords", "refresh", "onShowAddRelatedPanel",
            "onAddRelated");
    }

    componentInit() {
        this.loadData();
    }
    
    componentReceive(type, callback) {
        switch (type) {
            case "onSave":
                var data = {};
                data.Id = this.state.phyNodeItem.Id;
                data.ReletedIds = this.getRelatedIds();
                data.DeleteReletedIds = this.getDeleteRelatedIds();
                
                callback(data, this.errorCallBack);
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
        let url = `/api/RecordsExplorerApi/GetRelatedRecords?id=${this.state.phyNodeItem.Id}`;
        let option = {
            url: url,
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            let res = JSON.parse(result);
            this.setCacheData(res.Datas);
            this.refresh();
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }
    
    refresh(){
        this.onPageChange(0, this.state.pagerSize);
    }

    setCacheData(data){
        if(data && data.length > 0){
            data.map(item => {
                let isExits = this.cacheItems.find( r => r.Id == item.Id);
                if(!isExits){
                    item.isChecked = false;
                    item.isRemoved = false;
                    item.isNewAdd = false;
                    this.cacheItems.push(item);
                    this.oldRelatedIds.push(item.Id);
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

    onPageChange(index, size, callback) {
        let items = this.cacheItems.filter(d => d.isRemoved == false);
        if(this.searchIds.length > 0){
            items = items.filter(d => this.searchIds.indexOf(d.Id) > -1);
        }
        let currentPageItems = RM.deepcopy(items.slice(index * size, (index + 1) * size));
        this.dispatch(this.manageRelatedId, "initPageData", currentPageItems);
        this.setState({
            pagerIndex: index,
            pagerSize: size,
            pagerTotal: items.length,
            shownCount: currentPageItems.length,
        });
        if (callback) {
            callback(true);
        }
    }

    onCheckChanged(items) {
        let onePageData = items.slice();
        this.setCheckStatus(onePageData);
        this.setBtnStatus(this.cacheItems.filter(t => t.isChecked));
    }

    onClickCell(data) {
        this.setState({
            phyObjDetailParam: {
                isRequest: false,
                id: data.Id,
                nodeType: data.NodeType
            },
            showViewDetailPanel: {show: true}
        });
    }

    onCloseViewDetail() {
        this.setState({
            showViewDetailPanel: {show: false}
        });
    }

    onDeleteRecords(){
        let items = this.getSelectedItems();
        items.map(t => {
            t.isRemoved = true;
            t.isChecked = false;
        });
        this.setState({showDelBtn: false});
        this.refresh();
    }

    onShowAddRelatedPanel() {
        this.setState({ showAddRelatedPanel: { show: true } }, () => {
            this.dispatch(this.pageAddRelatedId, "initAddRelatedPanelData");
        });
    }

    onAddRelated(){
        let callback = (data) => {
            this.appendToCacheData(data);
        };
        this.dispatch(this.pageAddRelatedId, "onAdd", callback);
        this.setState({ showAddRelatedPanel: { show: false } });
    }
    
    setBtnStatus(items) {
        this.setState({
            showDelBtn: items.length != 0,
        });
    }

    appendToCacheData(items){
        let newItems = [];
        items.map(d=>{
            let isExits = this.cacheItems.find(t => t.Id == d.Id && t.isRemoved == false);
            if(!isExits){
                d.isChecked = false;
                d.isNewAdd = true;
                newItems.push(d);
            }
        });
        
        this.cacheItems = newItems.concat(this.cacheItems);
        this.refresh();
    }

    getNewAddedItems(){
        return this.cacheItems.filter(d => d.isNewAdd == true && d.isRemoved == false);
    }

    getSelectedItems(){
        return this.cacheItems.filter(t => t.isChecked && t.isRemoved == false);
    }
    
    getRelatedIds(){
        let relatedItems = this.cacheItems.filter(d => d.isRemoved == false);
        //return relatedItems;
        return relatedItems.map(t=> {return t.Id;});
    }

    getDeleteRelatedIds(){
        let needDelItems = this.cacheItems.filter(d => d.isRemoved == true && (this.oldRelatedIds.indexOf(d.Id) > -1));
        //return needDelItems;
        return needDelItems.map(t=> {return t.Id;});
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

    errorCallBack = (msg) => {
        this.showMessageTip("error", msg);
    }

    renderViewDetailPanel() {
        let oNodeType = this.state.phyObjDetailParam.nodeType;
        let isPhyObject = oNodeType == NodeType.PhyFile || oNodeType == NodeType.PhyRecord;
        return <R.Panel
            id="related_viewDetailPanel"
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

    renderAddRelatedPanel() {
        return <R.Panel
            id="addRelatedPanel"
            header={RMResx.RM_PRM_PRE_MRR_Add_Title}
            size={1000}
            actionType='back'
            status={this.state.showAddRelatedPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <AddRelatedRecords
                    id={this.pageAddRelatedId}
                    recordId={this.state.phyNodeItem.Id}
                    newAddedItems={this.getNewAddedItems()}
                ></AddRelatedRecords>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_BCM_Explorer_Button_Back} onClick={() => {
                    this.setState({ showAddRelatedPanel: { show: false } });
                }} />
                <R.Button slot="buttons" id="raPhyAddRelatedRecordsBtn" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add} onClick={this.onAddRelated} />
            </>
        </R.Panel>;

    }

    renderActionBar() {
        return <div className='navbar'>
            <div className='navbar-right'>
                {this.renderButton()}
            </div>
        </div>;
    }

    renderButton() {
        return <div className='navbar-actions'>
            <R.Button
                id="raPhyRelatedAddBtn"
                primary={true}
                classify="theme"
                text={RMResx.RM_JS_BCM_Explorer_ManageRelatedRecordsAddTitle}
                onClick={this.onShowAddRelatedPanel}/>
            {this.state.showDelBtn && <R.Button 
                id="raPhyRelatedDeleteBtn"
                icon="fia-delete" 
                text={RMResx.RM_JS_BCM_Explorer_MRR_Button_Delete}
                onClick={this.onDeleteRecords} />
            }
        </div>;
    }

    render(){
        return <div id={this.props.id}>
            <div className={this.state.showTip && "margin-bottom-m"}>
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                    onClose={this.hideMessageTip}
                />
            </div>
            {this.renderActionBar()}
            <div>
                <RelatedTable
                    id={this.manageRelatedId}
                    tableId={this.manageRelatedTableId}
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
                        <$g.Pager
                            itemsCount={this.state.pagerTotal}
                            pagerIndex={this.state.pagerIndex}
                            pagerSize={this.state.pagerSize}
                            showPagerSize={true}
                            pagerSizeOptions={[5, 10, 15]}
                            onChange={this.onPageChange} />
                    </div>
                </div>
            </div>
            {this.renderViewDetailPanel()}
            {this.renderAddRelatedPanel()}
        </div>;
    }
}