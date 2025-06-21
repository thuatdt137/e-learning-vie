using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace e_learning_vie.Models;

public partial class SchoolManagementContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public SchoolManagementContext()
    {
    }

    public SchoolManagementContext(DbContextOptions<SchoolManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Aspiration> Aspirations { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Quota> Quotas { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentClassHistory> StudentClassHistories { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("MyCnn");
			optionsBuilder.UseSqlServer(ConnectionString);
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.AcademicYearId).HasName("PK__Academic__C54C7A218FDBFA0E");

            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.YearName).HasMaxLength(20);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Activity__5E5499A86604819A");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Details).HasMaxLength(500);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ActivityL__UserI__5CD6CB2B");
        });

        modelBuilder.Entity<Aspiration>(entity =>
        {
            entity.HasKey(e => e.AspirationId).HasName("PK__Aspirati__1C828934783E2DE7");

            entity.Property(e => e.AspirationId).HasColumnName("AspirationID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TargetSchoolId).HasColumnName("TargetSchoolID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Aspirations)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Aspiratio__Acade__5070F446");

            entity.HasOne(d => d.Student).WithMany(p => p.Aspirations)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Aspiratio__Stude__4E88ABD4");

            entity.HasOne(d => d.TargetSchool).WithMany(p => p.Aspirations)
                .HasForeignKey(d => d.TargetSchoolId)
                .HasConstraintName("FK__Aspiratio__Targe__4F7CD00D");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927A052B95F6D");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Classes)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Classes__Academi__2E1BDC42");

            entity.HasOne(d => d.School).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Classes__SchoolI__300424B4");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Classes__Teacher__2F10007B");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grades__54F87A370A2E8566");

            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Grades)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Grades__Academic__3E52440B");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Grades__StudentI__3C69FB99");

            entity.HasOne(d => d.Subject).WithMany(p => p.Grades)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__Grades__SubjectI__3D5E1FD2");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32069E083C");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.RecipientType).HasMaxLength(20);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Notificat__Acade__4BAC3F29");

            entity.HasOne(d => d.School).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Notificat__Schoo__4AB81AF0");
        });

        modelBuilder.Entity<Quota>(entity =>
        {
            entity.HasKey(e => e.QuotaId).HasName("PK__Quotas__AE96C9E2C2E85A39");

            entity.Property(e => e.QuotaId).HasColumnName("QuotaID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Quota)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Quotas__Academic__5441852A");

            entity.HasOne(d => d.School).WithMany(p => p.Quota)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Quotas__SchoolID__534D60F1");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Requests__33A8519A9124A701");

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.Details).HasMaxLength(500);
            entity.Property(e => e.RequestType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Requests)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Requests__Academ__46E78A0C");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.RequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Requests__Approv__47DBAE45");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RequestCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Requests__Create__45F365D3");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9C8A5B69B9784EF6");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Room).HasMaxLength(20);
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Schedules__Acade__398D8EEE");

            entity.HasOne(d => d.Class).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Schedules__Class__36B12243");

            entity.HasOne(d => d.Subject).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__Schedules__Subje__37A5467C");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Schedules__Teach__38996AB5");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4677B37F6A97C");

            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PrincipalId).HasColumnName("PrincipalID");
            entity.Property(e => e.SchoolName).HasMaxLength(100);
            entity.Property(e => e.SchoolType).HasMaxLength(20);

            entity.HasOne(d => d.Principal).WithMany(p => p.Schools)
                .HasForeignKey(d => d.PrincipalId)
                .HasConstraintName("FK_Schools_PrincipalID");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A79E8FE8C3E");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Students__ClassI__32E0915F");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Students__School__33D4B598");
        });

        modelBuilder.Entity<StudentClassHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__StudentC__4D7B4ADDDD9E1FE5");

            entity.ToTable("StudentClassHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.StudentClassHistories)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__StudentCl__Acade__59063A47");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClassHistories)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__StudentCl__Class__5812160E");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClassHistories)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__StudentCl__Stude__571DF1D5");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subjects__AC1BA38831BD67C9");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.SubjectName).HasMaxLength(50);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF25944207E57EB");

            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.School).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.SchoolId)
                .HasConstraintName("FK__Teachers__School__2A4B4B5E");
        });

		modelBuilder.Entity<User>(entity =>
		{
			entity.HasOne(d => d.Student)
				.WithMany(p => p.Users)
				.HasForeignKey(d => d.StudentId)
				.HasConstraintName("FK_Users_StudentID");

			entity.HasOne(d => d.Teacher)
				.WithMany(p => p.Users)
				.HasForeignKey(d => d.TeacherId)
				.HasConstraintName("FK_Users_TeacherID");
		});

		OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
