using VRefSolutions.Domain.Entities;

namespace VRefSolutions.Service.Util
{
    public class LevenshteinDistance
    {
        const double MAX_ALLOWED_DISTANCE_PERCENTAGE = 0.6;

        public static List<string> CorrectMessages(List<string> messages, List<EcamMessage> ecamMessageList)
        {
            List<string> correctedMessages = new();

            // Find match for each input word
            for (int i = 0; i < messages.Count; i++)
            {
                Dictionary<string, int> eventsToDistance = new();

                // Find word's Levenshtein distance with each ECAM message 
                for (int j = 0; j < ecamMessageList.Count; j++)
                {
                    int distance = Compute(messages[i], ecamMessageList[j].Name);

                    eventsToDistance.TryAdd(ecamMessageList[j].Name, distance);
                }

                if (eventsToDistance.Count <= 0)
                    continue;

                // Find word with lowest distance
                KeyValuePair<string, int> lowestDistanceKvp = eventsToDistance.Aggregate((eventName, distance) => eventName.Value < distance.Value ? eventName : distance);

                // Skip word if the distance of the closest match is too large
                if (lowestDistanceKvp.Value >= messages[i].Length * MAX_ALLOWED_DISTANCE_PERCENTAGE)
                    continue;

                try
                {
                    correctedMessages.Add(lowestDistanceKvp.Key);
                }
                catch(Exception)
                {
                    continue;
                }
            }

            return correctedMessages;
        }

        private static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}
