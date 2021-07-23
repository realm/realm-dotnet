using System;

namespace Realms
{
    public class QueryModel
    {
        public WhereClause WhereClause { get; set; }

        public SortClause SortClause { get; set; }
    }

    public class SortClause
    {
        public ExpressionNode expNode { get; set; }
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

    public class BooleanPropertyNode : ExpressionNode
    {
        public string Property { get; set; }
    }

    public abstract class ComparisonNode : ExpressionNode
    {
        public PropertyNode PropertyNode { get; set; }

        public ValueNode ValueNode { get; set; }

        public abstract string Operator { get; }

        public ComparisonNode()
        {
            PropertyNode = new PropertyNode();

            ValueNode = new ValueNode();
        }
    }

    public class ValueNode : ExpressionNode
    {
        public object Value { get; set; }

        public string Type { get; set; }
    }

    public class PropertyNode : ExpressionNode
    {
        public object Property { get; set; }

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
