import "../../../Less/CP/stubSettings.less";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import { MessageType } from "../CPConstants";
import StubPanel from "./StubPanel";
import StubTable from "./StubTable";
import { showToast } from "../../../Utilities/CommonUtil";

export default class Management extends R.Component {
    constructor(props) {
        super(props);
        this.defaultShowActions = {
            showCreateBtn: true,
            showDeleteBtn: false,
        };
        this.state = {
            stubChecked: [],
            stubCount: 0,
            stubPagerIndex: 0,
            stubPagerSize: 10,
            stubPanelTitle: '',
            showStubSettingsPanel: { show: false },
            showActions: this.defaultShowActions,
            allColumns: this.getColumns(),
            items: [],
            cellStubId: null,
            recordsLabelValue: RMResx.RM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel,
            isConfiguredGeneral: false,
        };
        this.filterData = this.getDefaultPager();
    }

    componentInit() {
        this.initStubSettingData(true);
        this.loadDeclaredRecords();
    }

    initStubSettingData = (isResetPagerIndex) => {
        $$.loading(true);
        if (isResetPagerIndex) {
            this.filterData.PageIndex = 1;
            this.setState({ stubPagerIndex: 0 });
        }
        let urlData = "/api/StubSetting/GetAllStubSettings";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData,
        };
        fetchUtility(option).then((res) => {
            let data = res;
            this.setState({
                items: data.StubSettingUIDtosList,
                stubCount: data.TotalNumber, 
            });

            this.dispatch("raStubTable", { columns: this.state.allColumns, items: data.StubSettingUIDtosList, isReset: isResetPagerIndex, });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    loadDeclaredRecords = () => {
        $$.loading(true);
        const options = {
            url: "/api/RuleApi/GetRecordLabel",
            method: "GET",
        };
        fetchUtility(options)
            .then((res) => {
                this.setState({
                    recordsLabelValue: res ?? RMResx.RM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel,
                    isConfiguredGeneral: !!res
                });
            })
            .finally(() => $$.loading(false));
    }

    getColumns() {
        return [
            {
                header: RMResx.RM_AR_CP_Stub_ColName_Name,
                width: 350,
                resizeable: true,
                valuePath: "StubName",
            },
            {
                header: RMResx.RM_AR_CP_Common_ColName_ModifiedTime,
                width: 350,
                resizeable: true,
                valuePath: "ModifiedTime",
            },
        ];
    }

    getDefaultPager() {
        let param = {
            PageIndex: 1,
            PageSize: 10,
            TotalNumber: 0,
            SearchValue: "",
        };
        return param;
    }

    getShowActions() {
        let { showCreateBtn, showDeleteBtn } = this.state.showActions;
        let buttonsInfo = [
            { isStatic: true, name: RMResx.RM_JS_Common_Create, onClick: this.onCreateStub, isShow: showCreateBtn },
            { name: RMResx.RM_JS_Common_Delete, icon: "fia-delete", onClick: this.onDeleteStub, isShow: showDeleteBtn },
        ];
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    }

    onRefresh = () => {
        this.setState({ showActions: this.defaultShowActions });
        this.initStubSettingData(true);
    }

    onSelectChange = (items) => {
        let deleteBtn = false;
        if (items.length > 0) {
            deleteBtn = true;
        }
        this.setState({
            showActions: {
                showCreateBtn: true,
                showDeleteBtn: deleteBtn,
            },
            stubChecked: items
        }, () => {
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    onEditStub = (rowData) => {
        this.setState({
            stubPanelTitle: RMResx.RM_AR_CP_Stub_PanelTitle_Edit,
            showStubSettingsPanel: { show: true },
            cellStubId: rowData.Id
        });
    }

    onCreateStub = () => {
        this.setState({
            stubPanelTitle: RMResx.RM_AR_CP_Stub_PanelTitle_Create,
            showStubSettingsPanel: { show: true },
            cellStubId: null
        });
    }

    onDeleteStub = () => {
        this.args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>{RMResx.RM_AR_CP_Stub_DelMsg}</div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancelClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick = () => {
        $$.messagedialog(false);
        let urlData = "/api/StubSetting/DeleteStubSettings";
        let stubIdList = [];
        for (let key of this.state.stubChecked) {
            stubIdList.push(key.Id);
        }
        let option = {
            url: urlData,
            method: "POST",
            data: stubIdList
        };
        fetchUtility(option).then((res) => {
            this.onRefresh();
            if (res.MessageType == MessageType.Successful) {
                showToast.success(RMResx.RM_AR_CP_Stub_DelSuccessful);
            } else {
                showToast.error(res.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteCancelClick = () => {
        $$.messagedialog(false);
    }

    onSearchStart = (args) => {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.filterData.SearchValue = searchValue;
            this.initStubSettingData(true);
        } else {
            this.filterData.SearchValue = "";
            this.initStubSettingData(false);
        }
    }

    onPagerChange = (pagerIndex, pagerSize, callback) => {
        this.filterData.PageIndex = pagerIndex + 1;
        this.filterData.PageSize = pagerSize;
        this.filterData.SearchValue = "";
        this.setState({
            stubPagerIndex: pagerIndex,
            stubPagerSize: pagerSize
        });
        this.initStubSettingData(false);
        callback(true);
    };

    saveStubSettings = (e) => {
        this.dispatch("stubSettingsPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ showStubSettingsPanel: { show: false } });
                this.onRefresh();
            }
        });
        return false;
    }

    cancelStubSettings = () => {
        this.setState({ showStubSettingsPanel: { show: false } });
    }

    renderStubHeader() {
        return <div className="ra-main-header">
            <R.Searchbox
                placeholder={RMResx.RM_AR_CP_Common_Search}
                disabled={false}
                onSearch={this.onSearchStart}
                width={380}
            />
        </div>;
    }

    renderStubNavBar() {
        let selectStubCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.stubChecked.length, this.state.stubCount);
        return < div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: this.getShowActions() }}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className="ra-main-selected-counter">{selectStubCount}</div>
        </div >;
    }

    renderStubTable() {
        return <div className="ra-main-table">
            <StubTable
                id="raStubTable"
                columns={this.state.allColumns}
                uniqueKey={"Id"}
                checkable={true}
                onChange={this.onSelectChange}
                cellClick={this.onEditStub}
            />
        </div>;
    }

    renderStubFooter() {
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.stubCount}
                pagerIndex={this.state.stubPagerIndex}
                pagerSize={this.state.stubPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPagerChange} />
        </div>;
    }

    renderStubSettingsPanel() {
        return <R.Panel
            id="raStorageSettingsPanel"
            header={this.state.stubPanelTitle}
            size={670}
            status={this.state.showStubSettingsPanel}
            destroy={true}
        >
            <StubPanel
                id="stubSettingsPanel"
                ref={r => this.refStubSettingsPanel = r}
                cellStubId={this.state.cellStubId}
                recordsLabelValue={this.state.recordsLabelValue}
                isConfiguredGeneral={this.state.isConfiguredGeneral}
            ></StubPanel>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.cancelStubSettings}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.saveStubSettings}
                />
            </>
        </R.Panel>;
    }

    render() {
        return <div id="raStubSettingsManagement">
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_StubSettings]} />
            <div className="ra-page-container">
                <section>
                    {this.renderStubHeader()}
                    {this.renderStubNavBar()}
                    {this.renderStubTable()}
                    {this.renderStubFooter()}
                </section>
            </div>
            {this.renderStubSettingsPanel()}
        </div>;
    }
}