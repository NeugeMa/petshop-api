# PetShop API

API RESTful para gestão de um pet shop, construída com **C# .NET 10**, **ASP.NET Core Minimal APIs** (sem Controllers) e **Entity Framework Core**, com acesso a dados 100% assíncrono (`async`/`await`).

## Integrantes

- Mariana Neugebauer Dourado - RM550494
- Victor Pacifico Dias - RM558017
- Gustavo Ruiz Vieira Paulino - RM554779
- Guilherme Abe - RM554743

## Contexto do projeto

Pequenos pet shops e clínicas veterinárias costumam controlar clientes, animais e serviços em planilhas ou cadernos, o que gera perda de informação e conflitos de horário.

Esta API centraliza esse controle: cadastra **tutores**, os **pets** de cada tutor e os **agendamentos** de serviços (banho, tosa, consulta, vacina), aplicando regras de negócio como impedir dois serviços no mesmo horário para o mesmo pet. Destina-se a donos e atendentes de pet shops, e pode servir de back-end para um sistema web ou aplicativo.

## Banco de dados

**SQLite** (arquivo `petshop.db`, criado automaticamente na pasta `PetShopApi`).

## Tecnologias

- .NET 10 / ASP.NET Core Minimal APIs
- Entity Framework Core 10 (SQLite) com Migrations
- OpenAPI nativo + [Scalar](https://scalar.com) como interface de documentação/testes

## Como rodar localmente

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) e a ferramenta `dotnet-ef`.

```bash
dotnet tool install --global dotnet-ef
git clone https://github.com/NeugeMa/petshop-api.git
cd petshop-api/PetShopApi
dotnet ef database update
dotnet run
```

- `dotnet ef database update` aplica as migrations e cria o `petshop.db`. (A API também aplica migrations pendentes ao iniciar, então rodar `dotnet run` direto também funciona.)
- Abra a URL exibida no console (ex.: `http://localhost:5xxx`). A raiz `/` redireciona para a documentação interativa em **`/scalar`**. O documento OpenAPI fica em `/openapi/v1.json`.

## Modelo de dados

```
Tutor 1 ──── N Pet 1 ──── N Agendamento
```

| Entidade | Campos |
|---|---|
| Tutor | Id, Nome, Email (único), Telefone |
| Pet | Id, Nome, Especie, Raca?, DataNascimento?, TutorId |
| Agendamento | Id, PetId, DataHora, Servico (`Banho`, `Tosa`, `BanhoETosa`, `Consulta`, `Vacina`), Valor, Status (`Agendado`, `Concluido`, `Cancelado`) |

## Endpoints

### Tutores
| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/tutores` | Lista tutores | 200 |
| GET | `/tutores/{id}` | Busca por id | 200, 404 |
| GET | `/tutores/{id}/pets` | Lista os pets do tutor | 200, 404 |
| POST | `/tutores` | Cadastra tutor | 201, 400, 409 (e-mail duplicado) |
| PUT | `/tutores/{id}` | Atualiza tutor | 200, 400, 404, 409 |
| DELETE | `/tutores/{id}` | Remove tutor | 204, 404, 409 (possui pets) |

### Pets
| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/pets` | Lista pets | 200 |
| GET | `/pets/{id}` | Busca por id | 200, 404 |
| GET | `/pets/{id}/agendamentos` | Lista agendamentos do pet | 200, 404 |
| POST | `/pets` | Cadastra pet | 201, 400, 404 (tutor inexistente) |
| PUT | `/pets/{id}` | Atualiza pet | 200, 400, 404 |
| DELETE | `/pets/{id}` | Remove pet | 204, 404, 409 (possui agendamentos) |

### Agendamentos
| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/agendamentos` | Lista agendamentos | 200 |
| GET | `/agendamentos/{id}` | Busca por id | 200, 404 |
| POST | `/agendamentos` | Agenda um serviço | 201, 400, 404 (pet inexistente), 409 (horário ocupado) |
| PUT | `/agendamentos/{id}` | Atualiza (inclusive o status) | 200, 400, 404, 409 |
| DELETE | `/agendamentos/{id}` | Remove agendamento | 204, 404 |

### Exemplos de corpo (JSON)

```json
// POST /tutores
{ "nome": "Ana Souza", "email": "ana@email.com", "telefone": "11999999999" }

// POST /pets
{ "nome": "Rex", "especie": "Cachorro", "raca": "Labrador", "dataNascimento": "2020-05-01", "tutorId": 1 }

// POST /agendamentos
{ "petId": 1, "dataHora": "2030-01-01T10:00:00", "servico": "Banho", "valor": 50.00 }
```

## Regras de negócio e tratamento de erros

- Campos obrigatórios, e-mail válido e limites de tamanho/valor → **400** com detalhes por campo (`ValidationProblem`).
- Não cadastrar pet para tutor inexistente, nem agendar para pet inexistente → **404**.
- E-mail de tutor único → **409**.
- Não agendar no passado → **400**.
- Um pet não pode ter dois agendamentos ativos no mesmo horário → **409**.
- Não excluir tutor com pets, nem pet com agendamentos → **409**.
- Erros não tratados retornam `ProblemDetails` padronizado (500).

## Evidências de teste

Prints de cada endpoint funcionando (Scalar) estão em [`docs/evidencias`](docs/evidencias).

_(adicionar os prints e listar aqui)_
