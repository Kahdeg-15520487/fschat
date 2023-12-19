namespace be.DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User
    {
        [Key]
        public string UserID { get; set; }

        [Required]
        public string UserName { get; set; }

        public virtual List<UserGroup> UserGroups { get; set; }
    }

    public class Message
    {
        [Key]
        public Guid MessageID { get; set; }

        public string SenderID { get; set; }
        public Guid GroupID { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("SenderID")]
        public virtual User Sender { get; set; }

        [ForeignKey("GroupID")]
        public virtual Group Group { get; set; }
    }

    public class Group
    {
        [Key]
        public Guid GroupID { get; set; }

        [Required]
        public string GroupName { get; set; }

        public virtual List<UserGroup> UserGroups { get; set; }
    }

    public class UserGroup
    {
        public string UserID { get; set; }
        public Guid GroupID { get; set; }

        public User User { get; set; }
        public Group Group { get; set; }
    }
}
