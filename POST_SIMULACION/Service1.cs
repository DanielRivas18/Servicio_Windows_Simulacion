using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace POST_SIMULACION
{
    [RunInstaller(true)]

    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private static readonly HttpClient client = new HttpClient();
        private Random random;
        private const int minStudentId = 1;  
        private const int maxStudentId = 1000;  
        private const int minSubjectId = 1;  
        private const int maxSubjectId = 50;  

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            random = new Random();
            int tiempo = Settings1.Default.TIEMPO;
            timer = new Timer(1000); // Configurar el temporizador para 1 minuto
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            EnrollRandomStudent();
        }

        private async void EnrollRandomStudent()
        {
            var studentId = random.Next(minStudentId, maxStudentId + 1);
            var subjectId = random.Next(minSubjectId, maxSubjectId + 1);

            if (!await IsAlreadyEnrolled(studentId, subjectId))
            {
                await EnrollStudentAsync(studentId, subjectId);
            }
        }

        private async Task<bool> IsAlreadyEnrolled(int studentId, int subjectId)
        {
            var enrollmentCheck = new
            {
                subject_id = subjectId,
                student_id = studentId
            };

            var json = JsonConvert.SerializeObject(enrollmentCheck);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:3000/enrollments", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CheckEnrollmentResponse>(result).IsEnrolled;
            }

            return false;
        }

        private async Task EnrollStudentAsync(int studentId, int subjectId)
        {
            var enrollment = new
            {
                subject_id = subjectId,
                student_id = studentId
            };

            var json = JsonConvert.SerializeObject(enrollment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:3000/enrollments", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Successfully enrolled student {studentId} in subject {subjectId}");
            }
            else
            {
                Console.WriteLine($"Failed to enroll student {studentId} in subject {subjectId}: {response.ReasonPhrase}");
            }
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }

    public class CheckEnrollmentResponse
    {
        public bool IsEnrolled { get; set; }
    }
}
