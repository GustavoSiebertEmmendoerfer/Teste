using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questao2
{
    public class FootballMatchResponse
    {
        public int Total_pages { get; set; }
        public IEnumerable<FootballMatchDetailsResponse> Data { get; set; }
    }

    public class FootballMatchDetailsResponse
    {
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string Team1Goals { get; set; }
        public string Team2Goals { get; set; }
    }
}
