import { SourceFlags } from "../../../../Constants/Constants";
import { getSourceIcon } from "../../../../Utilities/CommonUtil";
import { IconText } from "../../../Common/IconText";
import { NormalCell } from "../../../Common/TableTemplateCell";

class AddScopeTemplate extends R.TableRow {
    constructor(props) {
        super(props);
    }

    render(Row, Cell) {
        const {
            FileName,
            FullPath,
            TermName,
            IsShowTermFullPath,
            TermFullPath,
            SourceFlag,
        } = this.props.rowData;

        return (
            <Row>
                <NormalCell Cell={Cell} tooltip={FileName}>
                    <IconText icon={getSourceIcon(SourceFlag)}>
                        {FileName}
                    </IconText>
                </NormalCell>
                {SourceFlag === SourceFlags.Google ? (
                    <NormalCell Cell={Cell} tooltip={FullPath}>
                        {FullPath}
                    </NormalCell>
                ) : (
                    <NormalCell Cell={Cell}>
                        <a
                            tabIndex="0"
                            className="ra-main-cell-link"
                            href={FullPath}
                            target="_blank"
                        >
                            {FullPath}
                        </a>
                    </NormalCell>
                )}
                <Cell>
                    {IsShowTermFullPath ? (
                        <div
                            className="text-overflow"
                            data-tooltip
                            data-tooltip-wrap="force"
                            aria-label={TermFullPath}
                            tabIndex="0"
                        >
                            {TermName}
                        </div>
                    ) : (
                        <div
                            className="text-overflow"
                            data-tooltip
                            data-tooltip-wrap="force"
                            aria-label={TermFullPath}
                            tabIndex="0"
                            onMouseOver={() =>
                                this.dispatch("showTermFullPath")
                            }
                            onFocus={() =>
                                this.dispatch("showTermFullPath")
                            }
                        >
                            {TermName}
                        </div>
                    )}
                </Cell>
            </Row>
        );
    }
}

export default AddScopeTemplate;
