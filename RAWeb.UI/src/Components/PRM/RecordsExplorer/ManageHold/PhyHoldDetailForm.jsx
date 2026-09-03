import { bindEvents } from "../../../../Utilities/CommonUtil";

export default class PhyHoldDetailForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        bindEvents(this, "showMessageTip", "hideMessageTip", "onHoldRadioChange", "onHoldTypeChange", "renderExtendForm");
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        bindEvents(this, "pagerChange");
        this.state = {
            pagerIndex: 0,
            pagerSize: 10,
            itemsCount: 0,
            hasNextPage: false,
            batchSelection: false,
            singleSelection: false,
            items: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
        };
        this.tableColumns = [ {
            header: RMResx.RM_JS_RDM_HoldDetail_RecordName,
            width: 200,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_RDM_HoldDetail_UniqueId,
            width: 220,
            resizeable: true,
            visible: true,
        }, {
            header: RMResx.RM_JS_RDM_HoldDetail_ReleaseTime,
            resizeable: true,
            width: 220,
        }];

    }
    componentInit() {
        this.initData(this.props.data);
    }

    initData(holdProfile) {
        this.formData = holdProfile;
        let postData = {
            holdId: holdProfile.Id,
            PagingInfo: { PageIndex: this.state.pagerIndex + 1, PageSize: this.state.pagerSize, HasNextPage: this.state.hasNextPage}
        };
        this.getRelatedRecords(postData);
    }

    getRelatedRecords(postData) {
        $$.loading(true);
        let option = {
            url: "/api/RecordsExplorerApi/GetRecordbyHoldId",
            method: "POST",
            data: postData
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let ret = JSON.parse(result);
            let hasNextPage = false;
            if(ret.PagingInfo.Total > ret.PagingInfo.PageIndex * ret.PagingInfo.PageSize){
                hasNextPage = true;
            }
            this.setState({
                items: ret.Datas,
                hasNextPage: hasNextPage,
                itemsCount: ret.PagingInfo.Total
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    pagerChange(pageIndex, pageSize) {
        this.setState({ pagerIndex: pageIndex, pagerSize: pageSize });
        let postData = {
            holdId: this.formData.Id,
            PagingInfo: { PageIndex: pageIndex + 1, PageSize: pageSize, HasNextPage: this.state.hasNextPage }
        };
        this.getRelatedRecords(postData);
    }

    render() {
        return <div id="phyholdForm">
            <R.Table
                id="phyholdDetailForm"
                disabled={false}
                rootData={this.state.rootData}
                columns={this.tableColumns}
                rowTemplate={PhyHoldDetailRowTemplate}
                items={this.state.items}
            />
            <div style={{ float: "right", paddingRight:"8px" }}>
                <$g.SimplePager
                    pagerIndex={this.state.pagerIndex}
                    pagerSize={this.state.pagerSize}
                    shownCount={this.state.items.length}
                    hasNext={this.state.hasNextPage}
                    onChange={this.pagerChange}
                ></$g.SimplePager>
            </div>
        </div>;
    }
}
class PhyHoldDetailRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }
      
    render(Row, Cell) { 

        let rowData = this.props.rowData;
        return (
            <Row>  
                <Cell> 
                    {rowData.LeafName} 
                </Cell>
                <Cell>
                    {rowData.RecordsId}
                </Cell> 
                <Cell>
                    {rowData.ReleaseTime}
                </Cell>
            </Row>
        );
    }
}