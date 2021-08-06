using System.Collections.Generic;

namespace Realms
{
    internal class QueryModel
    {
        public List<WhereClause> WhereClauses { get; set; } = new List<WhereClause>();

        public List<OrderingClause> OrderingClauses { get; set; } = new List<OrderingClause>();

    }

    public class OrderingClause
    {
        public bool IsAscending { get; set; }

        public string Property { get; set; }
    }

    // TODO: Fix modifier access to internal for all classes + fields
    public class WhereClause
    {
        public ExpressionNode Expression { get; set; }
    }

    public abstract class ExpressionNode
    {
    }

    public class NegationNode : ExpressionNode
    {
        public ExpressionNode Expression { get; set; }

        public string Kind => "not";
    }

    public abstract class BooleanBinaryNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public abstract string Kind { get; }
    }

    public class AndNode : BooleanBinaryNode
    {
        public override string Kind => "and";

    }

    public class OrNode : BooleanBinaryNode
    {
        public override string Kind => "or";

    }

    public class BooleanPropertyNode : ExpressionNode
    {
        public string Property { get; set; }

    }

    public abstract class StringComparisonNode : ExpressionNode
    {
        public ConstantNode Left { get; set; }

        public PropertyNode Right { get; set; }

        public abstract string Kind { get; }

        // TODO: Remove this and set left and right in appropriate assignement method
        public StringComparisonNode()
        {
            Left = new ConstantNode();

            Right = new PropertyNode();
        }
    }

    public class StartsWithNode : StringComparisonNode
    {
        public override string Kind => "beginsWith";
    }

    public class EndsWithNode : StringComparisonNode
    {
        public override string Kind => "endsWith";
    }

    public class ContainsNode : StringComparisonNode
    {
        public override string Kind => "contains";
    }

    public class LikeNode : StringComparisonNode
    {
        public override string Kind => "like";
    }

    public abstract class ComparisonNode : ExpressionNode
    {
        public PropertyNode Left { get; set; }

        public PropertyNode Right { get; set; }

        public abstract string Kind { get; }

        // TODO: Remove this and set left and right in appropriate assignement method
        public ComparisonNode()
        {
            Left = new PropertyNode();

            Right = new PropertyNode();
        }
    }

    public class ConstantNode
    {
        public string Kind { get; set; }

        public object Name { get; set; }

        public string Type { get; set; }
    }

    public class PropertyNode
    {
        public string Kind { get; set; }

        public object Value { get; set; }

        public string Type { get; set; }
    }

    public class EqualityNode : ComparisonNode
    {
        public override string Kind => "eq";
    }

    public class NotEqualNode : ComparisonNode
    {
        public override string Kind => "neq";
    }

    public class GteNode : ComparisonNode
    {
        public override string Kind => "gte";
    }

    public class GtNode : ComparisonNode
    {
        public override string Kind => "gt";
    }

    public class LteNode : ComparisonNode
    {
        public override string Kind => "lte";
    }

    public class LtNode : ComparisonNode
    {
        public override string Kind => "lt";
    }
}
