import RDSPTree from "../Common/Tree/Instances/SPTree/RDSPTree";
import AddRelatedRecordTable from "./AddRelatedRecordTable";
import { showToast } from "../../Utilities/CommonUtil";

export default class ManageRelatedRecords extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            currentItemName: "",
            selectedRelatedRecords: [],
            siteCollectionUrl: "",
            navigateUrl: "",
            originSelectedRelatedRecords: [],
        };
    }

    componentInit() {
        let relatedInfos = this.props.relatedInfos;
        this.setTreeInfo(relatedInfos);
        this.setRelatedRecordsInfo(relatedInfos);
    }

    setTreeInfo(relatedInfos){
        this.siteCollectionUrl = relatedInfos.HostUrl;
        this.setState({
            currentItemName: relatedInfos.CurrentItemName,
            siteCollectionUrl: this.siteCollectionUrl ,
        });
    }

    setRelatedRecordsInfo(relatedInfos){
        let relatedRecordsInfo = relatedInfos.RelatedInfos ? JSON.parse(relatedInfos.RelatedInfos) : [];
        for(let item of relatedRecordsInfo){
            item.checked = true;
        }
        this.setState({
            originSelectedRelatedRecords: RM.deepcopy(relatedRecordsInfo),
            selectedRelatedRecords: relatedRecordsInfo,
        });
    }
    
    onModifySiteCollection = () => {
        this.setState({siteCollectionUrl:  this.siteCollectionUrl});
        this.dispatch("raAddRelatedTable", "CLEAR_DATA");
    };

    onChangeSiteCollectionUrl = (value) => {
        this.siteCollectionUrl = value;
    }

    onSelectedTreeNodeChanged = (node) =>{
        this.dispatch("raAddRelatedTable", "LOAD_DATA", node);
    }

    onSelectTableItemChange = (items) =>{
        this.selectTableItem = items;
    }

    onSelectionRelatedRecords = (selectedRelatedRecords) =>{
        this.setState({selectedRelatedRecords: selectedRelatedRecords.newValue}); 
    }

    onSaveRelatedRecords = () => {
        $$.loading(true);
        let originSelectedRelatedRecordsIds = this.state.originSelectedRelatedRecords.map((item)=>{return item.id;});
        let selectedRelatedRecordsIds = this.state.selectedRelatedRecords.map((item)=>{return item.id;});
        let deleteRelatedRecords = this.state.originSelectedRelatedRecords.filter((item)=>{ 
            item.NeedDelete = true;
            return !selectedRelatedRecordsIds.includes(item.id);
        });
        let addRelatedRecords = this.state.selectedRelatedRecords.filter((item)=>{ 
            item.NeedDelete = false; 
            return !originSelectedRelatedRecordsIds.includes(item.id);
        });
        let param = [...deleteRelatedRecords, ...addRelatedRecords];
        let option = {
            url: "/api/RelatedRecordsApi/SubmitRelatedItems",
            method: "POST",
            data: param
        };
        fetchUtility(option).then((res) => {
            this.loadRelatedRecordsInfos();
            $$.loading(false);
        }).catch((e) => {
            showToast.error(RMResx.RM_RD_Submit_Failed);
            $$.loading(false);
        });
    }

    loadRelatedRecordsInfos(){
        $$.loading(true);
        let option = {
            url: "/api/RelatedRecordsApi/GetRelatedRecordsInfos",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.showSuccessMsgBox();
            this.setRelatedRecordsInfo(res);
            this.initRelatedRecordsCheckState();
            this.dispatch("raAddRelatedTable", "INIT_CHECK_STATE");
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showSuccessMsgBox(msg) {
        $$.messagedialog(true, {
            classify: "info",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_RD_Submit_Success,
            buttons: [
                { text: RMResx.RM_JS_Common_OK, onClick: this.onBackLibrary }
            ],
        });
    }

    onBackLibrary = () =>{
        window.location.href = this.props.relatedInfos.NavigateUrl;
    }

    initRelatedRecordsCheckState(){
        this.selectTableItem = [];
        this.dispatch("raAddRelatedTable", "INIT_CHECK_STATE");
    }

    onCancelBtnClick = () => {
        this.initRelatedRecordsCheckState();
        this.setState({selectedRelatedRecords: RM.deepcopy(this.state.originSelectedRelatedRecords)});
    }

    onInsertRelatedRecords = () => {
        this.dispatch("raAddRelatedTable", "INIT_CHECK_STATE");
        let selectTableItem = RM.deepcopy(this.selectTableItem);
        let selectedRelatedRecordsIds = this.state.selectedRelatedRecords.map(item=>{ 
            return item.id; 
        });
        selectTableItem = selectTableItem.filter(item =>{ 
            item.checked = true;
            return !(selectedRelatedRecordsIds.includes(item.id));
        });
        let selectedRelatedRecords = [...this.state.selectedRelatedRecords, ...selectTableItem];
        this.setState({selectedRelatedRecords: selectedRelatedRecords});
    }

    renderAddRaletedHeader() {
        return (
            <div className="ra-add-related-header">
                <div className="add-related-title">{RMResx.RM_RD_Selected}</div>
                <div>
                    <span>{RMResx.RM_RD_CurrentItemName}</span>
                    <span className="add-related-name">{this.state.currentItemName}</span>
                </div>
                <div className="add-related-site-collection">
                    <div className="margin-right-s">{RMResx.RM_RD_Location}</div>
                    <div className="margin-right-s">
                        <R.Input 
                            id="raRdSearchSiteCollection" 
                            type="text" 
                            width={500} 
                            value={this.state.siteCollectionUrl} 
                            onChange={this.onChangeSiteCollectionUrl}
                        />
                    </div>
                    <div className="fia-long-arrow search-site-collection-arrow" onClick={this.onModifySiteCollection}></div>
                </div>
            </div>
        );
    }

    renderAddRaletedContent() {
        return (
            <div className="ra-add-related-content">
                <R.Splitter minAsize="25%" minBsize="60%" defaultAsize="40%">
                    <div className="add-related-tree">
                        {
                            this.state.siteCollectionUrl && <RDSPTree 
                                siteCollectionUrl={this.state.siteCollectionUrl}
                                onSelectedNodeChanged={this.onSelectedTreeNodeChanged}
                            />
                        }
                    </div>
                    <div className="add-related-table">
                        <AddRelatedRecordTable 
                            id="raAddRelatedTable"
                            onSelectChange={this.onSelectTableItemChange}
                        />
                    </div>
                </R.Splitter>
            </div>
        );
    }

    renderSelectedRelatedRecords(){
        return <div className="rd-selected-related-records">
            <div className="selected-related-records-title">{RMResx.RM_RD_SelectRecord}</div>
            <div className="selected-related-records-content">
                <R.RichCombobox
                    width={"100%"}
                    searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                    searchable={false}
                    items={this.state.selectedRelatedRecords}
                    textField="name"
                    valueField="id"
                    checkedField="checked"
                    tooltipField="name"
                    invalidField="invalid"
                    excludeChecked={false}
                    onChange={this.onSelectionRelatedRecords}
                />
            </div>
        </div>;
    }

    renderActionBtns(){
        return <div className="ra-add-related-action">
            <R.Button text={RMResx.RM_RD_Btn_Insert} onClick={this.onInsertRelatedRecords}/>
            <R.Button text={RMResx.RM_RD_Btn_Cancel} onClick={this.onCancelBtnClick} className="margin-left-s"/>
            <R.Button text={RMResx.RM_RD_Btn_Submit} onClick={this.onSaveRelatedRecords} className="margin-left-s"/>
        </div>;
    }

    render() {
        return (
            <div id="raAddRelatedRecords">
                {this.renderAddRaletedHeader()}
                {this.renderAddRaletedContent()}
                {this.renderSelectedRelatedRecords()}
                {this.renderActionBtns()}
            </div>
        );
    }
}
