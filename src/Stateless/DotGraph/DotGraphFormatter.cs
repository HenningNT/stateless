﻿using System.Collections.Generic;
using System.Linq;
using Stateless.Reflection;

namespace Stateless.DotGraph
{
    /// <summary>
    /// DOT GraphViz text writer for reflection API.
    /// </summary>
    public static class DotGraphFormatter
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <param name="stateMachineInfo">The StateMachineInfo to be mapped.</param>
        /// <returns>DOT GraphViz text.</returns>
        public static string Format(StateMachineInfo stateMachineInfo)
        {
            var bindings = stateMachineInfo.States;

            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            foreach (var binding in bindings)
            {
                unknownDestinations.AddRange(binding.DynamicTransitions.Select(t => t.DestinationDescription));

                var source = binding.ToString();
                foreach (var transition in binding.FixedTransitions)
                {
                    string guardConditionsMethods = string.Join(", ", transition.GuardConditionsMethodDescriptions.Select(d => d.Description));
                    HandleTransitions(ref lines, source, transition.Trigger.ToString(), transition.DestinationState.ToString(), guardConditionsMethods);
                }
                
                foreach (var transition in binding.DynamicTransitions)
                {
                    string guardConditionsMethods = string.Join(", ", transition.GuardConditionsMethodDescriptions.Select(d => d.Description));
                    HandleTransitions(ref lines, source, transition.Trigger.ToString(), transition.DestinationDescription, guardConditionsMethods);
                }
            }

            if (unknownDestinations.Any())
            {
                string label = string.Format(" {{ node [label=\"?\"] {0} }};", string.Join(" ", unknownDestinations));
                lines.Insert(0, label);
            }

            if (bindings.Any(s => s.EntryAction!=null || s.ExitAction!=null))
            {
                lines.Add("node [shape=box];");

                foreach (var binding in bindings)
                {
                    var source = binding.ToString();

                    if (binding.EntryAction != null)
                        lines.Add( string.Format(" {0} -> \"{1}\" [label=\"On Entry\" style=dotted];", source, binding.EntryAction.Description));

                    if (binding.ExitAction != null)
                        lines.Add( string.Format(" {0} -> \"{1}\" [label=\"On Exit\" style=dotted];", source, binding.ExitAction.Description));
                }
            }

            return "digraph {" + System.Environment.NewLine +
                     string.Join(System.Environment.NewLine, lines) + System.Environment.NewLine +
                   "}";
        }

        private static void HandleTransitions(ref List<string> lines, string sourceState, string trigger, string destination, string guardDescription)
        {
            string line = string.IsNullOrWhiteSpace(guardDescription) ?
                string.Format(" {0} -> {1} [label=\"{2}\"];", sourceState, destination, trigger) :
                string.Format(" {0} -> {1} [label=\"{2} [{3}]\"];", sourceState, destination, trigger, guardDescription);

            lines.Add(line);
        }
    }
}
