# soat-vehicle-sales

Serviço de vendas da plataforma de revenda de veículos. Este repositório será responsável pelas listagens, compras, pagamentos e webhook, utilizando um banco de dados segregado e comunicação HTTP com o serviço principal.

## Documentação

- [docs/PROJECT-DEFINITION.md](../soat-vehicle-management/docs/PROJECT-DEFINITION.md) — Decisões de escopo, domínio, tecnologia e comunicação.
- [docs/REQUIREMENTS.md](../soat-vehicle-management/docs/REQUIREMENTS.md) — Requisitos funcionais (RF), não-funcionais (RNF), estados, regras de negócio e critérios de aceite.
- [docs/ARCHITECTURE.md](../soat-vehicle-management/docs/ARCHITECTURE.md) — Diagramas de componentes, fluxos de interação, topologia de mensageria e tratamento de falhas.

**Nota**: Os arquivos de documentação estão centralizados no repositório `soat-vehicle-management` para facilitar referência cruzada e manutenção.

## Docker Compose completo

Este repositório contém o Compose usado para a demonstração ponta a ponta:

- MySQL do gerenciamento;
- migrations do gerenciamento;
- API de gerenciamento;
- MySQL de vendas;
- migrations de vendas;
- API de vendas;
- gateway de pagamento publicado no Docker Hub.

```bash
cp .env.example .env
docker compose up --build
```

O gateway utilizado é `igortessaro/fiap-soat-payment-gateway:latest`.

Ao criar uma venda, a API salva a venda reservada e chama automaticamente o gateway em
`POST http://payment-gateway:8080/api/v1/payments`. O gateway então envia o resultado
(`PAID` ou `CANCELED`) para o webhook da API de vendas. Para execução local fora do
Compose, cadastre o gateway pela API:

```bash
curl -X POST http://localhost:5002/api/v1/payment-gateways \
  -H 'Content-Type: application/json' \
  -d '{"name":"Local payment gateway","baseUrl":"http://localhost:5100"}'
```

Somente um gateway pode ser cadastrado. Para alterar a URL do gateway registrado,
consulte `GET /api/v1/payment-gateways` e use `PUT /api/v1/payment-gateways/{id}`.