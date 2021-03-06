using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Semmle.Extraction.Kinds;

namespace Semmle.Extraction.CSharp.Entities.Statements
{
    class Switch : Statement<SwitchStatementSyntax>
    {
        static readonly object NullLabel = new object();
        public static readonly object DefaultLabel = new object();

        // Sometimes, the literal "null" is used as a label.
        // This is inconveniently represented by the "null" object.
        // This cannot be stored in a Dictionary<>, so substitute an object which can be.
        public static object LabelForValue(object label)
        {
            return label ?? NullLabel;
        }

        Switch(Context cx, SwitchStatementSyntax node, IStatementParentEntity parent, int child)
            : base(cx, node, StmtKind.SWITCH, parent, child) { }

        public static Switch Create(Context cx, SwitchStatementSyntax node, IStatementParentEntity parent, int child)
        {
            var ret = new Switch(cx, node, parent, child);
            ret.TryPopulate();
            return ret;
        }

        protected override void Populate()
        {
            Expression.Create(cx, Stmt.Expression, this, 0);
            int childIndex = 0;

            foreach (var section in Stmt.Sections)
            {
                foreach (var stmt in section.Labels.Select(label => Case<SwitchLabelSyntax>.Create(cx, label, this, childIndex)))
                {
                    childIndex += stmt.NumberOfStatements;
                }

                foreach (var stmt in section.Statements.Select(s => Create(cx, s, this, childIndex)))
                {
                    childIndex += stmt.NumberOfStatements;
                }
            }
        }
    }
}
