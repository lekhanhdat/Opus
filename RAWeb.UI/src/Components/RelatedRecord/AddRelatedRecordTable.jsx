class RowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        this.pagerBrowserState = [];
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.UNCPath}>
                    <a href={rowData.url}>{rowData.DisplayName}</a> 
                </div>
            </Cell>
            <Cell>{rowData.LastModifyTime}</Cell>
            <Cell>{rowData.LastModifierName}</Cell>
        </Row>;
    }
}

export default class AddRelatedRecordTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            columns: [
                {
                    header: RMResx.RM_JS_RD_Name,
                    width: [200],
                    resizeable: true,
                },
                {
                    header: RMResx.RM_JS_RD_Modified,
                    width: [300],
                    resizeable: true,
                },
                {
                    header: RMResx.RM_JS_RD_ModifiedBy,
                    resizeable: true,
                    width: [250],
                },
            ],
            items: [],
            shownCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            pagerHasNext: false
        };
        this.cacheData = {};
    }

    componentReceive(action, selectedTreeNode) {
        switch (action) {
            case "LOAD_DATA":
                this.selectedTreeNode = selectedTreeNode;
                this.loadItems(true);
                break;
            case "CLEAR_DATA":
                this.clearItems();
                break;
            case "INIT_CHECK_STATE":
                this.initCheckState();
                break;
        }
    }

    loadItems(isReset){
        if(isReset){
            this.pageBrowserState = null;
        }
        let option = {
            url: "/api/RelatedRecordsApi/GetItems",
            data:{
                FolderId: this.selectedTreeNode.FolderId,
                ListId: this.selectedTreeNode.ListId,
                NodeLevel: this.selectedTreeNode.NodeLevel,
                PageSize: this.state.pagerSize,
                PageIndex: isReset ? 1 : this.state.pagerIndex + 1,
                ServerRelativeUrl: this.selectedTreeNode.ServerRelativeUrl,
                WebUrl: this.selectedTreeNode.WebUrl,
                WebId: this.selectedTreeNode.WebId,
                pageInfo: this.pageBrowserState ? this.pageBrowserState : null
            },
            method: "POST",
        }; 
        $$.loading(true);
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let itemInfo = JSON.parse(res);
            let items = itemInfo.infos;
            this.cacheData= {};
            this.pageBrowserState = itemInfo.pageInfo;
            this.originItems = RM.deepcopy(items);
            for(let item of items){
                if(this.cacheData[item.id]){
                    Object.assign(item, this.cacheData[item.id]);
                }else{
                    this.cacheData[item.id] = item;
                }
            }
            this.setState({
                items: items,
                shownCount: itemInfo.infos.length,
                pagerHasNext: itemInfo.ChildrenCount > (this.state.pagerIndex + 1) * this.state.pagerSize
            });
        });
    }

    initCheckState(){
        this.cacheData = [];
        for(let item of this.state.items){
            item.checked = false;
        }
        this.setState({items: RM.deepcopy(this.state.items)});
    }

    clearItems(){
        this.cacheData = [];
        this.setState({ items: [] });
    }

    onSelectItem = () =>{
        for(let item of this.state.items){
            if(this.cacheData[item.id]){
                Object.assign(this.cacheData[item.id], item);
            }else{ 
                this.cacheData[item.id] = item;
            }
        }
        let selectedItem = Object.values(this.cacheData).filter((item)=>{return item.checked;});
        this.props.onSelectChange(selectedItem);
    }

    onPagerChange = (pagerIndex, pagerSize) => {
        this.setState({
            pagerIndex: pagerIndex,
            pagerSize: pagerSize
        },()=>{
            this.loadItems();
        });  
    };

    renderTable(){
        return <div className="ra-main-table">
            <R.Table
                id="raRdTable"
                columns={this.state.columns}
                rowTemplate={RowTemplate}
                items={this.state.items}
                checkable={true}
                onCheck={this.onSelectItem}
            />
        </div>;
    }

    renderFooter(){
        return <div className="ra-main-footer">
            <$g.SimplePager
                pagerIndex={this.state.pagerIndex}
                pagerSize={this.state.pagerSize}
                shownCount={this.state.shownCount}
                hasNext={this.state.pagerHasNext}
                onChange={this.onPagerChange}
            ></$g.SimplePager>
        </div>;
    }

    render() {
        return <div id={this.props.id}>
            {this.renderTable()}
            {this.renderFooter()}
        </div>;
    }
}

