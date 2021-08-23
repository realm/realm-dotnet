using System.Collections.Generic;

namespace Realms
{
    internal class QueryModel
    {
        public List<WhereClause> WhereClauses { get; set; } = new List<WhereClause>();

        public List<OrderingClause> OrderingClauses { get; set; } = new List<OrderingClause>();
    }

    #region Clause Nodes
    internal class OrderingClause
    {
        public bool IsAscending { get; set; }

        public string Property { get; set; }
    }

    internal class WhereClause
    {
        public ExpressionNode Expression { get; set; }
    }
    #endregion

    #region Parent Node
    internal abstract class ExpressionNode
    {
        public abstract string Kind { get; }
    }
    #endregion

    #region Base Level Nodes
    internal class PropertyNode : ExpressionNode
    {
        public override string Kind => "property";

        public string Name { get; set; }

        public string Type { get; set; }
    }

    internal class ConstantNode : ExpressionNode
    {
        public override string Kind => "constant";

        public string Value { get; set; }

        public string Type => "arg";
    }
    #endregion

    #region Boolean Operator Nodes
    internal abstract class BooleanBinaryNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public override abstract string Kind { get; }
    }

    internal class AndNode : BooleanBinaryNode
    {
        public override string Kind => "and";
    }

    internal class OrNode : BooleanBinaryNode
    {
        public override string Kind => "or";
    }

    internal class NegationNode : ExpressionNode
    {
        public ExpressionNode Expression { get; set; }

        public override string Kind => "not";
    }
    #endregion

    #region Comparison Nodes
    internal abstract class ComparisonNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public override abstract string Kind { get; }
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
    #endregion

    #region String Comparison Nodes
    internal abstract class StringComparisonNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public override abstract string Kind { get; }

        public bool CaseSensitivity { get; set; } = true;
    }

    internal class StringEqualityNode : StringComparisonNode
    {
        public override string Kind => "eq";
    }

    internal class StartsWithNode : StringComparisonNode
    {
        public override string Kind => "beginsWith";
    }

    internal class EndsWithNode : StringComparisonNode
    {
        public override string Kind => "endsWith";
    }

    internal class ContainsNode : StringComparisonNode
    {
        public override string Kind => "contains";
    }

    internal class LikeNode : StringComparisonNode
    {
        public override string Kind => "like";
    }
    #endregion
}
