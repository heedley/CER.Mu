﻿namespace CER.Graphs
{
    using CER.Graphs.SetExtensions;
    using CER.Runtime.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;

    public class DirectedGraph : Dictionary<string, string[]>
    {
        public static DirectedGraph EmptyGraph
        {
            get
            {
                return new DirectedGraph();
            }
        }

        public DirectedGraph() : base() { }
        public DirectedGraph(string json, string quote = "'") : this(json.ParseJsonToSimple<DirectedGraph>(quote)) { }
        public DirectedGraph(IDictionary<string, string[]> template) : base(template) { }
        public DirectedGraph(IEnumerable<Entity> entities)
        {
            foreach (var element in entities)
            {
                this[element.Address] = element.Entities().Select(x => x.Address).ToArray();
            }
        }

        public string[] Roots
        {
            get
            {
                var possible_roots = this.Select(x => x.Key).ToList();
                foreach (var child in this.SelectMany(x => x.Value).Distinct())
                {
                    possible_roots.Remove(child);
                }
                return possible_roots.ToArray();
            }
        }

        public string[] Sinks
        {
            get
            {
                var possible_sinks = this.SelectMany(x => x.Value).ToList();
                foreach (var node in this)
                {
                    possible_sinks.Remove(node.Key);
                }
                return possible_sinks.ToArray();
            }
        }

        public bool IsDirectedAcyclicGraph
        {
            get
            {
                var graph = new DirectedGraph(this);
                return this.DisassemblesToDirectedAcyclicGraph(graph);
            }
        }

        public KeyValuePair<string,string[]>[] DisassemblesToLoops(DirectedGraph graph)
        {
            this.DisassemblesToDirectedAcyclicGraph(graph);
            return graph.ToArray();
        }

        private bool DisassemblesToDirectedAcyclicGraph(DirectedGraph graph)
        {
            var roots = graph.Roots;
            if (roots.Count() == 0)
            {
                if (graph.Count > 0) { return false; } else { return true; }
            }

            foreach (var r in roots)
            {
                graph.Remove(r);
            }
            return this.DisassemblesToDirectedAcyclicGraph(graph);
        }
    }
}
