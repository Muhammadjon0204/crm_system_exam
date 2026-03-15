# CRM System

Учебный проект — backend API для управления студентами, группами и успеваемостью.

## Технологии
- ASP.NET Core Web API
- PostgreSQL + Dapper
- Serilog (логи в файл)
- Кастомный Middleware (глобальные исключения, логирование запросов)

## Структура
- Domain — модели и DTO
- Infrastructure — сервисы, интерфейсы, работа с БД
- WebApi — контроллеры, middleware, точка входа

## Запуск
1. Настроить строку подключения в appsettings.json
2. dotnet run