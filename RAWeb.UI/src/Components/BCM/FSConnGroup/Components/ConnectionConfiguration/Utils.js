export default class Utils {

    static ConvertToSimpleConnections(connections) {
        const simpleConnections = [];

        for(const connection of connections) {
            simpleConnections.push({
                Id: connection.Id,
                Name: connection.Name,
                UNCPath: connection.UNCPath,
                checked: false
            });
        }

        return simpleConnections;
    }

    static ConvertToSimpleAgents(agents) {
        const simpleAgents = [];

        for(const agent of agents) {
            simpleAgents.push({
                Id: agent.Id,
                Name: agent.Name,
                SourceType: agent.SourceType,
                Status: agent.Status,
                checked: false
            });
        }

        return simpleAgents;


    }

}