using Edu_Project.Data;
using Edu_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Edu_Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<Context>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "DefaultConnection")));

            builder.Services
                .AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<Context>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager =
                    services.GetRequiredService<
                        RoleManager<IdentityRole>>();

                var userManager =
                    services.GetRequiredService<
                        UserManager<User>>();

                var context =
                    services.GetRequiredService<Context>();

                string[] roles =
                {
                    "Student",
                    "Instructor",
                    "Admin"
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(
                            new IdentityRole(role));
                    }
                }

                var adminEmail =
                    "admin@edu.com";

                var adminPassword =
                    "Admin123!";

                var adminUser =
                    await userManager.FindByEmailAsync(
                        adminEmail);

                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        ProfileImg = ""
                    };

                    var result =
                        await userManager.CreateAsync(
                            adminUser,
                            adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            adminUser,
                            "Admin");
                    }
                }
                else
                {
                    if (!await userManager.IsInRoleAsync(
                        adminUser,
                        "Admin"))
                    {
                        await userManager.AddToRoleAsync(
                            adminUser,
                            "Admin");
                    }
                }

                await SeedExams(context);
            }

            app.Run();
        }

        private static async Task SeedExams(
            Context context)
        {
            var courses =
                await context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Lessons)
                    .ToListAsync();

            foreach (var course in courses)
            {
                var subject =
                    GetSubject(
                        course.Title,
                        course.Category?.Name,
                        course.Descciption);

                if (course.Lessons != null)
                {
                    foreach (var lesson in course.Lessons)
                    {
                        var quiz =
                            await context.Quizzes
                                .FirstOrDefaultAsync(
                                    q =>
                                        q.LessonId ==
                                        lesson.Id);

                        if (quiz == null)
                        {
                            var instructorId =
                                lesson.InstructorId
                                ?? course.InstructorId;

                            if (instructorId != null)
                            {
                                quiz = new Quiz
                                {
                                    Title =
                                        $"{lesson.Title} Quiz",

                                    Duration = 10,

                                    TotalMarks = 5,

                                    LessonId =
                                        lesson.Id,

                                    InstructorId =
                                        instructorId
                                };

                                context.Quizzes.Add(
                                    quiz);

                                await context
                                    .SaveChangesAsync();
                            }
                        }

                        if (quiz != null)
                        {
                            var questions =
                                await context.Questions
                                    .Where(q =>
                                        q.QuizId ==
                                        quiz.Id)
                                    .Include(q =>
                                        q.Answers)
                                    .OrderBy(q => q.Id)
                                    .ToListAsync();

                            var quizData =
                                GetQuizQuestions(
                                    subject,
                                    course.Title,
                                    lesson.Title);

                            if (questions.Count == 0)
                            {
                                AddQuestions(
                                    context,
                                    quiz.Id,
                                    null,
                                    quizData);
                            }
                            else if (
                                IsLegacyQuiz(
                                    questions))
                            {
                                ReplaceQuestions(
                                    context,
                                    questions,
                                    quiz.Id,
                                    null,
                                    quizData);
                            }

                            await context
                                .SaveChangesAsync();
                        }
                    }
                }

                var finalExam =
                    await context.FinalExams
                        .FirstOrDefaultAsync(
                            f =>
                                f.courseId ==
                                course.Id);

                if (finalExam == null &&
                    course.InstructorId != null)
                {
                    finalExam =
                        new FinalExam
                        {
                            Title =
                                $"{course.Title} Final Exam",

                            Duration = 30,

                            TotalMarks = 10,

                            courseId =
                                course.Id,

                            InstructorId =
                                course.InstructorId
                        };

                    context.FinalExams.Add(
                        finalExam);

                    await context
                        .SaveChangesAsync();
                }

                if (finalExam != null)
                {
                    var questions =
                        await context.Questions
                            .Where(q =>
                                q.FinalexamId ==
                                finalExam.Id)
                            .Include(q =>
                                q.Answers)
                            .OrderBy(q => q.Id)
                            .ToListAsync();

                    var finalData =
                        GetFinalQuestions(
                            subject,
                            course.Title);

                    var quizDataForDetection =
                        GetQuizQuestions(
                            subject,
                            course.Title,
                            course.Title);

                    if (questions.Count == 0)
                    {
                        AddQuestions(
                            context,
                            null,
                            finalExam.Id,
                            finalData);
                    }
                    else if (
                        IsLegacyFinal(
                            questions) ||
                        ContainsQuizQuestions(
                            questions,
                            quizDataForDetection))
                    {
                        ReplaceQuestions(
                            context,
                            questions,
                            null,
                            finalExam.Id,
                            finalData);
                    }

                    await context
                        .SaveChangesAsync();
                }
            }
        }

        private static string GetSubject(
            string? title,
            string? category,
            string? description)
        {
            var text =
                $"{title} {category} {description}"
                    .ToLower();

            if (text.Contains("cyber") ||
                text.Contains("security") ||
                text.Contains("ethical hacking"))
            {
                return "cyber";
            }

            if (text.Contains("cloud") ||
                text.Contains("aws") ||
                text.Contains("azure") ||
                text.Contains("huawei"))
            {
                return "cloud";
            }

            if (text.Contains("web") ||
                text.Contains("asp.net") ||
                text.Contains("mvc") ||
                text.Contains("html") ||
                text.Contains("css") ||
                text.Contains("javascript"))
            {
                return "web";
            }

            if (text.Contains("database") ||
                text.Contains("sql") ||
                text.Contains("entity framework") ||
                text.Contains("ef core"))
            {
                return "database";
            }

            if (text.Contains("network") ||
                text.Contains("cisco") ||
                text.Contains("routing") ||
                text.Contains("switching"))
            {
                return "network";
            }

            if (text.Contains("machine learning") ||
                text.Contains("artificial intelligence") ||
                text.Contains("data science"))
            {
                return "ai";
            }

            if (text.Contains("software engineering") ||
                text.Contains("software design"))
            {
                return "software";
            }

            if (text.Contains("graphic") ||
                text.Contains("ui") ||
                text.Contains("ux") ||
                text.Contains("design"))
            {
                return "design";
            }

            if (text.Contains("project management") ||
                text.Contains("management"))
            {
                return "management";
            }

            if (text.Contains("programming") ||
                text.Contains("c#") ||
                text.Contains("oop") ||
                text.Contains(".net"))
            {
                return "programming";
            }

            return "general";
        }

        private static bool IsLegacyQuiz(
            List<Question> questions)
        {
            return questions.Any(q =>
                q.Text.Contains(
                    "Which lesson is this quiz related to",
                    StringComparison.OrdinalIgnoreCase) ||
                q.Text.Contains(
                    "What should a student do before taking",
                    StringComparison.OrdinalIgnoreCase) ||
                q.Text.Contains(
                    "main purpose of the",
                    StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsLegacyFinal(
            List<Question> questions)
        {
            return questions.Any(q =>
                q.Text.Contains(
                    "Which course does this final exam belong to",
                    StringComparison.OrdinalIgnoreCase) ||
                q.Text.Contains(
                    "What is the purpose of a final exam",
                    StringComparison.OrdinalIgnoreCase) ||
                q.Text.Contains(
                    "good learning strategy",
                    StringComparison.OrdinalIgnoreCase));
        }

        private static bool ContainsQuizQuestions(
            List<Question> existing,
            List<ExamQuestionData> quizQuestions)
        {
            foreach (var question
                     in existing)
            {
                if (quizQuestions.Any(q =>
                    string.Equals(
                        q.Question,
                        question.Text,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ReplaceQuestions(
            Context context,
            List<Question> oldQuestions,
            int? quizId,
            int? finalExamId,
            List<ExamQuestionData> newQuestions)
        {
            foreach (var question
                     in oldQuestions)
            {
                if (question.Answers != null)
                {
                    context.Answers.RemoveRange(
                        question.Answers);
                }
            }

            context.Questions.RemoveRange(
                oldQuestions);

            AddQuestions(
                context,
                quizId,
                finalExamId,
                newQuestions);
        }

        private static void AddQuestions(
            Context context,
            int? quizId,
            int? finalExamId,
            List<ExamQuestionData> questions)
        {
            foreach (var item
                     in questions)
            {
                var question =
                    new Question
                    {
                        Text =
                            item.Question,

                        QuizId =
                            quizId,

                        FinalexamId =
                            finalExamId,

                        Answers =
                            new List<Answer>()
                    };

                for (var i = 0;
                     i <
                     item.Answers.Length;
                     i++)
                {
                    question.Answers.Add(
                        new Answer
                        {
                            Text =
                                item.Answers[i],

                            IsCorrect =
                                i ==
                                item.CorrectAnswer,

                            Question =
                                question
                        });
                }

                context.Questions.Add(
                    question);
            }
        }

        private static List<ExamQuestionData>
            GetQuizQuestions(
                string subject,
                string courseTitle,
                string lessonTitle)
        {
            return subject switch
            {
                "cyber" =>
                    CyberQuiz(),

                "cloud" =>
                    CloudQuiz(),

                "web" =>
                    WebQuiz(),

                "database" =>
                    DatabaseQuiz(),

                "network" =>
                    NetworkQuiz(),

                "programming" =>
                    ProgrammingQuiz(),

                "ai" =>
                    AiQuiz(),

                "software" =>
                    SoftwareQuiz(),

                "design" =>
                    DesignQuiz(),

                "management" =>
                    ManagementQuiz(),

                _ =>
                    GeneralQuiz(
                        courseTitle,
                        lessonTitle)
            };
        }

        private static List<ExamQuestionData>
            GetFinalQuestions(
                string subject,
                string courseTitle)
        {
            return subject switch
            {
                "cyber" =>
                    CyberFinal(),

                "cloud" =>
                    CloudFinal(),

                "web" =>
                    WebFinal(),

                "database" =>
                    DatabaseFinal(),

                "network" =>
                    NetworkFinal(),

                "programming" =>
                    ProgrammingFinal(),

                "ai" =>
                    AiFinal(),

                "software" =>
                    SoftwareFinal(),

                "design" =>
                    DesignFinal(),

                "management" =>
                    ManagementFinal(),

                _ =>
                    GeneralFinal(
                        courseTitle)
            };
        }

        private static ExamQuestionData Q(
            string question,
            int correct,
            params string[] answers)
        {
            return new ExamQuestionData
            {
                Question = question,
                Answers = answers,
                CorrectAnswer = correct
            };
        }

        private static List<ExamQuestionData>
            CyberQuiz()
        {
            return new()
            {
                Q(
                    "What is the main purpose of a firewall?",
                    1,
                    "Store passwords",
                    "Monitor and control network traffic",
                    "Create databases",
                    "Increase processor speed"),

                Q(
                    "What is phishing?",
                    0,
                    "A social engineering attack used to steal information",
                    "A file compression technique",
                    "A network cable type",
                    "A backup method"),

                Q(
                    "Which password is the strongest?",
                    2,
                    "12345678",
                    "password2026",
                    "T9#qL2!mX7@p",
                    "mona123"),

                Q(
                    "What is malware?",
                    0,
                    "Software designed to damage or exploit systems",
                    "A secure operating system",
                    "A programming language",
                    "A network protocol"),

                Q(
                    "What does encryption do?",
                    2,
                    "Deletes data",
                    "Compresses files",
                    "Converts data into a protected unreadable form",
                    "Increases internet speed")
            };
        }

        private static List<ExamQuestionData>
            CyberFinal()
        {
            return new()
            {
                Q(
                    "What does the CIA triad represent?",
                    0,
                    "Confidentiality, Integrity and Availability",
                    "Control, Internet and Authentication",
                    "Cyber, Identity and Access",
                    "Confidentiality, Internet and Authorization"),

                Q(
                    "What is the principle of least privilege?",
                    1,
                    "Give every user administrator access",
                    "Give users only the permissions required for their tasks",
                    "Allow anonymous access",
                    "Disable authentication"),

                Q(
                    "What is ransomware primarily designed to do?",
                    2,
                    "Improve operating system performance",
                    "Monitor internet speed",
                    "Encrypt or block data and demand payment",
                    "Create secure backups"),

                Q(
                    "What is multi-factor authentication?",
                    3,
                    "Using several usernames",
                    "Changing a password every day",
                    "Using one long password",
                    "Using more than one method to verify identity"),

                Q(
                    "Which attack attempts to make a service unavailable by overwhelming it with traffic?",
                    0,
                    "Denial of Service",
                    "Password hashing",
                    "Backup attack",
                    "Digital signature"),

                Q(
                    "What is a security vulnerability?",
                    2,
                    "A strong password",
                    "A firewall rule",
                    "A weakness that could be exploited",
                    "A backup policy"),

                Q(
                    "What is the purpose of penetration testing?",
                    1,
                    "Delete production data",
                    "Identify weaknesses before malicious attackers exploit them",
                    "Create new user accounts",
                    "Reduce storage capacity"),

                Q(
                    "What is social engineering?",
                    3,
                    "Installing antivirus software",
                    "Encrypting databases",
                    "Configuring routers",
                    "Manipulating people into revealing sensitive information"),

                Q(
                    "Why are security patches important?",
                    0,
                    "They can fix known vulnerabilities",
                    "They disable encryption",
                    "They make passwords public",
                    "They remove authentication"),

                Q(
                    "What is an intrusion detection system used for?",
                    2,
                    "Designing web pages",
                    "Creating databases",
                    "Detecting suspicious activity",
                    "Compressing files")
            };
        }

        private static List<ExamQuestionData>
            CloudQuiz()
        {
            return new()
            {
                Q(
                    "What does IaaS provide?",
                    1,
                    "Only email",
                    "Virtualized computing infrastructure",
                    "Only applications",
                    "Only source code"),

                Q(
                    "Which cloud model is available to many customers over the internet?",
                    0,
                    "Public cloud",
                    "Private cloud",
                    "Local network",
                    "Offline system"),

                Q(
                    "What is cloud scalability?",
                    2,
                    "Deleting resources permanently",
                    "Disabling servers",
                    "Adjusting resources according to demand",
                    "Changing passwords"),

                Q(
                    "What is virtualization?",
                    1,
                    "Deleting physical computers",
                    "Creating virtual versions of computing resources",
                    "Printing data",
                    "Creating passwords"),

                Q(
                    "Which service model provides complete applications to users?",
                    3,
                    "LAN",
                    "IaaS",
                    "PaaS",
                    "SaaS")
            };
        }

        private static List<ExamQuestionData>
            CloudFinal()
        {
            return new()
            {
                Q(
                    "What is cloud elasticity?",
                    1,
                    "Keeping resources permanently fixed",
                    "Automatically increasing or decreasing resources based on demand",
                    "Deleting all virtual machines",
                    "Disabling internet access"),

                Q(
                    "Which cloud deployment model is dedicated to one organization?",
                    2,
                    "Public cloud",
                    "Open cloud",
                    "Private cloud",
                    "Shared internet"),

                Q(
                    "What is a virtual machine?",
                    0,
                    "A software-based computer environment",
                    "A network cable",
                    "A database row",
                    "A physical keyboard"),

                Q(
                    "Why are availability zones used in cloud platforms?",
                    3,
                    "To change passwords",
                    "To delete data",
                    "To reduce storage",
                    "To improve resilience and availability"),

                Q(
                    "What does high availability aim to provide?",
                    1,
                    "Longer outages",
                    "Continuous access to services",
                    "Less redundancy",
                    "No backups"),

                Q(
                    "What is horizontal scaling?",
                    2,
                    "Increasing RAM in one server",
                    "Deleting servers",
                    "Adding more server instances",
                    "Changing a domain name"),

                Q(
                    "What is a cloud region?",
                    0,
                    "A geographic area containing cloud infrastructure",
                    "A password policy",
                    "A virtual machine type",
                    "A programming language"),

                Q(
                    "What is disaster recovery designed to support?",
                    3,
                    "Deleting old data",
                    "Reducing security",
                    "Removing redundancy",
                    "Restoring services after a major failure"),

                Q(
                    "What does PaaS mainly provide developers?",
                    1,
                    "Only physical servers",
                    "A managed platform for building and deploying applications",
                    "Only email accounts",
                    "Network cables"),

                Q(
                    "Why is load balancing useful?",
                    2,
                    "It deletes incoming requests",
                    "It disables servers",
                    "It distributes traffic across multiple resources",
                    "It removes databases")
            };
        }

        private static List<ExamQuestionData>
            WebQuiz()
        {
            return new()
            {
                Q(
                    "What is HTML mainly used for?",
                    0,
                    "Structuring web pages",
                    "Managing databases",
                    "Routing networks",
                    "Training AI"),

                Q(
                    "What is CSS mainly used for?",
                    2,
                    "SQL queries",
                    "Routing",
                    "Styling web pages",
                    "Database backup"),

                Q(
                    "What does HTTP stand for?",
                    1,
                    "High Text Transfer Program",
                    "Hypertext Transfer Protocol",
                    "Host Transfer Process",
                    "Hyper Tool Protocol"),

                Q(
                    "What does a Controller do in MVC?",
                    3,
                    "Styles pages",
                    "Creates images",
                    "Stores CSS",
                    "Handles requests and application flow"),

                Q(
                    "Which language is commonly used to add browser interactivity?",
                    1,
                    "SQL",
                    "JavaScript",
                    "CSS",
                    "XML")
            };
        }

        private static List<ExamQuestionData>
            WebFinal()
        {
            return new()
            {
                Q(
                    "What is the main responsibility of a Model in MVC?",
                    0,
                    "Represent application data and business information",
                    "Style HTML",
                    "Configure routers",
                    "Create images"),

                Q(
                    "What is a Razor View used for in ASP.NET Core MVC?",
                    1,
                    "Creating SQL tables",
                    "Rendering dynamic HTML",
                    "Configuring network switches",
                    "Creating virtual machines"),

                Q(
                    "Which HTTP method is normally used to create or submit data?",
                    2,
                    "GET",
                    "HEAD",
                    "POST",
                    "TRACE"),

                Q(
                    "What is responsive web design?",
                    3,
                    "A website with no CSS",
                    "A desktop-only website",
                    "A website with only one page",
                    "A design that adapts to different screen sizes"),

                Q(
                    "What is model binding in ASP.NET Core?",
                    0,
                    "Mapping request values to action parameters or models",
                    "Generating CSS automatically",
                    "Creating SQL backups",
                    "Compressing images"),

                Q(
                    "What does dependency injection help achieve?",
                    2,
                    "More duplicated code",
                    "Hard-coded dependencies",
                    "Loose coupling between components",
                    "No services"),

                Q(
                    "What is routing used for in a web application?",
                    1,
                    "Changing database passwords",
                    "Mapping URLs to application endpoints",
                    "Compressing files",
                    "Creating CSS"),

                Q(
                    "Why is server-side validation important?",
                    3,
                    "It changes page colors",
                    "It increases image size",
                    "It removes forms",
                    "It validates data even if client-side validation is bypassed"),

                Q(
                    "What does authentication determine?",
                    0,
                    "Who the user is",
                    "Which CSS file loads",
                    "Database table size",
                    "Internet speed"),

                Q(
                    "What does authorization determine?",
                    2,
                    "The user's password length",
                    "The page font",
                    "What an authenticated user is allowed to access",
                    "The database name")
            };
        }

        private static List<ExamQuestionData>
            DatabaseQuiz()
        {
            return new()
            {
                Q(
                    "What is a primary key?",
                    0,
                    "A unique identifier for a row",
                    "A duplicate column",
                    "A password",
                    "A backup"),

                Q(
                    "Which SQL command retrieves data?",
                    2,
                    "DELETE",
                    "DROP",
                    "SELECT",
                    "ALTER"),

                Q(
                    "What is a foreign key used for?",
                    1,
                    "Changing colors",
                    "Creating relationships between tables",
                    "Deleting databases",
                    "Encrypting passwords"),

                Q(
                    "What does normalization help reduce?",
                    3,
                    "Security",
                    "Indexes",
                    "Tables",
                    "Data redundancy"),

                Q(
                    "What does DbContext represent in EF Core?",
                    0,
                    "A session with the database",
                    "A CSS file",
                    "A router",
                    "A browser")
            };
        }

        private static List<ExamQuestionData>
            DatabaseFinal()
        {
            return new()
            {
                Q(
                    "Which SQL command inserts a new row?",
                    1,
                    "SELECT",
                    "INSERT",
                    "DROP",
                    "ALTER"),

                Q(
                    "What is an EF Core migration?",
                    2,
                    "A CSS update",
                    "A network operation",
                    "A tracked database schema change",
                    "A login operation"),

                Q(
                    "Which relationship allows many students to belong to many courses?",
                    3,
                    "One-to-one",
                    "Zero-to-one",
                    "One-to-many",
                    "Many-to-many"),

                Q(
                    "What does a database index commonly improve?",
                    0,
                    "Query performance",
                    "Screen brightness",
                    "Password length",
                    "Image quality"),

                Q(
                    "What is the purpose of SQL JOIN?",
                    1,
                    "Delete databases",
                    "Combine related rows from tables",
                    "Create passwords",
                    "Start a server"),

                Q(
                    "What does a UNIQUE constraint enforce?",
                    2,
                    "All values must be null",
                    "All rows must be deleted",
                    "Duplicate values are prevented in specified columns",
                    "Every table must have one row"),

                Q(
                    "What is a transaction used for?",
                    3,
                    "Changing CSS",
                    "Creating images",
                    "Managing URLs",
                    "Grouping operations that should succeed or fail together"),

                Q(
                    "What does a NOT NULL constraint do?",
                    0,
                    "Requires a value for a column",
                    "Deletes the column",
                    "Creates duplicates",
                    "Creates an index automatically"),

                Q(
                    "What is eager loading in EF Core?",
                    2,
                    "Deleting related records",
                    "Loading nothing",
                    "Loading related data as part of the query",
                    "Creating a migration"),

                Q(
                    "What does SaveChanges do in EF Core?",
                    1,
                    "Deletes the DbContext",
                    "Persists tracked changes to the database",
                    "Creates a new database every time",
                    "Closes Visual Studio")
            };
        }

        private static List<ExamQuestionData>
            NetworkQuiz()
        {
            return new()
            {
                Q(
                    "What device connects different networks?",
                    1,
                    "Switch",
                    "Router",
                    "Keyboard",
                    "Printer"),

                Q(
                    "What does IP stand for?",
                    0,
                    "Internet Protocol",
                    "Internal Program",
                    "Internet Process",
                    "Input Protocol"),

                Q(
                    "Which protocol automatically assigns IP addresses?",
                    2,
                    "HTTP",
                    "FTP",
                    "DHCP",
                    "SMTP"),

                Q(
                    "What does DNS do?",
                    3,
                    "Encrypt files",
                    "Create passwords",
                    "Control keyboards",
                    "Translate domain names to IP addresses"),

                Q(
                    "Which device forwards frames inside a LAN?",
                    1,
                    "Router only",
                    "Switch",
                    "Printer",
                    "Keyboard")
            };
        }

        private static List<ExamQuestionData>
            NetworkFinal()
        {
            return new()
            {
                Q(
                    "Which protocol is used for secure web communication?",
                    0,
                    "HTTPS",
                    "FTP",
                    "Telnet",
                    "ARP"),

                Q(
                    "What is the purpose of a subnet mask?",
                    2,
                    "Encrypt traffic",
                    "Assign passwords",
                    "Separate network and host portions of an IP address",
                    "Store files"),

                Q(
                    "What does LAN stand for?",
                    1,
                    "Large Access Network",
                    "Local Area Network",
                    "Linked Application Node",
                    "Long Area Number"),

                Q(
                    "Which protocol is commonly used to send email?",
                    3,
                    "DNS",
                    "DHCP",
                    "HTTP",
                    "SMTP"),

                Q(
                    "What is the main role of a default gateway?",
                    0,
                    "Forward traffic to other networks",
                    "Create database tables",
                    "Store passwords",
                    "Compile code"),

                Q(
                    "Which OSI layer is responsible for routing packets?",
                    2,
                    "Physical",
                    "Data Link",
                    "Network",
                    "Presentation"),

                Q(
                    "What is a MAC address?",
                    1,
                    "An internet domain",
                    "A hardware identifier associated with a network interface",
                    "A database password",
                    "A web protocol"),

                Q(
                    "What does NAT commonly do?",
                    3,
                    "Deletes packets",
                    "Creates DNS records",
                    "Encrypts files",
                    "Translates private and public IP addresses"),

                Q(
                    "What is TCP designed to provide?",
                    0,
                    "Reliable connection-oriented communication",
                    "Only wireless access",
                    "Domain name resolution",
                    "Automatic IP assignment"),

                Q(
                    "What is the purpose of a VLAN?",
                    2,
                    "Increase CPU speed",
                    "Create SQL tables",
                    "Logically segment a network",
                    "Generate passwords")
            };
        }

        private static List<ExamQuestionData>
            ProgrammingQuiz()
        {
            return new()
            {
                Q(
                    "What is a class in object-oriented programming?",
                    0,
                    "A blueprint for creating objects",
                    "A database only",
                    "A router",
                    "A CSS selector"),

                Q(
                    "What is encapsulation?",
                    2,
                    "Deleting methods",
                    "Creating databases",
                    "Hiding internal data and controlling access",
                    "Opening all fields"),

                Q(
                    "What does inheritance allow?",
                    1,
                    "Deleting parent classes",
                    "A class to reuse members of another class",
                    "Creating tables",
                    "Changing HTML"),

                Q(
                    "What is polymorphism?",
                    3,
                    "Using only one class",
                    "Removing methods",
                    "Creating one variable",
                    "Allowing different implementations through a common interface"),

                Q(
                    "Which C# keyword creates a new object?",
                    1,
                    "class",
                    "new",
                    "void",
                    "using")
            };
        }

        private static List<ExamQuestionData>
            ProgrammingFinal()
        {
            return new()
            {
                Q(
                    "What is a constructor?",
                    0,
                    "A method used to initialize an object",
                    "A database query",
                    "A CSS rule",
                    "A network address"),

                Q(
                    "Which access modifier restricts access to the same class?",
                    2,
                    "public",
                    "protected",
                    "private",
                    "global"),

                Q(
                    "What is an interface in C#?",
                    1,
                    "A database",
                    "A contract that defines members a type should implement",
                    "A loop",
                    "A CSS file"),

                Q(
                    "What does a loop do?",
                    3,
                    "Deletes classes",
                    "Creates databases",
                    "Stops all code",
                    "Repeats a block of code"),

                Q(
                    "What is an exception?",
                    0,
                    "An error condition that occurs during program execution",
                    "A valid password",
                    "A table",
                    "An HTML element"),

                Q(
                    "What does method overloading mean?",
                    2,
                    "Deleting methods",
                    "Using one method only",
                    "Having methods with the same name but different parameters",
                    "Replacing all classes"),

                Q(
                    "What is an abstract class?",
                    1,
                    "A class that must always be instantiated directly",
                    "A base class that can contain abstract and implemented members",
                    "A SQL table",
                    "A network class"),

                Q(
                    "What is a property in C# commonly used for?",
                    3,
                    "Routing network traffic",
                    "Creating databases",
                    "Styling HTML",
                    "Controlling access to an object's data"),

                Q(
                    "What does the static keyword indicate?",
                    0,
                    "A member belongs to the type rather than an instance",
                    "A class must be deleted",
                    "A method cannot run",
                    "A variable is always null"),

                Q(
                    "What is dependency injection?",
                    2,
                    "Hard-coding every dependency",
                    "Deleting services",
                    "Providing dependencies to a class from outside",
                    "Creating CSS automatically")
            };
        }

        private static List<ExamQuestionData>
            AiQuiz()
        {
            return new()
            {
                Q(
                    "What is supervised learning?",
                    1,
                    "Learning without data",
                    "Learning from labeled data",
                    "Creating websites",
                    "Routing networks"),

                Q(
                    "What is a feature in machine learning?",
                    0,
                    "An input variable used by a model",
                    "A password",
                    "A cable",
                    "An HTML tag"),

                Q(
                    "What is classification used for?",
                    3,
                    "Deleting files",
                    "Creating servers",
                    "Compressing images",
                    "Predicting discrete categories"),

                Q(
                    "What is regression commonly used to predict?",
                    2,
                    "Passwords",
                    "Categories only",
                    "Continuous numerical values",
                    "IP addresses"),

                Q(
                    "What is overfitting?",
                    1,
                    "Perfect generalization",
                    "Learning training data too closely and performing poorly on new data",
                    "Having no training data",
                    "Deleting a model")
            };
        }

        private static List<ExamQuestionData>
            AiFinal()
        {
            return new()
            {
                Q(
                    "What is a training dataset?",
                    0,
                    "Data used to train a machine learning model",
                    "A CSS theme",
                    "A router",
                    "A password file"),

                Q(
                    "What does classification accuracy measure?",
                    2,
                    "Database size",
                    "Network speed",
                    "The proportion of predictions that are correct",
                    "The number of model features"),

                Q(
                    "What is unsupervised learning?",
                    1,
                    "Learning only from labeled classes",
                    "Finding patterns in unlabeled data",
                    "Writing HTML",
                    "Creating SQL tables"),

                Q(
                    "What is data preprocessing?",
                    3,
                    "Deleting all data",
                    "Creating user accounts",
                    "Changing passwords",
                    "Preparing and cleaning data before training"),

                Q(
                    "What is a machine learning model?",
                    0,
                    "A learned representation used to make predictions",
                    "A network cable",
                    "A CSS file",
                    "A database password"),

                Q(
                    "What is a validation dataset used for?",
                    2,
                    "Deleting the training set",
                    "Replacing the model",
                    "Evaluating and tuning a model during development",
                    "Creating web pages"),

                Q(
                    "What is precision?",
                    1,
                    "All actual positives divided by the dataset",
                    "The proportion of predicted positives that are actually positive",
                    "The number of features",
                    "The training time"),

                Q(
                    "What is recall?",
                    3,
                    "The number of negative predictions",
                    "The model size",
                    "The number of classes",
                    "The proportion of actual positives correctly identified"),

                Q(
                    "What is a confusion matrix?",
                    0,
                    "A table summarizing classification prediction results",
                    "A database schema",
                    "A cloud region",
                    "A network map"),

                Q(
                    "Why is a test dataset used?",
                    2,
                    "To train the model repeatedly",
                    "To create features manually",
                    "To evaluate performance on unseen data",
                    "To change labels")
            };
        }

        private static List<ExamQuestionData>
            SoftwareQuiz()
        {
            return new()
            {
                Q(
                    "What is the main purpose of software requirements?",
                    0,
                    "Describe what the system should do",
                    "Choose colors only",
                    "Create cables",
                    "Delete source code"),

                Q(
                    "What does SDLC stand for?",
                    1,
                    "Secure Data Login Control",
                    "Software Development Life Cycle",
                    "System Database Link Code",
                    "Software Design Local Command"),

                Q(
                    "What is unit testing?",
                    2,
                    "Testing the internet",
                    "Testing only colors",
                    "Testing individual units of code",
                    "Testing cables"),

                Q(
                    "Why is version control useful?",
                    3,
                    "It deletes code",
                    "It blocks developers",
                    "It replaces databases",
                    "It tracks and manages code changes"),

                Q(
                    "What is maintainability?",
                    0,
                    "How easily software can be modified and maintained",
                    "The number of images",
                    "Network bandwidth",
                    "Password length")
            };
        }

        private static List<ExamQuestionData>
            SoftwareFinal()
        {
            return new()
            {
                Q(
                    "What is Agile software development?",
                    1,
                    "A hardware protocol",
                    "An iterative approach to developing software",
                    "A database engine",
                    "A network device"),

                Q(
                    "What is refactoring?",
                    2,
                    "Deleting an application",
                    "Changing all requirements",
                    "Improving code structure without changing its behavior",
                    "Removing all tests"),

                Q(
                    "What is integration testing?",
                    0,
                    "Testing how multiple components work together",
                    "Testing one variable",
                    "Testing passwords",
                    "Testing cables"),

                Q(
                    "What is a use case?",
                    3,
                    "A CSS rule",
                    "A database index",
                    "A router command",
                    "A description of interactions between a user and a system"),

                Q(
                    "What is code review?",
                    1,
                    "Deleting code",
                    "Examining code to identify issues and improve quality",
                    "Changing passwords",
                    "Creating images"),

                Q(
                    "What is functional testing concerned with?",
                    2,
                    "Server temperature",
                    "Code formatting only",
                    "Whether the system behaves according to requirements",
                    "Network cable length"),

                Q(
                    "What is technical debt?",
                    0,
                    "Future cost caused by choosing easier or quicker technical solutions now",
                    "A financial loan",
                    "A software license",
                    "A database backup"),

                Q(
                    "What is continuous integration?",
                    3,
                    "Building once per year",
                    "Never merging branches",
                    "Removing automated tests",
                    "Frequently integrating code changes with automated builds and tests"),

                Q(
                    "What is a software architecture?",
                    1,
                    "Only a UI design",
                    "The high-level structure and organization of a software system",
                    "A database row",
                    "A password policy"),

                Q(
                    "Why is documentation useful?",
                    2,
                    "It makes code slower",
                    "It removes testing",
                    "It helps developers understand, use and maintain the system",
                    "It prevents version control")
            };
        }

        private static List<ExamQuestionData>
            DesignQuiz()
        {
            return new()
            {
                Q(
                    "What does UI stand for?",
                    0,
                    "User Interface",
                    "Universal Internet",
                    "User Integration",
                    "Unified Input"),

                Q(
                    "What does UX mainly focus on?",
                    2,
                    "Database queries",
                    "Network routing",
                    "The user's overall experience",
                    "Server hardware"),

                Q(
                    "What is visual hierarchy?",
                    1,
                    "Random placement",
                    "Organizing elements based on importance",
                    "Deleting images",
                    "Using one font"),

                Q(
                    "Why is contrast useful in design?",
                    3,
                    "It hides elements",
                    "It removes text",
                    "It disables navigation",
                    "It helps important elements stand out"),

                Q(
                    "What is whitespace?",
                    0,
                    "Empty space around design elements",
                    "Only white text",
                    "A database field",
                    "A network protocol")
            };
        }

        private static List<ExamQuestionData>
            DesignFinal()
        {
            return new()
            {
                Q(
                    "Why is consistency important in interface design?",
                    1,
                    "It makes every screen unrelated",
                    "It makes interfaces easier to learn and use",
                    "It removes navigation",
                    "It changes passwords"),

                Q(
                    "What is a wireframe?",
                    2,
                    "A finished database",
                    "A network cable",
                    "A basic visual representation of a screen layout",
                    "An encryption algorithm"),

                Q(
                    "What does accessibility mean in UI design?",
                    0,
                    "Designing interfaces usable by people with different abilities",
                    "Removing labels",
                    "Using tiny text",
                    "Blocking keyboard input"),

                Q(
                    "What does responsive UI design do?",
                    3,
                    "Supports desktop only",
                    "Disables mobile devices",
                    "Removes CSS",
                    "Adapts layouts to different screen sizes"),

                Q(
                    "What is typography?",
                    1,
                    "Database modeling",
                    "The style and arrangement of text",
                    "Network routing",
                    "Cloud hosting"),

                Q(
                    "What is a design system?",
                    2,
                    "A database engine",
                    "A router configuration",
                    "A reusable collection of design standards and components",
                    "A programming language"),

                Q(
                    "Why are user personas created?",
                    0,
                    "To represent typical target users and their needs",
                    "To store passwords",
                    "To create SQL tables",
                    "To configure servers"),

                Q(
                    "What is usability testing?",
                    3,
                    "Testing network speed",
                    "Testing database size",
                    "Testing application source code only",
                    "Observing users interacting with a product to identify usability issues"),

                Q(
                    "What is a call-to-action?",
                    1,
                    "A database query",
                    "An element encouraging the user to perform a specific action",
                    "A network protocol",
                    "A password"),

                Q(
                    "What is information architecture?",
                    2,
                    "Image compression",
                    "Database normalization",
                    "Organizing and structuring content so users can find information easily",
                    "Cloud scaling")
            };
        }

        private static List<ExamQuestionData>
            ManagementQuiz()
        {
            return new()
            {
                Q(
                    "What is a project milestone?",
                    0,
                    "A significant point in a project",
                    "A database table",
                    "A programming language",
                    "A network device"),

                Q(
                    "What is project scope?",
                    2,
                    "Only the budget",
                    "Only team names",
                    "The defined work and objectives of a project",
                    "The website color"),

                Q(
                    "What is a project risk?",
                    1,
                    "A guaranteed success",
                    "An uncertain event that may affect the project",
                    "A database query",
                    "A CSS rule"),

                Q(
                    "What is a deadline?",
                    3,
                    "A programming loop",
                    "A network address",
                    "A database key",
                    "A date by which work should be completed"),

                Q(
                    "Why is project planning important?",
                    0,
                    "It organizes work, resources and objectives",
                    "It removes every risk",
                    "It eliminates communication",
                    "It disables tracking")
            };
        }

        private static List<ExamQuestionData>
            ManagementFinal()
        {
            return new()
            {
                Q(
                    "What is a stakeholder?",
                    1,
                    "Only the developer",
                    "A person or group affected by or interested in the project",
                    "A programming class",
                    "A database server"),

                Q(
                    "What does a Gantt chart commonly show?",
                    2,
                    "Passwords",
                    "Database records",
                    "Project tasks and their schedules",
                    "IP addresses"),

                Q(
                    "What is resource allocation?",
                    0,
                    "Assigning available resources to project activities",
                    "Deleting tasks",
                    "Creating passwords",
                    "Removing the team"),

                Q(
                    "What is project monitoring?",
                    3,
                    "Ignoring progress",
                    "Stopping work",
                    "Deleting plans",
                    "Tracking project progress against the plan"),

                Q(
                    "What is a project deliverable?",
                    1,
                    "A router",
                    "A measurable output produced by a project",
                    "A password",
                    "A programming error"),

                Q(
                    "What is scope creep?",
                    2,
                    "Reducing all requirements",
                    "Closing the project early",
                    "Uncontrolled expansion of project scope",
                    "Changing a password"),

                Q(
                    "What is risk mitigation?",
                    0,
                    "Taking actions to reduce the probability or impact of a risk",
                    "Ignoring risk",
                    "Removing all team members",
                    "Deleting the schedule"),

                Q(
                    "What is a project baseline?",
                    3,
                    "An email account",
                    "A database table",
                    "A design color",
                    "An approved reference point used to measure project performance"),

                Q(
                    "What is the critical path?",
                    1,
                    "The cheapest tasks",
                    "The sequence of activities that determines the shortest project duration",
                    "All optional tasks",
                    "Only completed tasks"),

                Q(
                    "Why is communication management important?",
                    2,
                    "It removes stakeholders",
                    "It avoids documentation",
                    "It ensures relevant project information reaches the right people",
                    "It eliminates meetings completely")
            };
        }

        private static List<ExamQuestionData>
            GeneralQuiz(
                string courseTitle,
                string lessonTitle)
        {
            return new()
            {
                Q(
                    $"Which course contains the lesson '{lessonTitle}'?",
                    0,
                    courseTitle,
                    "Another Course",
                    "Project Management",
                    "Graphic Design"),

                Q(
                    $"What should students study before the '{lessonTitle}' quiz?",
                    1,
                    "Unrelated material",
                    $"The concepts covered in {lessonTitle}",
                    "Account settings",
                    "Other users' profiles"),

                Q(
                    $"What is the quiz mainly evaluating in '{lessonTitle}'?",
                    2,
                    "Website appearance",
                    "Student email",
                    "Understanding of the lesson concepts",
                    "Account password"),

                Q(
                    $"Which material is most relevant when preparing for {courseTitle}?",
                    0,
                    "Course lessons",
                    "Unrelated courses",
                    "Login settings",
                    "Profile images"),

                Q(
                    $"What is the best way to improve understanding of '{lessonTitle}'?",
                    3,
                    "Skip the lesson",
                    "Ignore mistakes",
                    "Avoid quizzes",
                    "Review the lesson and practice")
            };
        }

        private static List<ExamQuestionData>
            GeneralFinal(
                string courseTitle)
        {
            return new()
            {
                Q(
                    $"What should students review before the {courseTitle} final exam?",
                    0,
                    "The complete course content",
                    "Only account settings",
                    "Unrelated subjects",
                    "Only the login page"),

                Q(
                    $"What does the {courseTitle} final exam mainly evaluate?",
                    2,
                    "Website colors",
                    "User profiles",
                    "Overall understanding of course concepts",
                    "Instructor passwords"),

                Q(
                    "What is the best way to prepare for a comprehensive exam?",
                    1,
                    "Skip difficult topics",
                    "Review all major topics and practice",
                    "Ignore previous work",
                    "Study unrelated subjects"),

                Q(
                    "Why is reviewing mistakes useful?",
                    3,
                    "It deletes results",
                    "It changes the course",
                    "It removes lessons",
                    "It helps identify and improve weak areas"),

                Q(
                    "What should students do when they do not understand a concept?",
                    0,
                    "Review the material and seek clarification",
                    "Ignore it",
                    "Delete the course",
                    "Skip every assessment"),

                Q(
                    "Why are assessments used in learning?",
                    2,
                    "To change passwords",
                    "To remove lessons",
                    "To measure understanding and progress",
                    "To change website colors"),

                Q(
                    "What is a useful study strategy?",
                    1,
                    "Study everything once at the last minute",
                    "Review material regularly over time",
                    "Avoid practice",
                    "Ignore feedback"),

                Q(
                    "Why should students connect related course concepts?",
                    3,
                    "To make studying harder",
                    "To remove details",
                    "To avoid understanding",
                    "To build a more complete understanding of the subject"),

                Q(
                    "What should a student do before submitting an exam?",
                    0,
                    "Review answers when time allows",
                    "Delete selected answers",
                    "Close the course",
                    "Change the account email"),

                Q(
                    $"What is the main goal of completing the {courseTitle} final exam?",
                    2,
                    "Change profile settings",
                    "Create a new account",
                    "Demonstrate understanding of the course",
                    "Remove course content")
            };
        }

        private class ExamQuestionData
        {
            public string Question { get; set; } =
                string.Empty;

            public string[] Answers { get; set; } =
                Array.Empty<string>();

            public int CorrectAnswer { get; set; }
        }
    }
}