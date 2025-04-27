using Common.Contracts.Logging;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Logging.Services;

public class ElkClient
{
    public ElasticsearchClient Client;
    public ElkClient(IConfiguration configuration)
    {
        var url = configuration["elk:host"];
        var fp = configuration["elk:fingerprint"];
        var user = configuration["elk:username"];
        var password = configuration["elk:password"];

        var settings = new ElasticsearchClientSettings(new Uri(url))
        .CertificateFingerprint(fp)
        .PrettyJson()
        .Authentication(new BasicAuthentication(user, password));
        Client = new ElasticsearchClient(settings);
    }

}