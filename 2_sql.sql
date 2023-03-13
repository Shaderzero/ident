-- 2. SQL запросы
create table Receptions (
	ID int primary key,
	ID_Patients  int not null,
	ID_Doctors  int not null,
	StartDateTime datetime not null,
	foreign key (ID_Patients) references Patient(patient_id),
	foreign key (ID_Doctors) references Doctor(doctor_id)
)

-- запрос 1
declare @year as int = 2015;  
declare @fromDate datetime = DATEADD(yyyy, @year - 1900, 0),@toDate datetime=DATEADD(yyyy, @year - 1900 + 1, -1);
  
with dates (dateNo) as (
	select DATEADD(DAY, DATEDIFF(DAY, 0, @toDate) - DATEDIFF(DAY, @fromDate, @toDate), 0)
	union all
	select DATEADD(DAY, 1, dateNo)
		from dates where DATEADD(DAY, 1, dateNo) <= @toDate
)

select dateNo, COUNT(Receptions.ID) from dates
left join Receptions on Receptions.StartDateTime = dateNo
group by dateNo

option (maxrecursion 365);

-- запрос 2
-- вариант 1
select p.LastVisit as 'последний визит', pname as 'пациент', dname as 'доктор' 
from (select MAX(Receptions.StartDateTime) as LastVisit, p.people_id, pname from Receptions
inner join (
	select p.people_id, CONCAT(p.surname, ' ', p.name) as pname from People p
) p on p.people_id = Receptions.ID_Patients
group by p.people_id, pname) p
inner join (
	select Receptions.StartDateTime, Receptions.ID_Patients, dname from Receptions
	inner join (
		select p.people_id, CONCAT(p.surname, ' ', p.name) as dname from People p
	) p on p.people_id = Receptions.ID_Doctors
) d on d.StartDateTime = p.LastVisit and d.ID_Patients = d.ID_Patients

-- вариант 2
select r.LastVisit as 'последний визит', pname as 'пациент', dname as 'доктор' from Patient
inner join (
	select p.people_id, CONCAT(p.surname, ' ', p.name) as pname from People p
) p on p.people_id = Patient.patient_id
inner join (
	select MAX(r.StartDateTime) as LastVisit, r.ID_Patients as patient_id from Receptions as r
	group by r.ID_Patients
) r on r.patient_id = Patient.patient_id
inner join (
	select r.StartDateTime, r.ID_Patients as patient_id, p.dname as dname from Receptions r
	inner join (
		select p.people_id, CONCAT(p.surname, ' ', p.name) as dname from People p
	) p on p.people_id = r.ID_Doctors
) rd on rd.StartDateTime = LastVisit and rd.patient_id = r.patient_id