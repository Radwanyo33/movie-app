namespace Live_Movies.Models
{
    public class MovieCast
    {
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; } = null!;
        public int CastMemberId { get; set; }
        public virtual CastMember CastMember { get; set; } = null!;
    }
}
