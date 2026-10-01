# BerClock

> **Um gerenciador de tempo moderno para Windows.**

O **BerClock** é uma aplicação desktop para Windows que reúne **alarmes, temporizador, cronômetro e relógios mundiais** em uma única ferramenta, com uma interface inspirada em elementos antigos, relíquias e artefatos místicos.

O projeto combina funcionalidades práticas para gerenciamento de horários com uma identidade visual própria, baseada em tons escuros, dourado e roxo.

## 🎯 Motivação

O BerClock nasceu de uma necessidade real dentro do ambiente escolar onde o projeto foi desenvolvido.

A instituição já utilizava um programa de alarme para auxiliar na organização dos horários e da rotina do colégio. Com o uso diário, surgiu a necessidade de uma alternativa mais moderna, personalizável e adequada às necessidades da instituição.

A partir dessa necessidade, o BerClock foi desenvolvido inicialmente como uma alternativa ao sistema de alarme utilizado no colégio, buscando oferecer uma experiência mais organizada e, ao mesmo tempo, incorporar recursos que não estavam disponíveis na ferramenta anterior.

O projeto posteriormente evoluiu para uma aplicação mais completa, incorporando **temporizador, cronômetro, relógio mundial, gerenciamento de grupos de alarmes, reprodução de áudio e uma janela dedicada para a execução dos alarmes**.

Embora tenha surgido para atender uma necessidade específica, o BerClock foi desenvolvido de forma que possa ser utilizado em diferentes ambientes que necessitem de uma ferramenta de controle de horários no Windows.

## ✨ Recursos

### ⏰ Alarmes

- Criação de alarmes personalizados
- Nome e identificação dos alarmes
- Organização por grupos
- Agendamento por dias da semana
- Ativação e desativação individual
- Reprodução de sons personalizados
- Sons integrados do sistema
- Duração configurável
- Fade-in e fade-out do áudio
- Janela dedicada para execução do alarme
- Contagem regressiva durante a execução

### ⌛ Temporizador

- Definição de horas, minutos e segundos
- Iniciar, pausar e reiniciar
- Exibição do tempo restante

### ⏱️ Cronômetro

- Iniciar e pausar
- Reiniciar
- Exibição com precisão de milissegundos

### 🌎 Relógio Mundial

Exibição simultânea dos horários de diferentes localidades:

- 🇧🇷 Rio de Janeiro
- 🇺🇸 Nova York
- 🇬🇧 Londres
- 🇯🇵 Tóquio

### 🖥️ Área de notificação do Windows

O BerClock pode permanecer em execução na **bandeja do sistema**, permitindo que a aplicação continue funcionando sem ocupar espaço na área de trabalho.

### 🎨 Interface personalizada

A interface foi desenvolvida com uma identidade visual própria, utilizando:

- Tema escuro
- Elementos dourados inspirados em relíquias
- Detalhes em tons de roxo
- Elementos visuais inspirados em artefatos antigos
- Interface dedicada para execução dos alarmes

### 🔔 Janela de alarme

A execução dos alarmes possui uma interface dedicada com:

- Exibição do nome do alarme
- Relógio em tempo real
- Contagem regressiva
- Reprodução de áudio
- Fade-out configurável
- Elementos visuais e animações

## 🛠️ Tecnologias

O BerClock foi desenvolvido utilizando:

- **C#**
- **.NET 10**
- **Windows Forms**
- **WPF** — utilizado na interface dedicada dos alarmes
- **NAudio** — reprodução e controle de áudio
- **JSON** — armazenamento dos dados da aplicação

## 📁 Estrutura do projeto

```text
BerClock/
├── Win7AlarmClassic/
│   ├── Models/
│   ├── Services/
│   ├── UI/
│   ├── Assets/
│   ├── Program.cs
│   └── *.csproj
├── docs/
│   ├── screenshot.png
│   └── demo.gif
├── .editorconfig
├── .gitignore
└── README.md
```

> O projeto mantém internamente o namespace `Win7AlarmClassic` por questões de compatibilidade e estrutura do código, enquanto **BerClock** é o nome público da aplicação.

## 🚀 Executando localmente

### Requisitos

- Windows
- .NET 10 SDK
- Visual Studio 2026, JetBrains Rider ou outra IDE compatível com .NET

### Clonando o projeto

```bash
git clone https://github.com/SEU-USUARIO/BerClock.git
cd BerClock
```

### Compilando

```bash
dotnet restore
dotnet build
```

### Executando

```bash
dotnet run --project Win7AlarmClassic
```

## 📦 Gerando uma versão para Windows

Para gerar uma versão **self-contained para Windows x64**:

```bash
dotnet publish Win7AlarmClassic -c Release -r win-x64 --self-contained true
```

Os arquivos publicados poderão ser encontrados na pasta de saída do processo de publicação e podem ser utilizados para distribuição da aplicação.

## 🎨 Identidade visual

O BerClock foi concebido com uma proposta diferente das interfaces tradicionais de relógios e alarmes.

Em vez de seguir exclusivamente o padrão de interfaces modernas e minimalistas, o projeto utiliza uma estética inspirada em **relíquias, artefatos antigos e elementos místicos**, criando uma identidade visual própria para a aplicação.

### Paleta principal

| Elemento | Cor |
|---|---|
| Fundo | `#0C0A10` |
| Dourado | `#E8BE48` |
| Dourado claro | `#FFDA69` |

A proposta é transmitir uma sensação de objeto antigo e elaborado, combinando essa estética com uma aplicação desktop funcional e moderna.

## 🖼️ Screenshots

As imagens da aplicação ficam disponíveis na pasta `docs/`.

```md
![BerClock](docs/screenshot.png)
```

Uma demonstração animada também pode ser adicionada:

```md
![BerClock Demo](docs/demo.gif)
```

## 🗺️ Roadmap

Algumas melhorias planejadas para versões futuras:

- [ ] Novos temas visuais para os alarmes
- [ ] Mais localidades no relógio mundial
- [ ] Melhorias nas animações da janela de alarme
- [ ] Mais opções de controle de áudio
- [ ] Versão portátil
- [ ] Instalador para Windows
- [ ] Sistema de atualização automática
- [ ] Mais opções de personalização
- [ ] Melhorias de acessibilidade

## 🤝 Desenvolvimento

O BerClock é um projeto desenvolvido de forma independente a partir de uma necessidade prática identificada em um ambiente escolar.

A aplicação evoluiu gradualmente de uma ferramenta voltada especificamente para alarmes para uma solução mais completa de gerenciamento de tempo para Windows.

## 📄 Licença

A licença do projeto deverá ser definida antes da publicação pública do repositório.

Caso o projeto utilize código, bibliotecas, imagens, sons ou outros componentes derivados de terceiros, suas respectivas licenças e requisitos de atribuição devem ser verificados antes da distribuição.

---

<p align="center">

**BerClock**

*Time, forged in gold.*

</p>
