using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public enum OrderStatus
    {
		OPEN, EXECUTED, CANCELLED, INDIVIDUAL_FILLS, CANCEL_REQUESTED, EXPIRED, REJECTED, PARTIAL, OPTION_EXERCISE, OPTION_ASSIGNMENT, DO_NOT_EXERCISE, DONE_TRADE_EXECUTED
	}
}
