# 🧪 Practical Exam – Academy CRM System

### Technology: **.NET + Dapper + PostgreSQL**

---

# 📌 Задание

Необходимо разработать **REST API систему CRM для академии**, которая будет управлять:

* студентами
* группами
* расписанием занятий
* посещаемостью
* оценками студентов

Система должна использовать:

* **ASP.NET Core Web API**
* **Dapper**
* **PostgreSQL**

Использование **Entity Framework запрещено**.

---

# 🗂 Таблицы базы данных

Необходимо создать следующие таблицы.

---

# 1️⃣ Students

Хранит информацию о студентах.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| FirstName   | text        |
| LastName    | text        |
| Phone       | text        |
| Email       | text        |
| CreatedDate | timestamptz |

---

# 2️⃣ Groups

Хранит учебные группы.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| Name        | text        |
| StartDate   | timestamptz |
| EndDate     | timestamptz |
| CreatedDate | timestamptz |

---

# 3️⃣ StudentGroups

Связь студентов и групп.

| Column     | Type        |
| ---------- | ----------- |
| Id         | int         |
| StudentId  | int         |
| GroupId    | int         |
| JoinedDate | timestamptz |

---

# 4️⃣ TimeTable

Расписание занятий для групп.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| DayOfWeek   | int         |
| FromTime    | time        |
| ToTime      | time        |
| CreatedDate | timestamptz |
| UpdatedDate | timestamptz |
| GroupId     | int         |

---

# 5️⃣ ProgressBook

Журнал посещаемости и оценок.

| Column         | Type        |
| -------------- | ----------- |
| Id             | int         |
| Grade          | int         |
| StudentId      | int         |
| IsAttended     | boolean     |
| Date           | timestamptz |
| GroupId        | int         |
| Notes          | text        |
| LateMinutes    | int         |
| UpdateByUserId | text        |

---

# 🌐 API Routes

Студенты должны реализовать следующие маршруты.

---

# 👨‍🎓 Students

| Method | Route          |
| ------ | -------------- |
| GET    | /students      |
| GET    | /students/{id} |
| POST   | /students      |
| PUT    | /students/{id} |
| DELETE | /students/{id} |

---

# 👥 Groups

| Method | Route        |
| ------ | ------------ |
| GET    | /groups      |
| GET    | /groups/{id} |
| POST   | /groups      |
| PUT    | /groups/{id} |
| DELETE | /groups/{id} |

---

# 👥 Students in Group

| Method | Route                                  |
| ------ | -------------------------------------- |
| POST   | /groups/{groupId}/students/{studentId} |
| DELETE | /groups/{groupId}/students/{studentId} |
| GET    | /groups/{groupId}/students             |

---

# 📅 Timetable

| Method | Route                       |
| ------ | --------------------------- |
| GET    | /groups/{groupId}/timetable |
| POST   | /timetable                  |
| PUT    | /timetable/{id}             |
| DELETE | /timetable/{id}             |

---

# 📘 ProgressBook

| Method | Route                      |
| ------ | -------------------------- |
| POST   | /progress                  |
| PUT    | /progress/{id}             |
| GET    | /students/{id}/progress    |
| GET    | /groups/{groupId}/progress |

---

# ⚠️ Валидации

Необходимо реализовать следующие проверки.

---

### 1️⃣ Студент должен существовать

Нельзя создать запись в **ProgressBook**, если студент не существует.

---

### 2️⃣ Студент должен быть в группе

Нельзя поставить оценку, если студент не состоит в группе.

Проверяется таблица:

```
StudentGroups
```

---

### 3️⃣ У группы должно быть расписание

Если у группы нет записи в **TimeTable**, то **нельзя создать запись в ProgressBook**.

---

### 4️⃣ Проверка оценки

Допустимые значения:

```
1 - 5
```

Если:

```
Grade < 1
Grade > 5
```

→ ошибка

---

### 5️⃣ Проверка LateMinutes

```
LateMinutes >= 0
LateMinutes <= 120
```

---

### 6️⃣ Проверка времени

