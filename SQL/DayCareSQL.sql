use DayCare

drop table parent
drop table kidlog
drop table Attendance
drop table kid
drop table classes
drop table DayCare


create table 
DayCare 
(DayCareId uniqueidentifier default newsequentialid() not null,
 DayCareName nvarchar(200) not null,
 [Address] nvarchar(500) not null,
 Phone nvarchar(20) not null,
 AdminName nvarchar(100) not null,
 Email nvarchar(200) not null,
 [Password] nvarchar(20) not null,
 DateCreated datetime not null,
 DateModified datetime null default null,
 CONSTRAINT DayCare_Id PRIMARY KEY (DayCareId)
 )

create table Classes
(
 ClassId uniqueidentifier default newsequentialid() not null,
 ClassName nvarchar(100) not null,
 DayCareId uniqueidentifier not null, 
 CONSTRAINT Classes_Id PRIMARY KEY (ClassId),
 CONSTRAINT Fk_Classes_DayCare foreign key (DayCareId) REFERENCES DayCare(DayCareId) 
) 

create table 
Kid 
(KidId int identity(1,1) not null,
 Name nvarchar(100) not null,
 Sex nvarchar(6) not null,
 DOB nvarchar(15) not null,
 [Address] nvarchar(500) not null,
 GuardianName nvarchar(100) not null, 
 Email nvarchar(200) not null,
 Phone nvarchar(20) not null,
 Allergies nvarchar(3000) not null,
 DayCareId uniqueidentifier not null, 
 ClassId uniqueidentifier null default null,
 DateCreated datetime not null,
 DateModified datetime null default null,
 CONSTRAINT Kid_Id PRIMARY KEY (KidId),
 CONSTRAINT Fk_Kid_DayCare foreign key (DayCareId) REFERENCES DayCare(DayCareId),
 CONSTRAINT Fk_Kid_Classes foreign key (ClassId) REFERENCES Classes(ClassId) 
 )
 
 create table 
 KidLog
 (LogId uniqueidentifier default newsequentialid() not null,
  KidId int not null,  
  [Day] Date not null,
  WhatKidAte nvarchar(4000) null,
  HowKidAte nvarchar(4000)  null,
  WhenKidAte nvarchar(4000)  null, 
  AnySnack nvarchar(4000)  null,
  DiaperCheckTime nvarchar(4000) null,  
  PottyTime nvarchar(4000) null,
  DiaperPottyType nvarchar(4000) null,
  NapTime nvarchar(4000) null,
  ActivityTime nvarchar(4000) null,
  Mood nvarchar(4000) null,
  Activities nvarchar(4000) null,
  ProblemsConcerns nvarchar(4000) null,
  SuppliesNeeded nvarchar(4000) null,
  Comments nvarchar(max) null,
  EmailSentToParent bit not null default 0,
  DateCreated datetime not null,
  CONSTRAINT Log_Id PRIMARY KEY (LogId),
  CONSTRAINT Fk_KidLog_Kid foreign key (KidId) REFERENCES Kid(KidId)
 )
 
 
 create table 
 Attendance
 (
  DayCareId uniqueidentifier not null,
  KidId int not null,  
  [Day] Date not null,
  Attended bit not null,
  DateCreated datetime not null,
  CONSTRAINT Fk_Attendance_Kid foreign key (KidId) REFERENCES Kid(KidId),
  CONSTRAINT Fk_Attendance_DayCare foreign key (DayCareId) REFERENCES DayCare(DayCareId)
 )
 
 
 create table 
 Parent
(ParentId uniqueidentifier default newsequentialid() not null,
 KidId nvarchar(3000) not null,
 Name nvarchar(100) not null, 
 Email nvarchar(200) not null,
 [Password] nvarchar(20) not null,
 Phone nvarchar(20) not null,
 DateCreated datetime not null,
 DateModified datetime null default null,
 CONSTRAINT Parent_Id PRIMARY KEY (ParentId)
 )
 
--insert day care admin
--modified 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE()
	        );	
	 SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

