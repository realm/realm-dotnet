using System.Collections.Generic;

namespace Realms
{
    internal class QueryModel
    {
        public List<WhereClause> WhereClauses { get; set; } = new List<WhereClause>();

        public List<OrderingClause> OrderingClauses { get; set; } = new List<OrderingClause>();
    }

    internal class OrderingClause
    {
        public bool IsAscending { get; set; }

        public string Property { get; set; }
    }

    // TODO: Changing access modifier to internal will make all tests fail(?)
    internal class WhereClause
    {
        public ExpressionNode Expression { get; set; }
    }

    internal abstract class ExpressionNode
    {
    }

    internal class NegationNode : ExpressionNode
    {
        public ExpressionNode Expression { get; set; }

        public string Kind => "not";
    }

    internal abstract class BooleanBinaryNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public abstract string Kind { get; }
    }

    internal class AndNode : BooleanBinaryNode
    {
        public override string Kind => "and";
    }

    internal class OrNode : BooleanBinaryNode
    {
        public override string Kind => "or";
    }

    internal class BooleanPropertyNode : ExpressionNode
    {
        public string Property { get; set; }
    }

    internal abstract class ComparisonNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public abstract string Kind { get; }
    }

    internal class PropertyNode : ExpressionNode
    {
        public string Kind => "property";

        public string Name { get; set; }

        public string Type { get; set; }
    }

    internal class ConstantNode : ExpressionNode
    {
        public string Kind => "constant";

        public object Value { get; set; }

        public string Type { get; set; }
    }

    internal class EqualityNode : ComparisonNode
    {
        public override string Kind => "eq";
    }

    internal class NotEqualNode : ComparisonNode
    {
        public override string Kind => "neq";
    }

    internal class GteNode : ComparisonNode
    {
        public override string Kind => "gte";
    }

    internal class GtNode : ComparisonNode
    {
        public override string Kind => "gt";
    }

    internal class LteNode : ComparisonNode
    {
        public override string Kind => "lte";
    }

    internal class LtNode : ComparisonNode
    {
        public override string Kind => "lt";
    }

    internal class StartsWithNode : ComparisonNode
    {
        public override string Kind => "beginsWith";
    }

    internal class EndsWithNode : ComparisonNode
    {
        public override string Kind => "endsWith";
    }

    internal class ContainsNode : ComparisonNode
    {
        public override string Kind => "contains";
    }

    internal class LikeNode : ComparisonNode
    {
        public override string Kind => "like";
    }
}
