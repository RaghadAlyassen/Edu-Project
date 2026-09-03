using Edu_Project.Models;
using Edu_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Edu_Project.Data
{
    public class Context : IdentityDbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }
        


        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonWatch> LessonsWatch { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizGrade> Quizgrades { get; set; }
        public DbSet<FinalExam> FinalExams { get; set; }
        public DbSet<FinalGrade> Finalgrades { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }

















        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentAnswer>()
                .HasOne(st => st.answer)
                .WithMany(a => a.StudentAnswers)
                .HasForeignKey(st => st.answerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentAnswer>()
                .HasOne(st => st.question)
                .WithMany(q => q.Studentanswers)
                .HasForeignKey(st => st.questionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentAnswer>()
                .HasOne(st => st.student)
                .WithMany(s => s.studentanswers)
                .HasForeignKey(st => st.studentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<QuizGrade>()
                .HasOne(qg => qg.Student)
                .WithMany(s => s.quizgrades)
                .HasForeignKey(qg => qg.StudentId);

            builder.Entity<QuizGrade>()
                .HasOne(qg => qg.quiz)
                .WithMany(q => q.Quizgrades)
                .HasForeignKey(qg => qg.quizId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FinalGrade>()
                .HasOne(fg => fg.Student)
                .WithMany(s => s.finalGrades)
                .HasForeignKey(fg => fg.StudentId);

            builder.Entity<FinalGrade>()
                .HasOne(fg => fg.Finalexam)
                .WithMany(f => f.finalgrades)
                .HasForeignKey(fg => fg.FinalexamId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LessonWatch>()
                .HasOne(lw => lw.Lesson)
                .WithMany(l => l.Lessonwatches)
                .HasForeignKey(lw => lw.LessonId);

            builder.Entity<LessonWatch>()
                .HasOne(lw => lw.Student)
                .WithMany(s => s.lessonwatches)
                .HasForeignKey(lw => lw.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.enrollments)
                .HasForeignKey(e => e.StudentId);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId);

            builder.Entity<Question>()
                .HasOne(q => q.Finalexam)
                .WithMany(fe => fe.Question)
                .HasForeignKey(q => q.FinalexamId);

            builder.Entity<FinalExam>()
                .HasOne(fe => fe.course)
                .WithOne(c => c.Finalexam)
                .HasForeignKey<FinalExam>(fe => fe.courseId)
                .OnDelete(DeleteBehavior.NoAction); ;

            builder.Entity<Quiz>()
                .HasOne(qz => qz.Lesson)
                .WithOne(l => l.quiz)
                .HasForeignKey<Quiz>(qz => qz.LessonId)
                .OnDelete(DeleteBehavior.NoAction); ;

            builder.Entity<Lesson>()
                .HasOne(l => l.Instructor)
                .WithMany(i => i.lessons)
                .HasForeignKey(l => l.InstructorId);

            builder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.courses)
                .HasForeignKey(c => c.InstructorId);

            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(ct => ct.Courses)
                .HasForeignKey(c => c.CategoryId);

            builder.Entity<Category>()
                .HasOne(ct => ct.Instructor)
                .WithMany(c => c.categories)
                .HasForeignKey(ct => ct.InstructorId);

            builder.Entity<Enrollment>()
                .HasKey(e => new { e.StudentId, e.CourseId });

            builder.Entity<StudentAnswer>()
                .HasKey(st => new { st.studentId, st.questionId, st.answerId });

            builder.Entity<QuizGrade>()
                .HasKey(qg => new { qg.StudentId, qg.quizId });

            builder.Entity<FinalGrade>()
                .HasKey(fg => new { fg.FinalexamId, fg.StudentId });

            builder.Entity<LessonWatch>()
                .HasKey(lw => new { lw.StudentId, lw.LessonId });

            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Exam>()
             .HasOne(e => e.Instructor)
             .WithMany(i => i.Exams)
             .HasForeignKey(e => e.InstructorId);






        }



    }

}
