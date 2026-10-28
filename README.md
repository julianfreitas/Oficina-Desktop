# Oficina — Sistema de gestão (C# / Windows Forms / SQLite)

Aplicação desktop 100% offline. O banco é um arquivo SQLite criado automaticamente em:
`%LocalAppData%\OficinaApp\oficina.db` (para backup, basta copiar esse arquivo).

## Como executar
1. Instale o **.NET 8 SDK** (https://dotnet.microsoft.com/download) no Windows.
2. Na pasta do projeto: `dotnet run`
   (ou abra `OficinaApp.csproj` no Visual Studio 2022 e pressione F5).

## Gerar um .exe para instalar na oficina (sem precisar do .NET)
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
O executável fica em `bin\Release\net8.0-windows\win-x64\publish\`.

## Telas
- **Home** – resumo do mês e atalhos de uso
- **Serviços e valores** – cadastro de serviços (valor cobrado e custo estimado)
- **Clientes** – nome, CPF (validado), marca/modelo do carro e e-mail; busca por nome/CPF/carro
- **Serviços realizados** – registra cada serviço feito (valor e custo editáveis)
- **Despesas** – gastos gerais (aluguel, energia, ferramentas...)
- **Dashboard** – por mês: faturamento, custos, despesas, lucro final, margem, tabela dos serviços e gráfico dos últimos 6 meses

Lucro final = Faturamento − Custos dos serviços − Despesas gerais.