--insert kid stored proc
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertKid] (@Name NVARCHAR(100),@Sex nvarchar(6),@DOB nvarchar(15),
@Address NVARCHAR(500),@GuardianName nvarchar(100),@Email NVARCHAR(200),@Phone NVARCHAR(20),@ClassId uniqueidentifier =null,@ClassName nvarchar(200)=null,
@Allergies NVARCHAR(3000),@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	IF EXISTS (SELECT * FROM dbo.Kid WHERE Email=@Email and Name=@Name)
	BEGIN
        RAISERROR ('The kid is already registred under the email.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN	    
	    If(@ClassId is null and (@ClassName!='' and @ClassName is not null))
	     begin
	       set @ClassId=NEWID();
	       insert into Classes values(@ClassId,@ClassName,@DayCareId);
	     end	     
		INSERT INTO dbo.Kid
	        ( Name,
	          Sex,
	          DOB,
	          [Address],
	          GuardianName,
	          ClassId,
	          Email,
	          Phone,
	          Allergies ,
	          DayCareId ,	          
	          DateCreated	          
	        )
		VALUES  ( @Name , -- UserName - nvarchar(200)
	          @Sex , -- Email - nvarchar(200)
	          @DOB,
	          @Address , -- Phone - nvarchar(20)
	          @GuardianName,
	          @ClassId,
	          @Email , -- StreetAddress - nvarchar(300)
	          @Phone , -- City - nvarchar(100)
	          @Allergies , -- PinCode - nvarchar(20)
	          @DayCareId,	          
	          GETDATE()
	        );
		SELECT KidId,Email FROM dbo.Kid WHERE Email=@Email and Name=@Name and DayCareId=@DayCareId;      	 
	END
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

--delete kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[DeleteKid] (@KidId int,@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	IF Not EXISTS (SELECT * FROM dbo.Kid WHERE KidId=@KidId and DayCareId=@DayCareId)
	BEGIN
        RAISERROR ('The kid doesnt belong to your day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	    delete from KidLog where KidId=@KidId;
	    delete from Attendance where KidId=@KidId and DayCareId=@DayCareId;
	    delete from Kid where KidId=@KidId and DayCareId=@DayCareId;   	 
	END
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

--insert the parent
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertParent] (@Name nvarchar(100),
@Email NVARCHAR(200),@Password nvarchar(20),@Phone NVARCHAR(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	begin transaction
	IF EXISTS (SELECT * FROM dbo.Parent WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a parent.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	declare @KidIdCsv nvarchar(4000);
	SELECT @kidIdCsv=SUBSTRING((SELECT ',' + CONVERT(varchar(10), k.KidId)
	FROM kid k where k.Email=@Email ORDER BY k.KidId FOR XML PATH('')),2,200000);
	INSERT INTO dbo.Parent
	        ( KidId,
	          Name,
	          Email,
	          [Password],
	          Phone,	          	          
	          DateCreated	          
	        )OUTPUT inserted.ParentId INTO @MyTempTable
	VALUES  ( @kidIdCsv , -- UserName - nvarchar(200)
	          @Name , -- Email - nvarchar(200)	          
	          @Email , -- StreetAddress - nvarchar(300)
	          @Password,
	          @Phone , -- City - nvarchar(100)	                    
	          GETDATE()
	        );
	  SELECT MyTempId as ParentId FROM @MyTempTable; 	 
	END
	commit
	end try
	begin catch	
	    IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

--insert class
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertClass] (@DayCareId uniqueidentifier,@ClassName nvarchar(100))
AS
BEGIN	
	SET NOCOUNT ON; 
	begin try
	IF EXISTS (SELECT ClassId FROM dbo.Classes WHERE ClassName=@ClassName and DayCareId=@DayCareId)
	BEGIN
        RAISERROR ('Class Name already exists.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);			
	insert into Classes(DayCareId,ClassName)OUTPUT inserted.ClassId INTO @MyTempTable values(@DayCareId,@ClassName);
	SELECT MyTempId as ClassId FROM @MyTempTable;
	end
	end try
	begin catch		    	
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch  
END
GO

--assign kid to class
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[AssignClassToKid] (@DayCareId uniqueidentifier,@ClassId uniqueidentifier,@KidIds nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 		
	DECLARE @Temp TABLE (idx INT,record int);	
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');
	update k set k.ClassId=@ClassId from Kid k join @Temp t on k.KidId=t.record
	where k.DayCareId=@DayCareId;
END
GO

--insert the kid log
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertKidLog] (@KidId int,@WhatKidAte nvarchar(4000),@HowKidAte nvarchar(4000),@WhenKidAte nvarchar(4000),
@AnySnack NVARCHAR(4000),@DiaperCheckTime NVARCHAR(4000),@PottyTime NVARCHAR(4000),@DiaperPottyType nvarchar(4000),
@NapTime nvarchar(4000),@Mood nvarchar(4000),@ActivityTime nvarchar(4000),@Activities nvarchar(4000),@ProblemsConcerns nvarchar(4000),
@SuppliesNeeded nvarchar(4000),@Comments nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	begin transaction	
	IF not EXISTS (SELECT * FROM dbo.KidLog WHERE KidId=@KidId and [DAY]=CAST(GETDATE() AS DATE))
	BEGIN	
		DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
		declare @logid uniqueidentifier;	
		INSERT INTO dbo.KidLog
	        ( KidId,
	          [Day],
	          WhatKidAte,
	          WhenKidAte,
	          HowKidAte,	          	          
	          AnySnack,
	          DiaperCheckTime,
	          PottyTime,
	          DiaperPottyType,
	          NapTime,
	          ActivityTime,
	          Mood,
	          Activities,
	          ProblemsConcerns,
	          SuppliesNeeded,
	          Comments,
	          DateCreated          
	        )OUTPUT inserted.LogId INTO @MyTempTable
		VALUES  ( 
			  @KidId,	
			  CAST(GETDATE() AS DATE) , -- UserName - nvarchar(200)
	          @WhatKidAte , -- Email - nvarchar(200)
	          @whenkidAte,	          
	          @HowKidAte , -- StreetAddress - nvarchar(300)
	          @AnySnack , -- City - nvarchar(100)
	          @DiaperCheckTime,
	          @PottyTime,
	          @DiaperPottyType,
	          @NapTime,
	          @ActivityTime,
	          @Mood,
	          @Activities,
	          @ProblemsConcerns,
	          @SuppliesNeeded,
	          @Comments,	                    
	          GETDATE()
	        );	
		SELECT @logid = MyTempId FROM @MyTempTable;	        
		select @logid as LogId, name from Kid where KidId=@KidId;  
	  END
	  else
	  begin
	    update KidLog set WhatKidAte=@WhatKidAte,WhenKidAte=@WhenKidAte,HowKidAte=@HowKidAte,
	    AnySnack=@AnySnack,DiaperCheckTime=@DiaperCheckTime,PottyTime=@PottyTime,DiaperPottyType=@DiaperPottyType,
	    NapTime=@NapTime,ActivityTime=@ActivityTime,Mood=@Mood,Activities=dbo.IsReplaceble(Activities,@Activities),
	    ProblemsConcerns=dbo.IsReplaceble(ProblemsConcerns,@ProblemsConcerns),
	    SuppliesNeeded=dbo.IsReplaceble(SuppliesNeeded,@SuppliesNeeded),Comments=dbo.IsReplaceble(Comments,@Comments),DateCreated=GETDATE()
	    where KidId=@KidId and [DAY]=CAST(GETDATE() AS DATE);
	    select LogId, 'test' as Name from KidLog where KidId=@KidId and [DAY]=CAST(GETDATE() AS DATE);
	  end	 	    
	Commit       
	end try
	begin catch	
	    IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

create FUNCTION IsReplaceble (@old nvarchar(4000),@new nvarchar(max))
    RETURNS nvarchar(max)
AS
begin
  declare @final nvarchar(max);
  if(@old is null)
  begin
   set @final= @new;
  end
  else if(@old is not null and @new ='')
  begin
   set @final= @old;
  end
  else if(@old is not null and @new !='')
  begin
   set @final= @new;
  end
  return @final;
end 
go

CREATE FUNCTION [dbo].[_BuildTable](@list nvarchar(MAX),@delimiter varchar(5))
returns @tbl  TABLE (idx int IDENTITY(1,1) PRIMARY KEY, record varchar(max))
as
begin 
	declare
		@I int, 			-- Current place in delimited string
		@StrLen int, 		-- Length of delimited string
		@StrStart int, 		-- Start point of current record
		@record varchar (max),	-- Current record
		@DelLen int -- delimter length	

	set @DelLen = len(@delimiter)	
	--make sure the list does not ends with a delimiter
	if right(@list,@DelLen) = @delimiter
		set @list=substring(@list, 1, len(@list) - @DelLen)
	set @I = 1
	set @StrLen = len(@list)
	--Insert strings separated by delimiter as individual records
	while (@I <= @StrLen)
	begin
 		set @StrStart = (select charindex(@delimiter, @list, @I))	
		if (@StrStart < @I)	begin
			set @record=substring(@list, @I, @StrLen)	
			set @I = @StrLen + @DelLen
		end
		else begin
 			set @record=substring(@list, @I, (@StrStart) - @I)
			set @I = @StrStart + @DelLen
		end
		insert into @tbl (record)
			select @record
	end	
	return
end
GO

--get the kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKid] (@KidId int,@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 		
	select a.*,b.ClassName,b.ClassId from Kid a left join Classes b on a.ClassId=b.ClassId where a.KidId=@KidId and a.DayCareId=@DayCareId;
END
GO

--check if kid has report today
create FUNCTION KidHasReportToday (@id int)
    RETURNS bit
AS
begin
  declare @HasReportToday bit;
  set @HasReportToday=0;
  if exists(select logid from KidLog where KidId=@id and [day]=CONVERT(date, getdate()))
	begin
	 set @HasReportToday=1;
	end
  else
    begin
     set @HasReportToday=0;
    end
   return @HasReportToday;  	  
end 
go

--get kids
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidsFromDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select *,dbo.KidHasReportToday(KidId) as HasReportToday from Kid where DayCareId=@DayCareId;
END
GO

--get kids in class
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidsInClass] (@ClassId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select k.* from Classes c join Kid k on c.ClassId=k.ClassId where c.ClassId=@ClassId;
END
GO

--get classes for daycare
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetClassesForDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.ClassId,a.ClassName,a.DayCareId ,COUNT(b.ClassId) as NoOfKids 
	from Classes a left join Kid b on a.DayCareId=b.DayCareId and b.ClassId=a.ClassId
	where a.DayCareId=@DayCareId
	group by b.ClassId,a.ClassId,a.ClassName,a.DayCareId;	
END
GO

--get kids without classes
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidsWithNoClass] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.KidId,b.Name from Kid b where b.DayCareId=@DayCareId and b.ClassId is null;	
END
GO

--get kids from parent
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidsFromParent] (@ParentId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	DECLARE @Temp TABLE (idx INT,record int);
	declare @KidIds nvarchar(4000);
	select @KidIds=KidId from Parent where ParentId=@ParentId;
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');	
	select a.*,dbo.KidHasReportToday(a.KidId) as HasReportToday from Kid a join @Temp b on b.record=a.KidId;
END
GO



--get the parent
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetParent] (@ParentId uniqueidentifier,@day date= null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	DECLARE @Temp TABLE (idx INT,record int);
	declare @KidIds nvarchar(4000);
	select @KidIds=KidId from Parent where ParentId=@ParentId;
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');
	select * from Parent where ParentId=@ParentId;
	select a.* from Kid a join @Temp b on b.record=a.KidId;
	if(@day is null)
	begin
	select a.* from KidLog a join @Temp b on b.record=a.KidId where a.Day=CONVERT(date, getdate());
	end
	else
	begin
	select a.* from KidLog a join @Temp b on b.record=a.KidId where a.Day=@day;
	end
END
GO

--get day care data
GO
/****** Object:  StoredProcedure [dbo].[GetDayCare]    Script Date: 08/16/2015 22:31:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetDayCareReport] (@DayCareId uniqueidentifier,@day date= null)
AS
BEGIN	
	SET NOCOUNT ON; 
	select * from Kid where DayCareId=@DayCareId;
	if(@day is null)
	begin	
	select a.* from KidLog a join Kid b on a.KidId=b.KidId 
	where b.DayCareId=@DayCareId and a.Day=CONVERT(date, getdate());
	end
	else
	begin
	select a.* from KidLog a join Kid b on a.KidId=b.KidId 
	where b.DayCareId=@DayCareId and a.Day=@day;
	end
END



--get the parent
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[CheckParentByEmail] (@EmailId nvarchar(200))
AS
BEGIN	
	SET NOCOUNT ON; 	
	declare @IsRegistered bit;
	declare @HasKidInSystem bit;
	if exists(select Email from Kid where Email=@EmailId)
	begin
	 set @HasKidInSystem=1;
	  if exists(select Email from Parent where Email=@EmailId)
	  begin
	   set @IsRegistered=1;
	  end
	  else
	  begin
	   set @IsRegistered=0;
	  end
	end	
	else
	begin
	 set @HasKidInSystem=0;
	 set @IsRegistered=0;
	end
	select @HasKidInSystem as HasKidInSystem,@IsRegistered as IsRegistered;
END
GO

--get the kid log by log id
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidLogByLogId] (@LogId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from KidLog where LogId=@LogId;
END
GO

--get emails for parents for a day care on a day
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetEmailsForParents] (@Id uniqueidentifier, @KidIds nvarchar(max))
AS
BEGIN	
	DECLARE @Temp TABLE (idx INT,record int);	
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');		
	select a.Email,a.Name,b.* from @Temp t join Kid a on a.KidId=t.record join KidLog b on a.KidId=b.KidId
	where a.DayCareId=@Id and b.Day=CAST(GETDATE() AS DATE) and b.EmailSentToParent=0;
	update a set a.EmailSentToParent=1 from kid k join KidLog a on k.KidId=a.KidId join @Temp t on a.KidId=t.record 
	where k.DayCareId=@Id and a.Day=CAST(GETDATE() AS DATE);
END
GO

--get the kid log on a day
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidLogOnADay] (@KidId int, @Id uniqueidentifier, @Day Date)
AS
BEGIN	
	SET NOCOUNT ON; 
	if exists (select * from DayCare where DayCareId=@id)
	begin	
		select b.* from Kid a join KidLog b on a.KidId=b.KidId 
		join DayCare c on c.DayCareId=a.DayCareId
		where b.KidId=@KidId and c.DayCareId=@Id and b.Day=@Day;
	end
	else if exists (select * from Parent where ParentId=@id and CHARINDEX(CONVERT(varchar(10), @KidId),KidId) > 0)
	begin	
		select a.* from KidLog a join Kid b on b.KidId=a.KidId where a.Day=@day
		and a.KidId=@KidId;
	end
END
GO

--get the kid log on a day range
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidLogInDayRange] (@KidId int, @DayCareId uniqueidentifier, @StartDay Date, @EndDay Date)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.* from Kid a join KidLog b on a.KidId=b.KidId 
	join DayCare c on c.DayCareId=a.DayCareId
	where b.KidId=@KidId and c.DayCareId=@DayCareId and b.Day between @StartDay and @EndDay;
END
GO

--get day care data
GO
/****** Object:  StoredProcedure [dbo].[GetDayCare]    Script Date: 08/16/2015 22:31:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId
	where a.DayCareId=@DayCareId;
END

--get the user creds
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[LoginUser] (@Email nvarchar(200),@Password nvarchar(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email and [Password]=@Password)
	BEGIN
       SELECT 'dayCare' as role,DayCareId as id,AdminName as name,Email,Phone FROM dbo.DayCare WHERE Email=@Email and [Password]=@Password;
    END   
    ELSE IF EXISTS(SELECT * FROM dbo.Parent WHERE Email=@Email and [Password]=@Password)
    BEGIN
       SELECT 'parent' as role,ParentId as id,Name,Email,Phone FROM dbo.Parent WHERE Email=@Email and [Password]=@Password
    END
END
GO

--insert attendance
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertAttendance] (@DayCareId uniqueidentifier,@KidId int,@HasAttended bit)
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.kid WHERE DayCareId=@DayCareId and KidId=@KidId)
	BEGIN
       if exists(SELECT * FROM dbo.Attendance WHERE DayCareId=@DayCareId and KidId=@KidId and DAY=CAST(GETDATE() AS DATE))
         begin
           update Attendance set Attended=@HasAttended where KidId=@KidId and DayCareId=@DayCareId and DAY=CAST(GETDATE() AS DATE);
         end
         else
         begin
           insert into Attendance(DayCareId,KidId,Attended,Day,DateCreated) values(@DayCareId,@KidId,@HasAttended,CAST(GETDATE() AS DATE),GETDATE());
         end
    END   
    ELSE 
    BEGIN
       RAISERROR('No Kid exists with that name', 1, 1);
    END
END
GO

--insert attendance
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetAttendance] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 
	DECLARE @MyTempTable table (Name nvarchar(200),KidId int,Attended bit);
	insert into @MyTempTable(Name,KidId,Attended)
	SELECT Name,KidId,null FROM Kid 
	WHERE DayCareId=@DayCareId;
	update temp set temp.Attended = a.Attended from @MyTempTable temp
	join Attendance a on a.KidId=temp.KidId  
	where a.DayCareId=@DayCareId and a.DAY=CAST(GETDATE() AS DATE); 
	select @DayCareId as DayCareId,* from @MyTempTable;
END
GO

use DayCare

--****************************************************************************************************************************************

--modified and new data

--*****************************************************************************************************************************
create table ErrorLogs(error nvarchar(2000),errorat nvarchar(200),errorby nvarchar(200),createddate datetime)


create table Settings(DayCareId uniqueidentifier,SettingsVisited bit not null default 0,
CustomReport bit not null default 0,GigglesWareReport bit not null default 0,MakePublic bit not null default 0)


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE()
	        );	
    Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic)
	SELECT MyTempId as DayCareId,0,0,1,0 FROM @MyTempTable;
	        	      
	SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

--get day care data
GO
/****** Object:  StoredProcedure [dbo].[GetDayCare]    Script Date: 08/16/2015 22:31:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,c.SettingsVisited,c.CustomReport
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId;
END

--settings
GO
/****** Object:  StoredProcedure [dbo].[GetDayCare]    Script Date: 08/16/2015 22:31:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetSettings] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from dbo.Settings 
	where DayCareId=@DayCareId;
	update dbo.Settings set SettingsVisited=1 where DayCareId=@DayCareId;
END

--save settings
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[SaveSettings] (@DayCareId uniqueidentifier,@CustomReport bit,@GigglesWareReport bit, @MakePublic bit,@SettingsVisited bit)
AS
BEGIN	
	SET NOCOUNT ON; 	
	update dbo.Settings set CustomReport=@CustomReport, GigglesWareReport=@GigglesWareReport, MakePublic=@MakePublic, SettingsVisited=@SettingsVisited
	where DayCareId=@DayCareId;
END

--check custom report created
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[CheckCustomReportExists] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from dbo.CustomReport where DayCareId=@DayCareId;
END

ALTER TABLE settings
ADD [Language] nvarchar(20) NOT NULL DEFAULT 'en-us'

--save settings
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[SaveSettings] (@DayCareId uniqueidentifier,@CustomReport bit,@GigglesWareReport bit, @MakePublic bit,@SettingsVisited bit,@Language nvarchar(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	update dbo.Settings set CustomReport=@CustomReport, GigglesWareReport=@GigglesWareReport, MakePublic=@MakePublic, SettingsVisited=@SettingsVisited, Language=@Language
	where DayCareId=@DayCareId;
END


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE()
	        );	
    Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic,[Language])
	SELECT MyTempId as DayCareId,0,0,1,0,'en-us' FROM @MyTempTable;
	        	      
	SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END
GO

Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic,[Language])
	SELECT DayCareId,0,0,1,0,'en-us' FROM daycare;

create table dbo.CustomReport
(CustomReportId uniqueidentifier not null,
DayCareId uniqueidentifier not null,
CreatedDate datetime not null,
CONSTRAINT CustomReport_Id PRIMARY KEY (CustomReportId),
CONSTRAINT Fk_CustomReport_DayCare foreign key (DayCareId) REFERENCES DayCare(DayCareId)
)

create table dbo.CustomReportQuestions(
CustomReportQuestionId uniqueidentifier not null,
CustomReportId uniqueidentifier not null,
Id int not null,
QuestionType varchar(200),
[Values] nvarchar(max),
[Options] nvarchar(max),
CreatedDate datetime,
CONSTRAINT CustomReportQuestion_Id PRIMARY KEY (CustomReportQuestionId),
CONSTRAINT Fk_CustomReportQuestion_CustomReport foreign key (CustomReportId) REFERENCES CustomReport(CustomReportId)
)

create table dbo.CustomReportAnswers(
CustomReportId uniqueidentifier not null,
KidId int not null,
CustomReportQuestionId uniqueidentifier not null,
[Day] date,
[InputAnswers] nvarchar(max),
[OptionAnswers] nvarchar(max),
CreatedDate datetime
)

alter table dbo.CustomReportAnswers add EmailSentToParent bit default 0;

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[CreateCustomReportQuestions] (@CustomReportId uniqueidentifier,@DayCareId uniqueidentifier,
@QuestionType varchar(200),@Id int,@Values nvarchar(max)=null,@Options nvarchar(max)=null)
AS
BEGIN	
	SET NOCOUNT ON; 
	IF Not EXISTS (SELECT * FROM dbo.CustomReport WHERE CustomReportId=@CustomReportId)
	BEGIN	
	insert into dbo.CustomReport(CustomReportId,DayCareId,CreatedDate) 	
	values(@CustomReportId,@DayCareId,GETDATE());
	END;
	insert into dbo.CustomReportQuestions(CustomReportQuestionId,CustomReportId,Id,QuestionType,[Values],[Options],CreatedDate) 
	values(newid(),@CustomReportId,@Id,@QuestionType,@Values,@Options,GETDATE());
END

--get the custom report
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetCustomReport] (@DayCareId uniqueidentifier,@Day Date=null,@KidId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.*,c.InputAnswers,c.OptionAnswers from dbo.CustomReport a join CustomReportQuestions b on a.CustomReportId=b.CustomReportId
	left join dbo.CustomReportAnswers c on b.CustomReportquestionId=c.CustomReportquestionId and c.[Day]=@Day and c.KidId=@KidId
	where a.DayCareId=@DayCareId order by b.CreatedDate asc;
END

--get the custom report on a day
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetCustomReportOnADay] (@DayCareId uniqueidentifier,@Day Date=null,@KidId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.*,c.InputAnswers,c.OptionAnswers from dbo.CustomReport a join CustomReportQuestions b on a.CustomReportId=b.CustomReportId
	join dbo.CustomReportAnswers c on b.CustomReportquestionId=c.CustomReportquestionId and c.[Day]=@Day and c.KidId=@KidId
	where a.DayCareId=@DayCareId order by b.CreatedDate asc;
END


--get the custom report
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[SaveCustomReport] (@DayCareId uniqueidentifier,@KidId int,@CustomReportQuestionId uniqueidentifier,
@InputAnswers nvarchar(max)=null,@OptionAnswers nvarchar(max)=null)
AS
BEGIN	
	SET NOCOUNT ON; 		
	declare @customReportId uniqueIdentifier;
	select @customReportId=customReportId from CustomReport where DayCareId=@DayCareId;
	IF Not EXISTS (SELECT * FROM dbo.CustomReportAnswers WHERE CustomReportId=@CustomReportId and KidId=@KidId
	and Day=CAST(GETDATE() AS DATE) and CustomReportQuestionId=@CustomReportQuestionId)
		BEGIN	
		  insert into dbo.CustomReportAnswers(CustomReportId,KidId,CustomReportQuestionId,[Day],InputAnswers,OptionAnswers,
		  CreatedDate) values (@customReportId,@KidId,@CustomReportQuestionId,CAST(GETDATE() AS DATE),@InputAnswers,
		  @OptionAnswers,getdate())
		END
    ELSE
		BEGIN
			update dbo.CustomReportAnswers set InputAnswers=@InputAnswers,OptionAnswers=@OptionAnswers
			where CustomReportId=@CustomReportId and KidId=@KidId
			and Day=CAST(GETDATE() AS DATE) and CustomReportQuestionId=@CustomReportQuestionId
		END
END


Alter FUNCTION KidHasReportToday (@id int, @dayCareId uniqueidentifier)
    RETURNS bit
AS
begin
  declare @HasReportToday bit;  
  declare @CustomReport bit;  
  set @HasReportToday=0;
  select @CustomReport=CustomReport from dbo.Settings where DayCareId=@dayCareId;
  If(@CustomReport=0)
  BEGIN
	if exists(select logid from KidLog where KidId=@id and [day]=CONVERT(date, getdate()))
		begin
		set @HasReportToday=1;
		end
	else
		begin
		set @HasReportToday=0;
		end
  END
  ELSE
  BEGIN
	 if exists( select * from dbo.CustomReport a join CustomReportQuestions b on a.CustomReportId=b.CustomReportId
			join dbo.CustomReportAnswers c on b.CustomReportquestionId=c.CustomReportquestionId and c.[Day]=CONVERT(date, getdate()) and c.KidId=@id
			where a.DayCareId=@dayCareId)
		begin
		set @HasReportToday=1;
		end
	  else
		begin
		set @HasReportToday=0;
		end	
  END
  return @HasReportToday;  	  
end 
go

--get kids
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetKidsFromDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select *,dbo.KidHasReportToday(KidId,@DayCareId) as HasReportToday from Kid where DayCareId=@DayCareId;
END
GO

--get kids from parent
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetKidsFromParent] (@ParentId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	DECLARE @Temp TABLE (idx INT,record int);
	declare @KidIds nvarchar(4000);
	select @KidIds=KidId from Parent where ParentId=@ParentId;
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');	
	select a.*,dbo.KidHasReportToday(a.KidId,(select daycareid from kid where KidId=a.kidId)) 
	as HasReportToday from Kid a join @Temp b on b.record=a.KidId;
END
GO

--get emails for parents for a day care on a day
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetCustomEmailsForParents] (@DayCareId uniqueidentifier, @KidId int)
AS
BEGIN	
    Declare @Email nvarchar(200);
	Declare @Name nvarchar(200);	
	select @Name=name,@Email=Email from dbo.kid where KidId=@KidId;
	select b.*,c.InputAnswers,c.OptionAnswers,@Email as Email,@Name as Name from dbo.CustomReport a join CustomReportQuestions b on a.CustomReportId=b.CustomReportId
	join dbo.CustomReportAnswers c on b.CustomReportquestionId=c.CustomReportquestionId and c.[Day]=CAST(GETDATE() AS DATE) and c.KidId=@KidId
	where a.DayCareId=@DayCareId and c.EmailSentToParent=0 order by b.CreatedDate asc;
	update c set c.EmailSentToParent=1 from dbo.CustomReport a join CustomReportQuestions b on a.CustomReportId=b.CustomReportId
	join dbo.CustomReportAnswers c on b.CustomReportquestionId=c.CustomReportquestionId
	where c.KidId=@KidId and c.[Day]=CAST(GETDATE() AS DATE) and a.DayCareId=@DayCareId;
END
GO


--changes for instant log dated 03/23
--instant log table
create table 
dbo.InstantLog(LogId int identity(1,1) not null,
KidId int not null,
[Day] Date not null,
DateCreated datetime not null,
CONSTRAINT InstantLog_Id PRIMARY KEY (LogId),
CONSTRAINT Fk_Kid_InstantLog foreign key (KidId) REFERENCES Kid(KidId))

--instant messages for log
create table dbo.InstantLogMessages(
MessageId int identity(1,1) not null,
LogId int not null,
Message nvarchar(2000) not null,
Type nvarchar(20) not null,
Time nvarchar(20) not null,
DateCreated datetime not null,
CONSTRAINT Message_Id PRIMARY KEY (MessageId),
CONSTRAINT Fk_InstantLogMessages_InstantLog foreign key (LogId) REFERENCES InstantLog(LogId)
)

--get the isnatnt log and log messages
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetInstantLog] (@KidId int,@Day Date=null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.LogId,b.* from InstantLog a left join InstantLogMessages b on a.LogId=b.LogId
	where a.KidId=@KidId and a.Day = @Day;
END

--insert message for instant log
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertInstantLogMessage] (@KidId int,@LogId int=null,@Message nvarchar(2000),@Type nvarchar(20),@Time nvarchar(20))
AS
BEGIN	
	SET NOCOUNT ON;
	declare @newLogId int;
	declare @newMessageId int; 	
	set @newLogId=@LogId;
	IF Not EXISTS (select * from InstantLog where LogId=@LogId)
	Begin
	  Insert into InstantLog values(@KidId,CAST(GETDATE() AS DATE),GETDATE());
	  set @newLogId=Scope_Identity();
	End
	insert into InstantLogMessages values(@newLogId,@Message,@Type,@Time,GETDATE());
	set @newMessageId=Scope_Identity();
	select @newLogId as LogId,@newMessageId as MessageId;
END

--get day care data
GO
/****** Object:  StoredProcedure [dbo].[GetDayCare]    Script Date: 08/16/2015 22:31:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,c.SettingsVisited,c.CustomReport,c.Language
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId;
END


--************editing deleting, documents, giggles assistant******************************
--modified data
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,b.GuardianName,c.SettingsVisited,c.CustomReport,c.Language,d.ClassName
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId left join dbo.Classes d on b.ClassId=d.ClassId
	join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId;
END

--edit kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateKid] (@Name NVARCHAR(100),@Sex nvarchar(6),@DOB nvarchar(15),
@Address NVARCHAR(500),@GuardianName nvarchar(100),@Email NVARCHAR(200),@Phone NVARCHAR(20),@ClassId uniqueidentifier =null,@ClassName nvarchar(200)=null,
@Allergies NVARCHAR(3000),@DayCareId uniqueidentifier,@KidId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	update kid set Name=@Name,Sex=@Sex,DOB=@DOB,Address=@Address,GuardianName=@GuardianName,
	Email=@Email,Phone=@Phone,Allergies=@Allergies,ClassId=@ClassId,DateModified=GETDATE()
	where KidId=@KidId and DayCareId=@DayCareId;
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END


Alter table dbo.kid add Active bit not null default 1;

Alter table dbo.kid add InActiveReason nvarchar(100) null default Null;

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[DeleteKid] (@KidId int,@DayCareId uniqueidentifier,@InActiveReason nvarchar(100)=null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	IF Not EXISTS (SELECT * FROM dbo.Kid WHERE KidId=@KidId and DayCareId=@DayCareId)
	BEGIN
        RAISERROR ('The kid doesnt belong to your day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	    Update Kid set Active=0,InActiveReason=@InActiveReason where KidId=@KidId and DayCareId=@DayCareId;   	 
	END
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,b.GuardianName,c.SettingsVisited,c.CustomReport,c.Language,d.ClassName
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId left join dbo.Classes d on b.ClassId=d.ClassId
	join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId and b.Active=1;
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetKidsFromDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select *,dbo.KidHasReportToday(KidId,@DayCareId) as HasReportToday from Kid where DayCareId=@DayCareId
	and Active=1;
END


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetAttendance] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 
	DECLARE @MyTempTable table (Name nvarchar(200),KidId int,Attended bit);
	insert into @MyTempTable(Name,KidId,Attended)
	SELECT Name,KidId,null FROM Kid 
	WHERE DayCareId=@DayCareId and Active=1;
	update temp set temp.Attended = a.Attended from @MyTempTable temp
	join Attendance a on a.KidId=temp.KidId  
	where a.DayCareId=@DayCareId and a.DAY=CAST(GETDATE() AS DATE); 
	select @DayCareId as DayCareId,* from @MyTempTable;
END


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetKidsInClass] (@ClassId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select k.* from Classes c join Kid k on c.ClassId=k.ClassId where c.ClassId=@ClassId and k.Active=1;
END

--notifications
Create table LU_Notifications
([Id] [int] Not Null,
[Type] nvarchar(30),
[Desc] nvarchar(2000),
CONSTRAINT [LU_Notifications_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

--insert look up notifications
insert into LU_Notifications values(1,'Report Missing','Some Kids are missing their reports today');
insert into LU_Notifications values(2,'Attendance Missing','Some Kids are missing their attendance today');
insert into LU_Notifications values(3,'2nd Month Vaccines','is ready for 2nd Month Vaccines: Hepatitis B, 
1st dose of Rotavirus, 1st dose of Diphtheria, tetanus, & acellular pertussis (DTaP), 1st dose of Haemophilus influenzae type b4(Hib)
, 1st dose of Pneumococcal conjugate5(PCV13), 1st dose of Inactivated poliovirus6(IPV)');
insert into LU_Notifications values(4,'4th Month Vaccines','is ready for 4th Month Vaccines:
2nd dose of Rotavirus, 2nd dose of Diphtheria, tetanus, & acellular pertussis (DTaP), 2nd dose of Haemophilus influenzae type b4(Hib)
, 2nd dose of Pneumococcal conjugate5(PCV13), 2nd dose of Inactivated poliovirus6(IPV)');
insert into LU_Notifications values(5,'6th Month Vaccines','is ready for 6th Month Vaccines:
3rd dose of Diphtheria, tetanus, & acellular pertussis (DTaP), 3rd dose of Pneumococcal conjugate5(PCV13), 3rd dose of Inactivated poliovirus6(IPV)');


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Notifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LU_Not_Id] int not null,
	[KidId] int null default null,
	[Notification] nvarchar(3000) Not Null,
	[Read] bit Not Null,
	[DayCareId] [uniqueidentifier] Not Null,
	[Day] [date] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [Notifications_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [Fk_DayCare_Notifications] FOREIGN KEY([DayCareId])
REFERENCES [dbo].[DayCare] ([DayCareId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [Fk_DayCare_Notifications]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [Fk_LU_Not_Notifications] FOREIGN KEY([LU_Not_Id])
REFERENCES [dbo].[LU_Notifications] ([Id])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [Fk_LU_Not_Notifications]
GO

ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [Fk_Kid_Notifications] FOREIGN KEY([KidId])
REFERENCES [dbo].[Kid] ([KidId])
GO

ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [Fk_Kid_Notifications]
GO

--function to check attendance
Create FUNCTION KidHasAttendanceToday (@id int, @dayCareId uniqueidentifier)
    RETURNS bit
AS
begin
  declare @HasAttenToday bit;
  set @HasAttenToday=0;
	if exists(select * from dbo.Attendance where KidId=@id and [day]=CONVERT(date, getdate()))
		begin
		set @HasAttenToday=1;
		end
	else
		begin
		set @HasAttenToday=0;
		end    
  return @HasAttenToday;  	  
end 
go



--stored proc for the notifications
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetNotifications] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 		
	Declare @ReportNotiBuf bit;
	Declare @AttendanceNotifBuf bit;
	Declare @BirthDate nvarchar(30);
	Declare @KidName nvarchar(200);
	Declare @KidId int;
	Declare @Months int;
	DECLARE @NotifCursor as CURSOR;	
	IF exists(select * from dbo.Notifications where DayCareId=@DayCareId and [day]=CONVERT(date, getdate()))
	  Begin
	    select * from dbo.Notifications where DayCareId=@DayCareId and ([day]=CONVERT(date, getdate()) or LU_Not_Id=3 or LU_Not_Id=4 or LU_Not_Id=5)
		and [Read]=0;
	  End
    Else
	  Begin	    
		 SET @NotifCursor = CURSOR FOR
			select DOB,Name,KidId,dbo.KidHasReportToday(KidId,@DayCareId),
			dbo.KidHasAttendanceToday(KidId,@DayCareId) from Kid where DayCareId=@DayCareId
		    and Active=1; 
         OPEN @NotifCursor;
         FETCH NEXT FROM @NotifCursor INTO @BirthDate,@KidName,@KidId,@ReportNotiBuf, @AttendanceNotifBuf; 
	     WHILE @@FETCH_STATUS = 0
		   BEGIN
		     --logic goes here
			 If not exists(select * from dbo.Notifications where DayCareId=@DayCareId and [day]=CONVERT(date, getdate()) and LU_Not_Id=1)
			   Begin			     
				 Insert into dbo.Notifications values(1,null,'Some Kids are missing their reports today',0,@DayCareId,CONVERT(date, getdate()),GETDATE());				 
			   End
             If not exists(select * from dbo.Notifications where DayCareId=@DayCareId and [day]=CONVERT(date, getdate()) and LU_Not_Id=2)
			   Begin			     
				 Insert into dbo.Notifications values(2,null,'Some Kids are missing their attendance today',0,@DayCareId,CONVERT(date, getdate()),GETDATE());				 
			   End
			 select @Months=DATEDIFF(month,CONVERT(Date, @BirthDate, 6),CONVERT(date, getdate()));			 
			 if((@Months=2) and not exists(select * from dbo.Notifications where DayCareId=@DayCareId and KidId=@KidId and LU_Not_Id=3))
			   begin
			     Insert into dbo.Notifications(LU_Not_Id,KidId,[Notification],[Read],DayCareId,[Day],DateCreated)
				 select Id,@KidId,@KidName+' '+[Desc],0,@DayCareId,CONVERT(date, getdate()),GETDATE() from dbo.LU_Notifications where Id=3;
			   end 
			 if((@Months=4) and not exists(select * from dbo.Notifications where DayCareId=@DayCareId and KidId=@KidId and LU_Not_Id=4))
			   begin
			     Insert into dbo.Notifications(LU_Not_Id,KidId,[Notification],[Read],DayCareId,[Day],DateCreated)
				 select Id,@KidId,@KidName+' '+[Desc],0,@DayCareId,CONVERT(date, getdate()),GETDATE() from dbo.LU_Notifications where Id=4;
			   end 
			 if((@Months=6) and not exists(select * from dbo.Notifications where DayCareId=@DayCareId and KidId=@KidId and LU_Not_Id=5))
			   begin
			     Insert into dbo.Notifications(LU_Not_Id,KidId,[Notification],[Read],DayCareId,[Day],DateCreated)
				 select Id,@KidId,@KidName+' '+[Desc],0,@DayCareId,CONVERT(date, getdate()),GETDATE() from dbo.LU_Notifications where Id=5;
			   end       
			 FETCH NEXT FROM @NotifCursor INTO @BirthDate,@KidName,@KidId,@ReportNotiBuf, @AttendanceNotifBuf; 
		   END 
		 CLOSE @NotifCursor;
	     DEALLOCATE @NotifCursor;  
		 select * from dbo.Notifications where DayCareId=@DayCareId and [day]=CONVERT(date, getdate())
		 and [Read]=0;  	    
	  End		
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[MarkNotitificationRead] (@DayCareId uniqueidentifier, @Id int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	Update dbo.Notifications set [Read]=1 where Id=@Id and DayCareId=@DayCareId; 
END

--check kid by name
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetKidByName] (@DayCareId uniqueidentifier, @Name nvarchar(200))
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from dbo.kid where DayCareId=@DayCareId and DIFFERENCE(Name,@Name)>3;
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--create table for documents
CREATE TABLE [dbo].[Documents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] nvarchar(500) Not Null,
	[Type] nvarchar(20) Not Null,
	[MimeType] varchar(1000) Not Null,
	[Title] nvarchar(500) Not Null,
	[DayCareId] [uniqueidentifier] Not Null,	
	[Active] bit Not Null default 1,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [Documents_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [Fk_DayCare_Documents] FOREIGN KEY([DayCareId])
REFERENCES [dbo].[DayCare] ([DayCareId])
GO

ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [Fk_DayCare_Documents]
GO

--get all documents
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetDocuments] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from dbo.Documents where DayCareId=@DayCareId and Active=1;
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--get document
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetDocument] (@Id int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select * from dbo.Documents where Id=@Id and Active=1;
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--insert document
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[InsertDocument] (@DayCareId uniqueidentifier,@Name nvarchar(500),@Title nvarchar(500),
@Type nvarchar(20),@MimeType varchar(1000))
AS
BEGIN	
	SET NOCOUNT ON; 	
	Insert into dbo.Documents values(@Name,@Type,@MimeType,@Title,@DayCareId,1,GETDATE());
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--delete document
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[DeleteDocument] (@Id int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	update dbo.Documents set Active=0 where Id=@Id;
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--for alexa

use daycare

create table Alexa_Session(
RequestId varchar(1000),
SessionId varchar(1000),
RequestType varchar(200),
Intent varchar(max),
DayCareId uniqueidentifier,
DateCreated datetime
)

--make alexa session insert
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[LogAlexaSession] (@DayCareId uniqueidentifier,
@RequestId varchar(1000),@SessionId varchar(1000),@RequestType varchar(200),@Intent varchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	insert into Alexa_Session values(@RequestId,@SessionId,@RequestType,@Intent,@DayCareId,GETDATE());
END
GO

--check kid by name
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetParentsKidByName] (@ParentId uniqueidentifier, @Name nvarchar(200))
AS
BEGIN	
	SET NOCOUNT ON; 	
	DECLARE @Temp TABLE (idx INT,record int);
	declare @KidIds nvarchar(4000);
	select @KidIds=KidId from Parent where ParentId=@ParentId;
	insert into @Temp(idx,record) select idx,record from _BuildTable(@KidIds,',');	
	select a.* from dbo.Kid a join @Temp b on b.record=a.KidId where DIFFERENCE(a.Name,@Name)>3;	
	--select * from dbo.kid where DayCareId=@DayCareId and DIFFERENCE(Name,@Name)>3;
END

--add trace to errorlogs
alter table errorlogs add trace varchar(max)

--insert short form for the kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertKidShort] (@Name NVARCHAR(100),@DayCareId uniqueidentifier,@ClassId uniqueidentifier=null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	INSERT INTO dbo.Kid
	        ( Name,
	          Sex,
	          DOB,
	          [Address],
	          GuardianName,
	          ClassId,
	          Email,
	          Phone,
	          Allergies ,
	          DayCareId ,	          
	          DateCreated	          
	        )
		VALUES  ( @Name , -- UserName - nvarchar(200)
	          'n/a' , -- Email - nvarchar(200)
	          'n/a',
	          'n/a' , -- Phone - nvarchar(20)
	          'n/a',
	          @ClassId,
	          'n/a' , -- StreetAddress - nvarchar(300)
	          'n/a' , -- City - nvarchar(100)
	          'n/a' , -- PinCode - nvarchar(20)
	          @DayCareId,	          
	          GETDATE()
	        );
SELECT KidId FROM dbo.Kid WHERE Email='n/a' and Name=@Name and DayCareId=@DayCareId and Active=1;      	 
END

--create table for alexa token storage

use daycare 

create table Alexa_Token(
Token nvarchar(max),
Id uniqueidentifier,
[Role] varchar(30),
DateCreated datetime
)

--sp to insert alexa token
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertAlexaToken] (@Token NVARCHAR(Max),@Id uniqueidentifier,@Role varchar(30))
AS
BEGIN	
	SET NOCOUNT ON;
	IF EXISTS (SELECT * FROM dbo.Alexa_Token WHERE Token=@Token and Id=@Id)
	Begin
	  Delete from dbo.Alexa_Token where Token=@Token and Id=@Id;
	End	 	
	Insert into dbo.Alexa_Token values(@Token,@Id,@Role,GETDATE());	
END

--sp to check the alexa token
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetAlexaToken] (@Token NVARCHAR(Max))
AS
BEGIN	
	SET NOCOUNT ON;
	select * from dbo.Alexa_Token where Token=@Token;
END


--insert short form for the kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[InsertKidShort] (@Name NVARCHAR(100),@DayCareId uniqueidentifier,@ClassId uniqueidentifier=null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	INSERT INTO dbo.Kid
	        ( Name,
	          Sex,
	          DOB,
	          [Address],
	          GuardianName,
	          ClassId,
	          Email,
	          Phone,
	          Allergies ,
	          DayCareId ,	          
	          DateCreated	          
	        )
		VALUES  ( @Name , -- UserName - nvarchar(200)
	          'n/a' , -- Email - nvarchar(200)
	          'n/a',
	          'n/a' , -- Phone - nvarchar(20)
	          'n/a',
	          @ClassId,
	          'n/a' , -- StreetAddress - nvarchar(300)
	          'n/a' , -- City - nvarchar(100)
	          'n/a' , -- PinCode - nvarchar(20)
	          @DayCareId,	          
	          GETDATE()
	        );
SELECT KidId FROM dbo.Kid WHERE Email='n/a' and Name=@Name and DayCareId=@DayCareId and Active=1;      	 
END

--table for app version warnings
create table AppVersion(OsType varchar(10),Version varchar(10));
insert into AppVersion values('ios','1.0.12');
insert into AppVersion values('android','1.0.12')

--sp to check the app version for warning
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetAppVersion] (@OsType varchar(10))
AS
BEGIN	
	SET NOCOUNT ON;
	select * from dbo.appversion where OsType=@OsType;
END


--get active kids in class
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetKidsInClass] (@ClassId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select k.* from Classes c join Kid k on c.ClassId=k.ClassId where c.ClassId=@ClassId and k.Active=1;
END
GO

--get active kids without classes
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
alter PROCEDURE [dbo].[GetKidsWithNoClass] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.KidId,b.Name from Kid b where b.DayCareId=@DayCareId and b.ClassId is null and b.Active=1;	
END
GO

alter table dbo.settings add EmailOnRegisterKid  bit not null default 1

--handle email on register kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SaveSettings] (@DayCareId uniqueidentifier,@CustomReport bit,@GigglesWareReport bit, @MakePublic bit,@SettingsVisited bit,@Language nvarchar(20),@EmailOnRegisterKid bit=1)
AS
BEGIN	
	SET NOCOUNT ON; 	
	update dbo.Settings set CustomReport=@CustomReport, GigglesWareReport=@GigglesWareReport, MakePublic=@MakePublic, SettingsVisited=@SettingsVisited, Language=@Language, EmailOnRegisterKid=@EmailOnRegisterKid
	where DayCareId=@DayCareId;
END

--alter get day care get new column
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE()
	        );	
    Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic,[Language],EmailOnRegisterKid)
	SELECT MyTempId as DayCareId,0,0,1,0,'en-us',1 FROM @MyTempTable;
	        	      
	SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--get the day care data with new settings
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,b.GuardianName,c.SettingsVisited,c.CustomReport,c.Language,c.EmailOnRegisterKid,d.ClassName
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId left join dbo.Classes d on b.ClassId=d.ClassId
	join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId and b.Active=1;
END

SET ANSI_NULLS ON

--handle source for daycare web or mobile
alter table daycare add Source varchar(20) null default null;

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20),@Source varchar(20)='web')
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated,
			  Source	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE(),
			  @Source
	        );	
    Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic,[Language],EmailOnRegisterKid)
	SELECT MyTempId as DayCareId,0,0,1,0,'en-us',1 FROM @MyTempTable;
	        	      
	SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--add whats new cards for mobile apps
create table LU_WhatsNew(Id int not null,Heading varchar(500),Details varchar(3000),ImagePath varchar(500),DateCreated datetime,
CONSTRAINT [LU_WhatsNew_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[WhatsNew](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LU_WhatsNew_Id] int not null,	
	[Read] bit Not Null,
	[DayCareId] [uniqueidentifier] Not Null,	
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [WhatsNew_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WhatsNew]  WITH CHECK ADD  CONSTRAINT [Fk_DayCare_WhatsNew] FOREIGN KEY([DayCareId])
REFERENCES [dbo].[DayCare] ([DayCareId])
GO

ALTER TABLE [dbo].[WhatsNew] CHECK CONSTRAINT [Fk_DayCare_WhatsNew]
GO

ALTER TABLE [dbo].[WhatsNew]  WITH CHECK ADD  CONSTRAINT [Fk_LU_WhatsNew_WhatsNew] FOREIGN KEY([LU_WhatsNew_Id])
REFERENCES [dbo].[LU_WhatsNew] ([Id])
GO

ALTER TABLE [dbo].[WhatsNew] CHECK CONSTRAINT [Fk_LU_WhatsNew_WhatsNew]
GO

--sp to get whats new
--stored proc for the notifications
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetWhatsNew] (@DayCareId uniqueidentifier)
AS
BEGIN	
	DECLARE @WhtsNewCursor as CURSOR;
	Declare @LU_WhatsNew_Id int;
	SET @WhtsNewCursor = CURSOR FOR
			select Id from dbo.Lu_WhatsNew;			 		    
         OPEN @WhtsNewCursor;	
		 FETCH NEXT FROM @WhtsNewCursor INTO @LU_WhatsNew_Id; 
		 WHILE @@FETCH_STATUS = 0
		   BEGIN
		   print @LU_WhatsNew_Id;
		    If not exists(select * from dbo.WhatsNew where DayCareId=@DayCareId and LU_WhatsNew_Id=@LU_WhatsNew_Id)
			 BEGIN
			   Insert into dbo.WhatsNew values(@LU_WhatsNew_Id,0,@DayCareId,getdate());
			 END
			FETCH NEXT FROM @WhtsNewCursor INTO @LU_WhatsNew_Id;
		   END
         CLOSE @WhtsNewCursor;
	     DEALLOCATE @WhtsNewCursor; 
		 select w.Id,l.Heading,l.Details,l.ImagePath from dbo.WhatsNew w join Lu_WhatsNew l on w.LU_WhatsNew_Id=l.Id
		 where DayCareId=@DayCareId and [Read]=0; 
END

--MARK NEWS AS READ
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[MarkNewsRead] (@DayCareId uniqueidentifier, @Id int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	Update dbo.WhatsNew set [Read]=1 where Id=@Id and DayCareId=@DayCareId; 
END


--insert ads for whatsnew
insert into LU_WhatsNew values(1,'Alexa','get alexa now and use your voice to rock','https://gigglesware.com/images/Assistant-Small.png',GETDATE());
insert into LU_WhatsNew values(2,'google home','get google home now and use your voice to rock','https://gigglesware.com/images/Assistant-Small.png',GETDATE())

--change the get daycare to handle 0 kids
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,b.GuardianName,c.SettingsVisited,c.CustomReport,c.Language,c.EmailOnRegisterKid,d.ClassName
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId and b.Active=1 left join dbo.Classes d on b.ClassId=d.ClassId
	join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId;
END

--avatars for kids and classes
alter table dbo.kid add Avatar varchar(300) null default null;

--add avatar to insert kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertKid] (@Name NVARCHAR(100),@Sex nvarchar(6),@DOB nvarchar(15),
@Address NVARCHAR(500),@GuardianName nvarchar(100),@Email NVARCHAR(200),@Phone NVARCHAR(20),@ClassId uniqueidentifier =null,@ClassName nvarchar(200)=null,
@Allergies NVARCHAR(3000),@DayCareId uniqueidentifier,@Avatar varchar(300)=null)
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	IF EXISTS (SELECT * FROM dbo.Kid WHERE Email=@Email and Name=@Name)
	BEGIN
        RAISERROR ('The kid is already registred under the email.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN	    
	    If(@ClassId is null and (@ClassName!='' and @ClassName is not null))
	     begin
	       set @ClassId=NEWID();
	       insert into Classes values(@ClassId,@ClassName,@DayCareId);
	     end	     
		INSERT INTO dbo.Kid
	        ( Name,
	          Sex,
	          DOB,
	          [Address],
	          GuardianName,
	          ClassId,
	          Email,
	          Phone,
	          Allergies ,
	          DayCareId ,	          
	          DateCreated,
			  Avatar	          
	        )
		VALUES  ( @Name , -- UserName - nvarchar(200)
	          @Sex , -- Email - nvarchar(200)
	          @DOB,
	          @Address , -- Phone - nvarchar(20)
	          @GuardianName,
	          @ClassId,
	          @Email , -- StreetAddress - nvarchar(300)
	          @Phone , -- City - nvarchar(100)
	          @Allergies , -- PinCode - nvarchar(20)
	          @DayCareId,	          
	          GETDATE(),
			  @Avatar
	        );
		SELECT KidId,Email FROM dbo.Kid WHERE Email=@Email and Name=@Name and DayCareId=@DayCareId;      	 
	END
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--get avatar for daycare
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCare] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select a.DayCareName,a.DayCareId,a.Email,b.KidId,b.Name,b.Phone,b.Email,b.Sex,b.DOB,b.Address,b.Allergies,b.GuardianName,b.Avatar,c.SettingsVisited,c.CustomReport,c.Language,c.EmailOnRegisterKid,d.ClassName
	from DayCare a left join Kid b on a.DayCareId=b.DayCareId and b.Active=1 left join dbo.Classes d on b.ClassId=d.ClassId
	join dbo.Settings c on a.DayCareId=c.DayCareId
	where a.DayCareId=@DayCareId;
END

--update attendance
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetAttendance] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 
	DECLARE @MyTempTable table (Name nvarchar(200),KidId int,Attended bit,Avatar varchar(300),Sex nvarchar(12));
	insert into @MyTempTable(Name,KidId,Attended,Avatar,Sex)
	SELECT Name,KidId,null,Avatar,Sex FROM Kid 
	WHERE DayCareId=@DayCareId and Active=1;
	update temp set temp.Attended = a.Attended from @MyTempTable temp
	join Attendance a on a.KidId=temp.KidId  
	where a.DayCareId=@DayCareId and a.DAY=CAST(GETDATE() AS DATE); 
	select @DayCareId as DayCareId,* from @MyTempTable;
END

--avatar for kids with no class
GO
/****** Object:  StoredProcedure [dbo].[GetKidsWithNoClass]    Script Date: 10/13/2016 2:08:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetKidsWithNoClass] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select b.KidId,b.Name,b.Avatar,b.Sex from Kid b where b.DayCareId=@DayCareId and b.ClassId is null and b.Active=1;	
END


SET ANSI_NULLS ON

--Schedules
CREATE TABLE [dbo].[Schedule](
	[ScheduleId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](200) NOT NULL,
	[DayCareId] [uniqueidentifier] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [Schedule_Id] PRIMARY KEY CLUSTERED 
(
	[ScheduleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [Fk_DayCareId_Schedule] FOREIGN KEY([DayCareId])
REFERENCES [dbo].[DayCare] ([DayCareId])
GO

ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [Fk_DayCareId_Schedule]
GO

--schedule messages
CREATE TABLE [dbo].[ScheduleMessages](
	[MessageId] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleId] [int] NOT NULL,
	[Time] [nvarchar](20) NOT NULL,
	[Message] [nvarchar](2000) NOT NULL,	
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [ScheduleMessage_Id] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ScheduleMessages]  WITH CHECK ADD  CONSTRAINT [Fk_ScheduleMessages_Schedule] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([ScheduleId])
GO

ALTER TABLE [dbo].[ScheduleMessages] CHECK CONSTRAINT [Fk_ScheduleMessages_Schedule]
GO

--get schedules
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetSchedules] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select @DayCareId as DayCareId,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId order by s.ScheduleId;
END

--get schedule by scheduleid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetSchedule] (@DayCareId uniqueidentifier,@ScheduleId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select @DayCareId as DayCareId,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId and s.ScheduleId=@ScheduleId;
END

--insert schedule
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertSchedule] (@DayCareId uniqueidentifier,@Name varchar(200),@Message nvarchar(max),@Time varchar(max))
AS
BEGIN	
	SET NOCOUNT ON;		
	DECLARE @Messages TABLE (idx INT,record varchar(2000));
	DECLARE @Times TABLE (idx INT,record varchar(20));
	DECLARE @MyTempTable table (MyTempId int);	
	Declare @ScheduleId int;
	Insert into dbo.Schedule(Name,DayCareId,DateCreated)OUTPUT inserted.ScheduleId INTO @MyTempTable
	values(@Name,@DayCareId,GETDATE());
	select @ScheduleId = MyTempId from @MyTempTable;
	insert into @Messages(idx,record) select idx,record from _BuildTable(@Message,'^');	
	insert into @Times(idx,record) select idx,record from _BuildTable(@Time,'^');
	insert into dbo.ScheduleMessages(ScheduleId,Time,Message,DateCreated) 
	select @ScheduleId,a.record,b.record,GETDATE() from @Times a join @Messages b on a.idx=b.idx;
	select @DayCareId as DayCareId,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId and s.ScheduleId=@ScheduleId;
END

--insert shcedule message
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[SaveScheduleMessage] (@DayCareId uniqueidentifier,@MessageId int,@Time varchar(20),@Message varchar(2000))
AS
BEGIN	
	SET NOCOUNT ON; 	
	update m set m.Time=@Time,m.Message=@Message From dbo.DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId
	Join dbo.ScheduleMessages m on s.ScheduleId=m.ScheduleId
	where d.DayCareId=@DayCareId and m.MessageId=@MessageId;
END

--remove shcedule message
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[RemoveScheduleMessage] (@DayCareId uniqueidentifier,@MessageId int,@ScheduleId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	Delete m From dbo.DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId
	Join dbo.ScheduleMessages m on s.ScheduleId=m.ScheduleId
	where d.DayCareId=@DayCareId and m.MessageId=@MessageId and s.ScheduleId=@ScheduleId;
END

--delete shcedule
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[RemoveSchedule] (@DayCareId uniqueidentifier,@ScheduleId int)
AS
BEGIN	
	SET NOCOUNT ON;
	begin try
	   IF EXISTS (SELECT * FROM dbo.Schedule WHERE DayCareId=@DayCareId and ScheduleId=@ScheduleId)
	   Begin	  
	     Delete from dbo.ScheduleMessages where ScheduleId=@ScheduleId;
	     Delete from dbo.Schedule where DayCareId=@DayCareId and ScheduleId=@ScheduleId;
	   End
	   Else
	   Begin
	     RAISERROR ('The daycare id or schedule id is wrong.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
	   End
    end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--add unique name for daycares

alter table dbo.DayCare add DayCareUniqueName nvarchar(400) not null default '';

update dbo.DayCare set DayCareUniqueName=DayCareName;

--change the insert sp to accomodate the uniquename for the daycare
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertDayCareAdmin] (@DayCareName NVARCHAR(200),@Address nvarchar(500),
@Phone NVARCHAR(20),@AdminName NVARCHAR(100),@Email NVARCHAR(200),@Password NVARCHAR(20),@Source varchar(20)='web')
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try
	begin transaction
	IF EXISTS (SELECT * FROM dbo.DayCare WHERE Email=@Email)
	BEGIN
        RAISERROR ('You are already registered as a day care.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
    END    
	ELSE    
	BEGIN
	DECLARE @MyTempTable table (MyTempId uniqueidentifier);	
	Declare @Counter int; set @counter=2;
	Declare @UniqueName nvarchar(200);
	Set @UniqueName = @DayCareName;
	While(EXISTS (SELECT * FROM dbo.DayCare WHERE DayCareName=@UniqueName))
	  Begin
	    set @UniqueName=@DayCareName+CAST(@Counter as nvarchar(20));
		set @counter=@counter+1;
	  End
	INSERT INTO dbo.DayCare
	        ( DayCareName ,
	          [Address] ,
	          Phone ,
	          AdminName ,
	          Email ,
	          [Password] ,
	          DateCreated,
			  Source,
			  DayCareUniqueName	          
	        )OUTPUT inserted.DayCareId INTO @MyTempTable
	VALUES  ( @DayCareName , -- UserName - nvarchar(200)
	          @Address , -- Email - nvarchar(200)
	          @Phone , -- Phone - nvarchar(20)
	          @AdminName , -- StreetAddress - nvarchar(300)
	          @Email , -- City - nvarchar(100)
	          @Password , -- PinCode - nvarchar(20)	          
	          GETDATE(),
			  @Source,
			  @UniqueName
	        );	
    Insert into dbo.Settings(DayCareId,SettingsVisited,CustomReport,GigglesWareReport,MakePublic,[Language],EmailOnRegisterKid)
	SELECT MyTempId as DayCareId,0,0,1,0,'en-us',1 FROM @MyTempTable;
	        	      
	SELECT MyTempId as DayCareId FROM @MyTempTable;        	     
	END
	COMMIT
	end try
	begin catch
		IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--daycare additional info
Create table DayCareInfo(DayCareId uniqueidentifier not null,DescriptionHome nvarchar(max) null
,DescriptionAboutUs nvarchar(max) null
,DescriptionProgram nvarchar(max) null
,Logo varchar(500) null,
DateCreated datetime null,DateModified Datetime null)

CREATE TABLE [dbo].[DayCareSnaps](
	[SnapId] [int] IDENTITY(1,1) NOT NULL,
	[DayCareId] [uniqueidentifier] NOT NULL,
	[SnapTitle] [nvarchar](200) NOT NULL,	
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [DayCareSnaps_Id] PRIMARY KEY CLUSTERED 
(
	[SnapId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[DayCareSnaps]  WITH CHECK ADD  CONSTRAINT [Fk_DayCareSnaps_DayCare] FOREIGN KEY([DayCareId])
REFERENCES [dbo].[DayCare] ([DayCareId])
GO

ALTER TABLE [dbo].[DayCareSnaps] CHECK CONSTRAINT [Fk_DayCareSnaps_DayCare]
GO

--stored procs to get the description and the snaps
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[GetDayCareInfo] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select d.DayCareName,i.DescriptionHome,i.DescriptionAboutUs,i.DescriptionProgram,i.Logo,s.SnapId,s.SnapTitle
	from dbo.DayCare d Join dbo.DayCareInfo i on d.DayCareId=i.DayCareId
	left join dbo.DayCareSnaps s on d.DayCareId=s.DayCareId where d.DayCareId=@DayCareId;
END

--manage day care info
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[ManageDayCareInfo] (@DayCareId uniqueidentifier,@Home nvarchar(max),@AboutUs nvarchar(max),@Program nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.DayCareInfo WHERE DayCareId=@DayCareId)
	 Begin
	   Update dbo.DayCareInfo set DescriptionHome=@Home,DescriptionAboutUs=@AboutUs,DescriptionProgram=@Program,DateModified=GETDATE()
	   where DayCareId=@DayCareId; 
	 End
	Else
	 Begin
	   Insert into dbo.DayCareInfo values(@DayCareId,@Home,@AboutUs,@Program,null,GETDATE(),null);
	 End 
END

--insert snap info
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[InsertSnapInfo] (@DayCareId uniqueidentifier,@SnapTitle nvarchar(400))
AS
BEGIN	
	SET NOCOUNT ON;
	begin try
	 Declare @count int;
	 select @count=count(*) from dbo.DayCareSnaps where daycareid=@DayCareId;	 
	 if(@count=5)
	  Begin
	    RAISERROR ('Maximum five snaps allowed.', -- Message text.
               16, -- Severity.
               1 -- State.
               );
	  End
     Else
	  Begin
	    DECLARE @MyTempTable table (MyTempId int);
	    Insert into dbo.DayCareSnaps(DayCareId,SnapTitle,DateCreated)OUTPUT inserted.SnapId INTO @MyTempTable
	    values(@DayCareId,@SnapTitle,GETDATE());
	    select MyTempId as SnapId from @MyTempTable;
      End
    end try
    begin catch
	IF (@@TRANCOUNT > 0)		
			ROLLBACK;		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--insert document unique name
alter table dbo.Documents add UniqueName nvarchar(1020) not null default '';
update dbo.Documents set UniqueName=Name;

--handle unique name for documents
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[InsertDocument] (@DayCareId uniqueidentifier,@Name nvarchar(500),@Title nvarchar(500),
@Type nvarchar(20),@MimeType varchar(1000))
AS
BEGIN	
	SET NOCOUNT ON; 	
	Declare @UniqueName nvarchar(1020);
	Declare @Counter int; set @counter=2;
	Set @UniqueName = @Name;
	While(EXISTS (SELECT * FROM dbo.Documents WHERE Name=@UniqueName))
	  Begin
	    set @UniqueName=@Name+CAST(@Counter as nvarchar(20));
		set @counter=@counter+1;
	  End
	Insert into dbo.Documents values(@Name,@Type,@MimeType,@Title,@DayCareId,1,GETDATE(),@UniqueName);
END
SET ANSI_NULLS ON

--insert schedule unique name
alter table dbo.schedule add UniqueName varchar(220) not null default '';
update dbo.schedule set UniqueName=Name;

--handle unique name for schedules
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
	select @DayCareId as DayCareId,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId and s.ScheduleId=@ScheduleId;
END

--public contacts for daycare
Create table PublicContact(DayCareId uniqueidentifier not null,Name nvarchar(200) not null,
Email varchar(200) not null,Phone varchar(20) not null,Comments nvarchar(3000) not null);

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[InsertPublicContact] (@DayCareId uniqueidentifier,@Name nvarchar(200),@Email varchar(200),@Phone varchar(20),
@Comments nvarchar(3000))
AS
BEGIN	
	SET NOCOUNT ON; 		
	Insert into dbo.PublicContact
	values(@DayCareId,@Name,@Email,@Phone,
	@Comments);	
END

--include daycare name
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetSettings] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select s.*,d.DayCareName from dbo.Settings s join dbo.DayCare d on s.DayCareId=d.DayCareId
	where s.DayCareId=@DayCareId;
	update dbo.Settings set SettingsVisited=1 where DayCareId=@DayCareId;
END

--handle avatar with update kid
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[UpdateKid] (@Name NVARCHAR(100),@Sex nvarchar(6),@DOB nvarchar(15),
@Address NVARCHAR(500),@GuardianName nvarchar(100),@Email NVARCHAR(200),@Phone NVARCHAR(20),@ClassId uniqueidentifier =null,@ClassName nvarchar(200)=null,
@Allergies NVARCHAR(3000),@DayCareId uniqueidentifier,@KidId int,@Avatar varchar(300))
AS
BEGIN	
	SET NOCOUNT ON; 	
	begin try	
	update kid set Name=@Name,Sex=@Sex,DOB=@DOB,Address=@Address,GuardianName=@GuardianName,
	Email=@Email,Phone=@Phone,Allergies=@Allergies,ClassId=@ClassId,DateModified=GETDATE(),
	Avatar=@Avatar
	where KidId=@KidId and DayCareId=@DayCareId;
	end try
	begin catch		
		-- Raise an error with the details of the exception
		DECLARE @ErrMsg nvarchar(4000), @ErrSeverity int
		SELECT @ErrMsg = ERROR_MESSAGE(),
         @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)
	end catch
END

--get daycarename for schedule url
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetSchedules] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select @DayCareId as DayCareId,d.DayCareName,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId order by s.ScheduleId;
END

--get daycarename for schedule url
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetSchedule] (@DayCareId uniqueidentifier,@ScheduleId int)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select @DayCareId as DayCareId,d.DayCareName,s.Name,s.ScheduleId,m.MessageId,m.Time,m.Message from DayCare d Join dbo.Schedule s on d.DayCareId=s.DayCareId Join dbo.ScheduleMessages m
	on s.ScheduleId=m.ScheduleId where d.DayCareId=@DayCareId and s.ScheduleId=@ScheduleId;
END

--manage day care info home
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[ManageDayCareInfoHome] (@DayCareId uniqueidentifier,@Home nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.DayCareInfo WHERE DayCareId=@DayCareId)
	 Begin
	   Update dbo.DayCareInfo set DescriptionHome=@Home,DateModified=GETDATE()
	   where DayCareId=@DayCareId; 
	 End
	Else
	 Begin
	   Insert into dbo.DayCareInfo values(@DayCareId,@Home,'','',null,GETDATE(),null);
	 End 
END

--manage day care info about
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[ManageDayCareInfoAbout] (@DayCareId uniqueidentifier,@AboutUs nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.DayCareInfo WHERE DayCareId=@DayCareId)
	 Begin
	   Update dbo.DayCareInfo set DescriptionAboutUs=@AboutUs,DateModified=GETDATE()
	   where DayCareId=@DayCareId; 
	 End
	Else
	 Begin
	   Insert into dbo.DayCareInfo values(@DayCareId,'',@AboutUs,'',null,GETDATE(),null);
	 End 
END

--manage day care info program
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[ManageDayCareInfoProgram] (@DayCareId uniqueidentifier,@Program nvarchar(max))
AS
BEGIN	
	SET NOCOUNT ON; 	
	IF EXISTS (SELECT * FROM dbo.DayCareInfo WHERE DayCareId=@DayCareId)
	 Begin
	   Update dbo.DayCareInfo set DescriptionProgram=@Program,DateModified=GETDATE()
	   where DayCareId=@DayCareId; 
	 End
	Else
	 Begin
	   Insert into dbo.DayCareInfo values(@DayCareId,'','',@Program,null,GETDATE(),null);
	 End 
END

--add active column to lu_whats new
alter table lu_whatsnew add Active bit not null default 1

--get all of the whats new
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetAllWhatsNew]
AS
BEGIN	
	select * from lu_whatsnew where active=1;
END

--get day care images
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[GetDayCareInfoImages] (@DayCareId uniqueidentifier)
AS
BEGIN	
	select * from DayCareSnaps where DayCareId=@DayCareId;
END

--get day care info
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayCareInfo] (@DayCareId uniqueidentifier)
AS
BEGIN	
	SET NOCOUNT ON; 	
	select d.DayCareName,i.DescriptionHome,i.DescriptionAboutUs,i.DescriptionProgram,i.Logo
	from dbo.DayCare d Join dbo.DayCareInfo i on d.DayCareId=i.DayCareId
	where d.DayCareId=@DayCareId;
END

--*****************************************************************
--working test data

insert into LU_WhatsNew values(1,'Alexa','get alexa now and use your voice to rock','https://gigglesware.com/images/Assistant-Small.png',GETDATE());
insert into lu_whatsnew values(2,'Free website for your daycare','With GigglesWare, your daycare will get a free profile which you can share and promote your daycare.',
'https://gigglesware.com/images/screens/profile.png',getdate(),1)

select * from appversion
update appversion set version='1.0.14' where ostype='android'

