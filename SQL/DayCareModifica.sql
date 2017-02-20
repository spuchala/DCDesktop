--use daycare

insert into LU_WhatsNew values(1,'Alexa now supports GigglesWare','Enable GigglesWare skill on Alexa and log kids activities with commands and ask Alexa for kids reports','https://gigglesware.com/images/Alexa-Small.png',GETDATE());



--*****************************************************************
--working tets data

delete from LU_WhatsNew

select * from schedule
select * from ScheduleMessages
select * from daycareinfo
delete from DayCareSnaps
delete from daycare where daycareid='7A85222E-0197-E611-9C1F-C4D9879CF00D'
select * from kid
select * from PublicContact

select d.DayCareName,i.DescriptionHome,i.DescriptionAboutUs,i.DescriptionProgram,i.Logo,s.SnapId,s.SnapTitle
	from dbo.DayCare d Join dbo.DayCareInfo i on d.DayCareId=i.DayCareId
	left join dbo.DayCareSnaps s on d.DayCareId=s.DayCareId where d.DayCareId='0ef9015d-1a24-e611-9c07-c4d9879cf00d';

delete from ScheduleMessages
delete from schedule

select * from classes 
select * from kid
update kid set ClassId=null
The UPDATE statement conflicted with the FOREIGN KEY constraint "Fk_Kid_Classes". The conflict occurred in database "DayCare", table "dbo.Classes", column 'ClassId'.
The statement has been terminated.

use daycare
delete from kid
delete from InstantLog
delete from kidlog where kidid in (1,2,10,11,12)
update kid set DOB='01 Apr, 2016'
select * from daycare
delete from daycare where daycareid='D534ECB4-A385-E611-9C1C-C4D9879CF00D'
update kid set active=1
select * from Documents
delete from Documents
select * from whatsnew

select * from instantlog
select * from InstantLogMessages
select * from parent
select * from settings
delete from kidlog
delete from InstantLogMessages 
delete from instantlog

SELECT CONVERT(Date, '27 May, 2016', 6)
SELECT CONVERT(date, getdate())

select DATEDIFF(month,'2015-09-13','2016-06-02')


SELECT DIFFERENCE('Dylan', 'Dillon,Ryder') 

select * from kid

delete from kid where kidid=9

update kid set name='Mary,Puchala' where kidid=1

select * from documents
update documents set active=1 

--********************************************************************