В таблице **TimeTable**:

```
FromTime < ToTime
```

---

### 7️⃣ Дублирование расписания

Нельзя создать **2 расписания в один день для одной группы**.

Проверка по:

```
GroupId + DayOfWeek
```

---

### 8️⃣ Студент не может быть дважды в группе

Нельзя добавить студента в группу второй раз.

---

### 9️⃣ Проверка дат группы

```
StartDate < EndDate
```

---

# ⭐ Дополнительные задания (Bonus)

---

### ⭐ Средний балл студента

Route:

```
GET /students/{id}/average-grade
```

---

### ⭐ Процент посещаемости

Route:

```
GET /students/{id}/attendance
```

---

### ⭐ Лучшие студенты группы

Route:

```
GET /groups/{groupId}/top-students
```


# 📌 Вазифа

Лозим аст, ки **REST API системаи CRM барои академия** таҳия карда шавад, ки идора кунад:

* донишҷӯён
* гурӯҳҳо
* расписании дарсҳо
* ҳозиршавӣ
* баҳои донишҷӯён

Система бояд истифода барад:

* **ASP.NET Core Web API**
* **Dapper**
* **PostgreSQL**

Истифодаи **Entity Framework манъ аст**.

---

# 🗂 Таблицаҳои базаи маълумот

Лозим аст, ки таблицаҳои зерин сохта шаванд.

---

# 1️⃣ Students

Маълумоти донишҷӯёнро нигоҳ медорад.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| FirstName   | text        |
| LastName    | text        |
| Phone       | text        |
| Email       | text        |
| CreatedDate | timestamptz |

---

# 2️⃣ Groups

Маълумоти гурӯҳҳои таълимиро нигоҳ медорад.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| Name        | text        |
| StartDate   | timestamptz |
| EndDate     | timestamptz |
| CreatedDate | timestamptz |

---

# 3️⃣ StudentGroups

Пайвастшавии донишҷӯён ва гурӯҳҳо.

| Column     | Type        |
| ---------- | ----------- |
| Id         | int         |
| StudentId  | int         |
| GroupId    | int         |
| JoinedDate | timestamptz |

---

# 4️⃣ TimeTable

Расписании дарсҳо барои гурӯҳҳо.

| Column      | Type        |
| ----------- | ----------- |
| Id          | int         |
| DayOfWeek   | int         |
| FromTime    | time        |
| ToTime      | time        |
| CreatedDate | timestamptz |
| UpdatedDate | timestamptz |
| GroupId     | int         |

---

# 5️⃣ ProgressBook

Журнали ҳозиршавӣ ва баҳо.

| Column         | Type        |
| -------------- | ----------- |
| Id             | int         |
| Grade          | int         |
| StudentId      | int         |
| IsAttended     | boolean     |
| Date           | timestamptz |
| GroupId        | int         |
| Notes          | text        |
| LateMinutes    | int         |
| UpdateByUserId | text        |

---

# 🌐 API Route-ҳо

Донишҷӯён бояд route-ҳои зеринро амалӣ намоянд.

---

# 👨‍🎓 Students

| Method | Route          |
| ------ | -------------- |
| GET    | /students      |
| GET    | /students/{id} |
| POST   | /students      |
| PUT    | /students/{id} |
| DELETE | /students/{id} |

---

# 👥 Groups

| Method | Route        |
| ------ | ------------ |
| GET    | /groups      |
| GET    | /groups/{id} |
| POST   | /groups      |
| PUT    | /groups/{id} |
| DELETE | /groups/{id} |

---

# 👥 Донишҷӯён дар гурӯҳ

| Method | Route                                  |
| ------ | -------------------------------------- |
| POST   | /groups/{groupId}/students/{studentId} |
| DELETE | /groups/{groupId}/students/{studentId} |
| GET    | /groups/{groupId}/students             |

---

# 📅 Timetable

| Method | Route                       |
| ------ | --------------------------- |
| GET    | /groups/{groupId}/timetable |
| POST   | /timetable                  |
| PUT    | /timetable/{id}             |
| DELETE | /timetable/{id}             |

