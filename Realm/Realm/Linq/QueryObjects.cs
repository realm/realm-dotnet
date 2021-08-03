using System;
using System.Collections.Generic;

namespace Realms
{
    internal class QueryModel
    {
        public List<WhereClause> WhereClauses { get; set; }

        public List<OrderingClause> OrderingClauses { get; set; }
    }

    public class OrderingClause
    {
        public bool IsAscending { get; set; }

        public string Property { get; set; }
    }

    public class WhereClause
    {
        public ExpressionNode Expression { get; set; }
    }

    public abstract class ExpressionNode
    {
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

    public abstract class ComparisonNode : ExpressionNode
    {
        public StandardNode Left { get; set; }

        public StandardNode Right { get; set; }

        public abstract string Kind { get; }

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

    public class StartsWithNode : ComparisonNode
    {
        public override string Kind => "StartsWith";
    }
}
