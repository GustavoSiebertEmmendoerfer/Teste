namespace Questao5.Application.Queries.Responses
{
    public class SaldoContaCorrenteResponse
    {
        public string Nome { get; set; }
        public int Numero { get; set; }
        public DateTime Data { get; set; }
        public decimal Saldo { get; set; }

        public SaldoContaCorrenteResponse(string nome, int numero, decimal saldo)
        {
            Nome = nome;
            Numero = numero;
            Data = DateTime.UtcNow;
            Saldo = saldo;
        }
    }
}
