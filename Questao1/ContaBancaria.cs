using System;
using System.Globalization;

namespace Questao1
{
    class ContaBancaria {

        public readonly int Numero;
        public string Titular { get; set; }
        public double Saldo { get; private set; }

        public ContaBancaria(int numero, string titular, double depositoInicial)
        {
            Numero = numero;
            Titular = titular;
            Saldo = IsPositive(depositoInicial) ? depositoInicial : 0d;
        }

        public ContaBancaria(int numero, string titular)
        {
            Numero = numero;
            Titular = titular;
            Saldo = 0d;
        }

        public override string ToString()
        {
            return $"Conta {Numero}, Titular: {Titular}, Saldo: $ {Saldo.ToString("F2", CultureInfo.InvariantCulture)}";
        }

        public void Deposito(double deposito)
        { 
            if(IsPositive(deposito))
                Saldo += deposito;
        }

        public void Saque(double saque)
        {
            if (IsPositive(saque))
                Saldo -= saque + 3.50;
        }

        private bool IsPositive(double valor)
        {
            return valor > 0d;
        }
    }
}
