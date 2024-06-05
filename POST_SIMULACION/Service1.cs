using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Timers;
using Npgsql; // Asegúrate de que esta línea está presente
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace POST_SIMULACION
{
    [RunInstaller(true)]
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private Random random;
        private static int maxStudentId;  // número de estudiantes
        private static int maxSubjectId; // número de materias
        private static double timeValue; // segundos
        private static readonly HttpClient client = new HttpClient();

        private static string connectionString;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            if (!double.TryParse(Environment.GetEnvironmentVariable("TIME_VALUE"), out timeValue))
            {
                timeValue = 0.5;
            }


            string host = Environment.GetEnvironmentVariable("PG_HOST") ?? "localhost";
            string port = Environment.GetEnvironmentVariable("PG_PORT") ?? "5432";
            string user = Environment.GetEnvironmentVariable("PG_USER") ?? "postgres";
            string password = Environment.GetEnvironmentVariable("PG_PASSWORD") ?? "root";
            string database = Environment.GetEnvironmentVariable("PG_DATABASE") ?? "simulacion";

            connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database}";

            random = new Random();
            InitializeMaxValues();

            timer = new Timer(timeValue * 1000);
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void InitializeMaxValues()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM students", conn))
                {
                    maxStudentId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM subjects", conn))
                {
                    maxSubjectId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private int GetRandomSubjectId()
        {
            return random.Next(1, maxSubjectId + 1);
        }

        private int GetRandomStudentId()
        {
            return random.Next(1, maxStudentId + 1);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            EnrollRandomStudent();
        }

        private async Task EnrollRandomStudent()
        {
            var studentId = GetRandomStudentId();
            var subjectId = GetRandomSubjectId();

            await Enrollment(studentId, subjectId);
        }

        private async Task Enrollment(int studentId, int subjectId)
        {
            var enrollment = new
            {
                subject_id = subjectId,
                student_id = studentId
            };

            var json = JsonConvert.SerializeObject(enrollment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync("http://localhost:3000/enrollments", content);
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }
}
