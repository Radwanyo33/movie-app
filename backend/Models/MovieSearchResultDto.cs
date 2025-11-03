namespace Live_Movies.Models
{
    public class MovieSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Release_Year { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image_url { get; set; } = string.Empty;
        public string Watch_url { get; set; } = string.Empty;
        public List<string> Genre { get; set; } = new List<string>();
        public List<string> Cast { get; set; } = new List<string>();
    }
}
