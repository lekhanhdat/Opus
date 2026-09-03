import "../index.less";
import FileScopeOptimizeExpander from "./FileScopeComponent/FileScopeOptimizeExpander";
import ObjectRuleExpander from "./ObjectRuleComponent/ObjectRuleExpander";
import ProcessActionExpander from "./ProcessActionExpander";
import StorageLocation from "./StorageLocation";
import ScheduleConfig from "./ScheduleConfig";

const PanelContent = ({ dataOptimizeParameter, onChange, o365TenantId }) => {
    return (
        <div>
            <FileScopeOptimizeExpander
                dataOptimizeParameter={dataOptimizeParameter}
                o365TenantId={o365TenantId}
                onChange={onChange}
            />
            <ObjectRuleExpander
                dataOptimizeParameter={dataOptimizeParameter}
                o365TenantId={o365TenantId}
                onChange={onChange}
            />
            <ProcessActionExpander
                dataOptimizeParameter={dataOptimizeParameter}
                onChange={onChange}
            />
            <StorageLocation
                dataOptimizeParameter={dataOptimizeParameter}
                onChange={onChange}
            />
            <ScheduleConfig
                dataOptimizeParameter={dataOptimizeParameter}
                onChange={onChange}
            />
        </div>
    );
};

export default PanelContent;