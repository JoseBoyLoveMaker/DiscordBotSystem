# 🚀 Discord Bot System

Sistema completo de bot para Discord com painel web e API, focado em gerenciamento de servidores (guilds), automações e experiência do usuário.

## 🎬 Demonstração

[![Assista ao vídeo] (https://www.youtube.com/watch?v=VoGqOUcDinc)

---

## 📌 Sobre o projeto

Este projeto é um sistema full-stack composto por:

- 🤖 Bot para Discord
- 🌐 API em ASP.NET
- 🖥️ Painel Web para gerenciamento
- 🗄️ Banco de dados MongoDB

O objetivo é permitir que administradores de servidores configurem funcionalidades como XP, níveis, mensagens automáticas, cargos e respostas personalizadas de forma simples e centralizada.

---

## 🧠 Funcionalidades

### 🎮 Sistema de XP e níveis
- XP por mensagens e atividade em call
- Níveis separados por servidor
- Sistema de ranking (top usuários)

### 👋 Moderação e automações
- Mensagens de entrada e saída
- Cargo automático ao entrar
- Cargos por nível (chat e voz)

### 💬 Sistema de respostas automáticas
- Triggers personalizadas
- Respostas dinâmicas
- Configuração por servidor

### ⚙️ Configuração por guild (multi-servidor)
- Cada servidor possui suas próprias configurações
- Isolamento total de dados

### 🌐 Painel Web
- Interface para gerenciar:
  - Mensagens de entrada/saída
  - Cargos
  - Respostas
- Integração com API

---

## 🏗️ Arquitetura

O projeto está dividido em múltiplos módulos:
DiscordBotSystem/
│
├── Bot/ # Bot do Discord (eventos, comandos)
├── API/ # Backend ASP.NET
├── PainelWeb/ # Frontend do painel
├── docs/ # Documentação e assets


### 🔧 Backend (API)
- ASP.NET Core
- MongoDB
- Estrutura em camadas:
  - Controllers
  - Services
  - DTOs

### 🤖 Bot
- Discord.Net
- Eventos:
  - UserJoined
  - MessageReceived
  - VoiceStateUpdated

### 🌐 Frontend
- HTML, CSS, JavaScript
- Comunicação com API via REST

---

## 🛠️ Tecnologias utilizadas

- C#
- ASP.NET Core
- MongoDB
- JavaScript
- HTML / CSS
- Discord.Net

---

## ⚙️ Como rodar o projeto

### 📌 Pré-requisitos
- .NET 7+
- MongoDB (local ou Atlas)
- Node.js (opcional, se usar build frontend)
- Conta no Discord 

---

### 🔐 Configuração

1. Clone o repositório:
```bash
git clone https://github.com/seu-usuario/DiscordBotSystem.git

Configure o arquivo appsettings.json:
{
  "MongoSettings": {
    "ConnectionString": "SUA_STRING",
    "DatabaseName": "NOME_DO_BANCO"
  },
  "Discord": {
    "Token": "SEU_TOKEN"
  }
}

cd API
dotnet run

cd Bot
dotnet run

abrir index.html

Desenvolvido por Emanuel Bernardo
