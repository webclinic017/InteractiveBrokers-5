using System;
using System.Runtime.Serialization;

namespace InteractiveBrokers
{
	[Serializable]
	public class InteractiveBrokersException : Exception
	{
		public InteractiveBrokersException()
		{
		}

		public InteractiveBrokersException(string message) : base(message)
		{
		}

		public InteractiveBrokersException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InteractiveBrokersException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
