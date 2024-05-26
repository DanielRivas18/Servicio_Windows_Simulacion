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
        private const int maxStudentId = 2000;
        private const int maxSubjectId = 49;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            random = new Random();
           
            timer = new Timer(10 * 10000); 
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            EnrollRandomStudent();
        }

        private async Task EnrollRandomStudent()
        {
            var studentId = GetRandomStudentId();
            var subjectId = GetRandomSubjectId();

            await EnrollStudentAsync(studentId, subjectId);
        }

        private int GetRandomSubjectId()
        {
           
            return random.Next(1, maxSubjectId+1); 
        }

        private int GetRandomStudentId()
        {
          
            return random.Next(1, maxStudentId+1); 
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

         
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }
}
