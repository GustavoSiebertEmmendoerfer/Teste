using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Questao5.Domain.Entities
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; protected set; }

        public int Numero { get; protected set; }

        public string Nome { get; protected set; }

        public bool Ativo { get; protected set; }

        public ContaCorrente() { }

        public ContaCorrente(string idContaCorrente)
        {
            IdContaCorrente = idContaCorrente;
        }

        public ContaCorrente(string idContaCorrente, int numero, string nome) : this(idContaCorrente)
        {
            Numero = numero;
            Nome = nome;
        }

        public void SetAtivo(bool value)
            => Ativo = value;
    }
}
