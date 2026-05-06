using CommunicationService.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CommunicationService.Services;

public class CommunicationProceduresService
{
    private readonly CommunicationDbContext _dbContext;

    public CommunicationProceduresService(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ResetItems(Guid correlationId)
    {
        var correlationid_par = new NpgsqlParameter("correlationid_par", System.Data.DbType.Guid)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = correlationId
        };
        var reset_par = new NpgsqlParameter("reset_par", System.Data.DbType.Boolean)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = true
        };
        var commit_par = new NpgsqlParameter("commit_par", System.Data.DbType.Boolean)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = true
        };
        await _dbContext.Database.ExecuteSqlRawAsync("call communication_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }

    public async Task CommitItems(Guid correlationId, bool commit)
    {
        var correlationid_par = new NpgsqlParameter("correlationid_par", System.Data.DbType.Guid)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = correlationId
        };
        var reset_par = new NpgsqlParameter("reset_par", System.Data.DbType.Boolean)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = false
        };
        var commit_par = new NpgsqlParameter("commit_par", System.Data.DbType.Boolean)
        {
            Direction = System.Data.ParameterDirection.Input,
            Value = commit
        };
        await _dbContext.Database.ExecuteSqlRawAsync("call communication_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }
}