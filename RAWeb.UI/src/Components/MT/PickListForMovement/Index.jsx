import SiteMapLinks from '../../../Constants/SiteMapLinks';
import PickListCommon from "../PickListCommon/Index";
import Template from "./Template";
import Actions from "./Actions";
import { TableColumns, StatusList } from "./Config";

const PickListForMovement = () => {
    return <>
        <$g.SiteMap data={[SiteMapLinks.MT_PickListForMovement]} />
        <div id="raMtPickListForMovement">
            <PickListCommon
                recordListApiUrl="/api/PickListApi/GetMoveRequets"
                tableColumns={TableColumns}
                tableTemplate={Template}
                statusList={StatusList}
                exportUrl={"/api/PickListApi/StartExportMoveJob"}
                Actions={Actions}
                useNumericPageIndex={true} 
            />
        </div>
    </>;
};

export default PickListForMovement;