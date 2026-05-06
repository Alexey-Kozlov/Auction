using Microsoft.EntityFrameworkCore;
using Npgsql;
using SearchService.Data;

namespace SearchService.Services;

public class SearchProceduresService
{
    private readonly SearchDbContext _dbContext;

    public SearchProceduresService(SearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    //вызов процедур обслуживания записей аукционов

    //ResetItems - при восстановлении из SnapShot - удаление всех записей из таблицы SearchItems
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
        await _dbContext.Database.ExecuteSqlRawAsync("call search_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }

    // CommitItems - при фиксации или откете распределенной транзакции - 
    // обработка записей в таблице SearchItems
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
        await _dbContext.Database.ExecuteSqlRawAsync("call search_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }
}