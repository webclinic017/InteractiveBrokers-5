using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public class OrderResponse
    {
		public OrderResponse()
		{
			this.Orders = new HashSet<Order>();
		}
		public string Marker { get; set; }
		public string Order { get; set; }
		public IEnumerable<Order> Orders { get; set; }
		public Messages Messages { get; set; }
	}
}
