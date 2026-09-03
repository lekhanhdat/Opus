import React, { forwardRef, useImperativeHandle, useState } from 'react';
import { DiscoveryActionType, DiscoverySizeRangeQueryMode } from '../../../Constants';
import _ from 'lodash';
import useStableCallback from '../../../../../Common/Hooks/useStableCallback';
import { GoogleDriveProfileRequester } from '../../../requests/GoogleDrive';
import { GoogleDriveFileExtension, GoogleDriveWithoutModifiedDate } from '../../Inactive/Components';
import SortBy from '../../../Components/SortBy';
import { GoogleDriveGroupedRule } from '../Components';
import { showToast } from '../../../../../../Utilities/CommonUtil';

const panelTitleMap = new Map([
    [DiscoveryActionType.Create, RMResx.RM_DA_Analysis_DiscoveryActionType_Create],
    [DiscoveryActionType.Edit, RMResx.RM_DA_Analysis_DiscoveryActionType_Edit],
]);

const defaultProfileInfo = {
    name: "",
    sizeRange: -1,
    sizeRangeQueryMode: DiscoverySizeRangeQueryMode.GenerateThanEqual,
    greaterThanEqualWithoutInDate: -1,
    lessThanEqualWithoutInDate: 999,
    fileExtensionIds: [],
    ruleIds: [],
    sortBy: "FileTotalSize",
};

const buildInSortByColumns = [
    {
        internalName: "FileTotalSize",
        displayName: RMResx.RM_FA_ROTRule_TreeNode_SizeDataSize,
    },
    {
        internalName: "RotFileTotalSize",
        displayName: RMResx.RM_FA_ROT_TableColumn_ROTTotalSize,
    },
    {
        internalName: "RCategoryFileTotalSize",
        displayName: RMResx.RM_FA_ROTRule_TreeNode_RedundantDataSize,
    },
    {
        internalName: "OCategoryFileTotalSize",
        displayName: RMResx.RM_FA_ROTRule_TreeNode_ObsoleteDataSize,
    },
    {
        internalName: "TCategoryFileTotalSize",
        displayName: RMResx.RM_FA_ROTRule_TreeNode_TrivialDataSize,
    },
];

