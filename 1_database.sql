-- 1. Проектирование БД
-- вариант 1
create table People (
	id int primary key,
	name nvarchar(50) not null,
	middlename nvarchar(50) null,
	surname nvarchar(50) not null,
	birthday datetime not null,
	phonenumber nvarchar(12) not null,
	inn char(12) null,
	isEmployee bit
)

--A - высокое удобство, все люди в одной таблице
--B - низкая расширяемость - любое новое поля для пациентов автоматически будет использоваться и для сотрудников и наоборот
--C - ... - правки для сотрудника также будут соответствовать ему как пациенту
--D - лишние столбцы для хранения данных для пациентов - ИНН, дополнительная фильтрация для вывода сотрудников - isEmployee
--E + одна таблица
--  + внесение изменений для работника в одном месте
--  - избыточные столбцы для пациентов (в данном примере ИНН)
--  - отсутствие проверки ИНН на пустое значение для сотрудников
--  - безопасность - нет разграничения доступа к данным сотрудников и пациентов

-- вариант 2
create table People (
	people_id int primary key,
	name nvarchar(50) not null,
	middlename nvarchar(50) null,
	surname nvarchar(50) not null,
	birthday datetime not null,
	phonenumber nvarchar(12) not null
)

create table Doctor (
	doctor_id int not null,
	inn char(12) not null,
	foreign key (doctor_id) references People(people_id)
)

--A - высокое удобство, все люди в одной таблице
--B - средняя расширяемость - любое новое поля для пациентов автоматически будет использоваться для сотрудников, которые никогда не были пациентами
--C - правки для сотрудника также будут соответствовать ему как пациенту
--D - нет лишних столбцов, но для вывода списка сотрудников необходимо связать таблицы
--E

-- вариант 3
create table Patient (
	patient_id int primary key,
	name nvarchar(50) not null,
	middlename nvarchar(50) null,
	surname nvarchar(50) not null,
	birthday datetime null,
	phonenumber nvarchar(12) null
)

create table Doctor (
	doctor_id int primary key,
	name nvarchar(50) not null,
	middlename nvarchar(50) not null,
	surname nvarchar(50) not null,
	birthday datetime not null,
	phonenumber nvarchar(12) not null
	inn char(12) not null
)

--A - среднее удобство, необходим механизм синхронизации например изменения/редактирования учетных данных если доктор пациент
--B - высокая расширяемость
--C - 
--D - высокая, нет необходимости фильтрации, нет лищних столбцов
--E - безопасность в разграничении доступа к списку сотрудников и пациентов, возможность различных требований к заполнению полей

-- вариант 4
create table People (
	people_id int primary key,
	name nvarchar(50) not null,
	middlename nvarchar(50) not null,
	surname nvarchar(50) not null,
	birthday datetime not null,
	phonenumber nvarchar(12) not null
)

create table Patient (
	patient_id int primary key,
	-- дополнительные поля
	foreign key (patient_id) references People(people_id)
)

create table Doctor (
	doctor_id int primary key,
	inn char(12) not null,
	foreign key (doctor_id) references People(people_id)
)

--A - высокое удобство
--B - высокая расширяемость
--C - высокая
--D - высокая, нет необходимости фильтрации, нет лишних столбцов
--E - безопасность в разграничении доступа к списку сотрудников и пациентов, возможность различных требований к заполнению полей
--	предпочтительный вариант