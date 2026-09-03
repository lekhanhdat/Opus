import { MessageType, StubFileType } from "../../../CP/CPConstants";
import "../../../../Less/BCM/ContentRepositoryManagement/convertStub.less";
import { showToast } from "../../../../Utilities/CommonUtil";

const StubTypeCol = [
    {
        name: RMResx.RM_AR_CP_Stub_Type_Aspx,
        value: StubFileType.Aspx,
        checked: true,
    },
    {
        name: RMResx.RM_AR_CP_Stub_Type_Txt,
        value: StubFileType.Txt,
        checked: false,
    },
    {
        name: RMResx.RM_AR_CP_Stub_Type_Html,
        value: StubFileType.Html,
        checked: false,
    },
    {
        name: RMResx.RM_AR_CP_Stub_Type_RestoreLink,
        value: StubFileType.Url,
        checked: false,
    },
]

export default class ConvertStubPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            stubType: StubFileType.Aspx,
            stubTypeList: RM.deepcopy(StubTypeCol),
            seletedStubTemplate: null,
            stubTemplateList: [],
        };
        this.source = this.props.source;
    }

    componentInit() {
        this.loadStubTemplates();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    loadStubTemplates = () => {
        $$.loading(true);
        let option = {
            url: "/api/StubSetting/GetAllStubSettings",
            method: "POST",
            data: {
                PageIndex: -1,
            }
        };
        fetchUtility(option).then((res) => {
            this.setState({
                stubTemplateList: res.StubSettingUIDtosList,
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }

        $$.loading(true);
        let dataObj = {
            StubType: this.state.stubType,
            StubTemplateId: this.state.seletedStubTemplate,
            NodeSetting: this.props.treeData,
        };
        let option = {
            url: '/api/SPSettingApi/RunConvertStubJob',
            method: "Post",
            data: dataObj
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                callback(true);
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onStubTypeChanged = (args) => {
        this.setState({ stubType: args.newValue.value });
    }

    onStubTemplateChanged = (args) => {
        this.setState({ seletedStubTemplate: args.newValue.Id });
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-convert-content">
                        <div className="ra-convert-title require">{RMResx.RM_JS_SP_ConvertStub_StubType}</div>
                        <R.Validation
                            element="Combobox"
                            require={RMResx.RM_AR_CP_Common_SelEmpty}
                        >
                            <R.Combobox
                                id="raStubTypeCom"
                                tooltipField="name"
                                width='100%'
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={this.state.stubTypeList}
                                onChange={this.onStubTypeChanged}
                                aria={{ ariaLabel: RMResx.RM_JS_SP_ConvertStub_StubType }}
                            />
                        </R.Validation>
                    </div>
                    <div className="ra-convert-content">
                        <div className="ra-convert-title require">{RMResx.RM_JS_SP_ConvertStub_StubTemplate}</div>
                        <R.Validation
                            element="Combobox"
                            require={RMResx.RM_AR_CP_Common_SelEmpty}
                        >
                            <R.Combobox
                                id="raStubTemplateCom"
                                tooltipField="Name"
                                width='100%'
                                textField="Name"
                                valueField="Id"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={this.state.stubTemplateList}
                                onChange={this.onStubTemplateChanged}
                                aria={{ ariaLabel: RMResx.RM_JS_SP_ConvertStub_StubTemplate }}
                            />
                        </R.Validation>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}