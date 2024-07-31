using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace ElasticSearchService.Services;

public class ElkClient
{
    public ElasticsearchClient Client;
    public ElkClient(IConfiguration configuration)
    {
        var url = configuration.GetValue<string>("ElasticSettings:Url");
        var fp = configuration.GetValue<string>("ElasticSettings:FingerPrint");
        var user = configuration.GetValue<string>("ElasticSettings:User");
        var password = configuration.GetValue<string>("ElasticSettings:Password");

        var settings = new ElasticsearchClientSettings(new Uri(url))
        .CertificateFingerprint(fp)
        .PrettyJson()
        .Authentication(new BasicAuthentication(user, password));

        Client = new ElasticsearchClient(settings);
    }
}