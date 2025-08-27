namespace HelpDesk.Vip.Api.VIP
{
    using System;

    public class VIP
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Sub { get; set; }

        public string Description { get; set; }

        public DateTime AddedOn { get; set; } = DateTime.UtcNow;

        public bool IsRemoved { get; set; } = false;
    }
