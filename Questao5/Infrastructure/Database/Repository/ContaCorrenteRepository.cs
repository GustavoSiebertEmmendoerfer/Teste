using Dapper;
using Microsoft.AspNetCore.Mvc;
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
            sql = string.Concat(sql, contaCorrenteId);
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            return await connection.QuerySingleOrDefaultAsync<ContaCorrente>(string.Concat(sql, contaCorrenteId), new { Id = contaCorrenteId });
        }

        public async Task<ContaCorrente> GetByNumero(int numero)
        {
            var sql = "SELECT * FROM contacorrente WHERE numero = @Numero";
            await using var connection = new SqliteConnection(_databaseConfig.Name);
            return await connection.QuerySingleOrDefaultAsync<ContaCorrente>(sql, new { Numero = numero });
        }

        public async Task<object> ListarHistoricoDeDispositivosPorPermissao<T>(T parametros,
                                                                            CancellationToken cancellationToken = default)
        {
            var especificacao = new HistoricoDispositivoSpecification();

            await using var connection = new SqliteConnection(_databaseConfig.Name);

            var items = await connection.QueryAsync<object>(
                new CommandDefinition(PermissaoMfaQueryStoreConsts.SQL_LISTAR_DISPOSITIVOS_INSTALADOS_ANTERIORMENTE_POR_PERMISSAO + especificacao.ParaSql(),
                                      especificacao.Parametros,
                                      cancellationToken: cancellationToken));

            var total = await connection.QueryFirstAsync<object>(
                new CommandDefinition(PermissaoMfaQueryStoreConsts.SQL_LISTAR_DISPOSITIVOS_INSTALADOS_ANTERIORMENTE_POR_PERMISSAO,
                                      especificacao.Parametros,
                                      cancellationToken: cancellationToken));

            return new object();
        }

    }



    public class ListarHistoricoDeDispositivosPorPermissaoQuery
    {
        public Guid Id { get; set; }

        [FromQuery]
        public string CamposOrdenar { get; set; }
    }

    public abstract class SpecificationComPaginacao : Specification
    {
        protected SpecificationComPaginacao(int pagina = 1,
                                            int quantidadeRegistros = 1)
        {
            Parametros.AddDynamicParams(new
            {
                pr_pagina = (pagina - 1) * quantidadeRegistros,
                pr_quantidaderegistros = quantidadeRegistros
            });
        }

        protected static readonly string SqlPaginacao = @" OFFSET :pr_pagina ROWS FETCH NEXT :pr_quantidaderegistros ROWS ONLY";

        public override string ParaSql()
        {
            return string.Concat(OrdenarPor, SqlPaginacao);
        }
    }

    public abstract class Specification
    {
        protected Specification()
        {
            Parametros = new();
        }

        public string OrdenarPor { get; set; } = string.Empty;

        public DynamicParameters Parametros { get; set; }

        protected void AgruparOrdenacao(string campo,
                                        string campoAuxiliarParaDirecaoOrdenacao)
        {
            if (string.IsNullOrWhiteSpace(campo))
                return;

            OrdenarPor = string.Concat(string.IsNullOrEmpty(OrdenarPor) ? " ORDER BY " : $"{OrdenarPor},", campo, " ", ObterDirecaoOrdenacao(campoAuxiliarParaDirecaoOrdenacao));
        }

        private string ObterDirecaoOrdenacao(string campoOrdenacao)
        {
            if (string.IsNullOrWhiteSpace(campoOrdenacao))
                return string.Empty;

            return campoOrdenacao.Contains('-') ? "DESC" : "ASC";
        }

        public abstract string ParaSql();

    }


    public sealed class HistoricoDispositivoSpecification : SpecificationComPaginacao
    {
        public HistoricoDispositivoSpecification(Guid idPermissao,
        int pagina,
        int quantidadeRegistros,
        string camposOrdenacao) : base(pagina, quantidadeRegistros)
        {
            Parametros.AddDynamicParams(new
            {
                pr_codigopermissao = idPermissao,
                pr_iderrotentativa = 1,
                pr_idsucessoinstalacao = 1,
            });

            MontarOrdenacao(camposOrdenacao);
        }

        public HistoricoDispositivoSpecification()
        {

        }

        private void MontarOrdenacao(string camposOrdenacao)
        {
            camposOrdenacao ??= string.Empty;

            var camposOrdenacaoSplitado = camposOrdenacao.Split(',');

            foreach (var campoOrdenacao in camposOrdenacaoSplitado)
            {
                switch (campoOrdenacao.ToUpper())
                {
                    case var dataInstalacaoToken when dataInstalacaoToken.Contains("DATAINSTALACAOTOKEN"):
                        AgruparOrdenacao("TDA.DHINSTALACAO_TOKEN", campoOrdenacao);
                        break;
                    case var versaoAplicativo when versaoAplicativo.Contains("VERSAOAPLICATIVO"):
                        AgruparOrdenacao("TDA.DSVERSAO_APP", campoOrdenacao);
                        break;
                    case var instalado when instalado.Contains("INSTALADO"):
                        AgruparOrdenacao("TDA.FLAPP_INSTALADO", campoOrdenacao);
                        break;
                    case var dispositivo when dispositivo.Contains("IDINSTALACAO"):
                        AgruparOrdenacao("TDA.CDINSTALACAO", campoOrdenacao);
                        break;
                    case var dispositivo when dispositivo.Contains("DISPOSITIVO"):
                        AgruparOrdenacao("TDA.CDDISPOSITIVO", campoOrdenacao);
                        break;
                    case var dataUltimoAcesso when dataUltimoAcesso.Contains("DATAULTIMOACESSO"):
                        AgruparOrdenacao("TDA.DHULTIMO_ACESSO", campoOrdenacao);
                        break;
                    case var dataInstalacaoAplicativo when dataInstalacaoAplicativo.Contains("BLOQUEADO"):
                        AgruparOrdenacao("CASE WHEN TCRI.IDCADASTRO_RESTRITIVO_INSTALACAO IS NOT NULL THEN 1 ELSE 0 END", campoOrdenacao);
                        break;
                    case var dataInstalacaoAplicativo when dataInstalacaoAplicativo.Contains("DATAINSTALACAOAPLICATIVO"):
                        AgruparOrdenacao("TDA.DHINSTALACAO_APLICATIVO", campoOrdenacao);
                        break;
                    case var dataInstalacaoAplicativo when dataInstalacaoAplicativo.Contains("QUANTIDADETENTATIVAS"):
                        AgruparOrdenacao("TDAI.QTTENTATIVA_INSTALACAO", campoOrdenacao);
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(OrdenarPor))
            {
                AgruparOrdenacao("TDA.DHINSTALACAO_TOKEN", "-");
            }
        }

    }

    public class PermissaoMfaQueryStoreConsts
    {

        public static readonly string SQL_BASE_OBTER_TODOS_POR_PERMISSAO = $@"WITH REGISTROS AS (SELECT * FROM (
                                                                                      SELECT TDA.IDDISPOSITIVO_AUTENTICADO,
                                                                                          TCC.IDCOOPERADO_CONTA AS IdPermissao, 
                                                                                          TCC.NRCONTA AS NumeroConta, 
                                                                                          CASE WHEN TPF.NMPESSOA IS NULL THEN TPJ.NMFANTASIA ELSE TPF.NMPESSOA END AS Nome, 
                                                                                          CASE WHEN TPF.NRCPF IS NULL THEN TPJ.NRCNPJ ELSE TPF.NRCPF END AS Documento, 
                                                                                          tc.CDCOOPERATIVA AS IdCooperativa, 
                                                                                          TPC.FLOBRIGATORIO AS Obrigatorio, 
                                                                                          TDA.FLAPP_INSTALADO AS Instalado,
                                                                                          TDA.DHREGISTRO AS DataRegistro,
                                                                                          TDA.DSVERSAO_APP AS VersaoApp,
                                                                                          TDA.CDDISPOSITIVO AS Dispositivo,
                                                                                          TPA.NRPA AS PostoAtendimento,
                                                                                          ROW_NUMBER() OVER (PARTITION BY TPC.IDPERMISSAO_CONTA ORDER BY TDA.DHINSTALACAO_TOKEN DESC) AS rn
                                                                                      FROM 
                                                                                          i.TB_DISPOSITIVO_AUTENTICADO TDA
                                                                                          JOIN i.TB_COOPERADO_CONTA TCC ON TDA.IDCOOPERADO_CONTA = TCC.IDCOOPERADO_CONTA
                                                                                          LEFT JOIN i.TB_PESSOA_FISICA TPF ON TPF.IDPESSOA = TCC.IDPESSOA
                                                                                          LEFT JOIN i.TB_PESSOA_JURIDICA TPJ ON TPJ.IDPESSOA = TCC.IDPESSOA
                                                                                          JOIN i.TB_POSTO_ATENDIMENTO TPA ON TPA.IDPOSTO_ATENDIMENTO = TCC.IDPOSTO_ATENDIMENTO
                                                                                          JOIN i.TB_COOPERATIVA TC ON TC.IDCOOPERATIVA = TPA.IDCOOPERATIVA
                                                                                          JOIN i.TB_PERMISSAO_CONTA TPC ON TPC.IDCONTA = TCC.IDCOOPERADO_CONTA
                                                                                      WHERE 
                                                                                          TPC.DHINATIVACAO IS NULL
                                                                                          AND TCC.DHINATIVACAO IS NULL
                                                                                          AND TDA.TPDISPOSITIVO = 1
                                                                                          AND TCC.NRCONTA = :pr_FiltroConta
                                                                                  )
                                                                                  WHERE rn = 1) ";


        public static readonly string SQL_LISTAR_DISPOSITIVOS_INSTALADOS_ANTERIORMENTE_POR_PERMISSAO = SQL_BASE_OBTER_TODOS_POR_PERMISSAO + $@"SELECT 
                                                                                  TDA.DHINSTALACAO_TOKEN DataInstalacaoToken, 
                                                                                  TDA.DSVERSAO_APP VersaoAplicativo, 
                                                                                  TDA.CDINSTALACAO IdInstalacao,
                                                                                  (SELECT 
                                                                                    CASE WHEN COUNT(TDAE.IDDISPOSITIVO_AUTENTICADO) > 0 THEN 1 ELSE 0 END
                                                                                   FROM i.TB_DISPOSITIVO_AUTENTICADO_EVENTO TDAE
                                                                                   WHERE TDAE.IDDISPOSITIVO_AUTENTICADO = TDA.IDDISPOSITIVO_AUTENTICADO
                                                                                   AND TDAE.IDEVENTO_INSTALACAO = :pr_idsucessoinstalacao) Instalado,
                                                                                  TDA.CDDISPOSITIVO Dispositivo,
                                                                                  TDA.DHULTIMO_ACESSO DataUltimoAcesso,
                                                                                  TDA.DHINSTALACAO_APLICATIVO DataInstalacaoAplicativo,
                                                                                  (SELECT 
                                                                                    COUNT(TDAE.IDDISPOSITIVO_AUTENTICADO)
                                                                                   FROM i.TB_DISPOSITIVO_AUTENTICADO_EVENTO TDAE
                                                                                   WHERE TDAE.IDDISPOSITIVO_AUTENTICADO = TDA.IDDISPOSITIVO_AUTENTICADO
                                                                                   AND TDAE.IDEVENTO_INSTALACAO = :pr_iderrotentativa) QuantidadeTentativas,
                                                                                  CASE WHEN TCRI.IDCADASTRO_RESTRITIVO_INSTALACAO IS NOT NULL THEN 1 ELSE 0 END BLOQUEADO
                                                                                FROM i.TB_DISPOSITIVO_AUTENTICADO TDA
                                                                                JOIN i.TB_COOPERADO_CONTA TCC
                                                                                    ON TDA.IDCOOPERADO_CONTA = TCC.IDCOOPERADO_CONTA
                                                                                JOIN i.TB_PERMISSAO_CONTA TPC
                                                                                    ON TPC.IDCONTA = TCC.IDCOOPERADO_CONTA
                                                                                LEFT JOIN i.TB_DISPOSITIVO_AUTENTICADO tda 
    	                                                                                ON tda.IDCOOPERADO_CONTA = TCC.IDCOOPERADO_CONTA
                                                                                LEFT JOIN i.TB_CADASTRO_RESTRITIVO_INSTALACAO TCRI
                                                                                    ON TCRI.CDINSTALACAO = TDA.CDINSTALACAO
                                                                                    AND TCRI.DHINATIVACAO IS NULL
                                                                                WHERE TCC.IDCOOPERADO_CONTA = HEXTORAW(:pr_codigopermissao)
                                                                                    AND TPC.DHINATIVACAO IS NULL           
                                                                                    AND TCC.DHINATIVACAO IS NULL
                                                                                    AND tda.TPDISPOSITIVO != 2
                                                                                    AND (TDA.IDDISPOSITIVO_AUTENTICADO) NOT IN (
                                                                                        SELECT IDDISPOSITIVO_AUTENTICADO
                                                                                        FROM REGISTROS)";
    }
}
