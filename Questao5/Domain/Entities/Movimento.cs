using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Questao5.Domain.Entities
{
    public class Movimento
    {
        public string IdMovimento { get; protected set; } = Guid.NewGuid().ToString();

        public DateTime DataMovimento { get; protected set; }

        public string TipoMovimento { get; protected set; }

        public decimal Valor { get; protected set; }

        public string IdContaCorrente { get; protected set; }

        public Movimento() { }

        public Movimento(string idContaCorrente, string tipoMovimento, decimal valor)
        {
            IdContaCorrente = idContaCorrente;
            TipoMovimento = tipoMovimento;
            DataMovimento = DateTime.UtcNow;
            Valor = valor;
        }
    }
}
