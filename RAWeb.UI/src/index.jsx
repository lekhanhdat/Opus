import GetLoginInfo from './Components/Base/LoginInfo';
import { initSessionManagement } from './Components/Base/SessionManagement';
import GainsightConfig from "./Components/Base/GainsightConfig";
import GainsightUtil from "./Utilities/GainsightUtil";
import * as ReactRouterDOM from "react-router-dom";
import RelatedRoute from './Components/Base/RelatedRoute';
import RelatedRecordsRouteConfig from "./Routers/RelatedRecordRouter";
import { Provider } from "react-redux";
import { store } from "./Redux/appStore";
import "../node_modules/@gui/allure-font/AllureFont.css";
import "./Components/Layouts/FontAwesomeFont.css";
import dompurify from "dompurify";

$$.useRouter(ReactRouterDOM);
window.DOMPurify = dompurify;

let option = {
    url: "/api/RelatedRecordsApi/IsRelatedRecord",
    method: "POST",
};
$$.loading(true);
fetchUtility(option).then(function (result) {
    $$.loading(false);

    let Parameter = result;
    if (Parameter) {
        RM.TimeSettingModel = JSON.parse(result.TimeSettingModel);
        RM.TimeUtil.init();
        RM.RedirectUrl = Parameter.RedirectUrl;
        let RelatedRouter = `/RelatedRecords${Parameter.QueryParameters}`;
        ReactDOM.render(
            <ReactRouterDOM.BrowserRouter>
                <ReactRouterDOM.Switch>
                    <RelatedRoute 
                        exact={false}
                        path={RelatedRecordsRouteConfig.url}
                        key={1}
                        routeConfig={RelatedRecordsRouteConfig}
                        resource={Parameter.ResourceViaPermission}
                    />
                    <ReactRouterDOM.Route path="*"><ReactRouterDOM.Redirect to={RelatedRouter}/></ReactRouterDOM.Route>
                </ReactRouterDOM.Switch>
            </ReactRouterDOM.BrowserRouter>,
            document.getElementById('root')
        );
    }else{
        initSessionManagement();
        let callback = function(){
            RM.SwitchLanguage.setFontFamily();
            RM.TimeUtil.init();
            $$.I18N.timezones = RM.TimeSettingModel.TimeZoneInfo;
            (async function() {
                await GainsightConfig();
                GainsightUtil.Identity();
                let RootRouter = require("./Routers/RootRouter").default;
                ReactDOM.render(
                    <Provider store={store}>
                        <RootRouter/>
                    </Provider>,
                    document.getElementById('root'));
            })();
        };
        GetLoginInfo(callback);
    }   
});

var messagedialogFunc = $$.messagedialog;
window.$$.messagedialog = function () {
    try {
        const args = Array.prototype.slice.call(arguments);
        messagedialogFunc.apply(null, args);
        // console.log(args);
    } catch { 
        console.log("Compatible messagedialog logic, cannot be deleted.");
    }
};
