namespace AVDGS.Web.Options
{
    public class WeatherOptions
    {
        public string Provider { get; set; } = "OpenMeteo";
        public string LocationLabel { get; set; } = "Lingayen Airport (RPUG), Pangasinan";
        public double Latitude { get; set; } = 16.0350583;
        public double Longitude { get; set; } = 120.2416139;
        public string Timezone { get; set; } = "Asia/Manila";
        public int PollSeconds { get; set; } = 60;
    }
}