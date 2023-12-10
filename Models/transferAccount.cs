namespace Task4.Models
{
    public class transferAccount
    {
        public Guid senderId { get; set; }
        public Guid receiverId { get; set; }

        public float transferAmount { get; set; }
    }
}