---

# 📘 ProgressBook

| Method | Route                      |
| ------ | -------------------------- |
| POST   | /progress                  |
| PUT    | /progress/{id}             |
| GET    | /students/{id}/progress    |
| GET    | /groups/{groupId}/progress |

---

# ⚠️ Validation (Санҷишҳо)

Лозим аст, ки санҷишҳои зерин амалӣ карда шаванд.

---

### 1️⃣ Донишҷӯ бояд вуҷуд дошта бошад

Агар донишҷӯ вуҷуд надошта бошад, **ProgressBook сохта намешавад**.

---

### 2️⃣ Донишҷӯ бояд дар гурӯҳ бошад

Баҳо гузошта намешавад, агар донишҷӯ дар гурӯҳ набошад.

Санҷида мешавад дар таблицаи:

```
StudentGroups
```

---

### 3️⃣ Барои гурӯҳ бояд расписание бошад

Агар дар гурӯҳ сабт дар **TimeTable** вуҷуд надошта бошад, **ProgressBook сохта намешавад**.

---

### 4️⃣ Санҷиши баҳо

Баҳо бояд дар диапазони зерин бошад:

```
1 - 5
```

Агар:

```
Grade < 1
Grade > 5
```

→ хатогӣ баргардонида мешавад.

---

### 5️⃣ Санҷиши LateMinutes

```
LateMinutes >= 0
LateMinutes <= 120
```

---

### 6️⃣ Санҷиши вақт

Дар таблицаи **TimeTable**:

```
FromTime < ToTime
```

---

### 7️⃣ Дублирование расписание

Дар як рӯз барои як гурӯҳ **2 расписание сохта намешавад**.

Санҷиш бо:

```
GroupId + DayOfWeek
```

---

### 8️⃣ Донишҷӯ ду бор дар як гурӯҳ шуда наметавонад

Наметавон донишҷӯро ба як гурӯҳ **боз як маротиба илова кард**.

---

### 9️⃣ Санҷиши санаҳои гурӯҳ

```
StartDate < EndDate
```

---

# ⭐ Вазифаҳои иловагӣ (Bonus)

---

### ⭐ Баҳои миёнаи донишҷӯ

Route:

```
GET /students/{id}/average-grade
```

---

### ⭐ Фоизи ҳозиршавӣ

Route:

```
GET /students/{id}/attendance
```

---

### ⭐ Донишҷӯёни беҳтарини гурӯҳ

Route:

```
GET /groups/{groupId}/top-students
```


CREATE TABLE IF NOT EXISTS students (
    id          SERIAL PRIMARY KEY,
    firstname   TEXT NOT NULL,
    lastname    TEXT NOT NULL,
    phone       TEXT NOT NULL,
    email       TEXT NOT NULL,
    createddate TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS groups (
    id          SERIAL PRIMARY KEY,
    name        TEXT NOT NULL,
    startdate   TIMESTAMPTZ NOT NULL,
    enddate     TIMESTAMPTZ NOT NULL,
    createddate TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS studentgroups (
    id         SERIAL PRIMARY KEY,
    studentid  INT NOT NULL REFERENCES students(id),
    groupid    INT NOT NULL REFERENCES groups(id),
    joineddate TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS timetable (
    id          SERIAL PRIMARY KEY,
    dayofweek   INT NOT NULL,
    fromtime    TIME NOT NULL,
    totime      TIME NOT NULL,
    createddate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updateddate TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    groupid     INT NOT NULL REFERENCES groups(id)
);

CREATE TABLE IF NOT EXISTS progressbook (
    id             SERIAL PRIMARY KEY,
    grade          INT NOT NULL,
    studentid      INT NOT NULL REFERENCES students(id),
    isattended     BOOLEAN NOT NULL DEFAULT FALSE,
    date           TIMESTAMPTZ NOT NULL,
    groupid        INT NOT NULL REFERENCES groups(id),
    notes          TEXT,
    lateminutes    INT NOT NULL DEFAULT 0,
    updatebyuserid TEXT
);