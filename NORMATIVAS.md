# Normativas de Trabalho do Projeto

## 1. Objetivo
Este documento define as normas técnicas e operacionais do projeto para garantir consistência de implementação, colaboração e entrega.

## 2. Escopo Oficial
- Escopo atual: Nível 3 (Fullstack + IA).
- Solução alvo:
  - Backend: .NET 10 com Minimal APIs.
  - Frontend: Blazor WebAssembly.
  - IA: integração local via porta/endpoint localhost.
  - Banco de dados: SQLite.

Fora do escopo neste ciclo:
- Fallback automático para provedores cloud de IA.
- Fluxos Git avançados (ex.: Gitflow completo).

## 3. Stack Tecnológica
- .NET SDK 10 (backend).
- ASP.NET Core Minimal APIs.
- Blazor WebAssembly (frontend).
- SQLite (persistência).
- Docker/containers para empacotamento de produção.

## 4. Estrutura de Pastas
Estrutura obrigatória para organização por responsabilidade:

- api: exposição de endpoints HTTP.
- ai: integração, contratos e orquestração de IA.
- core: domínio e regras de negócio.
- infra: acesso a dados, integrações externas e implementações técnicas.
- application: casos de uso e fluxo de aplicação.
- frontend: aplicação Blazor WebAssembly.
- database: scripts, migrações e seed de banco.
- project: documentação de movimentações e decisões dos agentes de IA.

## 5. Normas para IA Local (Porta Local)
Padrão oficial de integração:

- Provedor local: LM Studio (OpenAI-compatible).
- Endpoint base padrão: http://localhost:1234
- Compatibilidade esperada: API estilo OpenAI Chat Completions.

Regras de execução:
- Todo serviço que consome IA deve validar disponibilidade do endpoint antes de processar requisições críticas.
- Definir timeout de chamada de IA em configuração por ambiente.
- Em indisponibilidade da IA local: retornar erro amigável e orientativo ao usuário.
- Não realizar fallback para cloud neste ciclo.

## 6. Variáveis de Ambiente
Todo comportamento sensível a ambiente deve ser parametrizado.

Variáveis mínimas obrigatórias:
- ASPNETCORE_ENVIRONMENT
- ConnectionStrings__Default
- AI__Provider=LMStudio
- AI__BaseUrl=http://localhost:1234
- AI__Model=nome-do-modelo-configurado
- AI__TimeoutSeconds=30
- AI__RetryCount=1

Regras:
- Nunca versionar segredos reais.
- Manter arquivos de exemplo de configuração para dev e prod.
- Permitir troca simples entre dev/prod via flag de ambiente.

## 7. Governança Git e Pull Request
- Cada tarefa deve iniciar em uma nova branch.
- Uma branch deve representar uma unidade de trabalho rastreável.
- Merge em main somente via Pull Request.
- Pull Request deve conter descrição clara, impacto técnico e checklist de validação.

Convenção recomendada de branches:
- feature/nome-curto-da-tarefa
- fix/nome-curto-do-ajuste
- docs/nome-curto-da-documentacao
- chore/nome-curto-de-manutencao

## 8. Política de Ignorados (.gitignore e contexto de IA)
- Otimizar .gitignore para remover artefatos de build, cache e arquivos temporários.
- Manter arquivo de exclusão de contexto para agentes de IA (.agentsignore) para reduzir ruído e consumo de tokens.
- Excluir do contexto de IA conteúdos gerados, binários e diretórios irrelevantes para entendimento do código.

## 9. Qualidade e Revisão
Antes de abrir PR:
- Compilar solução sem erros.
- Validar cenário principal da funcionalidade alterada.
- Revisar impacto em configuração de ambiente.
- Atualizar documentação quando houver mudança de comportamento.

## 10. Build e Containerização
Após cada implementação concluída, deve ser feita a pergunta explícita:

"Deseja preparar o build e o container para subir em produção?"

Se a resposta for positiva:
- Gerar build de produção.
- Gerar imagem de container.
- Validar configuração de ambiente de produção.

## 11. Script Operacional Local
Para execução local rápida, o projeto deve manter um script versionado para subir API e Frontend.

Regras:
- Script oficial local: scripts/dev-up.ps1
- Sempre que uma mudança impactar execução local (porta, projeto de inicialização, variáveis de ambiente, provedor de IA ou dependências de run), o script deve ser atualizado no mesmo PR.
- Toda alteração no script deve refletir também na documentação de uso (README).

## 12. Critérios de Atualização deste Documento
Este documento é a referência oficial de normativas do projeto.

Atualizações devem:
- Ser feitas por Pull Request.
- Informar claramente o motivo da mudança.
- Preservar consistência com README e demais documentos de base.
