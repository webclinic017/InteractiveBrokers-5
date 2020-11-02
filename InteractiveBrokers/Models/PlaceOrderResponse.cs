using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
	public class PlaceOrderResponse
	{

		public PlaceOrderResponse()
		{
			this.Order = new HashSet<OrderDetail>();
			this.OrderIds = new HashSet<OrderId>();
		}
		public OrderType OrderType { get; set; }
		public Messages MessageList { get; set; }
		public decimal TotalOrderValue { get; set; }
		public decimal TotalCommission { get; set; }
		public long OrderId { get; set; }
		public IEnumerable<OrderDetail> Order { get; set; }
		public bool DstFlag { get; set; }
		public string OptionLevelCd { get; set; }
		public MarginLevelCd MarginLevelCd { get; set; }
		public bool IsEmployee { get; set; }
		public string CommissionMsg { get; set; }
		public IEnumerable<OrderId> OrderIds { get; set; }
		public long PlacedTime { get; set; }
		public string AccountId { get; set; }
		public PortfolioMargin PortfolioMargin { get; set; }
		public Disclosure Disclosure { get; set; }
		public string ClientOrderId { get; set; }

	}
}
