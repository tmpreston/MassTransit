// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Subscriptions.Actors
{
	using System;
	using Pipeline;

	public interface EndpointConnector
	{
		UnsubscribeAction Connect(Uri endpointUri, string correlationId);
	}

	public class EndpointConnector<TMessage> :
		EndpointConnector
		where TMessage : class
	{
		readonly IServiceBus _bus;

		public EndpointConnector(IServiceBus bus)
		{
			_bus = bus;
		}

		public UnsubscribeAction Connect(Uri endpointUri, string correlationId)
		{
			IEndpoint endpoint = _bus.GetEndpoint(endpointUri);

			return _bus.OutboundPipeline.ConnectEndpoint<TMessage>(endpoint);
		}
	}

	public class EndpointConnector<TMessage, TKey> :
		EndpointConnector
		where TMessage : class, CorrelatedBy<TKey>
	{
		readonly IServiceBus _bus;
		readonly Func<string, TKey> _converter;

		public EndpointConnector(IServiceBus bus, Func<string, TKey> converter)
		{
			_bus = bus;
			_converter = converter;
		}

		public UnsubscribeAction Connect(Uri endpointUri, string correlationId)
		{
			IEndpoint endpoint = _bus.GetEndpoint(endpointUri);

			TKey key = _converter(correlationId);

			return _bus.OutboundPipeline.ConnectEndpoint<TMessage, TKey>(key, endpoint);
		}
	}
}