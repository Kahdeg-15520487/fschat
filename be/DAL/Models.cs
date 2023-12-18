namespace be.DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; }

        // Add other user-related fields as needed
    }

    public class Message
    {
        [Key]
        public Guid MessageID { get; set; }

        public Guid SenderID { get; set; }
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
        public int GroupID { get; set; }

        [Required]
        public string GroupName { get; set; }

        // Add other group-related fields as needed

        public virtual List<UserGroup> UserGroups { get; set; }
    }

    public class UserGroup
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }

        public User User { get; set; }
        public Group Group { get; set; }
    }
}
