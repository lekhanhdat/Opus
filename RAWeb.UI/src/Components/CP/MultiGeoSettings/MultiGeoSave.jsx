import React, { Component } from 'react';
import { isEnableMultiGeoFeature, showToast } from "../../../Utilities/CommonUtil";

export default class MultiGeoSave extends R.Component { 
    constructor(props) {
        super(props);
        this.state = {
            isToggleStatus: false,
            isDisableSaveBtn: this.props?.ipErrors?.some(error => error !== ""),
            isEnableMultiGeo: isEnableMultiGeoFeature()
        };
        this.multiGeoSaveComponent = "multiGeoSaveComponent";
    } 

    handleSaveGeo = async () => {
        const { onValidate, onSaveSuccess } = this.props;
        const payload = onValidate ? onValidate() : null;

        if (!payload) {
            return;
        }
        this.setState({ isToggleStatus: false });
        $$.loading(true);

        const optionSaveSettings = {
            url: "/api/MultiGEOSettingApi/SaveMultiGeoSettings",
            method: "POST",
            data: payload
        };

        const optionEnableFeature = {
            url: "/api/MultiGEOManagementApi/EnableMultiGeoFeature",
            method: "POST"
        };

        try {
            const [saveResponse, enableResponse] = await Promise.all([
                fetchUtility(optionSaveSettings),
                fetchUtility(optionEnableFeature)
            ]);

            if (this.state.isEnableMultiGeo) {
                showToast.success(RMResx.RM_AR_CP_Multi_Geo_Save_Successful);
            } else {
                let content = <$g.I18NProvider msg={RMResx.RM_AR_CP_Multi_Geo_Message_Save}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            }

            if (window.RM && RM.gData) {
                RM.gData.enableMultiGEOFeature = true;
            }

            this.setState({ isEnableMultiGeo: true });

            this.dispatch('raGlobalDcSelector', { 
                type: 'MULTI_GEO_STATUS_CHANGED', 
                payload: { isEnabled: true } 
            });

            if (onSaveSuccess) {
                onSaveSuccess();
            }
        } catch (error) {
            showToast.error(RMResx.RM_AR_CP_Multi_Geo_Save_ErrorMessgae);
        } finally {
            $$.loading(false);
        }
    };

    componentUpdate(prevProps) {
        if (prevProps?.ipErrors !== this.props?.ipErrors) {
            this.setState({ 
                isDisableSaveBtn: this.props?.ipErrors?.some(error => error !== "")
            });
        }
    }

    handleCancel = () => {
        this.setState({ isToggleStatus: false });
    }

    handleShowDialog = () => {
        if (this.state.isEnableMultiGeo) {
            this.handleSaveGeo();
            return;
        }
        this.setState({ isToggleStatus: true });
    }

    showDialog = () => {
        return (
            <R.Dialog
                id="multiGeoSettingDialog"
                header={RMResx.RM_AR_CP_Multi_Geo_Toggle_Title}
                width={550}
                status={{ show: this.state.isToggleStatus }}
                struct={{ foot: true }}
                onHide={this.handleCancel}
                destroy={true}
            >
                <div id="multi-geo-setting-dialog">
                    <p>{RMResx.RM_AR_CP_Multi_Geo_Toggle_Tooltip}</p>
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleCancel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleSaveGeo} />
                </>
            </R.Dialog>
        )
    }

    render() {
        return (
            <div id={this.multiGeoSaveComponent}>
                <div className="ra-foot-btns flex justify-end align-center gap-s">
                    <R.Button text={RMResx.RM_JS_Common_Cancel} onClick={this.props?.onCancel}></R.Button>
                    <R.Button disabled={this.state.isDisableSaveBtn} primary text={RMResx.RM_JS_Common_Save} onClick={this.handleShowDialog}></R.Button>
                </div>
                {this.showDialog()}
            </div>
        )
    }
}