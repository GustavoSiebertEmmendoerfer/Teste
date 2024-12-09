using Dapper;
using Microsoft.Data.Sqlite;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Infrastructure.Sqlite;

namespace Questao5.Infrastructure.Database.Repository
{
    public class ContaCorrenteRepository : IContaCorrenteRepository
    {
        private readonly DatabaseConfig _databaseConfig;

        public ContaCorrenteRepository(DatabaseConfig databaseConfig)
        {
            _databaseConfig = databaseConfig;
        }

        public async Task<ContaCorrente> GetById(string contaCorrenteId)
        {
            var sql = "SELECT * FROM contacorrente WHERE idcontacorrente = @Id";
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            return await connection.QuerySingleOrDefaultAsync<ContaCorrente>( sql, new { Id = contaCorrenteId });
        }

        public async Task<ContaCorrente> GetByNumero(int numero)
        {
            var sql = "SELECT * FROM contacorrente WHERE numero = @Numero";
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            return await connection.QuerySingleOrDefaultAsync<ContaCorrente>(sql, new { Numero = numero });
        }
    }
}
