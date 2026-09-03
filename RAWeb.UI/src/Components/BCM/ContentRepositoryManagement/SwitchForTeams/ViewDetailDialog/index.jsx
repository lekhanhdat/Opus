import { useEffect, useState } from "react";

import {
    ILColumnSettingComponent,
    ILContainerLevelSettingComponent,
    ILDocumentLevelSettingComponent,
    ILGeneralSettingComponent,
    ILMannualApprovalSettingComponent,
} from "./InformationLifeCycle";
import { SOArchivingSettingComponent, SOGeneralSettingComponent } from "./StorageOptimization";
import { ModuleType } from "../Constants";
import { EnableRecordManagementSetting } from "../../CRMForSPO/ArchiveCRMForSPO";
import CRMCommonUtil from "../../Common/CRMCommonUtil";
import { checkPermission } from "../../../../../Utilities/permissionManager";

function ViewDetailDialog({ scopeId, lifecycleId, soId, moduleType }) {
    const [nodeSetting, setNodeSetting] = useState(null);
    const [isCSDTenant, setIsCSDTenant] = useState(false);

    useEffect(() => {
        if (scopeId) {
            if (moduleType === ModuleType.Lifecycle) {
                if (lifecycleId) {
                    loadChannelNodeSetting();
                }
            } else {
                getTeamsChannelNodeSetting();
            }
        }
    }, [scopeId, lifecycleId, soId]);

    useEffect(() => {
        checkIsCSDTenant();
    }, []);

    // For IL
    const loadChannelNodeSetting = async () => {
        const option = {
            url: "/api/SPSettingApi/LoadChannelNodeSettings",
            method: "POST",
            data: {
                ScopeId: scopeId,
                Id: lifecycleId,
            },
        };
        $$.loading(true);
        const res = await fetchUtility(option);
        $$.loading(false);
        setNodeSetting(JSON.parse(res));
    };

    // For SO
    const getTeamsChannelNodeSetting = async () => {
        const option = {
            url: `/api/TeamsSettingApi/GetTeamsChannelNodeSetting?scopeId=${scopeId}&Id=${soId}`,
            method: "GET",
        };
        $$.loading(true);
        const res = await fetchUtility(option);
        $$.loading(false);
        setNodeSetting(res);
    };

    const checkIsCSDTenant = () => {
        $$.loading(true);
        const option = {
            url: "/api/RuleApi/CheckIsCSDTenant",
            method: "POST",
        };
        fetchUtility(option)
            .then((res) => setIsCSDTenant(res))
            .finally(() => $$.loading(false));
    };

    const renderContent = () => {
        if (!nodeSetting) return null;

        if (moduleType === ModuleType.Lifecycle) {
            return (
                <>
                    <ILGeneralSettingComponent nodeSetting={nodeSetting} />
                    {nodeSetting.EnableRecordManagement === EnableRecordManagementSetting.Enable && (
                        <>
                            <ILColumnSettingComponent nodeSetting={nodeSetting} isCSDTenant={isCSDTenant} />
                            {(!nodeSetting.IsUsingExistColumnName
                                || (nodeSetting.IsUsingExistColumnName && nodeSetting.SetDocLevelTermForExistColumn)) && (
                                    <ILDocumentLevelSettingComponent nodeSetting={nodeSetting} isCSDTenant={isCSDTenant} />
                            )}
                            {!CRMCommonUtil.isFolder(nodeSetting) && (
                                <ILContainerLevelSettingComponent nodeSetting={nodeSetting} />
                            )}
                            <ILMannualApprovalSettingComponent nodeSetting={nodeSetting} />
                        </>
                    )}
                </>
            );
        }

        if (moduleType === ModuleType.SO) {
            return (
                <>
                    <SOGeneralSettingComponent nodeSetting={nodeSetting} />
                    {nodeSetting.EnableArchiverManagement === EnableRecordManagementSetting.Enable && (
                        <SOArchivingSettingComponent nodeSetting={nodeSetting} />
                    )}
                </>
            );
        }
    };

    return (
        <div id="raCrmConflictSettingDetail" className="flex flex-column gap-l">
            {renderContent()}
        </div>
    );
}

export default ViewDetailDialog;
