using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Questao5.Domain.Entities
{
    public class Idempotencia
    {
        public string Chave_Idempotencia { get; protected set; }

        public string Requisicao { get; protected set; }

        public string Resultado { get; protected set; }

        public Idempotencia() { }

        public Idempotencia(string chaveIdempotencia, string requisicao, string resultado)
        {
            Chave_Idempotencia = chaveIdempotencia;
            Requisicao = requisicao;
            Resultado = resultado;
        }
    }
}
