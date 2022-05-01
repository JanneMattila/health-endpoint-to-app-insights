# Health endpoint to App Insights

Monitor Spring Boot actuator health endpoint and report it to App Insights

## Prepare Azure infra

```bash
az extension add -n application-insights

workspace_name="log-monitoring"
ai_name="ai-health"
location="northeurope"
resource_group="rg-health-monitoring"

az group create -l $location -n $resource_group -o table


workspace_id=$(az monitor log-analytics workspace create -g $resource_group -n $workspace_name --query id -o tsv)
echo $workspace_id

ai_json=$(az monitor app-insights component create --app $ai_name --location $location --kind web -g $resource_group --workspace $workspace_id -o json)
ai_connectionstring=$(echo $ai_json | jq -r .connectionString)
echo $ai_connectionstring
```

## Links

[Sprint Boot and Production-ready Features](https://docs.spring.io/spring-boot/docs/current/reference/html/actuator.html#actuator.endpoints.health)

[Java codeless application monitoring on-premises - Azure Monitor Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-on-premises)

[Azure Monitor OpenTelemetry-based auto-instrumentation for Java applications](https://docs.microsoft.com/en-us/azure/azure-monitor/app/java-in-process-agent)
