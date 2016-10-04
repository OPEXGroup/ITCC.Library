using System;
using System.Collections.Generic;

namespace ITCC.HTTP.SslConfigUtil.Console
{
    internal class UtilCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Alias { get; set; }
        public List<ParameterSet> ParameterSets { get; set; }
    }
    internal abstract class ParameterSet
    {
        public abstract List<CommandParameter> Parameters { get; set; }
        public abstract string Execute();

        public bool Validate()
        {
            throw new NotImplementedException();
        }
    }
    internal class CommandParameter : IEquatable<CommandParameter>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFlag { get; set; }
        public bool IsRequired { get; set; }
        public List<string> Alias { get; set; } = new List<string>();
        public List<CommandParameter> RequiredParameters { get; set; } = new List<CommandParameter>();
        public List<CommandParameter> ConflictParameters { get; set; } = new List<CommandParameter>();

        public bool Equals(CommandParameter other) => Name.Equals(other.Name);
    }
}