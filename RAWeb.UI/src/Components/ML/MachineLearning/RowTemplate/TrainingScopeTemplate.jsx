import { NormalCell, TooptipByApiCell } from "../../../Common/TableTemplateCell";
import { SourceFlags, TooptipByApiCellType } from "../../../../Constants/Constants";
import { TrainingScopeStatusType, TrainingScopeStatus } from '../Config/Constains';
import { getSourceIcon } from "../../../../Utilities/CommonUtil";
import { IconText, LinkText } from '../../../Common/IconText';
import ExistStatusText from '../../../Common/ExistStatusText';

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
    }

    getStatus(status){
        switch(status){
            case TrainingScopeStatusType.NotTrain:
                return { status: "Disabled", name: TrainingScopeStatus.get(status)};
            case TrainingScopeStatusType.Training:
                return { status: "Info", name: TrainingScopeStatus.get(status)};
            case TrainingScopeStatusType.Trained:
                return { status: "Success", name: TrainingScopeStatus.get(status)};
        }
    }
    
    render(Row, Cell) {
        let {
            FileName,
            Status,
            TermName,
            SourceFlag,
            FullPath
        } = this.props.rowData;
        return <Row>
            <NormalCell Cell={Cell} tooltip={FullPath}>
                <IconText icon={getSourceIcon(SourceFlag)}>
                    {SourceFlag == SourceFlags.Google ? (
                        <div tabIndex={0} className="ra-ellipsis">{FileName}</div>
                    ) : (
                        <LinkText href={FullPath} text={FileName} className="ra-main-cell-link ra-traning-scope-cell-link" />
                    )}
                </IconText>
            </NormalCell>
            <NormalCell Cell={Cell}>
                {<TooptipByApiCell 
                    cellType={TooptipByApiCellType.Term} 
                    rowData={this.props.rowData}
                    contentText={TermName}
                />}
            </NormalCell>
            <NormalCell Cell={Cell}>
                <ExistStatusText {...this.getStatus(Status)}/>
            </NormalCell>
        </Row>; 
    }
}