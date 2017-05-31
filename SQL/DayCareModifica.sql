--use DC

--support day care name for schedules
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertSchedule] (@DayCareId uniqueidentifier,@Name varchar(200),@Message nvarchar(max),@Time varchar(max))
AS
BEGIN	
	SET NOCOUNT ON;		
	DECLARE @Messages TABLE (idx INT,record varchar(2000));
	DECLARE @Times TABLE (idx INT,record varchar(20));
	DECLARE @MyTempTable table (MyTempId int);	
	Declare @ScheduleId int;
	Declare @UniqueName varchar(220);
	Set @UniqueName = @Name;
	Declare @Counter int; set @counter=2;
	While(EXISTS (SELECT * FROM dbo.Schedule WHERE Name=@UniqueName))
	  Begin
	    set @UniqueName=@Name+CAST(@Counter as nvarchar(20));
		set @counter=@counter+1;
	  End
	Insert into dbo.Schedule(Name,DayCareId,DateCreated,UniqueName)OUTPUT inserted.ScheduleId INTO @MyTempTable
	values(@Name,@DayCareId,GETDATE(),@UniqueName);
	select @ScheduleId = MyTempId from @MyTempTable;
	insert into @Messages(idx,record) select idx,record from _BuildTable(@Message,'^');	
	insert into @Times(idx,record) select idx,record from _BuildTable(@Time,'^');
	insert into dbo.ScheduleMessages(ScheduleId,Time,Message,DateCreated) 
	select @ScheduleId,a.record,b.record,GETDATE() from @Times a join @Messages b on a.idx=b.idx;
	select @DayCareId as DayCareId,d.DayCareName,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId and s.ScheduleId=@ScheduleId;
END

--get parent's daycareid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetParentsDayCare] (@ParentId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	DECLARE @Temp TABLE (idx INT,record int);
	declare @KidIds nvarchar(4000);
	select @KidIds=KidId from dbo.Parent where ParentId=@ParentId;
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');
	select top 1 k.DayCareId from dbo.kid k join @Temp t on t.idx=k.kidId;
END

--*****************************************************************
--working tets data

insert into LU_WhatsNew values(1,'Alexa now supports GigglesWare','Enable GigglesWare skill on Alexa and log kids activities with commands and ask Alexa for kids reports','https://gigglesware.com/images/Alexa-Small.png',GETDATE());

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
