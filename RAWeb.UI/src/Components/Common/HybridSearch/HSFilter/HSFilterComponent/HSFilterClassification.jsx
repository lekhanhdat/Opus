import TermTree from "../../../../../Components/Common/Tree/Instances/TermTree/MulChoiceFilterTermTree";
import { ToSearchComponentDispatchType } from './../../Constants';
let idCount = 0;
export default class HSFilterClassification extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            echoTreeData: null,
            termSearchKey: "",
            isShownUnClassify: false,
            selectedTermsText: RMResx.RM_JS_Common_None,
            showTermTree: false,
            termTreeValid: true,
        };
        this.realFilteTermInfo = {};
        this.echoTreeData = null;
        this.termTreeId = "termTree" + idCount++;
        this.onDocumentMouseDown = this.onDocumentMouseDown.bind(this);
        window.addEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentDestroy() {
        window.removeEventListener("mousedown", this.onDocumentMouseDown, true);
    }

    componentReceive(type, termTreeData, searchColumnValue) {
        let {TermIds, WithOutTerms, TermNames} = searchColumnValue || {};
        switch (type) {
            case ToSearchComponentDispatchType.InitData:
                if (termTreeData) {
                    this.echoTreeData = RM.deepcopy(termTreeData);
                    this.getTreeData();
                } else {
                    this.echoTreeData = null;
                }
                this.realFilteTermInfo.TermIds = TermIds || [];
                this.realFilteTermInfo.TermNames = TermNames || [];
                this.realFilteTermInfo.WithOutTerms = WithOutTerms || false;
                this.setSelectedTermsText();
                break;
            case ToSearchComponentDispatchType.Valid:
                this.showValidMsg();
                break;
        }
    }

    getTreeData() {
        this.setState({ showTermTree: true, echoTreeData: RM.deepcopy(this.echoTreeData) }, () => {
            this.treeData = this.refTermTree.getTreeData().items;
            this.setState({ showTermTree: false });
        });
    }

    onUnClassifyChange = (checked) => {
        this.setState({ isShownUnClassify: checked });
    }

    onSearchTermTree = (args) => {
        this.setState({ termSearchKey: args });
    }

    onApplyClick = () => {
        let selectedTermIds = [];
        this.selectedTermNames = [];
        this.treeData = this.refTermTree.getTreeData().items;

        for (let key in this.treeData) {
            let item = this.treeData[key];
            if (item.Type != "TermGroup" && item.Type != "TermSet" && item.IsChecked) {
                selectedTermIds.push(item.UniqueId);
                this.selectedTermNames.push(item.Name);
            }
        }

        if (selectedTermIds.length > 0) {
            this.echoTreeData = this.getTreeDataByFormat(RM.deepcopy(this.treeData), selectedTermIds);
        } else {
            this.echoTreeData = null;
            this.setState({ termSearchKey: '', echoTreeData: null });
        }

        this.realFilteTermInfo.TermIds = selectedTermIds;
        this.realFilteTermInfo.WithOutTerms = this.state.isShownUnClassify || null;
        this.realFilteTermInfo.TermNames = this.selectedTermNames;

        let searchTermInfo = this.realFilteTermInfo;
        if (selectedTermIds.length == 0 && !this.state.isShownUnClassify) {
            searchTermInfo = null;
        }

        this.setSelectedTermsText();
        this.props.onChange(searchTermInfo, this.treeData);
    }

    setSelectedTermsText() {
        let selectedTermsText = RMResx.RM_JS_Common_None;
        if (this.realFilteTermInfo.TermIds) {
            let withOutTerms = this.realFilteTermInfo.WithOutTerms;
            let selectedTermIdsCount = this.realFilteTermInfo.TermIds.length + (withOutTerms ? 1 : 0);
            switch (selectedTermIdsCount) {
                case 0:
                    selectedTermsText = RMResx.RM_JS_Common_None;
                    break;
                case 1:
                    selectedTermsText = withOutTerms ? 
                        RMResx.RM_HS_Filter_Option_Unclassified  : 
                        this.realFilteTermInfo.TermNames?.[0];
                    break;
                default:
                    selectedTermsText = RMResx.RM_Common_Combobox_SelectedXItems.format(selectedTermIdsCount);
            }
            this.setState({
                selectedTermsText: selectedTermsText,
                termTreeValid: selectedTermIdsCount > 0
            });
        }
    }

    showValidMsg() {
        this.setState({ termTreeValid: false });
    }

    setApplyBtnDisabled() {
        let selectedAllTypesCount = [...this.selectedOtherTypeValues, ...this.selectedCommonTypeValues].length;
        this.setState({ applyBtnDisabled: selectedAllTypesCount == 0 });
    }

    getTreeDataByFormat(treeData, selectedTermIds) { //回显数据
        let newTreeData = [];
        if (selectedTermIds && selectedTermIds.length > 0) {
            let treeDataKeys = [];
            for (let key in treeData) {
                treeDataKeys.push(treeData[key]);
            }
            //将扁平形式转化为tree结构,缓存tree结构
            newTreeData = this.formatTreeTreeData(treeDataKeys);
        }
        return newTreeData;
    }

    onDocumentMouseDown(e) {
        this.mouseDownTarget = e.target;
    }

    isTreeRefreshClick(target) {
        let $target = $(target);
        return $target.closest(".ra-tree-menu-expand").length > 0;
    }

    onWillHideTermsFilterPopup = () => {
        let isTreeRefreshClick = this.isTreeRefreshClick(this.mouseDownTarget);
        this.mouseDownTarget = null;
        if (isTreeRefreshClick) {
            return false;
        }
    }

    formatTreeTreeData(treeDataKeys) {  //将扁平形式转化为tree结构
        let dataKeys = RM.deepcopy(treeDataKeys);
        return dataKeys.filter(parent => {
            let findChildren = dataKeys.filter(child => {
                return parent.UniqueId === child.ParentId;
            });
            findChildren.length > 0 ? parent.subTerms = findChildren : parent.subTerms = [];
            return parent.ParentId == 'Root';
        });
    }

    onShowTermsFilterPopup = () => {
        this.setState({
            isShownUnClassify: this.realFilteTermInfo.WithOutTerms || false,
            showTermTree: true,
            echoTreeData: RM.deepcopy(this.echoTreeData)
        });
    }

    onCloseTermsFilterPopup = () =>{
        this.setState({
            showTermTree: false
        });
    }

    onClearClick = () => {
        this.realFilteTermInfo = {};
        this.echoTreeData = null;
        this.setState({
            echoTreeData: null,
            termSearchKey: "",
            isShownUnClassify: false,
            selectedTermsText: RMResx.RM_JS_Common_None,
            showTermTree: false,
            termTreeValid: true
        },()=>{
            this.props.onChange(null, null);
        });
    }

    render() {
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={RMResx.RM_HS_Contains}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m">
                <R.ComboboxShell
                    content={this.state.selectedTermsText}
                    height={40}
                    popupHeight={[, 300]}
                    popupWidth={[,'100%']}
                    width={"100%"}
                    block={false}
                    triggerType="all"
                    compact={false}
                    id={this.termTreeId}
                    status={{ show: this.state.showTermTree }}
                    clearable={true}
                    onClear={this.onClearClick}
                    willHide={this.onWillHideTermsFilterPopup}
                    onHide={this.onCloseTermsFilterPopup}
                    onShow={this.onShowTermsFilterPopup}
                >
                    <div className="padding-m">
                        <div>
                            <R.Checkbox
                                name="withoutTerms"
                                text={RMResx.RM_JS_BCM_Explorer_Filter_WithoutTermsLabel}
                                value='0'
                                checked={this.state.isShownUnClassify}
                                onChange={this.onUnClassifyChange}
                            />
                        </div>
                        <div className="margin-top-s">
                            <R.Searchbox
                                ref={r => this.searchboxRef = r}
                                width={320}
                                placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                                onSearch={this.onSearchTermTree}
                            />
                        </div>
                        <div className="margin-top-s">
                            {this.state.showTermTree &&
                                <TermTree
                                    ref={r => this.refTermTree = r}
                                    searchKey={this.state.termSearchKey}
                                    data={this.state.echoTreeData}
                                />
                            }
                        </div>
                    </div>
                    <>
                        <R.Button
                            slot="buttons"
                            name="cancel"
                            text={RMResx.RM_JS_Common_Cancel}
                            value="close"
                            onClick={this.onCloseTermsFilterPopup}
                        />
                        <R.Button
                            slot="buttons"
                            name="save"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            value="close"
                            onClick={this.onApplyClick}
                        />
                    </>
                </R.ComboboxShell>
                <R.ValidationFaker valid={this.state.termTreeValid} of={`#${this.termTreeId}`} message={RMResx.RM_HS_InValidClassificationTip} />
            </div>
        </div>;
    }
}