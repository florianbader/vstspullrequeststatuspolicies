namespace AIT.PullRequestStatus.Domain.Entities
{
    public class PullRequestUpdate
    {
        public string CollectionId { get; set; }

        public int Id { get; set; }

        public string ProjectId { get; set; }

        public string RepositoryId { get; set; }

        public string Status { get; set; }

        public string Token { get; set; }
    }
}