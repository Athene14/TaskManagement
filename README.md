
# Task Management API

Эндпоинты потыкать можно в Swagger и в репозитории лежит json коллекция для Postman


## Аутентификация

Для всех защищенных эндпоинтов требуется JWT-токен в заголовке:

```
Authorization: Bearer <your_token>
```

---

## Аутентификация (Auth)

### Регистрация пользователя

**POST** `/api/auth/register`

Модель запроса
```json
{
  "fullName": "string",
  "email": "string@mail.ru",
  "password": "string"
}
```

Пример запроса
```
curl -X 'POST' \
  'http://localhost:5050/api/auth/register' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "fullName": "string",
  "email": "string@mail.ru",
  "password": "string"
}'
```

Модель ответа

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "string"
}
```
---

### Авторизация

**POST** `/api/auth/login`

Модель запроса

```json
{
  "email": "string@mail.ru",
  "password": "string"
}
```

Пример запроса

```
curl -X 'POST' \
  'http://localhost:5050/api/auth/login' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "string@mail.ru",
  "password": "string"
}'
```

Модель ответа

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "string"
}
```

---

### Получение информации о пользователе

**GET** `/api/auth/me`

Пример запроса

```
curl -X GET "http://localhost:5050/api/auth/me" \
  -H "Authorization: Bearer <token>"
```

Модель ответа

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "string",
  "fullName": "string"
}
```

---

## Уведомления (Notifications)

### Получение уведомлений

**GET** `/api/notifications/{userId}`

Можно указать query параметр unreadOnly - получить только непрочитанные уведомления

Пример запроса

```
curl -X GET "http://localhost:5050/api/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6?unreadOnly=true" \
  -H "Authorization: Bearer <token>"
```

Модель ответа

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "created": "2025-08-06T22:38:50.177Z",
    "message": "string",
    "isRead": true
  }
]
```

---

### Отметить как прочитанное

**PUT** `/api/notifications/{notificationId}/mark-as-read`

Пример запроса

```
curl -X PUT "http://localhost:5050/api/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6/mark-as-read" \
  -H "Authorization: Bearer <token>"
```

---

### Создание уведомления

**POST** `/api/notifications`

Отправить уведомление пользователям (пользователю)

Модель запроса

```json
{
  "message": "string",
  "recipientIds": [
    "uuid1","uuid2"
  ]
}
```

Пример запроса

```bash
curl -X POST "http://localhost:5050/api/notifications" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"message":"Ваша задача была обновлена","recipientIds":["uuid1","uuid2"]}'
```

Модель ответа

```json
{
  "createdNotificationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## Задачи (Tasks)

### Создание задачи

**POST** `/api/tasks`

Модель запроса

```
{
  "title": "string",
  "description": "string",
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Пример запроса

```
curl -X POST "http://localhost:5050/api/tasks" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fix authentication bug",
    "description": "JWT tokens expire too quickly",
    "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

Модель ответа

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "string",
  "description": "string",
  "createdAt": "2025-08-06T22:43:08.878Z",
  "updatedAt": "2025-08-06T22:43:08.878Z",
  "isActive": true,
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### Поиск задач

**GET** `/api/tasks`

Фильтры для поиска. Передаются в query.

| Параметр             | Тип     | Описание                             |
|----------------------|---------|--------------------------------------|
| Title                | string  | Фильтр по заголовку                  |
| AssignedUserId       | UUID    | Назначенный пользователь             |
| CreatedFromTimestamp | int     | Дата создания (от, UnixTimestamp)    |
| CreatedToTimestamp   | int     | Дата создания (до, UnixTimestamp)    |
| OnlyActive           | boolean | Только активные                      |
| CreatedBy            | UUID    | ID создателя                         |
| page                 | int     | Номер страницы (по умолчанию: 1)     |
| pageSize             | int     | Размер страницы (по умолчанию: 10)   |

Пример запроса

```
curl -X GET "http://localhost:5050/api/tasks?Title=API&OnlyActive=true&page=1&pageSize=10" \
  -H "Authorization: Bearer <token>"
```

Модель ответа

```json
{
  "page": 0,
  "pageSize": 0,
  "totalCount": 0,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "string",
      "description": "string",
      "createdAt": "2025-08-06T22:41:34.654Z",
      "updatedAt": "2025-08-06T22:41:34.654Z",
      "isActive": true,
      "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ]
}
```

---

### История задачи

**GET** `/api/tasks/{taskId}/history`

Получить историю изменения задачи

Пример запроса

```
curl -X 'GET' \
  'http://localhost:5050/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6/history' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <token>'
```

Модель ответа

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "string",
    "description": "string",
    "createdAt": "2025-08-06T22:43:29.932Z",
    "updatedAt": "2025-08-06T22:43:29.932Z",
    "isActive": true,
    "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
]
```

---

### Получить задачу

**GET** `/api/tasks/{taskId}`


Пример запроса

```
curl -X 'GET' \
  'http://localhost:5050/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <token>'
```

Модель ответа

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "string",
  "description": "string",
  "createdAt": "2025-08-06T22:43:29.935Z",
  "updatedAt": "2025-08-06T22:43:29.935Z",
  "isActive": true,
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### Изменить задачу

**PUT** `/api/tasks/{taskId}`

Изменить задачу 

Модель запроса

```json
{
  "title": "string",
  "description": "string",
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Поля не являются обязательными. Должно быть заполнено хотя-бы одно

Пример запроса

```
curl -X 'PUT' \
  'http://localhost:5050/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "title": "string",
  "description": "string",
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}'
```

Модель ответа

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "string",
  "description": "string",
  "createdAt": "2025-08-06T22:43:29.937Z",
  "updatedAt": "2025-08-06T22:43:29.937Z",
  "isActive": true,
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### Мягкое удаление задачи

**DELETE** `/api/tasks/{taskId}`


Пример запроса

```
curl -X 'DELETE' \
  'http://localhost:5050/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'
