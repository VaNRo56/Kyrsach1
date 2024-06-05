using static System.Net.WebRequestMethods;

namespace Kyrs1
{
    public class Constants
    {
        public static string Address = $"https://gnews.io/api/v4/search?q=example&lang=en&country=us&max=10&apikey={Api_Key}";
        public static string Api_Key = "1d6c5564f66b3b98d410f64ec4c548a4";
        //public static string Api_Host = "crypto-news34.p.rapidapi.com";
        public static string Connect = "Host=localhost;Username=postgres;Password=7215;Database=postgres";
    }
}
