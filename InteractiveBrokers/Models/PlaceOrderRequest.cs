using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public class PlaceOrderRequest
    {
		public OrderType OrderType { get; set; }
		public string ClientOrderId { get; set; }
		public IEnumerable<OrderDetail> Order { get; set; }
		public IEnumerable<PreviewId> PreviewIds { get; set; }
	}
}
