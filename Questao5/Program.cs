using MediatR;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Handlers;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Queries.Responses;
using Questao5.Domain.Interfaces.Repository;
using Questao5.Infrastructure.Database.Repository;
using Questao5.Infrastructure.Sqlite;
using FluentValidation;
using System.Reflection;
using Questao5.Application.Commands.Validadors;
using Questao5.Application.Validadors;
using Questao5.Configurations;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new GlobalExceptionFilter());
});

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// sqlite
builder.Services.AddSingleton(new DatabaseConfig { Name = builder.Configuration.GetValue<string>("DatabaseName", "Data Source=database.sqlite") });
builder.Services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();

builder.Services.AddScoped<IRequestHandler<CriaMovimentacaoParaContaCommand, MovimentacaoResponse>, MovimentacaoHandler>();
builder.Services.AddScoped<IRequestHandler<GetContaCorrenteByNumeroQuery, SaldoContaCorrenteResponse>, SaldoContaCorrenteQueryHandler>();

builder.Services.AddScoped<IValidator<CriaMovimentacaoParaContaCommand>, CriaMovimentacaoParaContaCommandValidator>();
builder.Services.AddScoped<IValidator<GetContaCorrenteByNumeroQuery>, GetContaCorrenteByNumeroQueryValidator>();

builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

builder.Services.Configure<RequestLocalizationOptions>(
    options => {
        var supportedCultures = new List<CultureInfo>{new CultureInfo("pt-BR"),new CultureInfo("en-US")
            };
        options.DefaultRequestCulture = new RequestCulture(culture: "pt-BR", uiCulture: "pt-BR");
        options.SupportedCultures = supportedCultures; options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    });

builder.Services.AddLocalization(options => options.ResourcesPath = "Domain/Language");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    ApplyCurrentCultureToResponseHeaders = true
});

app.UseMiddleware<AcceptLanguageMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// sqlite
#pragma warning disable CS8602 // Dereference of a possibly null reference.
app.Services.GetService<IDatabaseBootstrap>().Setup();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

app.Run();

// Informações úteis:
// Tipos do Sqlite - https://www.sqlite.org/datatype3.html


