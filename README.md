# 🚀 Discord Bot System

Sistema full-stack completo com Bot Discord + API + Painel Web

## 🎬 Demonstração

[![Watch the video](https://img.youtube.com/vi/VoGqOUcDinc/0.jpg)](https://www.youtube.com/watch?v=VoGqOUcDinc)

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

### 💡 Diferenciais

- Sistema multi-servidor (guild-based)
- Arquitetura modular (Bot + API + Painel)
- Integração completa com MongoDB
- Painel web para configuração em tempo real

## 🧠 Desafios técnicos resolvidos

- Sistema de XP com cálculo por atividade
- Sincronização de dados por servidor
- Persistência com MongoDB
- Estrutura escalável para múltiplas guilds

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
```
---
## 🌎 English version

# 🚀 Discord Bot System

Full-stack system with Discord Bot + API + Web Dashboard

---

## 🎬 Demo

[![Watch the video](https://img.youtube.com/vi/VoGqOUcDinc/0.jpg)](https://www.youtube.com/watch?v=VoGqOUcDinc)

---

## 📌 About the project

This project is a full-stack system composed of:

- 🤖 Discord Bot  
- 🌐 ASP.NET API  
- 🖥️ Web Dashboard  
- 🗄️ MongoDB Database  

The goal is to allow server administrators to configure features such as XP, levels, automated messages, roles, and custom responses in a simple and centralized way.

---

## 🧠 Features

### 🎮 XP and leveling system
- XP from messages and voice activity  
- Levels separated per server  
- Ranking system (top users)  

### 👋 Moderation and automation
- Welcome and leave messages  
- Auto role on join  
- Level-based roles (chat and voice)  

### 💬 Custom response system
- Custom triggers  
- Dynamic responses  
- Per-server configuration  

### ⚙️ Guild-based configuration (multi-server)
- Each server has its own configuration  
- Full data isolation  

### 🌐 Web Dashboard
- Interface to manage:
  - Welcome/leave messages  
  - Roles  
  - Responses  
- API integration  

---

## 💡 Highlights

- Multi-server system (guild-based)  
- Modular architecture (Bot + API + Dashboard)  
- Full MongoDB integration  
- Real-time configuration via web panel  

---

## 🧠 Technical challenges solved

- XP system with activity-based calculation  
- Per-server data synchronization  
- MongoDB data persistence  
- Scalable structure for multiple guilds  

---

## 🏗️ Architecture

The project is divided into multiple modules:


DiscordBotSystem/
│
├── Bot/ # Discord bot (events, commands)
├── API/ # ASP.NET backend
├── PainelWeb/ # Frontend dashboard
├── docs/ # Documentation and assets


### 🔧 Backend (API)
- ASP.NET Core  
- MongoDB  
- Layered structure:
  - Controllers  
  - Services  
  - DTOs  

### 🤖 Bot
- Discord.Net  
- Events:
  - UserJoined  
  - MessageReceived  
  - VoiceStateUpdated  

### 🌐 Frontend
- HTML, CSS, JavaScript  
- REST API communication  

---

## 🛠️ Technologies

- C#  
- ASP.NET Core  
- MongoDB  
- JavaScript  
- HTML / CSS  
- Discord.Net  

---

## ⚙️ Running the project

### 📌 Requirements
- .NET 7+  
- MongoDB (local or Atlas)  
- Node.js (optional, for frontend build)  
- Discord account  

---

### 🔐 Setup

1. Clone the repository:
```bash
git clone https://github.com/your-username/DiscordBotSystem.git
Configure appsettings.json:
{
  "MongoSettings": {
    "ConnectionString": "YOUR_CONNECTION_STRING",
    "DatabaseName": "DATABASE_NAME"
  },
  "Discord": {
    "Token": "YOUR_TOKEN"
  }
}
▶️ Run API
cd API
dotnet run
▶️ Run Bot
cd Bot
dotnet run
▶️ Run Dashboard

Open index.html or use a local server.

Developed by Emanuel Bernardo
```
