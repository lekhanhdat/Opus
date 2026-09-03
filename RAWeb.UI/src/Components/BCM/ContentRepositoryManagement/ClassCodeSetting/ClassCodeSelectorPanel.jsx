import { showToast } from "../../../../Utilities/CommonUtil";
import ClassCodeTree from "../../../Common/Tree/Instances/TermTree/SelectClassCodeTree";
import { RAMessageType } from "../Common/CRMCommonUtil";

export class ClassCodeSelectorPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            searchKey: ""
        };
        this.selectedClassCode = new Map();
    }

    componentReceive(type, selectedNode, callback) {
        if (type === "runJob") {
            this.onRunJob(selectedNode, callback);
        }
    }

    customDefaultClassCodeScopeValid = () => {
        var selectedDefaultClassCodeTree = this.selectedClassCode.size === 0;
        if (selectedDefaultClassCodeTree) {
            return RMResx.RM_JS_FS_SelectClassCode_ValidationMsg;
        } else {
            return true;
        }
    }

    onSearchClassCodeTree = (args) => {
        this.setState({ searchKey: args });
    }

    onNodeSelectedChange = (node) => {
        if (node) {
            if (!node.checked && this.selectedClassCode.has(node.nodeKey)) {
                this.selectedClassCode.delete(node.nodeKey);
            } else {
                this.selectedClassCode.set(node.nodeKey, node.origin?.UniqueId);
            }
        }
        $$.verify(this.refSelectClassCodeScopeValid.ref.current)
    }

    onRunJob(selectedNode, callback) {
        if (!$$.verify(this.refSelectClassCodeScopeValid.ref.current)) {
            return false;
        }
        const { Id, Level, FullPath, Name, Parent } = selectedNode;
        const payload = {
            ConnectionGroupID: selectedNode?.ConnGroupId,
            NodeId: Id,
            TermID: Array.from(this.selectedClassCode.values()),
            FullPath,
            Name,
            Level,
            Parent,
        }
        const option = {
            method: 'POST',
            url: '/api/FSSettingApi/RunFSClassCodeDisposalJob',
            data: payload
        }
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == RAMessageType.Successful) {
                let content = (
                    <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>
                );
                showToast.success(content);
            } else if (result.MessageType == RAMessageType.Failed) {
                if(result.Extension === "-1") {
                    this.confirmNoClassCodeMatchedDialog(result.Extsion1);
                    return;
                }
                if (result.ErrorMessage != "") {
                    showToast.error(result.ErrorMessage);
                }
            }
        }).catch((e) => {
            console.error('Error when running job: ', e);
            $$.loading(false);
        });

        callback();
    }

    confirmNoClassCodeMatchedDialog(classCodeNames) {
        const content = (
            <div>
                <div>{RMResx.RM_FS_ClassCode_Unassigned}</div>
                <div className="margin-top-m strong">{classCodeNames}</div>
                <div className="margin-top-m">{RMResx.RM_FS_ApplyClassCode_RunJob}</div>
            </div>
        );
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: content,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
            ],
        });
    }

    render() {
        return (
            <div id={this.props.id}>
                <div className="ra-selectterms-searchbox">
                    <R.Searchbox
                        width={'100%'}
                        placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                        onSearch={this.onSearchClassCodeTree}
                    />
                </div>

                <div className="margin-top-m require strong" tabIndex={0}>
                    {RMResx.RM_JS_FS_SelectClassCode}
                </div>

                <div className="margin-top-s margin-left-l">
                    <R.ValidationFaker valid={this.customDefaultClassCodeScopeValid} ref={r => this.refSelectClassCodeScopeValid = r} />
                </div>

                <div className="ra-setting-panel-treepadding">
                    <ClassCodeTree
                        ref={r => this.refClassCodeScopeTree = r}
                        searchKey={this.state.searchKey}
                        onNodeSelectedChange={this.onNodeSelectedChange}
                        termSetId={this.props.selectedNode?.TermSetId}
                    />
                </div>

            </div>
        )
    }
}