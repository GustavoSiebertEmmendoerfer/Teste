using Dapper;
using Microsoft.Data.Sqlite;
using Questao5.Domain.Entities;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Infrastructure.Sqlite;

namespace Questao5.Infrastructure.Database.Repository
{
    public class MovimentoRepository : IMovimentoRepository
    {
        private readonly DatabaseConfig _databaseConfig;
        public MovimentoRepository(DatabaseConfig databaseConfig)
        {
            _databaseConfig = databaseConfig;
        }


        public async Task AddAsync(Movimento movimento)
        {
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            var tipo = movimento.TipoMovimento == "C" ? 'C' : 'D';
            var sql = "INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";
            await connection.ExecuteAsync(sql, new { movimento.IdMovimento, movimento.IdContaCorrente, movimento.DataMovimento, TipoMovimento = tipo, movimento.Valor });
        }
        public async Task<IEnumerable<Movimento>> GetAllByIdContaCorrenteAndTipoMovimento(string idContaCorrente, string tipoMovimentacao)
        {
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            var tipo = tipoMovimentacao == "C" ? 'C' : 'D';
            var sql = "SELECT * FROM movimento where idcontacorrente = @ContaCorrenteId AND tipomovimento = @TipoMovimento";
            var result = await connection.QueryAsync<Movimento>(sql, new { ContaCorrenteId = idContaCorrente, TipoMovimento = tipo });
            return result;
        }

        public async Task<IEnumerable<Movimento>> GetMovimentacaoPorTipo(string contaCorrenteId, string tipoMovimentacao)
        {
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            var tipo = tipoMovimentacao == "C" ? 'C' : 'D';
            var sql = "SELECT * FROM movimento where idcontacorrente = @ContaCorrenteId AND tipomovimento = @TipoMovimento";
            var result = await connection.QueryAsync<Movimento>(sql, new { ContaCorrenteId = contaCorrenteId, TipoMovimento = tipo });
            return result;
        }
    }
}
