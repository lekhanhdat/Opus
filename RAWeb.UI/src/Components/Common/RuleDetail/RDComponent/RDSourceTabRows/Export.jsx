import { formatBoolean } from "../../../../../Utilities/CommonUtil";
import { ExportType } from "../../Constains";
import DetailRow from "../../Common/DetailRow";
import { ExportSPDataOption, RuleLevel } from "../../../RuleItem/Components/Constants";
import { useEffect, useState } from "react";

const Export = ({ruleItem}) =>{
    const [exportRows, setExportRows] = useState([]);

    useEffect(() => {
        if (ruleItem && ruleItem.RuleLevel != RuleLevel.Teams) {
            setExportRows([
                {
                    label: RMResx.RM_JS_Rule_Detail_EXSP,
                    value: formatBoolean(ruleItem.ExportInfo?.exportSPDataOption === ExportSPDataOption.ExportBeforeArchive),
                    show: true
                },
                {
                    label: RMResx.RM_JS_Rule_Detail_EXFormat,
                    value: ExportType[ruleItem.ExportInfo?.exportType],
                    show: ruleItem.EnableExport
                }
            ]);
        }
    }, [ruleItem])

    return <React.Fragment>
        {
            exportRows.map((item, index)=>{
                return (
                    item.show && <DetailRow key={index} label={item.label}>
                        {item.value}
                    </DetailRow>
                );
            })
        }
    </React.Fragment>;
};

export default Export;