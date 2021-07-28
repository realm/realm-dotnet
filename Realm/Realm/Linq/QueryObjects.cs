using System;

namespace Realms
{
    public class QueryModel
    {
        public WhereClause WhereClause { get; set; }

        public OrderbyClause OrderByClause { get; set; }
    }

    public class OrderbyClause
    {
        public OrderingNode OrderingNode { get; set; }

        public string Kind { get; } = "orderbyclause";
    }

    public class OrderingNode
    {
        public bool IsAscending { get; set; }

        public bool IsReplacing { get; set; }

        public string Property { get; set; }
    }

    public class WhereClause
    {
        public ExpressionNode ExpNode { get; set; }

        public string Kind { get; } = "whereclause";
    }

    public abstract class ExpressionNode
    {
        public abstract string Kind { get; }
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

        public override string Kind => "And";
    }

    public class OrNode : BooleanBinaryNode
    {
        public override string Operator => "||";

        public override string Kind => "Or";
    }

    public class BooleanPropertyNode : ExpressionNode
    {
        public string Property { get; set; }

        public override string Kind => throw new NotImplementedException();
    }

    public abstract class ComparisonNode : ExpressionNode
    {
        public StandardNode Left { get; set; }

        public StandardNode Right { get; set; }

        public abstract string Operator { get; }

        public override string Kind => "comparison";

        public ComparisonNode()
        {
            Left = new StandardNode();

            Right = new StandardNode();
        }
    }

    public class StandardNode
    {
        public string Kind { get; set; }

        public object Value { get; set; }

        public string Type { get; set; }
    }

    public class EqualityNode : ComparisonNode
    {
        public override string Operator => "=";
    }

    public class NotEqualNode : ComparisonNode
    {
        public override string Operator => "!=";
    }

    public class GteNode : ComparisonNode
    {
        public override string Operator => ">=";
    }

    public class GtNode : ComparisonNode
    {
        public override string Operator => ">";
    }

    public class LteNode : ComparisonNode
    {
        public override string Operator => "<=";
    }

    public class LtNode : ComparisonNode
    {
        public override string Operator => "<";
    }

    public class StartsWithNode : ComparisonNode
    {
        public override string Operator => "StartsWith";
    }
}
