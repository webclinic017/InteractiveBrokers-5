using System.Collections;
using System.Collections.Generic;

namespace InteractiveBrokers.Models
{
	public class Messages
	{
		public Messages()
		{
			this.Message = new HashSet<Message>();
		}
		public IEnumerable<Message> Message { get; set; }
	}
}
