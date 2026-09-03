import { NormalCell } from "../../../Common/TableTemplateCell";
import { IntelligentTermStatus, StatusByAccuracyCount, StatusByAccuracyStatus } from '../Config/Constains';
import ExistStatusText from '../../../Common/ExistStatusText';
import { ShowResultMsg } from '../Common';
import _ from "lodash";

export default class Template extends R.TableRow {
    
    constructor(props) {
        super(props);
        this.state = {
        };
    }
    
    getAccuracy(accuracyCount){
        let { NotApplicable, Bad, Normal } = StatusByAccuracyCount;
        switch(true){
            case accuracyCount == NotApplicable:
                return { status: "Disabled", name: StatusByAccuracyStatus[NotApplicable]};
            case _.inRange(accuracyCount, NotApplicable, Bad) :
                return { status: "Error", name: StatusByAccuracyStatus[Bad]};
            case _.inRange(accuracyCount, Bad, Normal) :
                return { status: "Warn", name: StatusByAccuracyStatus[Normal]};
            case accuracyCount >= StatusByAccuracyCount.Normal :
                return { status: "Success", name: RMResx.RM_ML_Accuracy_Good};
        }
    }

    onChangeAutoApply = (checked) => {
        const requestOption = {
            url: "/api/RMMLTermApi/SetAutoApply",
            data: {
                TermId: this.props.rowData.Id,  
                AutoApply: checked
            }
        };

        fetchUtility(requestOption).then((result) => {
            $$.loading(false);
            this.dispatch("SWITCH_AUTO_APPLY");
            ShowResultMsg(result, 
                checked ? RMResx.RM_ML_IT_ChangeAutoApplyTipSuccess : RMResx.RM_ML_IT_ChangeAutoApplyDisableTipSuccess, 
                RMResx.RM_ML_IT_ChangeAutoApplyTipError
            );
        }).catch((e) => {
            $$.loading(false);
        });
        return false;
    }

    onClickTrainingScope = () => {
        this.dispatch("CLICK_TRAINING_SCOPE");
    }
    
    render(Row, Cell) {
        let {
            Name,
            Status,
            FullPath,
            Accuracy,
            AutoApply,
            TrainingScope
        } = this.props.rowData;

        return <Row>
            <NormalCell Cell={Cell} contentText={Name} tooltip={FullPath}/>
            <NormalCell Cell={Cell} contentText={IntelligentTermStatus.get(Status)}/>
            <NormalCell Cell={Cell}>
                <ExistStatusText {...this.getAccuracy(Accuracy)}/>
            </NormalCell>
            <NormalCell Cell={Cell}>
                <a  
                    className="ra-main-cell-link" 
                    onClick={this.onClickTrainingScope}
                    tabIndex="0"
                >{RMResx.RM_ML_IT_TrainingScopeCounter.format(TrainingScope)}</a>
            </NormalCell>
            <NormalCell Cell={Cell}>
                <div className="ra-flex-align-center">
                    <R.Switch
                        checked={AutoApply}
                        willChange={this.onChangeAutoApply}
                    />
                    <div className="margin-left-xs">
                        {AutoApply ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}
                    </div>
                </div>
            </NormalCell>
        </Row>;
    }
}