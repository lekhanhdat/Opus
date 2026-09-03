import React from 'react';
import { Route } from 'react-router-dom';
import { Prompt } from 'react-router';
import { checkPermission } from '../../Utilities/permissionManager';

export default class RelatedRoute extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        let Resource = JSON.parse(this.props.resource);
        let isAllowed = false;
        if(window.location.pathname){
            let currentResource = window.location.pathname.toLocaleLowerCase();
            currentResource =  currentResource.endsWith("/") ? currentResource.slice(0,currentResource.length - 1) : currentResource;
            isAllowed = checkPermission(currentResource, [Resource]);
        }
        if(!isAllowed){
            window.location.href = window.location.origin + "/ErrorPage/NoPermission";
            console.log("redirect to /ErrorPage/NoPermission");
            return true;
        }

        let { routeConfig, exact } = this.props;
        return <React.Fragment>
            <Prompt message={RelatedRoute.PromptMsg} when={true} />
            <Route exact={exact} path={routeConfig.url} component={routeConfig.component} />
        </React.Fragment>;
    }
}

RelatedRoute.PromptMsg = "Prompt_RARoute";