import { NormalCell } from "../../Common/TableTemplateCell";
import { getSourceIcon } from "../../../Utilities/CommonUtil";
import { IconText, LinkText } from '../../Common/IconText';
import { SourceFlags } from "../../../Constants/Constants";

export default class Template extends R.TableRow {
    
    constructor(props) {
        super(props);
        this.state = {
        };
    }
 
    render(Row, Cell) {
        let {
            sourceFlag,
            leafName,
            predictTermName,
            fullPath,
            recordsId,
            fileExtension,
            retentionStatus,
            escalateFromDisplayName,
            reviewerDisplayNames,
            escalatedComment,
            modifiedBy,
            createdBy,
            collectionTime,
            createdTime,
            modifiedTime,
            predictTermFullPath
        } = this.props.rowData;

        return <Row>
            <NormalCell Cell={Cell} tooltip={fullPath}>
                <IconText icon={getSourceIcon(sourceFlag)}>
                    {sourceFlag == SourceFlags.Google ? (
                        <div tabIndex={0} className="ra-ellipsis">{leafName}</div>
                    ) : (
                        <LinkText href={fullPath} text={leafName}/>
                    )}
                </IconText>
            </NormalCell>
            <NormalCell Cell={Cell} contentText={predictTermName} tooltip={predictTermFullPath}></NormalCell>
            <NormalCell Cell={Cell} contentText={recordsId}/>
            <NormalCell Cell={Cell} contentText={`${fileExtension}${retentionStatus === 1 ? `(${RMResx.RM_MA_Extended_RetentionStatus})` : ""}`}/>
            <NormalCell Cell={Cell} contentText={escalateFromDisplayName}/>
            <NormalCell Cell={Cell} contentText={reviewerDisplayNames.join("; ")}/>
            <NormalCell Cell={Cell} contentText={escalatedComment}/>
            <NormalCell Cell={Cell} contentText={modifiedBy}/>
            <NormalCell Cell={Cell} contentText={modifiedTime}/>
            <NormalCell Cell={Cell} contentText={createdBy}/>
            <NormalCell Cell={Cell} contentText={createdTime}/>
            <NormalCell Cell={Cell} contentText={collectionTime}/>
        </Row>;
    }
}  