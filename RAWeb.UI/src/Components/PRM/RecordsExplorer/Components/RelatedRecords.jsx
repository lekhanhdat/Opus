import { bindEvents } from "../../../../Utilities/CommonUtil";
import RelatedTable from '../Components/Table/RelatedTable';
import PhyObjectDetail from '../../Common/PhyObjectDetail';
import SPObjectDetail from '../../Common/SPObjectDetail';
import {NodeType} from "../../../../Constants/DAEnums";
import {PhysicalDefaultColumnIDs} from "../../../../Constants/Constants";
import "../../../../Less/PRM/RelatedRecords.less";

export default class RelatedRecords extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            pagerIndex: 0,
            pagerSize: 10,
            pagerTotal: 1,
            shownCount: 0,
            phyObjDetailParam: {},
            showViewDetailPanel: {show: false},
        };

        this.tableColumns = this.initColumns();
        this.cacheItems = [];
        this.searchKey = "";
        this.searchIds = [];
        this.suffix = this.newGuid();
        this.containerId = "explorerRelatedRecords";
        this.relatedRecordsId = "relatedRecords"+ "_" + this.suffix;
        this.relatedRecordsTableId = "relatedRecordsTable"+ "_" + this.suffix;
        bindEvents(this, "onCheckChanged", "onClickCell", "onPageChange", "onSearch", "refresh");
    }

    componentInit() {
        this.initPanelButtons();
        this.loadData();
    }

    initPanelButtons(){
        this.addRelatedPanelBtns = [
            {
                text: ".Back", onClick: () => {
                    this.setState({showAddRelatedPanel: {show: false}});
                }
            }
        ];
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
        let id = this.props.data.id;
        let url = `/api/RecordsExplorerApi/GetRelatedRecords?id=${id}`;
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
                    this.cacheItems.push(item);
                }
            });
        }
    }

    onPageChange(index, size, callback) {
        let items = this.cacheItems;
        if(this.searchIds.length > 0){
            items = items.filter(d => this.searchIds.indexOf(d.Id) > -1);
        }
        let currentPageItems = RM.deepcopy(items.slice(index * size, (index + 1) * size));
        this.dispatch(this.relatedRecordsId, "initPageData", currentPageItems);
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

    onSearch = (args) => {
        this.searchKey = $.trim(args);
        if(this.searchKey){
            let allItems = RM.deepcopy(this.cacheItems);
            allItems.forEach(r => {
                var phyItemName = JSON.parse(r.MetaInfo)[PhysicalDefaultColumnIDs.NameOrTitle];
                if(phyItemName){
                    r.LeafName = phyItemName;
                }
            });
            let matchItems = allItems.filter(t => t.LeafName.indexOf(this.searchKey) > -1 || t.RecordsId.indexOf(this.searchKey) > -1);
            if(matchItems.length > 0){
                this.searchIds = matchItems.map(d => {return d.Id;});
                this.refresh();
            }
        } else {
            this.searchKey = "";
            this.searchIds = [];
            this.refresh();
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

    newGuid(){
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
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

    renderActionBar() {
        return <div className='navbar'>
            <div className='navbar-left'>
                <div className='navbar-search'>
                    <R.Searchbox
                        placeholder={RMResx.RM_JS_TM_SearchTxt}
                        disabled={false}
                        onSearch={this.onSearch}
                    />
                </div>
            </div>
        </div>;
    }

    render(){
        return <div id={this.containerId}> 
            {this.renderActionBar()}
            <div>
                <RelatedTable
                    id={this.relatedRecordsId}
                    tableId={this.relatedRecordsTableId}
                    onCheckChanged={this.onCheckChanged}
                    cellClick={this.onClickCell}
                    columns={this.tableColumns}
                    showCheckBox={false}
                    frozenCount={0}
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
        </div>;
    }
}