```
  
---

### Назначение задачи

**PUT** `/api/tasks/{taskId}/assign`

Назначить задачу на пользователя

Модель запроса

```json
{
  "assignedUserId": "7c71bc3e-4763-7f9a-37c3-df93f93aa8f8"
}
```

Пример запроса

```
curl -X 'PUT' \
  'http://localhost:5050/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6/assign' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}'
```

Модель ответа

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "string",
  "description": "string",
  "createdAt": "2025-08-06T22:43:29.941Z",
  "updatedAt": "2025-08-06T22:43:29.941Z",
  "isActive": true,
  "assignedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
---

## Health Checks

### Запрос состояния сервисов

**Get** `/health-check`

```
curl -X GET http://localhost:5050/health-check
```

### UI интерфейс

Открыть в браузере:  
`http://localhost:5050/health-check-ui`

---

## SignalR Уведомления

Хаб для  realtime-уведомлений.
Для подключения к хабу в заголовке так же должен быть указан Authorization.
При подключении к хабу пользователь сможет получать уведомления через метод OnNotificationCreated, которые адресованы ему и отправлять уведомления используя метод SendNotification 

Пример подключения на Java Script
```JS
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5050/notificationHub", {
        accessTokenFactory: () => {
            return localStorage.getItem('token'); // JWT токен должен быть в хранилище
        }
    })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.onclose(() => console.log("Disconnected"));
connection.onreconnecting(() => console.log("Reconnecting..."));

async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("Connection failed:", err);
        setTimeout(startConnection, 5000);
    }
}

startConnection();
```

Модель для OnNotificationCreated
```
{
  "notificationId":"9a619956-f493-49ac-9684-5db074d928a9",
  "message":"sting"
}
```

Пример получения уведомления на JS 

```JS
connection.on("OnNotificationCreated", (notification) => {
    console.log("Received notification:", notification);
});
```

Для отправки нужно вызвать метод SendNotification и передать message и recipientIds

Пример отправки на JS

```JS
connection.invoke("SendNotification", message, recipientIds);
```

Сообщение

```json
{
  "NotificationId": uuid,
  "Message": string
}
```

## Подключение к SignalR из Postman

Подключиться к SignalR из Postman напрямую нельзя, но можно эмулировать negotiation-запрос и использовать полученный `connectionToken` в WebSocket-клиенте.

1. Получите `connectionToken` из ответа Negotiation-запроса

```
curl -X POST "http://localhost:5050/notificationHub/negotiate?negotiateVersion=1" \
     -H 'Authorization: Bearer <token>'
```


2. Откройте WebSocket-соединение:

```
ws://localhost:5050/notificationHub?id=<connectionToken>
```
_(Для подключения к хабу в заголовке так же должен быть указан Authorization)_

3. Отправьте инициализирующее сообщение:

```json
{"protocol":"json","version":1}
```

_Символ `` (U+001E) обязателен в конце сообщения._

Если подключение прошло успешно, то хаб отправит
`{}`

Пример полученного уведомления

```
{
  "type": 1,
  "target": "OnNotificationCreated",
  "arguments": [
    {"notificationId":"9a619956-f493-49ac-9684-5db074d928a9","message":"Test message from Postman"}
  ]
}
```

Пример отправки уведомления

```
{
  "arguments": [
    "Test message from Postman",
    ["85253bc6-f39f-42d7-b748-bbcaddf778a3"]
  ],
  "target": "SendNotification",
  "type": 1
}
```


---

**Источник по подключению через WebSocket:**  
https://stackoverflow.com/questions/56474386/is-it-possible-to-call-a-signalr-hub-from-postman
