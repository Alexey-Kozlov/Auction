using BiddingService.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BiddingService.Services;

public class BidProceduresService
{
    private readonly BidDbContext _dbContext;

    public BidProceduresService(BidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ResetItems(Guid correlationId)
    {
        var correlationid_par = new NpgsqlParameter("correlationid_par", System.Data.DbType.Guid);
        correlationid_par.Direction = System.Data.ParameterDirection.Input;
        correlationid_par.Value = correlationId;
        var reset_par = new NpgsqlParameter("reset_par", System.Data.DbType.Boolean);
        reset_par.Direction = System.Data.ParameterDirection.Input;
        reset_par.Value = true;
        var commit_par = new NpgsqlParameter("commit_par", System.Data.DbType.Boolean);
        commit_par.Direction = System.Data.ParameterDirection.Input;
        commit_par.Value = true;
        await _dbContext.Database.ExecuteSqlRawAsync("call bid_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }

    public async Task CommitItems(Guid correlationId, bool commit)
    {
        var correlationid_par = new NpgsqlParameter("correlationid_par", System.Data.DbType.Guid);
        correlationid_par.Direction = System.Data.ParameterDirection.Input;
        correlationid_par.Value = correlationId;
        var reset_par = new NpgsqlParameter("reset_par", System.Data.DbType.Boolean);
        reset_par.Direction = System.Data.ParameterDirection.Input;
        reset_par.Value = false;
        var commit_par = new NpgsqlParameter("commit_par", System.Data.DbType.Boolean);
        commit_par.Direction = System.Data.ParameterDirection.Input;
        commit_par.Value = commit;
        await _dbContext.Database.ExecuteSqlRawAsync("call bid_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }
}