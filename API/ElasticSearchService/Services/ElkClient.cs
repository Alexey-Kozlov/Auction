using Common.Contracts;
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
        .DefaultIndex("search_index")
        .CertificateFingerprint(fp)
        .PrettyJson()
        .Authentication(new BasicAuthentication(user, password));

        Client = new ElasticsearchClient(settings);
        Client.Indices.CreateAsync<AuctionCreatingElk>("search_index", index =>
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
                    .Text(t => t.Title, t => t.Analyzer("rebuilt_russian"))
                    .Text(t => t.Properties, t => t.Analyzer("rebuilt_russian"))
                    .Text(t => t.Description, t => t.Analyzer("rebuilt_russian"))
                )
            )
        );
    }

}