using Common.Contracts;
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
        .DefaultIndex("logging_index")
        .CertificateFingerprint(fp)
        .PrettyJson()
        .Authentication(new BasicAuthentication(user, password));

        Client = new ElasticsearchClient(settings);
        Client.Indices.CreateAsync<ItemLoggingContract>("logging_index", index =>
            index.Settings(s =>
                s.Analysis(an => an
                    .Analyzers(a =>
                        a.Custom("rebuilt_russian", desc =>
                            desc.Tokenizer("standard")
                            .Filter(["lowercase", "russian_stemmer"])
                        )
                    )
                    .TokenFilters(f =>
                        f.Stemmer("russian_stemmer", desc =>
                            desc.Language("russian")
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties(p => p
                    .Text(t => t.ResponseLoggingContract.Result, t => t.Analyzer("rebuilt_russian"))
                    .Text(t => t.RequestLoggingContract.Body, t => t.Analyzer("rebuilt_russian"))
                )
            )
        );
    }

}