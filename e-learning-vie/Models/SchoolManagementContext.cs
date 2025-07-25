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

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ScoreType> ScoreTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<StudentScore> StudentScores { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }
    
    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<ClassSession> ClassSessions { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<TeachingAssignment> TeachingAssignments { get; set; }

    public virtual DbSet<Slot> Slots { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<SubjectGroup> SubjectGroups { get; set; }

    public virtual DbSet<StudentParent> StudentParents { get; set; }

    public virtual DbSet<SubjectScore> SubjectScores { get; set; }




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

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927A052B95F6D");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.GradeId).HasColumnName("GradeID");

            entity.HasOne(d => d.Grade).WithMany(p => p.Classes)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("FK__Classes__Grade");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__86JDNMENN78537");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");


            entity.Property(e => e.SemesterId).HasColumnName("SemesterID");
            entity.HasOne(d => d.Semester).WithMany(p => p.Exams)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK__Exam__Semester");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grade__24JEMEJK3DGFEA");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");

            entity.Property(e => e.GradeName).HasMaxLength(50);
            entity.Property(e => e.GradeDescription).HasMaxLength(100);

        });

        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasKey(e => e.ClassSessionId).HasName("PK__ClassSession__LKDFD823023J3J3");
            entity.Property(e => e.ClassSessionId).HasColumnName("ClassSessionID");


            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.HasOne(d => d.HomeroomTeacher).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__ClassSession__Teacher");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.HasOne(d => d.Class).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__ClassSession__Class");

            entity.Property(e => e.SemesterId).HasColumnName("SemesterID");
            entity.HasOne(d => d.Semester).WithMany(p => p.ClassSessions)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK__ClassSession__Semester");

        });

        modelBuilder.Entity<TeachingAssignment>(entity =>
        {
            entity.HasKey(e => e.TeachingAssignmentId).HasName("PK__TeachingAssignment__98374VB5Y743");
            entity.Property(e => e.TeachingAssignmentId).HasColumnName("TeachingAssignmentID");


            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.HasOne(d => d.Teacher).WithMany(p => p.TeachingAssignments)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__TeachingAssignment__Teacher");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.HasOne(d => d.Subject).WithMany(p => p.TeachingAssignments)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__TeachingAssignment__Subject");

            entity.Property(e => e.ClassSessionId).HasColumnName("ClassSessionID");
            entity.HasOne(d => d.Session).WithMany(p => p.TeachingAssignments)
                .HasForeignKey(d => d.ClassSessionId)
                .HasConstraintName("FK__TeachingAssignment__ClassSession");

        });

        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.HasKey(e => e.TeacherSubjectId).HasName("PK__TeacherSubject");
            entity.Property(e => e.TeacherSubjectId).HasColumnName("TeacherSubjectID");


            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__TeacherSubject__Teacher");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.HasOne(d => d.Subject).WithMany(p => p.TeacherSubjects)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__TeacherSubject__Subject");
        });


        modelBuilder.Entity<ScoreType>(entity =>
        {
            entity.HasKey(e => e.ScoreId).HasName("PK__Scores__54F87A370A2E8566");
            entity.Property(e => e.ScoreId).HasColumnName("ScoreID");

            entity.Property(e => e.TypeName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId).HasName("PK__Semester__49U49549O2U59O8");
            entity.Property(e => e.SemesterId).HasColumnName("SemesterID");

            entity.Property(e => e.SemesterName).HasMaxLength(50);

            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Semesters)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK__Semester__AcademicYear");

        });

        modelBuilder.Entity<StudentScore>(entity =>
        {
            entity.HasKey(e => e.StudentScoreId).HasName("PK__StuScores__83JWN6H9HWN78537");
            entity.Property(e => e.StudentScoreId).HasColumnName("StudentScoreID");
            entity.Property(e => e.Note).HasMaxLength(50);



            entity.Property(e => e.EnrollmentId).HasColumnName("EnrollmentID");
            entity.HasOne(d => d.Enrollment).WithMany(p => p.StudentScores)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("FK__StudentScore__Enrollment");

            entity.Property(e => e.SubjectScoreId).HasColumnName("SubjectScoreID");
            entity.HasOne(d => d.SubjectScore).WithMany(p => p.StudentScores)
                .HasForeignKey(d => d.SubjectScoreId)
                .HasConstraintName("FK__StudentScore__SubjectScore");

            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.HasOne(d => d.Exam).WithMany(p => p.StudentScores)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__StudentScore__Exam");

        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollment__934URO9AS8DFA3KJ2");

            entity.Property(e => e.EnrollmentId).HasColumnName("EnrollmentID");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Enrollment__Student");

            entity.Property(e => e.ClassSessionId).HasColumnName("ClassSessionID");
            entity.HasOne(d => d.ClassSession).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassSessionId)
                .HasConstraintName("FK__Enrollment__ClassSession");

        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendance__98USDF9S8DUF");

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Attendance__Student");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.HasOne(d => d.Schedule).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("FK__Attendance__Schedule");

        });

        modelBuilder.Entity<Slot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__Slot__9OS8DJF98S");

            entity.Property(e => e.SlotId).HasColumnName("SlotID");

        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__98YDS89F");

            entity.Property(e => e.RoomId).HasColumnName("RoomID");

        });


        modelBuilder.Entity<SubjectGroup>(entity =>
        {
            entity.HasKey(e => e.SubjectGroupId).HasName("PK__SubjectGroup__J908SFJ23J");

            entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroupID");

            entity.Property(e => e.LeadTeacherId).HasColumnName("LeadTeacherID");
            entity.HasOne(d => d.LeadTeacher).WithMany(p => p.SubjectGroups)
                .HasForeignKey(d => d.LeadTeacherId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__SubjectGroup__Teacher");

        });

        modelBuilder.Entity<SubjectScore>(entity =>
        {
            entity.HasKey(e => e.SubjectScoreId).HasName("PK__SubjectScore");

            entity.Property(e => e.SubjectScoreId).HasColumnName("SubjectScoreID");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectScores)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__SubjectScore__Subject");

            entity.Property(e => e.ScoreTypeId).HasColumnName("ScoreTypeID");
            entity.HasOne(d => d.ScoreType).WithMany(p => p.SubjectScores)
                .HasForeignKey(d => d.ScoreTypeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__SubjectScore__ScoreType");

        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__9AUSD8FD");
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");

            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.HasOne(d => d.Room).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Schedule_Room");

            entity.Property(e => e.SlotId).HasColumnName("SlotID");
            entity.HasOne(d => d.Slot).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.SlotId)
                .HasConstraintName("FK__Schedule_Slot");

        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32069E083C");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.AcademicYearId).HasColumnName("AcademicYearID");
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.RecipientType).HasMaxLength(20);

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AcademicYearId)
                .HasConstraintName("FK__Notificat__Acade__4BAC3F29");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A79E8FE8C3E");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Parents__H834H5934H53H4H53");

            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subjects__AC1BA38831BD67C9");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.HasOne(d => d.Grade).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("FK__Subject__Grade");

            entity.Property(e => e.SubjectGroupId).HasColumnName("SubjectGroupID");
            entity.HasOne(d => d.SubjectGroup).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.SubjectGroupId)
                .HasConstraintName("FK__Subject__SubGroup");

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
        });

        modelBuilder.Entity<StudentParent>(entity =>
        {
            entity.HasKey(e => e.StudentParentId).HasName("PK__StudentParent__JDL8D");
            entity.Property(e => e.StudentParentId).HasColumnName("StudentParentID");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.HasOne(d => d.Student).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__StudentParent__Student");

            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.HasOne(d => d.Parent).WithMany(p => p.StudentParents)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__StudentParent__Parent");

            entity.Property(e => e.RelationalName).HasMaxLength(100);

        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.Student)
                .WithOne(p => p.User)
                .HasForeignKey<User>(d => d.StudentId)
                .HasConstraintName("FK_Users_StudentID");

            entity.HasOne(d => d.Teacher)
                .WithOne(p => p.User)
                .HasForeignKey<User>(d => d.TeacherId)
                .HasConstraintName("FK_Users_TeacherID");

            entity.HasOne(d => d.Parent)
                .WithOne(p => p.User)
                .HasForeignKey<User>(d => d.ParentId)
                .HasConstraintName("FK_Users_ParentID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