const OptimizationProfilePanel = ({organizationId, reRenderProfilesFunc}, ref) => {
    const [showPanel, setShowPanel] = useState(false);

    const [actionType, setActionType] = useState(DiscoveryActionType.Create);
    const [profileInfo, setProfileInfo] = useState(_.cloneDeep(defaultProfileInfo));

    const [existsProfileInfoList, setExistsProfileInfoList] = useState([]);
    const [validateInfo, setValidateInfo] = useState(new Map());

    useImperativeHandle(ref, () => ({
        onAdd: (profileInfo, profileInfoList = []) => {
            setShowPanel(true);
            setActionType(DiscoveryActionType.Create);
            setExistsProfileInfoList(profileInfoList);
            if (profileInfo == null) {
                setProfileInfo(_.cloneDeep(defaultProfileInfo));
                return;
            }

            setProfileInfo(_.cloneDeep(profileInfo));
        },
        onEdit: (profileInfo, profileInfoList = []) => {
            setActionType(DiscoveryActionType.Edit);
            setShowPanel(true);
            setProfileInfo(profileInfo);
            setExistsProfileInfoList(profileInfoList);
        },
    }));

    const onProfileNameChange = (value) => {
        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.name = value;
        setProfileInfo(clonedProfileInfo);
        if (validateInfo.has("name")) {
            const clonedValidateInfo = _.cloneDeep(validateInfo);
            clonedValidateInfo.delete("name");
            setValidateInfo(clonedValidateInfo);
        }
    };

    const onDateRangeChange = (queryParameter) => {
        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.greaterThanEqualWithoutInDate = queryParameter.withoutDateQueryParameter.from;
        clonedProfileInfo.lessThanEqualWithoutInDate = queryParameter.withoutDateQueryParameter.to;

        setProfileInfo(clonedProfileInfo);
    };

    const onFileExtensionChange = (queryParameter) => {
        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.fileExtensionIds = queryParameter.fileExtensionQueryParameter.fileExtensions;

        setProfileInfo(clonedProfileInfo);
    };

    const onRuleInfoChange = (queryParameter) => {
        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.ruleIds = queryParameter.rotRuleQueryParameter.ruleIds;

        setProfileInfo(clonedProfileInfo);
        if (validateInfo.has("rule")) {
            const clonedValidateInfo = _.cloneDeep(validateInfo);
            clonedValidateInfo.delete("rule");

            setValidateInfo(clonedValidateInfo);
        }
    };

    const onSortByChange = (queryParameter) => {
        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.sortBy = queryParameter.nodeQueryParameter.sortBy;

        setProfileInfo(clonedProfileInfo);
    };

    const onCancel = () => {
        $$.verify('allValidation', false, true);
        setShowPanel(false);
    }

    const onSave = useStableCallback(async () => {
        if (!$$.verify('allValidation')) return false;

        const clonedProfileInfo = _.cloneDeep(profileInfo);
        clonedProfileInfo.organizationId = organizationId;
        if (actionType == DiscoveryActionType.Create) {
            const res = await GoogleDriveProfileRequester.addRotProfileInfo(clonedProfileInfo);
            if (res.MessageType == 1) {
                showToast.error(res.ErrorMessage);
                return false;
            } else {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_DA_Profile_ProfileSave}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                    </$g.I18NProvider>
                );
            }
        } else {
            const res = await GoogleDriveProfileRequester.updateRotProfileInfo(clonedProfileInfo);
            if (res.MessageType == 1) {
                showToast.error(res.ErrorMessage);
                return false;
            } else {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_DA_Profile_ProfileUpdated}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                    </$g.I18NProvider>
                );
            }
        }

        setShowPanel(false);

        if (!_.isNil(reRenderProfilesFunc)) {
            await reRenderProfilesFunc(actionType);
        }

        return true;
    });

    const verifyName = useStableCallback((value) => {
        if (!value.trim()) {
            return RMResx.RM_PRM_PRE_ColumnValid_RequireText;
        } else if (value.length > 255) {
            return RMResx.RM_PF_profileNametooLong;
        } else if (existsProfileInfoList.some(item => item.name.toLowerCase().trim() === profileInfo.name?.toLowerCase().trim() && item.id !== profileInfo.id)) {
            return RMResx.RM_DA_Profile_ProfileName_Exists;
        }

        return true;
    });

    return (
        <R.Panel
            id="reco-inactive-profile-panel"
            header={panelTitleMap.get(actionType)}
            size={660}
            status={{ show: showPanel }}
            onHide={onCancel}
            destroy={false}
        >
            <R.Validation>
                <div id="allValidation" className="reco-inactive-profile-panel">
                    <div className="reco-profile-field-item">
                        <div className="reco-profile-field-title require">
                            {RMResx.RM_DA_Profile_ProfileName}
                        </div>
                        <div className="reco-profile-field-input">
                            <R.Validation element="Input" rules={{ verifyName }}>
                                <R.Input
                                    value={profileInfo.name}
                                    type="text"
                                    width={"100%"}
                                    onChange={onProfileNameChange}
                                    aria={{ ariaLabel: RMResx.RM_DA_Profile_ProfileName }}
                                />
                            </R.Validation>
                        </div>
                    </div>
                    <div className="reco-profile-field-item">
                        <div className="reco-profile-field-title require" tabIndex="0">
                            {RMResx.RM_DA_Profile_ProfileModifiedTimeRange}
                        </div>
                        <div
                            className="reco-profile-field-input"
                            style={{ width: 374 }}
                        >
                            <GoogleDriveWithoutModifiedDate
                                queryParameter={{
                                    withoutDateQueryParameter: {
                                        from: profileInfo.greaterThanEqualWithoutInDate,
                                        to: profileInfo.lessThanEqualWithoutInDate,
                                    },
                                }}
                                onChange={onDateRangeChange}
                            />
                        </div>
                    </div>
                    <div className="reco-profile-field-item">
                        <div id="ariaGroupedRule" className="reco-profile-field-title require">
                            {RMResx.RM_FA_ROTRule_Optimization_ROTrule}
                        </div>
                        <div className="reco-profile-field-input">
                            <GoogleDriveGroupedRule
                                queryParameter={{
                                    rotRuleQueryParameter: {ruleIds: profileInfo.ruleIds},
                                }}
                                onChange={onRuleInfoChange}
                                ariaId="ariaGroupedRule"
                            />
                            <div
                                className="reco-profile-field-validate-message"
                                hidden={!validateInfo.has("rule")}
                                tabIndex="0"
                            >
                                {validateInfo.get("rule")}
                            </div>
                        </div>
                    </div>
                    <div className="reco-profile-field-item">
                        <div id="ariaROTFileExtension" className="reco-profile-field-title require">
                            {RMResx.RM_DA_Profile_ProfileFileType}
                        </div>
                        <div className="reco-profile-field-input">
                            <GoogleDriveFileExtension
                                organizationId={organizationId}
                                queryParameter={{
                                    fileExtensionQueryParameter: {fileExtensions: profileInfo.fileExtensionIds},
                                }}
                                onChange={onFileExtensionChange}
                                ariaId="ariaROTFileExtension"
                            />
                        </div>
                    </div>
                    <div className="reco-profile-field-item">
                        <div id="ariaROTSortBy" className="reco-profile-field-title require">
                            {RMResx.RM_DA_Profile_ProfileSortBy}
                        </div>
                        <div className="reco-profile-field-input">
                            <SortBy
                                sortByColumns={_.cloneDeep(buildInSortByColumns)}
                                queryParameter={{
                                    nodeQueryParameter: {sortBy: profileInfo.sortBy},
                                }}
                                onChange={onSortByChange}
                                ariaId="ariaROTSortBy"
                            />
                        </div>
                    </div>
                </div>
            </R.Validation>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={onCancel}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSave}
                />
            </>
        </R.Panel>
    )
}

export default forwardRef(OptimizationProfilePanel);