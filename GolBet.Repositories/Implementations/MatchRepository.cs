// GolBet.Repositories/Implementations/MatchRepository.cs
using GolBet.Entities;
using GolBet.Entities.Enums;
using GolBet.Repositories.Data;
using GolBet.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GolBet.Repositories.Implementations;

public class MatchRepository : GenericRepository<Match>, IMatchRepository
{
    public MatchRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Match>> GetAllWithTeamsAsync(MatchStatus? status = null, string? search = null)
    {
        var query = _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.IsActive)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        // NUEVO: Filtro de búsqueda por nombre de equipo local o visitante
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Entity Framework Core traduce .Contains() a un LIKE en SQL Server.
            // Como configuraste "SQL_Latin1_General_CP1_CI_AI" en el DbContext (Módulo 2), 
            // la búsqueda ya es insensible a mayúsculas y acentos automáticamente.
            query = query.Where(m => m.HomeTeam.Name.Contains(search) ||
                                     m.AwayTeam.Name.Contains(search));
        }

        return await query.OrderBy(m => m.Date).ToListAsync();
    }

    public async Task<Match?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Bets)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
}
