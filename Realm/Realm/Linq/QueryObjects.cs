using System;

namespace Realms
{
    public class WhereClauseProperties
    {
        public string Property { get; set; }

        public object Value { get; set; }

        public string Operator { get; set; }
    }

    public class WhereClause
    {
        public ExpressionNode ExpNode { get; set; }
    }

    public class ExpressionNode
    {

    }

    public abstract class BooleanBinaryNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public abstract string Operator { get; }
    }

    public class AndNode : BooleanBinaryNode
    {
        public override string Operator => "&&";
    }

    public class OrNode : BooleanBinaryNode
    {
        public override string Operator => "||";
    }

    public abstract class ComparisonNode : ExpressionNode
    {
        public string Property { get; set; }

        public object Value { get; set; }

        public abstract string Operator { get; }
    }

    public class EqualityNode : ComparisonNode
    {
        public override string Operator => "=";
    }

    public class BiggerThanNode : ComparisonNode
    {
        public override string Operator => ">=";
    }

    public class StartsWithNode : ComparisonNode
    {
        public override string Operator => "StartsWith";
    }
}
