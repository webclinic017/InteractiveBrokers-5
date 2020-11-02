namespace InteractiveBrokers.Models
{
	public class Message
	{
		public string Description { get; set; }
		public int Code { get; set; }
		public MessageType Type { get; set; }
	}
}
