Normas de projeto
Toda estrutura de backend deve ficar na pasta correspondente. O frontend fica na pasta frontend, a pasta project sera usada para documentar a movimentacao efetuada pelos agentes de IA e a pasta database contera scripts e artefatos do banco de dados.
Sera utilizada .NET 10 para o backend Minimal APIs e o frontend sera gerado com Blazor WASM, preferencialmente seguindo as definicoes do design.md. O banco de dados padrao sera SQLite.
Cada inicio de tarefa deve criar uma nova branch para versionamento e essa mesma branch deve ser utilizada para os commits.
Durante a criacao do projeto, o .gitignore deve ser otimizado junto com um arquivo .agentsignore para reduzir consumo de tokens de IA.
As variaveis de ambiente devem ser otimizadas para troca simples entre dev e prod via flag de ambiente.
Apos toda implementacao, deve ser feita a pergunta sobre preparar o build e o container para subida em producao.