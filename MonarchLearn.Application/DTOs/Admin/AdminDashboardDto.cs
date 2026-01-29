using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Admin
{
    public class AdminDashboardDto
    {
        //  İstifadəçi statistikası
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }

        //  Kurs statistikası
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; } // IsDeleted = false

        //  Enrollment statistikası
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public double CompletionRate { get; set; } // %

        //  Quiz statistikası
        public int TotalQuizAttempts { get; set; }
        public int FailedQuizAttempts { get; set; }
        public double QuizFailRate { get; set; } // %

        // Top  ən populyar kurslar
        public List<TopCourseDto> TopCourses { get; set; }

        // Top  ən çox tərk edilən kurslar
        public List<AbandonedCourseDto> MostAbandonedCourses { get; set; }
    }

   
}
