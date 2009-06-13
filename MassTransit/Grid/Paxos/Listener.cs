// Copyright 2007-2008 The Apache Software Foundation.
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
namespace MassTransit.Grid.Paxos
{
	using System;
	using Magnum.StateMachine;
	using Saga;

	public class Listener<T> :
		SagaStateMachine<Listener<T>>,
		ISaga
	{
		static Listener()
		{
			Define(() =>
				{
					Initially(
						When(ValueAccepted).And(message => !message.IsFinal)
							.Then(UpdateAcceptedValue)
							.TransitionTo(Active),
						When(ValueAccepted).And(message => message.IsFinal)
							.Then(UpdateAcceptedValue)
							.TransitionTo(Completed));

					During(Active,
					       When(ValueAccepted).And(message => !message.IsFinal)
					       	.Then(UpdateAcceptedValue),
					       When(ValueAccepted).And(message => message.IsFinal)
					       	.Then(UpdateAcceptedValue)
					       	.TransitionTo(Completed));
				});
		}

		public Listener(Guid correlationId)
		{
			CorrelationId = correlationId;
		}

		protected Listener()
		{
		}

		public static State Initial { get; set; }
		public static State Active { get; set; }
		public static State Completed { get; set; }

		public static Event<Accepted<T>> ValueAccepted { get; set; }

		public virtual long BallotId { get; set; }
		public virtual T Value { get; set; }

		public Guid CorrelationId { get; set; }

		public IServiceBus Bus { get; set; }

		private static void UpdateAcceptedValue(Listener<T> saga, Accepted<T> message)
		{
			saga.BallotId = message.ValueBallotId;
			saga.Value = message.Value;

			saga.NotifyAcceptedValue();
		}

		private void NotifyAcceptedValue()
		{
			var message = new ListenerAccepted<T>
				{
					BallotId = BallotId,
					CorrelationId = CorrelationId,
					Value = Value,
					ValueBallotId = BallotId,
				};

			Bus.Publish(message);
		}
	}
}