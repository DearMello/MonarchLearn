using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class UpdateQuizSettingsDto
    {
        // İmtahanın keçid balı (məsələn: 50, 70 və s.)
        public int PassingScorePercent { get; set; }

        // İmtahanın vaxt limiti (saniyə ilə)
        public int TimeLimitSeconds { get; set; }
    }
}
