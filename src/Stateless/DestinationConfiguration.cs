﻿using System;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// This class containd the required trigger information for a transition.
        /// </summary>
        public class DestinationConfiguration
        {
            private readonly TransitionConfiguration _transitionConfiguration;
            private readonly TriggerBehaviour _triggerBehaviour;
            private readonly StateRepresentation _representation;

            internal DestinationConfiguration(TransitionConfiguration transitionConfiguration, TriggerBehaviour triggerBehaviour, StateRepresentation representation)
            {
                _transitionConfiguration = transitionConfiguration;
                _triggerBehaviour = triggerBehaviour;
                _representation = representation;
            }

            /// <summary>
            /// Adds a guard function to the trigger. This guard function will determine if the transition will occur or not.
            /// </summary>
            /// <param name="guard">This method is run when the state machine fires the trigger.</param>
            /// <param name="description">Optional description of the guard</param>
            internal DestinationConfiguration If(Func<object[], bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(guard, description));
                return this;
            }

            /// <summary>
            /// Adds a guard function to the trigger. This guard function will determine if the transition will occur or not.
            /// </summary>
            /// <typeparam name="TArg">The parameter to the guard function </typeparam>
            /// <param name="guard">This method is run when the state machine fires the trigger.</param>
            /// <param name="description">Optional description of the guard</param>
            internal DestinationConfiguration If<TArg>(Func<TArg, bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(TransitionGuard.ToPackedGuard(guard), description));
                return this;
            }

            /// <summary>
            /// Creates a new transition. Use To(), Self(), Internal() or Dynamic() to set up the destination.
            /// </summary>
            /// <param name="trigger">The event trigger that will trigger this transition.</param>
            internal TransitionConfiguration Transition(TTrigger trigger)
            {
                return new TransitionConfiguration(_transitionConfiguration.StateConfiguration, _representation, trigger);
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            internal StateConfiguration Do(Action someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction());
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            internal StateConfiguration Do(Action<Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction(someAction);
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <typeparam name="TArg">The paramter used by the action.</typeparam>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            internal StateConfiguration Do<TArg>(Action<TArg, Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));
                
                _triggerBehaviour.AddAction((t, args) => someAction(ParameterConversion.Unpack<TArg>(args, 0), t));
                return _transitionConfiguration.StateConfiguration;
            }
        }
    }
}