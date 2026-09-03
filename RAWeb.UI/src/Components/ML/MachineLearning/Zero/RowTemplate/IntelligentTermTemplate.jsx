import { NormalCell } from "../../../../Common/TableTemplateCell";
import { ShowResultMsg } from "../../Common";

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onChangeAutoApply = (checked) => {
        const requestOption = {
            url: "/api/RMMLTermApi/SetAutoApply",
            data: {
                TermId: this.props.rowData.Id,
                AutoApply: checked,
            },
        };

        fetchUtility(requestOption)
            .then((result) => {
                $$.loading(false);
                this.dispatch("SWITCH_AUTO_APPLY");
                ShowResultMsg(
                    result,
                    checked
                        ? RMResx.RM_ML_IT_ChangeAutoApplyTipSuccess
                        : RMResx.RM_ML_IT_ChangeAutoApplyDisableTipSuccess,
                    RMResx.RM_ML_IT_ChangeAutoApplyTipError
                );
            })
            .catch((e) => {
                $$.loading(false);
            });
        return false;
    };

    render(Row, Cell) {
        let { Name, Description, FullPath, ZeroApprovalCount, ZeroReclassifyCount, AutoApply } = this.props.rowData;

        return (
            <Row>
                <NormalCell Cell={Cell} contentText={Name} tooltip={FullPath} />
                <NormalCell Cell={Cell} contentText={Description} tooltip={Description} />
                <NormalCell Cell={Cell} contentText={ZeroApprovalCount} tooltip={ZeroApprovalCount} />
                <NormalCell Cell={Cell} contentText={ZeroReclassifyCount} tooltip={ZeroReclassifyCount} />
                <NormalCell Cell={Cell}>
                    <div className="ra-flex-align-center">
                        <R.Switch
                            checked={AutoApply}
                            willChange={this.onChangeAutoApply}
                        />
                        <div className="margin-left-xs">
                            {AutoApply
                                ? RMResx.RM_JS_Common_Yes
                                : RMResx.RM_JS_Common_No}
                        </div>
                    </div>
                </NormalCell>
            </Row>
        );
    }
}
