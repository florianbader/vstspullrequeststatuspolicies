namespace AIT.PullRequestStatus.Domain.Entities
{
    public class RepositoryStatusPolicy
    {
        public string Description { get; set; }

        public string Id { get; set; }

        public bool IsActivated { get; set; }

        public string Name { get; set; }
    }
}