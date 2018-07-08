using System;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false)]
    public class StatusPolicyDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public StatusPolicyDescriptionAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}