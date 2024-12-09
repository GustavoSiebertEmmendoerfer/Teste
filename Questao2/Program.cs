using Newtonsoft.Json;
using Questao2;
using System.Web;

public class Program
{
    public static void Main()
    {
        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGoals = getTotalScoredGoals(teamName, year).Result;

        Console.WriteLine("Team "+ teamName +" scored "+ totalGoals.ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = getTotalScoredGoals(teamName, year).Result;

        Console.WriteLine("Team " + teamName + " scored " + totalGoals.ToString() + " goals in " + year);

        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014
    }

    public static async Task<int> getTotalScoredGoals(string team, int year)
    {
        var matchesTeam1 = await GetMatches(team, year, 1);
        var matchesTeam2 = await GetMatches(team, year, 2);

        var goals = matchesTeam1.Sum(m => int.Parse(m.Team1Goals)) + matchesTeam2.Sum(m => int.Parse(m.Team2Goals));

        return goals;
    }

    public static async Task<List<FootballMatchDetailsResponse>> GetMatches(string team, int year, int teamNumber)
    {
        var uri = new UriBuilder($"https://jsonmock.hackerrank.com/api/football_matches?year={year}&team{teamNumber}={team}");

        var footballMatchesDetails = new List<FootballMatchDetailsResponse>();

        var matchesResponse = await GetFootballMatch(uri.Uri);

        if (matchesResponse.Data.Any()) 
            footballMatchesDetails.AddRange(matchesResponse.Data);

        if (matchesResponse.Total_pages > 1)
        {
            for (int page = 2; page < matchesResponse.Total_pages + 1; page++)
            {
                var uriPaginated = new Uri(uri.Uri, $"&page={page}");

                var paginatedResponse = await GetFootballMatch(uriPaginated);

                if (matchesResponse.Data.Any()) 
                    footballMatchesDetails.AddRange(paginatedResponse.Data);
            }
        }

        return footballMatchesDetails;
    }

    public static async Task<FootballMatchResponse?> GetFootballMatch(Uri uri)
    {
        using (var client = new HttpClient())
        {
            var result = await client.GetAsync(uri);

            string responseBody = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode is true)
            {
                var matchesResponse = JsonConvert.DeserializeObject<FootballMatchResponse>(responseBody);

                return matchesResponse;
            }

            return null;
        }
    }

}